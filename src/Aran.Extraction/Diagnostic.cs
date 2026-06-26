namespace Aran.Extraction;

/// <summary>The severity of an extraction diagnostic.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Informational message about what was extracted.</summary>
    Info,

    /// <summary>A condition the user should review, such as a failed calibration.</summary>
    Warning,

    /// <summary>A condition that prevented a stage from producing output.</summary>
    Error,
}

/// <summary>A message produced by an extraction stage for review and audit.</summary>
/// <param name="Severity">The severity of the diagnostic.</param>
/// <param name="Stage">The name of the stage that produced the diagnostic.</param>
/// <param name="Message">The human-readable diagnostic text.</param>
public sealed record Diagnostic(DiagnosticSeverity Severity, string Stage, string Message);
