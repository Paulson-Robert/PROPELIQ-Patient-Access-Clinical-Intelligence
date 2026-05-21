using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically creates an immutable audit entry for every
/// state-changing command request (AC-01, AC-03). Queries (read-only) are intentionally skipped.
///
/// Detection heuristic: request type name ends with "Command" (project naming convention).
/// This avoids requiring all commands to implement a marker interface, preserving backward
/// compatibility with handlers already in the codebase.
///
/// Failure mode: audit failure is non-blocking — the original operation completes and the
/// error is logged at Critical level for alert/Sentry triage (US_044 edge case).
/// </summary>
public sealed class AuditLoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingBehavior(
        IAuditLogRepository auditLog,
        ICurrentUserContext currentUser,
        ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
    {
        _auditLog = auditLog;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Only audit state-changing commands — skip queries (AC-03).
        if (!requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
            return await next().ConfigureAwait(false);

        var response = await next().ConfigureAwait(false);

        // Build audit entry after the handler succeeds. Failed commands are not recorded
        // as successful mutations (the exception propagates and is handled by the API layer).
        var entry = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            ActorUserId = _currentUser.ActorUserId,
            ActorRole = _currentUser.ActorRole,
            ActionType = requestName,
            ResourceType = DeriveResourceType(requestName),
            ResourceId = string.Empty, // Commands do not expose a resource ID at pipeline level.
            IpAddress = _currentUser.IpAddress,
        };

        try
        {
            await _auditLog.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Audit failure must never break the originating operation (US_044 edge case).
            _logger.LogCritical(
                ex,
                "AUDIT_FAILURE: Failed to persist audit entry for {CommandName} by actor {ActorUserId}. " +
                "Manual reconciliation required.",
                requestName,
                _currentUser.ActorUserId);
        }

        return response;
    }

    /// <summary>
    /// Derives a resource type label from the command name by stripping the "Command" suffix.
    /// Example: "CreateUserCommand" → "CreateUser". Provides a stable, searchable ActionType value.
    /// </summary>
    private static string DeriveResourceType(string commandName)
    {
        const string suffix = "Command";
        return commandName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? commandName[..^suffix.Length]
            : commandName;
    }
}
