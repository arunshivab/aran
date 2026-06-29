using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Engines.NuclearMedicine;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.NuclearMedicine;

/// <summary>Renders per-modality calculation pages.</summary>
internal static class NmCalcPages
{
    internal static void RenderPet(ReportBuilder rb, PetShieldingInput input, PetShieldingResult result)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(result);

        rb.AddPageBreak();
        rb.AddHeading("PET / PET-CT Shielding Calculations — AERB (AAPM TG-108)", NmStyles.H1);
        rb.AddParagraph(
            "F-18 basis · " + input.PatientsPerWeek + " patients/week · " +
            Fmt(input.AdministeredActivityMbq) + " MBq/patient · uptake " +
            Fmt(input.UptakeTimeMin) + " min · imaging " + Fmt(input.ImagingTimeMin) + " min",
            NmStyles.Body);

        foreach (PetBarrierResult barrier in result.Barriers)
        {
            rb.AddHeading(barrier.BarrierId + " — " + barrier.RoomType + " Room", NmStyles.H2);
            rb.AddTable(NmCalcTableHelper.Build(barrier.Steps));

            // Summary: concrete + lead + pass/fail
            ReportTable summary = NmStyles.DataTable();
            summary.AddColumn(new ReportColumn { Header = "Material", Width = 100, WidthMode = ColumnWidthMode.Points });
            summary.AddColumn(new ReportColumn { Header = "Required (mm)", Width = 110, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
            summary.AddColumn(new ReportColumn { Header = "Provided (mm)", Width = 110, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
            summary.AddColumn(new ReportColumn { Header = "Result", Width = 1, WidthMode = ColumnWidthMode.Fraction });
            summary.AddRow(new[] { "Concrete (2.35 g/cm³)", Fmt(barrier.RequiredConcreteMm), "—", "required thickness" });
            summary.AddRow(new[] { "Lead", Fmt(barrier.RequiredLeadMm), "—", "required thickness" });
            ReportRow verdict = new ReportRow
            {
                Background = barrier.Passes ? NmStyles.PassGreen : NmStyles.FailRed,
            };
            verdict.Cells.Add(new ReportCell
            {
                Text = barrier.ProvidedMaterial,
                Font = ReportFont.HelveticaBold,
            });
            verdict.Cells.Add(new ReportCell
            {
                Text = barrier.Passes ? "—" : Fmt(barrier.Passes ? 0 : barrier.ProvidedMaterial == "Lead" ? barrier.RequiredLeadMm : barrier.RequiredConcreteMm),
                Alignment = TextAlignment.Right,
            });
            verdict.Cells.Add(new ReportCell
            {
                Text = Fmt(barrier.ProvidedThicknessMm),
                Alignment = TextAlignment.Right,
            });
            verdict.Cells.Add(new ReportCell
            {
                Text = barrier.Passes ? "PASS" : "FAIL",
                Font = ReportFont.HelveticaBold,
            });
            summary.AddRow(verdict);
            rb.AddTable(summary);

            foreach (string note in barrier.Notes)
            {
                rb.AddParagraph("Note: " + note, NmStyles.Note);
            }

            rb.AddHorizontalRule(NmStyles.Navy, 0.5, 4);
        }
    }

    internal static void RenderGammaCamera(ReportBuilder rb, GammaCameraShieldingResult result)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(result);

        rb.AddPageBreak();
        rb.AddHeading("Gamma Camera / SPECT Shielding Calculations — AERB", NmStyles.H1);
        rb.AddParagraph("Tc-99m basis · AERB NM guidance §B.1", NmStyles.Body);

        foreach (GammaCameraBarrierResult barrier in result.Barriers)
        {
            rb.AddHeading(barrier.BarrierId, NmStyles.H2);
            rb.AddTable(NmCalcTableHelper.Build(barrier.Steps));

            ReportTable summary = NmStyles.DataTable();
            summary.AddColumn(new ReportColumn { Header = "Item", Width = 160, WidthMode = ColumnWidthMode.Points });
            summary.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });
            summary.AddRow(new[] { "Weekly dose at barrier point", Fmt(barrier.WeeklyDoseSvAtBarrier * 1e6) + " µSv/week" });
            summary.AddRow(new[] { "AERB design goal", Fmt(barrier.DesignGoalSv * 1e6) + " µSv/week" });
            summary.AddRow(new[] { "AERB adequate thickness", Fmt(barrier.AerbAdequateMm) + " mm" });
            summary.AddRow(new[] { "Provided thickness", Fmt(barrier.ProvidedThicknessMm) + " mm" });
            ReportRow verdict = new ReportRow
            {
                Background = barrier.Passes ? NmStyles.PassGreen : NmStyles.FailRed,
            };
            verdict.Cells.Add(new ReportCell
            {
                Text = barrier.Passes ? "PASS — " + barrier.AerbStatement : "FAIL",
                Font = ReportFont.HelveticaBold,
                ColSpan = 2,
            });
            summary.AddRow(verdict);
            rb.AddTable(summary);

            foreach (string note in barrier.Notes)
            {
                rb.AddParagraph("Note: " + note, NmStyles.Note);
            }

            rb.AddHorizontalRule(NmStyles.Navy, 0.5, 4);
        }
    }

    internal static void RenderHdt(ReportBuilder rb, HdtShieldingResult result)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(result);

        rb.AddPageBreak();
        rb.AddHeading("High Dose Therapy (HDT) I-131 Shielding Calculations — AERB", NmStyles.H1);
        rb.AddParagraph("I-131 · AERB NM guidance §B.3 · TVL: concrete 10 cm, lead 1 cm", NmStyles.Body);

        foreach (HdtBarrierResult barrier in result.Barriers)
        {
            rb.AddHeading(barrier.BarrierId, NmStyles.H2);
            rb.AddTable(NmCalcTableHelper.Build(barrier.Steps));
            NmCalcTableHelper.AddPassFailRow(
                NmCalcTableHelper.Build(new List<NmCalcStep>()),
                barrier.RequiredThicknessMm,
                barrier.ProvidedThicknessMm,
                barrier.Passes);

            ReportTable summary = NmStyles.DataTable();
            summary.AddColumn(new ReportColumn { Header = "Result", Width = 80, WidthMode = ColumnWidthMode.Points });
            summary.AddColumn(new ReportColumn { Header = "Required (mm)", Width = 110, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
            summary.AddColumn(new ReportColumn { Header = "Provided (mm)", Width = 1, WidthMode = ColumnWidthMode.Fraction, Alignment = TextAlignment.Right });
            ReportRow row = new ReportRow
            {
                Background = barrier.Passes ? NmStyles.PassGreen : NmStyles.FailRed,
            };
            row.Cells.Add(new ReportCell
            {
                Text = barrier.Passes ? "PASS" : "FAIL",
                Font = ReportFont.HelveticaBold,
            });
            row.Cells.Add(new ReportCell
            {
                Text = Fmt(barrier.RequiredThicknessMm),
                Alignment = TextAlignment.Right,
            });
            row.Cells.Add(new ReportCell
            {
                Text = Fmt(barrier.ProvidedThicknessMm),
                Alignment = TextAlignment.Right,
            });
            summary.AddRow(row);
            rb.AddTable(summary);

            foreach (string note in barrier.Notes)
            {
                rb.AddParagraph("Note: " + note, NmStyles.Note);
            }

            rb.AddHorizontalRule(NmStyles.Navy, 0.5, 4);
        }
    }

    private static string Fmt(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);
}
