using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Model;

namespace Aran.Engines.NuclearMedicine;

/// <summary>
/// Evaluates High Dose Therapy (HDT) I-131 facility shielding using the
/// inverse-square / TVL method (AERB NM guidance §B.3).
///
/// Method: dose rate at 1 m = Γ_I131 × A (µSv/h).
/// Weekly dose at d = dose rate × h / d².
/// Required transmission B = P / weekly dose.
/// n = -log10(B). Required thickness t = n × TVL.
///
/// TVLs (AERB NM guidance §B.3.2): concrete 10 cm, lead 1 cm.
/// Maximum weekly activity: 300 mCi = 11100 MBq (AERB §B.3.1).
/// AERB design goals: 400/20 µSv/wk. T = 1, U = 1.
/// </summary>
public sealed class HdtShieldingEngine
{
    /// <summary>Evaluates all barriers for the given input.</summary>
    /// <param name="input">The HDT shielding input.</param>
    /// <returns>The evaluation result.</returns>
    public HdtShieldingResult Evaluate(HdtShieldingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        List<HdtBarrierResult> results = new List<HdtBarrierResult>();
        foreach (NmBarrierInput barrier in input.Barriers)
        {
            results.Add(EvaluateBarrier(input, barrier));
        }

        bool compliant = true;
        foreach (HdtBarrierResult r in results)
        {
            if (!r.Passes) { compliant = false; }
        }

        return new HdtShieldingResult("AERB", results, compliant);
    }

    private static HdtBarrierResult EvaluateBarrier(HdtShieldingInput input, NmBarrierInput barrier)
    {
        List<NmCalcStep> steps = new List<NmCalcStep>();
        List<string> notes = new List<string>
        {
            "AERB NM guidance §B.3. I-131: Γ = 0.22 mR·h⁻¹·mCi⁻¹ at 1 m (§B.3.1).",
            "TVL: concrete 10 cm, lead 1 cm (AERB NM guidance §B.3.2). T = 1, U = 1.",
        };

        double p = barrier.ProtectedClass == AreaClass.Controlled
            ? NmConstants.AerbControlledSvPerWk
            : NmConstants.AerbUncontrolledSvPerWk;

        double a = input.WeeklyActivityMbq;
        double h = input.OccupancyHoursPerWeek;
        double d = barrier.DistanceMetres;
        double gamma = NmConstants.I131DoseRateConstant;

        // Dose rate at 1 m = Γ × A (µSv/h)
        double drAt1m = gamma * a;
        steps.Add(new NmCalcStep(
            "Dose rate at 1 m",
            "DR = Γ × A",
            "DR = " + Fmt(gamma) + " × " + Fmt(a) + " MBq",
            Fmt(drAt1m) + " µSv/h at 1 m"));

        // Weekly dose at d (Sv/wk)
        double doseSv = drAt1m * h / (d * d) * 1e-6;
        steps.Add(new NmCalcStep(
            "Weekly dose at barrier point",
            "H = DR × h / d²",
            "H = " + Fmt(drAt1m) + " × " + Fmt(h) + " / " + Fmt(d) + "²",
            Fmt(doseSv * 1e6) + " µSv/week"));

        // B = P / H
        double b = doseSv > 0 ? p / doseSv : double.PositiveInfinity;
        steps.Add(new NmCalcStep(
            "Required transmission",
            "B = P / H",
            "B = " + Fmt(p * 1e6) + " µSv/wk / " + Fmt(doseSv * 1e6) + " µSv/wk",
            Fmt(b)));

        double n = b > 0 ? -Math.Log10(b) : 0.0;
        steps.Add(new NmCalcStep(
            "Number of TVLs",
            "n = -log10(B)",
            "n = -log10(" + Fmt(b) + ")",
            Fmt(n)));

        double tvlCm = barrier.Material == BarrierMaterial.Lead
            ? NmConstants.I131TvlLeadCm
            : NmConstants.I131TvlConcreteCm;

        if (barrier.Material != BarrierMaterial.Lead && barrier.Material != BarrierMaterial.Concrete)
        {
            notes.Add("No tabulated TVL for " + barrier.Material + "; using concrete TVL (10 cm).");
        }

        steps.Add(new NmCalcStep(
            "TVL for " + barrier.Material,
            "TVL (AERB NM guidance §B.3.2)",
            "TVL (" + barrier.Material + ")",
            Fmt(tvlCm) + " cm"));

        double requiredCm = Math.Max(0.0, n) * tvlCm;
        double requiredMm = requiredCm * 10.0;
        steps.Add(new NmCalcStep(
            "Required thickness",
            "t = n × TVL",
            "t = " + Fmt(n) + " × " + Fmt(tvlCm) + " cm",
            Fmt(requiredCm) + " cm = " + Fmt(requiredMm) + " mm"));

        bool passes = barrier.ProvidedThicknessMm + 1e-6 >= requiredMm;
        return new HdtBarrierResult(barrier.BarrierId, requiredMm, barrier.ProvidedThicknessMm,
            passes, steps, notes);
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
