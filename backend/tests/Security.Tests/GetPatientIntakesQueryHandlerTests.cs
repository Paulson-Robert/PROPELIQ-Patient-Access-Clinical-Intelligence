using Application.Queries;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Handlers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Security.Tests;

public sealed class GetPatientIntakesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ForPatientActor_ReturnsOwnManualIntake()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId);
        SeedIntake(context, profileId, appointmentId, IntakeMode.Manual, "Annual physical");
        await context.SaveChangesAsync();

        var handler = new GetPatientIntakesQueryHandler(context);

        var result = await handler.Handle(
            new GetPatientIntakesQuery(patientUserId, "Patient"),
            CancellationToken.None);

        var intake = Assert.Single(result);
        Assert.Equal(appointmentId, intake.AppointmentId);
        Assert.Equal("Manual", intake.IntakeMode);
        Assert.Equal("Annual physical", intake.ReasonForVisit);
    }

    [Fact]
    public async Task Handle_ForPatientActor_DoesNotReturnOtherPatientIntakes()
    {
        var context = CreateContext();
        var actorPatientUserId = Guid.NewGuid();
        var otherPatientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        SeedPatient(context, actorPatientUserId);
        var otherProfileId = SeedPatient(context, otherPatientUserId);
        SeedAppointment(context, appointmentId, otherPatientUserId);
        SeedIntake(context, otherProfileId, appointmentId, IntakeMode.Manual, "Wrong chart");
        await context.SaveChangesAsync();

        var handler = new GetPatientIntakesQueryHandler(context);

        var result = await handler.Handle(
            new GetPatientIntakesQuery(actorPatientUserId, "Patient"),
            CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ForStaffActor_ReturnsPatientIntakeRecords()
    {
        var context = CreateContext();
        var staffUserId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        SeedStaff(context, staffUserId);
        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId);
        SeedIntake(context, profileId, appointmentId, IntakeMode.Manual, "Walk-in triage");
        await context.SaveChangesAsync();

        var handler = new GetPatientIntakesQueryHandler(context);

        var result = await handler.Handle(
            new GetPatientIntakesQuery(staffUserId, "Staff"),
            CancellationToken.None);

        var intake = Assert.Single(result);
        Assert.Equal(appointmentId, intake.AppointmentId);
        Assert.Equal("Manual", intake.IntakeMode);
        Assert.Equal("Walk-in triage", intake.ReasonForVisit);
    }

    [Fact]
    public async Task Handle_ForPatientActor_ReturnsManualAndAiRecordsForSameAppointment()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId);
        SeedIntake(context, profileId, appointmentId, IntakeMode.Manual, "Manual symptom");
        SeedIntake(context, profileId, appointmentId, IntakeMode.AI, "AI symptom");
        await context.SaveChangesAsync();

        var handler = new GetPatientIntakesQueryHandler(context);

        var result = await handler.Handle(
            new GetPatientIntakesQuery(patientUserId, "Patient"),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.IntakeMode == "Manual" && i.ReasonForVisit == "Manual symptom");
        Assert.Contains(result, i => i.IntakeMode == "AI" && i.ReasonForVisit == "AI symptom");
    }

    [Fact]
    public async Task Handle_ForPatientActor_ReturnsRepeatedManualAndAiSubmissions()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId);
        SeedIntake(context, profileId, appointmentId, IntakeMode.Manual, "First manual symptom");
        SeedIntake(context, profileId, appointmentId, IntakeMode.Manual, "Second manual symptom");
        SeedIntake(context, profileId, appointmentId, IntakeMode.AI, "First AI symptom");
        SeedIntake(context, profileId, appointmentId, IntakeMode.AI, "Second AI symptom");
        await context.SaveChangesAsync();

        var handler = new GetPatientIntakesQueryHandler(context);

        var result = await handler.Handle(
            new GetPatientIntakesQuery(patientUserId, "Patient"),
            CancellationToken.None);

        Assert.Equal(4, result.Count);
        Assert.Contains(result, i => i.IntakeMode == "Manual" && i.ReasonForVisit == "First manual symptom");
        Assert.Contains(result, i => i.IntakeMode == "Manual" && i.ReasonForVisit == "Second manual symptom");
        Assert.Contains(result, i => i.IntakeMode == "AI" && i.ReasonForVisit == "First AI symptom");
        Assert.Contains(result, i => i.IntakeMode == "AI" && i.ReasonForVisit == "Second AI symptom");
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"patient-intakes-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options, null);
    }

    private static Guid SeedPatient(ApplicationDbContext context, Guid userId)
    {
        var now = DateTime.UtcNow;
        var profileId = Guid.NewGuid();

        context.Users.Add(new User
        {
            UserId = userId,
            Email = $"{userId:N}@patient.example",
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
            PatientProfileId = profileId,
            UserId = userId,
            FirstName = "Patient",
            LastName = "Example",
            CreatedAt = now,
        });

        return profileId;
    }

    private static void SeedStaff(ApplicationDbContext context, Guid userId)
    {
        var now = DateTime.UtcNow;

        context.Users.Add(new User
        {
            UserId = userId,
            Email = $"{userId:N}@staff.example",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Staff,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });
    }

    private static void SeedAppointment(
        ApplicationDbContext context,
        Guid appointmentId,
        Guid patientUserId)
    {
        var slotId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        context.AvailabilitySlots.Add(new AvailabilitySlot
        {
            SlotId = slotId,
            ProviderId = providerId,
            ProviderName = "Dr. Lane",
            Specialty = "Family Medicine",
            StartTime = now.AddDays(1),
            EndTime = now.AddDays(1).AddMinutes(30),
            IsAvailable = false,
            IsLocked = false,
            Version = 0,
        });

        context.Appointments.Add(new Appointment
        {
            AppointmentId = appointmentId,
            PatientId = patientUserId,
            ProviderId = providerId,
            SlotId = slotId,
            Status = AppointmentStatus.Scheduled,
            BookingType = BookingType.Online,
            CreatedByUserId = patientUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });
    }

    private static void SeedIntake(
        ApplicationDbContext context,
        Guid profileId,
        Guid appointmentId,
        IntakeMode mode,
        string reasonForVisit)
    {
        var now = DateTime.UtcNow;

        context.IntakeRecords.Add(new IntakeRecord
        {
            IntakeId = Guid.NewGuid(),
            PatientProfileId = profileId,
            AppointmentId = appointmentId,
            IntakeMode = mode,
            ReasonForVisit = reasonForVisit,
            CompletedAt = now,
            LastModifiedAt = now,
        });
    }
}
