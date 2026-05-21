using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Infrastructure.Data;
using Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Security.Tests;

public sealed class BookingConfirmationServiceInsuranceTests
{
    [Fact]
    public async Task ConfirmAsync_PersistsInsurance_WhenProvided()
    {
        var context = CreateContext();
        var patientId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        SeedPatientAndSlot(context, patientId, slotId);
        await context.SaveChangesAsync();

        var service = new BookingConfirmationService(
            context,
            new AlwaysValidSlotLockService(),
            new TestBackgroundJobClient(),
            new NoOpPublisher(),
            NullLogger<BookingConfirmationService>.Instance);

        var result = await service.ConfirmAsync(
            slotId,
            "lock-token",
            patientId,
            "Blue Cross Blue Shield",
            "BCBS-882341",
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Blue Cross Blue Shield", result.Appointment?.InsuranceProvider);
        Assert.Equal("BCBS-882341", result.Appointment?.InsurancePolicyNumber);
        Assert.Null(result.Appointment?.InsuranceValidationWarning);

        var appointment = await context.Appointments.SingleAsync();
        Assert.Equal("Blue Cross Blue Shield", appointment.InsuranceProvider);
        Assert.Equal("BCBS-882341", appointment.InsurancePolicyNumber);
    }

    [Fact]
    public async Task ConfirmAsync_AllowsInvalidInsuranceFormat_WithSoftWarning()
    {
        var context = CreateContext();
        var patientId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        SeedPatientAndSlot(context, patientId, slotId);
        await context.SaveChangesAsync();

        var service = new BookingConfirmationService(
            context,
            new AlwaysValidSlotLockService(),
            new TestBackgroundJobClient(),
            new NoOpPublisher(),
            NullLogger<BookingConfirmationService>.Instance);

        var result = await service.ConfirmAsync(
            slotId,
            "lock-token",
            patientId,
            "Aetna",
            "###bad###",
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Aetna", result.Appointment?.InsuranceProvider);
        Assert.Equal("###bad###", result.Appointment?.InsurancePolicyNumber);
        Assert.NotNull(result.Appointment?.InsuranceValidationWarning);

        var appointment = await context.Appointments.SingleAsync();
        Assert.Equal("###bad###", appointment.InsurancePolicyNumber);
    }

    [Fact]
    public async Task ConfirmAsync_AllowsNullInsuranceValues()
    {
        var context = CreateContext();
        var patientId = Guid.NewGuid();
        var slotId = Guid.NewGuid();

        SeedPatientAndSlot(context, patientId, slotId);
        await context.SaveChangesAsync();

        var service = new BookingConfirmationService(
            context,
            new AlwaysValidSlotLockService(),
            new TestBackgroundJobClient(),
            new NoOpPublisher(),
            NullLogger<BookingConfirmationService>.Instance);

        var result = await service.ConfirmAsync(
            slotId,
            "lock-token",
            patientId,
            null,
            null,
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.Appointment?.InsuranceProvider);
        Assert.Null(result.Appointment?.InsurancePolicyNumber);
        Assert.Null(result.Appointment?.InsuranceValidationWarning);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"booking-confirm-insurance-{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options, null);
    }

    private static void SeedPatientAndSlot(ApplicationDbContext context, Guid patientId, Guid slotId)
    {
        context.Users.Add(new User
        {
            UserId = patientId,
            Email = "patient@example.com",
            PasswordHash = "hash",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        context.AvailabilitySlots.Add(new AvailabilitySlot
        {
            SlotId = slotId,
            ProviderId = Guid.NewGuid(),
            ProviderName = "Dr. Sarah Chen",
            Specialty = "Internal Medicine",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            IsAvailable = true,
            IsLocked = true,
            LockExpiry = DateTime.UtcNow.AddMinutes(1),
            Version = 0,
        });
    }

    private sealed class AlwaysValidSlotLockService : ISlotLockService
    {
        public Task<SlotLockResult?> AcquireAsync(Guid slotId, CancellationToken cancellationToken = default)
            => Task.FromResult<SlotLockResult?>(
                new SlotLockResult(slotId, "token", DateTimeOffset.UtcNow.AddSeconds(30), 30));

        public Task<bool> ValidateAsync(Guid slotId, string lockToken, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> ReleaseAsync(Guid slotId, string lockToken, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class TestBackgroundJobClient : IBackgroundJobClient
    {
        public string Create(Job job, IState state)
            => Guid.NewGuid().ToString("N");

        public bool ChangeState(string jobId, IState state, string expectedState)
            => true;
    }

    private sealed class NoOpPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
