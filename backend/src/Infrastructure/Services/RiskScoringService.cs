using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Deterministic 4-factor weighted no-show risk scoring algorithm (US_041, FR-033, AIR-007).
///
/// Factor weights and scoring tables are compile-time constants to guarantee reproducibility (AC-02).
///
/// Factor 1 — Historical no-show count  (0–40 pts, weight 40%)
///   0 no-shows → 0 pts
///   1           → 8 pts
///   2           → 16 pts
///   3           → 24 pts
///   4           → 32 pts
///   5+          → 40 pts  (capped)
///
/// Factor 2 — Appointment lead time / gap  (0–25 pts, weight 25%, inversely proportional)
///   Walk-in (BookingType.WalkIn or ≤0 days) → 10 pts  (capped per Edge Case: patient is present)
///   1 day                                   → 25 pts
///   2–3 days                                → 20 pts
///   4–7 days                                → 15 pts
///   8–14 days                               → 10 pts
///   15–30 days                              → 6 pts
///   31+ days                                → 2 pts
///
/// Factor 3 — Time-of-day pattern  (0–20 pts, weight 20%)
///   "early_morning"  → 20 pts
///   "late_afternoon" → 15 pts
///   null / unknown   → 10 pts  (incomplete data — moderate risk; AC-06)
///   "morning"        → 8 pts
///   "afternoon"      → 5 pts
///   "midday"         → 5 pts
///
/// Factor 4 — New-patient flag  (0–15 pts, weight 15%)
///   IsNewPatient = true  → 15 pts
///   IsNewPatient = false → 0 pts
///
/// Risk tier thresholds:
///   Low    → 0–33
///   Medium → 34–66
///   High   → 67–100
/// </summary>
public sealed class RiskScoringService : IRiskScoringService
{
    // --- Factor weight ceilings (constants guarantee AC-02 reproducibility) ---
    private const decimal MaxNoShowPts   = 40m;
    private const decimal MaxLeadTimePts = 25m;
    private const decimal MaxTimeOfDayPts = 20m;
    private const decimal MaxNewPatientPts = 15m;

    private const int NoShowCountCap = 5;

    private const decimal LowTierMax    = 33m;
    private const decimal MediumTierMax = 66m;

    private readonly ApplicationDbContext _db;
    private readonly ILogger<RiskScoringService> _logger;

    public RiskScoringService(ApplicationDbContext db, ILogger<RiskScoringService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<RiskScoringResult> CalculateAndPersistAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        // Load appointment + slot (for lead time) + patient profile + risk factors
        var appointment = await _db.Appointments
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken)
            .ConfigureAwait(false);

        if (appointment is null)
        {
            _logger.LogWarning(
                "RiskScoringService: Appointment {AppointmentId} not found. Skipping scoring.",
                appointmentId);

            return new RiskScoringResult(
                Score: 50m,
                Tier: NoShowRiskTier.Medium,
                IsDataPending: true,
                MissingFactors: ["appointment"]);
        }

