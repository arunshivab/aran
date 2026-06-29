using System;
using System.Globalization;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.Linac;

/// <summary>
/// Generates a self-contained shielding report PDF for one standard (NCRP 151 or AERB).
/// Call <see cref="Generate"/> twice — once per standard — to produce two separate PDFs.
/// </summary>
public sealed class LinacReportGenerator
{
    /// <summary>
    /// Generates the shielding report and returns it as a byte array.
    /// </summary>
    /// <param name="input">The complete report input. <see cref="LinacReportInput.Maze"/> is mandatory.</param>
    /// <returns>The PDF bytes.</returns>
    public byte[] Generate(LinacReportInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        ReportBuilder rb = ReportBuilder.Create()
            .SetTitle("Shielding Report — " + input.WallResult.StandardName + " — " + input.FacilityName)
            .SetAuthor(input.PreparedBy)
            .SetSubject("Radiotherapy facility shielding calculation — " + input.Machine.Name)
            .WithPageSetup(new ReportPageSetup
            {
                PageSize = PageSize.A4,
                MarginLeft = 40,
                MarginRight = 40,
                MarginTop = 52,
                MarginBottom = 48,
            })
            .WithHeader(new HeaderFooterStyle
            {
                Text = input.FacilityName + "  |  " + input.Machine.Name + "  |  " + input.WallResult.StandardName,
                Font = ReportFont.Helvetica,
                FontSize = 7,
                Color = Styles.DarkGrey,
                Alignment = TextAlignment.Left,
                ShowOnFirstPage = false,
                RuleLine = true,
            })
            .WithFooter(new HeaderFooterStyle
            {
                Text = "Aran shielding report  |  Prepared: " +
                       input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) +
                       "  |  Page {page} of {pages}",
                Font = ReportFont.Helvetica,
                FontSize = 7,
                Color = Styles.DarkGrey,
                Alignment = TextAlignment.Center,
                RuleLine = true,
            });

        InputDataPage.Render(rb, input);
        WallCalcPages.Render(rb, input.WallResult);
        DoorCalcPages.Render(rb, input.DoorResult);
        UndertakingPage.Render(rb, input);

        return rb.ToByteArray();
    }

    /// <summary>
    /// Generates the shielding report and saves it to a file.
    /// </summary>
    /// <param name="input">The complete report input.</param>
    /// <param name="outputPath">The file path to write the PDF to.</param>
    public void GenerateToFile(LinacReportInput input, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(outputPath);
        byte[] bytes = Generate(input);
        System.IO.File.WriteAllBytes(outputPath, bytes);
    }
}
