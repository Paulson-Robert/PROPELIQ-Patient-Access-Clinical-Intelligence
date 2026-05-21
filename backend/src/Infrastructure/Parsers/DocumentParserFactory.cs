// DocumentFormat.OpenXml introduces a root namespace 'DocumentFormat' that conflicts
// with Domain.Enums.DocumentFormat — use fully-qualified type.

namespace Infrastructure.Parsers;

// ---------------------------------------------------------------------------
// DocumentParserFactory — resolves IDocumentParser by DocumentFormat (US_035)
// ---------------------------------------------------------------------------

/// <summary>
/// Resolves the registered <see cref="IDocumentParser"/> for a given <see cref="DocumentFormat"/>
/// by scanning the set of parsers registered with the DI container (US_035).
///
/// All registered <see cref="IDocumentParser"/> implementations are injected via
/// <c>IEnumerable&lt;IDocumentParser&gt;</c>; the factory builds a lookup dictionary
/// on first construction.
///
/// When no parser covers a format, <see cref="GetParser"/> returns <see langword="null"/>
/// so callers can decide how to handle unsupported formats without throwing.
/// </summary>
internal sealed class DocumentParserFactory : IDocumentParserFactory
{
    private readonly IReadOnlyDictionary<Domain.Enums.DocumentFormat, IDocumentParser> _lookup;

    public DocumentParserFactory(IEnumerable<IDocumentParser> parsers)
    {
        var dict = new Dictionary<Domain.Enums.DocumentFormat, IDocumentParser>();
        foreach (var parser in parsers)
            foreach (var format in parser.SupportedFormats)
                dict.TryAdd(format, parser); // first registration wins per format

        _lookup = dict;
    }

    /// <inheritdoc/>
    public IDocumentParser? GetParser(Domain.Enums.DocumentFormat format)
        => _lookup.TryGetValue(format, out var parser) ? parser : null;
}
