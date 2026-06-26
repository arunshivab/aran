using System;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 9. Finalises the model by recording the calibrated scale and emitting a
/// summary of what was extracted.
/// </summary>
public sealed class AssembleStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Assemble";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Model.Scale = context.Scale;
        bool hasScale = context.Scale is not null;
        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Assembled model (scaleCalibrated=" + hasScale + ").");
    }
}
