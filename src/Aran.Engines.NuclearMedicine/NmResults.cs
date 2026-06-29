using System.Collections.Generic;

namespace Aran.Engines.NuclearMedicine;

/// <summary>One step in a nuclear medicine shielding calculation.</summary>
/// <param name="Description">Step description.</param>
/// <param name="Formula">Symbolic formula.</param>
/// <param name="Substituted">Formula with values substituted.</param>
/// <param name="Result">Computed result string.</param>
public sealed record NmCalcStep(
    string Description,
    string Formula,
    string Substituted,
    string Result);

/// <summary>
/// The required thickness of a barrier in both concrete and lead, plus the calculation trace.
/// Used for PET where both materials are commonly used.
/// </summary>
/// <param name="BarrierId">The barrier identifier.</param>
/// <param name="RoomType">The room type (for example "Uptake" or "Imaging").</param>
/// <param name="TransmissionB">The required barrier transmission factor.</param>
/// <param name="RequiredConcreteMm">Required thickness in ordinary concrete (mm).</param>
/// <param name="RequiredLeadMm">Required thickness in lead (mm).</param>
/// <param name="ProvidedThicknessMm">Provided thickness (mm).</param>
/// <param name="ProvidedMaterial">The provided barrier material.</param>
/// <param name="Passes">Whether the provided thickness of the provided material is adequate.</param>
/// <param name="Steps">Calculation steps with formula and substituted trace.</param>
/// <param name="Notes">Caveats and citations.</param>
public sealed record PetBarrierResult(
    string BarrierId,
    string RoomType,
    double TransmissionB,
    double RequiredConcreteMm,
    double RequiredLeadMm,
    double ProvidedThicknessMm,
    string ProvidedMaterial,
    bool Passes,
    IReadOnlyList<NmCalcStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>The result of a PET facility shielding evaluation.</summary>
/// <param name="StandardName">The standard applied.</param>
/// <param name="Barriers">All barrier results (uptake + imaging rooms combined).</param>
/// <param name="IsCompliant">Whether every barrier passes.</param>
public sealed record PetShieldingResult(
    string StandardName,
    IReadOnlyList<PetBarrierResult> Barriers,
    bool IsCompliant);

/// <summary>The result for one barrier in a Gamma Camera evaluation.</summary>
/// <param name="BarrierId">The barrier identifier.</param>
/// <param name="WeeklyDoseSvAtBarrier">Calculated weekly dose at the barrier point (Sv/wk).</param>
/// <param name="DesignGoalSv">The AERB design goal (Sv/wk).</param>
/// <param name="ProvidedThicknessMm">Provided barrier thickness (mm).</param>
/// <param name="AerbAdequateMm">AERB minimum adequate thickness for the material (mm).</param>
/// <param name="Passes">Whether the provided thickness meets the AERB rule.</param>
/// <param name="AerbStatement">The AERB simplified compliance statement.</param>
/// <param name="Steps">Calculation steps.</param>
/// <param name="Notes">Caveats.</param>
public sealed record GammaCameraBarrierResult(
    string BarrierId,
    double WeeklyDoseSvAtBarrier,
    double DesignGoalSv,
    double ProvidedThicknessMm,
    double AerbAdequateMm,
    bool Passes,
    string AerbStatement,
    IReadOnlyList<NmCalcStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>Result of a Gamma Camera shielding evaluation.</summary>
/// <param name="StandardName">The standard applied.</param>
/// <param name="Barriers">Per-barrier results.</param>
/// <param name="IsCompliant">Whether every barrier passes.</param>
public sealed record GammaCameraShieldingResult(
    string StandardName,
    IReadOnlyList<GammaCameraBarrierResult> Barriers,
    bool IsCompliant);

/// <summary>The result for one barrier in an HDT I-131 evaluation.</summary>
/// <param name="BarrierId">The barrier identifier.</param>
/// <param name="RequiredThicknessMm">Required thickness in the provided material (mm).</param>
/// <param name="ProvidedThicknessMm">Provided thickness (mm).</param>
/// <param name="Passes">Whether the barrier passes.</param>
/// <param name="Steps">Calculation steps.</param>
/// <param name="Notes">Caveats.</param>
public sealed record HdtBarrierResult(
    string BarrierId,
    double RequiredThicknessMm,
    double ProvidedThicknessMm,
    bool Passes,
    IReadOnlyList<NmCalcStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>Result of an HDT I-131 shielding evaluation.</summary>
/// <param name="StandardName">The standard applied.</param>
/// <param name="Barriers">Per-barrier results.</param>
/// <param name="IsCompliant">Whether every barrier passes.</param>
public sealed record HdtShieldingResult(
    string StandardName,
    IReadOnlyList<HdtBarrierResult> Barriers,
    bool IsCompliant);
