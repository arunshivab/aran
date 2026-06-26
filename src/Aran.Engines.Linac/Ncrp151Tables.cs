using System;
using System.Collections.Generic;
using Aran.Model;

namespace Aran.Engines.Linac;

/// <summary>The result of a tenth-value-layer lookup.</summary>
/// <param name="Tvl1Cm">The first tenth-value layer (cm).</param>
/// <param name="TvlECm">The equilibrium tenth-value layer (cm).</param>
/// <param name="Citation">The source table and page.</param>
/// <param name="Notes">Any caveats, such as a nearest-energy substitution.</param>
public sealed record TvlLookup(double Tvl1Cm, double TvlECm, string Citation, IReadOnlyList<string> Notes);

/// <summary>The result of a scatter-fraction lookup.</summary>
/// <param name="Fraction">The scatter fraction (a).</param>
/// <param name="Citation">The source table and page.</param>
/// <param name="Notes">Any caveats, such as a nearest-energy substitution.</param>
public sealed record ScatterFractionLookup(double Fraction, string Citation, IReadOnlyList<string> Notes);

/// <summary>
/// Physical constants transcribed from NCRP Report No. 151 (2005), Appendix B.
/// These values are used by every shielding standard. They are reproduced here for
/// computation and must be verified by a qualified physicist against a licensed copy
/// of the report before any result is trusted; the verification gate lives on
/// <see cref="ShieldingStandard.IsConfirmed"/>.
/// </summary>
public static class Ncrp151Tables
{
    // Table B.2 (p.161): primary-barrier TVLs, (TVL1, TVLe) cm, by energy and material.
    private static readonly Dictionary<(int Mv, BarrierMaterial Mat), (double T1, double Te)> PrimaryTvls = new()
    {
        { (4, BarrierMaterial.Concrete), (35, 30) }, { (4, BarrierMaterial.Steel), (9.9, 9.9) }, { (4, BarrierMaterial.Lead), (5.7, 5.7) },
        { (6, BarrierMaterial.Concrete), (37, 33) }, { (6, BarrierMaterial.Steel), (10, 10) }, { (6, BarrierMaterial.Lead), (5.7, 5.7) },
        { (10, BarrierMaterial.Concrete), (41, 37) }, { (10, BarrierMaterial.Steel), (11, 11) }, { (10, BarrierMaterial.Lead), (5.7, 5.7) },
        { (15, BarrierMaterial.Concrete), (44, 41) }, { (15, BarrierMaterial.Steel), (11, 11) }, { (15, BarrierMaterial.Lead), (5.7, 5.7) },
        { (18, BarrierMaterial.Concrete), (45, 43) }, { (18, BarrierMaterial.Steel), (11, 11) }, { (18, BarrierMaterial.Lead), (5.7, 5.7) },
        { (20, BarrierMaterial.Concrete), (46, 44) }, { (20, BarrierMaterial.Steel), (11, 11) }, { (20, BarrierMaterial.Lead), (5.7, 5.7) },
        { (25, BarrierMaterial.Concrete), (49, 46) }, { (25, BarrierMaterial.Steel), (11, 11) }, { (25, BarrierMaterial.Lead), (5.7, 5.7) },
        { (30, BarrierMaterial.Concrete), (51, 49) }, { (30, BarrierMaterial.Steel), (11, 11) }, { (30, BarrierMaterial.Lead), (5.7, 5.7) },
    };

    // Table B.7 (p.167): leakage-radiation TVLs in ordinary concrete, (TVL1, TVLe) cm.
    private static readonly Dictionary<int, (double T1, double Te)> LeakageTvlsConcrete = new()
    {
        { 4, (33, 28) }, { 6, (34, 29) }, { 10, (35, 31) }, { 15, (36, 33) },
        { 18, (36, 34) }, { 20, (36, 34) }, { 25, (37, 35) }, { 30, (37, 36) },
    };

    // Table B.4 (p.163): patient scatter fractions (a), by energy and scatter angle (degrees).
    private static readonly Dictionary<(int Mv, int Angle), double> ScatterFractions = new()
    {
        { (6, 10), 1.04e-2 }, { (10, 10), 1.66e-2 }, { (18, 10), 1.42e-2 }, { (24, 10), 1.78e-2 },
        { (6, 20), 6.73e-3 }, { (10, 20), 5.79e-3 }, { (18, 20), 5.39e-3 }, { (24, 20), 6.32e-3 },
        { (6, 30), 2.77e-3 }, { (10, 30), 3.18e-3 }, { (18, 30), 2.53e-3 }, { (24, 30), 2.74e-3 },
        { (6, 45), 1.39e-3 }, { (10, 45), 1.35e-3 }, { (18, 45), 8.64e-4 }, { (24, 45), 8.30e-4 },
        { (6, 60), 8.24e-4 }, { (10, 60), 7.46e-4 }, { (18, 60), 4.24e-4 }, { (24, 60), 3.86e-4 },
        { (6, 90), 4.26e-4 }, { (10, 90), 3.81e-4 }, { (18, 90), 1.89e-4 }, { (24, 90), 1.74e-4 },
        { (6, 135), 3.00e-4 }, { (10, 135), 3.02e-4 }, { (18, 135), 1.24e-4 }, { (24, 135), 1.20e-4 },
        { (6, 150), 2.87e-4 }, { (10, 150), 2.74e-4 }, { (18, 150), 1.20e-4 }, { (24, 150), 1.13e-4 },
    };

