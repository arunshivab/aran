using System;
using System.Globalization;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.NuclearMedicine;

/// <summary>
/// Generates a single AERB nuclear medicine shielding report PDF covering
/// any combination of PET/PET-CT, Gamma Camera/SPECT and HDT I-131 installations.
/// Supply only the modalities present in the facility via <see cref="NmReportInput"/>.
/// </summary>
public sealed class NmReportGenerator
{
    /// <summary>Generates the NM shielding report and returns it as a byte array.</summary>
    /// <param name="input">The complete report input.</param>
    /// <returns>The PDF bytes.</returns>
    public byte[] Generate(NmReportInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.PetInput is null && input.GammaCameraInput is null && input.HdtInput is null)
        {
            throw new ArgumentException(
                "At least one modality (PET, Gamma Camera, or HDT) must be supplied.", nameof(input));
        }

        ReportBuilder rb = ReportBuilder.Create()
            .SetTitle("Nuclear Medicine Shielding Report — AERB — " + input.FacilityName)
            .SetAuthor(input.PreparedBy)
            .SetSubject("NM facility shielding calculation — " + input.InstallationTypes)
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
                Text = input.FacilityName + "  |  " + input.InstallationTypes + "  |  AERB",
                Font = ReportFont.Helvetica,
                FontSize = 7,
                Color = NmStyles.DarkGrey,
                Alignment = TextAlignment.Left,
                ShowOnFirstPage = false,
                RuleLine = true,
            })
            .WithFooter(new HeaderFooterStyle
            {
                Text = "Aran NM shielding report  |  Prepared: " +
                       input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) +
                       "  |  Page {page} of {pages}",
                Font = ReportFont.Helvetica,
                FontSize = 7,
                Color = NmStyles.DarkGrey,
                Alignment = TextAlignment.Center,
                RuleLine = true,
            });

        NmInputDataPage.Render(rb, input);

        if (input.PetResult is not null && input.PetInput is not null)
        {
            NmCalcPages.RenderPet(rb, input.PetInput, input.PetResult);
        }

        if (input.GammaCameraResult is not null)
        {
            NmCalcPages.RenderGammaCamera(rb, input.GammaCameraResult);
        }

        if (input.HdtResult is not null)
        {
            NmCalcPages.RenderHdt(rb, input.HdtResult);
        }

        NmUndertakingPage.Render(rb, input);

        return rb.ToByteArray();
    }

    /// <summary>Generates the NM shielding report and saves it to a file.</summary>
    /// <param name="input">The complete report input.</param>
    /// <param name="outputPath">The file path to write the PDF to.</param>
    public void GenerateToFile(NmReportInput input, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(outputPath);
        System.IO.File.WriteAllBytes(outputPath, Generate(input));
    }
}
