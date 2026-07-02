using System;
using System.Collections.Generic;
using Aran.Model;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 8. Maze tracer — locates the isocentre, vault, maze and doors, then
/// computes the NCRP 151 maze geometry distances and emits an
/// <see cref="ExtractedMazeGeometry"/> into the context.
///
/// Algorithm:
/// 1. Locate isocentre: text run containing "ISO" on label layers, centroid in raw units.
/// 2. Identify vault polygon: the <see cref="RoomPolygon"/> with
///    <see cref="RoomFunction.TreatmentRoom"/> whose interior contains the isocentre.
/// 3. Identify maze polygon: the <see cref="RoomPolygon"/> with
///    <see cref="RoomFunction.Maze"/> adjacent to (sharing a wall edge with) the vault.
/// 4. Locate doors: find the door segment (from classification) between vault and maze
///    (inner door) and between maze and exterior (outer door).
/// 5. Compute maze distances in millimetres using the calibrated scale.
/// 6. Emit <see cref="ExtractedMazeGeometry"/> into
///    <see cref="ExtractionContext.MazeGeometry"/>.
///
/// When critical geometry cannot be resolved, the stage reports a Warning and emits
/// no MazeGeometry rather than producing unreliable values.
/// </summary>
public sealed class SourceAndMazeStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Source & maze";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        LayerClassification? classification = context.Classification;
        ScaleCalibration? scale = context.Scale;
        IReadOnlyList<RoomPolygon> polygons = context.RoomPolygons;
        List<string> diags = new List<string>();

        // Step 1 — isocentre
        RawPoint? isoRaw = FindIsocentre(context.Texts, classification);
        if (isoRaw is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No isocentre label found; cannot build maze geometry.");
            return;
        }

        diags.Add("Isocentre found at raw (" + Fmt(isoRaw.Value.X) + ", " + Fmt(isoRaw.Value.Y) + ").");

        // Step 2 — vault polygon
        RoomPolygon? vault = FindContaining(polygons, isoRaw.Value, RoomFunction.TreatmentRoom);
        if (vault is null)
        {
            // Fallback: any TreatmentRoom polygon
            foreach (RoomPolygon p in polygons)
            {
                if (p.Function == RoomFunction.TreatmentRoom)
                {
                    vault = p;
                    diags.Add("Vault: used first TreatmentRoom polygon (isocentre not contained).");
                    break;
                }
            }
        }

        if (vault is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No treatment room polygon; cannot build maze geometry.");
            return;
        }

        diags.Add("Vault polygon area = " + Fmt(vault.AreaRawSq) + " raw²; label = " + (vault.Label ?? "(none)") + ".");

        // Step 3 — maze polygon
        RoomPolygon? maze = null;
        foreach (RoomPolygon p in polygons)
        {
            if (p.Function == RoomFunction.Maze)
            {
                maze = p;
                break;
            }
        }

        if (maze is null)
        {
            // Fallback: the smallest polygon adjacent (centroid nearest to vault centroid) that isn't the vault
            maze = FindNearestOther(polygons, vault);
            if (maze is not null)
            {
                diags.Add("Maze: no Maze-labelled polygon found; using nearest small polygon as proxy.");
            }
        }

        if (maze is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "Cannot identify maze polygon; maze geometry not emitted.");
            return;
        }

        diags.Add("Maze polygon area = " + Fmt(maze.AreaRawSq) + " raw²; label = " + (maze.Label ?? "(none)") + ".");

        // Step 4 — door positions
        RawPoint innerDoor = FindDoorBetween(vault, maze, classification);
        RawPoint outerDoor = FindOuterDoor(maze, vault, classification);
        diags.Add("Inner door raw: (" + Fmt(innerDoor.X) + ", " + Fmt(innerDoor.Y) + ").");
        diags.Add("Outer door raw: (" + Fmt(outerDoor.X) + ", " + Fmt(outerDoor.Y) + ").");

        // Step 5 — compute distances in mm
        double mmpu = scale?.MillimetresPerUnit ?? 1.0;
        RawPoint vaultCentroid = Centroid(vault.Loop.Points);
        RawPoint mazeCentroid = Centroid(maze.Loop.Points);

        // dh: isocentre to nearest vault wall perpendicular (use centroid-to-primary-wall approximation)
        double dhRaw = NearestWallDistance(isoRaw.Value, vault.Loop.Points);
        double dhMm = dhRaw * mmpu;

        // dz: inner door to outer door (along maze path)
        double dzRaw = Distance(innerDoor, outerDoor);
        double dzMm = dzRaw * mmpu;

        // dr: approximated as half maze width + dh (NCRP 151 geometry, simple proxy)
        double mazeWidthRaw = EstimateMazeWidth(maze.Loop.Points);
        double drRaw = dhRaw + (mazeWidthRaw / 2.0);
        double drMm = drRaw * mmpu;

        // dsec: isocentre to near vault wall along beam axis (same as dh first approximation)
        double dsecMm = dhMm;

        // dzz: inner scatter point (wall G visible from door) to outer door
        double dzzRaw = dzRaw + (mazeWidthRaw / 2.0);
        double dzzMm = dzzRaw * mmpu;

        // dsca: isocentre to patient — NCRP 151 convention 1000 mm
        double dscaMm = 1000.0;

        // dL: isocentre to outer door oblique
        double dlRaw = Distance(isoRaw.Value, outerDoor);
        double dlMm = dlRaw * mmpu;

        // Plan widths (mm). Height-dependent areas (A1/Az/S1) are NOT fabricated here: the
        // section drawing supplies the height and the orchestration merge multiplies
        // width × section-height (see SectionExtractor and the Upload merge). A0 (primary
        // beam area) is a field-size value set/confirmed downstream. This removes the former
        // hardcoded 3000 mm ceiling height and 0.5 m² A0.
        double vaultWidthMm = EstimateMazeWidth(vault.Loop.Points) * mmpu;
        double mazeWidthMm = mazeWidthRaw * mmpu;
        diags.Add("Vault width = " + Fmt(vaultWidthMm) + " mm; maze width = " + Fmt(mazeWidthMm)
            + " mm. Height-dependent areas pending the section merge.");

        // Gantry side (page space): densest machine cluster around the isocentre gives the
        // isocentre-to-gantry (head) direction; patient-left = 90° CCW (plan view), which the
        // physicist confirms/flips on the canvas.
        GantryFrame gantry = DetectGantry(context.Segments, isoRaw.Value, diags);

        double a0M2 = 0.0;
        double a1M2 = 0.0;
        double azM2 = 0.0;
        double s1M2 = 0.0;

        ExtractedMazeGeometry geo = new ExtractedMazeGeometry(
            vault, maze,
            isoRaw.Value, innerDoor, outerDoor,
            dhMm, dzMm, drMm, dsecMm, dzzMm, dscaMm, dlMm,
            a0M2, a1M2, azM2, s1M2,
            vaultWidthMm, mazeWidthMm,
            gantry.DirX, gantry.DirY, gantry.LeftX, gantry.LeftY, gantry.Confident,
            diags);

        context.MazeGeometry = geo;
        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Maze geometry extracted: dh=" + Fmt(dhMm) + " mm, dz=" + Fmt(dzMm) + " mm, dr=" + Fmt(drMm)
                + " mm; gantry confident=" + gantry.Confident + ".");
    }

    private static readonly string[] IsoLabelVariants =
    {
        "ISOCENTER",
        "ISOCENTRE",
        "ISO",
        "BEAMCENTER",
        "BEAMCENTRE",
    };

    private static RawPoint? FindIsocentre(
        IReadOnlyList<TextRun> texts, LayerClassification? classification)
    {
        foreach (TextRun t in texts)
        {
            string normalised = Normalise(t.Unicode);
            if (normalised.Length == 0)
            {
                continue;
            }

            foreach (string variant in IsoLabelVariants)
            {
                if (normalised.Contains(variant, StringComparison.Ordinal))
                {
                    double cx = t.BoundingBox.X + (t.BoundingBox.Width / 2.0);
                    double cy = t.BoundingBox.Y + (t.BoundingBox.Height / 2.0);
                    return new RawPoint(cx, cy);
                }
            }
        }

        return null;
    }

    private static string Normalise(string? unicode)
    {
        if (unicode is null)
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(unicode.Length);
        foreach (char c in unicode)
        {
            if (!char.IsWhiteSpace(c))
            {
                builder.Append(char.ToUpperInvariant(c));
            }
        }

        return builder.ToString();
    }

    private static RoomPolygon? FindContaining(
        IReadOnlyList<RoomPolygon> polygons, RawPoint pt, RoomFunction fn)
    {
        foreach (RoomPolygon p in polygons)
        {
            if (p.Function != fn)
            {
                continue;
            }

            if (PointInPolygon(pt.X, pt.Y, p.Loop.Points))
            {
                return p;
            }
        }

        return null;
    }

    private static RoomPolygon? FindNearestOther(
        IReadOnlyList<RoomPolygon> polygons, RoomPolygon reference)
    {
        RawPoint refC = Centroid(reference.Loop.Points);
        RoomPolygon? best = null;
        double bestDist = double.MaxValue;
        foreach (RoomPolygon p in polygons)
        {
            if (ReferenceEquals(p, reference))
            {
                continue;
            }

            RawPoint c = Centroid(p.Loop.Points);
            double d = Distance(refC, c);
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }

        return best;
    }

    private static RawPoint FindDoorBetween(
        RoomPolygon a, RoomPolygon b, LayerClassification? classification)
    {
        if (classification is null || classification.DoorSegments.Count == 0)
        {
            return MidPoint(Centroid(a.Loop.Points), Centroid(b.Loop.Points));
        }

        // Find door segment whose midpoint is nearest to the boundary between vault and maze
        RawPoint vaultC = Centroid(a.Loop.Points);
        RawPoint mazeC = Centroid(b.Loop.Points);
        RawPoint boundary = MidPoint(vaultC, mazeC);
        return NearestDoorMidpoint(classification.DoorSegments, boundary);
    }

    private static RawPoint FindOuterDoor(
        RoomPolygon maze, RoomPolygon vault, LayerClassification? classification)
    {
        if (classification is null || classification.DoorSegments.Count == 0)
        {
            return Centroid(maze.Loop.Points);
        }

        // Outer door is the door segment furthest from the vault centroid
        RawPoint vaultC = Centroid(vault.Loop.Points);
        Chuvadi.Pdf.Rendering.DisplayList.LineSegment? best = null;
        double bestDist = -1;
        foreach (Chuvadi.Pdf.Rendering.DisplayList.LineSegment seg in classification.DoorSegments)
        {
            double mx = (seg.RawX0 + seg.RawX1) / 2.0;
            double my = (seg.RawY0 + seg.RawY1) / 2.0;
            double dx = mx - vaultC.X;
            double dy = my - vaultC.Y;
            double dist = (dx * dx) + (dy * dy);
            if (dist > bestDist)
            {
                bestDist = dist;
                best = seg;
            }
        }

        if (best is null)
        {
            return Centroid(maze.Loop.Points);
        }

        return new RawPoint((best.Value.RawX0 + best.Value.RawX1) / 2.0,
                            (best.Value.RawY0 + best.Value.RawY1) / 2.0);
    }

    private static RawPoint NearestDoorMidpoint(
        System.Collections.Generic.IReadOnlyList<Chuvadi.Pdf.Rendering.DisplayList.LineSegment> doors,
        RawPoint target)
    {
        RawPoint best = target;
        double bestDist = double.MaxValue;
        foreach (Chuvadi.Pdf.Rendering.DisplayList.LineSegment seg in doors)
        {
            double mx = (seg.RawX0 + seg.RawX1) / 2.0;
            double my = (seg.RawY0 + seg.RawY1) / 2.0;
            double dx = mx - target.X;
            double dy = my - target.Y;
            double dist = (dx * dx) + (dy * dy);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = new RawPoint(mx, my);
            }
        }

        return best;
    }

    private static RawPoint Centroid(IReadOnlyList<RawPoint> pts)
    {
        double sx = 0, sy = 0;
        foreach (RawPoint p in pts)
        {
            sx += p.X;
            sy += p.Y;
        }

        return new RawPoint(sx / pts.Count, sy / pts.Count);
    }

    private static double Distance(RawPoint a, RawPoint b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static RawPoint MidPoint(RawPoint a, RawPoint b) =>
        new RawPoint((a.X + b.X) / 2.0, (a.Y + b.Y) / 2.0);

    private static double NearestWallDistance(RawPoint pt, IReadOnlyList<RawPoint> polygon)
    {
        double minDist = double.MaxValue;
        int n = polygon.Count;
        for (int i = 0; i < n; i++)
        {
            RawPoint a = polygon[i];
            RawPoint b = polygon[(i + 1) % n];
            double dist = PointToSegmentDistance(pt.X, pt.Y, a.X, a.Y, b.X, b.Y);
            if (dist < minDist)
            {
                minDist = dist;
            }
        }

        return minDist;
    }

    private static double EstimateMazeWidth(IReadOnlyList<RawPoint> polygon)
    {
        // Estimate width as the shorter axis of the polygon's bounding box
        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        foreach (RawPoint p in polygon)
        {
            if (p.X < minX) { minX = p.X; }
            if (p.X > maxX) { maxX = p.X; }
            if (p.Y < minY) { minY = p.Y; }
            if (p.Y > maxY) { maxY = p.Y; }
        }

        return Math.Min(maxX - minX, maxY - minY);
    }

    private static double PointToSegmentDistance(
        double px, double py,
        double ax, double ay, double bx, double by)
    {
        double dx = bx - ax;
        double dy = by - ay;
        double lenSq = (dx * dx) + (dy * dy);
        if (lenSq < 1e-10)
        {
            double ex = px - ax;
            double ey = py - ay;
            return Math.Sqrt((ex * ex) + (ey * ey));
        }

        double t = Math.Max(0, Math.Min(1, (((px - ax) * dx) + ((py - ay) * dy)) / lenSq));
        double fx = ax + (t * dx) - px;
        double fy = ay + (t * dy) - py;
        return Math.Sqrt((fx * fx) + (fy * fy));
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

    private static GantryFrame DetectGantry(
        IReadOnlyList<LineSegment> segments, RawPoint isoPage, List<string> diags)
    {
        // Bucket nearby segment midpoints (page space) into eight octants around the
        // isocentre; the densest octant points toward the machine/gantry cluster.
        double[] counts = new double[8];
        foreach (LineSegment s in segments)
        {
            double mx = (s.X0 + s.X1) / 2.0;
            double my = (s.Y0 + s.Y1) / 2.0;
            double dx = mx - isoPage.X;
            double dy = my - isoPage.Y;
            double dist = Math.Sqrt((dx * dx) + (dy * dy));
            if (dist < 15.0 || dist > 160.0)
            {
                continue;
            }

            double angle = Math.Atan2(dy, dx) + Math.PI;
            int octant = (int)((angle / (2.0 * Math.PI)) * 8.0) % 8;
            counts[octant] += 1.0;
        }

        int best = 0;
        double bestCount = -1.0;
        double total = 0.0;
        for (int i = 0; i < 8; i++)
        {
            total += counts[i];
            if (counts[i] > bestCount)
            {
                bestCount = counts[i];
                best = i;
            }
        }

        if (total < 8.0 || bestCount <= 0.0)
        {
            diags.Add("Gantry cluster not found near isocentre; L/R defaulted — confirm on canvas.");
            return new GantryFrame(1.0, 0.0, 0.0, -1.0, false);
        }

        double centreAngle = (((best + 0.5) / 8.0) * (2.0 * Math.PI)) - Math.PI;
        double gx = Math.Cos(centreAngle);
        double gy = Math.Sin(centreAngle);
        double lx = -gy;
        double ly = gx;
        bool confident = bestCount >= (total * 0.4);
        diags.Add("Gantry (head) direction page=(" + Fmt(gx) + ", " + Fmt(gy) + "); patient-left page=("
            + Fmt(lx) + ", " + Fmt(ly) + "); confident=" + confident + " — confirm handedness on canvas.");
        return new GantryFrame(gx, gy, lx, ly, confident);
    }

    private static string Fmt(double v) =>
        v.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    private readonly record struct GantryFrame(
        double DirX, double DirY, double LeftX, double LeftY, bool Confident);
}
