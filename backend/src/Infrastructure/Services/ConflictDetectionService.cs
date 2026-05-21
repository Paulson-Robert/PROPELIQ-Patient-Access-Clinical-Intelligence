using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Manages the full conflict lifecycle for a patient (US_039, AC-01):
/// detects value mismatches across extracted data sources, persists new conflicts,
/// re-opens resolved conflicts when contradicting data arrives, and notifies staff.
///
/// Detection strategy delegates to <see cref="IDataDeduplicationService"/>, keeping
/// mismatch logic in one place (DRY).
/// </summary>
public sealed class ConflictDetectionService : IConflictDetectionService
{
    private readonly ApplicationDbContext _db;
    private readonly IDataDeduplicationService _dedup;
    private readonly IStaffNotificationService _staffNotifications;
    private readonly ILogger<ConflictDetectionService> _logger;

    public ConflictDetectionService(
        ApplicationDbContext db,
        IDataDeduplicationService dedup,
        IStaffNotificationService staffNotifications,
        ILogger<ConflictDetectionService> logger)
    {
        _db = db;
        _dedup = dedup;
        _staffNotifications = staffNotifications;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DataConflictDto>> GetOpenConflictsAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default)
    {
        // AC-01: Surface all open conflicts for the patient.
        var rows = await _db.DataConflicts
            .AsNoTracking()
            .Where(c =>
                c.PatientProfileId == patientProfileId
                && c.ResolutionStatus == ResolutionStatus.Open)
            .OrderBy(c => c.FieldName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.ConvertAll(ToDto).AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task<ConflictDetectionResult> DetectAndPersistAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default)
    {
        // -------------------------------------------------------------------
        // 1. Load extracted records for the patient (AC-01: across sources)
        // -------------------------------------------------------------------
        var records = await _db.ExtractedDataRecords
            .AsNoTracking()
            .Where(r => r.PatientProfileId == patientProfileId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (records.Count == 0)
        {
            _logger.LogDebug(
                "ConflictDetection: No extracted records found for PatientProfileId {PatientProfileId}.",
                patientProfileId);

            return new ConflictDetectionResult(0, 0);
        }

        // -------------------------------------------------------------------
        // 2. Detect mismatches via deduplication (AC-01)
        // -------------------------------------------------------------------
        var deduplicationResult = await _dedup
            .DeduplicateAsync(records, cancellationToken)
            .ConfigureAwait(false);

        if (deduplicationResult.Conflicts.Count == 0)
        {
            return new ConflictDetectionResult(0, 0);
        }

        // -------------------------------------------------------------------
        // 3. Load existing conflict rows for the detected field names
        // -------------------------------------------------------------------
        var detectedFieldNames = deduplicationResult.Conflicts
            .Select(c => c.FieldName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingRows = await _db.DataConflicts
            .Where(c =>
                c.PatientProfileId == patientProfileId
                && detectedFieldNames.Contains(c.FieldName))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var existingByField = existingRows.ToDictionary(
            c => c.FieldName,
            c => c,
            StringComparer.OrdinalIgnoreCase);

        // -------------------------------------------------------------------
        // 4. Classify each detected conflict: new / already-open / re-open
        // -------------------------------------------------------------------
        var toInsert = new List<DataConflict>();
        var toReopen = new List<DataConflict>();

        foreach (var conflict in deduplicationResult.Conflicts)
        {
            if (!existingByField.TryGetValue(conflict.FieldName, out var existing))
            {
                // New conflict — insert as Open.
                toInsert.Add(new DataConflict
                {
                    ConflictId = Guid.NewGuid(),
                    PatientProfileId = patientProfileId,
                    ConflictType = MapConflictType(conflict.DataType),
                    FieldName = conflict.FieldName,
                    Value1 = conflict.Value1,
                    SourceDocumentId1 = conflict.SourceDocumentId1,
                    Value2 = conflict.Value2,
                    SourceDocumentId2 = conflict.SourceDocumentId2,
                    ResolutionStatus = ResolutionStatus.Open,
                });
            }
            else if (existing.ResolutionStatus == ResolutionStatus.Resolved)
            {
                // Edge case: new contradicting data arrived after resolution — re-open.
                existing.ResolutionStatus = ResolutionStatus.Open;
                existing.Value1 = conflict.Value1;
                existing.SourceDocumentId1 = conflict.SourceDocumentId1;
                existing.Value2 = conflict.Value2;
                existing.SourceDocumentId2 = conflict.SourceDocumentId2;
                existing.ResolvedByUserId = null;
                existing.ResolvedAt = null;
                existing.ResolutionNotes = null;
                toReopen.Add(existing);
            }
            // Already Open — no change needed (idempotent).
        }

        // -------------------------------------------------------------------
        // 5. Persist inserts and updates
        // -------------------------------------------------------------------
        if (toInsert.Count > 0)
            _db.DataConflicts.AddRange(toInsert);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // -------------------------------------------------------------------
        // 6. Edge case: notify all staff when resolved conflicts are re-opened
        // -------------------------------------------------------------------
        if (toReopen.Count > 0)
            await NotifyStaffOfReopenedConflictsAsync(patientProfileId, toReopen, cancellationToken)
                .ConfigureAwait(false);

        _logger.LogInformation(
            "ConflictDetection: PatientProfileId {PatientProfileId} — " +
            "NewInserted={NewInserted} Reopened={Reopened}.",
            patientProfileId,
            toInsert.Count,
            toReopen.Count);

        return new ConflictDetectionResult(toInsert.Count, toReopen.Count);
    }

    /// <inheritdoc/>
    public async Task<ConflictResolutionResult> ResolveAsync(
        ResolveConflictRequest request,
        CancellationToken cancellationToken = default)
    {
        // -------------------------------------------------------------------
        // Guard: validate inputs at the system boundary
        // -------------------------------------------------------------------
        if (request.ConflictId == Guid.Empty)
            return new ConflictResolutionResult(false, "INVALID_REQUEST", "ConflictId is required.");

        if (string.IsNullOrWhiteSpace(request.AcceptedValue))
            return new ConflictResolutionResult(false, "INVALID_REQUEST", "AcceptedValue is required.");

        // -------------------------------------------------------------------
        // Load the conflict row
        // -------------------------------------------------------------------
        var conflict = await _db.DataConflicts
            .FirstOrDefaultAsync(c => c.ConflictId == request.ConflictId, cancellationToken)
            .ConfigureAwait(false);

        if (conflict is null)
        {
            return new ConflictResolutionResult(
                false, "CONFLICT_NOT_FOUND",
                $"DataConflict {request.ConflictId} was not found.");
        }

        if (conflict.ResolutionStatus == ResolutionStatus.Resolved)
        {
            return new ConflictResolutionResult(
                false, "CONFLICT_ALREADY_RESOLVED",
                $"DataConflict {request.ConflictId} is already resolved.");
        }

        // -------------------------------------------------------------------
        // AC-02: Persist the selected value against the conflict row
        // -------------------------------------------------------------------
        var resolvedAt = DateTime.UtcNow;

        conflict.ResolutionStatus = ResolutionStatus.Resolved;
        conflict.ResolvedByUserId = request.ResolvedByUserId;
        conflict.ResolvedAt = resolvedAt;
        conflict.ResolutionNotes = string.IsNullOrWhiteSpace(request.Notes)
            ? request.AcceptedValue
            : $"{request.AcceptedValue} — {request.Notes.Trim()}";

        // -------------------------------------------------------------------
        // AC-03: Append immutable audit trail entry
        // -------------------------------------------------------------------
        var auditDetails = System.Text.Json.JsonSerializer.Serialize(new
        {
            fieldName = conflict.FieldName,
            acceptedValue = request.AcceptedValue,
            notes = request.Notes,
        });

        _db.AuditLogs.Add(new AuditLog
        {
            Timestamp = resolvedAt,
            ActorUserId = request.ResolvedByUserId,
            ActorRole = request.ResolvedByRole,
            ActionType = "CONFLICT_RESOLVED",
            ResourceType = "DataConflict",
            ResourceId = request.ConflictId.ToString(),
            Details = auditDetails,
        });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "ConflictResolution: ConflictId={ConflictId} FieldName={FieldName} " +
            "ResolvedBy={ResolvedByUserId} AcceptedValue={AcceptedValue}.",
            request.ConflictId,
            conflict.FieldName,
            request.ResolvedByUserId,
            request.AcceptedValue);

        return new ConflictResolutionResult(true, null, null);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Edge case: queries all active staff users and creates a warning notification
    /// for each re-opened conflict field.
    /// </summary>
    private async Task NotifyStaffOfReopenedConflictsAsync(
        Guid patientProfileId,
        IReadOnlyList<DataConflict> reopened,
        CancellationToken cancellationToken)
    {
        var staffIds = await _db.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Staff && u.IsActive)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (staffIds.Count == 0)
        {
            _logger.LogWarning(
                "ConflictDetection: No active staff users found to notify for re-opened conflicts " +
                "on PatientProfileId {PatientProfileId}.",
                patientProfileId);
            return;
        }

        var fieldNames = string.Join(", ", reopened.Select(c => c.FieldName));
        var title = $"Conflict re-opened — {reopened.Count} field(s) require review";
        var message = $"New data contradicts a previously resolved conflict for: {fieldNames}.";

        await _staffNotifications.CreateAsync(
            new CreateStaffNotificationRequest(
                StaffUserIds: staffIds,
                Variant: StaffNotificationVariant.Warning.ToString(),
                Title: title,
                Message: message),
            cancellationToken)
            .ConfigureAwait(false);
    }

    private static DataConflictDto ToDto(DataConflict c) =>
        new(c.ConflictId,
            c.PatientProfileId,
            c.FieldName,
            c.Value1,
            c.SourceDocumentId1,
            c.Value2,
            c.SourceDocumentId2,
            c.ResolutionStatus,
            c.ResolvedByUserId,
            c.ResolvedAt,
            c.ResolutionNotes);

    private static ConflictType MapConflictType(ExtractedDataType dataType) =>
        dataType switch
        {
            ExtractedDataType.Medication => ConflictType.Medication,
            ExtractedDataType.Allergy    => ConflictType.Allergy,
            ExtractedDataType.Diagnosis  => ConflictType.Diagnosis,
            _                            => ConflictType.Diagnosis, // Vital/Procedure map to nearest
        };
}
