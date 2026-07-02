using System.Collections.Generic;
using Aran.Model;

namespace Aran.Extraction;

/// <summary>
/// A room polygon in raw drawing units, with optional label assignment.
/// </summary>
/// <param name="Loop">The closed wall loop defining the room boundary.</param>
/// <param name="AreaRawSq">The area of the polygon in raw-unit squared.</param>
/// <param name="Label">The room label text, or null when not yet assigned.</param>
/// <param name="Function">The inferred room function, or Unknown when not yet assigned.</param>
public sealed record RoomPolygon(
    WallLoop Loop,
    double AreaRawSq,
    string? Label,
    RoomFunction Function);

/// <summary>
/// Extracted maze geometry in raw drawing units and millimetres.
/// Produced by stage 8. Converted to <c>MazeRun</c> by the UI/orchestration layer.
/// </summary>
/// <param name="VaultPolygon">The treatment vault room polygon.</param>
/// <param name="MazePolygon">The maze corridor room polygon.</param>
/// <param name="IsocentreRaw">The isocentre position in raw drawing units.</param>
/// <param name="InnerDoorCentreRaw">The inner maze door centre in raw units.</param>
/// <param name="OuterDoorCentreRaw">The outer maze door centre in raw units.</param>
/// <param name="DhMm">Distance from isocentre to primary wall, perpendicular (mm).</param>
/// <param name="DzMm">Distance from inner door centre to outer door centre (mm).</param>
/// <param name="DrMm">Distance from first-scatter point on wall G to maze centreline (mm).</param>
/// <param name="DsecMm">Distance from isocentre to wall G along beam axis (mm).</param>
/// <param name="DzzMm">Distance from scatter surface to outer door (mm).</param>
/// <param name="DscaMm">Distance from isocentre to patient (typically 1000 mm).</param>
/// <param name="DlMm">Distance from isocentre to outer door oblique (mm).</param>
/// <param name="A0M2">Primary beam area at wall G (m²).</param>
/// <param name="A1M2">Wall G area visible from door (m²).</param>
/// <param name="AzM2">Inner maze opening cross-section area (m²).</param>
/// <param name="MazeCrossM2">Maze cross-sectional area S1 (m²).</param>
/// <param name="VaultWidthMm">Plan width of the vault lateral span (mm); the merge multiplies by the section height.</param>
/// <param name="MazeWidthMm">Plan width of the maze throat (mm); the merge multiplies by the section height.</param>
/// <param name="GantryDirXPage">X of the isocentre-to-gantry unit direction, page space.</param>
/// <param name="GantryDirYPage">Y of the isocentre-to-gantry unit direction, page space.</param>
/// <param name="PatientLeftDirXPage">X of the patient-left unit direction (90° CCW of gantry), page space.</param>
/// <param name="PatientLeftDirYPage">Y of the patient-left unit direction (90° CCW of gantry), page space.</param>
/// <param name="GantryConfident">True when the gantry side came from a dense machine cluster; false means confirm on the canvas.</param>
/// <param name="Diagnostics">Any caveats from the tracer.</param>
public sealed record ExtractedMazeGeometry(
    RoomPolygon VaultPolygon,
    RoomPolygon MazePolygon,
    RawPoint IsocentreRaw,
    RawPoint InnerDoorCentreRaw,
    RawPoint OuterDoorCentreRaw,
    double DhMm,
    double DzMm,
    double DrMm,
    double DsecMm,
    double DzzMm,
    double DscaMm,
    double DlMm,
    double A0M2,
    double A1M2,
    double AzM2,
    double MazeCrossM2,
    double VaultWidthMm,
    double MazeWidthMm,
    double GantryDirXPage,
    double GantryDirYPage,
    double PatientLeftDirXPage,
    double PatientLeftDirYPage,
    bool GantryConfident,
    IReadOnlyList<string> Diagnostics);
