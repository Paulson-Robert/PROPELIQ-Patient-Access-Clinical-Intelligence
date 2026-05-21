using Application.Interfaces;
using MediatR;

namespace Application.EventHandlers;

/// <summary>
/// Published when an availability slot is released (cancellation or rescheduling).
/// Carries the freed slot ID so the swap engine can evaluate the queue (AC-02, AC-03).
/// </summary>
public sealed record SlotCancelledNotification(Guid FreedSlotId) : INotification;

/// <summary>
/// Triggers the swap engine when a slot becomes available.
/// Runs inline on the MediatR pipeline — the engine service handles Redis lock
/// protection to guard against duplicate swap attempts (Edge Case: rapid toggling).
/// </summary>
public sealed class SlotCancelledHandler : INotificationHandler<SlotCancelledNotification>
{
    private readonly ISwapEngineService _swapEngine;

    public SlotCancelledHandler(ISwapEngineService swapEngine)
    {
        _swapEngine = swapEngine;
    }

    public async Task Handle(
        SlotCancelledNotification notification,
        CancellationToken cancellationToken)
    {
        await _swapEngine
            .ProcessFreedSlotAsync(notification.FreedSlotId, cancellationToken)
            .ConfigureAwait(false);
    }
}
