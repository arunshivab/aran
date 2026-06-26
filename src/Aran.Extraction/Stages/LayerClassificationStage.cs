using System;
using System.Collections.Generic;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 2. Buckets the page's segments and text runs by layer role, so that later
/// stages can work on walls, dimensions and labels directly.
/// </summary>
public sealed class LayerClassificationStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Layer classification";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        List<LineSegment> walls = new List<LineSegment>();
        List<LineSegment> dimensionSegments = new List<LineSegment>();
        List<LineSegment> doors = new List<LineSegment>();
        HashSet<string> distinct = new HashSet<string>(StringComparer.Ordinal);

        foreach (LineSegment segment in context.Segments)
        {
            foreach (string layer in segment.Layers)
            {
                distinct.Add(layer);
            }

            LayerRole role = context.LayerMap.RoleOf(segment.Layers);
            if (role == LayerRole.Wall)
            {
                walls.Add(segment);
            }
            else if (role == LayerRole.Dimension)
            {
                dimensionSegments.Add(segment);
            }
            else if (role == LayerRole.Door)
            {
                doors.Add(segment);
            }
        }

        List<TextRun> labelTexts = new List<TextRun>();
        List<TextRun> dimensionTexts = new List<TextRun>();
        foreach (TextRun text in context.Texts)
        {
            foreach (string layer in text.Layers)
            {
                distinct.Add(layer);
            }

            LayerRole role = context.LayerMap.RoleOf(text.Layers);
            if (role == LayerRole.Label)
            {
                labelTexts.Add(text);
            }
            else if (role == LayerRole.Dimension)
            {
                dimensionTexts.Add(text);
            }
        }

        context.Classification = new LayerClassification(
            walls,
            dimensionSegments,
            doors,
            labelTexts,
            dimensionTexts,
            new List<string>(distinct));

        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Walls=" + walls.Count + " dimSegs=" + dimensionSegments.Count + " doors=" + doors.Count
                + " labelTexts=" + labelTexts.Count + " dimTexts=" + dimensionTexts.Count
                + " layers=" + distinct.Count + ".");
    }
}
