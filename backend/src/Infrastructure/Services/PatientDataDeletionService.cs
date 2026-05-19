using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Services;

public sealed class PatientDataDeletionService : IPatientDataDeletionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PatientDataDeletionService> _logger;
    private readonly IConnectionMultiplexer? _redis;

    public PatientDataDeletionService(
        ApplicationDbContext dbContext,
        ILogger<PatientDataDeletionService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _redis = redis;
    }

    public async Task<PatientDataDeletionResult> DeletePatientDataAsync(
        DeletePatientDataRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRole = NormalizeActorRole(request.ActorRole);
        var result = new PatientDataDeletionResult(false, CreateEmptyDeletionSummary(), 0);
        Guid? patientUserId = null;

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            var summary = CreateEmptyDeletionSummary();

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            var identity = await _dbContext.PatientProfiles
                .AsNoTracking()
                .Where(p => p.PatientProfileId == request.PatientProfileId)
                .Select(p => new { p.PatientProfileId, p.UserId })
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            patientUserId = identity?.UserId;

            if (identity is not null)
            {
                await _dbContext.Database
                    .ExecuteSqlInterpolatedAsync(
                        $"SELECT 1 FROM \"PatientProfiles\" WHERE \"PatientProfileId\" = {identity.PatientProfileId} FOR UPDATE",
                        cancellationToken)
                    .ConfigureAwait(false);

                summary = await BuildDeletionSummaryAsync(identity.PatientProfileId, identity.UserId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                ActorUserId = request.ActorUserId,
                ActorRole = normalizedRole,
                ActionType = "PATIENT_DATA_DELETION",
                ResourceType = "PatientProfile",
                ResourceId = request.PatientProfileId.ToString(),
                Details = JsonSerializer.Serialize(new
                {
                    PatientProfileId = request.PatientProfileId,
                    Status = identity is null ? "NOT_FOUND" : "DELETION_REQUESTED",
                    DeletedResources = summary
                }),
                IpAddress = request.IpAddress
            };

            await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (identity is null)
            {
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                result = new PatientDataDeletionResult(false, summary, 0);
                return;
            }

            var appointmentIds = await _dbContext.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == identity.UserId)
                .Select(a => a.AppointmentId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["DataConflicts"] = await _dbContext.DataConflicts
                .Where(c => c.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["MedicalCodeMappings"] = await _dbContext.MedicalCodeMappings
                .Where(m => m.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["ExtractedDataRecords"] = await _dbContext.ExtractedDataRecords
                .Where(e => e.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["ClinicalDocuments"] = await _dbContext.ClinicalDocuments
                .Where(d => d.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["IntakeRecords"] = await _dbContext.IntakeRecords
                .Where(i => i.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["PatientViews"] = await _dbContext.PatientViews
                .Where(v => v.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["NoShowRiskFactors"] = await _dbContext.NoShowRiskFactors
                .Where(r => r.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (appointmentIds.Count > 0)
            {
                summary["PreferredSlotQueues"] = await _dbContext.PreferredSlotQueues
                    .Where(q => appointmentIds.Contains(q.AppointmentId))
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);

                summary["Notifications"] = await _dbContext.Notifications
                    .Where(n => n.PatientId == identity.UserId || appointmentIds.Contains(n.AppointmentId))
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                summary["Notifications"] = await _dbContext.Notifications
                    .Where(n => n.PatientId == identity.UserId)
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            summary["Appointments"] = await _dbContext.Appointments
                .Where(a => a.PatientId == identity.UserId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["CalendarSyncs"] = await _dbContext.CalendarSyncs
                .Where(c => c.UserId == identity.UserId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            summary["PatientProfiles"] = await _dbContext.PatientProfiles
                .Where(p => p.PatientProfileId == identity.PatientProfileId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            result = new PatientDataDeletionResult(true, summary, 0);
        }).ConfigureAwait(false);

        if (!result.PatientFound || patientUserId is null)
            return result;

        var deletedCacheKeys = await DeleteRedisPatientKeysAsync(request.PatientProfileId, patientUserId.Value)
            .ConfigureAwait(false);

        return result with { DeletedCacheKeys = deletedCacheKeys };
    }

    private static Dictionary<string, int> CreateEmptyDeletionSummary() =>
        new(StringComparer.Ordinal)
        {
            ["ClinicalDocuments"] = 0,
            ["ExtractedDataRecords"] = 0,
            ["DataConflicts"] = 0,
            ["PatientViews"] = 0,
            ["MedicalCodeMappings"] = 0,
            ["IntakeRecords"] = 0,
            ["Appointments"] = 0,
            ["Notifications"] = 0,
            ["CalendarSyncs"] = 0,
            ["NoShowRiskFactors"] = 0,
            ["PreferredSlotQueues"] = 0,
            ["PatientProfiles"] = 0
        };

    private async Task<Dictionary<string, int>> BuildDeletionSummaryAsync(
        Guid patientProfileId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var appointmentIds = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == userId)
            .Select(a => a.AppointmentId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["ClinicalDocuments"] = await _dbContext.ClinicalDocuments.CountAsync(d => d.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false),
            ["ExtractedDataRecords"] = await _dbContext.ExtractedDataRecords.CountAsync(e => e.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false),
            ["DataConflicts"] = await _dbContext.DataConflicts.CountAsync(c => c.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false),
            ["PatientViews"] = await _dbContext.PatientViews.CountAsync(v => v.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false),
            ["MedicalCodeMappings"] = await _dbContext.MedicalCodeMappings.CountAsync(m => m.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false),
            ["IntakeRecords"] = await _dbContext.IntakeRecords.CountAsync(i => i.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false),
            ["Appointments"] = appointmentIds.Count,
            ["Notifications"] = await _dbContext.Notifications.CountAsync(n => n.PatientId == userId || appointmentIds.Contains(n.AppointmentId), cancellationToken).ConfigureAwait(false),
            ["CalendarSyncs"] = await _dbContext.CalendarSyncs.CountAsync(c => c.UserId == userId, cancellationToken).ConfigureAwait(false),
            ["NoShowRiskFactors"] = await _dbContext.NoShowRiskFactors.CountAsync(r => r.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false),
            ["PreferredSlotQueues"] = appointmentIds.Count == 0
                ? 0
                : await _dbContext.PreferredSlotQueues.CountAsync(q => appointmentIds.Contains(q.AppointmentId), cancellationToken).ConfigureAwait(false),
            ["PatientProfiles"] = await _dbContext.PatientProfiles.CountAsync(p => p.PatientProfileId == patientProfileId, cancellationToken).ConfigureAwait(false)
        };
    }

    private async Task<long> DeleteRedisPatientKeysAsync(Guid patientProfileId, Guid userId)
    {
        if (_redis is null)
            return 0;

        var db = _redis.GetDatabase();
        var databaseNumber = db.Database;
        var totalDeleted = 0L;

        var patterns = new[]
        {
            $"patient:{patientProfileId}:*",
            $"patient-profile:{patientProfileId}:*",
            $"cache:patient:{patientProfileId}:*",
            $"session:patient:{patientProfileId}:*",
            $"user:{userId}:*",
            $"cache:user:{userId}:*",
            $"session:{userId}:*",
            $"session:user:{userId}:*"
        };

        foreach (var endpoint in _redis.GetEndPoints())
        {
            try
            {
                var server = _redis.GetServer(endpoint);
                if (!server.IsConnected || server.IsReplica)
                    continue;

                foreach (var pattern in patterns)
                {
                    var keys = server.Keys(databaseNumber, pattern).ToArray();
                    if (keys.Length == 0)
                        continue;

                    totalDeleted += await db.KeyDeleteAsync(keys).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                _logger.LogWarning(
                    ex,
                    "Redis patient-key cleanup failed for endpoint {Endpoint}. Continuing without cache key deletion.",
                    endpoint);
            }
        }

        return totalDeleted;
    }

    private static string NormalizeActorRole(string actorRole)
    {
        var trimmed = actorRole?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return "SYSTEM";

        return trimmed.Length <= 32
            ? trimmed
            : trimmed[..32];
    }
}