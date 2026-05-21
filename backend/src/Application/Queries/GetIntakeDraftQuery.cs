using Application.Interfaces;
using MediatR;

namespace Application.Queries;

/// <summary>
/// Returns the in-progress or submitted intake draft for a given appointment (AC-02).
/// Returns <c>null</c> when no draft exists, allowing the client to start fresh.
/// </summary>
public sealed record GetIntakeDraftQuery(
    Guid PatientUserId,
    Guid AppointmentId) : IRequest<IntakeDraftDto?>;

internal sealed class GetIntakeDraftQueryHandler
    : IRequestHandler<GetIntakeDraftQuery, IntakeDraftDto?>
{
    private readonly IManualIntakeService _intake;

    public GetIntakeDraftQueryHandler(IManualIntakeService intake)
    {
        _intake = intake;
    }

    public Task<IntakeDraftDto?> Handle(
        GetIntakeDraftQuery request,
        CancellationToken cancellationToken)
        => _intake.GetDraftAsync(
            request.PatientUserId,
            request.AppointmentId,
            cancellationToken);
}
