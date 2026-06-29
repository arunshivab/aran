using System;
using System.Collections.Generic;
using Aran.Model;
using Chuvadi.Pdf.Documents;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction;

/// <summary>
/// The mutable state shared across extraction stages for a single page. Early
/// stages populate the loaded artifacts and classification; later stages read them
/// and accumulate elements into <see cref="Model"/>.
/// </summary>
public sealed class ExtractionContext
{
    /// <summary>Initialises a new context for the given document and page.</summary>
    /// <param name="document">The open source document.</param>
    /// <param name="pageIndex">The zero-based page index to extract.</param>
    /// <param name="layerMap">The layer profile used for classification.</param>
    public ExtractionContext(PdfDocument document, int pageIndex, LayerMap layerMap)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        LayerMap = layerMap ?? throw new ArgumentNullException(nameof(layerMap));
        PageIndex = pageIndex;
        Segments = Array.Empty<LineSegment>();
        Texts = Array.Empty<TextRun>();
        Model = new ModelBuilder();
        Diagnostics = new List<Diagnostic>();
        WallLoops = Array.Empty<WallLoop>();
        RoomPolygons = Array.Empty<RoomPolygon>();
    }

    /// <summary>The open source document.</summary>
    public PdfDocument Document { get; }

    /// <summary>The zero-based page index being extracted.</summary>
    public int PageIndex { get; }

    /// <summary>The layer profile used for classification.</summary>
    public LayerMap LayerMap { get; }

    /// <summary>The page display list, populated by the load stage.</summary>
    public PageDisplayList? DisplayList { get; set; }

    /// <summary>The page line segments, populated by the load stage.</summary>
    public IReadOnlyList<LineSegment> Segments { get; set; }

    /// <summary>The page text runs, populated by the load stage.</summary>
    public IReadOnlyList<TextRun> Texts { get; set; }

    /// <summary>The layer classification, populated by the classification stage.</summary>
    public LayerClassification? Classification { get; set; }

    /// <summary>The scale calibration, populated by the calibration stage.</summary>
    public ScaleCalibration? Scale { get; set; }

    /// <summary>
    /// Closed and open wall loops produced by stage 5 (wall body recovery).
    /// Empty until stage 5 completes.
    /// </summary>
    public IReadOnlyList<WallLoop> WallLoops { get; set; }

    /// <summary>
    /// Room polygons with label and function assignments, produced by stages 6 and 7.
    /// Empty until stage 7 completes.
    /// </summary>
    public IReadOnlyList<RoomPolygon> RoomPolygons { get; set; }

    /// <summary>
    /// The extracted maze geometry produced by stage 8, or null when no maze
    /// was identified. Converted to MazeRun by the orchestration layer.
    /// </summary>
    public ExtractedMazeGeometry? MazeGeometry { get; set; }

    /// <summary>The accumulating model.</summary>
    public ModelBuilder Model { get; }

    /// <summary>The diagnostics accumulated across stages.</summary>
    public IList<Diagnostic> Diagnostics { get; }

    /// <summary>Adds a diagnostic to the context.</summary>
    /// <param name="severity">The severity.</param>
    /// <param name="stage">The producing stage name.</param>
    /// <param name="message">The diagnostic text.</param>
    public void Report(DiagnosticSeverity severity, string stage, string message)
    {
        ArgumentNullException.ThrowIfNull(stage);
        ArgumentNullException.ThrowIfNull(message);
        Diagnostics.Add(new Diagnostic(severity, stage, message));
    }
}
