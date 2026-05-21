using Application.Interfaces;
using MediatR;

namespace Application.EventHandlers;

/// <summary>
/// Published when an appointment's no-show risk score must be (re)calculated.
/// Raised on appointment creation (AC-01) and reschedule (AC-03).
/// </summary>
public sealed record AppointmentRiskScoreRequested(Guid AppointmentId) : INotification;

/// <summary>
/// Triggers risk score recalculation via <see cref="IRiskScoringService"/> whenever
/// an <see cref="AppointmentRiskScoreRequested"/> notification is published (AC-03).
/// </summary>
public sealed class RecalculateRiskHandler : INotificationHandler<AppointmentRiskScoreRequested>
{
    private readonly IRiskScoringService _riskScoring;

    public RecalculateRiskHandler(IRiskScoringService riskScoring)
    {
        _riskScoring = riskScoring;
    }

    public async Task Handle(
        AppointmentRiskScoreRequested notification,
        CancellationToken cancellationToken)
    {
        await _riskScoring
            .CalculateAndPersistAsync(notification.AppointmentId, cancellationToken)
            .ConfigureAwait(false);
    }
}
