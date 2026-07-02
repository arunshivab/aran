using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Aran.Model;
using Chuvadi.Pdf.Documents;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction;

/// <summary>
/// Reads vertical (height) geometry from the section drawing PDF into a
/// <see cref="SectionGeometry"/>. The section is a separate sheet from the plan (the cut
/// through the ceiling-primary barrier and the maze). Heights are anchored on the isocentre
/// and maze text labels, whose wording varies between clients, so the anchor match is
/// tolerant (ISO / ISOCENTER / ISOCENTRE / ISO CENTER / BEAM CENTER; MAZE).
///
/// Algorithm (verified against a real client section):
/// 1. Locate the isocentre label (tolerant match) and the maze label.
/// 2. ISO to ceiling / floor: the dimension pair sharing a common dimension line (equal
///    cross-axis coordinate) that tightly flanks the isocentre along the height axis. The
///    member on the ceiling side is ISO to ceiling; the member on the floor side is ISO to
///    floor. Their sum is cross-checked against a printed total near the isocentre.
/// 3. Maze height: the dimension nearest the maze label.
/// 4. Ceiling slab: the largest dimension in the ceiling band.
/// Every value is a candidate; unresolved values are null with a diagnostic, for the canvas.
/// </summary>
public sealed class SectionExtractor
{
    private static readonly string[] IsoLabelVariants =
    {
        "ISOCENTER",
        "ISOCENTRE",
        "ISO",
        "BEAMCENTER",
        "BEAMCENTRE",
    };

    private const double SameLineTolerance = 8.0;
    private const double IsoDimensionBand = 260.0;
    private const double MazeDimensionBand = 150.0;
    private const double TotalCrossCheckTolerance = 2.0;

    /// <summary>
    /// Extracts <see cref="SectionGeometry"/> from the given section PDF page.
    /// </summary>
    /// <param name="document">The section PDF document.</param>
    /// <param name="pageIndex">The zero-based page index of the section.</param>
    /// <returns>The extracted section geometry, with unresolved values left null.</returns>
    public SectionGeometry Extract(PdfDocument document, int pageIndex)
    {
        ArgumentNullException.ThrowIfNull(document);

        IReadOnlyList<TextRun> texts = PdfPageExtensions.GetTextRuns(document, pageIndex);
        List<string> diagnostics = new List<string>();

        List<DimensionText> numbers = new List<DimensionText>();
        DimensionPoint? iso = null;
        DimensionPoint? maze = null;

        foreach (TextRun run in texts)
        {
            string normalised = Normalise(run.Unicode);
            if (normalised.Length == 0)
            {
                continue;
            }

            if (double.TryParse(normalised, NumberStyles.Any, CultureInfo.InvariantCulture, out double value)
                && value >= 100.0 && value <= 20000.0)
            {
                numbers.Add(new DimensionText(value, Centre(run)));
            }

            if (iso is null && IsIsocentreLabel(normalised))
            {
                iso = Centre(run);
            }

            if (maze is null && normalised.Contains("MAZE", StringComparison.Ordinal))
            {
                maze = Centre(run);
            }
        }

        if (iso is null)
        {
            diagnostics.Add("No isocentre label found on the section; heights unresolved.");
            return new SectionGeometry(
                MazeHeightMm: null,
                IsoToCeilingMm: null,
                IsoToFloorMm: null,
                VaultInternalHeightMm: null,
                CeilingSlabMm: null,
                FloorSlabMm: null,
                VoidHeightMm: null,
                Provenance: Provenance.GeometryInferred,
                IsConfirmed: false,
                Diagnostics: diagnostics);
        }

        DimensionPoint isoPoint = iso.Value;
        (double? ceiling, double? floor) = FindCeilingFloor(numbers, isoPoint, diagnostics);
        double? internalHeight = (ceiling.HasValue && floor.HasValue) ? ceiling + floor : null;

        if (internalHeight.HasValue)
        {
            DimensionText? total = numbers
                .Where(d => Math.Abs(d.Value - internalHeight.Value) <= TotalCrossCheckTolerance
                    && Math.Abs(d.Position.X - isoPoint.X) < 60.0
                    && Math.Abs(d.Position.Y - isoPoint.Y) < 160.0)
                .Cast<DimensionText?>()
                .FirstOrDefault();
            diagnostics.Add(total is null
                ? "Vault internal height not corroborated by a printed total; confirm on canvas."
                : "Vault internal height corroborated by printed total " + Fmt(total.Value.Value) + " mm.");
        }

        double? mazeHeight = FindMazeHeight(numbers, maze, diagnostics);
        double? ceilingSlab = FindCeilingSlab(numbers, isoPoint, diagnostics);

        return new SectionGeometry(
            MazeHeightMm: mazeHeight,
            IsoToCeilingMm: ceiling,
            IsoToFloorMm: floor,
            VaultInternalHeightMm: internalHeight,
            CeilingSlabMm: ceilingSlab,
            FloorSlabMm: null,
            VoidHeightMm: null,
            Provenance: Provenance.FromDimensionText,
            IsConfirmed: false,
            Diagnostics: diagnostics);
    }

