using System;
using System.Collections.Generic;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Engines.NuclearMedicine.Tests;

/// <summary>
/// PET engine tests pinned against AAPM TG-108 worked examples.
/// Example 1 (uptake room): 40 patients/wk, 555 MBq, 60 min uptake, d=4 m, T=1, uncontrolled.
/// B = 218 × 16 / (1 × 40 × 555 × 1 × 0.83) = 0.189  → 1.2 cm Pb or 15 cm concrete (Table IV).
/// Example 2 (imaging room): 40 patients/wk, 555 MBq, 60 min uptake, 30 min imaging, d=3 m, T=1, uncontrolled.
/// </summary>
public sealed class PetEngineTests
{
    private static PetShieldingInput BuildTg108Example1Input()
    {
        List<NmBarrierInput> uptake = new List<NmBarrierInput>
        {
            new NmBarrierInput("Wall", BarrierMaterial.Concrete, 0, 4.0, AreaClass.Uncontrolled),
        };
        // Example 1 uses 555 MBq and 60 min uptake (TG-108 convention, not AERB default)
        return new PetShieldingInput(40, 555.0, 60.0, 30.0, uptake, new List<NmBarrierInput>());
    }

    private static PetShieldingInput BuildTg108Example2Input()
    {
        List<NmBarrierInput> imaging = new List<NmBarrierInput>
        {
            new NmBarrierInput("Wall", BarrierMaterial.Concrete, 0, 3.0, AreaClass.Uncontrolled),
        };
        return new PetShieldingInput(40, 555.0, 60.0, 30.0, new List<NmBarrierInput>(), imaging);
    }

    [Fact]
    public void Uptake_room_transmission_matches_tg108_example_1()
    {
        // TG-108 Example 1: B = 218 × 16 / (40 × 555 × 1 × 0.83) = 0.189
        PetShieldingEngine engine = new PetShieldingEngine();
        PetShieldingResult result = engine.Evaluate(BuildTg108Example1Input());
        result.Barriers[0].TransmissionB.Should().BeApproximately(0.189, 0.005);
    }

    [Fact]
    public void Uptake_room_concrete_thickness_matches_tg108_example_1()
    {
        // TG-108 Example 1: 15 cm concrete required (from Table IV B=0.189)
        PetShieldingEngine engine = new PetShieldingEngine();
        PetShieldingResult result = engine.Evaluate(BuildTg108Example1Input());
        result.Barriers[0].RequiredConcreteMm.Should().BeApproximately(150.0, 5.0);
    }

    [Fact]
    public void Uptake_room_lead_thickness_matches_tg108_example_1()
    {
        // TG-108 Example 1: 1.2 cm lead required (from Table IV B=0.189)
        PetShieldingEngine engine = new PetShieldingEngine();
        PetShieldingResult result = engine.Evaluate(BuildTg108Example1Input());
        result.Barriers[0].RequiredLeadMm.Should().BeApproximately(12.0, 2.0);
    }

    [Fact]
    public void Imaging_room_transmission_matches_tg108_example_2()
    {
        // TG-108 Example 2: weekly dose = 59.7 µSv → B = 20/59.7 = 0.335
        PetShieldingEngine engine = new PetShieldingEngine();
        PetShieldingResult result = engine.Evaluate(BuildTg108Example2Input());
        result.Barriers[0].TransmissionB.Should().BeApproximately(0.335, 0.01);
    }

    [Fact]
    public void Steps_carry_formula_and_substituted_for_both_rooms()
    {
        PetShieldingEngine engine = new PetShieldingEngine();
        List<NmBarrierInput> uptake = new List<NmBarrierInput>
        {
            new NmBarrierInput("U1", BarrierMaterial.Concrete, 200, 3.0, AreaClass.Uncontrolled),
        };
        List<NmBarrierInput> imaging = new List<NmBarrierInput>
        {
            new NmBarrierInput("I1", BarrierMaterial.Concrete, 200, 3.0, AreaClass.Uncontrolled),
        };
        PetShieldingInput input = new PetShieldingInput(
            40, NmConstants.AerbPetAdministeredActivityMbq,
            NmConstants.AerbPetUptakeTimeMin, NmConstants.AerbPetImagingTimeMin,
            uptake, imaging);
        PetShieldingResult result = engine.Evaluate(input);
        result.Barriers.Should().HaveCount(2);
        foreach (PetBarrierResult barrier in result.Barriers)
        {
            foreach (NmCalcStep step in barrier.Steps)
            {
                step.Formula.Should().NotBeNullOrWhiteSpace();
                step.Substituted.Should().NotBeNullOrWhiteSpace();
            }
        }
    }

    [Fact]
    public void Engine_throws_on_null_input()
    {
        PetShieldingEngine engine = new PetShieldingEngine();
        Action act = () => engine.Evaluate(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
