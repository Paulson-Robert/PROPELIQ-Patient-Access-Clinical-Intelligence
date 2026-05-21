namespace Application.Interfaces;

/// <summary>
/// Search result for matching patients by demographics.
/// </summary>
public sealed record PatientSearchResultDto(
    Guid UserId,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string Email,
    string? Phone);

public interface IPatientSearchService
{
    /// <summary>
    /// Returns patients matching the supplied name and optional date of birth.
    /// </summary>
    Task<IReadOnlyList<PatientSearchResultDto>> SearchAsync(
        string name,
        DateOnly? dateOfBirth,
        CancellationToken cancellationToken = default);
}