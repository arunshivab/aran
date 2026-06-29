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

    private static readonly MachineModel TomotherapyModel = new MachineModel(
        "Tomotherapy",
        MachineType.LinacTomo,
        new BeamMode[]
        {
            new BeamMode("6X", 6, false),
        },
        null);

    private static readonly MachineModel CyberKnifeModel = new MachineModel(
        "CyberKnife",
        MachineType.CyberKnife,
        new BeamMode[]
        {
            new BeamMode("6X", 6, false),
        },
        null);

    private static readonly MachineModel TelecobaltModel = new MachineModel(
        "Telecobalt",
        MachineType.Telecobalt,
        new BeamMode[]
        {
            new BeamMode("Co-60", 0, false, true),
        },
        null);

    private static readonly MachineModel GammaKnifeModel = new MachineModel(
        "GammaKnife",
        MachineType.GammaKnife,
        new BeamMode[]
        {
            new BeamMode("Co-60", 0, false, true),
        },
        null);

    private static readonly MachineModel HdrIr192Model = new MachineModel(
        "HDR-Ir192",
        MachineType.HdrBrachy,
        new BeamMode[]
        {
            new BeamMode("Ir-192", 0, false),
        },
        null);

    private static readonly MachineModel HdrCo60Model = new MachineModel(
        "HDR-Co60",
        MachineType.HdrBrachy,
        new BeamMode[]
        {
            new BeamMode("Co-60", 0, false, true),
        },
        null);

    private static readonly MachineModel LdrCs137Model = new MachineModel(
        "LDR-Cs137",
        MachineType.LdrBrachyCs137,
        new BeamMode[]
        {
            new BeamMode("Cs-137", 0, false),
        },
        null);

    private static readonly MachineModel SimulatorModel = new MachineModel(
        "Simulator",
        MachineType.Simulator,
        new BeamMode[]
        {
            new BeamMode("100kVp", 0, false),
        },
        null);

    private static readonly MachineModel CtSimulatorModel = new MachineModel(
        "CT-Simulator",
        MachineType.CtSimulator,
        new BeamMode[]
        {
            new BeamMode("CT", 0, false),
        },
        null);

    /// <summary>The Varian TrueBeam (6X, 10X, 15X, 6FFF, 10FFF; no beam stopper).</summary>
    public static MachineModel TrueBeam => TrueBeamModel;

    /// <summary>The Varian Halcyon (6FFF only; O-ring beam stopper, transmission 0.001).</summary>
    public static MachineModel Halcyon => HalcyonModel;

    /// <summary>Helical tomotherapy unit (6 MV; beam stopper OEM-supplied, physicist must enter).</summary>
    public static MachineModel Tomotherapy => TomotherapyModel;

    /// <summary>CyberKnife robotic radiosurgery system (6 MV, no beam stopper).</summary>
    public static MachineModel CyberKnife => CyberKnifeModel;

    /// <summary>Cobalt-60 teletherapy unit.</summary>
    public static MachineModel Telecobalt => TelecobaltModel;

    /// <summary>Gamma Knife radiosurgery (OEM dose map required; engine refuses calculation).</summary>
    public static MachineModel GammaKnife => GammaKnifeModel;

    /// <summary>HDR afterloader with Ir-192 source.</summary>
    public static MachineModel HdrIr192 => HdrIr192Model;

    /// <summary>HDR afterloader with Co-60 source.</summary>
    public static MachineModel HdrCo60 => HdrCo60Model;

    /// <summary>Manual LDR brachytherapy with Cs-137 sources.</summary>
    public static MachineModel LdrCs137 => LdrCs137Model;

    /// <summary>Conventional radiographic simulator (100 kVp).</summary>
    public static MachineModel Simulator => SimulatorModel;

    /// <summary>CT simulator.</summary>
    public static MachineModel CtSimulator => CtSimulatorModel;

    /// <summary>All machine models in the catalog.</summary>
    public static IReadOnlyList<MachineModel> All { get; } = new MachineModel[]
    {
        TrueBeamModel, HalcyonModel, TomotherapyModel, CyberKnifeModel,
        TelecobaltModel, GammaKnifeModel, HdrIr192Model, HdrCo60Model,
        LdrCs137Model, SimulatorModel, CtSimulatorModel,
    };

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
