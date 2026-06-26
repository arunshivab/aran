using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Machines;
using Aran.Model;

namespace Aran.Engines.Linac;

/// <summary>
/// Evaluates a confirmed layout against a shielding standard using the NCRP 151
/// megavoltage photon method: primary barriers (Eq 2.1–2.4), leakage (Eq 2.8) and
/// patient scatter (Eq 2.7), combined by the two-source rule (§2.3). Every beam mode
/// is evaluated and the controlling (thickest) requirement is reported per barrier.
/// Each step carries a symbolic formula, the same formula with values substituted,
/// and the cited terms, so a report can render both lines faithfully.
/// </summary>
public sealed class LinacShieldingEngine
{
    private const double Log10Of2 = 0.301029995663981;

    /// <summary>Evaluates the input under a single shielding standard.</summary>
    /// <param name="input">The shielding input.</param>
    /// <param name="standard">The standard to apply; must be confirmed.</param>
    /// <returns>The per-barrier result for the standard.</returns>
    public LinacShieldingResult Evaluate(LinacShieldingInput input, ShieldingStandard standard)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(standard);
        if (!standard.IsConfirmed)
        {
            throw new InvalidOperationException(
                "Standard '" + standard.Name + "' is not confirmed. A physicist must verify its values before evaluation.");
        }

        Dictionary<string, Barrier> geometry = new Dictionary<string, Barrier>(StringComparer.Ordinal);
        foreach (Barrier barrier in input.Geometry.Barriers)
        {
            geometry[barrier.Id] = barrier;
        }

        List<LinacBarrierEvaluation> evaluations = new List<LinacBarrierEvaluation>();
        foreach (BarrierEvaluationInput barrierInput in input.Barriers)
        {
            if (!geometry.TryGetValue(barrierInput.BarrierId, out Barrier? barrier))
            {
                throw new ArgumentException(
                    "Barrier '" + barrierInput.BarrierId + "' is not present in the geometry model.", nameof(input));
            }

            evaluations.Add(EvaluateBarrier(input.Machine, barrierInput, barrier, standard, input.Workloads));
        }

        bool compliant = true;
        foreach (LinacBarrierEvaluation evaluation in evaluations)
        {
            if (!evaluation.Passes)
            {
                compliant = false;
            }
        }

