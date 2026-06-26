using System;
using System.Collections.Generic;
using Aran.Model;

namespace Aran.Machines;

/// <summary>The built-in catalog of treatment machine models.</summary>
public static class MachineCatalog
{
    private static readonly MachineModel TrueBeamModel = new MachineModel(
        "TrueBeam",
        MachineType.LinacTrueBeam,
        new BeamMode[]
        {
            new BeamMode("6X", 6, false),
            new BeamMode("10X", 10, false),
            new BeamMode("15X", 15, false),
            new BeamMode("6FFF", 6, true),
            new BeamMode("10FFF", 10, true),
        },
        null);

    private static readonly MachineModel HalcyonModel = new MachineModel(
        "Halcyon",
        MachineType.LinacHalcyon,
        new BeamMode[]
        {
            new BeamMode("6FFF", 6, true),
        },
        0.001);

    /// <summary>The Varian TrueBeam (6X, 10X, 15X, 6FFF, 10FFF; no beam stopper).</summary>
    public static MachineModel TrueBeam => TrueBeamModel;

    /// <summary>The Varian Halcyon (6FFF only; O-ring beam stopper, transmission 0.001).</summary>
    public static MachineModel Halcyon => HalcyonModel;

    /// <summary>All machine models in the catalog.</summary>
    public static IReadOnlyList<MachineModel> All { get; } = new MachineModel[] { TrueBeamModel, HalcyonModel };

    /// <summary>Finds a machine model by name (case-insensitive).</summary>
    /// <param name="name">The machine model name.</param>
    /// <returns>The matching model, or null when not found.</returns>
    public static MachineModel? FindByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        foreach (MachineModel model in All)
        {
            if (string.Equals(model.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return model;
            }
        }

        return null;
    }
}
