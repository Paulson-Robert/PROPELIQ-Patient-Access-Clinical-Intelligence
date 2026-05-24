namespace Infrastructure.Storage;

/// <summary>
/// Configuration options for local document file storage.
/// Bind from <c>DocumentStorage</c> section in appsettings.
/// </summary>
public sealed class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";
    public static readonly string DefaultBasePath = Path.Combine(Path.GetTempPath(), "propeliq-documents");

    /// <summary>
    /// Absolute or relative path to the directory where uploaded files are stored.
    /// Must be outside the web root to prevent direct HTTP access (OWASP A05).
    /// </summary>
    public string BasePath { get; set; } = DefaultBasePath;

    /// <summary>
    /// Returns a writable default path when configuration binds an empty string.
    /// </summary>
    public string EffectiveBasePath =>
        string.IsNullOrWhiteSpace(BasePath) ? DefaultBasePath : Path.GetFullPath(BasePath);

    /// <summary>Maximum permitted upload size in bytes. Default: 25 MB (AC-02).</summary>
    public long MaxFileSizeBytes { get; set; } = 26_214_400; // 25 MiB

    /// <summary>
    /// When true, a successful upload is treated as a completed local processing flow.
    /// Set to false in environments with a running malware scan / extraction pipeline.
    /// </summary>
    public bool CompleteUploadsImmediately { get; set; } = true;
}
