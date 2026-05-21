using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Acquires a 30-second Redis lock on the specified slot (AC-03).
/// Returns null when the slot is already locked or unavailable.
/// </summary>
public sealed record LockSlotCommand(Guid SlotId) : IRequest<SlotLockResult?>;

internal sealed class LockSlotCommandHandler : IRequestHandler<LockSlotCommand, SlotLockResult?>
{
    private readonly ISlotLockService _slotLock;

    public LockSlotCommandHandler(ISlotLockService slotLock)
    {
        _slotLock = slotLock;
    }

    public Task<SlotLockResult?> Handle(
        LockSlotCommand request,
        CancellationToken cancellationToken)
        => _slotLock.AcquireAsync(request.SlotId, cancellationToken);
}
