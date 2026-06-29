using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Engines.Linac;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.Linac;

/// <summary>Renders wall-barrier calculation pages.</summary>
internal static class WallCalcPages
{
    /// <summary>Appends one section per barrier to the report.</summary>
    internal static void Render(ReportBuilder rb, LinacShieldingResult result)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(result);

        rb.AddPageBreak();
        rb.AddHeading("Wall Shielding Calculations — " + result.StandardName, Styles.H1);

        foreach (LinacBarrierEvaluation barrier in result.Barriers)
        {
            rb.AddHeading(
                "Barrier " + barrier.BarrierId + "  —  " + barrier.Role + "  —  " + barrier.Material,
                Styles.H2);

            // group components by mode
            Dictionary<string, List<ComponentResult>> byMode =
                new Dictionary<string, List<ComponentResult>>(StringComparer.Ordinal);
            foreach (ComponentResult comp in barrier.Components)
            {
                if (!byMode.TryGetValue(comp.BeamModeName, out List<ComponentResult>? list))
                {
                    list = new List<ComponentResult>();
                    byMode[comp.BeamModeName] = list;
                }

                list.Add(comp);
            }

            foreach (KeyValuePair<string, List<ComponentResult>> modeGroup in byMode)
            {
                rb.AddHeading(modeGroup.Key + " (" + modeGroup.Value[0].EnergyMv + " MV)", Styles.H3);
                foreach (ComponentResult comp in modeGroup.Value)
                {
                    rb.AddParagraph(comp.Kind.ToString(), Styles.BodySmall);
                    ReportTable t = CalcTableHelper.Build(comp.Steps, barrier.Passes);
                    foreach (string note in comp.Notes)
                    {
                        ReportRow noteRow = new ReportRow
                        {
                            Background = Styles.LightGrey,
                        };
                        noteRow.Cells.Add(new ReportCell
                        {
                            Text = "Note: " + note,
                            ColSpan = 3,
                            Font = ReportFont.Helvetica,
                            FontSize = 7,
                        });
                        t.AddRow(noteRow);
                    }

                    rb.AddTable(t);
                }
            }

            // controlling result summary
            ReportTable summary = Styles.DataTable();
            summary.AddColumn(new ReportColumn { Header = "Result", Width = 100, WidthMode = ColumnWidthMode.Points });
            summary.AddColumn(new ReportColumn { Header = "Required (mm)", Width = 110, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
            summary.AddColumn(new ReportColumn { Header = "Provided (mm)", Width = 110, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
            summary.AddColumn(new ReportColumn { Header = "Governing component", Width = 1, WidthMode = ColumnWidthMode.Fraction });
            ReportRow sumRow = new ReportRow
            {
                Background = barrier.Passes ? Styles.PassGreen : Styles.FailRed,
            };
            sumRow.Cells.Add(new ReportCell
            {
                Text = barrier.Passes ? "PASS" : "FAIL",
                Font = ReportFont.HelveticaBold,
            });
            sumRow.Cells.Add(new ReportCell
            {
                Text = barrier.RequiredThicknessMm.ToString("0.#", CultureInfo.InvariantCulture),
                Font = ReportFont.HelveticaBold,
                Alignment = TextAlignment.Right,
            });
            sumRow.Cells.Add(new ReportCell
            {
                Text = barrier.ProvidedThicknessMm.ToString("0.#", CultureInfo.InvariantCulture),
                Alignment = TextAlignment.Right,
            });
            sumRow.Cells.Add(new ReportCell { Text = barrier.GoverningComponent });
            summary.AddRow(sumRow);
            rb.AddTable(summary);
            rb.AddHorizontalRule(Styles.Navy, 0.5, 4);
        }
    }
}
