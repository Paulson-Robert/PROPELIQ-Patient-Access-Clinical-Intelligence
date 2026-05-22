using Application.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// No-op swap engine when Redis is unavailable (degraded mode).
/// Returns a non-executed result indicating the service is unavailable.
/// </summary>
internal sealed class NoOpSwapEngineService : ISwapEngineService
{
    public Task<SwapEngineResult> ProcessFreedSlotAsync(
        Guid freedSlotId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new SwapEngineResult(
            SwapExecuted: false,
            WinningAppointmentId: null,
            WinningPatientUserId: null,
            FailureReason: "Swap engine unavailable: Redis is not configured."));
}
