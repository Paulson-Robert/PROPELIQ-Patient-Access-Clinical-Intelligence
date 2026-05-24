using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Security.Tests;

public sealed class ManualIntakeServiceTests
{
    [Fact]
    public async Task SubmitAsync_ForPatientAppointment_PersistsManualIntakeAndDoesNotReturnCompletedDraft()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, patientUserId);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SubmitAsync(
            new SubmitManualIntakeRequest(
                patientUserId,
                "Patient",
                appointmentId,
                "Asthma",
                "Appendectomy",
                "Mother has diabetes",
                "Headache",
                "2026-05-24",
                "4",
                "Albuterol",
                "Penicillin",
                "Annual physical"),
            CancellationToken.None);

        Assert.True(result.Success);

        var record = await context.IntakeRecords.SingleAsync();
        Assert.Equal(profileId, record.PatientProfileId);
        Assert.Equal(IntakeMode.Manual, record.IntakeMode);
        Assert.NotNull(record.CompletedAt);
        Assert.Contains("\"text\":\"Albuterol\"", record.Medications);
        Assert.Contains("\"text\":\"Penicillin\"", record.Allergies);

        var draft = await service.GetDraftAsync(patientUserId, "Patient", appointmentId, CancellationToken.None);

        Assert.Null(draft);
    }

    [Fact]
    public async Task SubmitAsync_ForDifferentPatientAppointment_IsRejected()
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

        var result = await service.SubmitAsync(
            new SubmitManualIntakeRequest(
                actorPatientUserId,
                "Patient",
                appointmentId,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "Wrong appointment"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("APPOINTMENT_NOT_FOUND", result.FailureCode);
        Assert.Empty(context.IntakeRecords);
    }

    [Fact]
    public async Task SubmitAsync_ForStaffActor_UsesAppointmentPatientProfile()
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

        var result = await service.SubmitAsync(
            new SubmitManualIntakeRequest(
                staffUserId,
                "Staff",
                appointmentId,
                null,
                null,
                null,
                "Cough",
                null,
                "3",
                null,
                null,
                "Walk-in triage"),
            CancellationToken.None);

        Assert.True(result.Success);

        var record = await context.IntakeRecords.SingleAsync();
        Assert.Equal(profileId, record.PatientProfileId);
        Assert.Equal(appointmentId, record.AppointmentId);
        Assert.Equal("Walk-in triage", record.ReasonForVisit);
    }

    [Fact]
    public async Task SubmitAsync_WhenAiRecordExists_CreatesManualRecordWithoutOverwritingAi()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, patientUserId);
        SeedCompletedIntake(context, profileId, appointmentId, IntakeMode.AI, "AI symptom");
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SubmitAsync(
            new SubmitManualIntakeRequest(
                patientUserId,
                "Patient",
                appointmentId,
                null,
                null,
                null,
                "Manual symptom",
                null,
                null,
                null,
                null,
                "Manual symptom"),
            CancellationToken.None);

        Assert.True(result.Success);

        var records = await context.IntakeRecords
            .OrderBy(r => r.IntakeMode)
            .ToListAsync();

        Assert.Equal(2, records.Count);
        Assert.Contains(records, r => r.IntakeMode == IntakeMode.AI && r.ReasonForVisit == "AI symptom");
        Assert.Contains(records, r => r.IntakeMode == IntakeMode.Manual && r.ReasonForVisit == "Manual symptom");
    }

    [Fact]
    public async Task SubmitAsync_WhenCompletedManualRecordExists_CreatesNewManualHistoryEntry()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var profileId = SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, patientUserId);
        SeedCompletedIntake(context, profileId, appointmentId, IntakeMode.Manual, "First manual symptom");
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SubmitAsync(
            new SubmitManualIntakeRequest(
                patientUserId,
                "Patient",
                appointmentId,
                null,
                null,
                null,
                "Second manual symptom",
                null,
                null,
                null,
                null,
                "Second manual symptom"),
            CancellationToken.None);

        Assert.True(result.Success);

        var records = await context.IntakeRecords
            .Where(r => r.IntakeMode == IntakeMode.Manual)
            .ToListAsync();

        Assert.Equal(2, records.Count);
        Assert.Contains(records, r => r.ReasonForVisit == "First manual symptom");
        Assert.Contains(records, r => r.ReasonForVisit == "Second manual symptom");
    }

    [Fact]
    public async Task SubmitAsync_WhenOpenDraftExists_CompletesDraftWithoutCreatingDuplicateDraft()
    {
        var context = CreateContext();
        var patientUserId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        SeedPatient(context, patientUserId);
        SeedAppointment(context, appointmentId, patientUserId, patientUserId);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var draftResult = await service.SaveDraftAsync(
            new SaveIntakeDraftRequest(
                patientUserId,
                "Patient",
                appointmentId,
                "Asthma",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "Draft reason"),
            CancellationToken.None);

        Assert.True(draftResult.Success);

        var submitResult = await service.SubmitAsync(
            new SubmitManualIntakeRequest(
                patientUserId,
                "Patient",
                appointmentId,
                "Asthma",
                null,
                null,
                "Cough",
                null,
                null,
                null,
                null,
                "Completed reason"),
            CancellationToken.None);

        Assert.True(submitResult.Success);
        Assert.Equal(draftResult.IntakeId, submitResult.IntakeId);

        var record = await context.IntakeRecords.SingleAsync();
        Assert.NotNull(record.CompletedAt);
        Assert.Equal("Completed reason", record.ReasonForVisit);

        var draft = await service.GetDraftAsync(patientUserId, "Patient", appointmentId, CancellationToken.None);
        Assert.Null(draft);
    }

    private static ManualIntakeService CreateService(ApplicationDbContext context)
        => new(context, NullLogger<ManualIntakeService>.Instance);

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"manual-intake-{Guid.NewGuid()}")
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
