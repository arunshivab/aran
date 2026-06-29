using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Model;

namespace Aran.Engines.NuclearMedicine;

/// <summary>
/// Evaluates PET/PET-CT facility shielding requirements using the AAPM TG-108 method
/// (Madsen et al., Med. Phys. 33(1), 2006) as adopted by the AERB NM guidance.
///
/// Two room types are evaluated per facility:
///   Uptake room  — Eq 4: B = 10.9 × P(µSv) × d² / (T × Nw × Ao × tU(h) × R(tU))
///   Imaging room — Eq 10: B = 10.9 × P(µSv) × d² / (T × Nw × Ao × 0.85 × FU × tI(h) × R(tI))
///
/// Thickness from B is obtained by log-linear interpolation in the broad-beam
/// 511 keV Monte Carlo transmission table (TG-108 Table IV), which correctly accounts
/// for scatter buildup — the analytical Archer model and the NCRP TVL diverge from the
/// Monte Carlo results for concrete (TG-108 §Shielding factors, Fig. 2).
///
/// AERB parameters: P = 400/20 µSv/wk, T = 1, Ao = 370 MBq, tU = 45 min, tI = 30 min.
/// </summary>
public sealed class PetShieldingEngine
{
    private const double Log2 = 0.693147180559945;

    // TG-108 Table IV: broad-beam 511 keV transmission factors.
    // Lead: thickness in mm.
    private static readonly double[] LeadMm =
        new[] { 0.0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 25, 30, 40, 50 };

    private static readonly double[] LeadB =
        new[] { 1.0, 0.8912, 0.7873, 0.6905, 0.6021, 0.5227, 0.4522, 0.3903, 0.3362, 0.2892,
                0.2485, 0.1831, 0.1347, 0.0990, 0.0728, 0.0535, 0.0247, 0.0114, 0.0024, 0.0005 };

    // Concrete (2.35 g/cm³): thickness in cm.
    private static readonly double[] ConcreteCm =
        new[] { 0.0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 25, 30, 40, 50 };

    private static readonly double[] ConcreteB =
        new[] { 1.0, 0.9583, 0.9088, 0.8519, 0.7889, 0.7218, 0.6528, 0.5842, 0.5180, 0.4558,
                0.3987, 0.3008, 0.2243, 0.1662, 0.1227, 0.0904, 0.0419, 0.0194, 0.0042, 0.0009 };

    /// <summary>Evaluates the PET facility shielding for both room types.</summary>
    /// <param name="input">The PET shielding input.</param>
    /// <returns>The result including all barrier evaluations.</returns>
    public PetShieldingResult Evaluate(PetShieldingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        List<PetBarrierResult> results = new List<PetBarrierResult>();
        double tUh = input.UptakeTimeMin / 60.0;
        double tIh = input.ImagingTimeMin / 60.0;
        double rU = DecayReductionFactor(input.UptakeTimeMin);
        double rI = DecayReductionFactor(input.ImagingTimeMin);
        double fU = Math.Exp(-Log2 * input.UptakeTimeMin / NmConstants.F18HalfLifeMin);

        foreach (NmBarrierInput barrier in input.UptakeRoomBarriers)
        {
            results.Add(EvaluateUptake(input, barrier, tUh, rU));
        }

        foreach (NmBarrierInput barrier in input.ImagingRoomBarriers)
        {
            results.Add(EvaluateImaging(input, barrier, tIh, rI, fU));
        }

        bool compliant = true;
        foreach (PetBarrierResult r in results)
        {
            if (!r.Passes) { compliant = false; }
        }

        return new PetShieldingResult("AERB (AAPM TG-108)", results, compliant);
    }

    private PetBarrierResult EvaluateUptake(PetShieldingInput input, NmBarrierInput barrier,
        double tUh, double rU)
    {
        List<NmCalcStep> steps = new List<NmCalcStep>();
        List<string> notes = new List<string>
        {
            "AERB NM guidance §B.2; AAPM TG-108 Eq 4. P in µSv.",
        };

        double pUsv = DesignGoalUSv(barrier.ProtectedClass);
        double d = barrier.DistanceMetres;
        double nw = input.PatientsPerWeek;
        double ao = input.AdministeredActivityMbq;

        steps.Add(new NmCalcStep(
            "Decay reduction factor R(tU)",
            "R(tU) = 1.443 × (T½/tU) × [1 − exp(−0.693×tU/T½)]",
            "R(" + Fmt(input.UptakeTimeMin) + " min) = 1.443 × ("
                + Fmt(NmConstants.F18HalfLifeMin) + "/" + Fmt(input.UptakeTimeMin) + ") × [1 − exp(−0.693×"
                + Fmt(input.UptakeTimeMin) + "/" + Fmt(NmConstants.F18HalfLifeMin) + ")]",
            Fmt(rU)));

        // B = 10.9 × P(µSv) × d² / (T × Nw × Ao(MBq) × tU(h) × R(tU))
        double denominator = NmConstants.AerbOccupancyFactor * nw * ao * tUh * rU;
        double b = denominator > 0 ? (10.9 * pUsv * d * d / denominator) : double.PositiveInfinity;

        steps.Add(new NmCalcStep(
            "Required transmission (uptake room, Eq 4)",
            "B = 10.9 × P(µSv) × d² / (T × Nw × Ao × tU(h) × R(tU))",
            "B = 10.9 × " + Fmt(pUsv) + " × " + Fmt(d) + "² / (1 × "
                + Fmt(nw) + " × " + Fmt(ao) + " × " + Fmt(tUh) + " × " + Fmt(rU) + ")",
            Fmt(b)));

        return CompleteResult(barrier, "Uptake", b, steps, notes);
    }

