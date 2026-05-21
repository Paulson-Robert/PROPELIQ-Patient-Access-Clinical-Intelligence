using Application.Interfaces;
using MediatR;

namespace Application.Commands;

// ---------------------------------------------------------------------------
// Command
// ---------------------------------------------------------------------------

/// <summary>
/// Persists a staff member's code verification decision for a suggested ICD-10/CPT mapping (US_040).
///
/// AC-03: Staff can verify, modify, or reject a suggested code.
/// AC-04: Any modification is logged with the supplied reason to the immutable audit trail.
///
/// The handler delegates all persistence and audit log writes to
/// <see cref="ICodeMappingService"/> so the Application layer remains
/// free of Infrastructure and EF Core dependencies.
/// </summary>
public sealed record VerifyCodeMappingCommand(
    Guid MappingId,
    Guid StaffUserId,
    string StaffRole,
    VerifyCodeAction Action,
    /// <summary>Replacement code value — required when <see cref="Action"/> is Modify.</summary>
    string? ModifiedCodeValue = null,
    /// <summary>Mandatory for Modify and Reject — persisted in the audit trail (AC-04).</summary>
    string? Reason = null) : IRequest<VerifyCodeMappingResult>;

// ---------------------------------------------------------------------------
// Handler
// ---------------------------------------------------------------------------

internal sealed class VerifyCodeMappingCommandHandler
    : IRequestHandler<VerifyCodeMappingCommand, VerifyCodeMappingResult>
{
    private readonly ICodeMappingService _codeMapping;

    public VerifyCodeMappingCommandHandler(ICodeMappingService codeMapping)
    {
        _codeMapping = codeMapping;
    }

    public async Task<VerifyCodeMappingResult> Handle(
        VerifyCodeMappingCommand request,
        CancellationToken cancellationToken)
    {
        return await _codeMapping
            .VerifyAsync(
                new VerifyCodeMappingRequest(
                    request.MappingId,
                    request.StaffUserId,
                    request.StaffRole,
                    request.Action,
                    request.ModifiedCodeValue,
                    request.Reason),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
