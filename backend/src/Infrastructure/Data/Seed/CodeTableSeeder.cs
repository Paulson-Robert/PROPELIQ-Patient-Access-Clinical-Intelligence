using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seed;

/// <summary>
/// Seeds representative ICD-10-CM and CPT-4 reference codes.
/// Idempotent — skips a version if any codes for it already exist (AC-03, AC-04).
/// </summary>
public static class CodeTableSeeder
{
    private const string Icd10Version = "2025-CM";
    private const string CptVersion = "2025";

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedIcd10CodesAsync(context);
        await SeedCptCodesAsync(context);
    }

    private static async Task SeedIcd10CodesAsync(ApplicationDbContext context)
    {
        bool exists = await context.Icd10Codes
            .AnyAsync(c => c.CodeSetVersion == Icd10Version);

        if (exists)
            return;

        var codes = new List<Icd10Code>
        {
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "Z00.00", Description = "Encounter for general adult medical examination without abnormal findings", Category = "Factors influencing health status", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "I10",    Description = "Essential (primary) hypertension", Category = "Diseases of the circulatory system", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "E11.9",  Description = "Type 2 diabetes mellitus without complications", Category = "Endocrine, nutritional and metabolic diseases", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "J18.9",  Description = "Unspecified pneumonia", Category = "Diseases of the respiratory system", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "M54.5",  Description = "Low back pain", Category = "Diseases of the musculoskeletal system", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "F32.9",  Description = "Major depressive disorder, single episode, unspecified", Category = "Mental and behavioural disorders", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "J06.9",  Description = "Acute upper respiratory infection, unspecified", Category = "Diseases of the respiratory system", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "K21.0",  Description = "Gastro-esophageal reflux disease with esophagitis", Category = "Diseases of the digestive system", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "N39.0",  Description = "Urinary tract infection, site not specified", Category = "Diseases of the genitourinary system", CodeSetVersion = Icd10Version },
            new() { Icd10CodeId = Guid.NewGuid(), CodeValue = "Z23",    Description = "Encounter for immunization", Category = "Factors influencing health status", CodeSetVersion = Icd10Version },
        };

        context.Icd10Codes.AddRange(codes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCptCodesAsync(ApplicationDbContext context)
    {
        bool exists = await context.CptCodes
            .AnyAsync(c => c.CodeSetVersion == CptVersion);

        if (exists)
            return;

        var codes = new List<CptCode>
        {
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "99213", Description = "Office or other outpatient visit, established patient, low complexity", Category = "Evaluation and Management", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "99214", Description = "Office or other outpatient visit, established patient, moderate complexity", Category = "Evaluation and Management", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "99203", Description = "Office or other outpatient visit, new patient, low complexity", Category = "Evaluation and Management", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "93000", Description = "Electrocardiogram, routine ECG with at least 12 leads", Category = "Cardiovascular", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "71046", Description = "Radiologic examination, chest, 2 views", Category = "Radiology", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "85025", Description = "Complete blood count (CBC) with automated differential WBC count", Category = "Pathology and Laboratory", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "80053", Description = "Comprehensive metabolic panel", Category = "Pathology and Laboratory", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "36415", Description = "Collection of venous blood by venipuncture", Category = "Surgery", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "90471", Description = "Immunization administration; first or only component", Category = "Medicine", CodeSetVersion = CptVersion },
            new() { CptCodeId = Guid.NewGuid(), CodeValue = "99232", Description = "Subsequent hospital care, per day, moderate complexity", Category = "Evaluation and Management", CodeSetVersion = CptVersion },
        };

        context.CptCodes.AddRange(codes);
        await context.SaveChangesAsync();
    }
}
