using System.Collections.Generic;

namespace Aran.Engines.Brachy;

/// <summary>One step in a brachytherapy shielding calculation.</summary>
/// <param name="Description">A short description of what the step computes.</param>
/// <param name="Formula">The formula in symbolic form.</param>
/// <param name="Substituted">The formula with actual values substituted.</param>
/// <param name="Result">The computed result of the step.</param>
public sealed record BrachyCalcStep(
    string Description,
    string Formula,
    string Substituted,
    string Result);

/// <summary>The shielding evaluation result for one barrier.</summary>
/// <param name="BarrierId">The barrier identifier.</param>
/// <param name="Isotope">The isotope evaluated.</param>
/// <param name="RequiredThicknessMm">The required barrier thickness (mm).</param>
/// <param name="ProvidedThicknessMm">The provided barrier thickness (mm).</param>
/// <param name="Passes">Whether the provided thickness is adequate.</param>
/// <param name="Steps">The ordered calculation steps.</param>
/// <param name="Notes">Caveats and citations.</param>
public sealed record BrachyBarrierResult(
    string BarrierId,
    string Isotope,
    double RequiredThicknessMm,
    double ProvidedThicknessMm,
    bool Passes,
    IReadOnlyList<BrachyCalcStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>The result of a complete brachytherapy shielding evaluation.</summary>
/// <param name="StandardName">The standard applied.</param>
/// <param name="Barriers">Per-barrier results.</param>
/// <param name="IsCompliant">Whether every barrier passes.</param>
public sealed record BrachyShieldingResult(
    string StandardName,
    IReadOnlyList<BrachyBarrierResult> Barriers,
    bool IsCompliant);
