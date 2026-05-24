using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.AI;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Security.Tests;

public sealed class AiIntakePersistenceServiceTests
{
    [Fact]
    public async Task PersistAsync_ForPatientAppointment_PersistsAiIntakeRecord()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, patientUserId);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.PersistAsync(
            patientUserId,
            "Patient",
            appointmentId,
            new AiIntakeSummary(
                "Asthma",
                "Albuterol",
                "Penicillin",
                "Appendectomy",
                "Annual physical"),
            CancellationToken.None);

        Assert.True(result.Success);

        var record = await context.IntakeRecords.SingleAsync();
        Assert.Equal(profileId, record.PatientProfileId);
        Assert.Equal(IntakeMode.AI, record.IntakeMode);
        Assert.NotNull(record.CompletedAt);
        Assert.Equal("Annual physical", record.ReasonForVisit);
        Assert.Contains("\"text\":\"Albuterol\"", record.Medications);
        Assert.Contains("\"text\":\"Penicillin\"", record.Allergies);
    }

    [Fact]
    public async Task PersistAsync_ForDifferentPatientAppointment_IsRejected()
    {
        var context = CreateContext();
        var actorPatientUserId = Guid.NewGuid();
        var appointmentPatientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        SeedPatient(context, actorPatientUserId);
        SeedPatient(context, appointmentPatientUserId);
        SeedAppointment(context, appointmentId, appointmentPatientUserId, appointmentPatientUserId);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.PersistAsync(
            actorPatientUserId,
            "Patient",
            appointmentId,
            new AiIntakeSummary(null, null, null, null, "Wrong appointment"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("APPOINTMENT_NOT_FOUND", result.FailureCode);
        Assert.Empty(context.IntakeRecords);
    }

    [Fact]
    public async Task PersistAsync_ForStaffActor_UsesAppointmentPatientProfile()
    {
        var context = CreateContext();
        var staffUserId = Guid.NewGuid();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        SeedStaff(context, staffUserId);
        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, staffUserId);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.PersistAsync(
            staffUserId,
            "Staff",
            appointmentId,
            new AiIntakeSummary(null, null, null, null, "Walk-in triage"),
            CancellationToken.None);

        Assert.True(result.Success);

        var record = await context.IntakeRecords.SingleAsync();
        Assert.Equal(profileId, record.PatientProfileId);
        Assert.Equal(appointmentId, record.AppointmentId);
        Assert.Equal(IntakeMode.AI, record.IntakeMode);
        Assert.Equal("Walk-in triage", record.ReasonForVisit);
    }

    [Fact]
    public async Task PersistAsync_WhenManualRecordExists_CreatesAiRecordWithoutOverwritingManual()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, patientUserId);
        SeedCompletedIntake(context, profileId, appointmentId, IntakeMode.Manual, "Manual symptom");
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.PersistAsync(
            patientUserId,
            "Patient",
            appointmentId,
            new AiIntakeSummary(null, null, null, null, "AI symptom"),
            CancellationToken.None);

        Assert.True(result.Success);

        var records = await context.IntakeRecords
            .OrderBy(r => r.IntakeMode)
            .ToListAsync();

        Assert.Equal(2, records.Count);
        Assert.Contains(records, r => r.IntakeMode == IntakeMode.Manual && r.ReasonForVisit == "Manual symptom");
        Assert.Contains(records, r => r.IntakeMode == IntakeMode.AI && r.ReasonForVisit == "AI symptom");
    }

    [Fact]
    public async Task PersistAsync_WhenCompletedAiRecordExists_CreatesNewAiHistoryEntry()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, patientUserId);
        SeedCompletedIntake(context, profileId, appointmentId, IntakeMode.AI, "First AI symptom");
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.PersistAsync(
            patientUserId,
            "Patient",
            appointmentId,
            new AiIntakeSummary(null, null, null, null, "Second AI symptom"),
            CancellationToken.None);

        Assert.True(result.Success);

        var records = await context.IntakeRecords
            .Where(r => r.IntakeMode == IntakeMode.AI)
            .ToListAsync();

        Assert.Equal(2, records.Count);
        Assert.Contains(records, r => r.ReasonForVisit == "First AI symptom");
        Assert.Contains(records, r => r.ReasonForVisit == "Second AI symptom");
    }

    private static AiIntakePersistenceService CreateService(ApplicationDbContext context)
        => new(context, NullLogger<AiIntakePersistenceService>.Instance);

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"ai-intake-{Guid.NewGuid()}")
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
        Guid patientUserId,
        Guid createdByUserId)
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
            CreatedByUserId = createdByUserId,
            CreatedAt = now,
            UpdatedAt = now,
        });
    }

    private static void SeedCompletedIntake(
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
