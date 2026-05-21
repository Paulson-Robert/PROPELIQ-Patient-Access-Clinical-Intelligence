namespace Application.Interfaces;

/// <summary>
/// Result of a swap engine execution attempt for a freed slot (AC-02, AC-05).
/// </summary>
public sealed record SwapEngineResult(
    bool SwapExecuted,
    Guid? WinningAppointmentId,
    Guid? WinningPatientUserId,
    string? FailureReason);

public interface ISwapEngineService
{
    /// <summary>
    /// Processes the swap queue for a freed slot: acquires a lock, selects the
    /// earliest FCFS candidate, executes the booking swap, notifies staff and
    /// patient, and updates queue status (AC-02, AC-03, AC-04, AC-05).
    /// </summary>
    Task<SwapEngineResult> ProcessFreedSlotAsync(
        Guid freedSlotId,
        CancellationToken cancellationToken = default);
}
