using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Security.Tests;

// ---------------------------------------------------------------------------
// ConflictDetectionServiceTests — AC-01 detection + edge case re-open
// ---------------------------------------------------------------------------

public sealed class ConflictDetectionServiceTests
{
    // -----------------------------------------------------------------------
    // GetOpenConflictsAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetOpenConflictsAsync_ReturnsOnlyOpenConflicts()
    {
        var context = CreateContext("conflict-get-open");
        var patientProfileId = Guid.NewGuid();

        SeedConflict(context, patientProfileId, "blood_pressure", ResolutionStatus.Open);
        SeedConflict(context, patientProfileId, "primary_phone", ResolutionStatus.Resolved);
        await context.SaveChangesAsync();

        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.GetOpenConflictsAsync(patientProfileId, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("blood_pressure", result[0].FieldName);
        Assert.Equal(ResolutionStatus.Open, result[0].ResolutionStatus);
    }

    [Fact]
    public async Task GetOpenConflictsAsync_NoOpenConflicts_ReturnsEmpty()
    {
        var context = CreateContext("conflict-get-open-empty");
        var patientProfileId = Guid.NewGuid();

        SeedConflict(context, patientProfileId, "blood_pressure", ResolutionStatus.Resolved);
        await context.SaveChangesAsync();

        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.GetOpenConflictsAsync(patientProfileId, CancellationToken.None);

        Assert.Empty(result);
    }

    // -----------------------------------------------------------------------
    // DetectAndPersistAsync — AC-01: new conflicts inserted
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DetectAndPersistAsync_InsertsNewConflictsAsOpen()
    {
        var context = CreateContext("conflict-detect-insert");
        var patientProfileId = Guid.NewGuid();
        var docId1 = Guid.NewGuid();
        var docId2 = Guid.NewGuid();

        SeedExtractedRecord(context, patientProfileId, docId1, "blood_pressure", "142/88 mmHg");
        SeedExtractedRecord(context, patientProfileId, docId2, "blood_pressure", "138/85 mmHg");
        await context.SaveChangesAsync();

        var dedup = new StubDeduplicationService(new[]
        {
            new DetectedConflict(
                ExtractedDataType.Vital, "blood_pressure",
                "142/88 mmHg", docId1, "138/85 mmHg", docId2),
        });
        var service = BuildService(context, dedup, new StubStaffNotificationService());

        var result = await service.DetectAndPersistAsync(patientProfileId, CancellationToken.None);

        Assert.Equal(1, result.NewConflictsInserted);
        Assert.Equal(0, result.ResolvedConflictsReopened);

        var saved = await context.DataConflicts.SingleAsync();
        Assert.Equal(patientProfileId, saved.PatientProfileId);
        Assert.Equal("blood_pressure", saved.FieldName);
        Assert.Equal(ResolutionStatus.Open, saved.ResolutionStatus);
    }

    [Fact]
    public async Task DetectAndPersistAsync_AlreadyOpenConflict_IsNotDuplicated()
    {
        var context = CreateContext("conflict-detect-skip-open");
        var patientProfileId = Guid.NewGuid();
        var docId1 = Guid.NewGuid();
        var docId2 = Guid.NewGuid();

        SeedConflict(context, patientProfileId, "blood_pressure", ResolutionStatus.Open);
        SeedExtractedRecord(context, patientProfileId, docId1, "blood_pressure", "142/88 mmHg");
        SeedExtractedRecord(context, patientProfileId, docId2, "blood_pressure", "138/85 mmHg");
        await context.SaveChangesAsync();

        var dedup = new StubDeduplicationService(new[]
        {
            new DetectedConflict(
                ExtractedDataType.Vital, "blood_pressure",
                "142/88 mmHg", docId1, "138/85 mmHg", docId2),
        });
        var service = BuildService(context, dedup, new StubStaffNotificationService());

        var result = await service.DetectAndPersistAsync(patientProfileId, CancellationToken.None);

        Assert.Equal(0, result.NewConflictsInserted);
        Assert.Equal(1, await context.DataConflicts.CountAsync()); // still just one row
    }

    // -----------------------------------------------------------------------
    // DetectAndPersistAsync — Edge case: re-open resolved + notify staff
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DetectAndPersistAsync_ReopensResolvedConflictAndNotifiesStaff()
    {
        var context = CreateContext("conflict-detect-reopen");
        var patientProfileId = Guid.NewGuid();
        var docId1 = Guid.NewGuid();
        var docId2 = Guid.NewGuid();
        var staffUserId = Guid.NewGuid();

        // Seed a previously resolved conflict
        SeedConflict(context, patientProfileId, "blood_pressure", ResolutionStatus.Resolved);

        // Seed an active staff user to receive the notification
        context.Users.Add(new User
        {
            UserId = staffUserId,
            Email = "staff@example.com",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Staff,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        SeedExtractedRecord(context, patientProfileId, docId1, "blood_pressure", "142/88 mmHg");
        SeedExtractedRecord(context, patientProfileId, docId2, "blood_pressure", "145/90 mmHg");
        await context.SaveChangesAsync();

        var dedup = new StubDeduplicationService(new[]
        {
            new DetectedConflict(
                ExtractedDataType.Vital, "blood_pressure",
                "142/88 mmHg", docId1, "145/90 mmHg", docId2),
        });
        var notifSvc = new StubStaffNotificationService();
        var service = BuildService(context, dedup, notifSvc);

        var result = await service.DetectAndPersistAsync(patientProfileId, CancellationToken.None);

        Assert.Equal(0, result.NewConflictsInserted);
        Assert.Equal(1, result.ResolvedConflictsReopened);

        var reopened = await context.DataConflicts.SingleAsync();
        Assert.Equal(ResolutionStatus.Open, reopened.ResolutionStatus);
        Assert.Null(reopened.ResolvedByUserId);
        Assert.Null(reopened.ResolvedAt);

        // Staff were notified
        Assert.Single(notifSvc.Requests);
        Assert.Contains(staffUserId, notifSvc.Requests[0].StaffUserIds);
        Assert.Equal("Warning", notifSvc.Requests[0].Variant);
    }

    [Fact]
    public async Task DetectAndPersistAsync_NoExtractedRecords_ReturnsZeroCounts()
    {
        var context = CreateContext("conflict-detect-no-records");
        var patientProfileId = Guid.NewGuid();

        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.DetectAndPersistAsync(patientProfileId, CancellationToken.None);

        Assert.Equal(0, result.NewConflictsInserted);
        Assert.Equal(0, result.ResolvedConflictsReopened);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ConflictDetectionService BuildService(
        ApplicationDbContext context,
        IDataDeduplicationService dedup,
        IStaffNotificationService notif) =>
        new(context, dedup, notif, NullLogger<ConflictDetectionService>.Instance);

    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options, null);
    }

    private static void SeedConflict(
        ApplicationDbContext context,
        Guid patientProfileId,
        string fieldName,
        ResolutionStatus status)
    {
        context.DataConflicts.Add(new DataConflict
        {
            ConflictId = Guid.NewGuid(),
            PatientProfileId = patientProfileId,
            ConflictType = ConflictType.Diagnosis,
            FieldName = fieldName,
            Value1 = "A",
            SourceDocumentId1 = Guid.NewGuid(),
            Value2 = "B",
            SourceDocumentId2 = Guid.NewGuid(),
            ResolutionStatus = status,
        });
    }

    private static void SeedExtractedRecord(
        ApplicationDbContext context,
        Guid patientProfileId,
        Guid documentId,
        string fieldName,
        string fieldValue)
    {
        context.ExtractedDataRecords.Add(new ExtractedDataRecord
        {
            RecordId = Guid.NewGuid(),
            DocumentId = documentId,
            PatientProfileId = patientProfileId,
            DataType = ExtractedDataType.Vital,
            FieldName = fieldName,
            FieldValue = fieldValue,
            Confidence = 0.9m,
            IsVerified = false,
        });
    }
}

// ---------------------------------------------------------------------------
// ConflictResolutionServiceTests — AC-02 persistence + AC-03 audit trail
// ---------------------------------------------------------------------------

public sealed class ConflictResolutionServiceTests
{
    [Fact]
    public async Task ResolveAsync_ResolvesConflict_AndWritesAuditLogEntry()
    {
        var context = CreateContext("resolve-happy-path");
        var conflictId = Guid.NewGuid();
        var staffUserId = Guid.NewGuid();

        context.DataConflicts.Add(new DataConflict
        {
            ConflictId = conflictId,
            PatientProfileId = Guid.NewGuid(),
            ConflictType = ConflictType.Diagnosis,
            FieldName = "blood_pressure",
            Value1 = "142/88 mmHg",
            SourceDocumentId1 = Guid.NewGuid(),
            Value2 = "138/85 mmHg",
            SourceDocumentId2 = Guid.NewGuid(),
            ResolutionStatus = ResolutionStatus.Open,
        });
        await context.SaveChangesAsync();

        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.ResolveAsync(
            new ResolveConflictRequest(
                ConflictId: conflictId,
                ResolvedByUserId: staffUserId,
                ResolvedByRole: "Staff",
                AcceptedValue: "142/88 mmHg",
                Notes: "EHR value confirmed"),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.FailureCode);

        // AC-02: conflict row updated
        var conflict = await context.DataConflicts.SingleAsync();
        Assert.Equal(ResolutionStatus.Resolved, conflict.ResolutionStatus);
        Assert.Equal(staffUserId, conflict.ResolvedByUserId);
        Assert.NotNull(conflict.ResolvedAt);
        Assert.Contains("142/88 mmHg", conflict.ResolutionNotes);

        // AC-03: audit log appended
        var auditEntry = await context.AuditLogs.SingleAsync();
        Assert.Equal("CONFLICT_RESOLVED", auditEntry.ActionType);
        Assert.Equal("DataConflict", auditEntry.ResourceType);
        Assert.Equal(conflictId.ToString(), auditEntry.ResourceId);
        Assert.Equal(staffUserId, auditEntry.ActorUserId);
        Assert.Contains("blood_pressure", auditEntry.Details);
        Assert.Contains("142/88 mmHg", auditEntry.Details);
    }

    [Fact]
    public async Task ResolveAsync_ConflictNotFound_ReturnsFailure()
    {
        var context = CreateContext("resolve-not-found");
        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.ResolveAsync(
            new ResolveConflictRequest(
                ConflictId: Guid.NewGuid(),
                ResolvedByUserId: Guid.NewGuid(),
                ResolvedByRole: "Staff",
                AcceptedValue: "some value"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CONFLICT_NOT_FOUND", result.FailureCode);
    }

    [Fact]
    public async Task ResolveAsync_AlreadyResolvedConflict_ReturnsFailure()
    {
        var context = CreateContext("resolve-already-resolved");
        var conflictId = Guid.NewGuid();

        context.DataConflicts.Add(new DataConflict
        {
            ConflictId = conflictId,
            PatientProfileId = Guid.NewGuid(),
            ConflictType = ConflictType.Diagnosis,
            FieldName = "blood_pressure",
            Value1 = "A",
            SourceDocumentId1 = Guid.NewGuid(),
            Value2 = "B",
            SourceDocumentId2 = Guid.NewGuid(),
            ResolutionStatus = ResolutionStatus.Resolved,
            ResolvedByUserId = Guid.NewGuid(),
            ResolvedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.ResolveAsync(
            new ResolveConflictRequest(
                ConflictId: conflictId,
                ResolvedByUserId: Guid.NewGuid(),
                ResolvedByRole: "Staff",
                AcceptedValue: "A"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CONFLICT_ALREADY_RESOLVED", result.FailureCode);

        // Audit log must NOT be written for already-resolved conflicts
        Assert.Empty(context.AuditLogs);
    }

    [Fact]
    public async Task ResolveAsync_EmptyConflictId_ReturnsInvalidRequest()
    {
        var context = CreateContext("resolve-empty-id");
        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.ResolveAsync(
            new ResolveConflictRequest(
                ConflictId: Guid.Empty,
                ResolvedByUserId: Guid.NewGuid(),
                ResolvedByRole: "Staff",
                AcceptedValue: "value"),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_REQUEST", result.FailureCode);
    }

    [Fact]
    public async Task ResolveAsync_EmptyAcceptedValue_ReturnsInvalidRequest()
    {
        var context = CreateContext("resolve-empty-value");
        var service = BuildService(context, new StubDeduplicationService(), new StubStaffNotificationService());

        var result = await service.ResolveAsync(
            new ResolveConflictRequest(
                ConflictId: Guid.NewGuid(),
                ResolvedByUserId: Guid.NewGuid(),
                ResolvedByRole: "Staff",
                AcceptedValue: "   "),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("INVALID_REQUEST", result.FailureCode);
    }

    // -----------------------------------------------------------------------
    // Helper — reuses the same factory as ConflictDetectionServiceTests
    // -----------------------------------------------------------------------

    private static ConflictDetectionService BuildService(
        ApplicationDbContext context,
        IDataDeduplicationService dedup,
        IStaffNotificationService notif) =>
        new(context, dedup, notif, NullLogger<ConflictDetectionService>.Instance);

    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ApplicationDbContext(options, null);
    }
}

// ---------------------------------------------------------------------------
// Stubs — minimal test doubles; no mocking library required
// ---------------------------------------------------------------------------

/// <summary>Returns a fixed list of conflicts regardless of input records.</summary>
file sealed class StubDeduplicationService : IDataDeduplicationService
{
    private readonly IReadOnlyList<DetectedConflict> _conflicts;

    public StubDeduplicationService(IEnumerable<DetectedConflict>? conflicts = null)
        => _conflicts = (conflicts ?? []).ToList().AsReadOnly();

    public Task<DeduplicationResult> DeduplicateAsync(
        IReadOnlyList<ExtractedDataRecord> records,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new DeduplicationResult(Array.Empty<CanonicalRecord>(), _conflicts));
}

/// <summary>Captures notification requests for assertion.</summary>
file sealed class StubStaffNotificationService : IStaffNotificationService
{
    public List<CreateStaffNotificationRequest> Requests { get; } = [];

    public Task<IReadOnlyList<StaffNotificationDto>> CreateAsync(
        CreateStaffNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        Requests.Add(request);
        return Task.FromResult<IReadOnlyList<StaffNotificationDto>>([]);
    }

    public Task<NotificationPageDto> GetHistoryAsync(
        Guid staffUserId, int page, int pageSize, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<StaffNotificationDto> MarkReadAsync(
        Guid staffNotificationId, Guid staffUserId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
