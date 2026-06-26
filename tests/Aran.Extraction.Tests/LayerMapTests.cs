using FluentAssertions;
using Xunit;

namespace Aran.Extraction.Tests;

/// <summary>Tests for the default layer profile classification.</summary>
public sealed class LayerMapTests
{
    [Theory]
    [InlineData("PDF9_rcc wall", LayerRole.Wall)]
    [InlineData("PDF9_AR-Wall", LayerRole.Wall)]
    [InlineData("PDF9_door", LayerRole.Door)]
    [InlineData("A-ANNO-DIM", LayerRole.Dimension)]
    [InlineData("PDF9_dim-1", LayerRole.Dimension)]
    [InlineData("PDF9_A-ANNO-MARK", LayerRole.Annotation)]
    [InlineData("PDF9_TEXT", LayerRole.Label)]
    [InlineData("TEXT", LayerRole.Label)]
    [InlineData("some-unrelated-layer", LayerRole.Unknown)]
    public void Classify_maps_known_layers(string layer, LayerRole expected)
    {
        LayerMap map = LayerMap.CreateDefault();
        map.Classify(layer).Should().Be(expected);
    }

    [Fact]
    public void RoleOf_prefers_wall_over_annotation()
    {
        LayerMap map = LayerMap.CreateDefault();
        string[] layers = new string[] { "PDF9_A-ANNO-MARK", "PDF9_rcc wall" };
        map.RoleOf(layers).Should().Be(LayerRole.Wall);
    }

    [Fact]
    public void RoleOf_returns_unknown_when_no_layer_matches()
    {
        LayerMap map = LayerMap.CreateDefault();
        string[] layers = new string[] { "foo", "bar" };
        map.RoleOf(layers).Should().Be(LayerRole.Unknown);
    }
}
