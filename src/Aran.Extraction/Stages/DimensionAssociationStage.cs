using System;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 7. Binds parsed dimension values to specific barriers and room spans so the
/// verification canvas can show measured lengths against geometry. Not yet
/// implemented in the skeleton.
/// </summary>
/// <remarks>
/// TODO: reuse the dimension-to-segment association from calibration to attach
/// real-world spans to the barriers and rooms they annotate.
/// </remarks>
public sealed class DimensionAssociationStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Dimension association";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Report(DiagnosticSeverity.Info, Name, "Not yet implemented (v1 skeleton).");
    }
}
