using Application.Interfaces;
using MediatR;

namespace Application.Queries;

/// <summary>
/// Returns the same-day patient queue ordered by position then slot start time (US_024 AC-01).
/// </summary>
public sealed record GetSameDayQueueQuery : IRequest<QueueResponseDto>;

internal sealed class GetSameDayQueueQueryHandler
    : IRequestHandler<GetSameDayQueueQuery, QueueResponseDto>
{
    private readonly IQueueService _queue;

    public GetSameDayQueueQueryHandler(IQueueService queue)
    {
        _queue = queue;
    }

    public Task<QueueResponseDto> Handle(
        GetSameDayQueueQuery request,
        CancellationToken cancellationToken)
        => _queue.GetTodayQueueAsync(cancellationToken);
}
