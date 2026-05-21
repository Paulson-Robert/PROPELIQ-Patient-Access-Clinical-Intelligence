using Application.Commands;
using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Handlers;

/// <summary>
/// Persists the ClamAV scan outcome for a <c>ClinicalDocument</c> (US_033).
///
/// - Clean  → advances pipeline to <see cref="DocumentProcessingStatus.Processing"/> (AC-04).
/// - Infected → quarantines by deleting the physical file and marking <see cref="DocumentProcessingStatus.Failed"/> (AC-02).
/// - Unavailable / Error → document stays in <see cref="MalwareScanStatus.Pending"/> for retry.
/// </summary>
internal sealed class ProcessScanResultCommandHandler
    : IRequestHandler<ProcessScanResultCommand, ProcessScanResultResult>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProcessScanResultCommandHandler> _logger;

    public ProcessScanResultCommandHandler(
        ApplicationDbContext db,
        ILogger<ProcessScanResultCommandHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ProcessScanResultResult> Handle(
        ProcessScanResultCommand request,
        CancellationToken cancellationToken)
    {
        var document = await _db.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId, cancellationToken)
            .ConfigureAwait(false);

        if (document is null)
        {
            _logger.LogError(
                "ProcessScanResult: DocumentId {DocumentId} not found.",
                request.DocumentId);
            return new ProcessScanResultResult(false, "Document not found.");
        }

        switch (request.Outcome)
        {
            case MalwareScanOutcome.Clean:
                document.MalwareScanStatus = MalwareScanStatus.Clean;
                document.ProcessingStatus = DocumentProcessingStatus.Processing;

                _logger.LogInformation(
                    "ProcessScanResult: DocumentId {DocumentId} is clean — queued for NER pipeline.",
                    document.DocumentId);
                break;

            case MalwareScanOutcome.Infected:
                document.MalwareScanStatus = MalwareScanStatus.Infected;
                document.ProcessingStatus = DocumentProcessingStatus.Failed;

                // AC-02: quarantine — remove the physical file so it is never accessible.
                QuarantineFile(document.StoragePath, document.DocumentId);

                _logger.LogWarning(
                    "ProcessScanResult: DocumentId {DocumentId} is INFECTED — file quarantined.",
                    document.DocumentId);
                break;

            default:
                // Unavailable or Error: leave MalwareScanStatus = Pending so the next
                // job run retries without losing state (edge case: ClamAV unavailable).
                _logger.LogWarning(
                    "ProcessScanResult: DocumentId {DocumentId} — scan outcome {Outcome}; document left in Pending state.",
                    document.DocumentId, request.Outcome);
                return new ProcessScanResultResult(true, null);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ProcessScanResultResult(true, null);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private void QuarantineFile(string storagePath, Guid documentId)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            return;

        try
        {
            if (File.Exists(storagePath))
                File.Delete(storagePath);
        }
        catch (Exception ex)
        {
            // Log but do not throw — the DB record is already marked Infected/Failed.
            // Manual clean-up can be performed by an operator if the delete fails.
            _logger.LogError(ex,
                "ProcessScanResult: failed to delete quarantined file '{Path}' for DocumentId {DocumentId}.",
                storagePath, documentId);
        }
    }
}
