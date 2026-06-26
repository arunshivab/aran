namespace Aran.Extraction;

/// <summary>The extraction role assigned to a CAD optional-content layer.</summary>
public enum LayerRole
{
    /// <summary>The layer has no recognised role.</summary>
    Unknown,

    /// <summary>The layer carries wall geometry.</summary>
    Wall,

    /// <summary>The layer carries dimension annotations.</summary>
    Dimension,

    /// <summary>The layer carries door geometry.</summary>
    Door,

    /// <summary>The layer carries room or general text labels.</summary>
    Label,

    /// <summary>The layer carries marks or non-dimension annotations.</summary>
    Annotation,
}
