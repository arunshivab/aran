using System;
using System.Collections.Generic;

namespace Aran.Extraction;

/// <summary>
/// Maps CAD optional-content layer names to extraction roles. The default profile
/// recognises the AutoCAD layer naming seen in typical radiotherapy layout exports
/// (for example wall, dimension and door layers).
/// </summary>
public sealed class LayerMap
{
    private static readonly LayerRole[] PriorityOrder = new[]
    {
        LayerRole.Wall,
        LayerRole.Door,
        LayerRole.Dimension,
        LayerRole.Annotation,
        LayerRole.Label,
    };

    private readonly IReadOnlyList<KeyValuePair<string, LayerRole>> _patterns;

    /// <summary>Initialises a new instance from an ordered list of substring patterns.</summary>
    /// <param name="patterns">Lower-case substrings mapped to roles, tested in order.</param>
    public LayerMap(IReadOnlyList<KeyValuePair<string, LayerRole>> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        _patterns = patterns;
    }

    /// <summary>Creates the default layer profile for radiotherapy CAD exports.</summary>
    /// <returns>A configured <see cref="LayerMap"/> instance.</returns>
    public static LayerMap CreateDefault()
    {
        List<KeyValuePair<string, LayerRole>> patterns = new List<KeyValuePair<string, LayerRole>>
        {
            new KeyValuePair<string, LayerRole>("rcc wall", LayerRole.Wall),
            new KeyValuePair<string, LayerRole>("rcc-wall", LayerRole.Wall),
            new KeyValuePair<string, LayerRole>("ar-wall", LayerRole.Wall),
            new KeyValuePair<string, LayerRole>("wall", LayerRole.Wall),
            new KeyValuePair<string, LayerRole>("door", LayerRole.Door),
            new KeyValuePair<string, LayerRole>("anno-dim", LayerRole.Dimension),
            new KeyValuePair<string, LayerRole>("dim", LayerRole.Dimension),
            new KeyValuePair<string, LayerRole>("anno-mark", LayerRole.Annotation),
            new KeyValuePair<string, LayerRole>("mark", LayerRole.Annotation),
            new KeyValuePair<string, LayerRole>("text", LayerRole.Label),
        };
        return new LayerMap(patterns);
    }

    /// <summary>Classifies a single layer name.</summary>
    /// <param name="layerName">The layer name to classify.</param>
    /// <returns>The role matched, or <see cref="LayerRole.Unknown"/>.</returns>
    public LayerRole Classify(string layerName)
    {
        ArgumentNullException.ThrowIfNull(layerName);
        string lower = layerName.ToLowerInvariant();
        foreach (KeyValuePair<string, LayerRole> pattern in _patterns)
        {
            if (lower.Contains(pattern.Key, StringComparison.Ordinal))
            {
                return pattern.Value;
            }
        }

        return LayerRole.Unknown;
    }

    /// <summary>
    /// Determines the strongest role across all layers an operator belongs to,
    /// using a fixed priority so that wall geometry wins over annotation, and so on.
    /// </summary>
    /// <param name="layers">The layer names the operator belongs to.</param>
    /// <returns>The highest-priority role found, or <see cref="LayerRole.Unknown"/>.</returns>
    public LayerRole RoleOf(IReadOnlyList<string> layers)
    {
        ArgumentNullException.ThrowIfNull(layers);
        HashSet<LayerRole> found = new HashSet<LayerRole>();
        foreach (string layer in layers)
        {
            LayerRole role = Classify(layer);
            if (role != LayerRole.Unknown)
            {
                found.Add(role);
            }
        }

        foreach (LayerRole candidate in PriorityOrder)
        {
            if (found.Contains(candidate))
            {
                return candidate;
            }
        }

        return LayerRole.Unknown;
    }
}
