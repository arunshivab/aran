using System.Collections.Generic;

namespace Aran.Engines.Brachy;

/// <summary>
/// Physical constants for a brachytherapy radionuclide, transcribed from
/// NCRP Report No. 49 (1976) Table 28 (p.89).
/// </summary>
/// <param name="Name">The radionuclide name (for example "Ir-192").</param>
/// <param name="GammaRayConstantMsvM2GbqH">
/// The specific gamma-ray constant Γ in mSv·m²·GBq⁻¹·h⁻¹ at 1 m.
/// Converted from NCRP 49 Table 28 values (R·cm²·mCi⁻¹·h⁻¹) by multiplying by 0.0956.
/// </param>
/// <param name="TvlConcreteCm">Tenth-value layer in ordinary concrete (cm).</param>
/// <param name="TvlLeadCm">Tenth-value layer in lead (cm).</param>
/// <param name="Citation">The source table and page.</param>
public sealed record IsotopeData(
    string Name,
    double GammaRayConstantMsvM2GbqH,
    double TvlConcreteCm,
    double TvlLeadCm,
    string Citation);

/// <summary>
/// Built-in radionuclide catalog for brachytherapy shielding calculations.
/// All values are from NCRP Report No. 49 (1976) Table 28 (p.89) and confirmed
/// against NCRP Report No. 151 Table B.2 for the Co-60 row.
/// </summary>
public static class IsotopeCatalog
{
    // NCRP 49 Table 28 Γ conversion: R·cm²·mCi⁻¹·h⁻¹ × 0.0956 = mSv·m²·GBq⁻¹·h⁻¹
    // Cs-137: 3.2 × 0.0956 = 0.0836 mSv·m²·GBq⁻¹·h⁻¹  TVL concrete 15.7 cm, lead 2.1 cm
    // Co-60:  13  × 0.0956 = 1.2228 mSv·m²·GBq⁻¹·h⁻¹  TVL concrete 20.6 cm, lead 4.0 cm
    // Ir-192: 5.0 × 0.0956 = 0.4780 mSv·m²·GBq⁻¹·h⁻¹  TVL concrete 14.7 cm, lead 2.0 cm

    private static readonly IsotopeData Cs137Data = new IsotopeData(
        "Cs-137",
        0.0836,
        15.7,
        2.1,
        "NCRP 49 Table 28 (p.89): TVL concrete 15.7 cm, lead 2.1 cm; Γ = 3.2 R·cm²·mCi⁻¹·h⁻¹");

    private static readonly IsotopeData Co60Data = new IsotopeData(
        "Co-60",
        1.2228,
        20.6,
        4.0,
        "NCRP 49 Table 28 (p.89): TVL concrete 20.6 cm, lead 4.0 cm; Γ = 13 R·cm²·mCi⁻¹·h⁻¹");

    private static readonly IsotopeData Ir192Data = new IsotopeData(
        "Ir-192",
        0.4780,
        14.7,
        2.0,
        "NCRP 49 Table 28 (p.89): TVL concrete 14.7 cm, lead 2.0 cm; Γ = 5.0 R·cm²·mCi⁻¹·h⁻¹");

    /// <summary>Cesium-137 (LDR manual brachytherapy).</summary>
    public static IsotopeData Cs137 => Cs137Data;

    /// <summary>Cobalt-60 (HDR brachytherapy, telecobalt).</summary>
    public static IsotopeData Co60 => Co60Data;

    /// <summary>Iridium-192 (HDR brachytherapy).</summary>
    public static IsotopeData Ir192 => Ir192Data;

    /// <summary>All catalogued isotopes.</summary>
    public static IReadOnlyList<IsotopeData> All { get; } =
        new IsotopeData[] { Cs137Data, Co60Data, Ir192Data };
}
