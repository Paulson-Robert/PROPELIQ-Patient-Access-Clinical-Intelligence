using Application.Interfaces;
using MediatR;

namespace Application.Commands;

/// <summary>
/// Creates a walk-in appointment and same-day queue entry.
/// </summary>
public sealed record CreateWalkInCommand(
    Guid SlotId,
    Guid StaffUserId,
    Guid? ExistingPatientUserId,
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? Email,
    string? Phone,
    bool CreateAccount) : IRequest<WalkInBookingResultDto>;

internal sealed class CreateWalkInCommandHandler
    : IRequestHandler<CreateWalkInCommand, WalkInBookingResultDto>
{
    private readonly IWalkInBookingService _walkInBooking;

    public CreateWalkInCommandHandler(IWalkInBookingService walkInBooking)
    {
        _walkInBooking = walkInBooking;
    }

    public Task<WalkInBookingResultDto> Handle(
        CreateWalkInCommand request,
        CancellationToken cancellationToken)
        => _walkInBooking.CreateAsync(
            new CreateWalkInRequest(
                request.SlotId,
                request.StaffUserId,
                request.ExistingPatientUserId,
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Email,
                request.Phone,
                request.CreateAccount),
            cancellationToken);
}