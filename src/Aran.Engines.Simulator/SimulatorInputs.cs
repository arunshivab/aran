using System.Collections.Generic;
using Aran.Model;

namespace Aran.Engines.Simulator;

/// <summary>The shielding standard for simulator calculations.</summary>
public enum SimulatorStandard
{
    /// <summary>NCRP Report No. 49 method with kVp TVL tables and designer parameters.</summary>
    Ncrp49,

    /// <summary>
    /// AERB simplified rule: 9 inch brick (23 cm) or 6 inch concrete (15 cm) adequate.
    /// No further numerical calculation; a structured compliance statement is returned.
    /// </summary>
    AerbSimplified,
}

/// <summary>Inputs for one simulator barrier evaluation.</summary>
/// <param name="BarrierId">Identifies the barrier.</param>
/// <param name="Material">The barrier material.</param>
/// <param name="ProvidedThicknessMm">Provided thickness (mm).</param>
/// <param name="DistanceMetres">Source to protected point (m).</param>
/// <param name="ProtectedClass">Area class of the protected space.</param>
/// <param name="UseFactor">The primary use factor (U).</param>
/// <param name="OccupancyFactor">The occupancy factor (T).</param>
/// <param name="IsSecondary">Whether the barrier is secondary (scatter/leakage).</param>
public sealed record SimulatorBarrierInput(
    string BarrierId,
    BarrierMaterial Material,
    double ProvidedThicknessMm,
    double DistanceMetres,
    AreaClass ProtectedClass,
    double UseFactor,
    double OccupancyFactor,
    bool IsSecondary);

/// <summary>Inputs for a simulator shielding evaluation.</summary>
/// <param name="KvPeak">The tube peak voltage (kV).</param>
/// <param name="WorkloadMaMinPerWeek">The weekly workload (mA·min/week).</param>
/// <param name="OutputFactorRPerMaMin">
/// The X-ray output factor at 1 m (R per mA·min at 1 m). Typical value for diagnostic
/// rooms at 100 kV: 8.46×10⁻⁴ R per mA·min (NCRP 49 Table B-1).
/// </param>
/// <param name="Standard">The standard to apply.</param>
/// <param name="Barriers">The barriers to evaluate.</param>
public sealed record SimulatorShieldingInput(
    int KvPeak,
    double WorkloadMaMinPerWeek,
    double OutputFactorRPerMaMin,
    SimulatorStandard Standard,
    IReadOnlyList<SimulatorBarrierInput> Barriers);
