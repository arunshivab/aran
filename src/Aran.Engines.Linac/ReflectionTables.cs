using System;
using System.Collections.Generic;

namespace Aran.Engines.Linac;

/// <summary>The result of a reflection-coefficient lookup.</summary>
/// <param name="Alpha">The differential dose albedo (dimensionless).</param>
/// <param name="Citation">The source table and page.</param>
/// <param name="Notes">Any caveats, such as energy or angle substitutions.</param>
public sealed record AlbedoLookup(double Alpha, string Citation, IReadOnlyList<string> Notes);

/// <summary>
/// Reflection coefficients (differential dose albedo) for ordinary concrete,
/// transcribed from NCRP 151 Tables B.8a–B.8f (pp.168–172).
/// All table entries are multiplied by 10^-3 as stated in the table footnotes.
/// </summary>
public static class ReflectionTables
{
    // Table B.8a (p.168): normal (0°) incidence on ordinary concrete.
    // Entry × 10^-3.  Columns: reflection angle 0,30,45,60,75 degrees.
    private static readonly Dictionary<string, double[]> B8a = new()
    {
        ["30MV"] = new[] { 3.0e-3, 2.7e-3, 2.6e-3, 2.2e-3, 1.5e-3 },
        ["24MV"] = new[] { 3.2e-3, 3.2e-3, 2.8e-3, 2.3e-3, 1.5e-3 },
        ["18MV"] = new[] { 3.4e-3, 3.4e-3, 3.0e-3, 2.5e-3, 1.6e-3 },
        ["10MV"] = new[] { 4.3e-3, 4.1e-3, 3.8e-3, 3.1e-3, 2.1e-3 },
        ["6MV"] = new[] { 5.3e-3, 5.2e-3, 4.7e-3, 4.0e-3, 2.7e-3 },
        ["4MV"] = new[] { 6.7e-3, 6.4e-3, 5.8e-3, 4.9e-3, 3.1e-3 },
        ["Co-60"] = new[] { 7.0e-3, 6.5e-3, 6.0e-3, 5.5e-3, 3.8e-3 },
        ["0.5MeV"] = new[] { 19.0e-3, 17.0e-3, 15.0e-3, 13.0e-3, 8.0e-3 },
        ["0.25MeV"] = new[] { 32.0e-3, 28.0e-3, 25.0e-3, 22.0e-3, 13.0e-3 },
    };

    private static readonly int[] B8aAngles = new[] { 0, 30, 45, 60, 75 };

    // Nominal MV → B.8a key, with effective modal-energy mapping:
    // NCRP §2.4.1: α0 uses beam energy; α1 for leakage uses ~1.4 MeV (6MV) / 1.5 MeV (10MV) → 0.5 MeV row conservative; patient scatter → 0.5 MeV.
    private static readonly Dictionary<int, string> MvToB8aKey = new()
    {
        { 4,  "4MV" }, { 6, "6MV" }, { 10, "10MV" }, { 15, "18MV" },
        { 18, "18MV" }, { 20, "18MV" }, { 24, "24MV" }, { 25, "24MV" }, { 30, "30MV" },
    };

    /// <summary>
    /// Reflection coefficient α0 for the primary beam striking the first surface
    /// (normal incidence, concrete). Uses beam energy row; reflection angle taken as 45°
    /// per standard maze geometry (beam → floor/wall; reflected toward maze).
    /// </summary>
    /// <param name="mv">Nominal beam energy (MV).</param>
    /// <param name="reflectionAngleDeg">Angle of reflection from the normal (degrees).</param>
    /// <returns>The albedo lookup.</returns>
    public static AlbedoLookup Alpha0Primary(int mv, double reflectionAngleDeg)
    {
        List<string> notes = new List<string>();
        string key = NearestB8aKey(mv, notes);
        double alpha = InterpolateB8a(B8a[key], B8aAngles, reflectionAngleDeg, notes);
        return new AlbedoLookup(alpha, "NCRP 151 Table B.8a (p.168)", notes);
    }

    /// <summary>
    /// Reflection coefficient αz for the second maze surface reflection.
    /// NCRP uses 0.5 MeV for this term (§2.4.1).
    /// </summary>
    /// <param name="reflectionAngleDeg">Angle of reflection from the normal (degrees).</param>
    /// <returns>The albedo lookup.</returns>
    public static AlbedoLookup AlphaZSecondSurface(double reflectionAngleDeg)
    {
        List<string> notes = new List<string> { "0.5 MeV energy assumed per NCRP 151 §2.4.1." };
        double alpha = InterpolateB8a(B8a["0.5MeV"], B8aAngles, reflectionAngleDeg, notes);
        return new AlbedoLookup(alpha, "NCRP 151 Table B.8a (p.168)", notes);
    }

    /// <summary>
    /// Reflection coefficient α1 for leakage scatter from Wall G, using the modal
    /// bremsstrahlung energy: ~1.4 MeV for 6 MV, ~1.5 MeV for 10 MV (Nelson &amp;
    /// LaRiviere 1984). Both are approximated by the 0.5 MeV row (conservative).
    /// </summary>
    /// <param name="mv">Nominal beam energy (MV).</param>
    /// <param name="reflectionAngleDeg">Angle of reflection from the normal (degrees).</param>
    /// <returns>The albedo lookup.</returns>
    public static AlbedoLookup Alpha1LeakageScatter(int mv, double reflectionAngleDeg)
    {
        List<string> notes = new List<string>
        {
            "Modal bremsstrahlung energy ~1.4–1.5 MeV (Nelson & LaRiviere 1984); " +
            "0.5 MeV row used as conservatively safe per NCRP 151 §2.4.1.",
        };
        double alpha = InterpolateB8a(B8a["0.5MeV"], B8aAngles, reflectionAngleDeg, notes);
        return new AlbedoLookup(alpha, "NCRP 151 Table B.8a (p.168)", notes);
    }

    /// <summary>
    /// Reflection coefficient α1 for patient-scattered radiation (0.5 MeV conservative).
    /// </summary>
    /// <param name="reflectionAngleDeg">Angle of reflection from the normal (degrees).</param>
    /// <returns>The albedo lookup.</returns>
    public static AlbedoLookup Alpha1PatientScatter(double reflectionAngleDeg)
    {
        List<string> notes = new List<string> { "0.5 MeV energy assumed per NCRP 151 §2.4.1." };
        double alpha = InterpolateB8a(B8a["0.5MeV"], B8aAngles, reflectionAngleDeg, notes);
        return new AlbedoLookup(alpha, "NCRP 151 Table B.8a (p.168)", notes);
    }

    private static string NearestB8aKey(int mv, List<string> notes)
    {
        if (MvToB8aKey.TryGetValue(mv, out string? key))
        {
            return key;
        }

        notes.Add("No B.8a row for " + mv + " MV; using 18 MV row.");
        return "18MV";
    }

    private static double InterpolateB8a(double[] row, int[] angles, double targetDeg, List<string> notes)
    {
        int best = 0;
        double bestDiff = Math.Abs(angles[0] - targetDeg);
        for (int i = 1; i < angles.Length; i++)
        {
            double diff = Math.Abs(angles[i] - targetDeg);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
            }
        }

        if (Math.Abs(angles[best] - targetDeg) > 0.5)
        {
            notes.Add("No B.8a column for " + targetDeg + "°; using nearest " + angles[best] + "°.");
        }

        return row[best];
    }
}
