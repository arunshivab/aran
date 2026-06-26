using System;
using System.Collections.Generic;
using Aran.Machines;
using Aran.Model;

namespace Aran.Engines.Linac;

/// <summary>A value used in a calculation together with the source it came from.</summary>
/// <param name="Value">The numeric value.</param>
/// <param name="Citation">The source of the value.</param>
public sealed record CitedValue(double Value, string Citation);

/// <summary>The resolved primary and leakage workloads for a beam mode under a standard.</summary>
/// <param name="PrimaryGyPerWeek">Primary absorbed dose per week at 1 m (Gy week^-1).</param>
/// <param name="LeakageGyPerWeek">Leakage workload per week at 1 m (Gy week^-1).</param>
/// <param name="Citation">The source of the workload values.</param>
/// <param name="Notes">Any caveats raised while resolving the workload.</param>
public sealed record WorkloadValue(
    double PrimaryGyPerWeek,
    double LeakageGyPerWeek,
    string Citation,
    IReadOnlyList<string> Notes);

/// <summary>
/// A shielding standard: a self-contained set of policies (design goals, occupancy,
/// use factor, workload, beam-stopper handling) plus the shared NCRP 151 attenuation
/// tables. Each standard resolves the same inputs to its own numbers, so one geometry
/// yields one report per standard with that standard's values substituted directly.
/// The engine refuses to run until <see cref="IsConfirmed"/> is set true by a physicist.
/// </summary>
public abstract record ShieldingStandard
{
    /// <summary>The display name of the standard.</summary>
    public abstract string Name { get; }

    /// <summary>Whether a physicist has confirmed this standard's values against the source.</summary>
    public bool IsConfirmed { get; init; }

    /// <summary>Resolves the shielding design goal for a protected area (Sv week^-1).</summary>
    /// <param name="areaClass">The protection class of the area.</param>
    /// <returns>The design goal with its citation.</returns>
    public abstract CitedValue DesignGoalSvPerWeek(AreaClass areaClass);

    /// <summary>Resolves the occupancy factor for a location.</summary>
    /// <param name="category">The occupancy category.</param>
    /// <returns>The occupancy factor with its citation.</returns>
    public abstract CitedValue Occupancy(OccupancyCategory category);

    /// <summary>Resolves the use factor for a barrier.</summary>
    /// <param name="machine">The treatment machine.</param>
    /// <param name="role">The barrier role.</param>
    /// <param name="designerUseFactor">The designer-supplied use factor.</param>
    /// <returns>The use factor with its citation.</returns>
    public abstract CitedValue UseFactor(MachineModel machine, BarrierRole role, double designerUseFactor);

    /// <summary>Resolves the primary and leakage workloads for a beam mode.</summary>
    /// <param name="machine">The treatment machine.</param>
    /// <param name="mode">The beam mode.</param>
    /// <param name="inputWorkloads">The physicist-supplied workloads.</param>
    /// <returns>The resolved workloads with citation and notes.</returns>
    public abstract WorkloadValue Workload(MachineModel machine, BeamMode mode, IReadOnlyList<EnergyWorkload> inputWorkloads);

    /// <summary>Resolves the primary-beam transmission of the machine's beam stopper.</summary>
    /// <param name="machine">The treatment machine.</param>
    /// <returns>The transmission factor with its citation.</returns>
    public abstract CitedValue BeamStopperTransmission(MachineModel machine);

    /// <summary>Primary-barrier TVLs (Table B.2).</summary>
    /// <param name="mv">Nominal energy (MV).</param>
    /// <param name="material">Barrier material.</param>
    /// <returns>The TVL lookup.</returns>
    public TvlLookup PrimaryTvl(int mv, BarrierMaterial material) => Ncrp151Tables.PrimaryTvl(mv, material);

    /// <summary>Leakage TVLs (Table B.7).</summary>
    /// <param name="mv">Nominal energy (MV).</param>
    /// <param name="material">Barrier material.</param>
    /// <returns>The TVL lookup.</returns>
    public TvlLookup LeakageTvl(int mv, BarrierMaterial material) => Ncrp151Tables.LeakageTvl(mv, material);

    /// <summary>Patient scatter fraction (Table B.4).</summary>
    /// <param name="mv">Nominal energy (MV).</param>
    /// <param name="angleDegrees">Scatter angle (degrees).</param>
    /// <returns>The scatter-fraction lookup.</returns>
    public ScatterFractionLookup ScatterFraction(int mv, double angleDegrees) => Ncrp151Tables.ScatterFraction(mv, angleDegrees);

    /// <summary>Patient-scattered TVLs (Tables B.5a/B.5b).</summary>
    /// <param name="mv">Nominal energy (MV).</param>
    /// <param name="angleDegrees">Scatter angle (degrees).</param>
    /// <param name="material">Barrier material.</param>
    /// <returns>The TVL lookup.</returns>
    public TvlLookup ScatterTvl(int mv, double angleDegrees, BarrierMaterial material) => Ncrp151Tables.ScatterTvl(mv, angleDegrees, material);

    /// <summary>Finds the supplied primary workload for a beam mode.</summary>
    /// <param name="inputWorkloads">The supplied workloads.</param>
    /// <param name="mode">The beam mode.</param>
    /// <returns>The primary workload, or zero when none is supplied.</returns>
    protected static double SuppliedPrimary(IReadOnlyList<EnergyWorkload> inputWorkloads, BeamMode mode)
    {
        ArgumentNullException.ThrowIfNull(inputWorkloads);
        ArgumentNullException.ThrowIfNull(mode);
        foreach (EnergyWorkload workload in inputWorkloads)
        {
            if (string.Equals(workload.ModeName, mode.Name, StringComparison.OrdinalIgnoreCase))
            {
                return workload.PrimaryGyPerWeek;
            }
        }

        return 0.0;
    }
}
