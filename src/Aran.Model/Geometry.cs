using System.Collections.Generic;

namespace Aran.Model;

/// <summary>A point in real-world drawing space, expressed in millimetres.</summary>
/// <param name="X">The horizontal coordinate in millimetres.</param>
/// <param name="Y">The vertical coordinate in millimetres.</param>
public readonly record struct PointMm(double X, double Y);

/// <summary>A straight line segment between two points in millimetre space.</summary>
/// <param name="A">The first endpoint.</param>
/// <param name="B">The second endpoint.</param>
public readonly record struct LineMm(PointMm A, PointMm B);

/// <summary>An open sequence of connected points in millimetre space.</summary>
/// <param name="Points">The ordered vertices of the polyline.</param>
public sealed record Polyline(IReadOnlyList<PointMm> Points);

/// <summary>A closed boundary in millimetre space.</summary>
/// <param name="Vertices">The ordered vertices of the polygon boundary.</param>
public sealed record Polygon(IReadOnlyList<PointMm> Vertices);
