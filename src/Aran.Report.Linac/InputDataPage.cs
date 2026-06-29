using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Engines.Linac;
using Aran.Machines;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.Linac;

/// <summary>Renders the input-data summary page (page 1 of each report).</summary>
internal static class InputDataPage
{
    /// <summary>Appends the input-data page to the report builder.</summary>
    internal static void Render(ReportBuilder rb, LinacReportInput input)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(input);

        rb.AddHeading("Shielding Calculation Report — " + input.WallResult.StandardName, Styles.H1);
        rb.AddParagraph(input.FacilityName, Styles.H2);
        rb.AddParagraph(input.FacilityAddress + " · " + input.FacilityFloor, Styles.Body);
        rb.AddParagraph(
            "Drawing: " + input.DrawingReference + "   |   " +
            "Prepared: " + input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + "   |   " +
            "Standard: " + input.WallResult.StandardName,
            Styles.BodySmall);
        rb.AddHorizontalRule(Styles.Navy, 0.8, 6);

        // --- Facility & machine ---
        rb.AddHeading("A.  Facility & Machine", Styles.H2);
        ReportTable facTable = FacilityTable(input);
        rb.AddTable(facTable);

        // --- Design parameters ---
        rb.AddHeading("B.  Design Parameters", Styles.H2);
        rb.AddTable(DesignTable(input));

        // --- Barrier inventory ---
        rb.AddHeading("C.  Barrier Inventory", Styles.H2);
        rb.AddTable(BarrierTable(input));

        // --- Maze geometry ---
        rb.AddHeading("D.  Maze & Door Geometry", Styles.H2);
        rb.AddTable(MazeTable(input.Maze));

        // --- Workloads ---
        rb.AddHeading("E.  Workloads", Styles.H2);
        rb.AddTable(WorkloadTable(input));

