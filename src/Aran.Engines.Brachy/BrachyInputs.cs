using Aran.Model;

namespace Aran.Engines.Brachy;

/// <summary>The shielding standard for brachytherapy calculations.</summary>
public enum BrachyStandard
{
    /// <summary>NCRP Report No. 49 / 151 method with designer occupancy.</summary>
    Ncrp,

    /// <summary>AERB simplified rule (T = 1, P = 400/20 µSv/wk, U = 1 all walls).</summary>
    Aerb,
}

/// <summary>Inputs for a single-barrier brachytherapy evaluation.</summary>
/// <param name="BarrierId">Identifies the barrier in the geometry model.</param>
/// <param name="Material">The barrier material.</param>
/// <param name="ProvidedThicknessMm">The barrier thickness present in the model (mm).</param>
/// <param name="DistanceMetres">Source to the nearest occupied point beyond the barrier (m).</param>
/// <param name="ProtectedClass">The area-protection class outside the barrier.</param>
/// <param name="OccupancyFactor">The occupancy factor T (1.0 for AERB, designer-specified for NCRP).</param>
public sealed record BrachyBarrierInput(
    string BarrierId,
    BarrierMaterial Material,
    double ProvidedThicknessMm,
    double DistanceMetres,
    AreaClass ProtectedClass,
    double OccupancyFactor);

/// <summary>Inputs for a brachytherapy shielding evaluation (HDR or LDR).</summary>
/// <param name="Isotope">The radionuclide data.</param>
/// <param name="ActivityGbq">The source activity (GBq). For HDR this is nominal source activity; for LDR the maximum total activity implanted.</param>
/// <param name="WeeklyTreatmentHours">The weekly beam-on / implant time (hours/week).</param>
/// <param name="Standard">The shielding standard to apply.</param>
/// <param name="Barriers">The barriers to evaluate.</param>
public sealed record BrachyShieldingInput(
    IsotopeData Isotope,
    double ActivityGbq,
    double WeeklyTreatmentHours,
    BrachyStandard Standard,
    System.Collections.Generic.IReadOnlyList<BrachyBarrierInput> Barriers);
