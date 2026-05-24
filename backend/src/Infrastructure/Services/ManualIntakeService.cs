using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Concrete implementation of <see cref="IManualIntakeService"/>.
///
/// Persists manual intake data to <see cref="IntakeRecord"/> using EF Core.
/// Clinical section columns (<c>MedicalHistory</c>, <c>CurrentSymptoms</c>) store
/// JSON-serialised payloads to preserve structured sub-fields within the existing
/// JSONB schema without a migration.
///
/// Drafts are updated in place while <c>CompletedAt IS NULL</c>; completed submissions
/// are append-only history entries so repeated intake submissions remain visible.
/// </summary>
public sealed class ManualIntakeService : IManualIntakeService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ApplicationDbContext _db;
    private readonly ILogger<ManualIntakeService> _logger;

    public ManualIntakeService(
        ApplicationDbContext db,
        ILogger<ManualIntakeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // Submit (AC-01)
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<ManualIntakeResult> SubmitAsync(
        SubmitManualIntakeRequest request,
        CancellationToken cancellationToken = default)
    {
        var owner = await ResolveOwnerAsync(
                request.ActorUserId,
                request.ActorRole,
                request.AppointmentId,
                cancellationToken)
            .ConfigureAwait(false);

        if (!owner.Success || owner.Profile is null)
        {
            _logger.LogWarning(
                "ManualIntake submit failed: {FailureCode} for ActorUserId {ActorUserId}, AppointmentId {AppointmentId}.",
                owner.FailureCode,
                request.ActorUserId,
                request.AppointmentId);

            return new ManualIntakeResult(
                Success: false,
                IntakeId: null,
                FailureReason: owner.FailureReason,
                FailureCode: owner.FailureCode);
        }

        var existing = await _db.IntakeRecords
            .OrderByDescending(r => r.LastModifiedAt)
            .FirstOrDefaultAsync(
                r => r.AppointmentId == request.AppointmentId
                     && r.PatientProfileId == owner.Profile.PatientProfileId
                     && r.IntakeMode == IntakeMode.Manual
                     && r.CompletedAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;
        var reasonForVisit = request.ReasonForVisit.Trim();
        var medicalHistory = SerializeMedicalHistory(request.ChronicConditions, request.PastSurgeries, request.FamilyHistory);
        var currentSymptoms = SerializeSymptoms(request.SymptomsDescription, request.SymptomOnset, request.SymptomSeverity);

        if (existing is not null)
        {
            existing.IntakeMode = IntakeMode.Manual;
            existing.MedicalHistory = medicalHistory;
            existing.CurrentSymptoms = currentSymptoms;
            existing.Medications = SerializeFreeText(request.CurrentMedications);
            existing.Allergies = SerializeFreeText(request.KnownAllergies);
            existing.ReasonForVisit = reasonForVisit;
            existing.CompletedAt = now;
            existing.LastModifiedAt = now;
        }
        else
        {
            existing = new IntakeRecord
            {
                IntakeId = Guid.NewGuid(),
                PatientProfileId = owner.Profile.PatientProfileId,
                AppointmentId = request.AppointmentId,
                IntakeMode = IntakeMode.Manual,
                MedicalHistory = medicalHistory,
                CurrentSymptoms = currentSymptoms,
                Medications = SerializeFreeText(request.CurrentMedications),
                Allergies = SerializeFreeText(request.KnownAllergies),
                ReasonForVisit = reasonForVisit,
                CompletedAt = now,
                LastModifiedAt = now,
            };

            _db.IntakeRecords.Add(existing);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "ManualIntake submitted: IntakeId {IntakeId} for AppointmentId {AppointmentId}.",
            existing.IntakeId,
            request.AppointmentId);

        return new ManualIntakeResult(
            Success: true,
            IntakeId: existing.IntakeId,
            FailureReason: null,
            FailureCode: null);
    }

    // -------------------------------------------------------------------------
    // Draft save (AC-02)
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<IntakeDraftResult> SaveDraftAsync(
        SaveIntakeDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var owner = await ResolveOwnerAsync(
                request.ActorUserId,
                request.ActorRole,
                request.AppointmentId,
                cancellationToken)
            .ConfigureAwait(false);

        if (!owner.Success || owner.Profile is null)
        {
            return new IntakeDraftResult(
                Success: false,
                IntakeId: null,
                FailureReason: owner.FailureReason,
                FailureCode: owner.FailureCode);
        }

        var existing = await _db.IntakeRecords
            .OrderByDescending(r => r.LastModifiedAt)
            .FirstOrDefaultAsync(
                r => r.AppointmentId == request.AppointmentId
                     && r.PatientProfileId == owner.Profile.PatientProfileId
                     && r.IntakeMode == IntakeMode.Manual
                     && r.CompletedAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;

        if (existing is not null)
        {
            existing.IntakeMode = IntakeMode.Manual;

            // Merge: only overwrite columns when the incoming request provides data.
            if (HasMedicalHistoryData(request))
                existing.MedicalHistory = SerializeMedicalHistory(request.ChronicConditions, request.PastSurgeries, request.FamilyHistory);

            if (HasSymptomsData(request))
                existing.CurrentSymptoms = SerializeSymptoms(request.SymptomsDescription, request.SymptomOnset, request.SymptomSeverity);

            if (request.CurrentMedications is not null)
                existing.Medications = SerializeFreeText(request.CurrentMedications);

            if (request.KnownAllergies is not null)
                existing.Allergies = SerializeFreeText(request.KnownAllergies);

            if (request.ReasonForVisit is not null)
                existing.ReasonForVisit = request.ReasonForVisit;

            existing.LastModifiedAt = now;
        }
        else
        {
            existing = new IntakeRecord
            {
                IntakeId = Guid.NewGuid(),
                PatientProfileId = owner.Profile.PatientProfileId,
                AppointmentId = request.AppointmentId,
                IntakeMode = IntakeMode.Manual,
                MedicalHistory = HasMedicalHistoryData(request)
                    ? SerializeMedicalHistory(request.ChronicConditions, request.PastSurgeries, request.FamilyHistory)
                    : null,
                CurrentSymptoms = HasSymptomsData(request)
                    ? SerializeSymptoms(request.SymptomsDescription, request.SymptomOnset, request.SymptomSeverity)
                    : null,
                Medications = SerializeFreeText(request.CurrentMedications),
                Allergies = SerializeFreeText(request.KnownAllergies),
                ReasonForVisit = request.ReasonForVisit ?? string.Empty,
                CompletedAt = null,
                LastModifiedAt = now,
            };

            _db.IntakeRecords.Add(existing);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new IntakeDraftResult(
            Success: true,
            IntakeId: existing.IntakeId,
            FailureReason: null,
            FailureCode: null);
    }

    // -------------------------------------------------------------------------
    // Draft load (AC-02)
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<IntakeDraftDto?> GetDraftAsync(
        Guid actorUserId,
        string actorRole,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var owner = await ResolveOwnerAsync(actorUserId, actorRole, appointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (!owner.Success || owner.Profile is null)
            return null;

        var record = await _db.IntakeRecords
            .AsNoTracking()
            .OrderByDescending(r => r.LastModifiedAt)
            .FirstOrDefaultAsync(
                r => r.AppointmentId == appointmentId
                     && r.PatientProfileId == owner.Profile.PatientProfileId
                     && r.IntakeMode == IntakeMode.Manual
                     && r.CompletedAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
            return null;

        var medHistory = DeserializeMedicalHistory(record.MedicalHistory);
        var symptoms = DeserializeSymptoms(record.CurrentSymptoms);

        return new IntakeDraftDto(
            IntakeId: record.IntakeId,
            ChronicConditions: medHistory?.ChronicConditions,
            PastSurgeries: medHistory?.PastSurgeries,
            FamilyHistory: medHistory?.FamilyHistory,
            SymptomsDescription: symptoms?.Description,
            SymptomOnset: symptoms?.Onset,
            SymptomSeverity: symptoms?.Severity,
            CurrentMedications: DeserializeFreeText(record.Medications),
            KnownAllergies: DeserializeFreeText(record.Allergies),
            ReasonForVisit: string.IsNullOrEmpty(record.ReasonForVisit) ? null : record.ReasonForVisit,
            IsSubmitted: record.CompletedAt.HasValue,
            LastModifiedAt: record.LastModifiedAt);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<IntakeOwnerResolution> ResolveOwnerAsync(
        Guid actorUserId,
        string actorRole,
        Guid appointmentId,
        CancellationToken ct)
    {
        if (actorUserId == Guid.Empty || appointmentId == Guid.Empty || string.IsNullOrWhiteSpace(actorRole))
        {
            return IntakeOwnerResolution.Failed(
                "INVALID_REQUEST",
                "ActorUserId, ActorRole, and AppointmentId are required.");
        }

        var isStaffScoped = IsStaffScopedRole(actorRole);

        var patientUserId = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentId == appointmentId && (isStaffScoped || a.PatientId == actorUserId))
            .Select(a => (Guid?)a.PatientId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (patientUserId is null)
        {
            return IntakeOwnerResolution.Failed(
                "APPOINTMENT_NOT_FOUND",
                "Appointment was not found for this intake.");
        }

        var profile = await _db.PatientProfiles
            .FirstOrDefaultAsync(p => p.UserId == patientUserId.Value, ct)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return IntakeOwnerResolution.Failed(
                "PATIENT_NOT_FOUND",
                "Patient profile not found.");
        }

        return IntakeOwnerResolution.Resolved(profile);
    }

    private static bool IsStaffScopedRole(string actorRole)
        => actorRole.Equals("Staff", StringComparison.OrdinalIgnoreCase)
           || actorRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);

    private static bool HasMedicalHistoryData(SaveIntakeDraftRequest r)
        => r.ChronicConditions is not null || r.PastSurgeries is not null || r.FamilyHistory is not null;

    private static bool HasSymptomsData(SaveIntakeDraftRequest r)
        => r.SymptomsDescription is not null || r.SymptomOnset is not null || r.SymptomSeverity is not null;

    private static string? SerializeMedicalHistory(string? conditions, string? surgeries, string? family)
    {
        if (conditions is null && surgeries is null && family is null)
            return null;

        return JsonSerializer.Serialize(
            new { chronicConditions = conditions, pastSurgeries = surgeries, familyHistory = family },
            JsonOptions);
    }

    private static string? SerializeSymptoms(string? description, string? onset, string? severity)
    {
        if (description is null && onset is null && severity is null)
            return null;

        return JsonSerializer.Serialize(
            new { description, onset, severity },
            JsonOptions);
    }

    private static ManualMedicalHistory? DeserializeMedicalHistory(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ManualMedicalHistory>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static ManualSymptoms? DeserializeSymptoms(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ManualSymptoms>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeFreeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return JsonSerializer.Serialize(new IntakeFreeText(value.Trim()), JsonOptions);
    }

    private static string? DeserializeFreeText(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return root.GetString();

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("text", out var text) &&
                text.ValueKind == JsonValueKind.String)
            {
                return text.GetString();
            }

            return json;
        }
        catch
        {
            return json;
        }
    }

    private sealed record IntakeFreeText(string Text);

    private sealed record IntakeOwnerResolution(
        bool Success,
        PatientProfile? Profile,
        string? FailureCode,
        string? FailureReason)
    {
        public static IntakeOwnerResolution Resolved(PatientProfile profile)
            => new(true, profile, null, null);

        public static IntakeOwnerResolution Failed(string code, string reason)
            => new(false, null, code, reason);
    }
}
