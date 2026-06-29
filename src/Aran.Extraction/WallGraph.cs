using System;
using System.Collections.Generic;

namespace Aran.Extraction;

/// <summary>
/// A 2-D point in raw drawing units (before scale application).
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
public readonly record struct RawPoint(double X, double Y);

/// <summary>
/// A closed or open polyline of wall outline points in raw drawing units.
/// </summary>
/// <param name="Points">The ordered vertices.</param>
/// <param name="IsClosed">Whether the last point connects back to the first.</param>
public sealed record WallLoop(IReadOnlyList<RawPoint> Points, bool IsClosed);

/// <summary>
/// Builds a planar wall graph from raw line segments by snapping nearby endpoints,
/// chaining collinear segments, and closing loops. This is the core of stage 5.
/// </summary>
public static class WallGraph
{
    /// <summary>
    /// Endpoint snapping tolerance in raw drawing units. Two endpoints closer than
    /// this are treated as the same node. At 5.47 mm/unit this is approximately 10 mm.
    /// </summary>
    public const double SnapTolerance = 2.0;

    /// <summary>
    /// Minimum segment length to include (raw units). Segments shorter than this are
    /// gap-filling artefacts and are dropped.
    /// </summary>
    public const double MinSegmentLength = 0.5;

    /// <summary>
    /// Builds wall loops from a set of raw segment endpoints.
    /// Returns closed loops (rooms) and any unclosed chains (partial walls).
    /// </summary>
    /// <param name="segments">The input segments as (x0,y0,x1,y1) tuples in raw units.</param>
    /// <returns>The extracted loops.</returns>
    public static IReadOnlyList<WallLoop> BuildLoops(
        IReadOnlyList<(double X0, double Y0, double X1, double Y1)> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        // Step 1: snap endpoints into a node dictionary
        List<RawPoint> nodes = new List<RawPoint>();
        Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();

        int NodeFor(double x, double y)
        {
            for (int k = 0; k < nodes.Count; k++)
            {
                double dx = nodes[k].X - x;
                double dy = nodes[k].Y - y;
                if ((dx * dx) + (dy * dy) <= SnapTolerance * SnapTolerance)
                {
                    return k;
                }
            }

            nodes.Add(new RawPoint(x, y));
            return nodes.Count - 1;
        }

        // Step 2: build adjacency
        foreach ((double x0, double y0, double x1, double y1) in segments)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            double len = Math.Sqrt((dx * dx) + (dy * dy));
            if (len < MinSegmentLength)
            {
                continue;
            }

            int a = NodeFor(x0, y0);
            int b = NodeFor(x1, y1);
            if (a == b)
            {
                continue;
            }

            if (!adjacency.TryGetValue(a, out List<int>? listA))
            {
                listA = new List<int>();
                adjacency[a] = listA;
            }

            if (!adjacency.TryGetValue(b, out List<int>? listB))
            {
                listB = new List<int>();
                adjacency[b] = listB;
            }

            if (!listA.Contains(b))
            {
                listA.Add(b);
            }

            if (!listB.Contains(a))
            {
                listB.Add(a);
            }
        }

        // Step 3: trace chains using DFS, preferring nodes with exactly 2 connections
        bool[] visited = new bool[nodes.Count];
        List<WallLoop> loops = new List<WallLoop>();

        for (int start = 0; start < nodes.Count; start++)
        {
            if (visited[start])
            {
                continue;
            }

            if (!adjacency.TryGetValue(start, out List<int>? startAdj) || startAdj.Count == 0)
            {
                continue;
            }

            // Walk the chain from this node
            List<int> chain = new List<int> { start };
            visited[start] = true;
            int current = start;
            int previous = -1;

            while (true)
            {
                if (!adjacency.TryGetValue(current, out List<int>? adj))
                {
                    break;
                }

                // Pick next unvisited neighbour (or the start node to close the loop)
                int next = -1;
                bool closedToStart = false;
                foreach (int neighbour in adj)
                {
                    if (neighbour == start && chain.Count >= 3)
                    {
                        closedToStart = true;
                        break;
                    }

                    if (!visited[neighbour] && neighbour != previous)
                    {
                        next = neighbour;
                        break;
                    }
                }

                if (closedToStart)
                {
                    List<RawPoint> pts = new List<RawPoint>(chain.Count);
                    foreach (int idx in chain)
                    {
                        pts.Add(nodes[idx]);
                    }

                    loops.Add(new WallLoop(pts, true));
                    break;
                }

                if (next == -1)
                {
                    if (chain.Count >= 2)
                    {
                        List<RawPoint> pts = new List<RawPoint>(chain.Count);
                        foreach (int idx in chain)
                        {
                            pts.Add(nodes[idx]);
                        }

                        loops.Add(new WallLoop(pts, false));
                    }

                    break;
                }

                previous = current;
                current = next;
                visited[next] = true;
                chain.Add(next);
            }
        }

        return loops;
    }

    /// <summary>
    /// Computes the signed area of a polygon (shoelace formula).
    /// Positive = counter-clockwise, negative = clockwise.
    /// </summary>
    public static double SignedArea(IReadOnlyList<RawPoint> pts)
    {
        ArgumentNullException.ThrowIfNull(pts);
        int n = pts.Count;
        if (n < 3)
        {
            return 0.0;
        }

        double area = 0.0;
        for (int i = 0; i < n; i++)
        {
            RawPoint a = pts[i];
            RawPoint b = pts[(i + 1) % n];
            area += (a.X * b.Y) - (b.X * a.Y);
        }

        return area / 2.0;
    }
}
