using System.Collections.Generic;
using Aran.Machines;
using Aran.Model;

namespace Aran.Engines.Linac;

/// <summary>The primary photon workload of a beam mode, at 1 m from the target.</summary>
/// <param name="ModeName">The beam mode this workload applies to.</param>
/// <param name="PrimaryGyPerWeek">The primary absorbed dose per week at 1 m (Gy week^-1).</param>
public sealed record EnergyWorkload(string ModeName, double PrimaryGyPerWeek);

/// <summary>Distances used for a primary-barrier evaluation.</summary>
/// <param name="TargetToPointMetres">Distance from the x-ray target to the protected point (m).</param>
public sealed record PrimaryDistances(double TargetToPointMetres);

/// <summary>Distances and geometry used for a secondary-barrier evaluation.</summary>
/// <param name="IsocentreToPointMetres">Distance from isocentre to the protected point, for leakage (m).</param>
/// <param name="TargetToPatientMetres">Distance from the target to the patient/scatterer, for scatter (m).</param>
/// <param name="PatientToPointMetres">Distance from the scatterer to the protected point, for scatter (m).</param>
/// <param name="ScatterAngleDegrees">The scattering angle from the primary beam (degrees).</param>
/// <param name="FieldAreaCm2">The field area at mid-depth of the patient at 1 m (cm^2).</param>
public sealed record SecondaryDistances(
    double IsocentreToPointMetres,
    double TargetToPatientMetres,
    double PatientToPointMetres,
    double ScatterAngleDegrees,
    double FieldAreaCm2);

/// <summary>The per-barrier physics inputs the physicist supplies on the canvas.</summary>
/// <param name="BarrierId">Identifies the barrier in the confirmed geometry model.</param>
/// <param name="Role">Whether the barrier is primary or secondary.</param>
/// <param name="ProtectedClass">The radiation-protection class of the protected area.</param>
/// <param name="Occupancy">The occupancy category of the protected area.</param>
/// <param name="UseFactor">The designer-supplied primary use factor (ignored where a standard fixes it).</param>
/// <param name="Primary">Primary distances, when the role is primary.</param>
/// <param name="Secondary">Secondary distances, when the role is secondary.</param>
public sealed record BarrierEvaluationInput(
    string BarrierId,
    BarrierRole Role,
    AreaClass ProtectedClass,
    OccupancyCategory Occupancy,
    double UseFactor,
    PrimaryDistances? Primary,
    SecondaryDistances? Secondary);

/// <summary>The complete input to a LINAC shielding evaluation.</summary>
/// <param name="Geometry">The confirmed geometry model.</param>
/// <param name="Machine">The treatment machine model.</param>
/// <param name="Workloads">Primary workloads per beam mode (used where a standard does not supply its own).</param>
/// <param name="Barriers">The per-barrier physics inputs.</param>
public sealed record LinacShieldingInput(
    ShieldingGeometryModel Geometry,
    MachineModel Machine,
    IReadOnlyList<EnergyWorkload> Workloads,
    IReadOnlyList<BarrierEvaluationInput> Barriers);
