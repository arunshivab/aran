using System;
using Chuvadi.Pdf.Rendering.DisplayList;

namespace Aran.Extraction.Stages;

/// <summary>
/// Stage 8. Detects radiation sources from isocentre labels and dashed beam-axis
/// lines. The skeleton reports how many isocentre labels are present but does not yet
/// emit sources.
/// </summary>
/// <remarks>
/// TODO: cluster dashed axis segments through each isocentre label, infer the machine
/// class from nearby text, and emit a source with its beam axes.
/// </remarks>
public sealed class SourceDetectionStage : IExtractionStage
{
    /// <inheritdoc />
    public string Name => "Source detection";

    /// <inheritdoc />
    public void Execute(ExtractionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        int isocentreLabels = 0;
        foreach (TextRun text in context.Texts)
        {
            if (text.Unicode is not null && text.Unicode.Contains("ISO", StringComparison.OrdinalIgnoreCase))
            {
                isocentreLabels++;
            }
        }

        context.Report(
            DiagnosticSeverity.Info,
            Name,
            "Not yet implemented (v1 skeleton); found " + isocentreLabels + " isocentre label(s).");
    }
}
