using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Aran.Extraction;
using Aran.Model;

namespace Aran.ExtractionHarness;

/// <summary>
/// Console harness that runs the extraction pipeline over a layout PDF, prints the
/// per-stage diagnostics and a model summary, and writes an SVG of the extracted
/// barrier centrelines for visual inspection of stages 1 to 4.
/// </summary>
public static class Program
{
    /// <summary>Entry point.</summary>
    /// <param name="args">The PDF path, optional page index and optional output directory.</param>
    /// <returns>Zero on success, non-zero on usage error.</returns>
    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: ExtractionHarness <layout.pdf> [pageIndex] [outputDir]");
            return 1;
        }

        string path = args[0];
        int pageIndex = args.Length > 1 ? int.Parse(args[1], CultureInfo.InvariantCulture) : 0;
        string outputDir = args.Length > 2 ? args[2] : Directory.GetCurrentDirectory();

        ShieldingExtractor extractor = new ShieldingExtractor();
        ExtractionResult result = extractor.Extract(path, pageIndex);

        Console.WriteLine("=== Diagnostics ===");
        foreach (Diagnostic diagnostic in result.Diagnostics)
        {
            Console.WriteLine("[" + diagnostic.Severity + "] " + diagnostic.Stage + ": " + diagnostic.Message);
        }

        Console.WriteLine();
        Console.WriteLine("=== Model summary ===");
        if (result.Model.Scale is null)
        {
            Console.WriteLine("Scale: (not calibrated)");
        }
        else
        {
            Console.WriteLine("Scale: " + result.Model.Scale.MillimetresPerUnit.ToString("0.######", CultureInfo.InvariantCulture)
                + " mm/unit (confidence " + result.Model.Scale.Confidence.ToString("0.00", CultureInfo.InvariantCulture) + ")");
        }

        Console.WriteLine("Rooms:    " + result.Model.Rooms.Count);
        Console.WriteLine("Barriers: " + result.Model.Barriers.Count);
        Console.WriteLine("Sources:  " + result.Model.Sources.Count);

        int shown = 0;
        foreach (Barrier barrier in result.Model.Barriers)
        {
            if (shown >= 8)
            {
                break;
            }

            Console.WriteLine("  " + barrier.Id + ": thickness "
                + barrier.ThicknessMm.ToString("0", CultureInfo.InvariantCulture) + " mm");
            shown++;
        }

        string svgPath = Path.Combine(outputDir, "barriers.svg");
        WriteBarrierSvg(result.Model, svgPath);
        Console.WriteLine();
        Console.WriteLine("Wrote " + svgPath);
        return 0;
    }

    private static void WriteBarrierSvg(ShieldingGeometryModel model, string svgPath)
    {
        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;
        foreach (Barrier barrier in model.Barriers)
        {
            foreach (PointMm point in barrier.CentrelineMm.Points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        if (model.Barriers.Count == 0)
        {
            minX = 0.0;
            minY = 0.0;
            maxX = 100.0;
            maxY = 100.0;
        }

        double pad = 500.0;
        double width = (maxX - minX) + (2.0 * pad);
        double height = (maxY - minY) + (2.0 * pad);
        StringBuilder svg = new StringBuilder();
        svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 ");
        svg.Append(width.ToString("0.###", CultureInfo.InvariantCulture));
        svg.Append(' ');
        svg.Append(height.ToString("0.###", CultureInfo.InvariantCulture));
        svg.Append("\">\n");
        svg.Append("<rect x=\"0\" y=\"0\" width=\"100%\" height=\"100%\" fill=\"white\"/>\n");

        foreach (Barrier barrier in model.Barriers)
        {
            IReadOnlyList<PointMm> points = barrier.CentrelineMm.Points;
            if (points.Count < 2)
            {
                continue;
            }

            double x1 = (points[0].X - minX) + pad;
            double y1 = (maxY - points[0].Y) + pad;
            double x2 = (points[1].X - minX) + pad;
            double y2 = (maxY - points[1].Y) + pad;
            double strokeWidth = Math.Max(barrier.ThicknessMm, 20.0);
            svg.Append("<line x1=\"");
            svg.Append(x1.ToString("0.###", CultureInfo.InvariantCulture));
            svg.Append("\" y1=\"");
            svg.Append(y1.ToString("0.###", CultureInfo.InvariantCulture));
            svg.Append("\" x2=\"");
            svg.Append(x2.ToString("0.###", CultureInfo.InvariantCulture));
            svg.Append("\" y2=\"");
            svg.Append(y2.ToString("0.###", CultureInfo.InvariantCulture));
            svg.Append("\" stroke=\"#1f4e79\" stroke-opacity=\"0.5\" stroke-width=\"");
            svg.Append(strokeWidth.ToString("0.###", CultureInfo.InvariantCulture));
            svg.Append("\"/>\n");
        }

        svg.Append("</svg>\n");
        File.WriteAllText(svgPath, svg.ToString(), new UTF8Encoding(false));
    }
}
