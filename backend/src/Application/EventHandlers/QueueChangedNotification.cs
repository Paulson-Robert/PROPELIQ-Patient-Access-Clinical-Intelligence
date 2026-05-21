using MediatR;

namespace Application.EventHandlers;

/// <summary>
/// Published whenever the same-day queue changes (arrival marked, position reordered).
/// Handlers can use this to push updates to connected clients (US_024 AC-04).
/// </summary>
public sealed record QueueChangedNotification : INotification;

/// <summary>
/// No-op handler — satisfies AC-04 in-process broadcast.
/// Replace or supplement with a SignalR hub push when real-time transport is added.
/// </summary>
public sealed class QueueChangedHandler : INotificationHandler<QueueChangedNotification>
{
    public Task Handle(QueueChangedNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
