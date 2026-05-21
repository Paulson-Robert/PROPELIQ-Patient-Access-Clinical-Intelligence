using Application.EventHandlers;
using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Marks a patient as arrived and records the arrival timestamp (US_024 AC-02).
/// </summary>
public sealed record MarkArrivedCommand(
    Guid AppointmentId,
    Guid StaffUserId) : IRequest<MarkArrivedResultDto>;

internal sealed class MarkArrivedCommandHandler
    : IRequestHandler<MarkArrivedCommand, MarkArrivedResultDto>
{
    private readonly IQueueService _queue;
    private readonly IPublisher _publisher;

    public MarkArrivedCommandHandler(IQueueService queue, IPublisher publisher)
    {
        _queue = queue;
        _publisher = publisher;
    }

    public async Task<MarkArrivedResultDto> Handle(
        MarkArrivedCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _queue
            .MarkArrivedAsync(request.AppointmentId, request.StaffUserId, cancellationToken)
            .ConfigureAwait(false);

        await _publisher
            .Publish(new QueueChangedNotification(), cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}
