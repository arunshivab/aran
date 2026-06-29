using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Aran.Extraction.Tests;

/// <summary>Tests for WallGraph: endpoint snapping, loop building, area calculation.</summary>
public sealed class WallGraphTests
{
    [Fact]
    public void Square_four_segments_forms_one_closed_loop()
    {
        // Unit square with exact endpoints
        List<(double, double, double, double)> segs = new List<(double, double, double, double)>
        {
            (0, 0, 10, 0),
            (10, 0, 10, 10),
            (10, 10, 0, 10),
            (0, 10, 0, 0),
        };

        IReadOnlyList<WallLoop> loops = WallGraph.BuildLoops(segs);
        loops.Should().ContainSingle(l => l.IsClosed, "four segments forming a square should close into one loop");
    }

    [Fact]
    public void Square_with_snappable_gap_still_closes()
    {
        // Square with a small endpoint gap within SnapTolerance
        double gap = WallGraph.SnapTolerance * 0.5;
        List<(double, double, double, double)> segs = new List<(double, double, double, double)>
        {
            (0, 0, 10, 0),
            (10 + gap, 0, 10, 10),
            (10, 10, 0, 10),
            (0, 10, 0, 0),
        };

        IReadOnlyList<WallLoop> loops = WallGraph.BuildLoops(segs);
        bool hasClosedLoop = false;
        foreach (WallLoop l in loops)
        {
            if (l.IsClosed) { hasClosedLoop = true; }
        }

        hasClosedLoop.Should().BeTrue("gap within snap tolerance should be bridged");
    }

    [Fact]
    public void Signed_area_of_unit_square_is_positive_100()
    {
        // CCW square (positive area)
        List<RawPoint> pts = new List<RawPoint>
        {
            new RawPoint(0, 0),
            new RawPoint(10, 0),
            new RawPoint(10, 10),
            new RawPoint(0, 10),
        };
        WallGraph.SignedArea(pts).Should().BeApproximately(100.0, 0.001);
    }

    [Fact]
    public void Very_short_segment_is_dropped()
    {
        List<(double, double, double, double)> segs = new List<(double, double, double, double)>
        {
            (0, 0, 0.1, 0),  // below MinSegmentLength
            (0, 0, 10, 0),
            (10, 0, 10, 10),
            (10, 10, 0, 10),
            (0, 10, 0, 0),
        };

        IReadOnlyList<WallLoop> loops = WallGraph.BuildLoops(segs);
        bool hasClosedLoop = false;
        foreach (WallLoop l in loops)
        {
            if (l.IsClosed) { hasClosedLoop = true; }
        }

        hasClosedLoop.Should().BeTrue("short segment should be ignored, square should still close");
    }

    [Fact]
    public void LayerMap_with_layer0_classifies_zero_as_wall()
    {
        LayerMap map = LayerMap.CreateWithLayer0AsWall();
        map.Classify("0").Should().Be(LayerRole.Wall);
    }

    [Fact]
    public void LayerMap_default_does_not_classify_zero_as_wall()
    {
        LayerMap map = LayerMap.CreateDefault();
        map.Classify("0").Should().NotBe(LayerRole.Wall);
    }

    [Fact]
    public void LayerMap_with_layer0_still_classifies_named_wall_layers()
    {
        LayerMap map = LayerMap.CreateWithLayer0AsWall();
        map.Classify("AR-Wall").Should().Be(LayerRole.Wall);
        map.Classify("RCC wall").Should().Be(LayerRole.Wall);
    }

}
