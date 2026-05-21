using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Moves an existing scheduled appointment to a newly locked slot.
/// </summary>
public sealed record RescheduleAppointmentCommand(
    Guid AppointmentId,
    Guid NewSlotId,
    string LockToken,
    Guid PatientUserId) : IRequest<AppointmentMutationResult>;

internal sealed class RescheduleAppointmentCommandHandler
    : IRequestHandler<RescheduleAppointmentCommand, AppointmentMutationResult>
{
    private readonly IAppointmentManagementService _appointmentManagement;

    public RescheduleAppointmentCommandHandler(IAppointmentManagementService appointmentManagement)
    {
        _appointmentManagement = appointmentManagement;
    }

    public Task<AppointmentMutationResult> Handle(
        RescheduleAppointmentCommand request,
        CancellationToken cancellationToken)
        => _appointmentManagement.RescheduleAsync(
            request.AppointmentId,
            request.NewSlotId,
            request.LockToken,
            request.PatientUserId,
            cancellationToken);
}
