using System;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 1. Builds the page display list and extracts the line segments and text
/// runs that every later stage consumes.
/// </summary>
public sealed class LoadStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Load";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        PageDisplayList displayList = PdfPageExtensions.BuildDisplayList(context.Document, context.PageIndex);
        context.DisplayList = displayList;
        context.Texts = PdfPageExtensions.GetTextRuns(context.Document, context.PageIndex);
        context.Segments = LineSegmentExtraction.ExtractLineSegments(displayList, LineSegmentExtraction.DefaultFlattenTolerance);
        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Loaded " + context.Segments.Count + " segments, " + context.Texts.Count + " text runs, " + displayList.Count + " ops.");
    }
}
