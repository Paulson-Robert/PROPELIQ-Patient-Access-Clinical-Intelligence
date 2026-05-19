using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting development seed data initialization.");

        await UserSeeder.SeedAsync(context);
        await AppointmentSeeder.SeedAsync(context);
        await ClinicalDataSeeder.SeedAsync(context);
        await CodeTableSeeder.SeedAsync(context);

        logger.LogInformation("Development seed data initialization completed.");
    }
}

internal static class SeedDataConstants
{
    public const string PatientEmail = "patient.seed@propeliq.local";
    public const string StaffEmail = "staff.seed@propeliq.local";
    public const string AdminEmail = "admin.seed@propeliq.local";

    public static readonly Guid PatientUserId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid StaffUserId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminUserId = new("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PatientProfileId = new("44444444-4444-4444-4444-444444444444");

    public static readonly Guid SlotOneId = new("55555555-5555-5555-5555-555555555551");
    public static readonly Guid SlotTwoId = new("55555555-5555-5555-5555-555555555552");
    public static readonly Guid SlotThreeId = new("55555555-5555-5555-5555-555555555553");
    public static readonly Guid SlotFourId = new("55555555-5555-5555-5555-555555555554");
    public static readonly Guid SlotFiveId = new("55555555-5555-5555-5555-555555555555");

    public static readonly Guid AppointmentOneId = new("66666666-6666-6666-6666-666666666661");
    public static readonly Guid AppointmentTwoId = new("66666666-6666-6666-6666-666666666662");

    public static readonly Guid ClinicalDocumentId = new("77777777-7777-7777-7777-777777777777");
    public static readonly Guid ExtractedRecordOneId = new("88888888-8888-8888-8888-888888888881");
    public static readonly Guid ExtractedRecordTwoId = new("88888888-8888-8888-8888-888888888882");
    public static readonly Guid ExtractedRecordThreeId = new("88888888-8888-8888-8888-888888888883");

    public static readonly DateTime SeedCreatedAtUtc = new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
}