using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Engines.Linac;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.Linac;

/// <summary>Renders door/maze calculation pages.</summary>
internal static class DoorCalcPages
{
    /// <summary>Appends the door calculation section to the report.</summary>
    internal static void Render(ReportBuilder rb, DoorEvaluation door)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(door);

        rb.AddPageBreak();
        rb.AddHeading("Door / Maze Calculations — " + door.StandardName, Styles.H1);
        rb.AddParagraph("Door: " + door.DoorId, Styles.Body);

        // group components by mode then kind
        Dictionary<string, List<DoorComponentResult>> byMode =
            new Dictionary<string, List<DoorComponentResult>>(StringComparer.Ordinal);
        foreach (DoorComponentResult comp in door.Components)
        {
            if (!byMode.TryGetValue(comp.BeamModeName, out List<DoorComponentResult>? list))
            {
                list = new List<DoorComponentResult>();
                byMode[comp.BeamModeName] = list;
            }

            list.Add(comp);
        }

        foreach (KeyValuePair<string, List<DoorComponentResult>> modeGroup in byMode)
        {
            rb.AddHeading(modeGroup.Key + " (" + modeGroup.Value[0].EnergyMv + " MV)", Styles.H2);
            foreach (DoorComponentResult comp in modeGroup.Value)
            {
                rb.AddParagraph(ComponentLabel(comp.Kind), Styles.H3);
                ReportTable t = CalcTableHelper.Build(comp.Steps, door.BarePasses);
                foreach (string note in comp.Notes)
                {
                    ReportRow noteRow = new ReportRow { Background = Styles.LightGrey };
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

        // bare-door total
        rb.AddHeading("Total Dose at Door", Styles.H2);
        ReportTable totTable = Styles.DataTable();
        totTable.AddColumn(new ReportColumn { Header = "Quantity", Width = 160, WidthMode = ColumnWidthMode.Points });
        totTable.AddColumn(new ReportColumn { Header = "Value", Width = 120, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        totTable.AddColumn(new ReportColumn { Header = "Unit", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        totTable.AddRow(new[] { "Total bare dose (HW)", Fmt(door.BareDoseSvPerWeek * 1e6), "µSv/week" });
        totTable.AddRow(new[] { "Design goal (P·T)", Fmt(door.DesignGoalSvPerWeek * 1e6), "µSv/week" });
        ReportRow verdict = new ReportRow
        {
            Background = door.BarePasses ? Styles.PassGreen : Styles.FailRed,
        };
        verdict.Cells.Add(new ReportCell
        {
            Text = door.BarePasses ? "BARE DOOR PASSES" : "BARE DOOR FAILS — shielding required",
            Font = ReportFont.HelveticaBold,
            ColSpan = 3,
        });
        totTable.AddRow(verdict);
        rb.AddTable(totTable);

        // sandwich shielding if required
        if (door.Shielding is DoorShielding sh)
        {
            rb.AddHeading("Door Shielding — Lead / BPE Sandwich", Styles.H2);
            rb.AddTable(CalcTableHelper.Build(sh.Steps, true));
            ReportTable shTable = Styles.DataTable();
            shTable.AddColumn(new ReportColumn { Header = "Layer", Width = 120, WidthMode = ColumnWidthMode.Points });
            shTable.AddColumn(new ReportColumn { Header = "Thickness (mm)", Width = 120, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
            shTable.AddColumn(new ReportColumn { Header = "Purpose", Width = 1, WidthMode = ColumnWidthMode.Fraction });
            shTable.AddRow(new[] { "Lead", sh.LeadMm.ToString("0.#", CultureInfo.InvariantCulture), "3.6 MeV capture gamma attenuation" });
            shTable.AddRow(new[] { "Borated polyethylene (BPE)", sh.BpeMm.ToString("0.#", CultureInfo.InvariantCulture), "Neutron moderation and absorption" });
            shTable.AddRow(new[] { "Lead (outer)", sh.LeadMm.ToString("0.#", CultureInfo.InvariantCulture), "Capture gamma from BPE (symmetric sandwich)" });
            rb.AddTable(shTable);
            foreach (string note in sh.Notes)
            {
                rb.AddParagraph("Note: " + note, Styles.Note);
            }
        }
    }

    private static string ComponentLabel(DoorComponentKind kind) => kind switch
    {
        DoorComponentKind.PrimaryScatterHs => "HS — Primary scatter from Wall G (Eq 2.9)",
        DoorComponentKind.LeakageScatterHls => "HLS — Leakage scatter from Wall G (Eq 2.10)",
        DoorComponentKind.PatientScatterHps => "Hps — Patient scatter (Eq 2.11)",
        DoorComponentKind.LeakageTransmissionHlt => "HLT — Leakage through inner maze wall (Eq 2.12)",
        DoorComponentKind.CaptureGammaHcg => "Hcg — Neutron capture gamma (Eq 2.15–2.17)",
        DoorComponentKind.NeutronHn => "Hn — Neutrons, Wu–McGinley (Eq 2.19–2.21)",
        _ => kind.ToString(),
    };

    private static string Fmt(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);
}
