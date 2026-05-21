using Application.Interfaces;
using Domain.Enums;

namespace Infrastructure.ML;

// ---------------------------------------------------------------------------
// RuleBasedCodeMapper — deterministic ICD-10/CPT lookup table (AC-01, TR-012)
// ---------------------------------------------------------------------------

/// <summary>
/// Stateless, thread-safe code mapper backed by a curated lookup table.
///
/// Lookup strategy (AC-01):
///   1. Exact normalised match → confidence 0.95.
///   2. Substring/partial match against the same table → confidence 0.82.
///   3. No match → returns empty list (caller falls back to ML model).
///
/// Edge case — ambiguous diagnosis: certain terms map to multiple codes.
/// All matching candidates are returned ranked by confidence descending.
/// </summary>
internal sealed class RuleBasedCodeMapper
{
    // Each entry: (normalised key, CodeType, CodeValue, Description, IsAmbiguous partner of another key)
    // Confidence 0.95m for exact match; 0.82m for substring match.
    private static readonly IReadOnlyList<RuleEntry> Rules = BuildRules();

    /// <summary>
    /// Maps <paramref name="text"/> to zero or more code candidates.
    /// Returns candidates in descending confidence order (AC-02).
    /// </summary>
    public IReadOnlyList<CodeMappingCandidate> Map(
        string text,
        string codeSetVersion,
        string cptVersion)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var normalised = Normalise(text);
        var results = new List<CodeMappingCandidate>();

        foreach (var rule in Rules)
        {
            // Exact match — highest confidence
            if (normalised == rule.Key)
            {
                results.Add(new CodeMappingCandidate(
                    text,
                    rule.CodeType,
                    rule.CodeValue,
                    rule.Description,
                    ConfidenceExact,
                    "Rule-based",
                    rule.CodeType == MedicalCodeType.ICD10 ? codeSetVersion : cptVersion));
                continue;
            }

            // Partial match — lower confidence; avoid if already added via exact
            if (!results.Any(r => r.CodeValue == rule.CodeValue) &&
                (normalised.Contains(rule.Key, StringComparison.Ordinal) ||
                 rule.Key.Contains(normalised, StringComparison.Ordinal)))
            {
                results.Add(new CodeMappingCandidate(
                    text,
                    rule.CodeType,
                    rule.CodeValue,
                    rule.Description,
                    ConfidencePartial,
                    "Rule-based",
                    rule.CodeType == MedicalCodeType.ICD10 ? codeSetVersion : cptVersion));
            }
        }

