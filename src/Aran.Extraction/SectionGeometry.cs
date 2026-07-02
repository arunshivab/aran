using System.Collections.Generic;
using Aran.Model;

namespace Aran.Extraction;

/// <summary>
/// Vertical (height) geometry read from the section drawing PDF. The section is a
/// separate sheet from the plan; it is the cut through the ceiling-primary barrier and the
/// maze. All values are in millimetres. Every value is a candidate the physicist confirms on
/// the verification canvas; unresolved values are left as <c>null</c> with a diagnostic.
/// </summary>
/// <param name="MazeHeightMm">Maze cross-section height (mm), or null when not resolved.</param>
/// <param name="IsoToCeilingMm">Isocentre to inner ceiling face (mm), or null.</param>
/// <param name="IsoToFloorMm">Isocentre to inner floor face (mm), or null.</param>
/// <param name="VaultInternalHeightMm">Internal vault height = ceiling + floor (mm), or null.</param>
/// <param name="CeilingSlabMm">Ceiling primary slab thickness (mm), or null.</param>
/// <param name="FloorSlabMm">Floor slab thickness (mm), or null.</param>
/// <param name="VoidHeightMm">Void/plenum height above the ceiling (mm), or null.</param>
/// <param name="Provenance">How the values were obtained.</param>
/// <param name="IsConfirmed">Whether the physicist has confirmed these values.</param>
/// <param name="Diagnostics">Any caveats raised while reading the section.</param>
public sealed record SectionGeometry(
    double? MazeHeightMm,
    double? IsoToCeilingMm,
    double? IsoToFloorMm,
    double? VaultInternalHeightMm,
    double? CeilingSlabMm,
    double? FloorSlabMm,
    double? VoidHeightMm,
    Provenance Provenance,
    bool IsConfirmed,
    IReadOnlyList<string> Diagnostics);
