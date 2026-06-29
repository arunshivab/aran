using System;
using System.Globalization;
using Aran.Engines.Linac;
using Aran.Machines;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.Linac;

/// <summary>
/// Renders the AERB undertaking page (last page of the AERB report) or a
/// calculation-summary closing page (NCRP report).
/// </summary>
internal static class UndertakingPage
{
    /// <summary>Appends the appropriate closing page.</summary>
    internal static void Render(ReportBuilder rb, LinacReportInput input)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(input);

        rb.AddPageBreak();
        bool isAerb = input.WallResult.StandardName.Contains("AERB", StringComparison.OrdinalIgnoreCase);

        if (isAerb)
        {
            RenderAerbUndertaking(rb, input);
        }
        else
        {
            RenderNcrpSummary(rb, input);
        }
    }

    private static void RenderAerbUndertaking(ReportBuilder rb, LinacReportInput input)
    {
        rb.AddHeading("Undertaking — AERB Radiotherapy Facility", Styles.H1);
        rb.AddParagraph(
            "To be submitted along with the calculation sheet while submitting site and layout " +
            "application in eLORA. (AERB Technical Guidance for Shielding Calculations — Radiotherapy " +
            "Facilities, Section C.)",
            Styles.Note);
        rb.AddHorizontalRule(Styles.Navy, 0.8, 8);

        ReportTable t = Styles.InputTable();
        t.AddColumn(new ReportColumn { Header = "Field", Width = 180, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });

        t.AddRow(new[] { "Type of equipment and model", input.Machine.Name + " (" + input.Machine.Type + ")" });
        t.AddRow(new[] { "Photon energy (ies)", ModeEnergies(input) });
        t.AddRow(new[]
        {
            "Beam stopper (if available) — transmission factor",
            input.Machine.BeamStopperTransmission.HasValue
                ? input.Machine.BeamStopperTransmission.Value.ToString("0.###", CultureInfo.InvariantCulture) +
                  " (as provided by manufacturer)"
                : "Not applicable",
        });
        t.AddRow(new[] { "Facility name", input.FacilityName });
        t.AddRow(new[] { "Facility address", input.FacilityAddress });
        t.AddRow(new[] { "Floor", input.FacilityFloor });
        t.AddRow(new[] { "Drawing reference", input.DrawingReference });
        t.AddRow(new[] { "Date of calculation", input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) });
        rb.AddTable(t);

        rb.AddSpacer(6);
        rb.AddParagraph(
            "Shielding calculation is carried out taking workload, use factor as provided in AERB " +
            "guidelines and methodology as per NCRP Report 151 and found to be adequate from radiation " +
            "safety viewpoint for radiation workers and members of public. Detailed calculation sheet " +
            "is attached with this application.",
            Styles.Body);

        rb.AddSpacer(20);
        RenderSignatureBlock(rb, input);
    }

    private static void RenderNcrpSummary(ReportBuilder rb, LinacReportInput input)
    {
        rb.AddHeading("Calculation Summary — NCRP 151", Styles.H1);
        rb.AddParagraph(
            "Shielding calculations performed per NCRP Report No. 151 (2005) — Structural Shielding " +
            "Design and Evaluation for Megavoltage X- and Gamma-Ray Radiotherapy Facilities.",
            Styles.Body);
        rb.AddHorizontalRule(Styles.Navy, 0.5, 8);

        ReportTable t = Styles.DataTable();
        t.AddColumn(new ReportColumn { Header = "Barrier", Width = 80, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Role", Width = 80, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Required (mm)", Width = 100, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        t.AddColumn(new ReportColumn { Header = "Provided (mm)", Width = 100, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        t.AddColumn(new ReportColumn { Header = "Result", Width = 1, WidthMode = ColumnWidthMode.Fraction });

        foreach (LinacBarrierEvaluation b in input.WallResult.Barriers)
        {
            ReportRow row = new ReportRow
            {
                Background = b.Passes ? Styles.PassGreen : Styles.FailRed,
            };
            row.Cells.Add(new ReportCell(b.BarrierId));
            row.Cells.Add(new ReportCell(b.Role.ToString()));
            row.Cells.Add(new ReportCell
            {
                Text = b.RequiredThicknessMm.ToString("0.#", CultureInfo.InvariantCulture),
                Alignment = TextAlignment.Right,
            });
            row.Cells.Add(new ReportCell
            {
                Text = b.ProvidedThicknessMm.ToString("0.#", CultureInfo.InvariantCulture),
                Alignment = TextAlignment.Right,
            });
            row.Cells.Add(new ReportCell
            {
                Text = b.Passes ? "PASS" : "FAIL",
                Font = ReportFont.HelveticaBold,
            });
            t.AddRow(row);
        }

        rb.AddTable(t);

        // door summary
        rb.AddSpacer(4);
        ReportTable doorSum = Styles.DataTable();
        doorSum.AddColumn(new ReportColumn { Header = "Door", Width = 80, WidthMode = ColumnWidthMode.Points });
        doorSum.AddColumn(new ReportColumn { Header = "Bare dose (µSv/wk)", Width = 130, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        doorSum.AddColumn(new ReportColumn { Header = "Design goal (µSv/wk)", Width = 140, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        doorSum.AddColumn(new ReportColumn { Header = "Result", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        DoorEvaluation door = input.DoorResult;
        ReportRow dRow = new ReportRow
        {
            Background = door.BarePasses ? Styles.PassGreen : Styles.FailRed,
        };
        dRow.Cells.Add(new ReportCell(door.DoorId));
        dRow.Cells.Add(new ReportCell
        {
            Text = (door.BareDoseSvPerWeek * 1e6).ToString("0.##", CultureInfo.InvariantCulture),
            Alignment = TextAlignment.Right,
        });
        dRow.Cells.Add(new ReportCell
        {
            Text = (door.DesignGoalSvPerWeek * 1e6).ToString("0.##", CultureInfo.InvariantCulture),
            Alignment = TextAlignment.Right,
        });
        dRow.Cells.Add(new ReportCell
        {
            Text = door.BarePasses ? "PASS" : "FAIL — see door calc",
            Font = ReportFont.HelveticaBold,
        });
        doorSum.AddRow(dRow);
        rb.AddTable(doorSum);

        rb.AddSpacer(20);
        RenderSignatureBlock(rb, input);
    }

    private static void RenderSignatureBlock(ReportBuilder rb, LinacReportInput input)
    {
        rb.AddHorizontalRule(Styles.DarkGrey, 0.4, 4);
        rb.AddParagraph("Name and signature of Technical Expert for shielding calculation:", Styles.Body);
        rb.AddParagraph(input.PreparedBy, Styles.H3);
        rb.AddSpacer(16);
        rb.AddParagraph("eLORA RP Registration Number:  " + input.EloraRpId, Styles.Body);
        rb.AddSpacer(16);
        rb.AddParagraph("Name and signature of Employer:", Styles.Body);
        rb.AddSpacer(24);
        rb.AddParagraph("Seal of Institute:", Styles.Body);
        rb.AddSpacer(40);
        rb.AddParagraph(
            "Date: " + input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
            Styles.Body);
    }

    private static string ModeEnergies(LinacReportInput input)
    {
        System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
        foreach (BeamMode m in input.Machine.Modes)
        {
            parts.Add(m.NominalMv + " MV" + (m.Fff ? " FFF" : ""));
        }

        return string.Join(", ", parts);
    }
}
