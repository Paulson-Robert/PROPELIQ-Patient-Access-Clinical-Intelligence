namespace Application.Interfaces;

public interface INotificationDeliveryService
{
    /// <summary>
    /// Delivers a queued notification. Implements exponential backoff retry (max 3 attempts).
    /// Marks the notification failed and alerts staff when all retries are exhausted.
    /// Suppresses delivery when the appointment has been cancelled.
    /// </summary>
    Task DeliverAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
