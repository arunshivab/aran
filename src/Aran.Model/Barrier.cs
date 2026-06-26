namespace Aran.Model;

/// <summary>
/// A shielding barrier (wall, floor or ceiling line) recovered from the layout.
/// Every field is a draft proposal until <see cref="IsConfirmed"/> is set true.
/// </summary>
/// <param name="Id">A stable identifier unique within the model.</param>
/// <param name="CentrelineMm">The barrier centreline in millimetre space.</param>
/// <param name="ThicknessMm">The barrier thickness in millimetres.</param>
/// <param name="Material">The proposed construction material.</param>
/// <param name="DensityGramsPerCc">The proposed material density, if known.</param>
/// <param name="RoomAId">The identifier of the room on one side, if resolved.</param>
/// <param name="RoomBId">The identifier of the room on the other side, if resolved.</param>
/// <param name="Provenance">How the barrier was derived.</param>
/// <param name="IsConfirmed">Whether the user has confirmed this barrier.</param>
public sealed record Barrier(
    string Id,
    Polyline CentrelineMm,
    double ThicknessMm,
    BarrierMaterial Material,
    double? DensityGramsPerCc,
    string? RoomAId,
    string? RoomBId,
    Provenance Provenance,
    bool IsConfirmed);
