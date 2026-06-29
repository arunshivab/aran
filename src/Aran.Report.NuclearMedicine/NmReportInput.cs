using System;
using Aran.Engines.NuclearMedicine;

namespace Aran.Report.NuclearMedicine;

/// <summary>
/// All inputs required to render a single AERB nuclear medicine shielding report.
/// Supply only the modalities present in the facility; null means the modality
/// is not installed and will be omitted from the report.
/// </summary>
/// <param name="FacilityName">Full name of the facility.</param>
/// <param name="FacilityAddress">Full address including city, state and PIN.</param>
/// <param name="FacilityFloor">Floor description (for example "Ground Floor").</param>
/// <param name="DrawingReference">Drawing number and date.</param>
/// <param name="PreparedBy">Name of the technical expert.</param>
/// <param name="EloraRpId">eLORA RP registration number of the technical expert.</param>
/// <param name="PreparedDate">Date of preparation.</param>
/// <param name="InstallationTypes">
/// Comma-separated list of installation types for the undertaking page
/// (for example "PET/PET-CT, Gamma Camera/SPECT").
/// </param>
/// <param name="PetInput">PET/PET-CT shielding input; null when not installed.</param>
/// <param name="PetResult">PET shielding result; null when not installed.</param>
/// <param name="GammaCameraInput">Gamma Camera input; null when not installed.</param>
/// <param name="GammaCameraResult">Gamma Camera result; null when not installed.</param>
/// <param name="HdtInput">HDT I-131 input; null when not installed.</param>
/// <param name="HdtResult">HDT result; null when not installed.</param>
public sealed record NmReportInput(
    string FacilityName,
    string FacilityAddress,
    string FacilityFloor,
    string DrawingReference,
    string PreparedBy,
    string EloraRpId,
    DateOnly PreparedDate,
    string InstallationTypes,
    PetShieldingInput? PetInput,
    PetShieldingResult? PetResult,
    GammaCameraShieldingInput? GammaCameraInput,
    GammaCameraShieldingResult? GammaCameraResult,
    HdtShieldingInput? HdtInput,
    HdtShieldingResult? HdtResult);
