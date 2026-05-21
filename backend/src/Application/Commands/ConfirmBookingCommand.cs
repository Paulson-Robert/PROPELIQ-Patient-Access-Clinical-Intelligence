using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Validates the slot lock token, persists the appointment, and enqueues PDF delivery (AC-04).
/// </summary>
public sealed record ConfirmBookingCommand(
    Guid SlotId,
    string LockToken,
    Guid PatientUserId,
    string? InsuranceProvider,
    string? InsurancePolicyNumber) : IRequest<BookingConfirmationResult>;

internal sealed class ConfirmBookingCommandHandler
    : IRequestHandler<ConfirmBookingCommand, BookingConfirmationResult>
{
    private readonly IBookingConfirmationService _bookingConfirmation;

    public ConfirmBookingCommandHandler(IBookingConfirmationService bookingConfirmation)
    {
        _bookingConfirmation = bookingConfirmation;
    }

    public Task<BookingConfirmationResult> Handle(
        ConfirmBookingCommand request,
        CancellationToken cancellationToken)
        => _bookingConfirmation.ConfirmAsync(
            request.SlotId,
            request.LockToken,
            request.PatientUserId,
            request.InsuranceProvider,
            request.InsurancePolicyNumber,
            cancellationToken);
}
