using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that syncs scheduled and cancelled appointments to Outlook Calendar via
/// Microsoft Graph. Runs every 5 minutes (AC-05). On token refresh failure the integration is
/// marked disconnected and the user must re-authorize (Edge Case: consent revoked).
/// </summary>
public sealed class OutlookCalendarSyncJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutlookCalendarSyncJob> _logger;

    public OutlookCalendarSyncJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OutlookCalendarSyncJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var outlookService = scope.ServiceProvider.GetRequiredService<IOutlookCalendarService>();
        var tokenStore = scope.ServiceProvider.GetRequiredService<IOutlookCalendarTokenStore>();

        var activeSyncs = await db.CalendarSyncs
            .AsNoTracking()
            .Where(c => c.Provider == CalendarProvider.Microsoft && c.IsActive)
            .ToListAsync()
            .ConfigureAwait(false);

        _logger.LogInformation(
            "OutlookCalendarSyncJob started. ActiveIntegrations={Count}.",
            activeSyncs.Count);

        foreach (var sync in activeSyncs)
        {
            await SyncUserAsync(sync.UserId, sync.LastSyncAt, db, outlookService, tokenStore)
                .ConfigureAwait(false);
        }

        _logger.LogInformation("OutlookCalendarSyncJob completed.");
    }

    private async Task SyncUserAsync(
        Guid userId,
        DateTime? lastSyncAt,
        ApplicationDbContext db,
        IOutlookCalendarService outlookService,
        IOutlookCalendarTokenStore tokenStore)
    {
        var tokens = await tokenStore
            .GetActiveTokensAsync(userId)
            .ConfigureAwait(false);

        if (tokens is null)
            return;

        var accessToken = await EnsureFreshTokenAsync(userId, tokens, outlookService, tokenStore)
            .ConfigureAwait(false);

        if (accessToken is null)
            return; // integration marked disconnected by EnsureFreshTokenAsync

        var since = lastSyncAt ?? DateTime.UtcNow.AddYears(-1);

        await CreateEventsForNewAppointmentsAsync(userId, since, accessToken, db, outlookService)
            .ConfigureAwait(false);

        await DeleteEventsForCancelledAppointmentsAsync(userId, since, accessToken, db, outlookService)
            .ConfigureAwait(false);

        var syncRecord = await db.CalendarSyncs
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == CalendarProvider.Microsoft)
            .ConfigureAwait(false);

        if (syncRecord is not null)
        {
            syncRecord.LastSyncAt = DateTime.UtcNow;
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns a valid access token, refreshing if expired.
    /// Returns <c>null</c> and marks the integration disconnected if refresh fails (AC-06, Edge Case).
    /// </summary>
    private async Task<string?> EnsureFreshTokenAsync(
        Guid userId,
        CalendarTokenResult tokens,
        IOutlookCalendarService outlookService,
        IOutlookCalendarTokenStore tokenStore)
    {
        const int ExpiryBufferSeconds = 60;

        if (tokens.TokenExpiry > DateTime.UtcNow.AddSeconds(ExpiryBufferSeconds))
            return tokens.AccessToken;

        _logger.LogInformation(
            "Outlook Calendar access token expired for UserId={UserId}. Refreshing.",
            userId);

        var refreshed = await outlookService
            .RefreshAccessTokenAsync(tokens.RefreshToken)
            .ConfigureAwait(false);

        if (refreshed is null)
        {
            _logger.LogWarning(
                "Outlook Calendar token refresh failed for UserId={UserId}. Marking integration disconnected.",
                userId);

            await tokenStore.MarkDisconnectedAsync(userId).ConfigureAwait(false);
            return null;
        }

        var newExpiry = DateTime.UtcNow.AddSeconds(refreshed.ExpiresInSeconds);

        await tokenStore
            .UpdateAccessTokenAsync(userId, refreshed.AccessToken, newExpiry)
            .ConfigureAwait(false);

        return refreshed.AccessToken;
    }

    private async Task CreateEventsForNewAppointmentsAsync(
        Guid userId,
        DateTime since,
        string accessToken,
        ApplicationDbContext db,
        IOutlookCalendarService outlookService)
    {
        var newAppointments = await db.Appointments
            .Include(a => a.Slot)
            .Where(a =>
                a.PatientId == userId &&
                a.Status == AppointmentStatus.Scheduled &&
                a.CreatedAt >= since)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var appointment in newAppointments)
        {
            var request = new AppointmentEventRequest(
                AppointmentId: appointment.AppointmentId,
                ProviderName: appointment.Slot.ProviderName,
                Specialty: appointment.Slot.Specialty,
                StartTimeUtc: appointment.Slot.StartTime,
                EndTimeUtc: appointment.Slot.EndTime);

            var eventId = await outlookService
                .CreateEventAsync(accessToken, request)
                .ConfigureAwait(false);

            if (eventId is null)
            {
                _logger.LogWarning(
                    "Failed to create Outlook Calendar event for AppointmentId={AppointmentId}.",
                    appointment.AppointmentId);
            }
            else
            {
                _logger.LogInformation(
                    "Created Outlook Calendar event. AppointmentId={AppointmentId}, EventId={EventId}.",
                    appointment.AppointmentId,
                    eventId);
            }
        }
    }

    private async Task DeleteEventsForCancelledAppointmentsAsync(
        Guid userId,
        DateTime since,
        string accessToken,
        ApplicationDbContext db,
        IOutlookCalendarService outlookService)
    {
        var cancelledAppointments = await db.Appointments
            .Where(a =>
                a.PatientId == userId &&
                a.Status == AppointmentStatus.Cancelled &&
                a.UpdatedAt >= since)
            .Select(a => a.AppointmentId)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var appointmentId in cancelledAppointments)
        {
            var deleted = await outlookService
                .DeleteEventByAppointmentIdAsync(accessToken, appointmentId)
                .ConfigureAwait(false);

            if (!deleted)
            {
                _logger.LogWarning(
                    "Failed to delete Outlook Calendar event for AppointmentId={AppointmentId}.",
                    appointmentId);
            }
        }
    }
}
