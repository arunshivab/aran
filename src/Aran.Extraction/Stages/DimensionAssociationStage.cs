using System;
using System.Collections.Generic;
using Aran.Model;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 7. Room labelling — assigns room names and functions to polygons by
/// point-in-polygon matching of label text centroids.
///
/// Assignment strategy (smallest-first):
/// Polygons are tested in ascending area order so that a label centroid contained
/// in a small room polygon wins over a larger enclosing outline. The two largest
/// polygons (building boundary and the primary building outline) are skipped for
/// label containment because they encompass the entire drawing. Labels not
/// contained in any qualifying polygon fall back to nearest-centroid assignment.
///
/// Room function is inferred from keyword matching (case-insensitive) against a
/// fixed vocabulary derived from the AERB RT guidance room taxonomy.
/// Labelled rooms are emitted into <see cref="ModelBuilder"/> as
/// <see cref="Room"/> objects.
/// </summary>
public sealed class DimensionAssociationStage : IExtractionStage
{
    // Number of largest polygons to skip during PiP (building boundary + main outline).
    private const int SkipLargest = 2;

    private static readonly (string Keyword, RoomFunction Function)[] FunctionKeywords =
        new (string, RoomFunction)[]
        {
            ("accelerator", RoomFunction.TreatmentRoom),
            ("treatment", RoomFunction.TreatmentRoom),
            ("linac", RoomFunction.TreatmentRoom),
            ("vault", RoomFunction.TreatmentRoom),
            ("bunker", RoomFunction.TreatmentRoom),
            ("brachytherapy", RoomFunction.TreatmentRoom),
            ("brachy", RoomFunction.TreatmentRoom),
            ("pet", RoomFunction.TreatmentRoom),
            ("gamma knife", RoomFunction.TreatmentRoom),
            ("maze", RoomFunction.Maze),
            ("corridor", RoomFunction.Corridor),
            ("passage", RoomFunction.Corridor),
            ("console", RoomFunction.ControlRoom),
            ("control", RoomFunction.ControlRoom),
            ("operator", RoomFunction.ControlRoom),
            ("waiting", RoomFunction.PublicArea),
            ("reception", RoomFunction.PublicArea),
            ("toilet", RoomFunction.Toilet),
            ("wc", RoomFunction.Toilet),
            ("washroom", RoomFunction.Toilet),
            ("plant", RoomFunction.UtilityRoom),
            ("ups", RoomFunction.UtilityRoom),
            ("utility", RoomFunction.UtilityRoom),
            ("electrical", RoomFunction.UtilityRoom),
            ("server", RoomFunction.UtilityRoom),
            ("store", RoomFunction.UtilityRoom),
            ("office", RoomFunction.Office),
            ("admin", RoomFunction.Office),
            ("staff", RoomFunction.Office),
            ("clinic", RoomFunction.AdjacentClinical),
        };