        // Resolve patient profile via PatientId (UserId link)
        var riskFactor = await _db.NoShowRiskFactors
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.PatientProfile.UserId == appointment.PatientId,
                cancellationToken)
            .ConfigureAwait(false);

        // Edge Case: insufficient data — default to Medium risk with DataPending flag (task Edge Cases)
        if (riskFactor is null)
        {
            _logger.LogInformation(
                "Incomplete risk assessment for AppointmentId={AppointmentId}: NoShowRiskFactor record absent. " +
                "Defaulting to Medium risk.",
                appointmentId);

            appointment.NoShowRiskScore = 50m;
            appointment.NoShowRiskTier = NoShowRiskTier.Medium;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new RiskScoringResult(
                Score: 50m,
                Tier: NoShowRiskTier.Medium,
                IsDataPending: true,
                MissingFactors: ["no_show_history", "time_of_day", "new_patient_flag"]);
        }

        var missingFactors = new List<string>();

        // --- Factor 1: Historical no-show count ---
        var noShowPts = CalculateNoShowPoints(riskFactor.HistoricalNoShowCount);

        // --- Factor 2: Appointment lead time (inverse) ---
        var leadTimeDays = appointment.Slot is not null
            ? (appointment.Slot.StartTime - appointment.CreatedAt).TotalDays
            : -1;

        bool isWalkIn = appointment.BookingType == BookingType.WalkIn;
        var leadTimePts = CalculateLeadTimePoints(leadTimeDays, isWalkIn);

        // --- Factor 3: Time-of-day pattern ---
        bool timeOfDayMissing = string.IsNullOrWhiteSpace(riskFactor.PreferredTimeOfDay);
        if (timeOfDayMissing)
            missingFactors.Add("time_of_day");
        var timeOfDayPts = CalculateTimeOfDayPoints(riskFactor.PreferredTimeOfDay);

        // --- Factor 4: New-patient flag ---
        var newPatientPts = riskFactor.IsNewPatient ? MaxNewPatientPts : 0m;

        // Log incomplete assessment when any factor is absent (AC-06)
        if (missingFactors.Count > 0)
        {
            _logger.LogInformation(
                "Incomplete risk assessment for AppointmentId={AppointmentId}. " +
                "Missing factors: {MissingFactors}. Score computed from available factors.",
                appointmentId,
                string.Join(", ", missingFactors));
        }

        var rawScore = noShowPts + leadTimePts + timeOfDayPts + newPatientPts;

        // Clamp to valid range — floating-point safety net
        var score = Math.Clamp(Math.Round(rawScore, 2), 0m, 100m);
        var tier = ClassifyTier(score);

        // Persist score and tier onto appointment
        appointment.NoShowRiskScore = score;
        appointment.NoShowRiskTier = tier;
        appointment.UpdatedAt = DateTime.UtcNow;

        // Update risk factor's last-calculated timestamp
        var trackedFactor = await _db.NoShowRiskFactors
            .FirstOrDefaultAsync(
                r => r.PatientProfile.UserId == appointment.PatientId,
                cancellationToken)
            .ConfigureAwait(false);

        if (trackedFactor is not null)
            trackedFactor.LastCalculatedAt = appointment.UpdatedAt;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Risk score calculated. AppointmentId={AppointmentId}, Score={Score}, Tier={Tier}, " +
            "NoShowPts={NoShowPts}, LeadTimePts={LeadTimePts}, TimeOfDayPts={TimeOfDayPts}, NewPatientPts={NewPatientPts}.",
            appointmentId, score, tier, noShowPts, leadTimePts, timeOfDayPts, newPatientPts);

        return new RiskScoringResult(
            Score: score,
            Tier: tier,
            IsDataPending: false,
            MissingFactors: missingFactors.AsReadOnly());
    }

    // ---------------------------------------------------------------------------
    // Private factor calculators (static — pure functions for reproducibility)
    // ---------------------------------------------------------------------------

    private static decimal CalculateNoShowPoints(int count)
    {
        if (count <= 0) return 0m;
        var capped = Math.Min(count, NoShowCountCap);
        return Math.Round((decimal)capped / NoShowCountCap * MaxNoShowPts, 2);
    }

    private static decimal CalculateLeadTimePoints(double leadTimeDays, bool isWalkIn)
    {
        // Edge Case: walk-in — patient is present, lead time factor must not inflate risk (US_041 Edge Case)
        if (isWalkIn || leadTimeDays <= 0)
            return 10m;

        return leadTimeDays switch
        {
            <= 1  => MaxLeadTimePts,          // 25 pts — very short lead time
            <= 3  => 20m,
            <= 7  => 15m,
            <= 14 => 10m,
            <= 30 => 6m,
            _     => 2m,                      // 31+ days — ample notice, low urgency risk
        };
    }

    private static decimal CalculateTimeOfDayPoints(string? preferredTimeOfDay)
    {
        if (string.IsNullOrWhiteSpace(preferredTimeOfDay))
            return 10m; // moderate default for missing data (AC-06)

        return preferredTimeOfDay.ToLowerInvariant() switch
        {
            "early_morning"  => MaxTimeOfDayPts,   // 20 pts — highest no-show rate
            "late_afternoon" => 15m,
            "morning"        => 8m,
            "afternoon"      => 5m,
            "midday"         => 5m,
            _                => 10m,               // unknown bucket → moderate
        };
    }

    private static NoShowRiskTier ClassifyTier(decimal score) =>
        score switch
        {
            <= LowTierMax    => NoShowRiskTier.Low,
            <= MediumTierMax => NoShowRiskTier.Medium,
            _                => NoShowRiskTier.High,
        };
}
