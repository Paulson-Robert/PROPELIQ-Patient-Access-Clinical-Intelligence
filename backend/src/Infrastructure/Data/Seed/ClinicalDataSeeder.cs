using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seed;

public static class ClinicalDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var patientProfileId = await context.PatientProfiles
            .Where(p => p.User.Email == SeedDataConstants.PatientEmail)
            .Select(p => p.PatientProfileId)
            .SingleAsync();

        var documentId = await SeedDocumentAsync(context, patientProfileId);
        await SeedExtractedRecordsAsync(context, patientProfileId, documentId);
    }

    private static async Task<Guid> SeedDocumentAsync(ApplicationDbContext context, Guid patientProfileId)
    {
        var existingDocumentId = await context.ClinicalDocuments
            .Where(d => d.DocumentId == SeedDataConstants.ClinicalDocumentId
                        || (d.PatientProfileId == patientProfileId
                            && d.FileName == "seed-labs-2026-01.pdf"))
            .Select(d => d.DocumentId)
            .FirstOrDefaultAsync();

        if (existingDocumentId != Guid.Empty)
            return existingDocumentId;

        var document = new ClinicalDocument
        {
            DocumentId = SeedDataConstants.ClinicalDocumentId,
            PatientProfileId = patientProfileId,
            FileName = "seed-labs-2026-01.pdf",
            FileFormat = DocumentFormat.Pdf,
            FileSizeBytes = 245_760,
            StoragePath = "seed/patient-records/seed-labs-2026-01.pdf",
            MalwareScanStatus = MalwareScanStatus.Clean,
            ProcessingStatus = DocumentProcessingStatus.Completed,
            UploadedAt = SeedDataConstants.SeedCreatedAtUtc,
            ScanningStartedAt = SeedDataConstants.SeedCreatedAtUtc.AddMinutes(1),
            ProcessingStartedAt = SeedDataConstants.SeedCreatedAtUtc.AddMinutes(2),
            ProcessedAt = SeedDataConstants.SeedCreatedAtUtc.AddMinutes(3),
        };

        context.ClinicalDocuments.Add(document);
        await context.SaveChangesAsync();

        return document.DocumentId;
    }

    private static async Task SeedExtractedRecordsAsync(
        ApplicationDbContext context,
        Guid patientProfileId,
        Guid documentId)
    {
        var recordTemplates = new List<(Guid RecordId, ExtractedDataType DataType, string FieldName, string FieldValue, decimal Confidence)>
        {
            (SeedDataConstants.ExtractedRecordOneId, ExtractedDataType.Diagnosis, "primary_diagnosis", "I10", 0.9821m),
            (SeedDataConstants.ExtractedRecordTwoId, ExtractedDataType.Medication, "current_medication", "Lisinopril 10 mg daily", 0.9634m),
            (SeedDataConstants.ExtractedRecordThreeId, ExtractedDataType.Allergy, "allergy", "Penicillin", 0.9510m),
        };

        var recordsToInsert = new List<ExtractedDataRecord>();

        foreach (var template in recordTemplates)
        {
            bool recordExists = await context.ExtractedDataRecords.AnyAsync(r =>
                r.RecordId == template.RecordId
                || (r.DocumentId == documentId
                    && r.DataType == template.DataType
                    && r.FieldName == template.FieldName));

            if (recordExists)
                continue;

            recordsToInsert.Add(new ExtractedDataRecord
            {
                RecordId = template.RecordId,
                DocumentId = documentId,
                PatientProfileId = patientProfileId,
                DataType = template.DataType,
                FieldName = template.FieldName,
                FieldValue = template.FieldValue,
                Confidence = template.Confidence,
                IsVerified = false,
                VerifiedByUserId = null,
                VerifiedAt = null,
                SourceLocation = "page:1",
            });
        }

        if (recordsToInsert.Count == 0)
            return;

        context.ExtractedDataRecords.AddRange(recordsToInsert);
        await context.SaveChangesAsync();
    }
}