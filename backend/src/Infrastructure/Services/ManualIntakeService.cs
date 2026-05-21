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
/// Idempotency: <see cref="SubmitAsync"/> returns <c>FailureCode = "ALREADY_SUBMITTED"</c>
/// when a record with <c>CompletedAt IS NOT NULL</c> already exists, preventing duplicate
/// submissions (Edge Case).
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
    // Submit (AC-01) + idempotency guard (Edge Case)
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<ManualIntakeResult> SubmitAsync(
        SubmitManualIntakeRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await ResolveProfileAsync(request.PatientUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            _logger.LogWarning(
                "ManualIntake submit failed: PatientProfile not found for UserId {UserId}.",
                request.PatientUserId);

            return new ManualIntakeResult(
                Success: false,
                IntakeId: null,
                FailureReason: "Patient profile not found.",
                FailureCode: "PATIENT_NOT_FOUND");
        }

        var existing = await _db.IntakeRecords
            .FirstOrDefaultAsync(
                r => r.AppointmentId == request.AppointmentId
                     && r.PatientProfileId == profile.PatientProfileId,
                cancellationToken)
            .ConfigureAwait(false);

        // Edge Case: duplicate submission — idempotency guard
        if (existing?.CompletedAt is not null)
        {
            _logger.LogInformation(
                "ManualIntake submit — idempotency hit for AppointmentId {AppointmentId}.",
                request.AppointmentId);

            return new ManualIntakeResult(
                Success: true,
                IntakeId: existing.IntakeId,
                FailureReason: null,
                FailureCode: "ALREADY_SUBMITTED");
        }

        var now = DateTime.UtcNow;
        var medicalHistory = SerializeMedicalHistory(request.ChronicConditions, request.PastSurgeries, request.FamilyHistory);
        var currentSymptoms = SerializeSymptoms(request.SymptomsDescription, request.SymptomOnset, request.SymptomSeverity);

        if (existing is not null)
        {
            existing.MedicalHistory = medicalHistory;
            existing.CurrentSymptoms = currentSymptoms;
            existing.Medications = request.CurrentMedications;
            existing.Allergies = request.KnownAllergies;
            existing.ReasonForVisit = request.ReasonForVisit;
            existing.CompletedAt = now;
            existing.LastModifiedAt = now;
        }
        else
        {
            existing = new IntakeRecord
            {
                IntakeId = Guid.NewGuid(),
                PatientProfileId = profile.PatientProfileId,
                AppointmentId = request.AppointmentId,
                IntakeMode = IntakeMode.Manual,
                MedicalHistory = medicalHistory,
                CurrentSymptoms = currentSymptoms,
                Medications = request.CurrentMedications,
                Allergies = request.KnownAllergies,
                ReasonForVisit = request.ReasonForVisit,
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
        var profile = await ResolveProfileAsync(request.PatientUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return new IntakeDraftResult(
                Success: false,
                IntakeId: null,
                FailureReason: "Patient profile not found.");
        }

        var existing = await _db.IntakeRecords
            .FirstOrDefaultAsync(
                r => r.AppointmentId == request.AppointmentId
                     && r.PatientProfileId == profile.PatientProfileId,
                cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;

        if (existing is not null)
        {
            // Merge: only overwrite columns when the incoming request provides data.
            if (HasMedicalHistoryData(request))
                existing.MedicalHistory = SerializeMedicalHistory(request.ChronicConditions, request.PastSurgeries, request.FamilyHistory);

            if (HasSymptomsData(request))
                existing.CurrentSymptoms = SerializeSymptoms(request.SymptomsDescription, request.SymptomOnset, request.SymptomSeverity);

            if (request.CurrentMedications is not null)
                existing.Medications = request.CurrentMedications;

            if (request.KnownAllergies is not null)
                existing.Allergies = request.KnownAllergies;

            if (request.ReasonForVisit is not null)
                existing.ReasonForVisit = request.ReasonForVisit;

            existing.LastModifiedAt = now;
        }
        else
        {
            existing = new IntakeRecord
            {
                IntakeId = Guid.NewGuid(),
                PatientProfileId = profile.PatientProfileId,
                AppointmentId = request.AppointmentId,
                IntakeMode = IntakeMode.Manual,
                MedicalHistory = HasMedicalHistoryData(request)
                    ? SerializeMedicalHistory(request.ChronicConditions, request.PastSurgeries, request.FamilyHistory)
                    : null,
                CurrentSymptoms = HasSymptomsData(request)
                    ? SerializeSymptoms(request.SymptomsDescription, request.SymptomOnset, request.SymptomSeverity)
                    : null,
                Medications = request.CurrentMedications,
                Allergies = request.KnownAllergies,
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
            FailureReason: null);
    }

    // -------------------------------------------------------------------------
    // Draft load (AC-02)
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<IntakeDraftDto?> GetDraftAsync(
        Guid patientUserId,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var profile = await ResolveProfileAsync(patientUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
            return null;

        var record = await _db.IntakeRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.AppointmentId == appointmentId
                     && r.PatientProfileId == profile.PatientProfileId,
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
            CurrentMedications: record.Medications,
            KnownAllergies: record.Allergies,
            ReasonForVisit: string.IsNullOrEmpty(record.ReasonForVisit) ? null : record.ReasonForVisit,
            IsSubmitted: record.CompletedAt.HasValue,
            LastModifiedAt: record.LastModifiedAt);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private Task<PatientProfile?> ResolveProfileAsync(Guid userId, CancellationToken ct)
        => _db.PatientProfiles
               .FirstOrDefaultAsync(p => p.UserId == userId, ct);

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
}
