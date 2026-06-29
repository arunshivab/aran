using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Model;

namespace Aran.Engines.Simulator;

/// <summary>
/// Evaluates simulator and CT-simulator barrier shielding requirements.
///
/// NCRP method (NCRP 49, 1976): B = P × d² / (W × output × U × T).
/// n = -log10(B). t = n × TVL (kVp-dependent, Table 27 p.88).
/// Output factor converts mA·min/week to dose at 1 m (R per mA·min).
/// For NCRP: P = 1 mR/wk (1e-5 Sv) controlled / 0.1 mR/wk (1e-6 Sv) uncontrolled.
/// (NCRP 49 §2.1: 100 mR/wk design level; non-occupational 10 mR/wk.)
///
/// AERB simplified rule (AERB RT guidance §B.8):
/// 9 inch brick (229 mm) or 6 inch concrete (152 mm) is adequate for simulator and
/// CT-simulator installations. A structured compliance statement is returned; no
/// numerical transmission calculation is performed.
/// </summary>
public sealed class SimulatorShieldingEngine
{
    // NCRP 49 §2.1 design levels in Sv/wk
    private const double NcrpControlledSvPerWk = 1.0e-3;       // 100 mR/wk
    private const double NcrpUncontrolledSvPerWk = 1.0e-4;     // 10 mR/wk

    // AERB §B.8 minimum adequate thicknesses (mm)
    private const double AerbBrickMm = 229.0;    // 9 inch
    private const double AerbConcreteMm = 152.0; // 6 inch

    /// <summary>Evaluates all barriers for the given input.</summary>
    /// <param name="input">The simulator shielding input.</param>
    /// <returns>The evaluation result.</returns>
    public SimulatorShieldingResult Evaluate(SimulatorShieldingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        List<SimulatorBarrierResult> results = new List<SimulatorBarrierResult>();

        if (input.Standard == SimulatorStandard.AerbSimplified)
        {
            foreach (SimulatorBarrierInput barrier in input.Barriers)
            {
                results.Add(EvaluateAerb(barrier));
            }

            return new SimulatorShieldingResult("AERB (simplified)", results, AllPass(results));
        }

        foreach (SimulatorBarrierInput barrier in input.Barriers)
        {
            results.Add(EvaluateNcrp(input, barrier));
        }

        return new SimulatorShieldingResult("NCRP 49", results, AllPass(results));
    }

    private SimulatorBarrierResult EvaluateNcrp(SimulatorShieldingInput input, SimulatorBarrierInput barrier)
    {
        List<string> notes = new List<string>();
        List<SimCalcStep> steps = new List<SimCalcStep>();

        double p = barrier.ProtectedClass == AreaClass.Controlled
            ? NcrpControlledSvPerWk
            : NcrpUncontrolledSvPerWk;
        string pNote = barrier.ProtectedClass == AreaClass.Controlled
            ? "NCRP 49 §2.1 (100 mR/wk controlled)"
            : "NCRP 49 §2.1 (10 mR/wk uncontrolled)";

        double w = input.WorkloadMaMinPerWeek;
        double outputFactor = input.OutputFactorRPerMaMin;
        double u = barrier.IsSecondary ? 1.0 : barrier.UseFactor;
        double t = barrier.OccupancyFactor;
        double d = barrier.DistanceMetres;

        // Dose at 1 m per week = W × output (R/wk at 1 m)
        double doseAt1m = w * outputFactor;
        steps.Add(new SimCalcStep(
            "Weekly dose at 1 m",
            "D1m = W × output",
            "D1m = " + Fmt(w) + " × " + Fmt(outputFactor),
            Fmt(doseAt1m) + " R/wk at 1 m"));

        // Required transmission B = P / (D1m × (1/d²) × U × T)
        double denominator = doseAt1m * (1.0 / (d * d)) * u * t;
        double b = denominator > 0.0 ? (p / 0.01 / denominator) : double.PositiveInfinity;
        // p in Sv → convert to R (1 R ≈ 0.01 Sv) for dimensional consistency with R-based output
        steps.Add(new SimCalcStep(
            "Required transmission",
            "B = P / (D1m × 1/d² × U × T)  [P in R]",
            "B = " + Fmt(p / 0.01) + " / (" + Fmt(doseAt1m) + " × 1/" + Fmt(d) + "² × " + Fmt(u) + " × " + Fmt(t) + ")",
            Fmt(b)));
        notes.Add(pNote);

        double n = b > 0.0 ? -Math.Log10(b) : 0.0;
        steps.Add(new SimCalcStep(
            "Number of TVLs",
            "n = -log10(B)",
            "n = -log10(" + Fmt(b) + ")",
            Fmt(n)));

        KvpTvlLookup tvl = Ncrp49Tables.TvlForKvp(input.KvPeak, barrier.Material);
        foreach (string note in tvl.Notes) { notes.Add(note); }
        steps.Add(new SimCalcStep(
            "TVL at " + input.KvPeak + " kVp",
            "TVL",
            tvl.Citation,
            Fmt(tvl.TvlCm) + " cm"));

        double requiredCm = Math.Max(0.0, n) * tvl.TvlCm;
        double requiredMm = requiredCm * 10.0;
        steps.Add(new SimCalcStep(
            "Required thickness",
            "t = n × TVL",
            "t = " + Fmt(n) + " × " + Fmt(tvl.TvlCm) + " cm",
            Fmt(requiredCm) + " cm = " + Fmt(requiredMm) + " mm"));

        bool passes = barrier.ProvidedThicknessMm + 1e-6 >= requiredMm;
        return new SimulatorBarrierResult(
            barrier.BarrierId, requiredMm, barrier.ProvidedThicknessMm,
            passes, null, steps, notes);
    }

    private static SimulatorBarrierResult EvaluateAerb(SimulatorBarrierInput barrier)
    {
        List<string> notes = new List<string>
        {
            "AERB RT guidance §B.8: 9 inch brick (229 mm) or 6 inch concrete (152 mm) is " +
            "adequate for simulator and CT-simulator installations. No further numerical " +
            "calculation is required per AERB guidance.",
        };

        double adequateMm = barrier.Material == BarrierMaterial.Brick
            ? AerbBrickMm
            : AerbConcreteMm;

        string statement = "AERB §B.8 simplified rule: " + barrier.Material + " barrier of " +
            adequateMm.ToString("0", CultureInfo.InvariantCulture) + " mm adequate. " +
            "Provided: " + barrier.ProvidedThicknessMm.ToString("0", CultureInfo.InvariantCulture) + " mm.";

        bool passes = barrier.ProvidedThicknessMm + 1e-6 >= adequateMm;

        return new SimulatorBarrierResult(
            barrier.BarrierId, adequateMm, barrier.ProvidedThicknessMm,
            passes, statement, new List<SimCalcStep>(), notes);
    }

    private static bool AllPass(IReadOnlyList<SimulatorBarrierResult> results)
    {
        foreach (SimulatorBarrierResult r in results)
        {
            if (!r.Passes) { return false; }
        }

        return true;
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
