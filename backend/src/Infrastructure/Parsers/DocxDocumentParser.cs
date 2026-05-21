// DocumentFormat.OpenXml introduces a root namespace 'DocumentFormat' — use fully-qualified Domain enum.
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Parsers;

// ---------------------------------------------------------------------------
// DocxDocumentParser — DocumentFormat.OpenXml DOCX extraction (AC-02, NFR-011)
// ---------------------------------------------------------------------------

/// <summary>
/// Extracts plain text from DOCX documents using DocumentFormat.OpenXml (AC-02, NFR-011).
///
/// Reads all paragraph runs from the main document body.
/// Corrupt or non-OOXML files are caught and returned as a failed
/// <see cref="DocumentParseResult"/> (edge case: corrupt file).
/// </summary>
internal sealed class DocxDocumentParser : IDocumentParser
{
    private static readonly IReadOnlySet<Domain.Enums.DocumentFormat> _formats =
        new HashSet<Domain.Enums.DocumentFormat> { Domain.Enums.DocumentFormat.Docx };

    private readonly ILogger<DocxDocumentParser> _logger;

    public DocxDocumentParser(ILogger<DocxDocumentParser> logger) => _logger = logger;

    public IReadOnlySet<Domain.Enums.DocumentFormat> SupportedFormats => _formats;

    public Task<DocumentParseResult> ParseAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            using var document = WordprocessingDocument.Open(content, isEditable: false);

            var body = document.MainDocumentPart?.Document?.Body;
            if (body is null)
                return Task.FromResult(DocumentParseResult.Fail("DOCX body is null — document may be malformed."));

            var sb = new StringBuilder();
            foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.AppendLine(paragraph.InnerText);
            }

            return Task.FromResult(DocumentParseResult.Ok(sb.ToString()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DOCX parsing failed — file may be corrupt or not a valid OOXML document");
            return Task.FromResult(DocumentParseResult.Fail(ex.Message));
        }
    }
}