    private static (double? Ceiling, double? Floor) FindCeilingFloor(
        IReadOnlyList<DimensionText> numbers, DimensionPoint iso, List<string> diagnostics)
    {
        DimensionText? ceiling = null;
        DimensionText? floor = null;
        double bestGap = double.MaxValue;

        foreach (DimensionText candidate in numbers)
        {
            if (candidate.Position.X <= iso.X || Math.Abs(candidate.Position.Y - iso.Y) > IsoDimensionBand)
            {
                continue;
            }

            DimensionText? partner = numbers
                .Where(other => other.Position.X < iso.X
                    && Math.Abs(other.Position.Y - candidate.Position.Y) <= SameLineTolerance)
                .OrderBy(other => iso.X - other.Position.X)
                .Cast<DimensionText?>()
                .FirstOrDefault();

            if (partner is null)
            {
                continue;
            }

            double gap = candidate.Position.X - iso.X;
            if (gap < bestGap)
            {
                bestGap = gap;
                ceiling = candidate;
                floor = partner;
            }
        }

        if (ceiling is null || floor is null)
        {
            diagnostics.Add("ISO ceiling/floor dimension pair not found; confirm on canvas.");
            return (null, null);
        }

        diagnostics.Add("ISO to ceiling = " + Fmt(ceiling.Value.Value)
            + " mm; ISO to floor = " + Fmt(floor.Value.Value) + " mm.");
        return (ceiling.Value.Value, floor.Value.Value);
    }

    private static double? FindMazeHeight(
        IReadOnlyList<DimensionText> numbers, DimensionPoint? maze, List<string> diagnostics)
    {
        if (maze is null)
        {
            diagnostics.Add("No maze label found; maze height unresolved.");
            return null;
        }

        DimensionPoint mazePoint = maze.Value;
        DimensionText? nearest = numbers
            .Where(d => Math.Abs(d.Position.Y - mazePoint.Y) <= MazeDimensionBand)
            .OrderBy(d => Math.Abs(d.Position.X - mazePoint.X))
            .Cast<DimensionText?>()
            .FirstOrDefault();

        if (nearest is null)
        {
            diagnostics.Add("Maze label found but no nearby dimension; maze height unresolved.");
            return null;
        }

        diagnostics.Add("Maze height = " + Fmt(nearest.Value.Value) + " mm.");
        return nearest.Value.Value;
    }

    private static double? FindCeilingSlab(
        IReadOnlyList<DimensionText> numbers, DimensionPoint iso, List<string> diagnostics)
    {
        // The ceiling-primary slab thickness is not reliably distinguishable on a section
        // from story heights and other ceiling-side dimensions, so v1 does not guess it: it
        // is captured for the future (see SectionGeometry) but left for the physicist to set
        // on the verification canvas. The iso reference is retained for a future geometric
        // rule keyed on the confirmed ceiling face.
        _ = iso;
        diagnostics.Add("Ceiling slab not auto-identified from the section; set it on the canvas.");
        return null;
    }

    private static bool IsIsocentreLabel(string normalised)
    {
        foreach (string variant in IsoLabelVariants)
        {
            if (normalised.Contains(variant, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static DimensionPoint Centre(TextRun run) =>
        new DimensionPoint(
            run.BoundingBox.X + (run.BoundingBox.Width / 2.0),
            run.BoundingBox.Y + (run.BoundingBox.Height / 2.0));

    private static string Normalise(string? unicode)
    {
        if (unicode is null)
        {
            return string.Empty;
        }

        char[] kept = unicode.Where(c => !char.IsWhiteSpace(c)).ToArray();
        return new string(kept).ToUpperInvariant();
    }

    private static string Fmt(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private readonly record struct DimensionPoint(double X, double Y);

    private readonly record struct DimensionText(double Value, DimensionPoint Position);
}
