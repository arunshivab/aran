namespace Aran.Engines.Linac;

/// <summary>The shielding role a barrier plays for a treatment machine.</summary>
public enum BarrierRole
{
    /// <summary>Directly struck by the primary beam.</summary>
    Primary,

    /// <summary>Receives only leakage and scattered radiation.</summary>
    Secondary,
}

/// <summary>A radiation component evaluated against a barrier.</summary>
public enum ComponentKind
{
    /// <summary>The primary beam.</summary>
    Primary,

    /// <summary>Head-leakage radiation.</summary>
    Leakage,

    /// <summary>Radiation scattered by the patient.</summary>
    PatientScatter,
}

/// <summary>
/// The occupancy category of a protected location. Each shielding standard resolves
/// the category to an occupancy factor in its own way.
/// </summary>
public enum OccupancyCategory
{
    /// <summary>Full occupancy: offices, control rooms, nurse stations, attended areas.</summary>
    FullOccupancy,

    /// <summary>Treatment room or examination room adjacent to the vault.</summary>
    AdjacentTreatmentRoom,

    /// <summary>Corridors, employee lounges, staff rest rooms.</summary>
    Corridor,

    /// <summary>The treatment vault door.</summary>
    VaultDoor,

    /// <summary>Toilets, storage, unattended waiting, attics, janitors' closets.</summary>
    LimitedOccupancy,

    /// <summary>Outdoor transient traffic, parking, stairways, unattended elevators.</summary>
    TransientOccupancy,
}
