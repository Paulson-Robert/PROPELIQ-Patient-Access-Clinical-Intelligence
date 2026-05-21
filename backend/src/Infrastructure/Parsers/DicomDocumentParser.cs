// DocumentFormat.OpenXml introduces a root namespace 'DocumentFormat' — use fully-qualified Domain enum.
using FellowOakDicom;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Infrastructure.Parsers;

// ---------------------------------------------------------------------------
// DicomDocumentParser — FellowOakDicom metadata extraction (AC-04)
// ---------------------------------------------------------------------------

/// <summary>
/// Extracts key metadata from DICOM files using fo-dicom (AC-04).
///
/// Produces a plain-text representation of the most clinically relevant DICOM
/// header tags (patient demographics, study details, modality) suitable for
/// ingestion by the NER pipeline.
///
/// Only metadata is extracted — pixel data is not read.
/// Corrupt DICOM files are caught and returned as a failed <see cref="DocumentParseResult"/>
/// (edge case: corrupt file).
/// </summary>
internal sealed class DicomDocumentParser : IDocumentParser
{
    private static readonly IReadOnlySet<Domain.Enums.DocumentFormat> _formats =
        new HashSet<Domain.Enums.DocumentFormat> { Domain.Enums.DocumentFormat.Dicom };

    // Ordered list of DICOM tags to extract and their human-readable labels.
    private static readonly (DicomTag Tag, string Label)[] TagMap =
    [
        (DicomTag.PatientName,          "PatientName"),
        (DicomTag.PatientID,            "PatientID"),
        (DicomTag.PatientBirthDate,     "PatientBirthDate"),
        (DicomTag.PatientSex,           "PatientSex"),
        (DicomTag.StudyDate,            "StudyDate"),
        (DicomTag.StudyTime,            "StudyTime"),
        (DicomTag.StudyDescription,     "StudyDescription"),
        (DicomTag.SeriesDescription,    "SeriesDescription"),
        (DicomTag.Modality,             "Modality"),
        (DicomTag.InstitutionName,      "InstitutionName"),
        (DicomTag.ReferringPhysicianName, "ReferringPhysician"),
        (DicomTag.StudyID,              "StudyID"),
        (DicomTag.AccessionNumber,      "AccessionNumber"),
    ];

    private readonly ILogger<DicomDocumentParser> _logger;

    public DicomDocumentParser(ILogger<DicomDocumentParser> logger) => _logger = logger;

    public IReadOnlySet<Domain.Enums.DocumentFormat> SupportedFormats => _formats;

    public async Task<DocumentParseResult> ParseAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            // fo-dicom loads only the dataset header by default (no pixel data).
            var file = await DicomFile.OpenAsync(content, FileReadOption.SkipLargeTags)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var ds = file.Dataset;
            var sb = new StringBuilder();

            foreach (var (tag, label) in TagMap)
            {
                var value = ds.GetSingleValueOrDefault(tag, string.Empty);
                if (!string.IsNullOrWhiteSpace(value))
                    sb.AppendLine($"{label}: {value}");
            }

            return DocumentParseResult.Ok(sb.ToString());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DICOM parsing failed — file may be corrupt or not a valid DICOM object");
            return DocumentParseResult.Fail(ex.Message);
        }
    }
}
