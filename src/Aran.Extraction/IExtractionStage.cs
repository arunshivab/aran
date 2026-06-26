namespace Aran.Extraction;

/// <summary>A single ordered step in the extraction pipeline.</summary>
public interface IExtractionStage
{
    /// <summary>The stable name of the stage, used in diagnostics.</summary>
    string Name { get; }

    /// <summary>Executes the stage against the shared context.</summary>
    /// <param name="context">The extraction context.</param>
    void Execute(ExtractionContext context);
}
