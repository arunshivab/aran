using Aran.Machines;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Engines.Linac.Tests;

public sealed class StandardsTests
{
    [Fact]
    public void Ncrp_and_Aerb_design_goals_differ_for_controlled_areas()
    {
        Standards.Ncrp151.DesignGoalSvPerWeek(AreaClass.Controlled).Value.Should().Be(1.0e-4);
        Standards.Aerb.DesignGoalSvPerWeek(AreaClass.Controlled).Value.Should().Be(4.0e-4);
    }

    [Fact]
    public void Uncontrolled_design_goals_agree()
    {
        Standards.Ncrp151.DesignGoalSvPerWeek(AreaClass.Uncontrolled).Value.Should().Be(2.0e-5);
        Standards.Aerb.DesignGoalSvPerWeek(AreaClass.Uncontrolled).Value.Should().Be(2.0e-5);
    }

    [Fact]
    public void Aerb_occupancy_is_flat_one_while_ncrp_is_graded()
    {
        Standards.Aerb.Occupancy(OccupancyCategory.Corridor).Value.Should().Be(1.0);
        Standards.Ncrp151.Occupancy(OccupancyCategory.Corridor).Value.Should().Be(1.0 / 5.0);
    }

    [Fact]
    public void Aerb_fixes_primary_use_factor_by_machine_type()
    {
        Standards.Aerb.UseFactor(MachineCatalog.TrueBeam, BarrierRole.Primary, 0.9).Value.Should().Be(0.25);
        Standards.Aerb.UseFactor(MachineCatalog.Halcyon, BarrierRole.Primary, 0.9).Value.Should().Be(0.12);
    }
}
