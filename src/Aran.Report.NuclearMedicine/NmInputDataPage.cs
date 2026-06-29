using System;
using System.Globalization;
using Chuvadi.Pdf.Authoring;

namespace Aran.Report.NuclearMedicine;

/// <summary>Renders the input-data summary page (page 1 of the NM report).</summary>
internal static class NmInputDataPage
{
    internal static void Render(ReportBuilder rb, NmReportInput input)
    {
        ArgumentNullException.ThrowIfNull(rb);
        ArgumentNullException.ThrowIfNull(input);

        rb.AddHeading("Shielding Calculation Report — Nuclear Medicine Facility — AERB", NmStyles.H1);
        rb.AddParagraph(input.FacilityName, NmStyles.H2);
        rb.AddParagraph(input.FacilityAddress + " · " + input.FacilityFloor, NmStyles.Body);
        rb.AddParagraph(
            "Drawing: " + input.DrawingReference + "   |   " +
            "Prepared: " + input.PreparedDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) +
            "   |   Standard: AERB",
            NmStyles.BodySmall);
        rb.AddHorizontalRule(NmStyles.Navy, 0.8, 6);

        rb.AddHeading("A.  Facility & Installation", NmStyles.H2);
        ReportTable fac = NmStyles.InputTable();
        fac.AddColumn(new ReportColumn { Header = "Parameter", Width = 160, WidthMode = ColumnWidthMode.Points });
        fac.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        fac.AddRow(new[] { "Installation type(s)", input.InstallationTypes });
        fac.AddRow(new[] { "Shielding material", "Ordinary concrete 2.35 g/cm³ (primary); lead as applicable" });
        fac.AddRow(new[] { "Occupancy factor (T)", "1 — all areas (AERB NM guidance §A.5)" });
        fac.AddRow(new[] { "Use factor (U)", "1 — all barriers (isotropic source)" });
        rb.AddTable(fac);

        rb.AddHeading("B.  Design Goals", NmStyles.H2);
        ReportTable goals = NmStyles.InputTable();
        goals.AddColumn(new ReportColumn { Header = "Area", Width = 160, WidthMode = ColumnWidthMode.Points });
        goals.AddColumn(new ReportColumn { Header = "Design goal", Width = 1, WidthMode = ColumnWidthMode.Fraction });
        goals.AddRow(new[] { "Radiation workers (controlled)", "400 µSv/week (AERB NM guidance §A.3)" });
        goals.AddRow(new[] { "Members of public (uncontrolled)", "20 µSv/week (AERB NM guidance §A.3)" });
        rb.AddTable(goals);

        if (input.PetInput is not null)
        {
            rb.AddHeading("C.  PET / PET-CT Parameters", NmStyles.H2);
            ReportTable pet = NmStyles.InputTable();
            pet.AddColumn(new ReportColumn { Header = "Parameter", Width = 200, WidthMode = ColumnWidthMode.Points });
            pet.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });
            pet.AddRow(new[] { "Basis radionuclide", "F-18 FDG (AERB NM guidance §B.2)" });
            pet.AddRow(new[] { "Dose rate constant Γ (F-18)", "0.143 µSv·m²·MBq⁻¹·h⁻¹ (AAPM TG-108 Table II)" });
            pet.AddRow(new[] { "Patient attenuation factor", "0.36 (AAPM TG-108 §Patient attenuation)" });
            pet.AddRow(new[] { "Administered activity (Ao)", Fmt(input.PetInput.AdministeredActivityMbq) + " MBq" });
            pet.AddRow(new[] { "Uptake time (tU)", Fmt(input.PetInput.UptakeTimeMin) + " min" });
            pet.AddRow(new[] { "Imaging time (tI)", Fmt(input.PetInput.ImagingTimeMin) + " min" });
            pet.AddRow(new[] { "Patients per week (Nw)", input.PetInput.PatientsPerWeek.ToString(CultureInfo.InvariantCulture) });
            pet.AddRow(new[] { "F-18 half-life (T½)", "109.8 min" });
            pet.AddRow(new[] { "Method", "AAPM TG-108 Eq 4 (uptake room) + Eq 10 (imaging room)" });
            pet.AddRow(new[] { "Thickness lookup", "TG-108 Table IV log-linear interpolation (concrete + lead)" });
            rb.AddTable(pet);
        }

        if (input.GammaCameraInput is not null)
        {
            rb.AddHeading("D.  Gamma Camera / SPECT Parameters", NmStyles.H2);
            ReportTable gc = NmStyles.InputTable();
            gc.AddColumn(new ReportColumn { Header = "Parameter", Width = 200, WidthMode = ColumnWidthMode.Points });
            gc.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });
            gc.AddRow(new[] { "Basis radionuclide", "Tc-99m (AERB NM guidance §B.1)" });
            gc.AddRow(new[] { "Specific gamma-ray constant (Γ)", "0.078 mR·h⁻¹·mCi⁻¹ at 1 m (AERB NM guidance §B.1.2)" });
            gc.AddRow(new[] { "Activity per patient", Fmt(input.GammaCameraInput.ActivityMbqPerPatient) + " MBq" });
            gc.AddRow(new[] { "Patients per week", input.GammaCameraInput.PatientsPerWeek.ToString(CultureInfo.InvariantCulture) });
            gc.AddRow(new[] { "Imaging time per patient", Fmt(input.GammaCameraInput.ImagingTimeHoursPerPatient) + " h" });
            gc.AddRow(new[] { "AERB adequate shielding", "23 cm brick (1.65 g/cm³) or 15 cm concrete (2.35 g/cm³)" });
            rb.AddTable(gc);
        }

        if (input.HdtInput is not null)
        {
            rb.AddHeading("E.  High Dose Therapy (HDT) I-131 Parameters", NmStyles.H2);
            ReportTable hdt = NmStyles.InputTable();
            hdt.AddColumn(new ReportColumn { Header = "Parameter", Width = 200, WidthMode = ColumnWidthMode.Points });
            hdt.AddColumn(new ReportColumn { Header = "Value", Width = 1, WidthMode = ColumnWidthMode.Fraction });
            hdt.AddRow(new[] { "Radioisotope", "I-131 (AERB NM guidance §B.3)" });
            hdt.AddRow(new[] { "Specific gamma-ray constant (Γ)", "0.22 mR·h⁻¹·mCi⁻¹ at 1 m (AERB NM guidance §B.3.1)" });
            hdt.AddRow(new[] { "Weekly activity", Fmt(input.HdtInput.WeeklyActivityMbq) + " MBq (" + Fmt(input.HdtInput.WeeklyActivityMbq / 37.0) + " mCi)" });
            hdt.AddRow(new[] { "AERB maximum weekly activity", "300 mCi = 11100 MBq (AERB NM guidance §B.3.1)" });
            hdt.AddRow(new[] { "Occupancy hours per week", Fmt(input.HdtInput.OccupancyHoursPerWeek) + " h" });
            hdt.AddRow(new[] { "TVL (concrete)", "10 cm (AERB NM guidance §B.3.2)" });
            hdt.AddRow(new[] { "TVL (lead)", "1 cm (AERB NM guidance §B.3.2)" });
            rb.AddTable(hdt);
        }
    }

    private static string Fmt(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
}