    // Table B.5a (p.164): patient-scattered TVLs in concrete (cm), single value, by energy and angle.
    private static readonly Dictionary<(int Mv, int Angle), double> ScatterTvlsConcrete = new()
    {
        { (4, 15), 30 }, { (6, 15), 34 }, { (10, 15), 39 }, { (15, 15), 42 }, { (18, 15), 44 }, { (20, 15), 46 }, { (24, 15), 49 },
        { (4, 30), 25 }, { (6, 30), 26 }, { (10, 30), 28 }, { (15, 30), 31 }, { (18, 30), 32 }, { (20, 30), 33 }, { (24, 30), 36 },
        { (4, 45), 22 }, { (6, 45), 23 }, { (10, 45), 25 }, { (15, 45), 26 }, { (18, 45), 27 }, { (20, 45), 27 }, { (24, 45), 29 },
        { (4, 60), 21 }, { (6, 60), 21 }, { (10, 60), 22 }, { (15, 60), 23 }, { (18, 60), 23 }, { (20, 60), 24 }, { (24, 60), 24 },
        { (4, 90), 17 }, { (6, 90), 17 }, { (10, 90), 18 }, { (15, 90), 18 }, { (18, 90), 19 }, { (20, 90), 19 }, { (24, 90), 19 },
        { (4, 135), 14 }, { (6, 135), 15 }, { (10, 135), 15 }, { (15, 135), 15 }, { (18, 135), 15 }, { (20, 135), 15 }, { (24, 135), 16 },
    };

    // Table B.5b (p.165): patient-scattered TVLs in lead (cm), (TVL1, TVL2), by energy and angle.
    private static readonly Dictionary<(int Mv, int Angle), (double T1, double T2)> ScatterTvlsLead = new()
    {
        { (4, 30), (3.3, 3.7) }, { (6, 30), (3.8, 4.4) }, { (10, 30), (4.3, 4.5) },
        { (4, 45), (2.4, 3.1) }, { (6, 45), (2.8, 3.4) }, { (10, 45), (3.1, 3.6) },
        { (4, 60), (1.8, 2.5) }, { (6, 60), (1.9, 2.6) }, { (10, 60), (2.1, 2.7) },
        { (4, 75), (1.3, 1.9) }, { (6, 75), (1.4, 1.9) }, { (10, 75), (1.5, 1.9) },
        { (4, 90), (0.9, 1.3) }, { (6, 90), (1.0, 1.5) }, { (10, 90), (1.2, 1.6) },
        { (4, 105), (0.7, 1.2) }, { (6, 105), (0.7, 1.2) }, { (10, 105), (0.95, 1.4) },
        { (4, 120), (0.5, 0.8) }, { (6, 120), (0.5, 0.8) }, { (10, 120), (0.8, 1.4) },
    };

    private static readonly int[] PrimaryEnergies = new[] { 4, 6, 10, 15, 18, 20, 25, 30 };
    private static readonly int[] LeakageEnergies = new[] { 4, 6, 10, 15, 18, 20, 25, 30 };
    private static readonly int[] ScatterFractionEnergies = new[] { 6, 10, 18, 24 };
    private static readonly int[] ScatterConcreteEnergies = new[] { 4, 6, 10, 15, 18, 20, 24 };
    private static readonly int[] ScatterLeadEnergies = new[] { 4, 6, 10 };
    private static readonly int[] ScatterFractionAngles = new[] { 10, 20, 30, 45, 60, 90, 135, 150 };
    private static readonly int[] ScatterConcreteAngles = new[] { 15, 30, 45, 60, 90, 135 };
    private static readonly int[] ScatterLeadAngles = new[] { 30, 45, 60, 75, 90, 105, 120 };

