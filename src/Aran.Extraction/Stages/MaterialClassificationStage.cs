using System;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 6. Assigns a material and density to each barrier, first from the source
/// wall layer name and then by matching hatch texture against the legend. Not yet
/// implemented in the skeleton.
/// </summary>
/// <remarks>
/// TODO: map specific wall layers (for example concrete versus brick) to materials,
/// then for ambiguous regions rasterise the barrier band and correlate the hatch
/// pattern against legend swatches to recover density.
/// </remarks>
public sealed class MaterialClassificationStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Material classification";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Report(DiagnosticSeverity.Info, Name, "Not yet implemented (v1 skeleton).");
    }
}
