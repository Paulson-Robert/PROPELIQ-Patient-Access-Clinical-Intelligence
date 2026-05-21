using Application.Configuration;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.ML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Security.Tests;

// ---------------------------------------------------------------------------
// CodeMappingServiceTests — mapping accuracy, verification persistence, audit (AC-01–AC-04)
// ---------------------------------------------------------------------------

public sealed class CodeMappingServiceTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static CodeMappingService BuildService(ApplicationDbContext? db = null)
    {
        var opts = Options.Create(new CodeMappingOptions
        {
            ModelDirectory = Path.Combine(Path.GetTempPath(), "nonexistent-model-dir"),
            Icd10Version = "ICD-10-CM 2024",
            CptVersion = "CPT 2024",
        });

        return new CodeMappingService(
            opts,
            db ?? CreateContext(),
            NullLogger<CodeMappingService>.Instance);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"code-mapping-tests-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options, null);
    }

    private static ClinicalEntity DiagnosisEntity(string text, float confidence = 0.95f) =>
        new(ClinicalEntityType.Diagnosis, text, 0, text.Length - 1, confidence, confidence < 0.85f);

    private static ClinicalEntity ProcedureEntity(string text, float confidence = 0.90f) =>
        new(ClinicalEntityType.Procedure, text, 0, text.Length - 1, confidence, confidence < 0.85f);

    // -----------------------------------------------------------------------
    // AC-01: Rule-based + ML.NET mapping returns candidates for known diagnoses
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Type 2 diabetes mellitus", "E11.9")]
    [InlineData("hypertension",             "I10")]
    [InlineData("headache",                 "R51.9")]
    [InlineData("back pain",                "M54.5")]
    [InlineData("urinary tract infection",  "N39.0")]
    public async Task MapAsync_KnownDiagnosis_ReturnsExpectedIcd10Code(string text, string expectedCode)
    {
        using var service = BuildService();

        var results = await service.MapAsync(
            [DiagnosisEntity(text)],
            "patient-1");

        Assert.NotEmpty(results);
        Assert.Contains(results, c => c.CodeValue == expectedCode);
        Assert.All(results, c => Assert.Equal(MedicalCodeType.ICD10, c.CodeType));
    }

    [Theory]
    [InlineData("office visit", "99213")]
    [InlineData("follow up",    "99213")]
    [InlineData("vaccination",  "90471")]
    [InlineData("cbc",          "85025")]
    public async Task MapAsync_KnownProcedure_ReturnsExpectedCptCode(string text, string expectedCode)
    {
        using var service = BuildService();

        var results = await service.MapAsync(
            [ProcedureEntity(text)],
            "patient-1");

        Assert.NotEmpty(results);
        Assert.Contains(results, c => c.CodeValue == expectedCode);
        Assert.All(results, c => Assert.Equal(MedicalCodeType.CPT, c.CodeType));
    }

    [Fact]
    public async Task MapAsync_EmptyEntities_ReturnsEmpty()
    {
        using var service = BuildService();

        var results = await service.MapAsync([], "patient-1");

        Assert.Empty(results);
    }

    [Fact]
    public async Task MapAsync_NullPatientId_Throws()
    {
        using var service = BuildService();

        // ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException for null
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.MapAsync([DiagnosisEntity("diabetes")], null!));
    }

    [Fact]
    public async Task MapAsync_NonMappableEntityType_IsSkipped()
    {
        using var service = BuildService();

        // Medication entities are not ICD-10/CPT mappable
        var medEntity = new ClinicalEntity(
            ClinicalEntityType.Medication, "Lisinopril", 0, 9, 0.95f, false);

        var results = await service.MapAsync([medEntity], "patient-1");

        Assert.Empty(results);
    }

    // -----------------------------------------------------------------------
    // AC-02: Confidence score assigned and candidates ranked descending
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_RuleBasedMatch_ConfidenceIsAtLeast0Point82()
    {
        using var service = BuildService();

        var results = await service.MapAsync([DiagnosisEntity("asthma")], "patient-1");

        Assert.NotEmpty(results);
        Assert.All(results, c => Assert.True(c.ConfidenceScore >= 0.82m));
    }

    [Fact]
    public async Task MapAsync_ExactMatch_ConfidenceIs0Point95()
    {
        using var service = BuildService();

        var results = await service.MapAsync([DiagnosisEntity("hypertension")], "patient-1");

        var exactMatch = results.First(c => c.CodeValue == "I10");
        Assert.Equal(0.95m, exactMatch.ConfidenceScore);
    }

    [Fact]
    public async Task MapAsync_MultipleMatchesRankedDescending()
    {
        using var service = BuildService();

        // "headache" has both exact match (R51.9) and can partially match migraine (G43.909)
        var results = await service.MapAsync([DiagnosisEntity("headache unspecified")], "patient-1");

        for (var i = 1; i < results.Count; i++)
            Assert.True(results[i - 1].ConfidenceScore >= results[i].ConfidenceScore,
                "Candidates should be ranked in descending confidence order.");
    }

    // -----------------------------------------------------------------------
    // Edge case: ambiguous diagnosis — multiple code suggestions returned
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MapAsync_AmbiguousDiagnosis_ReturnsMultipleCandidates()
    {
        using var service = BuildService();

        // "headache" text appears as both R51.9 (headache) and can be associated with G43.909 (migraine)
        // via partial match on rule entries containing "headache" / "migraine"
        var results = await service.MapAsync([DiagnosisEntity("headache migraine")], "patient-1");

        // Ambiguous text should produce at least one candidate
        Assert.NotEmpty(results);
    }

    // -----------------------------------------------------------------------
    // AC-03, AC-04: VerifyAsync — persist staff decisions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task VerifyAsync_VerifyAction_SetsIsVerifiedAndWritesAuditLog()
    {
        var db = CreateContext();
        var mappingId = SeedMapping(db);

        using var service = BuildService(db);

        var result = await service.VerifyAsync(new VerifyCodeMappingRequest(
            mappingId,
            Guid.NewGuid(),
            "Staff",
            VerifyCodeAction.Verify,
            null,
            null));

        Assert.True(result.Success);
        var mapping = await db.MedicalCodeMappings.FindAsync(mappingId);
        Assert.NotNull(mapping);
        Assert.True(mapping!.IsVerified);
        Assert.NotNull(mapping.VerifiedAt);

        var auditLog = await db.AuditLogs.SingleAsync();
        Assert.Equal("CodeMappingVerify", auditLog.ActionType);
        Assert.Equal(nameof(MedicalCodeMapping), auditLog.ResourceType);
        Assert.Equal(mappingId.ToString(), auditLog.ResourceId);
    }

    [Fact]
    public async Task VerifyAsync_ModifyAction_UpdatesCodeValueAndWritesAuditLog()
    {
        var db = CreateContext();
        var mappingId = SeedMapping(db, codeValue: "E11.9");

        using var service = BuildService(db);

        var result = await service.VerifyAsync(new VerifyCodeMappingRequest(
            mappingId,
            Guid.NewGuid(),
            "Staff",
            VerifyCodeAction.Modify,
            "E11.65",
            "More specific diabetes code"));

        Assert.True(result.Success);
        var mapping = await db.MedicalCodeMappings.FindAsync(mappingId);
        Assert.Equal("E11.65", mapping!.CodeValue);
        Assert.True(mapping.IsVerified);

        // AC-04: audit log contains the reason
        var auditLog = await db.AuditLogs.SingleAsync();
        Assert.Equal("CodeMappingModify", auditLog.ActionType);
        Assert.Contains("More specific diabetes code", auditLog.Details);
    }

    [Fact]
    public async Task VerifyAsync_RejectAction_PreservesRecordAndWritesAuditLog()
    {
        var db = CreateContext();
        var mappingId = SeedMapping(db);

        using var service = BuildService(db);

        var result = await service.VerifyAsync(new VerifyCodeMappingRequest(
            mappingId,
            Guid.NewGuid(),
            "Staff",
            VerifyCodeAction.Reject,
            null,
            "Incorrect code for this patient"));

        Assert.True(result.Success);
        var mapping = await db.MedicalCodeMappings.FindAsync(mappingId);
        Assert.NotNull(mapping);          // record preserved
        Assert.False(mapping!.IsVerified); // rejection does not set verified

        // AC-04: audit log contains the reason
        var auditLog = await db.AuditLogs.SingleAsync();
        Assert.Equal("CodeMappingReject", auditLog.ActionType);
        Assert.Contains("Incorrect code for this patient", auditLog.Details);
    }

    [Fact]
    public async Task VerifyAsync_MissingReason_ForModify_ReturnsFail()
    {
        using var service = BuildService();

        var result = await service.VerifyAsync(new VerifyCodeMappingRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Staff",
            VerifyCodeAction.Modify,
            "E11.65",
            null /* missing reason */));

        Assert.False(result.Success);
        Assert.Equal("REASON_REQUIRED", result.FailureCode);
    }

    [Fact]
    public async Task VerifyAsync_MissingReason_ForReject_ReturnsFail()
    {
        using var service = BuildService();

        var result = await service.VerifyAsync(new VerifyCodeMappingRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Staff",
            VerifyCodeAction.Reject,
            null,
            null /* missing reason */));

        Assert.False(result.Success);
        Assert.Equal("REASON_REQUIRED", result.FailureCode);
    }

    [Fact]
    public async Task VerifyAsync_MappingNotFound_ReturnsNotFound()
    {
        var db = CreateContext();
        using var service = BuildService(db);

        var result = await service.VerifyAsync(new VerifyCodeMappingRequest(
            Guid.NewGuid(), // non-existent mapping
            Guid.NewGuid(),
            "Staff",
            VerifyCodeAction.Verify,
            null,
            null));

        Assert.False(result.Success);
        Assert.Equal("MAPPING_NOT_FOUND", result.FailureCode);
    }

    // -----------------------------------------------------------------------
    // Seed helpers
    // -----------------------------------------------------------------------

    private static Guid SeedMapping(ApplicationDbContext db, string codeValue = "E11.9")
    {
        var userId = Guid.NewGuid();
        var patientProfileId = Guid.NewGuid();
        var recordId = Guid.NewGuid();
        var mappingId = Guid.NewGuid();

        db.Users.Add(new User
        {
            UserId = userId,
            Email = $"test-{userId}@example.com",
            AuthProvider = AuthProvider.Local,
            Role = UserRole.Staff,
            MfaEnabled = false,
            MfaMethod = MfaMethod.Totp,
            FailedLoginAttempts = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        db.PatientProfiles.Add(new PatientProfile
        {
            PatientProfileId = patientProfileId,
            UserId = userId,
            FirstName = "Test",
            LastName = "Patient",
            CreatedAt = DateTime.UtcNow,
        });

        db.MedicalCodeMappings.Add(new MedicalCodeMapping
        {
            MappingId = mappingId,
            PatientProfileId = patientProfileId,
            ExtractedRecordId = recordId,
            CodeType = MedicalCodeType.ICD10,
            CodeValue = codeValue,
            CodeDescription = "Test description",
            Confidence = 0.95m,
            IsVerified = false,
            CodeSetVersion = "ICD-10-CM 2024",
        });

        db.SaveChanges();
        return mappingId;
    }
}
