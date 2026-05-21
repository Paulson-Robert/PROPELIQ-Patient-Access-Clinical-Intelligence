namespace Application.Interfaces;

/// <summary>
/// Search result DTO for a single bookable availability slot (AC-01, AC-02).
/// </summary>
public sealed record SlotDto(
    Guid SlotId,
    Guid ProviderId,
    string ProviderName,
    string Specialty,
    DateTime StartTime,
    DateTime EndTime,
    int DurationMinutes,
    bool IsAvailable,
    bool IsLocked);

public interface ISlotSearchService
{
    /// <summary>
    /// Returns available slots filtered by optional provider name substring and/or specialty.
    /// Results are sorted ascending by StartTime.
    /// </summary>
    Task<IReadOnlyList<SlotDto>> SearchAsync(
        string? provider,
        string? specialty,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default);
}
