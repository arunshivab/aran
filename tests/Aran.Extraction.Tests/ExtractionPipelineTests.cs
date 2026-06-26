using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Aran.Extraction.Tests;

/// <summary>Tests for the default pipeline composition and failure isolation.</summary>
public sealed class ExtractionPipelineTests
{
    [Fact]
    public void CreateDefault_has_nine_stages_in_order()
    {
        ExtractionPipeline pipeline = ExtractionPipeline.CreateDefault();
        List<string> names = new List<string>();
        foreach (IExtractionStage stage in pipeline.Stages)
        {
            names.Add(stage.Name);
        }

        names.Should().Equal(
            "Load",
            "Layer classification",
            "Scale calibration",
            "Wall reconstruction",
            "Room detection",
            "Material classification",
            "Dimension association",
            "Source detection",
            "Assemble");
    }
}
