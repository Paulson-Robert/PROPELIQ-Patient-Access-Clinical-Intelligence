using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seed;

public static class UserSeeder
{
    private const string SeedPassword = "P@ssword123!";

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedUsersAsync(context);
        await SeedPatientProfileAsync(context);
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        var existingUsersByEmail = await context.Users
            .Where(u => u.Email == SeedDataConstants.PatientEmail
                        || u.Email == SeedDataConstants.StaffEmail
                        || u.Email == SeedDataConstants.AdminEmail)
            .Select(u => u.Email)
            .ToListAsync();

        var existingUserEmails = existingUsersByEmail.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var usersToInsert = new List<User>();

        if (!existingUserEmails.Contains(SeedDataConstants.PatientEmail))
        {
            usersToInsert.Add(new User
            {
                UserId = SeedDataConstants.PatientUserId,
                Email = SeedDataConstants.PatientEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedPassword),
                AuthProvider = AuthProvider.Local,
                Role = UserRole.Patient,
                MfaEnabled = false,
                IsActive = true,
                CreatedAt = SeedDataConstants.SeedCreatedAtUtc,
                UpdatedAt = SeedDataConstants.SeedCreatedAtUtc,
            });
        }

        if (!existingUserEmails.Contains(SeedDataConstants.StaffEmail))
        {
            usersToInsert.Add(new User
            {
                UserId = SeedDataConstants.StaffUserId,
                Email = SeedDataConstants.StaffEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedPassword),
                AuthProvider = AuthProvider.Local,
                Role = UserRole.Staff,
                MfaEnabled = true,
                IsActive = true,
                CreatedAt = SeedDataConstants.SeedCreatedAtUtc,
                UpdatedAt = SeedDataConstants.SeedCreatedAtUtc,
            });
        }

        if (!existingUserEmails.Contains(SeedDataConstants.AdminEmail))
        {
            usersToInsert.Add(new User
            {
                UserId = SeedDataConstants.AdminUserId,
                Email = SeedDataConstants.AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedPassword),
                AuthProvider = AuthProvider.Local,
                Role = UserRole.Admin,
                MfaEnabled = true,
                IsActive = true,
                CreatedAt = SeedDataConstants.SeedCreatedAtUtc,
                UpdatedAt = SeedDataConstants.SeedCreatedAtUtc,
            });
        }

        if (usersToInsert.Count == 0)
            return;

        context.Users.AddRange(usersToInsert);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPatientProfileAsync(ApplicationDbContext context)
    {
        var patientUser = await context.Users
            .Where(u => u.Email == SeedDataConstants.PatientEmail)
            .Select(u => new { u.UserId })
            .SingleAsync();

        bool profileExists = await context.PatientProfiles
            .AnyAsync(p => p.UserId == patientUser.UserId);

        if (profileExists)
            return;

        context.PatientProfiles.Add(new PatientProfile
        {
            PatientProfileId = SeedDataConstants.PatientProfileId,
            UserId = patientUser.UserId,
            FirstName = "Taylor",
            LastName = "Reed",
            DateOfBirth = new DateOnly(1992, 5, 10),
            Phone = "+1-555-0100",
            InsuranceId = "SEED-INS-1001",
            InsuranceName = "SeedCare Health",
            InsuranceValidationStatus = InsuranceValidationStatus.Valid,
            CreatedAt = SeedDataConstants.SeedCreatedAtUtc,
        });

        await context.SaveChangesAsync();
    }
}