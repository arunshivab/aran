using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Model;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 3. Derives the drawing scale (millimetres per raw drawing unit) by
/// associating each printed dimension number with the nearest dimension line in
/// page space and taking the consensus of value-over-raw-length ratios. The scale
/// is only accepted when at least two independent dimensions agree; otherwise the
/// stage flags the page for manual calibration rather than guessing.
/// </summary>
public sealed class ScaleCalibrationStage : IExtractionStage
{
    private const double RelativeClusterTolerance = 0.02;
    private const double MinRawLength = 1.0;

    /// <inheritdoc />
    public string Name => "Scale calibration";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        LayerClassification? classification = context.Classification;
        if (classification is null)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No classification available; cannot calibrate.");
            return;
        }

        if (classification.DimensionSegments.Count == 0)
        {
            context.Report(DiagnosticSeverity.Warning, Name, "No dimension segments found; manual calibration required.");
            return;
        }

        List<double> ratios = new List<double>();
        foreach (TextRun text in classification.DimensionTexts)
        {
            if (!TryParseMillimetres(text.Unicode, out double valueMm))
            {
                continue;
            }

            LineSegment? nearest = FindNearestSegment(text, classification.DimensionSegments);
            if (nearest is null)
            {
                continue;
            }

            double rawLength = RawLength(nearest.Value);
            if (rawLength < MinRawLength)
            {
                continue;
            }

            ratios.Add(valueMm / rawLength);
        }

        if (ratios.Count < 2)
        {
            context.Report(
                DiagnosticSeverity.Warning,
                Name,
                "Only " + ratios.Count + " dimension sample(s); need two agreeing dimensions. Manual calibration required.");
            return;
        }

        if (!TryFindConsensus(ratios, out double millimetresPerUnit, out int clusterSize))
        {
            context.Report(
                DiagnosticSeverity.Warning,
                Name,
                "Dimension ratios did not agree within tolerance; manual calibration required.");
            return;
        }

        double confidence = (double)clusterSize / ratios.Count;
        ScaleCalibration scale = new ScaleCalibration(millimetresPerUnit, confidence, true, Provenance.FromDimensionText);
        context.Scale = scale;
        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Scale = " + millimetresPerUnit.ToString("0.######", CultureInfo.InvariantCulture)
                + " mm/unit from " + clusterSize + "/" + ratios.Count + " agreeing dimensions.");
    }

    private static bool TryParseMillimetres(string unicode, out double valueMm)
    {
        valueMm = 0.0;
        if (string.IsNullOrWhiteSpace(unicode))
        {
            return false;
        }

        string trimmed = unicode.Trim();
        if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
        {
            return false;
        }

        if (parsed <= 0.0)
        {
            return false;
        }

        valueMm = parsed;
        return true;
    }

    private static LineSegment? FindNearestSegment(TextRun text, IReadOnlyList<LineSegment> segments)
    {
        double centreX = text.BoundingBox.X + (text.BoundingBox.Width / 2.0);
        double centreY = text.BoundingBox.Y + (text.BoundingBox.Height / 2.0);
        double bestDistance = double.MaxValue;
        LineSegment? best = null;
        foreach (LineSegment segment in segments)
        {
            double midX = (segment.X0 + segment.X1) / 2.0;
            double midY = (segment.Y0 + segment.Y1) / 2.0;
            double dx = midX - centreX;
            double dy = midY - centreY;
            double distance = (dx * dx) + (dy * dy);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = segment;
            }
        }

        return best;
    }

    private static double RawLength(LineSegment segment)
    {
        double dx = segment.RawX1 - segment.RawX0;
        double dy = segment.RawY1 - segment.RawY0;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static bool TryFindConsensus(IReadOnlyList<double> ratios, out double consensus, out int clusterSize)
    {
        consensus = 0.0;
        clusterSize = 0;
        for (int i = 0; i < ratios.Count; i++)
        {
            double pivot = ratios[i];
            List<double> cluster = new List<double>();
            foreach (double candidate in ratios)
            {
                double relative = Math.Abs(candidate - pivot) / pivot;
                if (relative <= RelativeClusterTolerance)
                {
                    cluster.Add(candidate);
                }
            }

            if (cluster.Count > clusterSize)
            {
                clusterSize = cluster.Count;
                consensus = Median(cluster);
            }
        }

        return clusterSize >= 2;
    }

    private static double Median(List<double> values)
    {
        values.Sort();
        int mid = values.Count / 2;
        if ((values.Count % 2) == 0)
        {
            return (values[mid - 1] + values[mid]) / 2.0;
        }

        return values[mid];
    }
}
