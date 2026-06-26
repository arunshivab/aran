using System;
using System.Collections.Generic;

namespace Aran.Machines;

/// <summary>
/// Neutron source data for treatment machines, indexed by machine name.
/// Values are drawn from NCRP 151 Table B.9 (Followill et al., 2003;
/// McGinley, 2002). A physicist must confirm the entry before the door
/// shielding engine will accept it (see <see cref="NeutronSource"/>).
/// </summary>
public static class NeutronCatalog
{
    private static readonly Dictionary<string, NeutronSource> Sources =
        new Dictionary<string, NeutronSource>(StringComparer.OrdinalIgnoreCase)
        {
            // TrueBeam 15X: Varian 2300CD @ 18 MV used as a conservatively safe
            // proxy (Qn rises with energy; 18 MV > 15 MV). H0 not tabulated for
            // 2300CD (Followill et al. measured Qn only).
            ["TrueBeam"] = new NeutronSource(
                0.95e12,
                null,
                1.0,
                "NCRP 151 Table B.9 (Varian 2300CD, 18 MV, Followill et al. 2003); " +
                "applied to 15 MV beam as a conservatively safe value."),

            // Generic 10 MV linac (AERB ≥10 MV threshold):
            // Varian 1800 @ 10 MV, Qn = 0.06e12 (McGinley 2002).
            ["Generic10MV"] = new NeutronSource(
                0.06e12,
                0.04,
                1.0,
                "NCRP 151 Table B.9 (Varian 1800, 10 MV, McGinley 2002)."),
        };

    /// <summary>
    /// Returns the neutron source data for a machine, or null when none is
    /// catalogued. For 10 MV under AERB the caller should use "Generic10MV".
    /// </summary>
    /// <param name="machineName">The machine model name.</param>
    /// <returns>The neutron source, or null.</returns>
    public static NeutronSource? ForMachine(string machineName)
    {
        ArgumentNullException.ThrowIfNull(machineName);
        Sources.TryGetValue(machineName, out NeutronSource? source);
        return source;
    }

    /// <summary>Returns the 10 MV generic neutron source (AERB ≥10 MV threshold).</summary>
    public static NeutronSource Generic10Mv => Sources["Generic10MV"];
}