    /// <inheritdoc />
    public string Name => "Dimension association";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        IReadOnlyList<RoomPolygon> polygons = context.RoomPolygons;
        LayerClassification? classification = context.Classification;
        if (polygons.Count == 0 || classification is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name,
                "No room polygons or no classification; skipping room labelling.");
            return;
        }

        // Work on a mutable copy sorted ascending by area so smallest wins in PiP
        List<RoomPolygon> ascending = new List<RoomPolygon>(polygons);
        ascending.Sort((a, b) => a.AreaRawSq.CompareTo(b.AreaRawSq));

        // Determine the skip threshold: skip the SkipLargest polygons by area
        List<RoomPolygon> descending = new List<RoomPolygon>(polygons);
        // polygons is already sorted descending from stage 6
        double skipThreshold = polygons.Count >= SkipLargest
            ? polygons[SkipLargest - 1].AreaRawSq
            : double.MaxValue;

        // Mutable label accumulator keyed by polygon identity
        Dictionary<RoomPolygon, (string Label, RoomFunction Fn)> assignments =
            new Dictionary<RoomPolygon, (string, RoomFunction)>(ReferenceEqualityComparer.Instance);

        int pip = 0;
        int fallback = 0;

        foreach (TextRun text in classification.LabelTexts)
        {
            if (string.IsNullOrWhiteSpace(text.Unicode))
            {
                continue;
            }

            string trimmed = text.Unicode.Trim();
            double cx = text.BoundingBox.X + (text.BoundingBox.Width / 2.0);
            double cy = text.BoundingBox.Y + (text.BoundingBox.Height / 2.0);

            // PiP: test ascending (smallest first), skip the two largest
            RoomPolygon? matched = null;
            foreach (RoomPolygon poly in ascending)
            {
                if (poly.AreaRawSq >= skipThreshold)
                {
                    continue;
                }

                if (PointInPolygon(cx, cy, poly.Loop.Points))
                {
                    matched = poly;
                    pip++;
                    break;
                }
            }

            // Fallback: nearest qualifying polygon centroid
            if (matched is null)
            {
                double bestDist = double.MaxValue;
                foreach (RoomPolygon poly in ascending)
                {
                    if (poly.AreaRawSq >= skipThreshold)
                    {
                        continue;
                    }

                    RawPoint c = Centroid(poly.Loop.Points);
                    double dx = c.X - cx;
                    double dy = c.Y - cy;
                    double dist = (dx * dx) + (dy * dy);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        matched = poly;
                    }
                }

                if (matched is not null)
                {
                    fallback++;
                }
            }

            if (matched is null)
            {
                continue;
            }

            if (assignments.TryGetValue(matched, out (string Label, RoomFunction Fn) existing))
            {
                string combined = existing.Label + " " + trimmed;
                assignments[matched] = (combined, InferFunction(combined));
            }
            else
            {
                assignments[matched] = (trimmed, InferFunction(trimmed));
            }
        }

        // Rebuild RoomPolygon list with labels applied
        List<RoomPolygon> labelled = new List<RoomPolygon>(polygons.Count);
        foreach (RoomPolygon poly in polygons)
        {
            if (assignments.TryGetValue(poly, out (string Label, RoomFunction Fn) asgn))
            {
                labelled.Add(new RoomPolygon(poly.Loop, poly.AreaRawSq, asgn.Label, asgn.Fn));
            }
            else
            {
                labelled.Add(poly);
            }
        }

        context.RoomPolygons = labelled;

        // Emit labelled rooms into model
        ScaleCalibration? scale = context.Scale;
        int emitted = 0;
        foreach (RoomPolygon poly in labelled)
        {
            if (poly.Label is null)
            {
                continue;
            }

            List<PointMm> pts = new List<PointMm>(poly.Loop.Points.Count);
            foreach (RawPoint rp in poly.Loop.Points)
            {
                double xMm = scale is not null ? rp.X * scale.MillimetresPerUnit : rp.X;
                double yMm = scale is not null ? rp.Y * scale.MillimetresPerUnit : rp.Y;
                pts.Add(new PointMm(xMm, yMm));
            }

            context.Model.AddRoom(new Room(
                context.Model.NextId("R"),
                new Polygon(pts),
                poly.Label,
                poly.Function,
                null,
                AreaClass.Unknown,
                Provenance.GeometryInferred,
                false));
            emitted++;
        }

        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Labelled " + assignments.Count + " polygon(s) (PiP=" + pip + " fallback=" + fallback
                + ") from " + classification.LabelTexts.Count + " label texts; emitted " + emitted + " room(s).");
    }

    private static RoomFunction InferFunction(string label)
    {
        string lower = label.ToLowerInvariant();
        foreach ((string keyword, RoomFunction fn) in FunctionKeywords)
        {
            if (lower.Contains(keyword, StringComparison.Ordinal))
            {
                return fn;
            }
        }

        return RoomFunction.Unknown;
    }

    private static RawPoint Centroid(IReadOnlyList<RawPoint> pts)
    {
        double sx = 0;
        double sy = 0;
        foreach (RawPoint p in pts)
        {
            sx += p.X;
            sy += p.Y;
        }

        return new RawPoint(sx / pts.Count, sy / pts.Count);
    }

    private static bool PointInPolygon(double px, double py, IReadOnlyList<RawPoint> pts)
    {
        int n = pts.Count;
        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            double xi = pts[i].X;
            double yi = pts[i].Y;
            double xj = pts[j].X;
            double yj = pts[j].Y;
            bool intersect =
                ((yi > py) != (yj > py)) &&
                (px < (((xj - xi) * (py - yi)) / (yj - yi)) + xi);
            if (intersect)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
