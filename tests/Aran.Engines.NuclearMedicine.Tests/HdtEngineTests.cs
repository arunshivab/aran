using System;
using System.Collections.Generic;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Engines.NuclearMedicine.Tests;

public sealed class HdtEngineTests
{
    // Hand-verification: I-131, A = 11100 MBq (300 mCi), h = 40 hr/wk, d = 3 m, concrete, public
    // DR at 1m = 0.08140 × 11100 = 903.5 µSv/h
    // Weekly dose = 903.5 × 40 / 9 = 4015.6 µSv/wk = 4.016e-3 Sv/wk
    // B = 2e-5 / 4.016e-3 = 4.98e-3
    // n = -log10(4.98e-3) = 2.303
    // t = 2.303 × 10 cm = 23.03 cm = 230.3 mm

    private static HdtShieldingInput BuildInput()
    {
        List<NmBarrierInput> barriers = new List<NmBarrierInput>
        {
            new NmBarrierInput("W1", BarrierMaterial.Concrete, 300.0, 3.0, AreaClass.Uncontrolled),
        };
        return new HdtShieldingInput(NmConstants.AerbHdtMaxActivityMbq, 40.0, barriers);
    }

    [Fact]
    public void Required_thickness_matches_hand_calc()
    {
        HdtShieldingEngine engine = new HdtShieldingEngine();
        HdtShieldingResult result = engine.Evaluate(BuildInput());
        result.Barriers[0].RequiredThicknessMm.Should().BeApproximately(230.3, 2.0);
    }

    [Fact]
    public void Provided_300mm_passes()
    {
        HdtShieldingEngine engine = new HdtShieldingEngine();
        HdtShieldingResult result = engine.Evaluate(BuildInput());
        result.Barriers[0].Passes.Should().BeTrue();
        result.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public void Engine_throws_on_null()
    {
        HdtShieldingEngine engine = new HdtShieldingEngine();
        Action act = () => engine.Evaluate(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
