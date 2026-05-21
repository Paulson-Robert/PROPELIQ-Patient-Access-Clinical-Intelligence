using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Creates walk-in appointments and tracks them in the same-day queue table.
/// </summary>
public sealed class WalkInBookingService : IWalkInBookingService
{
    private readonly ApplicationDbContext _db;

    public WalkInBookingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<WalkInBookingResultDto> CreateAsync(
        CreateWalkInRequest request,
        CancellationToken cancellationToken = default)
    {
        var slot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.SlotId == request.SlotId, cancellationToken)
            .ConfigureAwait(false);

        if (slot is null)
        {
            throw new InvalidOperationException("Selected slot was not found.");
        }

        if (!slot.IsAvailable)
        {
            throw new InvalidOperationException("Selected slot is no longer available.");
        }

        var now = DateTime.UtcNow;
        var patientUserId = request.ExistingPatientUserId;
        var temporaryRecord = false;

        if (!patientUserId.HasValue)
        {
            patientUserId = await CreatePatientForWalkInAsync(request, now, cancellationToken)
                .ConfigureAwait(false);
            temporaryRecord = !request.CreateAccount;
        }

        var appointment = new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            PatientId = patientUserId.Value,
            ProviderId = slot.ProviderId,
            SlotId = request.SlotId,
            Status = AppointmentStatus.Scheduled,
            BookingType = BookingType.WalkIn,
            CreatedByUserId = request.StaffUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var queueEntry = new PreferredSlotQueue
        {
            QueueId = Guid.NewGuid(),
            AppointmentId = appointment.AppointmentId,
            PreferredSlotId = request.SlotId,
            RequestedAt = now,
            Status = QueueStatus.Waiting,
        };

        slot.IsAvailable = false;
        slot.IsLocked = false;
        slot.LockExpiry = null;
        slot.Version++;

        _db.Appointments.Add(appointment);
        _db.PreferredSlotQueues.Add(queueEntry);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new WalkInBookingResultDto(
            appointment.AppointmentId,
            appointment.PatientId,
            queueEntry.QueueId,
            nameof(BookingType.WalkIn),
            nameof(AppointmentStatus.Scheduled),
            appointment.CreatedAt,
            temporaryRecord);
    }

    private async Task<Guid> CreatePatientForWalkInAsync(
        CreateWalkInRequest request,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new InvalidOperationException("First name is required when creating a walk-in patient.");
        }

        var firstName = request.FirstName.Trim();
        var lastName = string.IsNullOrWhiteSpace(request.LastName)
            ? "Guest"
            : request.LastName.Trim();

        var email = ResolveEmail(request);

        if (request.CreateAccount)
        {
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email == email, cancellationToken)
                .ConfigureAwait(false);

            if (emailExists)
            {
                throw new InvalidOperationException("Patient with this email already exists.");
            }
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = email,
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var profile = new PatientProfile
        {
            PatientProfileId = Guid.NewGuid(),
            UserId = user.UserId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = request.DateOfBirth,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            CreatedAt = now,
        };

        _db.Users.Add(user);
        _db.PatientProfiles.Add(profile);

        return user.UserId;
    }

    private static string ResolveEmail(CreateWalkInRequest request)
    {
        if (request.CreateAccount)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new InvalidOperationException("Email is required when creating a patient account.");
            }

            return request.Email.Trim().ToLowerInvariant();
        }

        // Temporary records use a deterministic domain so they can be identified for later merge.
        return $"guest-{Guid.NewGuid():N}@walkin.local";
    }
}