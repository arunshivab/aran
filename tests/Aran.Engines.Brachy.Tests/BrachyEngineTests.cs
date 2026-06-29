using System;
using System.Collections.Generic;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Engines.Brachy.Tests;

public sealed class BrachyEngineTests
{
    // Hand-verification: Cs-137, A=10 GBq, h=10 h/wk, d=2 m, concrete, controlled AERB (P=4e-4 Sv/wk, T=1)
    // DR at 1m = 0.0836 × 10 = 0.836 mSv/h
    // H = 0.836 × (1/4) × 10 × 1 = 2.09 mSv/wk = 2.09e-3 Sv/wk
    // B = 4e-4 / 2.09e-3 = 0.1914
    // n = -log10(0.1914) = 0.7180
    // t = 0.7180 × 15.7 = 11.27 cm = 112.7 mm

    private static BrachyShieldingInput BuildCs137Input(BrachyStandard std)
    {
        List<BrachyBarrierInput> barriers = new List<BrachyBarrierInput>
        {
            new BrachyBarrierInput("W1", BarrierMaterial.Concrete, 200.0, 2.0, AreaClass.Controlled, 1.0),
        };
        return new BrachyShieldingInput(IsotopeCatalog.Cs137, 10.0, 10.0, std, barriers);
    }

    [Fact]
    public void Cs137_aerb_required_thickness_matches_hand_calc()
    {
        BrachytherapyShieldingEngine engine = new BrachytherapyShieldingEngine();
        BrachyShieldingResult result = engine.Evaluate(BuildCs137Input(BrachyStandard.Aerb));
        result.Barriers[0].RequiredThicknessMm.Should().BeApproximately(112.7, 1.0);
    }

    [Fact]
    public void Cs137_ncrp_requires_more_shielding_than_aerb()
    {
        BrachytherapyShieldingEngine engine = new BrachytherapyShieldingEngine();
        double ncrp = engine.Evaluate(BuildCs137Input(BrachyStandard.Ncrp)).Barriers[0].RequiredThicknessMm;
        double aerb = engine.Evaluate(BuildCs137Input(BrachyStandard.Aerb)).Barriers[0].RequiredThicknessMm;
        ncrp.Should().BeGreaterThan(aerb, "NCRP controlled P is 4× lower than AERB");
    }

    [Fact]
    public void Thick_concrete_wall_passes()
    {
        BrachytherapyShieldingEngine engine = new BrachytherapyShieldingEngine();
        BrachyShieldingResult result = engine.Evaluate(BuildCs137Input(BrachyStandard.Aerb));
        result.Barriers[0].Passes.Should().BeTrue();
        result.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public void Engine_throws_on_null_input()
    {
        BrachytherapyShieldingEngine engine = new BrachytherapyShieldingEngine();
        Action act = () => engine.Evaluate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Isotope_catalog_has_three_entries()
    {
        IsotopeCatalog.All.Should().HaveCount(3);
    }

    [Fact]
    public void Steps_carry_formula_and_substituted_lines()
    {
        BrachytherapyShieldingEngine engine = new BrachytherapyShieldingEngine();
        BrachyShieldingResult result = engine.Evaluate(BuildCs137Input(BrachyStandard.Aerb));
        foreach (BrachyCalcStep step in result.Barriers[0].Steps)
        {
            step.Formula.Should().NotBeNullOrWhiteSpace();
            step.Substituted.Should().NotBeNullOrWhiteSpace();
        }
    }
}
