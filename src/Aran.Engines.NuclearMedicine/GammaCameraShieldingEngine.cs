using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Model;

namespace Aran.Engines.NuclearMedicine;

/// <summary>
/// Evaluates Gamma Camera / SPECT / SPECT-CT facility shielding using the
/// specific dose constant method (AERB NM guidance §B.1).
///
/// Basis radionuclide: Tc-99m (Γ = 0.078 mR·h⁻¹·mCi⁻¹ at 1 m).
/// AERB states 23 cm brick (1.65 g/cm³) or 15 cm concrete (2.35 g/cm³) is
/// adequate for all barriers of a Gamma Camera / SPECT / SPECT-CT facility.
/// The engine calculates the dose at each barrier point and then confirms
/// whether the provided thickness meets the AERB simplified criterion.
/// </summary>
public sealed class GammaCameraShieldingEngine
{
    /// <summary>Evaluates all barriers for the given input.</summary>
    /// <param name="input">The Gamma Camera shielding input.</param>
    /// <returns>The evaluation result.</returns>
    public GammaCameraShieldingResult Evaluate(GammaCameraShieldingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        List<GammaCameraBarrierResult> results = new List<GammaCameraBarrierResult>();
        foreach (NmBarrierInput barrier in input.Barriers)
        {
            results.Add(EvaluateBarrier(input, barrier));
        }

        bool compliant = true;
        foreach (GammaCameraBarrierResult r in results)
        {
            if (!r.Passes) { compliant = false; }
        }

        return new GammaCameraShieldingResult("AERB", results, compliant);
    }

    private static GammaCameraBarrierResult EvaluateBarrier(
        GammaCameraShieldingInput input, NmBarrierInput barrier)
    {
        List<NmCalcStep> steps = new List<NmCalcStep>();
        List<string> notes = new List<string>
        {
            "AERB NM guidance §B.1. Tc-99m basis: Γ = 0.078 mR·h⁻¹·mCi⁻¹ at 1 m.",
            "AERB §B.1.3: 23 cm brick (1.65 g/cm³) or 15 cm concrete (2.35 g/cm³) adequate for all barriers.",
        };

        double p = barrier.ProtectedClass == AreaClass.Controlled
            ? NmConstants.AerbControlledSvPerWk
            : NmConstants.AerbUncontrolledSvPerWk;

        double activityMbq = input.ActivityMbqPerPatient * input.PatientsPerWeek;
        double d = barrier.DistanceMetres;
        double h = input.ImagingTimeHoursPerPatient * input.PatientsPerWeek;

        // Dose rate at 1 m = Γ × A (µSv/h)
        double drAt1m = NmConstants.Tc99mDoseRateConstant * activityMbq;
        steps.Add(new NmCalcStep(
            "Weekly dose rate at 1 m",
            "DR = Γ × A_weekly",
            "DR = " + Fmt(NmConstants.Tc99mDoseRateConstant) + " × " + Fmt(activityMbq) + " MBq",
            Fmt(drAt1m) + " µSv/h at 1 m"));

        // Weekly dose at d (Sv/wk)
        double doseSv = drAt1m * h / (d * d) * 1e-6;
        steps.Add(new NmCalcStep(
            "Weekly dose at barrier point",
            "H = DR × h / d²  (Sv/wk)",
            "H = " + Fmt(drAt1m) + " × " + Fmt(h) + " / " + Fmt(d) + "²",
            Fmt(doseSv * 1e6) + " µSv/week"));

        // AERB adequate thickness
        double aerbMm = barrier.Material == BarrierMaterial.Brick
            ? NmConstants.AerbGcBrickMm
            : NmConstants.AerbGcConcreteMm;

        string statement = "AERB NM guidance §B.1.3: " + barrier.Material + " barrier of "
            + aerbMm.ToString("0", CultureInfo.InvariantCulture) + " mm adequate. "
            + "Provided: " + barrier.ProvidedThicknessMm.ToString("0", CultureInfo.InvariantCulture) + " mm.";

        bool passes = barrier.ProvidedThicknessMm + 1e-6 >= aerbMm;

        return new GammaCameraBarrierResult(
            barrier.BarrierId, doseSv, p,
            barrier.ProvidedThicknessMm, aerbMm,
            passes, statement, steps, notes);
    }

    private static string Fmt(double v)
    {
        if (v == 0.0) { return "0"; }
        double abs = Math.Abs(v);
        if (abs < 1e-3 || abs >= 1e5)
        {
            return v.ToString("0.###e+00", CultureInfo.InvariantCulture);
        }

        return v.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
