using System;
using System.Collections.Generic;
using Aran.Machines;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Engines.Linac.Tests;

public sealed class EngineTests
{
    private static LinacShieldingInput BuildPrimaryCase()
    {
        List<PointMm> centreline = new List<PointMm> { new PointMm(0, 0), new PointMm(0, 4000) };
        Barrier barrier = new Barrier(
            "B1",
            new Polyline(centreline),
            2000.0,
            BarrierMaterial.Concrete,
            2.35,
            "vault",
            "outside",
            Provenance.Manual,
            true);
        List<Barrier> barriers = new List<Barrier> { barrier };
        ShieldingGeometryModel geometry = new ShieldingGeometryModel(
            null,
            new List<Room>(),
            barriers,
            new List<RadiationSource>());

        List<BeamMode> modes = new List<BeamMode> { new BeamMode("6X", 6, false) };
        MachineModel machine = new MachineModel("TestLinac", MachineType.LinacTrueBeam, modes, null);

        List<EnergyWorkload> workloads = new List<EnergyWorkload> { new EnergyWorkload("6X", 1000.0) };
        List<BarrierEvaluationInput> inputs = new List<BarrierEvaluationInput>
        {
            new BarrierEvaluationInput(
                "B1",
                BarrierRole.Primary,
                AreaClass.Controlled,
                OccupancyCategory.FullOccupancy,
                0.25,
                new PrimaryDistances(4.0),
                null),
        };

        return new LinacShieldingInput(geometry, machine, workloads, inputs);
    }

    [Fact]
    public void Engine_refuses_to_run_on_an_unconfirmed_standard()
    {
        LinacShieldingEngine engine = new LinacShieldingEngine();
        LinacShieldingInput input = BuildPrimaryCase();

        Action act = () => engine.Evaluate(input, Standards.Ncrp151);

        act.Should().Throw<InvalidOperationException>().WithMessage("*not confirmed*");
    }

    [Fact]
    public void Ncrp_primary_barrier_matches_hand_calculation()
    {
        LinacShieldingEngine engine = new LinacShieldingEngine();
        Ncrp151Standard ncrp = Standards.Ncrp151 with { IsConfirmed = true };

        LinacShieldingResult result = engine.Evaluate(BuildPrimaryCase(), ncrp);

        LinacBarrierEvaluation barrier = result.Barriers[0];
        barrier.Components[0].TransmissionB.Should().BeApproximately(6.4e-6, 1e-8);
        barrier.RequiredThicknessMm.Should().BeApproximately(1754.0, 2.0);
        barrier.Passes.Should().BeTrue();
    }

    [Fact]
    public void Aerb_allows_thinner_primary_than_ncrp_for_same_layout()
    {
        LinacShieldingEngine engine = new LinacShieldingEngine();
        Ncrp151Standard ncrp = Standards.Ncrp151 with { IsConfirmed = true };
        AerbStandard aerb = Standards.Aerb with { IsConfirmed = true };
        LinacShieldingInput input = BuildPrimaryCase();

        double ncrpRequired = engine.Evaluate(input, ncrp).Barriers[0].RequiredThicknessMm;
        double aerbRequired = engine.Evaluate(input, aerb).Barriers[0].RequiredThicknessMm;

        aerbRequired.Should().BeLessThan(ncrpRequired);
        aerbRequired.Should().BeApproximately(1555.0, 2.0);
    }

    [Fact]
    public void Substituted_line_carries_the_active_standard_value_of_P()
    {
        LinacShieldingEngine engine = new LinacShieldingEngine();
        Ncrp151Standard ncrp = Standards.Ncrp151 with { IsConfirmed = true };

        LinacShieldingResult result = engine.Evaluate(BuildPrimaryCase(), ncrp);

        CalculationStep transmission = result.Barriers[0].Components[0].Steps[0];
        transmission.Formula.Should().Contain("B_pri = P*d_pri^2");
        transmission.Substituted.Should().Contain("1e-04");
    }
}
