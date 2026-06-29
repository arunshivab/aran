using System;
using System.Collections.Generic;
using Aran.Model;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 6. Free-space detection — converts closed wall loops into room polygons.
///
/// Each closed loop from stage 5 is treated as a candidate room boundary.
/// Loops smaller than a minimum area threshold are dropped (hatch artefacts,
/// furniture outlines, etc.). The remaining loops are stored as
/// <see cref="RoomPolygon"/> objects in <see cref="ExtractionContext.RoomPolygons"/>,
/// with label and function set to defaults pending stage 7 assignment.
///
/// The outermost loop (largest area) is retained as the building boundary but is
/// not emitted as a room — it provides the exterior reference for the maze tracer.
/// </summary>
public sealed class MaterialClassificationStage : IExtractionStage
{
    // Minimum loop area in raw-unit squared to be treated as a room.
    // At 5.47 mm/unit a 2×2 m room has area = (2000/5.47)² ≈ 133,700 raw²
    private const double MinAreaRawSq = 10000.0;

    /// <inheritdoc />
    public string Name => "Material classification";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        IReadOnlyList<WallLoop> loops = context.WallLoops;
        if (loops.Count == 0)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No wall loops from stage 5; skipping room polygon extraction.");
            return;
        }

        List<RoomPolygon> polygons = new List<RoomPolygon>();
        int dropped = 0;
        foreach (WallLoop loop in loops)
        {
            if (!loop.IsClosed)
            {
                continue;
            }

            double area = Math.Abs(WallGraph.SignedArea(loop.Points));
            if (area < MinAreaRawSq)
            {
                dropped++;
                continue;
            }

            polygons.Add(new RoomPolygon(loop, area, null, RoomFunction.Unknown));
        }

        // Sort descending by area so index 0 = largest (building boundary)
        polygons.Sort((a, b) => b.AreaRawSq.CompareTo(a.AreaRawSq));
        context.RoomPolygons = polygons;
        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Extracted " + polygons.Count + " room polygon(s); dropped " + dropped + " below area threshold.");
    }
}
