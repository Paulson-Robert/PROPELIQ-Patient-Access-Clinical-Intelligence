namespace Infrastructure.Security;

/// <summary>
/// Configuration for the ClamAV malware scanning daemon (clamd).
/// Bind from the <c>ClamAV</c> section in appsettings.
/// </summary>
public sealed class ClamAvOptions
{
    public const string SectionName = "ClamAV";

    /// <summary>Hostname or IP address of the clamd daemon. Default: localhost.</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>TCP port clamd is listening on. Default: 3310.</summary>
    public int Port { get; set; } = 3310;

    /// <summary>
    /// Per-scan timeout in seconds. If the scan does not complete within this
    /// window the attempt is considered a timeout. Default: 30 seconds.
    /// </summary>
    public int ScanTimeoutSeconds { get; set; } = 30;
}
