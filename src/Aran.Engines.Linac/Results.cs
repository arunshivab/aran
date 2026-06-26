using System.Collections.Generic;
using Aran.Model;

namespace Aran.Engines.Linac;

/// <summary>The evaluation of one radiation component for one beam mode against a barrier.</summary>
/// <param name="Kind">The radiation component.</param>
/// <param name="BeamModeName">The beam mode evaluated.</param>
/// <param name="EnergyMv">The nominal energy used for table lookups (MV).</param>
/// <param name="TransmissionB">The required barrier transmission factor.</param>
/// <param name="RequiredThicknessMm">The barrier thickness required for this component (mm).</param>
/// <param name="Steps">The ordered calculation steps, for the report.</param>
/// <param name="Notes">Any caveats raised during evaluation (for example a nearest-energy substitution).</param>
public sealed record ComponentResult(
    ComponentKind Kind,
    string BeamModeName,
    int EnergyMv,
    double TransmissionB,
    double RequiredThicknessMm,
    IReadOnlyList<CalculationStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>The evaluation of a single barrier under a single standard.</summary>
/// <param name="BarrierId">The barrier identifier.</param>
/// <param name="Role">The barrier role.</param>
/// <param name="Material">The barrier material from the geometry model.</param>
/// <param name="ProvidedThicknessMm">The thickness present in the geometry model (mm).</param>
/// <param name="RequiredThicknessMm">The controlling required thickness across all components and modes (mm).</param>
/// <param name="GoverningComponent">The component and mode that governed the requirement.</param>
/// <param name="Passes">Whether the provided thickness meets or exceeds the required thickness.</param>
/// <param name="Components">Every component evaluation contributing to this barrier.</param>
public sealed record LinacBarrierEvaluation(
    string BarrierId,
    BarrierRole Role,
    BarrierMaterial Material,
    double ProvidedThicknessMm,
    double RequiredThicknessMm,
    string GoverningComponent,
    bool Passes,
    IReadOnlyList<ComponentResult> Components);

/// <summary>The result of evaluating a layout under one shielding standard.</summary>
/// <param name="StandardName">The name of the standard applied.</param>
/// <param name="Barriers">The per-barrier evaluations.</param>
/// <param name="IsCompliant">Whether every barrier passes.</param>
public sealed record LinacShieldingResult(
    string StandardName,
    IReadOnlyList<LinacBarrierEvaluation> Barriers,
    bool IsCompliant);
