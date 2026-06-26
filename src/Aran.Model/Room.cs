namespace Aran.Model;

/// <summary>
/// A room recovered from the layout. Every field is a draft proposal until
/// <see cref="IsConfirmed"/> is set true by the verification step.
/// </summary>
/// <param name="Id">A stable identifier unique within the model.</param>
/// <param name="BoundaryMm">The room boundary polygon in millimetre space.</param>
/// <param name="Label">The text label associated with the room, if any.</param>
/// <param name="SuggestedFunction">The proposed functional role of the room.</param>
/// <param name="OccupancyT">The proposed occupancy factor, if assigned.</param>
/// <param name="Classification">The proposed radiation-protection area class.</param>
/// <param name="Provenance">How the room was derived.</param>
/// <param name="IsConfirmed">Whether the user has confirmed this room.</param>
public sealed record Room(
    string Id,
    Polygon BoundaryMm,
    string? Label,
    RoomFunction SuggestedFunction,
    double? OccupancyT,
    AreaClass Classification,
    Provenance Provenance,
    bool IsConfirmed);
