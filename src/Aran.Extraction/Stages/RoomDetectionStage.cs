using System;
using System.Collections.Generic;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 5. Wall body recovery — builds closed room loops from wall outline segments.
///
/// Approach (raster-skeleton alternative via vector graph):
/// 1. Collect all segments classified as Wall (including layer "0" outlines).
/// 2. Snap nearby endpoints into shared nodes within <see cref="WallGraph.SnapTolerance"/>.
/// 3. Trace chains from each node; close chains back to their start to form loops.
/// 4. Store resulting loops in <see cref="ExtractionContext.WallLoops"/>.
///
/// Produces both closed loops (rooms) and open chains (partial or exterior walls).
/// Stage 6 consumes the closed loops to identify room interiors.
/// </summary>
public sealed class RoomDetectionStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Room detection";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        LayerClassification? classification = context.Classification;
        if (classification is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No classification; skipping wall body recovery.");
            return;
        }

        IReadOnlyList<LineSegment> wallSegs = classification.WallSegments;
        if (wallSegs.Count == 0)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No wall segments; no loops can be built.");
            return;
        }

        List<(double, double, double, double)> rawSegs =
            new List<(double, double, double, double)>(wallSegs.Count);
        foreach (LineSegment seg in wallSegs)
        {
            rawSegs.Add((seg.RawX0, seg.RawY0, seg.RawX1, seg.RawY1));
        }

        IReadOnlyList<WallLoop> loops = WallGraph.BuildLoops(rawSegs);

        int closed = 0;
        int open = 0;
        foreach (WallLoop loop in loops)
        {
            if (loop.IsClosed)
            {
                closed++;
            }
            else
            {
                open++;
            }
        }

        context.WallLoops = loops;
        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Built " + loops.Count + " loops (" + closed + " closed, " + open + " open) from "
                + wallSegs.Count + " wall segments.");
    }
}
