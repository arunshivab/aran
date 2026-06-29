using System;
using Aran.Engines.Linac;
using Aran.Machines;

namespace Aran.Report.Linac;

/// <summary>
/// All inputs required to render a single shielding report (one standard).
/// Both the wall results and the door evaluation are mandatory; a report cannot
/// be generated without a confirmed maze run.
/// </summary>
/// <param name="FacilityName">Full name of the facility.</param>
/// <param name="FacilityAddress">Full address including city, state and PIN.</param>
/// <param name="FacilityFloor">Floor description (for example "Basement").</param>
/// <param name="DrawingReference">Drawing number and date (for example "R0 — 16.10.2025").</param>
/// <param name="PreparedBy">Name of the technical expert preparing the report.</param>
/// <param name="EloraRpId">eLORA RP registration number of the technical expert.</param>
/// <param name="PreparedDate">Date of preparation.</param>
/// <param name="Machine">The treatment machine.</param>
/// <param name="Input">The shielding evaluation input (geometry, workloads, barriers).</param>
/// <param name="Maze">The confirmed maze run (mandatory).</param>
/// <param name="WallResult">Wall shielding result for the standard.</param>
/// <param name="DoorResult">Door evaluation for the standard.</param>
public sealed record LinacReportInput(
    string FacilityName,
    string FacilityAddress,
    string FacilityFloor,
    string DrawingReference,
    string PreparedBy,
    string EloraRpId,
    DateOnly PreparedDate,
    MachineModel Machine,
    LinacShieldingInput Input,
    MazeRun Maze,
    LinacShieldingResult WallResult,
    DoorEvaluation DoorResult);
