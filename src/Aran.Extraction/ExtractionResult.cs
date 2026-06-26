using System.Collections.Generic;
using Aran.Model;

namespace Aran.Extraction;

/// <summary>The output of an extraction run: the draft model and its diagnostics.</summary>
/// <param name="Model">The assembled draft geometry model.</param>
/// <param name="Diagnostics">The diagnostics produced during extraction.</param>
public sealed record ExtractionResult(ShieldingGeometryModel Model, IReadOnlyList<Diagnostic> Diagnostics);
