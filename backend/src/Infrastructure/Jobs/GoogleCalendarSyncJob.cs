using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that syncs scheduled and cancelled appointments to Google Calendar.
/// Runs every 5 minutes (AC-05). On token refresh failure the integration is marked disconnected
/// and the user must re-authorize (Edge Case: token refresh failure).
/// </summary>
public sealed class GoogleCalendarSyncJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GoogleCalendarSyncJob> _logger;

    public GoogleCalendarSyncJob(
        IServiceScopeFactory scopeFactory,
        ILogger<GoogleCalendarSyncJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var calendarService = scope.ServiceProvider.GetRequiredService<ICalendarService>();
        var tokenStore = scope.ServiceProvider.GetRequiredService<ICalendarTokenStore>();

        var activeSyncs = await db.CalendarSyncs
            .AsNoTracking()
            .Where(c => c.Provider == CalendarProvider.Google && c.IsActive)
            .ToListAsync()
            .ConfigureAwait(false);

        _logger.LogInformation(
            "GoogleCalendarSyncJob started. ActiveIntegrations={Count}.",
            activeSyncs.Count);

        foreach (var sync in activeSyncs)
        {
            await SyncUserAsync(sync.UserId, sync.LastSyncAt, db, calendarService, tokenStore)
                .ConfigureAwait(false);
        }

        _logger.LogInformation("GoogleCalendarSyncJob completed.");
    }

    private async Task SyncUserAsync(
        Guid userId,
        DateTime? lastSyncAt,
        ApplicationDbContext db,
        ICalendarService calendarService,
        ICalendarTokenStore tokenStore)
    {
        var tokens = await tokenStore
            .GetActiveTokensAsync(userId)
            .ConfigureAwait(false);

        if (tokens is null)
            return;

        var accessToken = await EnsureFreshTokenAsync(userId, tokens, calendarService, tokenStore)
            .ConfigureAwait(false);

        if (accessToken is null)
            return; // integration marked disconnected by EnsureFreshTokenAsync

        var since = lastSyncAt ?? DateTime.UtcNow.AddYears(-1);

        await CreateEventsForNewAppointmentsAsync(userId, since, accessToken, db, calendarService)
            .ConfigureAwait(false);

        await DeleteEventsForCancelledAppointmentsAsync(userId, since, accessToken, db, calendarService)
            .ConfigureAwait(false);

        // Update LastSyncAt on the CalendarSync record
        var syncRecord = await db.CalendarSyncs
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == CalendarProvider.Google)
            .ConfigureAwait(false);

        if (syncRecord is not null)
        {
            syncRecord.LastSyncAt = DateTime.UtcNow;
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns a valid access token, refreshing if expired.
    /// Returns <c>null</c> and marks the integration disconnected if refresh fails.
    /// </summary>
    private async Task<string?> EnsureFreshTokenAsync(
        Guid userId,
        CalendarTokenResult tokens,
        ICalendarService calendarService,
        ICalendarTokenStore tokenStore)
    {
        const int ExpiryBufferSeconds = 60;

        if (tokens.TokenExpiry > DateTime.UtcNow.AddSeconds(ExpiryBufferSeconds))
            return tokens.AccessToken;

        _logger.LogInformation(
            "Google Calendar access token expired for UserId={UserId}. Refreshing.",
            userId);

        var refreshed = await calendarService
            .RefreshAccessTokenAsync(tokens.RefreshToken)
            .ConfigureAwait(false);

        if (refreshed is null)
        {
            _logger.LogWarning(
                "Google Calendar token refresh failed for UserId={UserId}. Marking integration disconnected.",
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
        ICalendarService calendarService)
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

            var eventId = await calendarService
                .CreateEventAsync(accessToken, request)
                .ConfigureAwait(false);

            if (eventId is null)
            {
                _logger.LogWarning(
                    "Failed to create Google Calendar event for AppointmentId={AppointmentId}.",
                    appointment.AppointmentId);
            }
            else
            {
                _logger.LogInformation(
                    "Created Google Calendar event. AppointmentId={AppointmentId}, EventId={EventId}.",
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
        ICalendarService calendarService)
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
            var deleted = await calendarService
                .DeleteEventByAppointmentIdAsync(accessToken, appointmentId)
                .ConfigureAwait(false);

            if (!deleted)
            {
                _logger.LogWarning(
                    "Failed to delete Google Calendar event for AppointmentId={AppointmentId}.",
                    appointmentId);
            }
        }
    }
}
