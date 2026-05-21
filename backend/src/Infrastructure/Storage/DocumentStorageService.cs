using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Hangfire;
using FileFormat = Domain.Enums.DocumentFormat;
using Infrastructure.Data;
using Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage;

/// <summary>
/// Stores uploaded clinical documents to the local file system and persists
/// their metadata to the database (US_032).
///
/// Security controls applied (OWASP A01 / A05):
/// - Client-supplied Content-Type is treated as advisory only; format is
///   confirmed by file extension to prevent MIME spoofing.
/// - Storage key is a UUID — the original filename is never used in the path
///   to prevent path-traversal attacks.
/// - Files are written outside the web root (configurable <see cref="DocumentStorageOptions.BasePath"/>).
/// - 25 MB ceiling enforced server-side regardless of client declaration (AC-02).
/// </summary>
public sealed class DocumentStorageService : IDocumentStorageService
{
    // AC-02: permitted formats keyed by lower-cased extension → domain enum value.
    private static readonly Dictionary<string, FileFormat> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"]  = FileFormat.Pdf,
            [".docx"] = FileFormat.Docx,
            [".jpg"]  = FileFormat.Jpg,
            [".jpeg"] = FileFormat.Jpg,
            [".png"]  = FileFormat.Png,
            [".dcm"]  = FileFormat.Dicom,
        };

    private readonly ApplicationDbContext _db;
    private readonly DocumentStorageOptions _options;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<DocumentStorageService> _logger;

    public DocumentStorageService(
        ApplicationDbContext db,
        IOptions<DocumentStorageOptions> options,
        IBackgroundJobClient jobClient,
        ILogger<DocumentStorageService> logger)
    {
        _db = db;
        _options = options.Value;
        _jobClient = jobClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<UploadDocumentResult> StoreAsync(
        UploadDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        // -----------------------------------------------------------------------
        // AC-02: server-side format validation (extension-based, MIME not trusted)
        // -----------------------------------------------------------------------
        var extension = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.TryGetValue(extension, out var documentFormat))
        {
            _logger.LogWarning(
                "DocumentStorage: rejected file '{FileName}' — unsupported extension '{Ext}'.",
                request.FileName, extension);

            return new UploadDocumentResult(
                Success: false,
                DocumentId: null,
                FailureCode: "UNSUPPORTED_FORMAT",
                FailureReason: $"File format '{extension}' is not permitted. Accepted: PDF, DOCX, JPG, PNG, DICOM.");
        }

        // -----------------------------------------------------------------------
        // AC-02: server-side size validation
        // -----------------------------------------------------------------------
        if (request.FileSizeBytes > _options.MaxFileSizeBytes)
        {
            _logger.LogWarning(
                "DocumentStorage: rejected file '{FileName}' — size {SizeBytes} exceeds limit {LimitBytes}.",
                request.FileName, request.FileSizeBytes, _options.MaxFileSizeBytes);

            return new UploadDocumentResult(
                Success: false,
                DocumentId: null,
                FailureCode: "FILE_TOO_LARGE",
                FailureReason: $"File size exceeds the {_options.MaxFileSizeBytes / (1024 * 1024)} MB limit.");
        }

        // -----------------------------------------------------------------------
        // Resolve PatientProfile from UserId (AC-03: metadata association)
        // -----------------------------------------------------------------------
        var profile = await _db.PatientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.PatientUserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            _logger.LogWarning(
                "DocumentStorage: PatientProfile not found for UserId {UserId}.",
                request.PatientUserId);

            return new UploadDocumentResult(
                Success: false,
                DocumentId: null,
                FailureCode: "PATIENT_NOT_FOUND",
                FailureReason: "Patient profile not found.");
        }

        // -----------------------------------------------------------------------
        // Persist file (OWASP A05: UUID-keyed path, no original filename in path)
        // -----------------------------------------------------------------------
        var storageKey = Guid.NewGuid().ToString("N");
        var storagePath = Path.Combine(_options.BasePath, storageKey + extension);

        try
        {
            Directory.CreateDirectory(_options.BasePath);
            await WriteFileAsync(request.FileStream, storagePath, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Edge Case: storage failure — return error, no partial state (task requirement).
            _logger.LogError(ex,
                "DocumentStorage: failed to write file for UserId {UserId} to '{Path}'.",
                request.PatientUserId, storagePath);

            return new UploadDocumentResult(
                Success: false,
                DocumentId: null,
                FailureCode: "STORAGE_ERROR",
                FailureReason: "File could not be stored. Please try again.");
        }

        // -----------------------------------------------------------------------
        // AC-03: persist metadata — patient, upload date, file type
        // AC-04: set MalwareScanStatus = Pending so the scan pipeline (US_033)
        //        picks up this record and processes it asynchronously.
        // -----------------------------------------------------------------------
        var now = DateTime.UtcNow;
        var document = new ClinicalDocument
        {
            DocumentId = Guid.NewGuid(),
            PatientProfileId = profile.PatientProfileId,
            FileName = SanitizeFileName(request.FileName),
            FileFormat = documentFormat,
            FileSizeBytes = request.FileSizeBytes,
            StoragePath = storagePath,
            MalwareScanStatus = MalwareScanStatus.Pending,  // AC-04: triggers scan pipeline
            ProcessingStatus = DocumentProcessingStatus.Scanning,
            UploadedAt = now,
            ScanningStartedAt = now,
        };

        try
        {
            _db.ClinicalDocuments.Add(document);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Edge Case: metadata persistence failure — attempt to clean up the stored file
            // so there is no orphaned file without a DB record (no partial state).
            _logger.LogError(ex,
                "DocumentStorage: metadata save failed for DocumentId {DocumentId}. Removing orphaned file.",
                document.DocumentId);

            TryDeleteFile(storagePath);

            return new UploadDocumentResult(
                Success: false,
                DocumentId: null,
                FailureCode: "STORAGE_ERROR",
                FailureReason: "Document metadata could not be saved. Please try again.");
        }

        _logger.LogInformation(
            "DocumentStorage: stored DocumentId {DocumentId} for PatientProfileId {PatientProfileId}.",
            document.DocumentId, profile.PatientProfileId);

        // US_033: enqueue malware scan immediately after upload (AC-01).
        _jobClient.Enqueue<MalwareScanJob>(job => job.ExecuteAsync(document.DocumentId));

        _logger.LogInformation(
            "DocumentStorage: stored DocumentId {DocumentId} for PatientProfileId {PatientProfileId}.",
            document.DocumentId, profile.PatientProfileId);

        return new UploadDocumentResult(
            Success: true,
            DocumentId: document.DocumentId,
            FailureCode: null,
            FailureReason: null);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    /// <summary>Writes the stream to <paramref name="destinationPath"/> with a 4 KB buffer.</summary>
    private static async Task WriteFileAsync(
        Stream source,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes path-traversal characters and directory separators from the
    /// client-supplied filename (OWASP A05).
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName); // strip any directory components
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(name.Where(c => !invalidChars.Contains(c)));
    }

    /// <summary>Best-effort deletion of an orphaned file on rollback.</summary>
    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DocumentStorage: could not delete orphaned file '{Path}'.", path);
        }
    }
}