        // --- Neutron source note ---
        rb.AddSpacer(4);
        rb.AddParagraph(
            "Neutron source: " + NeutronCatalog.ForMachine(input.Machine.Name)?.Citation ??
            "No neutron source tabulated for this machine.",
            Styles.Note);
    }

    private static ReportTable FacilityTable(LinacReportInput input)
    {
        ReportTable t = Styles.InputTable();
        t.AddColumn(new ReportColumn { Header = "Parameter", Width = 160, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        t.AddRow(new[] { "Machine model", input.Machine.Name });
        t.AddRow(new[] { "Machine type", input.Machine.Type.ToString() });
        t.AddRow(new[] { "Beam modes", string.Join(", ", ModeNames(input.Machine)) });
        t.AddRow(new[] { "Beam stopper transmission",
            input.Machine.BeamStopperTransmission.HasValue
                ? input.Machine.BeamStopperTransmission.Value.ToString("0.###", CultureInfo.InvariantCulture)
                : "None" });
        t.AddRow(new[] { "Shielding material", "Ordinary concrete 2.35 g/cm³" });
        return t;
    }

    private static ReportTable DesignTable(LinacReportInput input)
    {
        ReportTable t = Styles.InputTable();
        t.AddColumn(new ReportColumn { Header = "Parameter", Width = 160, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Controlled area", Width = 140, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Uncontrolled area", Width = 1, WidthMode = ColumnWidthMode.Fraction });

        string stdName = input.WallResult.StandardName;
        if (stdName.Contains("NCRP", StringComparison.OrdinalIgnoreCase))
        {
            t.AddRow(new[] { "Design goal (P)", "0.1 mSv/week", "0.02 mSv/week" });
            t.AddRow(new[] { "Occupancy (T)", "Graded — NCRP 151 Table B.1", "Graded — NCRP 151 Table B.1" });
            t.AddRow(new[] { "Use factor U (primary)", "Designer-specified", "—" });
            t.AddRow(new[] { "Neutron threshold", "> 10 MV", "—" });
        }
        else
        {
            t.AddRow(new[] { "Design goal (P)", "400 µSv/week", "20 µSv/week" });
            t.AddRow(new[] { "Occupancy (T)", "1 (all areas)", "1 (all areas)" });
            t.AddRow(new[] { "Use factor U (primary)", "0.25 (linac) / 0.12 (O-ring)", "—" });
            t.AddRow(new[] { "Neutron threshold", "≥ 10 MV", "—" });
        }

        t.AddRow(new[] { "TVL / scatter tables", "NCRP 151 Appendix B", "NCRP 151 Appendix B" });
        return t;
    }

    private static ReportTable BarrierTable(LinacReportInput input)
    {
        ReportTable t = Styles.DataTable();
        t.AddColumn(new ReportColumn { Header = "Barrier", Width = 60, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Role", Width = 70, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Material", Width = 80, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Provided (mm)", Width = 90, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        t.AddColumn(new ReportColumn { Header = "Protected area", Width = 80, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Occupancy", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        foreach (LinacBarrierEvaluation barrier in input.WallResult.Barriers)
        {
            t.AddRow(new[]
            {
                barrier.BarrierId,
                barrier.Role.ToString(),
                barrier.Material.ToString(),
                barrier.ProvidedThicknessMm.ToString("0", CultureInfo.InvariantCulture),
                barrier.BarrierId,
                "—",
            });
        }

        return t;
    }

    private static ReportTable MazeTable(MazeRun maze)
    {
        ReportTable t = Styles.InputTable();
        t.AddColumn(new ReportColumn { Header = "Quantity", Width = 160, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Symbol", Width = 60, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Value", Width = 80, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        t.AddColumn(new ReportColumn { Header = "Unit", Width = 1, WidthMode = ColumnWidthMode.Fraction });

        MazePhotonGeometry g = maze.Photon;
        t.AddRow(new[] { "Target to Wall G (perp.)", "dh", F(g.Dh), "m" });
        t.AddRow(new[] { "First-scatter to maze CL point b", "dr", F(g.Dr), "m" });
        t.AddRow(new[] { "Point b to door (centreline)", "dz", F(g.Dz), "m" });
        t.AddRow(new[] { "Target to Wall G (oblique)", "dsec", F(g.Dsec), "m" });
        t.AddRow(new[] { "Scatter surface A1 to door", "dzz", F(g.Dzz), "m" });
        t.AddRow(new[] { "Target to patient", "dsca", F(g.Dsca), "m" });
        t.AddRow(new[] { "Target to door (oblique)", "dL", F(g.DL), "m" });
        t.AddRow(new[] { "Primary beam area at Wall G", "A0", F(g.A0), "m²" });
        t.AddRow(new[] { "Wall G area visible from door", "A1", F(g.A1), "m²" });
        t.AddRow(new[] { "Inner maze opening area (projected)", "Az", F(g.Az), "m²" });
        t.AddRow(new[] { "Inner wall transmission", "B", g.InnerWallTransmissionB.ToString("0.##e+0", CultureInfo.InvariantCulture), "" });
        t.AddRow(new[] { "Patient scatter angle", "θ", F(maze.ScatterAngleDegrees), "°" });
        t.AddRow(new[] { "Field area at 1 m", "F", F(maze.FieldAreaCm2), "cm²" });
        t.AddRow(new[] { "Patient transmission", "f", F(maze.PatientTransmissionF), "" });

        if (maze.Neutron is MazeNeutronGeometry ng)
        {
            t.AddRow(new[] { "Isocentre to inner maze point", "d1", F(ng.D1), "m" });
            t.AddRow(new[] { "Inner maze point to door", "d2", F(ng.D2), "m" });
            t.AddRow(new[] { "Room surface area", "Sr", F(ng.RoomSurfaceAreaM2), "m²" });
            t.AddRow(new[] { "Inner maze opening area", "S0", F(ng.S0), "m²" });
            t.AddRow(new[] { "Maze cross-section area", "S1", F(ng.S1), "m²" });
        }

        return t;
    }

    private static ReportTable WorkloadTable(LinacReportInput input)
    {
        ReportTable t = Styles.DataTable();
        t.AddColumn(new ReportColumn { Header = "Mode", Width = 70, WidthMode = ColumnWidthMode.Points });
        t.AddColumn(new ReportColumn { Header = "Nominal MV", Width = 80, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        t.AddColumn(new ReportColumn { Header = "Primary W (Gy/wk)", Width = 120, WidthMode = ColumnWidthMode.Points, Alignment = TextAlignment.Right });
        t.AddColumn(new ReportColumn { Header = "Source", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        foreach (EnergyWorkload wl in input.Input.Workloads)
        {
            t.AddRow(new[] { wl.ModeName, "—", F(wl.PrimaryGyPerWeek), "Physicist-supplied" });
        }

        return t;
    }

    private static IEnumerable<string> ModeNames(MachineModel machine)
    {
        List<string> names = new List<string>();
        foreach (BeamMode m in machine.Modes)
        {
            names.Add(m.Name + " (" + m.NominalMv + " MV)");
        }

        return names;
    }

    private static string F(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
}
