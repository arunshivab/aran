namespace Aran.Model;

/// <summary>Describes how an extracted value was derived, for review and audit.</summary>
public enum Provenance
{
    /// <summary>Classified from an optional-content (CAD) layer name.</summary>
    FromLayer,

    /// <summary>Read from a printed dimension annotation.</summary>
    FromDimensionText,

    /// <summary>Inferred by matching a hatch region against a legend swatch.</summary>
    TextureMatched,

    /// <summary>Inferred from raw geometry without a stronger signal.</summary>
    GeometryInferred,

    /// <summary>Entered or corrected manually by the user.</summary>
    Manual,
}

/// <summary>The functional role of a room, which drives occupancy and area class.</summary>
public enum RoomFunction
{
    /// <summary>Role not yet determined.</summary>
    Unknown,

    /// <summary>The shielded treatment or imaging vault.</summary>
    TreatmentRoom,

    /// <summary>The operator control room.</summary>
    ControlRoom,

    /// <summary>A maze passage leading into the vault.</summary>
    Maze,

    /// <summary>A corridor or general circulation space.</summary>
    Corridor,

    /// <summary>An office or staff work area.</summary>
    Office,

    /// <summary>A toilet or washroom.</summary>
    Toilet,

    /// <summary>A plant, utility or services room.</summary>
    UtilityRoom,

    /// <summary>A waiting area or other public space.</summary>
    PublicArea,

    /// <summary>An adjacent clinical area such as a lab or ward.</summary>
    AdjacentClinical,

    /// <summary>An area not normally occupied.</summary>
    UnoccupiedArea,
}

/// <summary>The radiation-protection classification of an area.</summary>
public enum AreaClass
{
    /// <summary>Classification not yet determined.</summary>
    Unknown,

    /// <summary>A controlled area subject to occupational dose limits.</summary>
    Controlled,

    /// <summary>An uncontrolled area subject to public dose limits.</summary>
    Uncontrolled,
}

/// <summary>The construction material of a barrier.</summary>
public enum BarrierMaterial
{
    /// <summary>Material not yet determined.</summary>
    Unknown,

    /// <summary>Poured concrete.</summary>
    Concrete,

    /// <summary>Brick masonry.</summary>
    Brick,

    /// <summary>Natural earth or backfill.</summary>
    NaturalEarth,

    /// <summary>Lead sheet or plate.</summary>
    Lead,

    /// <summary>Structural steel.</summary>
    Steel,
}

/// <summary>The class of radiation-producing machine occupying a vault.</summary>
public enum MachineType
{
    /// <summary>Machine not yet determined.</summary>
    Unknown,

    /// <summary>Varian TrueBeam class megavoltage linear accelerator.</summary>
    LinacTrueBeam,

    /// <summary>Varian Halcyon class ring-gantry linear accelerator.</summary>
    LinacHalcyon,

    /// <summary>Helical tomotherapy unit.</summary>
    LinacTomo,

    /// <summary>High dose-rate brachytherapy afterloader.</summary>
    HdrBrachy,

    /// <summary>Computed-tomography simulator.</summary>
    CtSimulator,

    /// <summary>Positron-emission-tomography combined imaging unit.</summary>
    PetCt,
}
