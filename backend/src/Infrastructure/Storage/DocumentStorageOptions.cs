namespace Infrastructure.Storage;

/// <summary>
/// Configuration options for local document file storage.
/// Bind from <c>DocumentStorage</c> section in appsettings.
/// </summary>
public sealed class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    /// <summary>
    /// Absolute or relative path to the directory where uploaded files are stored.
    /// Must be outside the web root to prevent direct HTTP access (OWASP A05).
    /// </summary>
    public string BasePath { get; set; } = Path.Combine(Path.GetTempPath(), "propeliq-documents");

    /// <summary>Maximum permitted upload size in bytes. Default: 25 MB (AC-02).</summary>
    public long MaxFileSizeBytes { get; set; } = 26_214_400; // 25 MiB
}
