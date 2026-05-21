using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Security.Tests;

public sealed class PatientSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_ByNameAndDob_ReturnsMatchingPatients()
    {
        var context = CreateContext();
        var matchUserId = Guid.NewGuid();

        SeedPatient(context, matchUserId, "alana.stone@example.com", "Alana", "Stone", new DateOnly(1992, 6, 10));
        SeedPatient(context, Guid.NewGuid(), "alan.rivers@example.com", "Alan", "Rivers", new DateOnly(1989, 4, 1));
        SeedPatient(context, Guid.NewGuid(), "nina.wu@example.com", "Nina", "Wu", new DateOnly(1992, 6, 10));
        await context.SaveChangesAsync();

        var service = new PatientSearchService(context);

        var results = await service.SearchAsync("ala", new DateOnly(1992, 6, 10), CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(matchUserId, results[0].UserId);
        Assert.Equal("Alana", results[0].FirstName);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyName_ReturnsEmptyList()
    {
        var context = CreateContext();
        var service = new PatientSearchService(context);

        var results = await service.SearchAsync("   ", null, CancellationToken.None);

        Assert.Empty(results);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"patient-search-tests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options, null);
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
}
