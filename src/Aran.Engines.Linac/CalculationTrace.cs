using System.Collections.Generic;

namespace Aran.Engines.Linac;

/// <summary>
/// One named quantity used in a calculation step, carrying its value, unit and the
/// source it came from, so a report can show provenance next to the number.
/// </summary>
/// <param name="Symbol">The variable symbol as it appears in the formula (for example "P").</param>
/// <param name="Value">The numeric value substituted for the symbol.</param>
/// <param name="Unit">The unit of the value.</param>
/// <param name="Source">The citation or origin of the value.</param>
public sealed record CalculationTerm(string Symbol, double Value, string Unit, string Source);

/// <summary>
/// One step of a calculation: the formula in symbolic form, the same formula with the
/// actual values substituted, the terms used, and the computed result. A report renders
/// the symbolic line followed by the substituted line directly from this record.
/// </summary>
/// <param name="Description">A short description of what the step computes.</param>
/// <param name="Formula">The formula in symbolic form.</param>
/// <param name="Substituted">The formula with actual values substituted for the symbols.</param>
/// <param name="Terms">The input terms used in the step.</param>
/// <param name="Result">The computed result of the step.</param>
public sealed record CalculationStep(
    string Description,
    string Formula,
    string Substituted,
    IReadOnlyList<CalculationTerm> Terms,
    CalculationTerm Result);
