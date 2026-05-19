using Application.Commands;
using Application.Interfaces;
using MediatR;

namespace Application.Handlers;

public sealed class DeletePatientDataCommandHandler : IRequestHandler<DeletePatientDataCommand, PatientDataDeletionResult>
{
    private readonly IPatientDataDeletionService _patientDataDeletionService;

    public DeletePatientDataCommandHandler(IPatientDataDeletionService patientDataDeletionService)
    {
        _patientDataDeletionService = patientDataDeletionService;
    }

    public Task<PatientDataDeletionResult> Handle(
        DeletePatientDataCommand request,
        CancellationToken cancellationToken)
    {
        var deletionRequest = new DeletePatientDataRequest(
            request.PatientProfileId,
            request.ActorUserId,
            request.ActorRole,
            request.IpAddress);

        return _patientDataDeletionService.DeletePatientDataAsync(deletionRequest, cancellationToken);
    }
}