using Application.EventHandlers;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Security.Tests;

public sealed class AppointmentManagementServiceTests
{
    [Fact]
    public async Task CancelAsync_ReleasesSlot_AndPublishesFreedSlotNotification()
    {
        var context = CreateContext();
        var userId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        SeedUser(context, userId);
        SeedBookedAppointment(context, appointmentId, userId, slotId);
        SeedCalendarSync(context, userId);
        await context.SaveChangesAsync();

        var publisher = new TestPublisher();
        var service = new AppointmentManagementService(
            context,
            new TestSlotLockService(validateResult: true),
            publisher,
            NullLogger<AppointmentManagementService>.Instance);

        var result = await service.CancelAsync(appointmentId, userId, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Cancelled", result.Appointment?.Status);

        var appointment = await context.Appointments.FirstAsync(a => a.AppointmentId == appointmentId);
        var slot = await context.AvailabilitySlots.FirstAsync(s => s.SlotId == slotId);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.True(slot.IsAvailable);
        Assert.False(slot.IsLocked);

        Assert.Contains(publisher.Published, p => p is SlotCancelledNotification n && n.FreedSlotId == slotId);
        Assert.Contains(context.Notifications, n => n.AppointmentId == appointmentId && n.NotificationType == NotificationType.Cancellation);
    }

    [Fact]
    public async Task RescheduleAsync_MovesAppointment_AndReleasesPreviousSlot()
    {
        var context = CreateContext();
        var userId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var oldSlotId = Guid.NewGuid();
        var newSlotId = Guid.NewGuid();

        SeedUser(context, userId);
        SeedBookedAppointment(context, appointmentId, userId, oldSlotId);
        context.AvailabilitySlots.Add(new AvailabilitySlot
        {
            SlotId = newSlotId,
            ProviderId = Guid.NewGuid(),
            ProviderName = "Dr. New",
            Specialty = "Cardiology",
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddMinutes(30),
            IsAvailable = true,
            IsLocked = true,
            LockExpiry = DateTime.UtcNow.AddMinutes(1),
            Version = 0,
        });
        await context.SaveChangesAsync();

        var publisher = new TestPublisher();
        var service = new AppointmentManagementService(
            context,
            new TestSlotLockService(validateResult: true),
            publisher,
            NullLogger<AppointmentManagementService>.Instance);

        var result = await service.RescheduleAsync(
            appointmentId,
            newSlotId,
            "lock-token",
            userId,
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Scheduled", result.Appointment?.Status);
        Assert.Equal(newSlotId, result.Appointment?.SlotId);

        var appointment = await context.Appointments.FirstAsync(a => a.AppointmentId == appointmentId);
        var oldSlot = await context.AvailabilitySlots.FirstAsync(s => s.SlotId == oldSlotId);
        var newSlot = await context.AvailabilitySlots.FirstAsync(s => s.SlotId == newSlotId);

        Assert.Equal(newSlotId, appointment.SlotId);
        Assert.True(oldSlot.IsAvailable);
        Assert.False(newSlot.IsAvailable);

        Assert.Contains(publisher.Published, p => p is SlotCancelledNotification n && n.FreedSlotId == oldSlotId);
    }

    [Fact]
    public async Task RescheduleAsync_WhenNewSlotUnavailable_PreservesOldAppointmentAndSlot()
    {
        var context = CreateContext();
        var userId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var oldSlotId = Guid.NewGuid();
        var newSlotId = Guid.NewGuid();

        SeedUser(context, userId);
        SeedBookedAppointment(context, appointmentId, userId, oldSlotId);
        context.AvailabilitySlots.Add(new AvailabilitySlot
        {
            SlotId = newSlotId,
            ProviderId = Guid.NewGuid(),
            ProviderName = "Dr. Busy",
            Specialty = "Cardiology",
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddMinutes(30),
            IsAvailable = false,
            IsLocked = false,
            Version = 0,
        });
        await context.SaveChangesAsync();

        var service = new AppointmentManagementService(
            context,
            new TestSlotLockService(validateResult: true),
            new TestPublisher(),
            NullLogger<AppointmentManagementService>.Instance);

        var result = await service.RescheduleAsync(
            appointmentId,
            newSlotId,
            "lock-token",
            userId,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("SLOT_UNAVAILABLE", result.FailureCode);

        var appointment = await context.Appointments.FirstAsync(a => a.AppointmentId == appointmentId);
        var oldSlot = await context.AvailabilitySlots.FirstAsync(s => s.SlotId == oldSlotId);

        Assert.Equal(oldSlotId, appointment.SlotId);
        Assert.False(oldSlot.IsAvailable);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"appointment-mgmt-{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options, null);
    }

    private static void SeedUser(ApplicationDbContext context, Guid userId)
    {
        context.Users.Add(new User
        {
            UserId = userId,
            Email = "patient@example.com",
            PasswordHash = "hash",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
    }

    private static void SeedBookedAppointment(
        ApplicationDbContext context,
        Guid appointmentId,
        Guid userId,
        Guid slotId)
    {
        context.AvailabilitySlots.Add(new AvailabilitySlot
        {
            SlotId = slotId,
            ProviderId = Guid.NewGuid(),
            ProviderName = "Dr. Sarah Chen",
            Specialty = "Internal Medicine",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            IsAvailable = false,
            IsLocked = false,
            Version = 0,
        });

        context.Appointments.Add(new Appointment
        {
            AppointmentId = appointmentId,
            PatientId = userId,
            ProviderId = Guid.NewGuid(),
            SlotId = slotId,
            Status = AppointmentStatus.Scheduled,
            BookingType = BookingType.Online,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
    }

    private static void SeedCalendarSync(ApplicationDbContext context, Guid userId)
    {
        context.CalendarSyncs.Add(new CalendarSync
        {
            SyncId = Guid.NewGuid(),
            UserId = userId,
            Provider = CalendarProvider.Google,
            AccessToken = "token",
            RefreshToken = "refresh",
            TokenExpiry = DateTime.UtcNow.AddHours(1),
            IsActive = true,
        });
    }

    private sealed class TestSlotLockService : ISlotLockService
    {
        private readonly bool _validateResult;

        public TestSlotLockService(bool validateResult)
        {
            _validateResult = validateResult;
        }

        public Task<SlotLockResult?> AcquireAsync(Guid slotId, CancellationToken cancellationToken = default)
            => Task.FromResult<SlotLockResult?>(
                new SlotLockResult(slotId, "token", DateTimeOffset.UtcNow.AddSeconds(30), 30));

        public Task<bool> ValidateAsync(Guid slotId, string lockToken, CancellationToken cancellationToken = default)
            => Task.FromResult(_validateResult);

        public Task<bool> ReleaseAsync(Guid slotId, string lockToken, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class TestPublisher : IPublisher
    {
        public List<object> Published { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }
    }
}
