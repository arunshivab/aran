using System;
using System.Collections.Generic;
using Aran.Engines.Linac;
using Aran.Machines;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Report.Linac.Tests;

public sealed class ReportGeneratorTests
{
    private static LinacReportInput BuildInput(string standardName, bool isAerb)
    {
        List<PointMm> wall = new List<PointMm> { new PointMm(0, 0), new PointMm(0, 5000) };
        Barrier barrier = new Barrier("B1", new Polyline(wall), 2000.0,
            BarrierMaterial.Concrete, 2.35, "vault", "office", Provenance.Manual, true);
        ShieldingGeometryModel geometry = new ShieldingGeometryModel(
            null, new List<Room>(), new List<Barrier> { barrier }, new List<RadiationSource>());

        List<BeamMode> modes = new List<BeamMode> { new BeamMode("6X", 6, false) };
        MachineModel machine = new MachineModel("TrueBeam", MachineType.LinacTrueBeam, modes, null);
        List<EnergyWorkload> workloads = new List<EnergyWorkload> { new EnergyWorkload("6X", 1000.0) };
        List<BarrierEvaluationInput> barriers = new List<BarrierEvaluationInput>
        {
            new BarrierEvaluationInput(
                "B1", BarrierRole.Primary, AreaClass.Controlled,
                OccupancyCategory.FullOccupancy, 0.25, new PrimaryDistances(5.0), null),
        };
        LinacShieldingInput input = new LinacShieldingInput(geometry, machine, workloads, barriers);

        ShieldingStandard std = isAerb
            ? (ShieldingStandard)(Standards.Aerb with { IsConfirmed = true })
            : (Standards.Ncrp151 with { IsConfirmed = true });

        LinacShieldingEngine wallEngine = new LinacShieldingEngine();
        LinacShieldingResult wallResult = wallEngine.Evaluate(input, std);

        MazePhotonGeometry photon = new MazePhotonGeometry(
            5.0, 3.5, 4.0, 5.0, 8.5, 1.0, 6.0,
            0.5, 5.0, 6.0, 1e-4, 45, 45, 45);
        MazeNeutronGeometry neutron = new MazeNeutronGeometry(6.4, 8.5, 236.0, 9.2, 8.4);
        MazeRun maze = new MazeRun(
            "Door1", AreaClass.Controlled, OccupancyCategory.FullOccupancy,
            0.25, 0.25, 90.0, 400.0, photon, null);

        DoorShieldingEngine doorEngine = new DoorShieldingEngine();
        DoorEvaluation doorResult = doorEngine.Evaluate(input, maze, std);

        return new LinacReportInput(
            "Index Medical College",
            "Gram Morodhat, Nemawar Road, Indore, MP 452016",
            "Basement",
            "R0 — 16.10.2025",
            "Dr. Test Expert",
            "RP-12345",
            new DateOnly(2026, 6, 29),
            machine,
            input,
            maze,
            wallResult,
            doorResult);
    }

    [Fact]
    public void Generate_ncrp_report_returns_valid_pdf()
    {
        LinacReportInput input = BuildInput("NCRP 151", false);
        LinacReportGenerator gen = new LinacReportGenerator();
        byte[] pdf = gen.Generate(input);
        pdf.Should().NotBeNullOrEmpty();
        // PDF magic bytes
        pdf[0].Should().Be((byte)'%');
        pdf[1].Should().Be((byte)'P');
        pdf[2].Should().Be((byte)'D');
        pdf[3].Should().Be((byte)'F');
    }

    [Fact]
    public void Generate_aerb_report_returns_valid_pdf()
    {
        LinacReportInput input = BuildInput("AERB", true);
        LinacReportGenerator gen = new LinacReportGenerator();
        byte[] pdf = gen.Generate(input);
        pdf.Should().NotBeNullOrEmpty();
        pdf[0].Should().Be((byte)'%');
        pdf[1].Should().Be((byte)'P');
        pdf[2].Should().Be((byte)'D');
        pdf[3].Should().Be((byte)'F');
    }

    [Fact]
    public void Generate_throws_on_null_input()
    {
        LinacReportGenerator gen = new LinacReportGenerator();
        Action act = () => gen.Generate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ncrp_and_aerb_reports_differ_in_content()
    {
        LinacReportGenerator gen = new LinacReportGenerator();
        byte[] ncrp = gen.Generate(BuildInput("NCRP 151", false));
        byte[] aerb = gen.Generate(BuildInput("AERB", true));
        ncrp.Should().NotEqual(aerb, "different standards produce different PDFs");
    }
}
