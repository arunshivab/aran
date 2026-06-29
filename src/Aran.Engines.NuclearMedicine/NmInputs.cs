using System.Collections.Generic;
using Aran.Model;

namespace Aran.Engines.NuclearMedicine;

/// <summary>A barrier to be evaluated in a nuclear medicine shielding calculation.</summary>
/// <param name="BarrierId">Identifies the barrier.</param>
/// <param name="Material">The barrier material.</param>
/// <param name="ProvidedThicknessMm">The provided barrier thickness (mm).</param>
/// <param name="DistanceMetres">Distance from source to the nearest occupied point (m).</param>
/// <param name="ProtectedClass">Area class of the protected space.</param>
public sealed record NmBarrierInput(
    string BarrierId,
    BarrierMaterial Material,
    double ProvidedThicknessMm,
    double DistanceMetres,
    AreaClass ProtectedClass);

/// <summary>
/// Inputs for a PET/PET-CT facility shielding evaluation (AAPM TG-108 / AERB).
/// AERB default parameters are pre-filled; the physicist may override them.
/// </summary>
/// <param name="PatientsPerWeek">Number of patients scanned per week (max 120 per AERB).</param>
/// <param name="AdministeredActivityMbq">Activity administered per patient (MBq). AERB default: 370.</param>
/// <param name="UptakeTimeMin">Uptake time per patient (minutes). AERB default: 45.</param>
/// <param name="ImagingTimeMin">Imaging time per patient (minutes). AERB default: 30.</param>
/// <param name="UptakeRoomBarriers">Barriers of the uptake / post-admin waiting room.</param>
/// <param name="ImagingRoomBarriers">Barriers of the PET imaging room.</param>
public sealed record PetShieldingInput(
    int PatientsPerWeek,
    double AdministeredActivityMbq,
    double UptakeTimeMin,
    double ImagingTimeMin,
    IReadOnlyList<NmBarrierInput> UptakeRoomBarriers,
    IReadOnlyList<NmBarrierInput> ImagingRoomBarriers);

/// <summary>
/// Inputs for a Gamma Camera / SPECT / SPECT-CT shielding evaluation (AERB).
/// </summary>
/// <param name="ActivityMbqPerPatient">Activity administered per patient (MBq). Tc-99m basis.</param>
/// <param name="PatientsPerWeek">Patients per week.</param>
/// <param name="ImagingTimeHoursPerPatient">Imaging time per patient (hours).</param>
/// <param name="Barriers">Barriers to evaluate.</param>
public sealed record GammaCameraShieldingInput(
    double ActivityMbqPerPatient,
    int PatientsPerWeek,
    double ImagingTimeHoursPerPatient,
    IReadOnlyList<NmBarrierInput> Barriers);

/// <summary>
/// Inputs for a High Dose Therapy (HDT) I-131 shielding evaluation (AERB).
/// </summary>
/// <param name="WeeklyActivityMbq">
/// Maximum activity handled per week (MBq). AERB maximum: 11100 MBq (300 mCi).
/// </param>
/// <param name="OccupancyHoursPerWeek">Hours per week the source is effectively unshielded.</param>
/// <param name="Barriers">Barriers to evaluate.</param>
public sealed record HdtShieldingInput(
    double WeeklyActivityMbq,
    double OccupancyHoursPerWeek,
    IReadOnlyList<NmBarrierInput> Barriers);
