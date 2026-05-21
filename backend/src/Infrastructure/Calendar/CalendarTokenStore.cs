using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Calendar;

/// <summary>
/// Persists Google Calendar OAuth tokens encrypted at rest using ASP.NET Core Data Protection.
/// Keys must be persisted to durable storage in production (e.g. Azure Key Vault, AWS SSM).
/// </summary>
public sealed class CalendarTokenStore : ICalendarTokenStore
{
    private const string PurposeName = "GoogleCalendarTokens.v1";

    private readonly ApplicationDbContext _db;
    private readonly IDataProtector _protector;

    public CalendarTokenStore(ApplicationDbContext db, IDataProtectionProvider dataProtectionProvider)
    {
        _db = db;
        _protector = dataProtectionProvider.CreateProtector(PurposeName);
    }

    public async Task StoreTokensAsync(
        Guid userId,
        string accessToken,
        string refreshToken,
        DateTime tokenExpiry,
        CancellationToken cancellationToken = default)
    {
        var encryptedAccess = _protector.Protect(accessToken);
        var encryptedRefresh = _protector.Protect(refreshToken);

        var existing = await _db.CalendarSyncs
            .FirstOrDefaultAsync(
                c => c.UserId == userId && c.Provider == CalendarProvider.Google,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            _db.CalendarSyncs.Add(new CalendarSync
            {
                SyncId = Guid.NewGuid(),
                UserId = userId,
                Provider = CalendarProvider.Google,
                AccessToken = encryptedAccess,
                RefreshToken = encryptedRefresh,
                TokenExpiry = tokenExpiry,
                IsActive = true,
            });
        }
        else
        {
            existing.AccessToken = encryptedAccess;
            existing.RefreshToken = encryptedRefresh;
            existing.TokenExpiry = tokenExpiry;
            existing.IsActive = true;
            existing.FailedLoginAttempts = 0;
            existing.LockedUntilUtc = null;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<CalendarTokenResult?> GetActiveTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var sync = await _db.CalendarSyncs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.UserId == userId && c.Provider == CalendarProvider.Google && c.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        if (sync is null)
            return null;

        var accessToken = _protector.Unprotect(sync.AccessToken);
        var refreshToken = _protector.Unprotect(sync.RefreshToken);

        return new CalendarTokenResult(accessToken, refreshToken, sync.TokenExpiry);
    }

    public async Task UpdateAccessTokenAsync(
        Guid userId,
        string newAccessToken,
        DateTime newExpiry,
        CancellationToken cancellationToken = default)
    {
        var sync = await _db.CalendarSyncs
            .FirstOrDefaultAsync(
                c => c.UserId == userId && c.Provider == CalendarProvider.Google && c.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        if (sync is null)
            return;

        sync.AccessToken = _protector.Protect(newAccessToken);
        sync.TokenExpiry = newExpiry;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkDisconnectedAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var sync = await _db.CalendarSyncs
            .FirstOrDefaultAsync(
                c => c.UserId == userId && c.Provider == CalendarProvider.Google,
                cancellationToken)
            .ConfigureAwait(false);

        if (sync is null)
            return;

        sync.IsActive = false;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