        return new LinacShieldingResult(standard.Name, evaluations, compliant);
    }

    private LinacBarrierEvaluation EvaluateBarrier(
        MachineModel machine,
        BarrierEvaluationInput barrierInput,
        Barrier barrier,
        ShieldingStandard standard,
        IReadOnlyList<EnergyWorkload> workloads)
    {
        CitedValue designGoal = standard.DesignGoalSvPerWeek(barrierInput.ProtectedClass);
        CitedValue occupancy = standard.Occupancy(barrierInput.Occupancy);
        List<ComponentResult> components = new List<ComponentResult>();
        double required = 0.0;
        string governing = "none";

        foreach (BeamMode mode in machine.Modes)
        {
            WorkloadValue workload = standard.Workload(machine, mode, workloads);
            if (barrierInput.Role == BarrierRole.Primary)
            {
                Computed primary = BuildPrimary(machine, barrierInput, barrier, standard, mode, designGoal, occupancy, workload);
                components.Add(primary.Result);
                if (primary.ThicknessMm > required)
                {
                    required = primary.ThicknessMm;
                    governing = mode.Name + " primary";
                }
            }
            else
            {
                Computed leakage = BuildLeakage(barrierInput, barrier, standard, mode, designGoal, occupancy, workload);
                Computed scatter = BuildScatter(barrierInput, barrier, standard, mode, designGoal, occupancy, workload);
                components.Add(leakage.Result);
                components.Add(scatter.Result);
                (double combinedMm, string label) = TwoSource(leakage, scatter, mode.Name);
                if (combinedMm > required)
                {
                    required = combinedMm;
                    governing = label;
                }
            }
        }

        bool passes = barrier.ThicknessMm + 1.0e-6 >= required;
        return new LinacBarrierEvaluation(
            barrierInput.BarrierId,
            barrierInput.Role,
            barrier.Material,
            barrier.ThicknessMm,
            required,
            governing,
            passes,
            components);
    }

    private Computed BuildPrimary(
        MachineModel machine,
        BarrierEvaluationInput input,
        Barrier barrier,
        ShieldingStandard standard,
        BeamMode mode,
        CitedValue p,
        CitedValue t,
        WorkloadValue workload)
    {
        PrimaryDistances distances = input.Primary
            ?? throw new ArgumentException("Primary barrier '" + input.BarrierId + "' has no primary distances.", nameof(input));
        CitedValue u = standard.UseFactor(machine, BarrierRole.Primary, input.UseFactor);
        CitedValue stopper = standard.BeamStopperTransmission(machine);
        double d = distances.TargetToPointMetres;
        double denominator = workload.PrimaryGyPerWeek * stopper.Value * u.Value * t.Value;
        double b = denominator > 0.0 ? (p.Value * d * d) / denominator : double.PositiveInfinity;

        List<string> notes = new List<string>(workload.Notes);
        TvlLookup tvl = standard.PrimaryTvl(mode.NominalMv, barrier.Material);
        foreach (string note in tvl.Notes)
        {
            notes.Add(note);
        }

        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Required primary transmission",
                "B_pri = P*d_pri^2 / (W*t_bs*U*T)",
                "B_pri = (" + Fmt(p.Value) + "*" + Fmt(d) + "^2) / (" + Fmt(workload.PrimaryGyPerWeek) + "*" + Fmt(stopper.Value) + "*" + Fmt(u.Value) + "*" + Fmt(t.Value) + ")",
                new CalculationTerm[]
                {
                    new CalculationTerm("P", p.Value, "Sv/wk", p.Citation),
                    new CalculationTerm("d_pri", d, "m", "layout"),
                    new CalculationTerm("W", workload.PrimaryGyPerWeek, "Gy/wk", workload.Citation),
                    new CalculationTerm("t_bs", stopper.Value, "", stopper.Citation),
                    new CalculationTerm("U", u.Value, "", u.Citation),
                    new CalculationTerm("T", t.Value, "", t.Citation),
                },
                new CalculationTerm("B_pri", b, "", "NCRP 151 Eq 2.1")),
        };
        return CompleteThickness(ComponentKind.Primary, mode, b, tvl, steps, notes);
    }

    private Computed BuildLeakage(
        BarrierEvaluationInput input,
        Barrier barrier,
        ShieldingStandard standard,
        BeamMode mode,
        CitedValue p,
        CitedValue t,
        WorkloadValue workload)
    {
        SecondaryDistances distances = input.Secondary
            ?? throw new ArgumentException("Secondary barrier '" + input.BarrierId + "' has no secondary distances.", nameof(input));
        double d = distances.IsocentreToPointMetres;
        double denominator = workload.LeakageGyPerWeek * t.Value;
        double b = denominator > 0.0 ? (p.Value * d * d) / denominator : double.PositiveInfinity;

        List<string> notes = new List<string>(workload.Notes);
        TvlLookup tvl = standard.LeakageTvl(mode.NominalMv, barrier.Material);
        foreach (string note in tvl.Notes)
        {
            notes.Add(note);
        }

        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Required leakage transmission",
                "B_L = P*d_L^2 / (W_L*T)",
                "B_L = (" + Fmt(p.Value) + "*" + Fmt(d) + "^2) / (" + Fmt(workload.LeakageGyPerWeek) + "*" + Fmt(t.Value) + ")",
                new CalculationTerm[]
                {
                    new CalculationTerm("P", p.Value, "Sv/wk", p.Citation),
                    new CalculationTerm("d_L", d, "m", "layout"),
                    new CalculationTerm("W_L", workload.LeakageGyPerWeek, "Gy/wk", workload.Citation),
                    new CalculationTerm("T", t.Value, "", t.Citation),
                },
                new CalculationTerm("B_L", b, "", "NCRP 151 Eq 2.8")),
        };
        return CompleteThickness(ComponentKind.Leakage, mode, b, tvl, steps, notes);
    }

    private Computed BuildScatter(
        BarrierEvaluationInput input,
        Barrier barrier,
        ShieldingStandard standard,
        BeamMode mode,
        CitedValue p,
        CitedValue t,
        WorkloadValue workload)
    {
        SecondaryDistances distances = input.Secondary
            ?? throw new ArgumentException("Secondary barrier '" + input.BarrierId + "' has no secondary distances.", nameof(input));
        ScatterFractionLookup scatter = standard.ScatterFraction(mode.NominalMv, distances.ScatterAngleDegrees);
        double dSca = distances.TargetToPatientMetres;
        double dSec = distances.PatientToPointMetres;
        double f = distances.FieldAreaCm2;
        double denominator = scatter.Fraction * workload.PrimaryGyPerWeek * t.Value * f;
        double b = denominator > 0.0 ? (p.Value * dSca * dSca * dSec * dSec * 400.0) / denominator : double.PositiveInfinity;

        List<string> notes = new List<string>(workload.Notes);
        foreach (string note in scatter.Notes)
        {
            notes.Add(note);
        }

        TvlLookup tvl = standard.ScatterTvl(mode.NominalMv, distances.ScatterAngleDegrees, barrier.Material);
        foreach (string note in tvl.Notes)
        {
            notes.Add(note);
        }

        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Required patient-scatter transmission",
                "B_ps = P*d_sca^2*d_sec^2*400 / (a*W*T*F)",
                "B_ps = (" + Fmt(p.Value) + "*" + Fmt(dSca) + "^2*" + Fmt(dSec) + "^2*400) / (" + Fmt(scatter.Fraction) + "*" + Fmt(workload.PrimaryGyPerWeek) + "*" + Fmt(t.Value) + "*" + Fmt(f) + ")",
                new CalculationTerm[]
                {
                    new CalculationTerm("P", p.Value, "Sv/wk", p.Citation),
                    new CalculationTerm("d_sca", dSca, "m", "layout"),
                    new CalculationTerm("d_sec", dSec, "m", "layout"),
                    new CalculationTerm("a", scatter.Fraction, "", scatter.Citation),
                    new CalculationTerm("W", workload.PrimaryGyPerWeek, "Gy/wk", workload.Citation),
                    new CalculationTerm("T", t.Value, "", t.Citation),
                    new CalculationTerm("F", f, "cm^2", "layout"),
                },
                new CalculationTerm("B_ps", b, "", "NCRP 151 Eq 2.7")),
        };
        return CompleteThickness(ComponentKind.PatientScatter, mode, b, tvl, steps, notes);
    }

    private Computed CompleteThickness(
        ComponentKind kind,
        BeamMode mode,
        double b,
        TvlLookup tvl,
        List<CalculationStep> steps,
        List<string> notes)
    {
        double n = b > 0.0 ? -Math.Log10(b) : 0.0;
        steps.Add(new CalculationStep(
            "Number of TVLs",
            "n = -log10(B)",
            "n = -log10(" + Fmt(b) + ")",
            new CalculationTerm[] { new CalculationTerm("B", b, "", "above") },
            new CalculationTerm("n", n, "", "NCRP 151 Eq 2.2")));

        double thicknessCm = ThicknessCm(n, tvl.Tvl1Cm, tvl.TvlECm);
        steps.Add(new CalculationStep(
            "Required thickness",
            "t = TVL1 + (n-1)*TVLe",
            "t = " + Fmt(tvl.Tvl1Cm) + " + (" + Fmt(n) + "-1)*" + Fmt(tvl.TvlECm),
            new CalculationTerm[]
            {
                new CalculationTerm("TVL1", tvl.Tvl1Cm, "cm", tvl.Citation),
                new CalculationTerm("TVLe", tvl.TvlECm, "cm", tvl.Citation),
                new CalculationTerm("n", n, "", "above"),
            },
            new CalculationTerm("t", thicknessCm, "cm", "NCRP 151 Eq 2.3")));

        double thicknessMm = thicknessCm * 10.0;
        ComponentResult result = new ComponentResult(kind, mode.Name, mode.NominalMv, b, thicknessMm, steps, notes);
        return new Computed(result, thicknessMm, tvl.TvlECm * 10.0);
    }

    private static (double CombinedMm, string Label) TwoSource(Computed leakage, Computed scatter, string modeName)
    {
        Computed larger = leakage.ThicknessMm >= scatter.ThicknessMm ? leakage : scatter;
        Computed smaller = leakage.ThicknessMm >= scatter.ThicknessMm ? scatter : leakage;
        string largerKind = larger.Result.Kind == ComponentKind.Leakage ? "leakage" : "patient-scatter";
        if (larger.ThicknessMm - smaller.ThicknessMm < larger.TvleMm)
        {
            double hvl = Log10Of2 * larger.TvleMm;
            return (larger.ThicknessMm + hvl, modeName + " " + largerKind + " + 1 HVL (two-source, NCRP 151 §2.3)");
        }

        return (larger.ThicknessMm, modeName + " " + largerKind + " (two-source: larger governs)");
    }

    private static double ThicknessCm(double n, double tvl1, double tvle)
    {
        if (n <= 0.0)
        {
            return 0.0;
        }

        if (n <= 1.0)
        {
            return n * tvl1;
        }

        return tvl1 + ((n - 1.0) * tvle);
    }

    private static string Fmt(double value)
    {
        if (value == 0.0)
        {
            return "0";
        }

        double abs = Math.Abs(value);
        if (abs < 1.0e-3 || abs >= 1.0e5)
        {
            return value.ToString("0.###e+00", CultureInfo.InvariantCulture);
        }

        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private sealed record Computed(ComponentResult Result, double ThicknessMm, double TvleMm);
}
