using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Security.Tests;

/// <summary>
/// Regression tests for BUG_notification_retrycount_null.
///
/// Root cause: NotificationConfiguration previously called .HasDefaultValue(0) on RetryCount,
/// which caused EF Core to set ValueGeneratedOnAdd and omit the column from INSERT statements
/// when the value was 0. The actual PostgreSQL column has no DEFAULT clause, so the INSERT
/// failed with a NOT NULL constraint violation (PostgreSQL error 23502).
/// </summary>
public sealed class NotificationRetryCountInsertTests
{
    // -------------------------------------------------------------------------
    // Model metadata: RetryCount must NOT be ValueGenerated.OnAdd
    // -------------------------------------------------------------------------

    [Fact]
    public void RetryCount_EfCoreModel_HasValueGeneratedNever()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(Notification))!;
        var property = entityType.FindProperty(nameof(Notification.RetryCount))!;

        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
    }

    // -------------------------------------------------------------------------
    // Functional: Notification with RetryCount = 0 must persist without error
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddNotification_WithRetryCountZero_SaveChangesSucceeds()
    {
        using var context = CreateContext();
        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        context.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            AppointmentId = appointmentId,
            PatientId = patientId,
            Channel = NotificationChannel.Email,
            NotificationType = NotificationType.Reminder24Hour,
            Status = NotificationStatus.Queued,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
        });

        // Must not throw DbUpdateException
        await context.SaveChangesAsync();

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(0, saved.RetryCount);
    }

    [Fact]
    public async Task AddNotification_WithRetryCountNonZero_SaveChangesPreservesValue()
    {
        using var context = CreateContext();

        context.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            Channel = NotificationChannel.SMS,
            NotificationType = NotificationType.Reminder2Hour,
            Status = NotificationStatus.Failed,
            RetryCount = 3,
            CreatedAt = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(3, saved.RetryCount);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"notification-retrycount-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options, null);
    }
}
