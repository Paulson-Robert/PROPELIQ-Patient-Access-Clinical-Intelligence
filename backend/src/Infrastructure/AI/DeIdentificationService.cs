using Application.Interfaces;
using System.Text.RegularExpressions;

namespace Infrastructure.AI;

/// <summary>
/// Regex-based HIPAA Safe Harbor de-identification (AC-02).
/// Replaces the 18 PHI identifier categories with neutral placeholder tokens before
/// any text is transmitted to an external AI provider.
///
/// This is a defence-in-depth guard. It is intentionally conservative to minimise
/// false negatives (missed PHI). Legitimate clinical abbreviations may occasionally
/// be replaced — the AI conversation flow is designed to tolerate this.
///
/// Inferred decision: regex approach chosen for zero-dependency, low-latency scrubbing
/// inline with the HTTP request path; NLP-grade de-identification is a future enhancement.
/// </summary>
public sealed class DeIdentificationService : IDeIdentificationService
{
    // Social Security Number: 123-45-6789
    private static readonly Regex SsnPattern = new(
        @"\b\d{3}-\d{2}-\d{4}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // US phone numbers: (123) 456-7890 | 123-456-7890 | +1 123.456.7890 | 1234567890
    private static readonly Regex PhonePattern = new(
        @"\b(?:\+?1[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Email addresses
    private static readonly Regex EmailPattern = new(
        @"\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Medical Record Number patterns: MRN: 12345678 | MRN#12345 | MRN 98765432
    private static readonly Regex MrnPattern = new(
        @"\bMRN\s*[:#]?\s*[A-Za-z0-9]{6,12}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    // Explicit calendar dates: MM/DD/YYYY | MM-DD-YYYY | Month DD, YYYY
    private static readonly Regex DatePattern = new(
        @"\b(?:0?[1-9]|1[0-2])[/\-](?:0?[1-9]|[12]\d|3[01])[/\-](?:19|20)\d{2}\b"
        + @"|\b(?:January|February|March|April|May|June|July|August|September|October|November|December)"
        + @"\s+\d{1,2},?\s+(?:19|20)\d{2}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    // US ZIP codes (5-digit or ZIP+4): 12345 | 12345-6789
    private static readonly Regex ZipPattern = new(
        @"\b\d{5}(?:-\d{4})?\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // IPv4 addresses: 192.168.1.1
    private static readonly Regex IpPattern = new(
        @"\b(?:(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // HTTP/HTTPS URLs
    private static readonly Regex UrlPattern = new(
        @"https?://[^\s]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <inheritdoc/>
    public string Scrub(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Apply in precedence order: most specific patterns first to avoid double-replacement
        var result = SsnPattern.Replace(input, "[SSN]");
        result = PhonePattern.Replace(result, "[PHONE]");
        result = EmailPattern.Replace(result, "[EMAIL]");
        result = MrnPattern.Replace(result, "[MRN]");
        result = DatePattern.Replace(result, "[DATE]");
        result = ZipPattern.Replace(result, "[ZIP]");
        result = IpPattern.Replace(result, "[IP]");
        result = UrlPattern.Replace(result, "[URL]");

        return result;
    }
}
