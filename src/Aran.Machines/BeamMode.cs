namespace Aran.Machines;

/// <summary>
/// A selectable beam mode on a treatment machine. The nominal megavoltage is used
/// for shielding table lookups; flattening-filter-free modes share the nominal energy
/// of their flattened counterpart. Cobalt-60 teletherapy modes set
/// <see cref="IsCo60"/> to true and use <see cref="NominalMv"/> = 0 as a sentinel.
/// </summary>
/// <param name="Name">The display name of the mode (for example "6X", "6FFF", or "Co-60").</param>
/// <param name="NominalMv">
/// The nominal accelerating potential in megavolts, or 0 for Cobalt-60 teletherapy.
/// </param>
/// <param name="Fff">Whether the mode is flattening-filter-free.</param>
/// <param name="IsCo60">Whether the mode is a Cobalt-60 gamma beam.</param>
public sealed record BeamMode(string Name, int NominalMv, bool Fff, bool IsCo60 = false);