        // AC-02: rank by confidence descending
        results.Sort((a, b) => b.ConfidenceScore.CompareTo(a.ConfidenceScore));
        return results.AsReadOnly();
    }

    // -----------------------------------------------------------------------
    // Constants
    // -----------------------------------------------------------------------

    private const decimal ConfidenceExact = 0.95m;
    private const decimal ConfidencePartial = 0.82m;

    // -----------------------------------------------------------------------
    // Rule table
    // -----------------------------------------------------------------------

    private static List<RuleEntry> BuildRules() =>
    [
        // ICD-10 — common diagnoses
        new("type 2 diabetes",                          MedicalCodeType.ICD10, "E11.9",   "Type 2 diabetes mellitus without complications"),
        new("diabetes mellitus",                        MedicalCodeType.ICD10, "E11.9",   "Type 2 diabetes mellitus without complications"),
        new("diabetes",                                 MedicalCodeType.ICD10, "E11.9",   "Type 2 diabetes mellitus without complications"),
        new("hypertension",                             MedicalCodeType.ICD10, "I10",     "Essential (primary) hypertension"),
        new("high blood pressure",                      MedicalCodeType.ICD10, "I10",     "Essential (primary) hypertension"),
        new("elevated blood pressure",                  MedicalCodeType.ICD10, "I10",     "Essential (primary) hypertension"),
        new("headache",                                 MedicalCodeType.ICD10, "R51.9",   "Headache, unspecified"),
        new("headache unspecified",                     MedicalCodeType.ICD10, "R51.9",   "Headache, unspecified"),
        new("migraine",                                 MedicalCodeType.ICD10, "G43.909", "Migraine, unspecified, not intractable, without status migrainosus"),
        new("chest pain",                               MedicalCodeType.ICD10, "R07.9",   "Chest pain, unspecified"),
        new("upper respiratory infection",              MedicalCodeType.ICD10, "J06.9",   "Acute upper respiratory infection, unspecified"),
        new("upper respiratory tract infection",        MedicalCodeType.ICD10, "J06.9",   "Acute upper respiratory infection, unspecified"),
        new("uri",                                      MedicalCodeType.ICD10, "J06.9",   "Acute upper respiratory infection, unspecified"),
        new("cold",                                     MedicalCodeType.ICD10, "J06.9",   "Acute upper respiratory infection, unspecified"),
        new("pneumonia",                                MedicalCodeType.ICD10, "J18.9",   "Pneumonia, unspecified organism"),
        new("anxiety",                                  MedicalCodeType.ICD10, "F41.1",   "Generalized anxiety disorder"),
        new("anxiety disorder",                         MedicalCodeType.ICD10, "F41.1",   "Generalized anxiety disorder"),
        new("depression",                               MedicalCodeType.ICD10, "F32.9",   "Major depressive disorder, single episode, unspecified"),
        new("depressive disorder",                      MedicalCodeType.ICD10, "F32.9",   "Major depressive disorder, single episode, unspecified"),
        new("asthma",                                   MedicalCodeType.ICD10, "J45.909", "Unspecified asthma, uncomplicated"),
        new("low back pain",                            MedicalCodeType.ICD10, "M54.5",   "Low back pain"),
        new("back pain",                                MedicalCodeType.ICD10, "M54.5",   "Low back pain"),
        new("urinary tract infection",                  MedicalCodeType.ICD10, "N39.0",   "Urinary tract infection, site not specified"),
        new("uti",                                      MedicalCodeType.ICD10, "N39.0",   "Urinary tract infection, site not specified"),
        new("hypothyroidism",                           MedicalCodeType.ICD10, "E03.9",   "Hypothyroidism, unspecified"),
        new("hyperlipidemia",                           MedicalCodeType.ICD10, "E78.5",   "Hyperlipidemia, unspecified"),
        new("high cholesterol",                         MedicalCodeType.ICD10, "E78.5",   "Hyperlipidemia, unspecified"),
        new("obesity",                                  MedicalCodeType.ICD10, "E66.9",   "Obesity, unspecified"),
        new("osteoarthritis",                           MedicalCodeType.ICD10, "M19.90",  "Primary osteoarthritis, unspecified site"),
        new("arthritis",                                MedicalCodeType.ICD10, "M19.90",  "Primary osteoarthritis, unspecified site"),
        new("chronic kidney disease",                   MedicalCodeType.ICD10, "N18.9",   "Chronic kidney disease, unspecified"),
        new("ckd",                                      MedicalCodeType.ICD10, "N18.9",   "Chronic kidney disease, unspecified"),

        // CPT — common procedures / visit types
        new("office visit established patient",         MedicalCodeType.CPT, "99213", "Office/outpatient visit, established patient, low-mod complexity"),
        new("established patient visit",                MedicalCodeType.CPT, "99213", "Office/outpatient visit, established patient, low-mod complexity"),
        new("follow up",                                MedicalCodeType.CPT, "99213", "Office/outpatient visit, established patient, low-mod complexity"),
        new("follow-up",                                MedicalCodeType.CPT, "99213", "Office/outpatient visit, established patient, low-mod complexity"),
        new("office visit",                             MedicalCodeType.CPT, "99213", "Office/outpatient visit, established patient, low-mod complexity"),
        new("new patient visit",                        MedicalCodeType.CPT, "99203", "Office/outpatient visit, new patient, low-mod complexity"),
        new("new patient",                              MedicalCodeType.CPT, "99203", "Office/outpatient visit, new patient, low-mod complexity"),
        new("annual wellness visit",                    MedicalCodeType.CPT, "99387", "Preventive visit, new patient, 40-64 years"),
        new("wellness visit",                           MedicalCodeType.CPT, "99387", "Preventive visit, new patient, 40-64 years"),
        new("blood pressure check",                     MedicalCodeType.CPT, "99211", "Office/outpatient visit, established patient, minimal complexity"),
        new("vaccination",                              MedicalCodeType.CPT, "90471", "Immunization administration, 1 vaccine"),
        new("immunization",                             MedicalCodeType.CPT, "90471", "Immunization administration, 1 vaccine"),
        new("wound care",                               MedicalCodeType.CPT, "97597", "Debridement, open wound"),
        new("ecg",                                      MedicalCodeType.CPT, "93000", "Electrocardiogram, routine, with interpretation"),
        new("electrocardiogram",                        MedicalCodeType.CPT, "93000", "Electrocardiogram, routine, with interpretation"),
        new("ekg",                                      MedicalCodeType.CPT, "93000", "Electrocardiogram, routine, with interpretation"),
        new("complete blood count",                     MedicalCodeType.CPT, "85025", "Blood count; complete (CBC), automated"),
        new("cbc",                                      MedicalCodeType.CPT, "85025", "Blood count; complete (CBC), automated"),
        new("lipid panel",                              MedicalCodeType.CPT, "80061", "Lipid panel"),
        new("comprehensive metabolic panel",            MedicalCodeType.CPT, "80053", "Comprehensive metabolic panel"),
        new("cmp",                                      MedicalCodeType.CPT, "80053", "Comprehensive metabolic panel"),
    ];

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>Lower-cases and collapses whitespace for consistent comparison.</summary>
    private static string Normalise(string text) =>
        string.Join(' ', text.ToLowerInvariant().Split((char[])null!, StringSplitOptions.RemoveEmptyEntries));

    // -----------------------------------------------------------------------
    // Internal types
    // -----------------------------------------------------------------------

    private sealed record RuleEntry(
        string Key,
        MedicalCodeType CodeType,
        string CodeValue,
        string Description);
}
