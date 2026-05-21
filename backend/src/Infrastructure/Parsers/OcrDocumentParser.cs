// DocumentFormat.OpenXml introduces a root namespace 'DocumentFormat' — use fully-qualified Domain enum.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tesseract;

namespace Infrastructure.Parsers;

// ---------------------------------------------------------------------------
// OcrDocumentParser — Tesseract OCR for images and scanned PDFs (AC-03, NFR-011)
// ---------------------------------------------------------------------------

/// <summary>
/// Configuration for the Tesseract OCR engine.
/// Bind from the "Tesseract" section in appsettings.
/// </summary>
internal sealed class TesseractOptions
{
    public const string SectionName = "Tesseract";

    /// <summary>
    /// Path to the tessdata directory containing language data files (.traineddata).
    /// Relative paths are resolved from <see cref="AppContext.BaseDirectory"/>.
    /// Default: "tessdata".
    /// </summary>
    public string TessDataPath { get; set; } = "tessdata";

    /// <summary>
    /// ISO 639-3 language code passed to Tesseract.
    /// Default: "eng" — enforces English extraction (edge case: multi-language document).
    /// </summary>
    public string Language { get; set; } = "eng";
}

/// <summary>
/// Extracts text from JPEG and PNG images (including scanned PDFs) using Tesseract OCR (AC-03, NFR-011).
///
/// <see cref="TesseractEngine"/> is not concurrency-safe; a <see cref="SemaphoreSlim"/>
/// serialises concurrent OCR calls. The engine is initialised once and reused.
///
/// Multi-language edge case: defaults to English. Override via <see cref="TesseractOptions.Language"/>.
/// Corrupt images are caught and returned as a failed <see cref="DocumentParseResult"/>.
/// </summary>
internal sealed class OcrDocumentParser : IDocumentParser, IDisposable
{
    private static readonly IReadOnlySet<Domain.Enums.DocumentFormat> _formats =
        new HashSet<Domain.Enums.DocumentFormat> { Domain.Enums.DocumentFormat.Jpg, Domain.Enums.DocumentFormat.Png };

    private readonly TesseractEngine _engine;
    private readonly ILogger<OcrDocumentParser> _logger;

    // TesseractEngine is not thread-safe; serialise via semaphore (size 1).
    private readonly SemaphoreSlim _gate = new(1, 1);

    private bool _disposed;

    public OcrDocumentParser(
        IOptions<TesseractOptions> options,
        ILogger<OcrDocumentParser> logger)
    {
        _logger = logger;

        var tessDataPath = Path.IsPathRooted(options.Value.TessDataPath)
            ? options.Value.TessDataPath
            : Path.Combine(AppContext.BaseDirectory, options.Value.TessDataPath);

        _engine = new TesseractEngine(tessDataPath, options.Value.Language, EngineMode.Default);
    }

    public IReadOnlySet<Domain.Enums.DocumentFormat> SupportedFormats => _formats;

    public async Task<DocumentParseResult> ParseAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        // Buffer image bytes before entering the semaphore to minimise lock hold time.
        byte[] imageBytes;
        try
        {
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            imageBytes = ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read image stream for OCR");
            return DocumentParseResult.Fail(ex.Message);
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var pix = Pix.LoadFromMemory(imageBytes);
            using var page = _engine.Process(pix);
            return DocumentParseResult.Ok(page.GetText());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed — image may be corrupt or unsupported");
            return DocumentParseResult.Fail(ex.Message);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gate.Dispose();
        _engine.Dispose();
    }
}
