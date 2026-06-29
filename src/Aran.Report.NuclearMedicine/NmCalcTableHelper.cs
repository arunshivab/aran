using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Engines.NuclearMedicine;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.NuclearMedicine;

/// <summary>Renders a list of <see cref="NmCalcStep"/> as a three-column calculation trace table.</summary>
internal static class NmCalcTableHelper
{
    internal static ReportTable Build(IReadOnlyList<NmCalcStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ReportTable t = NmStyles.DataTable();
        t.AddColumn(new ReportColumn { Header = "Step", Width = 130, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Formula", Width = 160, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Substituted  =  Result", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        foreach (NmCalcStep step in steps)
        {
            ReportRow row = new ReportRow();
            row.Cells.Add(new ReportCell(step.Description));
            row.Cells.Add(new ReportCell { Text = step.Formula, Font = ReportFont.Courier, FontSize = 7.5 });
            row.Cells.Add(new ReportCell
            {
                Text = step.Substituted + "  =  " + step.Result,
                Font = ReportFont.Courier,
                FontSize = 7.5,
            });
            t.AddRow(row);
        }

        return t;
    }

    internal static void AddPassFailRow(ReportTable t, double requiredMm, double providedMm, bool pass)
    {
        ArgumentNullException.ThrowIfNull(t);
        ReportRow row = new ReportRow { Background = pass ? NmStyles.PassGreen : NmStyles.FailRed };
        row.Cells.Add(new ReportCell
        {
            Text = pass ? "PASS" : "FAIL",
            Font = ReportFont.HelveticaBold,
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
}
