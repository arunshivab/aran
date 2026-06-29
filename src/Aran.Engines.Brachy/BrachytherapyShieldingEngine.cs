using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Model;

namespace Aran.Engines.Brachy;

/// <summary>
/// Evaluates brachytherapy (HDR and LDR) barrier shielding requirements using
/// the inverse-square / TVL method from NCRP Report No. 49 (1976).
///
/// Method: dose rate at 1 m = Γ × A (mSv/h).
/// Dose at barrier point = dose rate × (1/d²) × weekly hours (mSv/week).
/// Required transmission B = P / (dose rate × (1/d²) × h × T).
/// n = -log10(B). Required thickness t = n × TVL (cm).
///
/// For AERB: P = 400/20 µSv/wk, T = 1, U = 1 all walls.
/// For NCRP: P = 100/20 µSv/wk (controlled/uncontrolled), T = designer-specified.
/// Isotope TVLs from NCRP 49 Table 28 (p.89); confirmed against NCRP 151 Table B.2.
/// </summary>
public sealed class BrachytherapyShieldingEngine
{
    /// <summary>Evaluates all barriers for the given input.</summary>
    /// <param name="input">The brachytherapy shielding input.</param>
    /// <returns>The evaluation result.</returns>
    public BrachyShieldingResult Evaluate(BrachyShieldingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        string stdName = input.Standard == BrachyStandard.Aerb ? "AERB" : "NCRP";
        List<BrachyBarrierResult> results = new List<BrachyBarrierResult>();

        foreach (BrachyBarrierInput barrier in input.Barriers)
        {
            results.Add(EvaluateBarrier(input, barrier));
        }

        bool compliant = true;
        foreach (BrachyBarrierResult r in results)
        {
            if (!r.Passes) { compliant = false; }
        }

        return new BrachyShieldingResult(stdName, results, compliant);
    }

    private BrachyBarrierResult EvaluateBarrier(BrachyShieldingInput input, BrachyBarrierInput barrier)
    {
        List<string> notes = new List<string>();
        List<BrachyCalcStep> steps = new List<BrachyCalcStep>();
        IsotopeData iso = input.Isotope;

        // Design goal P (Sv/wk)
        double p;
        if (input.Standard == BrachyStandard.Aerb)
        {
            p = barrier.ProtectedClass == AreaClass.Controlled ? 4.0e-4 : 2.0e-5;
        }
        else
        {
            p = barrier.ProtectedClass == AreaClass.Controlled ? 1.0e-4 : 2.0e-5;
        }

        double t = barrier.OccupancyFactor;
        double d = barrier.DistanceMetres;
        double h = input.WeeklyTreatmentHours;
        double a = input.ActivityGbq;
        double gamma = iso.GammaRayConstantMsvM2GbqH;

        // Dose rate at 1 m: DR = Γ × A  (mSv/h)
        double drAt1m = gamma * a;
        steps.Add(new BrachyCalcStep(
            "Dose rate at 1 m",
            "DR = Γ × A",
            "DR = " + Fmt(gamma) + " × " + Fmt(a) + " GBq",
            Fmt(drAt1m) + " mSv/h at 1 m"));

        // Weekly dose at distance d (no barrier): H = DR × (1/d²) × h × T
        double hWeeklySv = drAt1m * (1.0 / (d * d)) * h * t / 1000.0;  // DR in mSv/h → convert to Sv/wk
        steps.Add(new BrachyCalcStep(
            "Weekly dose at barrier point",
            "H = DR(mSv/h) × (1/d²) × h × T / 1000",
            "H = " + Fmt(drAt1m) + " × (1/" + Fmt(d) + "²) × " + Fmt(h) + " × " + Fmt(t) + " / 1000",
            Fmt(hWeeklySv * 1e6) + " µSv/week"));

        // Required transmission B = P / H
        double b = hWeeklySv > 0.0 ? (p / hWeeklySv) : double.PositiveInfinity;
        steps.Add(new BrachyCalcStep(
            "Required transmission",
            "B = P / H",
            "B = " + Fmt(p * 1e6) + " µSv/wk / " + Fmt(hWeeklySv * 1e6) + " µSv/wk",
            Fmt(b)));

        // n = -log10(B)
        double n = b > 0.0 ? -Math.Log10(b) : 0.0;
        steps.Add(new BrachyCalcStep(
            "Number of TVLs required",
            "n = -log10(B)",
            "n = -log10(" + Fmt(b) + ")",
            Fmt(n)));

        // TVL lookup from barrier material
        double tvlCm = TvlForMaterial(iso, barrier.Material, notes);
        steps.Add(new BrachyCalcStep(
            "TVL for " + barrier.Material,
            "TVL",
            iso.Citation,
            Fmt(tvlCm) + " cm"));

        // Required thickness t = n × TVL (single TVL — monoenergetic source)
        double requiredCm = Math.Max(0.0, n) * tvlCm;
        double requiredMm = requiredCm * 10.0;
        steps.Add(new BrachyCalcStep(
            "Required thickness",
            "t = n × TVL",
            "t = " + Fmt(n) + " × " + Fmt(tvlCm) + " cm",
            Fmt(requiredCm) + " cm = " + Fmt(requiredMm) + " mm"));

        bool passes = barrier.ProvidedThicknessMm + 1e-6 >= requiredMm;
        return new BrachyBarrierResult(
            barrier.BarrierId,
            iso.Name,
            requiredMm,
            barrier.ProvidedThicknessMm,
            passes,
            steps,
            notes);
    }

    private static double TvlForMaterial(IsotopeData iso, BarrierMaterial material, List<string> notes)
    {
        switch (material)
        {
            case BarrierMaterial.Concrete:
                return iso.TvlConcreteCm;
            case BarrierMaterial.Lead:
                return iso.TvlLeadCm;
            default:
                notes.Add("No tabulated TVL for " + material + " — using concrete TVL conservatively.");
                return iso.TvlConcreteCm;
        }
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
