// DocumentFormat.OpenXml introduces a root namespace 'DocumentFormat' — use fully-qualified type.
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Parsers;

// ---------------------------------------------------------------------------
// PdfDocumentParser — iText7 text extraction (AC-01, NFR-011)
// ---------------------------------------------------------------------------

/// <summary>
/// Extracts plain text from PDF documents using iText7 (AC-01, NFR-011).
///
/// Processes each page sequentially and concatenates the output.
/// Corrupt or password-protected PDFs are caught and returned as a failed
/// <see cref="DocumentParseResult"/> with the exception message logged
/// (edge case: corrupt file).
/// </summary>
internal sealed class PdfDocumentParser : IDocumentParser
{
    private static readonly IReadOnlySet<Domain.Enums.DocumentFormat> _formats =
        new HashSet<Domain.Enums.DocumentFormat> { Domain.Enums.DocumentFormat.Pdf };

    private readonly ILogger<PdfDocumentParser> _logger;

    public PdfDocumentParser(ILogger<PdfDocumentParser> logger) => _logger = logger;

    public IReadOnlySet<Domain.Enums.DocumentFormat> SupportedFormats => _formats;

    public Task<DocumentParseResult> ParseAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            var sb = new StringBuilder();

            using var reader = new PdfReader(content);
            using var pdfDoc = new PdfDocument(reader);

            int pageCount = pdfDoc.GetNumberOfPages();
            for (int i = 1; i <= pageCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.AppendLine(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
            }

            return Task.FromResult(DocumentParseResult.Ok(sb.ToString()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF parsing failed — file may be corrupt or password-protected");
            return Task.FromResult(DocumentParseResult.Fail(ex.Message));
        }
    }
}
