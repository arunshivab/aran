using System;
using System.Collections.Generic;
using Aran.Model;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 4. Reconstructs barriers by pairing parallel wall segments that sit a
/// plausible wall thickness apart and overlap along their length. Each pair yields
/// a barrier centreline and thickness in millimetres. This is a first-cut pairing:
/// it is greedy and assumes the scale has been calibrated. Without a scale the stage
/// flags the page and produces no barriers rather than guessing a thickness.
/// </summary>
public sealed class WallReconstructionStage : IExtractionStage
{
    private const double MinThicknessMm = 50.0;
    private const double MaxThicknessMm = 2000.0;
    private const double ParallelSineTolerance = 0.035;
    private const double MinSegmentRawLength = 1.0;

    /// <inheritdoc />
    public string Name => "Wall reconstruction";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        LayerClassification? classification = context.Classification;
        if (classification is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No classification available; cannot reconstruct walls.");
            return;
        }

        ScaleCalibration? scale = context.Scale;
        if (scale is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No scale calibrated; skipping wall reconstruction.");
            return;
        }

        double mmPerUnit = scale.MillimetresPerUnit;
        double minThicknessRaw = MinThicknessMm / mmPerUnit;
        double maxThicknessRaw = MaxThicknessMm / mmPerUnit;
        IReadOnlyList<LineSegment> walls = classification.WallSegments;
        bool[] used = new bool[walls.Count];
        int barrierCount = 0;

        for (int i = 0; i < walls.Count; i++)
        {
            if (used[i])
            {
                continue;
            }

            double ix0 = walls[i].RawX0;
            double iy0 = walls[i].RawY0;
            double ix1 = walls[i].RawX1;
            double iy1 = walls[i].RawY1;
            double idx = ix1 - ix0;
            double idy = iy1 - iy0;
            double iLength = Math.Sqrt((idx * idx) + (idy * idy));
            if (iLength < MinSegmentRawLength)
            {
                continue;
            }

            double iux = idx / iLength;
            double iuy = idy / iLength;

            for (int j = i + 1; j < walls.Count; j++)
            {
                if (used[j])
                {
                    continue;
                }

                double jx0 = walls[j].RawX0;
                double jy0 = walls[j].RawY0;
                double jx1 = walls[j].RawX1;
                double jy1 = walls[j].RawY1;
                double jdx = jx1 - jx0;
                double jdy = jy1 - jy0;
                double jLength = Math.Sqrt((jdx * jdx) + (jdy * jdy));
                if (jLength < MinSegmentRawLength)
                {
                    continue;
                }

                double jux = jdx / jLength;
                double juy = jdy / jLength;

                double sine = Math.Abs((iux * juy) - (iuy * jux));
                if (sine > ParallelSineTolerance)
                {
                    continue;
                }

                double perpDistance = Math.Abs(((jx0 - ix0) * (-iuy)) + ((jy0 - iy0) * iux));
                if (perpDistance < minThicknessRaw || perpDistance > maxThicknessRaw)
                {
                    continue;
                }

                double t0 = 0.0;
                double t1 = iLength;
                double tj0 = ((jx0 - ix0) * iux) + ((jy0 - iy0) * iuy);
                double tj1 = ((jx1 - ix0) * iux) + ((jy1 - iy0) * iuy);
                double jMin = Math.Min(tj0, tj1);
                double jMax = Math.Max(tj0, tj1);
                double overlapMin = Math.Max(t0, jMin);
                double overlapMax = Math.Min(t1, jMax);
                if (overlapMax - overlapMin <= 0.0)
                {
                    continue;
                }

                double nx = -iuy;
                double ny = iux;
                double side = ((jx0 - ix0) * nx) + ((jy0 - iy0) * ny);
                double sign = side >= 0.0 ? 1.0 : -1.0;
                double halfGap = perpDistance / 2.0;

                double startX = (ix0 + (iux * overlapMin)) + (nx * halfGap * sign);
                double startY = (iy0 + (iuy * overlapMin)) + (ny * halfGap * sign);
                double endX = (ix0 + (iux * overlapMax)) + (nx * halfGap * sign);
                double endY = (iy0 + (iuy * overlapMax)) + (ny * halfGap * sign);

                List<PointMm> centreline = new List<PointMm>
                {
                    new PointMm(startX * mmPerUnit, startY * mmPerUnit),
                    new PointMm(endX * mmPerUnit, endY * mmPerUnit),
                };

                Barrier barrier = new Barrier(
                    context.Model.NextId("B"),
                    new Polyline(centreline),
                    perpDistance * mmPerUnit,
                    BarrierMaterial.Unknown,
                    null,
                    null,
                    null,
                    Provenance.FromLayer,
                    false);
                context.Model.AddBarrier(barrier);
                barrierCount++;
                used[i] = true;
                used[j] = true;
                break;
            }
        }

        context.Report(DiagnosticSeverity.Info, Name, "Reconstructed " + barrierCount + " barrier(s) from wall pairs.");
    }
}
