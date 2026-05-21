// DocumentFormat.OpenXml introduces a root namespace 'DocumentFormat' — use fully-qualified Domain enum.
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Parsers;

// ---------------------------------------------------------------------------
// FhirDocumentParser — Hl7.Fhir.R4 structured data extraction (AC-05)
// ---------------------------------------------------------------------------

/// <summary>
/// Parses HL7 FHIR R4 documents (JSON or XML) and produces plain text
/// suitable for ingestion by the NER pipeline (AC-05).
///
/// Extraction strategy:
/// 1. Strip HTML from the FHIR narrative <c>Text.Div</c> field (if present).
/// 2. For <c>Bundle</c> resources, recurse into each entry.
/// 3. For typed resources (<c>Patient</c>, <c>Condition</c>, <c>MedicationRequest</c>,
///    <c>Procedure</c>), extract key fields as labelled text lines.
///
/// Format detection: attempts JSON first; falls back to XML (inferred decision).
/// Corrupt or invalid FHIR content is caught and returned as a failed
/// <see cref="DocumentParseResult"/> (edge case: corrupt file).
/// </summary>
internal sealed partial class FhirDocumentParser : IDocumentParser
{
    private static readonly IReadOnlySet<Domain.Enums.DocumentFormat> _formats =
        new HashSet<Domain.Enums.DocumentFormat> { Domain.Enums.DocumentFormat.Hl7Fhir };

    // Pre-compiled regex to strip HTML tags from FHIR narrative.
    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    private readonly FhirJsonParser _jsonParser = new(new ParserSettings { AcceptUnknownMembers = true });
    private readonly FhirXmlParser _xmlParser = new(new ParserSettings { AcceptUnknownMembers = true });
    private readonly ILogger<FhirDocumentParser> _logger;

    public FhirDocumentParser(ILogger<FhirDocumentParser> logger) => _logger = logger;

    public IReadOnlySet<Domain.Enums.DocumentFormat> SupportedFormats => _formats;

    public async Task<DocumentParseResult> ParseAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        string raw;
        try
        {
            using var reader = new StreamReader(content, leaveOpen: true);
            raw = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read FHIR document stream");
            return DocumentParseResult.Fail(ex.Message);
        }

        cancellationToken.ThrowIfCancellationRequested();

        Resource? resource = TryParseJson(raw) ?? TryParseXml(raw);

        if (resource is null)
        {
            _logger.LogWarning("FHIR document could not be parsed as JSON or XML");
            return DocumentParseResult.Fail("Not a valid FHIR R4 JSON or XML document.");
        }

        var sb = new StringBuilder();
        ExtractResource(resource, sb, cancellationToken);
        return DocumentParseResult.Ok(sb.ToString());
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private Resource? TryParseJson(string content)
    {
        try { return _jsonParser.Parse<Resource>(content); }
        catch { return null; }
    }

    private Resource? TryParseXml(string content)
    {
        try { return _xmlParser.Parse<Resource>(content); }
        catch { return null; }
    }

    private void ExtractResource(Resource resource, StringBuilder sb, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        switch (resource)
        {
            case Bundle bundle:
                foreach (var entry in bundle.Entry)
                    if (entry.Resource is not null)
                        ExtractResource(entry.Resource, sb, ct);
                break;

            case Patient patient:
                AppendIfPresent(sb, "PatientName", patient.Name?.FirstOrDefault()?.Text);
                AppendIfPresent(sb, "BirthDate", patient.BirthDate);
                AppendIfPresent(sb, "Gender", patient.Gender?.ToString());
                break;

            case Condition condition:
                AppendIfPresent(sb, "Diagnosis", condition.Code?.Text ?? condition.Code?.Coding?.FirstOrDefault()?.Display);
                AppendIfPresent(sb, "ClinicalStatus", condition.ClinicalStatus?.Coding?.FirstOrDefault()?.Code);
                AppendIfPresent(sb, "OnsetDate", (condition.Onset as FhirDateTime)?.Value);
                break;

            case MedicationRequest medReq:
                AppendIfPresent(sb, "Medication", medReq.Medication is CodeableConcept cc
                    ? cc.Text ?? cc.Coding?.FirstOrDefault()?.Display
                    : null);
                AppendIfPresent(sb, "DosageInstruction", medReq.DosageInstruction?.FirstOrDefault()?.Text);
                break;

            case Procedure procedure:
                AppendIfPresent(sb, "Procedure", procedure.Code?.Text ?? procedure.Code?.Coding?.FirstOrDefault()?.Display);
                AppendIfPresent(sb, "Status", procedure.Status?.ToString());
                AppendIfPresent(sb, "PerformedDate", (procedure.Performed as FhirDateTime)?.Value);
                break;

            default:
                // For all other resource types, extract the FHIR narrative HTML as plain text.
                if (resource is DomainResource dr && dr.Text?.Div is { } div)
                    sb.AppendLine(HtmlTagRegex().Replace(div, " ").Trim());
                break;
        }
    }

    private static void AppendIfPresent(StringBuilder sb, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            sb.AppendLine($"{label}: {value}");
    }
}
