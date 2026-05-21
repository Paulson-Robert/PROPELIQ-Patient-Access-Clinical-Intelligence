using Application.EventHandlers;
using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Moves a queue entry to a new position and persists the staff reason (US_024 AC-03).
/// Raises <see cref="QueueConcurrencyException"/> when a concurrent reorder is detected.
/// </summary>
public sealed record ReorderQueueCommand(
    Guid AppointmentId,
    int NewPosition,
    string Reason,
    Guid StaffUserId) : IRequest<ReorderQueueResultDto>;

internal sealed class ReorderQueueCommandHandler
    : IRequestHandler<ReorderQueueCommand, ReorderQueueResultDto>
{
    private readonly IQueueService _queue;
    private readonly IPublisher _publisher;

    public ReorderQueueCommandHandler(IQueueService queue, IPublisher publisher)
    {
        _queue = queue;
        _publisher = publisher;
    }

    public async Task<ReorderQueueResultDto> Handle(
        ReorderQueueCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _queue
            .ReorderAsync(
                request.AppointmentId,
                request.NewPosition,
                request.Reason,
                request.StaffUserId,
                cancellationToken)
            .ConfigureAwait(false);

        await _publisher
            .Publish(new QueueChangedNotification(), cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}
