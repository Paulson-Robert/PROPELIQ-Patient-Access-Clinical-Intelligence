using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Orchestrates the full patient data aggregation pipeline (US_038, AC-01–04).
///
/// Execution order:
/// 1. Verify the patient profile exists.
/// 2. Load NER-extracted records from the database (AC-01: NER source).
/// 3. Load the most-recent completed intake record (AC-01: intake source).
/// 4. Deduplicate extracted records via <see cref="IDataDeduplicationService"/> (AC-02).
/// 5. Persist newly detected conflicts as <see cref="DataConflict"/> rows (Edge Cases).
/// 6. Build category-grouped JSON for each PatientView JSONB column.
/// 7. Upsert the <see cref="PatientView"/> row (AC-01, AC-04).
/// 8. Compute and persist the aggregate verification status (AC-03).
/// </summary>
public sealed class PatientAggregationService : IPatientAggregationService
{
    private readonly ApplicationDbContext _db;
    private readonly IDataDeduplicationService _dedup;
    private readonly IVerificationTrackingService _verification;
    private readonly ILogger<PatientAggregationService> _logger;

    // Source label embedded in aggregated JSON so the UI can display provenance.
    private const string IntakeSourceLabel = "Intake";

    public PatientAggregationService(
        ApplicationDbContext db,
        IDataDeduplicationService dedup,
        IVerificationTrackingService verification,
        ILogger<PatientAggregationService> logger)
    {
        _db = db;
        _dedup = dedup;
        _verification = verification;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AggregatePatientDataServiceResult> AggregateAsync(
        AggregatePatientDataRequest request,
        CancellationToken cancellationToken = default)
    {
        // -------------------------------------------------------------------
        // 1. Verify patient exists
        // -------------------------------------------------------------------
        var profileExists = await _db.PatientProfiles
            .AsNoTracking()
            .AnyAsync(p => p.PatientProfileId == request.PatientProfileId, cancellationToken)
            .ConfigureAwait(false);

        if (!profileExists)
        {
            _logger.LogWarning(
                "PatientAggregation: PatientProfile {PatientProfileId} not found.",
                request.PatientProfileId);

            return new AggregatePatientDataServiceResult(
                false, "PATIENT_NOT_FOUND", "Patient profile not found.");
        }

        // -------------------------------------------------------------------
        // 2. Load NER-extracted records (AC-01: NER source)
        // -------------------------------------------------------------------
        var extractedRecords = await _db.ExtractedDataRecords
            .AsNoTracking()
            .Where(r => r.PatientProfileId == request.PatientProfileId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // -------------------------------------------------------------------
        // 3. Load most-recent completed intake record (AC-01: intake source)
        // -------------------------------------------------------------------
        var latestIntake = await _db.IntakeRecords
            .AsNoTracking()
            .Where(i =>
                i.PatientProfileId == request.PatientProfileId
                && i.CompletedAt != null)
            .OrderByDescending(i => i.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        // -------------------------------------------------------------------
        // 4. Deduplicate extracted records across sources (AC-02)
        // -------------------------------------------------------------------
        var deduplicationResult = await _dedup
            .DeduplicateAsync(extractedRecords, cancellationToken)
            .ConfigureAwait(false);

        // -------------------------------------------------------------------
        // 5. Persist newly detected conflicts (Edge Cases)
        // -------------------------------------------------------------------
        int newConflictCount = 0;

        if (deduplicationResult.Conflicts.Count > 0)
        {
            newConflictCount = await PersistNewConflictsAsync(
                request.PatientProfileId,
                deduplicationResult.Conflicts,
                cancellationToken)
                .ConfigureAwait(false);
        }

        // -------------------------------------------------------------------
        // 6. Build aggregated JSON per category (AC-01)
        // -------------------------------------------------------------------
        var canonicals = deduplicationResult.CanonicalRecords;

        var aggregatedVitals = SerializeCategory(canonicals, ExtractedDataType.Vital);
        var aggregatedMedications = MergeMedicationSources(canonicals, latestIntake?.Medications);
        var aggregatedAllergies = MergeAllergyAndIntakeSources(canonicals, latestIntake?.Allergies);
        var aggregatedDiagnoses = SerializeCategory(canonicals, ExtractedDataType.Diagnosis);
        var aggregatedProcedures = SerializeCategory(canonicals, ExtractedDataType.Procedure);

        // -------------------------------------------------------------------
        // 7. Upsert PatientView (AC-01, AC-04)
        // -------------------------------------------------------------------
        var view = await _db.PatientViews
            .FirstOrDefaultAsync(
                v => v.PatientProfileId == request.PatientProfileId,
                cancellationToken)
            .ConfigureAwait(false);

        if (view is null)
        {
            view = new PatientView
            {
                PatientViewId = Guid.NewGuid(),
                PatientProfileId = request.PatientProfileId,
            };
            _db.PatientViews.Add(view);
        }

        view.AggregatedVitals = aggregatedVitals;
        view.AggregatedMedications = aggregatedMedications;
        view.AggregatedAllergies = aggregatedAllergies;
        view.AggregatedDiagnoses = aggregatedDiagnoses;
        view.AggregatedProcedures = aggregatedProcedures;
        view.LastAggregatedAt = DateTime.UtcNow;

        // -------------------------------------------------------------------
        // 8. Compute and persist verification status (AC-03)
        // -------------------------------------------------------------------
        view.VerificationStatus = await _verification
            .ComputeVerificationStatusAsync(request.PatientProfileId, cancellationToken)
            .ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "PatientAggregation: PatientView upserted for PatientProfileId {PatientProfileId}. " +
            "NewConflicts={NewConflictCount} VerificationStatus={VerificationStatus}.",
            request.PatientProfileId,
            newConflictCount,
            view.VerificationStatus);

        return new AggregatePatientDataServiceResult(true, null, null, newConflictCount);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Persists detected conflicts that are not already open in the database.
    /// Returns the count of newly inserted rows.
    /// </summary>
    private async Task<int> PersistNewConflictsAsync(
        Guid patientProfileId,
        IReadOnlyList<DetectedConflict> conflicts,
        CancellationToken cancellationToken)
    {
        // Load existing open conflict field names to avoid duplicates.
        var existingOpenFields = await _db.DataConflicts
            .AsNoTracking()
            .Where(c =>
                c.PatientProfileId == patientProfileId
                && c.ResolutionStatus == ResolutionStatus.Open)
            .Select(c => c.FieldName)
            .ToHashSetAsync(cancellationToken)
            .ConfigureAwait(false);

        var toInsert = conflicts
            .Where(c => !existingOpenFields.Contains(c.FieldName))
            .Select(c => new DataConflict
            {
                ConflictId = Guid.NewGuid(),
                PatientProfileId = patientProfileId,
                ConflictType = MapToConflictType(c.DataType),
                FieldName = c.FieldName,
                Value1 = c.Value1,
                SourceDocumentId1 = c.SourceDocumentId1,
                Value2 = c.Value2,
                SourceDocumentId2 = c.SourceDocumentId2,
                ResolutionStatus = ResolutionStatus.Open,
            })
            .ToList();

        if (toInsert.Count > 0)
            _db.DataConflicts.AddRange(toInsert);

        return toInsert.Count;
    }

    /// <summary>Serialises a filtered set of canonical records to compact JSON.</summary>
    private static string? SerializeCategory(
        IReadOnlyList<CanonicalRecord> records,
        ExtractedDataType dataType)
    {
        var items = records
            .Where(r => r.DataType == dataType)
            .Select(r => new
            {
                field = r.FieldName,
                value = r.FieldValue,
                source = r.SourceDocumentId,
                confidence = r.Confidence,
                isVerified = r.IsVerified,
            })
            .ToList();

        return items.Count == 0 ? null : JsonSerializer.Serialize(items);
    }

    /// <summary>
    /// Merges NER medication records with free-text intake medications (AC-01).
    /// </summary>
    private static string? MergeMedicationSources(
        IReadOnlyList<CanonicalRecord> records,
        string? intakeMedicationsJson)
    {
        var items = records
            .Where(r => r.DataType == ExtractedDataType.Medication)
            .Select(r => (object)new
            {
                field = r.FieldName,
                value = r.FieldValue,
                source = r.SourceDocumentId.ToString(),
                confidence = r.Confidence,
                isVerified = r.IsVerified,
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(intakeMedicationsJson))
        {
            items.Add(new
            {
                field = "medications",
                value = ExtractIntakeFreeText(intakeMedicationsJson),
                source = IntakeSourceLabel,
                confidence = 1.0m,
                isVerified = false,
            });
        }

        return items.Count == 0 ? null : JsonSerializer.Serialize(items);
    }

    /// <summary>
    /// Merges NER allergy records with free-text intake allergies (AC-01).
    /// </summary>
    private static string? MergeAllergyAndIntakeSources(
        IReadOnlyList<CanonicalRecord> records,
        string? intakeAllergiesJson)
    {
        var items = records
            .Where(r => r.DataType == ExtractedDataType.Allergy)
            .Select(r => (object)new
            {
                field = r.FieldName,
                value = r.FieldValue,
                source = r.SourceDocumentId.ToString(),
                confidence = r.Confidence,
                isVerified = r.IsVerified,
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(intakeAllergiesJson))
        {
            items.Add(new
            {
                field = "allergies",
                value = ExtractIntakeFreeText(intakeAllergiesJson),
                source = IntakeSourceLabel,
                confidence = 1.0m,
                isVerified = false,
            });
        }

        return items.Count == 0 ? null : JsonSerializer.Serialize(items);
    }

    /// <summary>Maps an <see cref="ExtractedDataType"/> to the closest <see cref="ConflictType"/>.</summary>
    private static ConflictType MapToConflictType(ExtractedDataType dataType) =>
        dataType switch
        {
            ExtractedDataType.Medication => ConflictType.Medication,
            ExtractedDataType.Allergy => ConflictType.Allergy,
            ExtractedDataType.Diagnosis => ConflictType.Diagnosis,
            // Inferred decision: Vital and Procedure fields map to the Diagnosis conflict bucket
            // because ConflictType does not have Vital/Procedure variants in this domain version.
            _ => ConflictType.Diagnosis,
        };

    private static string ExtractIntakeFreeText(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString() ?? string.Empty;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("text", out var text) &&
                text.ValueKind == JsonValueKind.String)
            {
                return text.GetString() ?? string.Empty;
            }
        }
        catch
        {
            // Legacy rows may contain plain text from earlier builds.
        }

        return json;
    }
}
