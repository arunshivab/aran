using System;
using System.Collections.Generic;

namespace Aran.Model;

/// <summary>
/// The complete draft geometry extracted from a facility layout: the scale,
/// the rooms, the barriers and the sources. This is the single contract consumed
/// by the verification canvas and, once confirmed, by the physics engines.
/// </summary>
/// <param name="Scale">The scale calibration, or null if calibration failed.</param>
/// <param name="Rooms">The rooms recovered from the layout.</param>
/// <param name="Barriers">The barriers recovered from the layout.</param>
/// <param name="Sources">The radiation sources recovered from the layout.</param>
public sealed record ShieldingGeometryModel(
    ScaleCalibration? Scale,
    IReadOnlyList<Room> Rooms,
    IReadOnlyList<Barrier> Barriers,
    IReadOnlyList<RadiationSource> Sources)
{
    /// <summary>An empty model with no scale and no extracted elements.</summary>
    public static ShieldingGeometryModel Empty { get; } = new ShieldingGeometryModel(
        null,
        Array.Empty<Room>(),
        Array.Empty<Barrier>(),
        Array.Empty<RadiationSource>());
}
