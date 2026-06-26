using System;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 5. Recovers rooms by closing wall centrelines into polygons and assigning
/// labels by point-in-polygon. Not yet implemented in the skeleton.
/// </summary>
/// <remarks>
/// TODO: build a planar graph from confirmed wall centrelines, find minimal closed
/// faces, drop the outer face, and attach the nearest enclosed label text to each
/// face. Until then the verification canvas relies on barriers and labels alone.
/// </remarks>
public sealed class RoomDetectionStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Room detection";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Report(DiagnosticSeverity.Info, Name, "Not yet implemented (v1 skeleton).");
    }
}
