using Application.Interfaces;
using Infrastructure.Data;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Generates a booking confirmation PDF using iText7 (AC-04, NFR-011).
/// Called from a Hangfire background job so failures are retried automatically (Edge Case).
/// </summary>
public sealed class BookingPdfService : IBookingPdfService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingPdfService> _logger;

    public BookingPdfService(
        ApplicationDbContext db,
        IEmailService emailService,
        ILogger<BookingPdfService> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task GenerateAndDeliverAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appt = await _db.Appointments
            .AsNoTracking()
            .Include(a => a.Slot)
            .Include(a => a.Patient)
            .Where(a => a.AppointmentId == appointmentId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (appt is null)
        {
            _logger.LogWarning(
                "BookingPdfService: Appointment {AppointmentId} not found. Skipping PDF generation.",
                appointmentId);
            return;
        }

        byte[] pdfBytes;

        try
        {
            pdfBytes = GeneratePdf(appt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "PDF generation failed for appointment {AppointmentId}. " +
                "Hangfire will retry this job.",
                appointmentId);
            throw; // Let Hangfire retry
        }

        // Deliver via email as an attachment
        var subject = "Your appointment confirmation";
        var body = BuildEmailBody(appt);

        await _emailService.SendConfirmationEmailAsync(
            appt.Patient.Email,
            subject,
            body,
            pdfBytes,
            $"appointment-{appointmentId:N}.pdf",
            cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Booking confirmation PDF delivered. AppointmentId={AppointmentId}, Email={Email}.",
            appointmentId, appt.Patient.Email);
    }

    private static byte[] GeneratePdf(Domain.Entities.Appointment appt)
    {
        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4);
        document.SetMargins(50, 50, 50, 50);

        // Header
        var headerFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
        var bodyFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

        document.Add(new Paragraph("Appointment Confirmation")
            .SetFont(headerFont)
            .SetFontSize(20)
            .SetFontColor(new DeviceRgb(0x21, 0x21, 0x21))
            .SetMarginBottom(4));

        document.Add(new Paragraph("HealthAccess Patient Portal")
            .SetFont(bodyFont)
            .SetFontSize(11)
            .SetFontColor(new DeviceRgb(0x55, 0x55, 0x55))
            .SetMarginBottom(24));

        // Divider
        document.Add(new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine(1f))
            .SetMarginBottom(20));

        // Detail rows
        var table = new Table(UnitValue.CreatePercentArray([40f, 60f]))
            .UseAllAvailableWidth();

        AddRow(table, "Provider", appt.Slot.ProviderName, headerFont, bodyFont);
        AddRow(table, "Specialty", appt.Slot.Specialty, headerFont, bodyFont);
        AddRow(table, "Date", appt.Slot.StartTime.ToString("dddd, MMMM d, yyyy"), headerFont, bodyFont);
        AddRow(table, "Time", appt.Slot.StartTime.ToString("hh:mm tt"), headerFont, bodyFont);
        AddRow(table, "Duration", $"{(int)(appt.Slot.EndTime - appt.Slot.StartTime).TotalMinutes} minutes", headerFont, bodyFont);
        AddRow(table, "Status", "Confirmed", headerFont, bodyFont);
        AddRow(table, "Insurance provider", appt.InsuranceProvider ?? "Not provided", headerFont, bodyFont);
        AddRow(table, "Policy number", appt.InsurancePolicyNumber ?? "Not provided", headerFont, bodyFont);
        AddRow(table, "Reference", appt.AppointmentId.ToString("N")[..8].ToUpperInvariant(), headerFont, bodyFont);

        document.Add(table);

        document.Add(new Paragraph(
                "Please arrive 10 minutes before your appointment. Contact us if you need to cancel or reschedule.")
            .SetFont(bodyFont)
            .SetFontSize(9)
            .SetFontColor(new DeviceRgb(0x88, 0x88, 0x88))
            .SetMarginTop(24));

        document.Close();

        return stream.ToArray();
    }

    private static void AddRow(
        Table table,
        string label,
        string value,
        PdfFont labelFont,
        PdfFont valueFont)
    {
        var gray = new DeviceRgb(0x55, 0x55, 0x55);
        var dark = new DeviceRgb(0x21, 0x21, 0x21);

        table.AddCell(new Cell()
            .Add(new Paragraph(label).SetFont(labelFont).SetFontSize(10).SetFontColor(gray))
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetPaddingBottom(6));

        table.AddCell(new Cell()
            .Add(new Paragraph(value).SetFont(valueFont).SetFontSize(10).SetFontColor(dark))
            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
            .SetPaddingBottom(6));
    }

    private static string BuildEmailBody(Domain.Entities.Appointment appt) =>
        $"Hi,\n\nYour appointment with {appt.Slot.ProviderName} on " +
        $"{appt.Slot.StartTime:dddd, MMMM d, yyyy} at {appt.Slot.StartTime:hh:mm tt} is confirmed. " +
        $"Please find your confirmation PDF attached.\n\nThank you,\nHealthAccess";
}
