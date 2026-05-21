using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Cancels an existing appointment and releases the booked slot.
/// </summary>
public sealed record CancelAppointmentCommand(
    Guid AppointmentId,
    Guid PatientUserId) : IRequest<AppointmentMutationResult>;

internal sealed class CancelAppointmentCommandHandler
    : IRequestHandler<CancelAppointmentCommand, AppointmentMutationResult>
{
    private readonly IAppointmentManagementService _appointmentManagement;

    public CancelAppointmentCommandHandler(IAppointmentManagementService appointmentManagement)
    {
        _appointmentManagement = appointmentManagement;
    }

    public Task<AppointmentMutationResult> Handle(
        CancelAppointmentCommand request,
        CancellationToken cancellationToken)
        => _appointmentManagement.CancelAsync(
            request.AppointmentId,
            request.PatientUserId,
            cancellationToken);
}
