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
        var now   = DateTime.UtcNow;
        var today = now.Date; // DateTimeKind.Utc preserved from UtcNow

        // Legacy slots — fixed dates, referenced by seeded test appointments
        var legacyTemplates = new List<(string Key, Guid SlotId, DateTime Start, DateTime End)>
        {
            ("slot-1", SeedDataConstants.SlotOneId,   new DateTime(2026, 1, 16,  9,  0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16,  9, 30, 0, DateTimeKind.Utc)),
            ("slot-2", SeedDataConstants.SlotTwoId,   new DateTime(2026, 1, 16,  9, 30, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16, 10,  0, 0, DateTimeKind.Utc)),
            ("slot-3", SeedDataConstants.SlotThreeId, new DateTime(2026, 1, 16, 10,  0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16, 10, 30, 0, DateTimeKind.Utc)),
            ("slot-4", SeedDataConstants.SlotFourId,  new DateTime(2026, 1, 16, 10, 30, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16, 11,  0, 0, DateTimeKind.Utc)),
            ("slot-5", SeedDataConstants.SlotFiveId,  new DateTime(2026, 1, 16, 11,  0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 16, 11, 30, 0, DateTimeKind.Utc)),
        };

        // Rolling future slots — 20 slots spread 2 per week across 10 weeks (~70 days).
        // With this spread a user can book appointments in any month from today through
        // ~10 weeks out without exhausting the pool between restarts.
        // On each restart any consumed (booked/locked) or expired slot is rolled forward.
        var futureTemplates = new List<(string Key, Guid SlotId, int DaysAhead, TimeSpan StartHour, TimeSpan Duration, string ProviderName, string Specialty)>
        {
            // Week 1
            ("future-1",  SeedDataConstants.FutureSlotOneId,       2, new TimeSpan( 9,  0, 0), TimeSpan.FromMinutes(30), "Dr. Sarah Chen",      "Internal Medicine"),
            ("future-2",  SeedDataConstants.FutureSlotTwoId,       4, new TimeSpan(10,  0, 0), TimeSpan.FromMinutes(45), "Dr. Michael Okafor",  "Cardiology"),
            // Week 2
            ("future-3",  SeedDataConstants.FutureSlotThreeId,     7, new TimeSpan(14,  0, 0), TimeSpan.FromMinutes(30), "Dr. Sarah Chen",      "Internal Medicine"),
            ("future-4",  SeedDataConstants.FutureSlotFourId,      9, new TimeSpan( 9,  0, 0), TimeSpan.FromMinutes(20), "Dr. Emily Johansson", "Dermatology"),
            // Week 3
            ("future-5",  SeedDataConstants.FutureSlotFiveId,     14, new TimeSpan(11,  0, 0), TimeSpan.FromMinutes(45), "Dr. Michael Okafor",  "Cardiology"),
            ("future-6",  SeedDataConstants.FutureSlotSixId,      16, new TimeSpan(13,  0, 0), TimeSpan.FromMinutes(30), "Dr. Lisa Nakamura",   "Family Medicine"),
            // Week 4
            ("future-7",  SeedDataConstants.FutureSlotSevenId,    21, new TimeSpan( 9,  0, 0), TimeSpan.FromMinutes(30), "Dr. Sarah Chen",      "Internal Medicine"),
            ("future-8",  SeedDataConstants.FutureSlotEightId,    23, new TimeSpan(10,  0, 0), TimeSpan.FromMinutes(45), "Dr. Michael Okafor",  "Cardiology"),
            // Week 5
            ("future-9",  SeedDataConstants.FutureSlotNineId,     28, new TimeSpan(14,  0, 0), TimeSpan.FromMinutes(30), "Dr. Emily Johansson", "Dermatology"),
            ("future-10", SeedDataConstants.FutureSlotTenId,      30, new TimeSpan( 9,  0, 0), TimeSpan.FromMinutes(30), "Dr. Sarah Chen",      "Internal Medicine"),
            // Week 6
            ("future-11", SeedDataConstants.FutureSlotElevenId,   35, new TimeSpan(11,  0, 0), TimeSpan.FromMinutes(45), "Dr. Michael Okafor",  "Cardiology"),
            ("future-12", SeedDataConstants.FutureSlotTwelveId,   37, new TimeSpan(13,  0, 0), TimeSpan.FromMinutes(30), "Dr. Lisa Nakamura",   "Family Medicine"),
            // Week 7
            ("future-13", SeedDataConstants.FutureSlotThirteenId, 42, new TimeSpan( 9,  0, 0), TimeSpan.FromMinutes(30), "Dr. Sarah Chen",      "Internal Medicine"),
            ("future-14", SeedDataConstants.FutureSlotFourteenId, 44, new TimeSpan(10,  0, 0), TimeSpan.FromMinutes(45), "Dr. Michael Okafor",  "Cardiology"),
            // Week 8
            ("future-15", SeedDataConstants.FutureSlotFifteenId,  49, new TimeSpan(14,  0, 0), TimeSpan.FromMinutes(30), "Dr. Emily Johansson", "Dermatology"),
            ("future-16", SeedDataConstants.FutureSlotSixteenId,  51, new TimeSpan( 9,  0, 0), TimeSpan.FromMinutes(30), "Dr. Lisa Nakamura",   "Family Medicine"),
            // Week 9
            ("future-17", SeedDataConstants.FutureSlotSeventeenId,56, new TimeSpan(11,  0, 0), TimeSpan.FromMinutes(45), "Dr. Sarah Chen",      "Internal Medicine"),
            ("future-18", SeedDataConstants.FutureSlotEighteenId, 58, new TimeSpan(13,  0, 0), TimeSpan.FromMinutes(30), "Dr. Michael Okafor",  "Cardiology"),
            // Week 10
            ("future-19", SeedDataConstants.FutureSlotNineteenId, 63, new TimeSpan( 9,  0, 0), TimeSpan.FromMinutes(30), "Dr. Emily Johansson", "Dermatology"),
            ("future-20", SeedDataConstants.FutureSlotTwentyId,   65, new TimeSpan(10,  0, 0), TimeSpan.FromMinutes(45), "Dr. Lisa Nakamura",   "Family Medicine"),
        };

        var resolvedSlotIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var newSlots = new List<AvailabilitySlot>();

        // --- Legacy slots: insert once, never update ---
        foreach (var t in legacyTemplates)
        {
            var existing = await context.AvailabilitySlots
                .Where(s => s.SlotId == t.SlotId
                            || (s.ProviderId == providerUserId
                                && s.StartTime == t.Start
                                && s.EndTime == t.End))
                .Select(s => new { s.SlotId })
                .FirstOrDefaultAsync();

            if (existing is not null)
            {
                resolvedSlotIds[t.Key] = existing.SlotId;
                continue;
            }

            newSlots.Add(new AvailabilitySlot
            {
                SlotId        = t.SlotId,
                ProviderId    = providerUserId,
                ProviderName  = "Dr. Morgan Lee",
                Specialty     = "Family Medicine",
                StartTime     = t.Start,
                EndTime       = t.End,
                IsAvailable   = true,
                IsLocked      = false,
                LockExpiry    = null,
                RecurrencePattern = null,
            });

            resolvedSlotIds[t.Key] = t.SlotId;
        }

        // --- Rolling future slots: insert on first run; roll forward on restart if consumed or expired ---
        foreach (var t in futureTemplates)
        {
            var rollingStart = today.AddDays(t.DaysAhead).Add(t.StartHour);
            var rollingEnd   = rollingStart.Add(t.Duration);

            var existing = await context.AvailabilitySlots
                .Where(s => s.SlotId == t.SlotId)
                .FirstOrDefaultAsync();

            if (existing is not null)
            {
                // Reset if the slot has expired OR has been consumed (booked/locked).
                // In a dev seed context every restart should restore a full set of bookable slots.
                bool expired  = existing.StartTime < now;
                bool consumed = !existing.IsAvailable || existing.IsLocked;

                if (expired || consumed)
                {
                    existing.StartTime   = rollingStart;
                    existing.EndTime     = rollingEnd;
                    existing.IsAvailable = true;
                    existing.IsLocked    = false;
                    existing.LockExpiry  = null;
                }

                resolvedSlotIds[t.Key] = existing.SlotId;
                continue;
            }

            newSlots.Add(new AvailabilitySlot
            {
                SlotId        = t.SlotId,
                ProviderId    = providerUserId,
                ProviderName  = t.ProviderName,
                Specialty     = t.Specialty,
                StartTime     = rollingStart,
                EndTime       = rollingEnd,
                IsAvailable   = true,
                IsLocked      = false,
                LockExpiry    = null,
                RecurrencePattern = null,
            });

            resolvedSlotIds[t.Key] = t.SlotId;
        }

        if (newSlots.Count > 0)
            context.AvailabilitySlots.AddRange(newSlots);

        await context.SaveChangesAsync();

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