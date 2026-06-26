using System;
using System.Collections.Generic;
using Aran.Machines;
using Aran.Model;

namespace Aran.Engines.Linac;

/// <summary>The shielding standard implementing NCRP Report No. 151 directly.</summary>
public sealed record Ncrp151Standard : ShieldingStandard
{
    /// <inheritdoc />
    public override string Name => "NCRP 151";

    /// <inheritdoc />
    public override CitedValue DesignGoalSvPerWeek(AreaClass areaClass)
    {
        if (areaClass == AreaClass.Controlled)
        {
            return new CitedValue(1.0e-4, "NCRP 151 §1.4.1 (0.1 mSv week^-1, controlled)");
        }

        return new CitedValue(2.0e-5, "NCRP 151 §1.4.2 (0.02 mSv week^-1, uncontrolled)");
    }

    /// <inheritdoc />
    public override CitedValue Occupancy(OccupancyCategory category)
    {
        string cite = "NCRP 151 Table B.1 (p.160)";
        switch (category)
        {
            case OccupancyCategory.FullOccupancy:
                return new CitedValue(1.0, cite);
            case OccupancyCategory.AdjacentTreatmentRoom:
                return new CitedValue(1.0 / 2.0, cite);
            case OccupancyCategory.Corridor:
                return new CitedValue(1.0 / 5.0, cite);
            case OccupancyCategory.VaultDoor:
                return new CitedValue(1.0 / 8.0, cite);
            case OccupancyCategory.LimitedOccupancy:
                return new CitedValue(1.0 / 20.0, cite);
            default:
                return new CitedValue(1.0 / 40.0, cite);
        }
    }

    /// <inheritdoc />
    public override CitedValue UseFactor(MachineModel machine, BarrierRole role, double designerUseFactor)
    {
        ArgumentNullException.ThrowIfNull(machine);
        if (role == BarrierRole.Secondary)
        {
            return new CitedValue(1.0, "NCRP 151 §2.3 (U = 1 for secondary radiation)");
        }

        return new CitedValue(designerUseFactor, "designer-specified use factor (NCRP 151 §2.2.1)");
    }

    /// <inheritdoc />
    public override WorkloadValue Workload(MachineModel machine, BeamMode mode, IReadOnlyList<EnergyWorkload> inputWorkloads)
    {
        ArgumentNullException.ThrowIfNull(machine);
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(inputWorkloads);
        List<string> notes = new List<string>();
        double primary = SuppliedPrimary(inputWorkloads, mode);
        if (primary <= 0.0)
        {
            notes.Add("No workload supplied for mode " + mode.Name + ".");
        }

        double leakage = 1.0e-3 * primary;
        return new WorkloadValue(primary, leakage, "facility workload; leakage = 10^-3 W (NCRP 151 Eq 2.8)", notes);
    }

    /// <inheritdoc />
    public override CitedValue BeamStopperTransmission(MachineModel machine)
    {
        ArgumentNullException.ThrowIfNull(machine);
        if (machine.BeamStopperTransmission is double t)
        {
            return new CitedValue(t, "machine beam-stopper transmission");
        }

        return new CitedValue(1.0, "no beam stopper");
    }
}

/// <summary>
/// The shielding standard implementing the AERB technical guidance for radiotherapy
/// facilities, using AERB design goals, occupancy, use factors and workloads directly
/// while drawing attenuation tables from NCRP 151.
/// </summary>
public sealed record AerbStandard : ShieldingStandard
{
    /// <inheritdoc />
    public override string Name => "AERB";

    /// <inheritdoc />
    public override CitedValue DesignGoalSvPerWeek(AreaClass areaClass)
    {
        if (areaClass == AreaClass.Controlled)
        {
            return new CitedValue(4.0e-4, "AERB RT guidance §A.1 (400 µSv week^-1, radiation worker)");
        }

        return new CitedValue(2.0e-5, "AERB RT guidance §A.1 (20 µSv week^-1, public)");
    }

    /// <inheritdoc />
    public override CitedValue Occupancy(OccupancyCategory category)
    {
        return new CitedValue(1.0, "AERB RT guidance §A.4 (occupancy factor 1 for all areas)");
    }

    /// <inheritdoc />
    public override CitedValue UseFactor(MachineModel machine, BarrierRole role, double designerUseFactor)
    {
        ArgumentNullException.ThrowIfNull(machine);
        if (role == BarrierRole.Secondary)
        {
            return new CitedValue(1.0, "AERB RT guidance §B (U = 1 for secondary barriers)");
        }

        if (machine.Type == MachineType.LinacHalcyon)
        {
            return new CitedValue(0.12, "AERB RT guidance §B.4 (O-ring primary U = 0.12)");
        }

        return new CitedValue(0.25, "AERB RT guidance §B.1.2 (linac primary U = 0.25)");
    }

    /// <inheritdoc />
    public override WorkloadValue Workload(MachineModel machine, BeamMode mode, IReadOnlyList<EnergyWorkload> inputWorkloads)
    {
        ArgumentNullException.ThrowIfNull(machine);
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(inputWorkloads);
        List<string> notes = new List<string>();

        if (machine.Type == MachineType.LinacHalcyon)
        {
            return new WorkloadValue(1000.0, 3100.0, "AERB RT guidance §B.4 (O-ring primary 1e5, leakage 3.1e5 cGy week^-1)", notes);
        }

        double primary;
        if (mode.NominalMv == 6)
        {
            primary = 1000.0;
        }
        else if (mode.NominalMv == 15)
        {
            primary = 500.0;
        }
        else
        {
            primary = SuppliedPrimary(inputWorkloads, mode);
            notes.Add("AERB does not tabulate a workload for " + mode.NominalMv + " MV; using supplied workload.");
        }

        double leakage = 1.0e-3 * primary;
        return new WorkloadValue(primary, leakage, "AERB RT guidance §B.1.1 (typical linac workload); leakage = 0.1 % of workload", notes);
    }

    /// <inheritdoc />
    public override CitedValue BeamStopperTransmission(MachineModel machine)
    {
        ArgumentNullException.ThrowIfNull(machine);
        return new CitedValue(1.0, "AERB workloads already account for the beam stopper");
    }
}

/// <summary>The built-in shielding standards. Both ship unconfirmed.</summary>
public static class Standards
{
    /// <summary>The NCRP 151 standard (unconfirmed until verified by a physicist).</summary>
    public static Ncrp151Standard Ncrp151 { get; } = new Ncrp151Standard();

    /// <summary>The AERB radiotherapy guidance standard (unconfirmed until verified by a physicist).</summary>
    public static AerbStandard Aerb { get; } = new AerbStandard();
}
