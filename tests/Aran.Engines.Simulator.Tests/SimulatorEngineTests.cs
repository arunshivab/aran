using System;
using System.Collections.Generic;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Engines.Simulator.Tests;

public sealed class SimulatorEngineTests
{
    private static SimulatorShieldingInput BuildNcrpInput()
    {
        List<SimulatorBarrierInput> barriers = new List<SimulatorBarrierInput>
        {
            new SimulatorBarrierInput("W1", BarrierMaterial.Concrete, 200.0, 3.0,
                AreaClass.Controlled, 0.25, 1.0, false),
        };
        // 100 kVp, 500 mAmin/wk (AERB typical simulator), output 8.46e-4 R/mAmin
        return new SimulatorShieldingInput(100, 500.0, 8.46e-4, SimulatorStandard.Ncrp49, barriers);
    }

    private static SimulatorShieldingInput BuildAerbInput()
    {
        List<SimulatorBarrierInput> barriers = new List<SimulatorBarrierInput>
        {
            new SimulatorBarrierInput("W1", BarrierMaterial.Concrete, 200.0, 3.0,
                AreaClass.Controlled, 0.25, 1.0, false),
        };
        return new SimulatorShieldingInput(100, 500.0, 8.46e-4, SimulatorStandard.AerbSimplified, barriers);
    }

    [Fact]
    public void Ncrp_compliant_when_dose_below_design_goal()
    {
        // At 500 mAmin/wk, 100 kVp, d=3 m, U=0.25, T=1, dose < NCRP 100 mR/wk limit
        // so B > 1, n < 0, required = 0 (no barrier needed at this distance)
        SimulatorShieldingEngine engine = new SimulatorShieldingEngine();
        SimulatorShieldingResult result = engine.Evaluate(BuildNcrpInput());
        result.Barriers[0].Passes.Should().BeTrue();
        result.Barriers[0].RequiredThicknessMm.Should().Be(0.0);
    }

    [Fact]
    public void Aerb_simplified_returns_statement_and_no_steps()
    {
        SimulatorShieldingEngine engine = new SimulatorShieldingEngine();
        SimulatorShieldingResult result = engine.Evaluate(BuildAerbInput());
        SimulatorBarrierResult barrier = result.Barriers[0];
        barrier.AerbSimplifiedStatement.Should().NotBeNullOrWhiteSpace();
        barrier.Steps.Should().BeEmpty();
    }

    [Fact]
    public void Aerb_simplified_required_thickness_is_152mm_for_concrete()
    {
        SimulatorShieldingEngine engine = new SimulatorShieldingEngine();
        SimulatorShieldingResult result = engine.Evaluate(BuildAerbInput());
        result.Barriers[0].RequiredThicknessMm.Should().Be(152.0);
    }

    [Fact]
    public void Kvp_tvl_table_returns_concrete_5pt3cm_at_100kv()
    {
        KvpTvlLookup tvl = Ncrp49Tables.TvlForKvp(100, BarrierMaterial.Concrete);
        tvl.TvlCm.Should().Be(5.3);
    }

    [Fact]
    public void Engine_throws_on_null_input()
    {
        SimulatorShieldingEngine engine = new SimulatorShieldingEngine();
        Action act = () => engine.Evaluate(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
