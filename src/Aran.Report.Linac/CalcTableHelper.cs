using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Engines.Linac;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.Linac;

/// <summary>
/// Renders a <see cref="CalculationStep"/> list as a three-column table:
/// Description | Formula (symbolic) | Substituted = Result.
/// </summary>
internal static class CalcTableHelper
{
    /// <summary>Builds a calculation-trace table from a step list.</summary>
    internal static ReportTable Build(IReadOnlyList<CalculationStep> steps, bool pass)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ReportTable t = Styles.DataTable();
        t.AddColumn(new ReportColumn { Header = "Step", Width = 120, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Formula", Width = 160, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Substituted  =  Result", Width = 1, WidthMode = ColumnWidthMode.Fraction });

        foreach (CalculationStep step in steps)
        {
            ReportRow row = new ReportRow();
            row.Cells.Add(new ReportCell(step.Description));
            row.Cells.Add(new ReportCell
            {
                Text = step.Formula,
                Font = ReportFont.Courier,
                FontSize = 7.5,
            });
            row.Cells.Add(new ReportCell
            {
                Text = step.Substituted + "  =  " + FormatResult(step.Result),
                Font = ReportFont.Courier,
                FontSize = 7.5,
            });
            t.AddRow(row);
        }

        return t;
    }

    /// <summary>Adds a pass/fail summary row to an existing table.</summary>
    internal static void AddPassFailRow(ReportTable t, double requiredMm, double providedMm, bool pass)
    {
        ArgumentNullException.ThrowIfNull(t);
        ReportRow row = new ReportRow
        {
            Background = pass ? Styles.PassGreen : Styles.FailRed,
        };
        row.Cells.Add(new ReportCell
        {
            Text = pass ? "PASS" : "FAIL",
            Font = ReportFont.HelveticaBold,
            ColSpan = 1,
        });
        row.Cells.Add(new ReportCell
        {
            Text = "Required: " + requiredMm.ToString("0.#", CultureInfo.InvariantCulture) + " mm",
            Font = ReportFont.HelveticaBold,
        });
        row.Cells.Add(new ReportCell
        {
            Text = "Provided: " + providedMm.ToString("0.#", CultureInfo.InvariantCulture) + " mm",
            Font = ReportFont.HelveticaBold,
        });
        t.AddRow(row);
    }

    private static string FormatResult(CalculationTerm term)
    {
        string unit = string.IsNullOrWhiteSpace(term.Unit) ? "" : " " + term.Unit;
        double v = term.Value;
        string vStr;
        if (v == 0.0) { vStr = "0"; }
        else if (Math.Abs(v) < 1e-3 || Math.Abs(v) >= 1e5)
        {
            vStr = v.ToString("0.###e+00", CultureInfo.InvariantCulture);
        }
        else
        {
            vStr = v.ToString("0.####", CultureInfo.InvariantCulture);
        }

        return vStr + unit;
    }
}
