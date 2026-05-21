using System.Text.Json;
using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Permanently removes a single clinical document and all dependent data (US_034).
///
/// Deletion sequence (all within a serialisable-isolation transaction):
/// 1. Authorise — document must belong to the requesting patient.
/// 2. Cancel NER processing when the document is in Scanning or Processing state (edge case).
/// 3. Auto-resolve open DataConflict entries that reference the document (US_034 edge case).
/// 4. Bulk-delete MedicalCodeMappings sourced from this document's ExtractedDataRecords.
/// 5. Bulk-delete DataConflicts referencing this document.
/// 6. Bulk-delete ExtractedDataRecords for this document.
/// 7. Delete the physical file from storage.
/// 8. Delete the ClinicalDocument record.
/// 9. Append an immutable AuditLog entry — no document content is stored (AC-05).
/// 10. Enqueue PatientView re-aggregation via Hangfire (AC-02).
/// </summary>
public sealed class DocumentDeletionService : IDocumentDeletionService
{
    private readonly ApplicationDbContext _db;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<DocumentDeletionService> _logger;

    public DocumentDeletionService(
        ApplicationDbContext db,
        IBackgroundJobClient jobClient,
        ILogger<DocumentDeletionService> logger)
    {
        _db = db;
        _jobClient = jobClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DeleteDocumentResult> DeleteAsync(
        DeleteDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        // -------------------------------------------------------------------
        // 1. Authorise: resolve patient profile and verify ownership
        // -------------------------------------------------------------------
        var profile = await _db.PatientProfiles
            .AsNoTracking()
            .Select(p => new { p.PatientProfileId, p.UserId })
            .FirstOrDefaultAsync(p => p.UserId == request.RequestingPatientUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            _logger.LogWarning(
                "DocumentDeletion: PatientProfile not found for UserId {UserId}.",
                request.RequestingPatientUserId);

            return new DeleteDocumentResult(
                Success: false,
                FailureCode: "PATIENT_NOT_FOUND",
                FailureReason: "Patient profile not found.");
        }

        var executionStrategy = _db.Database.CreateExecutionStrategy();
        DeleteDocumentResult serviceResult = new(false, "INTERNAL_ERROR", "An unexpected error occurred.");

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            // ---------------------------------------------------------------
            // 2. Load and lock document row; verify patient ownership
            // ---------------------------------------------------------------
            var document = await _db.ClinicalDocuments
                .FirstOrDefaultAsync(
                    d => d.DocumentId == request.DocumentId
                      && d.PatientProfileId == profile.PatientProfileId,
                    cancellationToken)
                .ConfigureAwait(false);

            if (document is null)
            {
                _logger.LogWarning(
                    "DocumentDeletion: DocumentId {DocumentId} not found for PatientProfileId {PatientProfileId}.",
                    request.DocumentId, profile.PatientProfileId);

                serviceResult = new DeleteDocumentResult(
                    Success: false,
                    FailureCode: "NOT_FOUND",
                    FailureReason: "Document not found or does not belong to this patient.");

                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            // ---------------------------------------------------------------
            // Edge case: cancel active NER processing before deleting
            // Mark as Failed so the processing pipeline skips it.
            // ---------------------------------------------------------------
            if (document.ProcessingStatus is DocumentProcessingStatus.Scanning
                                          or DocumentProcessingStatus.Processing)
            {
                _logger.LogInformation(
                    "DocumentDeletion: DocumentId {DocumentId} is in {Status} — marking Failed before deletion.",
                    document.DocumentId, document.ProcessingStatus);

                document.ProcessingStatus = DocumentProcessingStatus.Failed;
                // SaveChanges here updates the DB row so the Hangfire job
                // can detect Failed status and abort processing.
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            // ---------------------------------------------------------------
            // 3. Auto-resolve open conflicts that reference this document
            //    (US_034 edge case: staff notification is deferred to the job)
            // ---------------------------------------------------------------
            await _db.DataConflicts
                .Where(c => c.PatientProfileId == profile.PatientProfileId
                         && c.ResolutionStatus == ResolutionStatus.Open
                         && (c.SourceDocumentId1 == document.DocumentId
                          || c.SourceDocumentId2 == document.DocumentId))
                .ExecuteUpdateAsync(
                    s => s.SetProperty(c => c.ResolutionStatus, ResolutionStatus.Resolved)
                          .SetProperty(c => c.ResolutionNotes, "Auto-resolved: source document deleted by patient."),
                    cancellationToken)
                .ConfigureAwait(false);

            // ---------------------------------------------------------------
            // 4. Bulk-delete MedicalCodeMappings from this document's records
            // ---------------------------------------------------------------
            var extractedRecordIds = await _db.ExtractedDataRecords
                .AsNoTracking()
                .Where(r => r.DocumentId == document.DocumentId)
                .Select(r => r.RecordId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (extractedRecordIds.Count > 0)
            {
                await _db.MedicalCodeMappings
                    .Where(m => extractedRecordIds.Contains(m.ExtractedRecordId))
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            // ---------------------------------------------------------------
            // 5. Bulk-delete DataConflicts referencing this document
            // ---------------------------------------------------------------
            await _db.DataConflicts
                .Where(c => c.SourceDocumentId1 == document.DocumentId
                         || c.SourceDocumentId2 == document.DocumentId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            // ---------------------------------------------------------------
            // 6. Bulk-delete ExtractedDataRecords
            // ---------------------------------------------------------------
            await _db.ExtractedDataRecords
                .Where(r => r.DocumentId == document.DocumentId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            // ---------------------------------------------------------------
            // 7. Delete physical file from storage (best-effort; log on failure)
            // ---------------------------------------------------------------
            var storagePath = document.StoragePath;
            TryDeleteFile(storagePath, document.DocumentId);

            // ---------------------------------------------------------------
            // 8. Delete the ClinicalDocument record
            // ---------------------------------------------------------------
            _db.ClinicalDocuments.Remove(document);

            // ---------------------------------------------------------------
            // 9. Append immutable audit log — no document content stored (AC-05)
            // ---------------------------------------------------------------
            _db.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                ActorUserId = request.RequestingPatientUserId,
                ActorRole = "patient",
                ActionType = "DocumentDeleted",
                ResourceType = "ClinicalDocument",
                ResourceId = document.DocumentId.ToString(),
                Details = JsonSerializer.Serialize(new
                {
                    FileName = document.FileName,
                    FileFormat = document.FileFormat.ToString(),
                    PatientProfileId = profile.PatientProfileId,
                }),
                IpAddress = request.IpAddress,
            });

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

            // ---------------------------------------------------------------
            // 10. Enqueue PatientView re-aggregation (AC-02)
            //     Runs outside the transaction so the job sees committed data.
            // ---------------------------------------------------------------
            _jobClient.Enqueue<PatientViewAggregationJob>(
                j => j.ExecuteAsync(profile.PatientProfileId));

            _logger.LogInformation(
                "DocumentDeletion: DocumentId {DocumentId} permanently deleted for PatientProfileId {PatientProfileId}.",
                document.DocumentId, profile.PatientProfileId);

            serviceResult = new DeleteDocumentResult(Success: true, FailureCode: null, FailureReason: null);
        }).ConfigureAwait(false);

        return serviceResult;
    }

    private void TryDeleteFile(string path, Guid documentId)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            // Non-fatal: the record is deleted from DB regardless.
            // The retention cleanup job (RetentionCleanupJob) will collect orphaned files.
            _logger.LogError(ex,
                "DocumentDeletion: failed to delete physical file '{Path}' for DocumentId {DocumentId}.",
                path, documentId);
        }
    }
}

/// <summary>
/// Hangfire background job that triggers PatientView re-aggregation (AC-04).
/// Dispatches <see cref="AggregatePatientDataCommand"/> via MediatR so the full
/// merge, deduplication, conflict-flagging, and verification pipeline runs.
/// </summary>
public sealed class PatientViewAggregationJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<PatientViewAggregationJob> _logger;

    public PatientViewAggregationJob(
        IMediator mediator,
        ILogger<PatientViewAggregationJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid patientProfileId)
    {
        var result = await _mediator
            .Send(new AggregatePatientDataCommand(patientProfileId))
            .ConfigureAwait(false);

        if (!result.Success)
        {
            _logger.LogError(
                "PatientViewAggregationJob: Aggregation failed for PatientProfileId {PatientProfileId}. " +
                "Code={FailureCode} Reason={FailureReason}.",
                patientProfileId, result.FailureCode, result.FailureReason);
        }
    }
}
