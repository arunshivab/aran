namespace Aran.Machines;

/// <summary>
/// Neutron source data for a machine, taken from NCRP 151 Table B.9 or derived from it.
/// These values must be confirmed by a physicist before the engine will use them.
/// </summary>
/// <param name="QnPerGray">
/// Total neutron source strength emitted from the accelerator head per gray of x-ray
/// absorbed dose at the isocenter (neutrons Gy^-1).
/// </param>
/// <param name="H0mSvPerGy">
/// Total neutron dose equivalent at 1.41 m from the target per unit absorbed dose at
/// the isocenter (mSv Gy^-1), as used in Kersey's method (Eq 2.18). Null when not
/// tabulated for the selected reference unit.
/// </param>
/// <param name="Beta">
/// Transmission factor for neutrons that penetrate the head shielding:
/// 1.0 for lead shielding, 0.85 for tungsten.
/// </param>
/// <param name="Citation">The source of the values.</param>
public sealed record NeutronSource(
    double QnPerGray,
    double? H0mSvPerGy,
    double Beta,
    string Citation);
