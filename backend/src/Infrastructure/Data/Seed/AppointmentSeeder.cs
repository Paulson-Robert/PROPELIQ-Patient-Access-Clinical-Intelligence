using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seed;

public static class AppointmentSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var patientUserId = await context.Users
            .Where(u => u.Email == SeedDataConstants.PatientEmail)
            .Select(u => u.UserId)
            .SingleAsync();

        var providerUserId = await context.Users
            .Where(u => u.Email == SeedDataConstants.StaffEmail)
            .Select(u => u.UserId)
            .SingleAsync();

        var slotIds = await SeedSlotsAsync(context, providerUserId);
        await SeedAppointmentsAsync(context, patientUserId, providerUserId, slotIds);
    }

    private static async Task<Dictionary<string, Guid>> SeedSlotsAsync(ApplicationDbContext context, Guid providerUserId)
    {
        var slotTemplates = new List<(string Key, Guid SlotId, DateTime Start, DateTime End)>
        {
            ("slot-1", SeedDataConstants.SlotOneId, new DateTime(2026, 1, 16, 9, 0, 0, DateTimeKind.Utc),  new DateTime(2026, 1, 16, 9, 30, 0, DateTimeKind.Utc)),
            ("slot-2", SeedDataConstants.SlotTwoId, new DateTime(2026, 1, 16, 9, 30, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16, 10, 0, 0, DateTimeKind.Utc)),
            ("slot-3", SeedDataConstants.SlotThreeId, new DateTime(2026, 1, 16, 10, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16, 10, 30, 0, DateTimeKind.Utc)),
            ("slot-4", SeedDataConstants.SlotFourId, new DateTime(2026, 1, 16, 10, 30, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16, 11, 0, 0, DateTimeKind.Utc)),
            ("slot-5", SeedDataConstants.SlotFiveId, new DateTime(2026, 1, 16, 11, 0, 0, DateTimeKind.Utc),  new DateTime(2026, 1, 16, 11, 30, 0, DateTimeKind.Utc)),
        };

        var resolvedSlotIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var newSlots = new List<AvailabilitySlot>();

        foreach (var template in slotTemplates)
        {
            var existingSlot = await context.AvailabilitySlots
                .Where(s => s.SlotId == template.SlotId
                            || (s.ProviderId == providerUserId
                                && s.StartTime == template.Start
                                && s.EndTime == template.End))
                .Select(s => new { s.SlotId })
                .FirstOrDefaultAsync();

            if (existingSlot is not null)
            {
                resolvedSlotIds[template.Key] = existingSlot.SlotId;
                continue;
            }

            newSlots.Add(new AvailabilitySlot
            {
                SlotId = template.SlotId,
                ProviderId = providerUserId,
                ProviderName = "Dr. Morgan Lee",
                Specialty = "Family Medicine",
                StartTime = template.Start,
                EndTime = template.End,
                IsAvailable = true,
                IsLocked = false,
                LockExpiry = null,
                RecurrencePattern = null,
            });

            resolvedSlotIds[template.Key] = template.SlotId;
        }

        if (newSlots.Count > 0)
        {
            context.AvailabilitySlots.AddRange(newSlots);
            await context.SaveChangesAsync();
        }

        return resolvedSlotIds;
    }

    private static async Task SeedAppointmentsAsync(
        ApplicationDbContext context,
        Guid patientUserId,
        Guid providerUserId,
        IReadOnlyDictionary<string, Guid> slotIds)
    {
        var appointmentTemplates = new List<(Guid AppointmentId, string SlotKey, BookingType BookingType, AppointmentStatus Status)>
        {
            (SeedDataConstants.AppointmentOneId, "slot-2", BookingType.Online, AppointmentStatus.Scheduled),
            (SeedDataConstants.AppointmentTwoId, "slot-4", BookingType.WalkIn, AppointmentStatus.Scheduled),
        };

        var appointmentsToInsert = new List<Appointment>();

        foreach (var template in appointmentTemplates)
        {
            Guid slotId = slotIds[template.SlotKey];

            bool appointmentExists = await context.Appointments.AnyAsync(a =>
                a.AppointmentId == template.AppointmentId
                || (a.PatientId == patientUserId && a.SlotId == slotId));

            if (appointmentExists)
                continue;

            appointmentsToInsert.Add(new Appointment
            {
                AppointmentId = template.AppointmentId,
                PatientId = patientUserId,
                ProviderId = providerUserId,
                SlotId = slotId,
                Status = template.Status,
                BookingType = template.BookingType,
                PreferredSlotId = null,
                CreatedByUserId = providerUserId,
                CreatedAt = SeedDataConstants.SeedCreatedAtUtc,
                UpdatedAt = SeedDataConstants.SeedCreatedAtUtc,
            });
        }

        if (appointmentsToInsert.Count == 0)
            return;

        context.Appointments.AddRange(appointmentsToInsert);
        await context.SaveChangesAsync();
    }
}