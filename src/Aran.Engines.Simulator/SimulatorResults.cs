using System.Collections.Generic;

namespace Aran.Engines.Simulator;

/// <summary>One calculation step for the simulator engine.</summary>
/// <param name="Description">Step description.</param>
/// <param name="Formula">Symbolic formula.</param>
/// <param name="Substituted">Formula with values substituted.</param>
/// <param name="Result">Computed result string.</param>
public sealed record SimCalcStep(
    string Description,
    string Formula,
    string Substituted,
    string Result);

/// <summary>The result for one simulator barrier.</summary>
/// <param name="BarrierId">The barrier identifier.</param>
/// <param name="RequiredThicknessMm">Required thickness (mm); 0 when AERB simplified rule applies.</param>
/// <param name="ProvidedThicknessMm">Provided thickness (mm).</param>
/// <param name="Passes">Whether the barrier passes.</param>
/// <param name="AerbSimplifiedStatement">
/// The AERB simplified compliance statement, or null when NCRP method was used.
/// </param>
/// <param name="Steps">Calculation steps (NCRP method only).</param>
/// <param name="Notes">Caveats and citations.</param>
public sealed record SimulatorBarrierResult(
    string BarrierId,
    double RequiredThicknessMm,
    double ProvidedThicknessMm,
    bool Passes,
    string? AerbSimplifiedStatement,
    IReadOnlyList<SimCalcStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>The result of a simulator shielding evaluation.</summary>
/// <param name="StandardName">The standard applied.</param>
/// <param name="Barriers">Per-barrier results.</param>
/// <param name="IsCompliant">Whether every barrier passes.</param>
public sealed record SimulatorShieldingResult(
    string StandardName,
    IReadOnlyList<SimulatorBarrierResult> Barriers,
    bool IsCompliant);
