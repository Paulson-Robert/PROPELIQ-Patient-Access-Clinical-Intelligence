using Application.Interfaces;
using MediatR;

namespace Application.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Resolves an open <see cref="Domain.Entities.DataConflict"/> by persisting the
/// staff-selected value and appending an immutable audit trail entry (US_039).
///
/// AC-02: Persists the accepted value against the conflict row.
/// AC-03: Appends an immutable <see cref="Domain.Entities.AuditLog"/> entry capturing
///        who resolved the conflict, when, and which value was accepted.
///
/// The handler delegates all persistence to <see cref="IConflictDetectionService"/>
/// so the Application layer remains free of Infrastructure dependencies.
/// </summary>
public sealed record ResolveConflictCommand(
    Guid ConflictId,
    Guid ResolvedByUserId,
    string ResolvedByRole,
    /// <summary>The value accepted by the staff member (Value1, Value2, or a custom string).</summary>
    string AcceptedValue,
    string? Notes = null) : IRequest<ResolveConflictResult>;

/// <summary>Result returned by <see cref="ResolveConflictCommandHandler"/>.</summary>
public sealed record ResolveConflictResult(
    bool Success,
    string? FailureCode,
    string? FailureReason);

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

internal sealed class ResolveConflictCommandHandler
    : IRequestHandler<ResolveConflictCommand, ResolveConflictResult>
{
    private readonly IConflictDetectionService _conflicts;

    public ResolveConflictCommandHandler(IConflictDetectionService conflicts)
    {
        _conflicts = conflicts;
    }

    public async Task<ResolveConflictResult> Handle(
        ResolveConflictCommand request,
        CancellationToken cancellationToken)
    {
        var serviceResult = await _conflicts
            .ResolveAsync(
                new ResolveConflictRequest(
                    request.ConflictId,
                    request.ResolvedByUserId,
                    request.ResolvedByRole,
                    request.AcceptedValue,
                    request.Notes),
                cancellationToken)
            .ConfigureAwait(false);

        return new ResolveConflictResult(
            serviceResult.Success,
            serviceResult.FailureCode,
            serviceResult.FailureReason);
    }
}
