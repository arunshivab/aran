using System;
using System.Collections.Generic;
using Aran.Engines.NuclearMedicine;
using Aran.Model;
using FluentAssertions;
using Xunit;

namespace Aran.Report.NuclearMedicine.Tests;

public sealed class NmReportGeneratorTests
{
    private static NmReportInput BuildPetOnly()
    {
        List<NmBarrierInput> uptake = new List<NmBarrierInput>
        {
            new NmBarrierInput("UptakeWall", BarrierMaterial.Concrete, 200, 4.0, AreaClass.Uncontrolled),
        };
        List<NmBarrierInput> imaging = new List<NmBarrierInput>
        {
            new NmBarrierInput("ImagingWall", BarrierMaterial.Concrete, 200, 3.0, AreaClass.Uncontrolled),
        };
        PetShieldingInput petInput = new PetShieldingInput(
            40, NmConstants.AerbPetAdministeredActivityMbq,
            NmConstants.AerbPetUptakeTimeMin, NmConstants.AerbPetImagingTimeMin,
            uptake, imaging);
        PetShieldingResult petResult = new PetShieldingEngine().Evaluate(petInput);
        return new NmReportInput(
            "Test NM Facility", "123 Test Road, Mumbai 400001", "Ground Floor",
            "R0 — 01.01.2026", "Dr. Test Expert", "RP-00001",
            new DateOnly(2026, 6, 29),
            "PET/PET-CT",
            petInput, petResult,
            null, null, null, null);
    }

    private static NmReportInput BuildAllModalities()
    {
        List<NmBarrierInput> uptake = new List<NmBarrierInput>
        {
            new NmBarrierInput("U1", BarrierMaterial.Concrete, 200, 4.0, AreaClass.Uncontrolled),
        };
        List<NmBarrierInput> imaging = new List<NmBarrierInput>
        {
            new NmBarrierInput("I1", BarrierMaterial.Concrete, 200, 3.0, AreaClass.Uncontrolled),
        };
        PetShieldingInput petIn = new PetShieldingInput(40, 370, 45, 30, uptake, imaging);
        PetShieldingResult petRes = new PetShieldingEngine().Evaluate(petIn);

        List<NmBarrierInput> gcBarriers = new List<NmBarrierInput>
        {
            new NmBarrierInput("GC1", BarrierMaterial.Concrete, 200, 3.0, AreaClass.Uncontrolled),
        };
        GammaCameraShieldingInput gcIn = new GammaCameraShieldingInput(370, 20, 0.5, gcBarriers);
        GammaCameraShieldingResult gcRes = new GammaCameraShieldingEngine().Evaluate(gcIn);

        List<NmBarrierInput> hdtBarriers = new List<NmBarrierInput>
        {
            new NmBarrierInput("HDT1", BarrierMaterial.Concrete, 300, 3.0, AreaClass.Uncontrolled),
        };
        HdtShieldingInput hdtIn = new HdtShieldingInput(NmConstants.AerbHdtMaxActivityMbq, 40, hdtBarriers);
        HdtShieldingResult hdtRes = new HdtShieldingEngine().Evaluate(hdtIn);

        return new NmReportInput(
            "Full NM Facility", "456 Test Avenue, Delhi 110001", "Basement",
            "R1 — 01.01.2026", "Dr. Expert", "RP-00002",
            new DateOnly(2026, 6, 29),
            "PET/PET-CT, Gamma Camera/SPECT, HDT",
            petIn, petRes, gcIn, gcRes, hdtIn, hdtRes);
    }

    [Fact]
    public void Generate_pet_only_report_is_valid_pdf()
    {
        NmReportGenerator gen = new NmReportGenerator();
        byte[] pdf = gen.Generate(BuildPetOnly());
        pdf.Should().NotBeNullOrEmpty();
        pdf[0].Should().Be((byte)'%');
        pdf[1].Should().Be((byte)'P');
        pdf[2].Should().Be((byte)'D');
        pdf[3].Should().Be((byte)'F');
    }

    [Fact]
    public void Generate_all_modalities_report_is_valid_pdf()
    {
        NmReportGenerator gen = new NmReportGenerator();
        byte[] pdf = gen.Generate(BuildAllModalities());
        pdf.Should().NotBeNullOrEmpty();
        pdf[0].Should().Be((byte)'%');
    }

    [Fact]
    public void All_modalities_report_is_larger_than_pet_only()
    {
        NmReportGenerator gen = new NmReportGenerator();
        byte[] pet = gen.Generate(BuildPetOnly());
        byte[] all = gen.Generate(BuildAllModalities());
        all.Length.Should().BeGreaterThan(pet.Length);
    }

    [Fact]
    public void Generate_throws_on_null_input()
    {
        NmReportGenerator gen = new NmReportGenerator();
        Action act = () => gen.Generate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generate_throws_when_no_modality_supplied()
    {
        NmReportGenerator gen = new NmReportGenerator();
        NmReportInput empty = new NmReportInput(
            "F", "A", "G", "R", "P", "E",
            new DateOnly(2026, 1, 1), "None",
            null, null, null, null, null, null);
        Action act = () => gen.Generate(empty);
        act.Should().Throw<ArgumentException>().WithMessage("*modality*");
    }
}
