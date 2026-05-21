using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Security.Tests;

public sealed class WalkInBookingServiceTests
{
    [Fact]
    public async Task CreateAsync_WithExistingPatient_CreatesWalkInAppointmentAndQueueEntry()
    {
        var context = CreateContext();
        var staffUserId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        SeedStaff(context, staffUserId);
        SeedPatient(context, patientUserId, "existing.patient@example.com", "Ana", "Stone", new DateOnly(1991, 5, 12));
        SeedSlot(context, slotId);
        await context.SaveChangesAsync();

        var service = new WalkInBookingService(context);

        var result = await service.CreateAsync(
            new CreateWalkInRequest(
                slotId,
                staffUserId,
                patientUserId,
                null,
                null,
                null,
                null,
                null,
                false),
            CancellationToken.None);

        Assert.Equal("WalkIn", result.BookingType);
        Assert.Equal("Scheduled", result.Status);
        Assert.False(result.TemporaryPatientRecordCreated);
        Assert.Equal(patientUserId, result.PatientUserId);

        var appointment = await context.Appointments.SingleAsync();
        var queueEntry = await context.PreferredSlotQueues.SingleAsync();
        var slot = await context.AvailabilitySlots.SingleAsync();

        Assert.Equal(BookingType.WalkIn, appointment.BookingType);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Equal(queueEntry.AppointmentId, appointment.AppointmentId);
        Assert.Equal(QueueStatus.Waiting, queueEntry.Status);
        Assert.False(slot.IsAvailable);
    }

    [Fact]
    public async Task CreateAsync_ForGuestWalkIn_CreatesTemporaryPatientRecord()
    {
        var context = CreateContext();
        var staffUserId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        SeedStaff(context, staffUserId);
        SeedSlot(context, slotId);
        await context.SaveChangesAsync();

        var service = new WalkInBookingService(context);

        var result = await service.CreateAsync(
            new CreateWalkInRequest(
                slotId,
                staffUserId,
                null,
                "Guest",
                null,
                null,
                null,
                null,
                false),
            CancellationToken.None);

        Assert.True(result.TemporaryPatientRecordCreated);

        var createdUser = await context.Users.FirstAsync(u => u.UserId == result.PatientUserId);
        var createdProfile = await context.PatientProfiles.FirstAsync(p => p.UserId == result.PatientUserId);

        Assert.StartsWith("guest-", createdUser.Email);
        Assert.EndsWith("@walkin.local", createdUser.Email);
        Assert.Equal("Guest", createdProfile.FirstName);
        Assert.Equal("Guest", createdProfile.LastName);
    }

    [Fact]
    public async Task CreateAsync_WhenAccountEmailAlreadyExists_Throws()
    {
        var context = CreateContext();
        var staffUserId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        SeedStaff(context, staffUserId);
        SeedPatient(context, Guid.NewGuid(), "dup.patient@example.com", "Nia", "Reed", new DateOnly(1988, 8, 3));
        SeedSlot(context, slotId);
        await context.SaveChangesAsync();

        var service = new WalkInBookingService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(
                new CreateWalkInRequest(
                    slotId,
                    staffUserId,
                    null,
                    "Nia",
                    "Reed",
                    new DateOnly(1988, 8, 3),
                    "dup.patient@example.com",
                    "555-0144",
                    true),
                CancellationToken.None));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"walkin-tests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options, null);
    }

    private static void SeedStaff(ApplicationDbContext context, Guid userId)
    {
        context.Users.Add(new User
        {
            UserId = userId,
            Email = "staff@example.com",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Staff,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
    }

    private static void SeedPatient(
        ApplicationDbContext context,
        Guid userId,
        string email,
        string firstName,
        string lastName,
        DateOnly dateOfBirth)
    {
        var now = DateTime.UtcNow;

        context.Users.Add(new User
        {
            UserId = userId,
            Email = email,
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        context.PatientProfiles.Add(new PatientProfile
        {
            PatientProfileId = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            CreatedAt = now,
        });
    }

    private static void SeedSlot(ApplicationDbContext context, Guid slotId)
    {
        context.AvailabilitySlots.Add(new AvailabilitySlot
        {
            SlotId = slotId,
            ProviderId = Guid.NewGuid(),
            ProviderName = "Dr. Lane",
            Specialty = "Family Medicine",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(1).AddMinutes(30),
            IsAvailable = true,
            IsLocked = false,
            Version = 0,
        });
    }
}
