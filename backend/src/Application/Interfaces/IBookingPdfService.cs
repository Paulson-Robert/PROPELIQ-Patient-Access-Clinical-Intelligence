namespace Application.Interfaces;

/// <summary>
/// Generates a PDF booking confirmation document (AC-04, NFR-011).
/// </summary>
public interface IBookingPdfService
{
    /// <summary>
    /// Generates and delivers (email + storage) a booking confirmation PDF.
    /// On generation failure the appointment is already committed — this method is
    /// called from a Hangfire background job and retried on failure (Edge Case).
    /// </summary>
    Task GenerateAndDeliverAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);
}
