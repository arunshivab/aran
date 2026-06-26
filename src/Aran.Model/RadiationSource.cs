using System.Collections.Generic;

namespace Aran.Model;

/// <summary>
/// A radiation source recovered from the layout. Every field is a draft proposal
/// until <see cref="IsConfirmed"/> is set true by the verification step.
/// </summary>
/// <param name="Id">A stable identifier unique within the model.</param>
/// <param name="Isocentre">The source or isocentre position in millimetre space.</param>
/// <param name="BeamAxes">The beam or rotation axes associated with the source.</param>
/// <param name="Machine">The proposed machine class occupying the vault.</param>
/// <param name="EnergiesMv">The proposed photon energies in megavolts.</param>
/// <param name="Provenance">How the source was derived.</param>
/// <param name="IsConfirmed">Whether the user has confirmed this source.</param>
public sealed record RadiationSource(
    string Id,
    PointMm Isocentre,
    IReadOnlyList<LineMm> BeamAxes,
    MachineType Machine,
    IReadOnlyList<int> EnergiesMv,
    Provenance Provenance,
    bool IsConfirmed);
