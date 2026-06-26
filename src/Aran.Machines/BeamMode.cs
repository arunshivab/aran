namespace Aran.Machines;

/// <summary>
/// A selectable beam mode on a treatment machine. The nominal megavoltage is used
/// for shielding table lookups; flattening-filter-free modes share the nominal energy
/// of their flattened counterpart.
/// </summary>
/// <param name="Name">The display name of the mode (for example "6X" or "6FFF").</param>
/// <param name="NominalMv">The nominal accelerating potential in megavolts.</param>
/// <param name="Fff">Whether the mode is flattening-filter-free.</param>
public sealed record BeamMode(string Name, int NominalMv, bool Fff);
