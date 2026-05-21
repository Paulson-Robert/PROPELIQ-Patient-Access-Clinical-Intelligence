namespace Application.Configuration;

// ---------------------------------------------------------------------------
// CodeMappingOptions — configuration for ICD-10/CPT code mapping engine
// (AC-01, TR-012)
// ---------------------------------------------------------------------------

/// <summary>
/// Configuration for the code mapping engine.
/// Bind from the "CodeMapping" configuration section.
/// </summary>
public sealed class CodeMappingOptions
{
    /// <summary>Configuration section key bound in appsettings.</summary>
    public const string SectionName = "CodeMapping";

    /// <summary>
    /// Directory containing the trained ML.NET code mapping model zip file.
    /// Relative to the application working directory.
    /// When the file is absent, the service degrades gracefully to rule-based mapping only.
    /// </summary>
    public string ModelDirectory { get; set; } = "ml-models/code-mapping";

    /// <summary>
    /// ICD-10 code set version label appended to every mapped candidate.
    /// Inferred decision: calendar-year version string used when requirements specify no version.
    /// </summary>
    public string Icd10Version { get; set; } = "ICD-10-CM 2024";

    /// <summary>CPT code set version label appended to every CPT candidate.</summary>
    public string CptVersion { get; set; } = "CPT 2024";
}
