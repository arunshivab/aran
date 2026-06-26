using System;
using System.Collections.Generic;
using Chuvadi.Pdf.Documents;

namespace Aran.Extraction;

/// <summary>
/// The top-level facade for extracting a draft geometry model from a layout PDF.
/// Opens the document, runs the default pipeline and returns the draft model.
/// </summary>
public sealed class ShieldingExtractor
{
    private readonly ExtractionPipeline _pipeline;
    private readonly LayerMap _layerMap;

    /// <summary>Initialises a new extractor with the default pipeline and layer profile.</summary>
    public ShieldingExtractor()
        : this(ExtractionPipeline.CreateDefault(), LayerMap.CreateDefault())
    {
    }

    /// <summary>Initialises a new extractor with explicit pipeline and layer profile.</summary>
    /// <param name="pipeline">The pipeline to run.</param>
    /// <param name="layerMap">The layer profile to apply.</param>
    public ShieldingExtractor(ExtractionPipeline pipeline, LayerMap layerMap)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _layerMap = layerMap ?? throw new ArgumentNullException(nameof(layerMap));
    }

    /// <summary>Extracts a draft model from a file on disk.</summary>
    /// <param name="filePath">The path to the layout PDF.</param>
    /// <param name="pageIndex">The zero-based page index to extract.</param>
    /// <returns>The extraction result.</returns>
    public ExtractionResult Extract(string filePath, int pageIndex)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using PdfDocument document = PdfDocument.Open(filePath);
        return Extract(document, pageIndex);
    }

    /// <summary>Extracts a draft model from an already-open document.</summary>
    /// <param name="document">The open source document.</param>
    /// <param name="pageIndex">The zero-based page index to extract.</param>
    /// <returns>The extraction result.</returns>
    public ExtractionResult Extract(PdfDocument document, int pageIndex)
    {
        ArgumentNullException.ThrowIfNull(document);
        ExtractionContext context = new ExtractionContext(document, pageIndex, _layerMap);
        _pipeline.Run(context);
        return new ExtractionResult(context.Model.Build(), new List<Diagnostic>(context.Diagnostics));
    }
}