    private PetBarrierResult EvaluateImaging(PetShieldingInput input, NmBarrierInput barrier,
        double tIh, double rI, double fU)
    {
        List<NmCalcStep> steps = new List<NmCalcStep>();
        List<string> notes = new List<string>
        {
            "AERB NM guidance §B.2; AAPM TG-108 Eq 10. Factor 0.85 = voiding (15% bladder excretion).",
        };

        double pUsv = DesignGoalUSv(barrier.ProtectedClass);
        double d = barrier.DistanceMetres;
        double nw = input.PatientsPerWeek;
        double ao = input.AdministeredActivityMbq;

        steps.Add(new NmCalcStep(
            "Uptake decay factor FU = exp(−0.693×tU/T½)",
            "FU = exp(−0.693 × tU / T½)",
            "FU = exp(−0.693 × " + Fmt(input.UptakeTimeMin) + " / " + Fmt(NmConstants.F18HalfLifeMin) + ")",
            Fmt(fU)));

        steps.Add(new NmCalcStep(
            "Decay reduction factor R(tI)",
            "R(tI) = 1.443 × (T½/tI) × [1 − exp(−0.693×tI/T½)]",
            "R(" + Fmt(input.ImagingTimeMin) + " min) = 1.443 × ("
                + Fmt(NmConstants.F18HalfLifeMin) + "/" + Fmt(input.ImagingTimeMin) + ") × [1 − exp(−0.693×"
                + Fmt(input.ImagingTimeMin) + "/" + Fmt(NmConstants.F18HalfLifeMin) + ")]",
            Fmt(rI)));

        // B = 10.9 × P(µSv) × d² / (T × Nw × Ao × 0.85 × FU × tI(h) × R(tI))
        double denominator = nw * ao * NmConstants.VoidingFactor * fU * tIh * rI;
        double b = denominator > 0 ? (10.9 * pUsv * d * d / denominator) : double.PositiveInfinity;

        steps.Add(new NmCalcStep(
            "Required transmission (imaging room, Eq 10)",
            "B = 10.9 × P(µSv) × d² / (T × Nw × Ao × 0.85 × FU × tI(h) × R(tI))",
            "B = 10.9 × " + Fmt(pUsv) + " × " + Fmt(d) + "² / (" + Fmt(nw)
                + " × " + Fmt(ao) + " × 0.85 × " + Fmt(fU) + " × " + Fmt(tIh) + " × " + Fmt(rI) + ")",
            Fmt(b)));

        return CompleteResult(barrier, "Imaging", b, steps, notes);
    }

    private static PetBarrierResult CompleteResult(NmBarrierInput barrier, string roomType,
        double b, List<NmCalcStep> steps, List<string> notes)
    {
        double concreteMm = b >= 1.0 ? 0.0 : InterpolateThickness(ConcreteCm, ConcreteB, b) * 10.0;
        double leadMm = b >= 1.0 ? 0.0 : InterpolateThickness(LeadMm, LeadB, b);

        steps.Add(new NmCalcStep(
            "Required concrete thickness (TG-108 Table IV, log-linear interpolation)",
            "x_concrete from Table IV",
            "Interpolate B=" + Fmt(b) + " in Table IV concrete column",
            Fmt(concreteMm / 10.0) + " cm = " + Fmt(concreteMm) + " mm (concrete, 2.35 g/cm³)"));

        steps.Add(new NmCalcStep(
            "Required lead thickness (TG-108 Table IV, log-linear interpolation)",
            "x_lead from Table IV",
            "Interpolate B=" + Fmt(b) + " in Table IV lead column",
            Fmt(leadMm) + " mm (lead)"));

        double requiredMm = barrier.Material == BarrierMaterial.Lead ? leadMm : concreteMm;
        bool passes = barrier.ProvidedThicknessMm + 1e-6 >= requiredMm;

        return new PetBarrierResult(
            barrier.BarrierId, roomType, b,
            concreteMm, leadMm,
            barrier.ProvidedThicknessMm, barrier.Material.ToString(),
            passes, steps, notes);
    }

    /// <summary>
    /// Log-linear interpolation in a (thickness, transmission) table.
    /// Performs interpolation in log(B) space for accuracy at low transmissions.
    /// </summary>
    private static double InterpolateThickness(double[] thicknesses, double[] transmissions, double bTarget)
    {
        double logTarget = Math.Log(bTarget);
        for (int i = 0; i < thicknesses.Length - 1; i++)
        {
            double logHi = Math.Log(transmissions[i]);
            double logLo = Math.Log(transmissions[i + 1]);
            if (logHi >= logTarget && logTarget >= logLo)
            {
                double frac = (logHi - logTarget) / (logHi - logLo);
                return thicknesses[i] + frac * (thicknesses[i + 1] - thicknesses[i]);
            }
        }

        return thicknesses[thicknesses.Length - 1];
    }

    private static double DecayReductionFactor(double tMin)
    {
        double t12 = NmConstants.F18HalfLifeMin;
        return 1.443 * (t12 / tMin) * (1.0 - Math.Exp(-Log2 * tMin / t12));
    }

    private static double DesignGoalUSv(AreaClass cls) =>
        cls == AreaClass.Controlled
            ? NmConstants.AerbControlledSvPerWk * 1e6
            : NmConstants.AerbUncontrolledSvPerWk * 1e6;

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
