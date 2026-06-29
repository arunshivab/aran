using System;
using System.Collections.Generic;
using Aran.Model;

namespace Aran.Engines.Simulator;

/// <summary>The result of a kVp TVL lookup from NCRP 49.</summary>
/// <param name="TvlCm">The tenth-value layer (cm).</param>
/// <param name="Citation">The source table and page.</param>
/// <param name="Notes">Any caveats.</param>
public sealed record KvpTvlLookup(double TvlCm, string Citation, IReadOnlyList<string> Notes);

/// <summary>
/// Tenth-value layers for diagnostic X-ray shielding, transcribed from
/// NCRP Report No. 49 (1976) Table 27 (p.88).
/// Values are for broad-beam conditions at high attenuation.
/// </summary>
public static class Ncrp49Tables
{
    // Table 27 (p.88): TVL cm for lead (mm) and concrete (cm) by peak voltage (kV).
    // Note: lead values in mm in the source; stored here in cm for unit consistency.
    private static readonly Dictionary<int, (double LeadCm, double ConcreteCm)> KvpTvls = new()
    {
        { 50,   (0.017,  1.5)  },
        { 70,   (0.052,  2.8)  },
        { 100,  (0.088,  5.3)  },
        { 125,  (0.093,  6.6)  },
        { 150,  (0.099,  7.4)  },
        { 200,  (0.17,   8.4)  },
        { 250,  (0.29,   9.4)  },
        { 300,  (0.48,  10.4)  },
        { 400,  (0.83,  10.9)  },
        { 500,  (1.19,  11.7)  },
        { 1000, (2.6,   14.7)  },
    };

    private static readonly int[] KvpValues = new[]
    {
        50, 70, 100, 125, 150, 200, 250, 300, 400, 500, 1000,
    };

    /// <summary>Looks up the TVL for a given peak voltage and barrier material.</summary>
    /// <param name="kVp">The peak tube voltage (kV).</param>
    /// <param name="material">The barrier material.</param>
    /// <returns>The TVL lookup with citation and notes.</returns>
    public static KvpTvlLookup TvlForKvp(int kVp, BarrierMaterial material)
    {
        List<string> notes = new List<string>();
        int nearest = NearestKvp(kVp, notes);
        (double leadCm, double concreteCm) = KvpTvls[nearest];

        double tvlCm;
        switch (material)
        {
            case BarrierMaterial.Lead:
                tvlCm = leadCm;
                break;
            case BarrierMaterial.Concrete:
                tvlCm = concreteCm;
                break;
            default:
                notes.Add("No tabulated TVL for " + material + " at diagnostic kVp; using concrete.");
                tvlCm = concreteCm;
                break;
        }

        return new KvpTvlLookup(tvlCm, "NCRP 49 Table 27 (p.88)", notes);
    }

    private static int NearestKvp(int target, List<string> notes)
    {
        int best = KvpValues[0];
        int bestDiff = Math.Abs(KvpValues[0] - target);
        foreach (int kv in KvpValues)
        {
            int diff = Math.Abs(kv - target);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = kv;
            }
        }

        if (best != target)
        {
            notes.Add("No tabulated TVL for " + target + " kVp; using nearest " + best + " kVp.");
        }

        return best;
    }
}
