using System.Collections.Generic;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction;

/// <summary>
/// The result of bucketing a page's line segments and text runs by layer role.
/// Produced by the layer-classification stage and consumed by later stages.
/// </summary>
/// <param name="WallSegments">Segments on wall layers.</param>
/// <param name="DimensionSegments">Segments on dimension layers.</param>
/// <param name="DoorSegments">Segments on door layers.</param>
/// <param name="LabelTexts">Text runs on label layers.</param>
/// <param name="DimensionTexts">Text runs on dimension layers.</param>
/// <param name="DistinctLayers">All distinct layer names seen on the page.</param>
public sealed record LayerClassification(
    IReadOnlyList<LineSegment> WallSegments,
    IReadOnlyList<LineSegment> DimensionSegments,
    IReadOnlyList<LineSegment> DoorSegments,
    IReadOnlyList<TextRun> LabelTexts,
    IReadOnlyList<TextRun> DimensionTexts,
    IReadOnlyList<string> DistinctLayers);
