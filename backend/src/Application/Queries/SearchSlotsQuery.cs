using Application.Interfaces;
using MediatR;

namespace Application.Queries;

/// <summary>
/// Returns available appointment slots filtered by provider name, specialty, and/or date range.
/// Results are sorted ascending by slot start time (AC-01, AC-02).
/// </summary>
public sealed record SearchSlotsQuery(
    string? Provider,
    string? Specialty,
    DateOnly? From,
    DateOnly? To) : IRequest<IReadOnlyList<SlotDto>>;

internal sealed class SearchSlotsQueryHandler : IRequestHandler<SearchSlotsQuery, IReadOnlyList<SlotDto>>
{
    private readonly ISlotSearchService _slotSearch;

    public SearchSlotsQueryHandler(ISlotSearchService slotSearch)
    {
        _slotSearch = slotSearch;
    }

    public Task<IReadOnlyList<SlotDto>> Handle(
        SearchSlotsQuery request,
        CancellationToken cancellationToken)
        => _slotSearch.SearchAsync(
            request.Provider,
            request.Specialty,
            request.From,
            request.To,
            cancellationToken);
}
