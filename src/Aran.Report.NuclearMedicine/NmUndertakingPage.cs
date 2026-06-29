using System;
using System.Globalization;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.NuclearMedicine;

/// <summary>Renders the AERB NM undertaking page (last page of the report).</summary>
internal static class NmUndertakingPage
{
    internal static void Render(ReportBuilder rb, NmReportInput input)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(input);

        rb.AddPageBreak();
        rb.AddHeading("Undertaking — AERB Nuclear Medicine Facility", NmStyles.H1);
        rb.AddParagraph(
            "To be submitted along with the shielding calculation sheet while submitting " +
            "site and layout application in eLORA. (AERB Technical Guidance for Shielding " +
            "Calculations — Nuclear Medicine Facilities, Section C.)",
            NmStyles.Note);
        rb.AddHorizontalRule(NmStyles.Navy, 0.8, 8);

        ReportTable t = NmStyles.InputTable();
        t.AddColumn(new ReportColumn { Header = "Field", Width = 200, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        t.AddRow(new[] { "Type of Installation", input.InstallationTypes });
        if (input.PetInput is not null)
        {
            t.AddRow(new[] { "Proposed workload (PET)", input.PetInput.PatientsPerWeek + " patients/week" });
        }

        if (input.HdtInput is not null)
        {
            int wards = 1;
            t.AddRow(new[] { "No. of HDT isolation wards", wards.ToString(CultureInfo.InvariantCulture) });
        }

        t.AddRow(new[] { "Facility name", input.FacilityName });
        t.AddRow(new[] { "Facility address", input.FacilityAddress });
        t.AddRow(new[] { "Floor", input.FacilityFloor });
        t.AddRow(new[] { "Drawing reference", input.DrawingReference });
        t.AddRow(new[] { "Date of calculation", input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) });
        rb.AddTable(t);

        rb.AddSpacer(6);

        if (input.PetInput is not null)
        {
            rb.AddParagraph(
                "(a) Shielding calculation is carried out as per AAPM Task Group Report No. 108 " +
                "(TG-108) for PET/PET-CT/PET-MR facility and found to be adequate from radiation " +
                "safety viewpoint for radiation workers and members of public.",
                NmStyles.Body);
        }

        if (input.GammaCameraInput is not null)
        {
            rb.AddParagraph(
                "(b) Shielding calculation for Gamma Camera/SPECT/SPECT-CT facility is carried " +
                "out as per the AERB guidance document and found to be adequate from radiation " +
                "safety viewpoint for radiation workers and members of public.",
                NmStyles.Body);
        }

        if (input.HdtInput is not null)
        {
            rb.AddParagraph(
                "(c) Shielding calculation for HDT facility is carried out as per the AERB guidance " +
                "document and found to be adequate from radiation safety viewpoint for radiation " +
                "workers and members of public.",
                NmStyles.Body);
        }

        rb.AddParagraph(
            "Detailed calculation sheet is attached with this application. All associated rooms " +
            "required for NM installations are provided in the layouts.",
            NmStyles.Body);

        rb.AddSpacer(20);
        rb.AddHorizontalRule(NmStyles.Navy, 0.4, 4);
        rb.AddParagraph("Name and signature of Technical Expert for shielding calculation:", NmStyles.Body);
        rb.AddParagraph(input.PreparedBy, NmStyles.H3);
        rb.AddSpacer(16);
        rb.AddParagraph("eLORA RP Registration Number:  " + input.EloraRpId, NmStyles.Body);
        rb.AddSpacer(16);
        rb.AddParagraph("Name and signature of Employer:", NmStyles.Body);
        rb.AddSpacer(24);
        rb.AddParagraph("Seal of Institute:", NmStyles.Body);
        rb.AddSpacer(40);
        rb.AddParagraph(
            "Date: " + input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
            NmStyles.Body);
    }
}