    /// <summary>Looks up the primary-barrier TVLs for an energy and material (Table B.2).</summary>
    /// <param name="mv">The nominal energy (MV).</param>
    /// <param name="material">The barrier material.</param>
    /// <returns>The TVL lookup with citation and any notes.</returns>
    public static TvlLookup PrimaryTvl(int mv, BarrierMaterial material)
    {
        List<string> notes = new List<string>();
        BarrierMaterial mat = ResolveTvlMaterial(material, notes);
        int e = Nearest(PrimaryEnergies, mv, notes, "primary TVL");
        (double t1, double te) = PrimaryTvls[(e, mat)];
        return new TvlLookup(t1, te, "NCRP 151 Table B.2 (p.161)", notes);
    }

    /// <summary>Looks up the leakage TVLs in concrete for an energy (Table B.7).</summary>
    /// <param name="mv">The nominal energy (MV).</param>
    /// <param name="material">The barrier material.</param>
    /// <returns>The TVL lookup with citation and any notes.</returns>
    public static TvlLookup LeakageTvl(int mv, BarrierMaterial material)
    {
        List<string> notes = new List<string>();
        if (material != BarrierMaterial.Concrete && material != BarrierMaterial.Unknown)
        {
            notes.Add("Leakage TVLs are tabulated for ordinary concrete only; applied to " + material + ".");
        }

        int e = Nearest(LeakageEnergies, mv, notes, "leakage TVL");
        (double t1, double te) = LeakageTvlsConcrete[e];
        return new TvlLookup(t1, te, "NCRP 151 Table B.7 (p.167)", notes);
    }

    /// <summary>Looks up the patient scatter fraction for an energy and angle (Table B.4).</summary>
    /// <param name="mv">The nominal energy (MV).</param>
    /// <param name="angleDegrees">The scatter angle (degrees).</param>
    /// <returns>The scatter-fraction lookup with citation and any notes.</returns>
    public static ScatterFractionLookup ScatterFraction(int mv, double angleDegrees)
    {
        List<string> notes = new List<string>();
        int e = Nearest(ScatterFractionEnergies, mv, notes, "scatter fraction");
        int a = Nearest(ScatterFractionAngles, (int)Math.Round(angleDegrees), notes, "scatter fraction angle");
        return new ScatterFractionLookup(ScatterFractions[(e, a)], "NCRP 151 Table B.4 (p.163)", notes);
    }

    /// <summary>Looks up the patient-scattered TVLs for an energy, angle and material (Tables B.5a/B.5b).</summary>
    /// <param name="mv">The nominal energy (MV).</param>
    /// <param name="angleDegrees">The scatter angle (degrees).</param>
    /// <param name="material">The barrier material.</param>
    /// <returns>The TVL lookup with citation and any notes.</returns>
    public static TvlLookup ScatterTvl(int mv, double angleDegrees, BarrierMaterial material)
    {
        List<string> notes = new List<string>();
        int angle = (int)Math.Round(angleDegrees);
        if (material == BarrierMaterial.Lead)
        {
            int e = Nearest(ScatterLeadEnergies, mv, notes, "lead scatter TVL");
            int a = Nearest(ScatterLeadAngles, angle, notes, "lead scatter TVL angle");
            (double t1, double t2) = ScatterTvlsLead[(e, a)];
            return new TvlLookup(t1, t2, "NCRP 151 Table B.5b (p.165)", notes);
        }

        BarrierMaterial mat = ResolveTvlMaterial(material, notes);
        if (mat != BarrierMaterial.Concrete)
        {
            notes.Add("Scattered-radiation TVLs are tabulated for concrete and lead; applied to " + material + " as concrete.");
        }

        int ce = Nearest(ScatterConcreteEnergies, mv, notes, "concrete scatter TVL");
        int ca = Nearest(ScatterConcreteAngles, angle, notes, "concrete scatter TVL angle");
        double tvl = ScatterTvlsConcrete[(ce, ca)];
        return new TvlLookup(tvl, tvl, "NCRP 151 Table B.5a (p.164)", notes);
    }

    private static BarrierMaterial ResolveTvlMaterial(BarrierMaterial material, List<string> notes)
    {
        if (material == BarrierMaterial.Concrete || material == BarrierMaterial.Steel || material == BarrierMaterial.Lead)
        {
            return material;
        }

        notes.Add("No tabulated TVLs for " + material + "; using ordinary concrete.");
        return BarrierMaterial.Concrete;
    }

    private static int Nearest(int[] available, int target, List<string> notes, string label)
    {
        int best = available[0];
        int bestDiff = Math.Abs(available[0] - target);
        foreach (int candidate in available)
        {
            int diff = Math.Abs(candidate - target);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = candidate;
            }
        }

        if (best != target)
        {
            notes.Add("No tabulated " + label + " for " + target + "; using nearest value at " + best + ".");
        }

        return best;
    }
}
