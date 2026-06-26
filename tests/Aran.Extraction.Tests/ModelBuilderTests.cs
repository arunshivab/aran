using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Extraction.Tests;

/// <summary>Tests for the model accumulator.</summary>
public sealed class ModelBuilderTests
{
    [Fact]
    public void NextId_increments_with_prefix()
    {
        ModelBuilder builder = new ModelBuilder();
        builder.NextId("B").Should().Be("B1");
        builder.NextId("B").Should().Be("B2");
        builder.NextId("R").Should().Be("R3");
    }

    [Fact]
    public void Build_carries_scale_and_added_elements()
    {
        ModelBuilder builder = new ModelBuilder();
        builder.Scale = new ScaleCalibration(2.5, 1.0, true, Provenance.FromDimensionText);
        Polyline line = new Polyline(new PointMm[] { new PointMm(0.0, 0.0), new PointMm(10.0, 0.0) });
        builder.AddBarrier(new Barrier("B1", line, 300.0, BarrierMaterial.Concrete, 2.35, null, null, Provenance.FromLayer, false));

        ShieldingGeometryModel model = builder.Build();

        model.Scale.Should().NotBeNull();
        model.Scale!.MillimetresPerUnit.Should().Be(2.5);
        model.Barriers.Should().ContainSingle();
        model.Barriers[0].ThicknessMm.Should().Be(300.0);
        model.Rooms.Should().BeEmpty();
        model.Sources.Should().BeEmpty();
    }
}
