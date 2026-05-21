using Application.Interfaces;
using MediatR;

namespace Application.Queries;

/// <summary>
/// Searches patients by name and optional date of birth for walk-in workflows.
/// </summary>
public sealed record SearchPatientsQuery(
    string Name,
    DateOnly? DateOfBirth) : IRequest<IReadOnlyList<PatientSearchResultDto>>;

internal sealed class SearchPatientsQueryHandler
    : IRequestHandler<SearchPatientsQuery, IReadOnlyList<PatientSearchResultDto>>
{
    private readonly IPatientSearchService _patientSearch;

    public SearchPatientsQueryHandler(IPatientSearchService patientSearch)
    {
        _patientSearch = patientSearch;
    }

    public Task<IReadOnlyList<PatientSearchResultDto>> Handle(
        SearchPatientsQuery request,
        CancellationToken cancellationToken)
        => _patientSearch.SearchAsync(request.Name, request.DateOfBirth, cancellationToken);
}