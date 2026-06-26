using Aran.Machines;
using FluentAssertions;
using Xunit;

namespace Aran.Engines.Linac.Tests;

public sealed class MachineCatalogTests
{
    [Fact]
    public void TrueBeam_has_five_modes_and_no_beam_stopper()
    {
        MachineCatalog.TrueBeam.Modes.Should().HaveCount(5);
        MachineCatalog.TrueBeam.BeamStopperTransmission.Should().BeNull();
    }

    [Fact]
    public void Halcyon_has_single_fff_mode_and_beam_stopper()
    {
        MachineCatalog.Halcyon.Modes.Should().ContainSingle();
        MachineCatalog.Halcyon.Modes[0].Fff.Should().BeTrue();
        MachineCatalog.Halcyon.BeamStopperTransmission.Should().Be(0.001);
    }

    [Fact]
    public void FindByName_is_case_insensitive()
    {
        MachineCatalog.FindByName("truebeam").Should().BeSameAs(MachineCatalog.TrueBeam);
    }
}
