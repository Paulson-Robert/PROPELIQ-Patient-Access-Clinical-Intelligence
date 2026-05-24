using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Infrastructure.Data;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Security.Tests;

public sealed class DocumentStorageServiceTests
{
    [Fact]
    public async Task StoreAsync_WhenPatientProfileIsMissing_CreatesProfileAndStoresDocument()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.Users.Add(new User
        {
            UserId = userId,
            Email = "paulson.robert@kanini.com",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var storagePath = Path.Combine(
            AppContext.BaseDirectory,
            "document-storage-tests",
            Guid.NewGuid().ToString("N"));

        try
        {
            var service = new DocumentStorageService(
                context,
                Options.Create(new DocumentStorageOptions { BasePath = storagePath }),
                new CapturingBackgroundJobClient(),
                NullLogger<DocumentStorageService>.Instance);

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test clinical pdf"));

            var result = await service.StoreAsync(
                new UploadDocumentRequest(
                    userId,
                    "visit-summary.pdf",
                    "application/pdf",
                    stream.Length,
                    stream),
                CancellationToken.None);

            Assert.True(result.Success, result.FailureReason);
            Assert.NotNull(result.DocumentId);

            var profile = await context.PatientProfiles.SingleAsync(p => p.UserId == userId);
            Assert.Equal("Paulson", profile.FirstName);
            Assert.Equal("Robert", profile.LastName);

            var document = await context.ClinicalDocuments.SingleAsync(d => d.DocumentId == result.DocumentId);
            Assert.Equal(profile.PatientProfileId, document.PatientProfileId);
            Assert.Equal("visit-summary.pdf", document.FileName);
            Assert.Equal(DocumentProcessingStatus.Completed, document.ProcessingStatus);
            Assert.Equal(MalwareScanStatus.Clean, document.MalwareScanStatus);
            Assert.True(File.Exists(document.StoragePath));
        }
        finally
        {
            if (Directory.Exists(storagePath))
                Directory.Delete(storagePath, recursive: true);
        }
    }

    [Fact]
    public async Task StoreAsync_WhenBasePathIsBlank_UsesDefaultStoragePath()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        context.Users.Add(new User
        {
            UserId = userId,
            Email = "patient@example.com",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Patient,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        context.PatientProfiles.Add(new PatientProfile
        {
            PatientProfileId = profileId,
            UserId = userId,
            FirstName = "Test",
            LastName = "Patient",
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = new DocumentStorageService(
            context,
            Options.Create(new DocumentStorageOptions { BasePath = "" }),
            new CapturingBackgroundJobClient(),
            NullLogger<DocumentStorageService>.Instance);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test clinical pdf"));

        var result = await service.StoreAsync(
            new UploadDocumentRequest(
                userId,
                "blank-base-path.pdf",
                "application/pdf",
                stream.Length,
                stream),
            CancellationToken.None);

        Assert.True(result.Success, result.FailureReason);

        var document = await context.ClinicalDocuments.SingleAsync(d => d.DocumentId == result.DocumentId);
        Assert.StartsWith(
            DocumentStorageOptions.DefaultBasePath,
            document.StoragePath,
            StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(document.StoragePath));

        File.Delete(document.StoragePath);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"document-storage-tests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options, null);
    }

    private sealed class CapturingBackgroundJobClient : IBackgroundJobClient
    {
        public string Create(Job job, IState state) => Guid.NewGuid().ToString("N");

        public bool ChangeState(string jobId, IState state, string expectedState) => true;
    }
}
