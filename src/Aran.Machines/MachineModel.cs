using System.Collections.Generic;
using Aran.Model;

namespace Aran.Machines;

/// <summary>
/// A treatment machine model and its selectable beam modes, as used for shielding.
/// </summary>
/// <param name="Name">The machine model name (for example "TrueBeam").</param>
/// <param name="Type">The machine class.</param>
/// <param name="Modes">The selectable beam modes.</param>
/// <param name="BeamStopperTransmission">
/// The primary-beam transmission of an integral beam stopper, or null when the
/// machine has no beam stopper. For an O-ring unit this is typically 0.001.
/// </param>
public sealed record MachineModel(
    string Name,
    MachineType Type,
    IReadOnlyList<BeamMode> Modes,
    double? BeamStopperTransmission);
