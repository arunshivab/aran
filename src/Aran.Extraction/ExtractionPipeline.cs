using System;
using System.Collections.Generic;
using Aran.Extraction.Stages;

namespace Aran.Extraction;

/// <summary>An ordered set of extraction stages executed against a context.</summary>
public sealed class ExtractionPipeline
{
    private readonly IReadOnlyList<IExtractionStage> _stages;

    /// <summary>Initialises a new pipeline from an ordered stage list.</summary>
    /// <param name="stages">The stages to execute, in order.</param>
    public ExtractionPipeline(IReadOnlyList<IExtractionStage> stages)
    {
        _stages = stages ?? throw new ArgumentNullException(nameof(stages));
    }

    /// <summary>The stages in execution order.</summary>
    public IReadOnlyList<IExtractionStage> Stages => _stages;

    /// <summary>Creates the default nine-stage extraction pipeline.</summary>
    /// <returns>A configured <see cref="ExtractionPipeline"/>.</returns>
    public static ExtractionPipeline CreateDefault()
    {
        IExtractionStage[] stages = new IExtractionStage[]
        {
            new LoadStage(),
            new LayerClassificationStage(),
            new ScaleCalibrationStage(),
            new WallReconstructionStage(),
            new RoomDetectionStage(),
            new MaterialClassificationStage(),
            new DimensionAssociationStage(),
            new SourceDetectionStage(),
            new AssembleStage(),
        };
        return new ExtractionPipeline(stages);
    }

    /// <summary>Runs every stage in order, isolating per-stage failures as diagnostics.</summary>
    /// <param name="context">The extraction context.</param>
    public void Run(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        foreach (IExtractionStage stage in _stages)
        {
            try
            {
                stage.Execute(context);
            }
            catch (Exception ex)
            {
                context.Report(DiagnosticSeverity.Error, stage.Name, "Stage failed: " + ex.Message);
            }
        }
    }
}
