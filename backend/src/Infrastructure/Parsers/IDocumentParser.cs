// DocumentFormat.OpenXml introduces a root namespace 'DocumentFormat' which conflicts
// with Domain.Enums.DocumentFormat. Use the fully-qualified type throughout this file.

namespace Infrastructure.Parsers;

// ---------------------------------------------------------------------------
// Document parser abstractions — text extraction for NER pipeline (US_035)
// All types are internal to Infrastructure; no Application coupling needed.
// ---------------------------------------------------------------------------

/// <summary>
/// The outcome of a single document parse attempt.
/// </summary>
/// <param name="Success">True when text was extracted without fatal errors.</param>
/// <param name="Text">Extracted plain text. Empty string on failure.</param>
/// <param name="ErrorMessage">Diagnostic message set when <see cref="Success"/> is false (edge case: corrupt file).</param>
internal sealed record DocumentParseResult(bool Success, string Text, string? ErrorMessage)
{
    /// <summary>Convenience factory for a successful extraction.</summary>
    internal static DocumentParseResult Ok(string text) => new(true, text, null);

    /// <summary>Convenience factory for a failed extraction (edge case: corrupt or unreadable file).</summary>
    internal static DocumentParseResult Fail(string message) => new(false, string.Empty, message);
}

/// <summary>
/// Format-specific document text extractor.
/// Each implementation handles one or more <see cref="Domain.Enums.DocumentFormat"/> values.
/// </summary>
internal interface IDocumentParser
{
    /// <summary>The set of <see cref="Domain.Enums.DocumentFormat"/> values this parser handles.</summary>
    IReadOnlySet<Domain.Enums.DocumentFormat> SupportedFormats { get; }

    /// <summary>
    /// Extracts plain text from <paramref name="content"/>.
    /// Never throws — corrupt file errors are surfaced via <see cref="DocumentParseResult.Fail"/>.
    /// </summary>
    Task<DocumentParseResult> ParseAsync(Stream content, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves the correct <see cref="IDocumentParser"/> for a given <see cref="Domain.Enums.DocumentFormat"/>.
/// </summary>
internal interface IDocumentParserFactory
{
    /// <summary>
    /// Returns the parser registered for <paramref name="format"/>, or
    /// <see langword="null"/> when no parser is available for that format.
    /// </summary>
    IDocumentParser? GetParser(Domain.Enums.DocumentFormat format);
}
