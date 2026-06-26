using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Engines.Linac;
using Aran.Machines;
using Aran.Model;

namespace Aran.LinacHarness;

/// <summary>
/// Demonstrates the LINAC shielding engine by running a sample layout under both the
/// NCRP 151 and AERB standards and printing each calculation as a symbolic formula line
/// followed by the same formula with values substituted.
/// </summary>
public static class Program
{
    /// <summary>Runs the demonstration.</summary>
    public static void Main()
    {
        LinacShieldingInput input = BuildSample();
        LinacShieldingEngine engine = new LinacShieldingEngine();

        Ncrp151Standard ncrp = Standards.Ncrp151 with { IsConfirmed = true };
        AerbStandard aerb = Standards.Aerb with { IsConfirmed = true };

        Print(engine.Evaluate(input, ncrp));
        Print(engine.Evaluate(input, aerb));
    }

    private static void Print(LinacShieldingResult result)
    {
        Console.WriteLine("================ " + result.StandardName + " ================");
        Console.WriteLine("Compliant: " + result.IsCompliant);
        foreach (LinacBarrierEvaluation barrier in result.Barriers)
        {
            Console.WriteLine();
            Console.WriteLine("Barrier " + barrier.BarrierId + " (" + barrier.Role + ", " + barrier.Material + ")");
            Console.WriteLine("  required " + F(barrier.RequiredThicknessMm) + " mm | provided "
                + F(barrier.ProvidedThicknessMm) + " mm | governed by " + barrier.GoverningComponent
                + " | " + (barrier.Passes ? "PASS" : "FAIL"));
            foreach (ComponentResult component in barrier.Components)
            {
                Console.WriteLine("  -- " + component.Kind + " [" + component.BeamModeName + ", "
                    + component.EnergyMv + " MV] -> " + F(component.RequiredThicknessMm) + " mm");
                foreach (CalculationStep step in component.Steps)
                {
                    Console.WriteLine("       " + step.Formula);
                    Console.WriteLine("       " + step.Substituted + "  =  " + F(step.Result.Value) + " " + step.Result.Unit);
                }

                foreach (string note in component.Notes)
                {
                    Console.WriteLine("       note: " + note);
                }
            }
        }

        Console.WriteLine();
    }

    private static string F(double value)
    {
        if (value == 0.0)
        {
            return "0";
        }

        double abs = Math.Abs(value);
        if (abs < 1.0e-3 || abs >= 1.0e5)
        {
            return value.ToString("0.###e+00", CultureInfo.InvariantCulture);
        }

        return value.ToString("0.####", CultureInfo.InvariantCulture);
    }

    private static LinacShieldingInput BuildSample()
    {
        List<PointMm> wall = new List<PointMm> { new PointMm(0, 0), new PointMm(0, 5000) };
        Barrier primary = new Barrier("PrimaryWall", new Polyline(wall), 2000.0, BarrierMaterial.Concrete, 2.35, "vault", "office", Provenance.Manual, true);
        Barrier secondary = new Barrier("SideWall", new Polyline(wall), 1200.0, BarrierMaterial.Concrete, 2.35, "vault", "corridor", Provenance.Manual, true);
        List<Barrier> barriers = new List<Barrier> { primary, secondary };
        ShieldingGeometryModel geometry = new ShieldingGeometryModel(null, new List<Room>(), barriers, new List<RadiationSource>());

        List<EnergyWorkload> workloads = new List<EnergyWorkload>
        {
            new EnergyWorkload("6X", 1000.0),
            new EnergyWorkload("10X", 1000.0),
            new EnergyWorkload("15X", 500.0),
            new EnergyWorkload("6FFF", 1000.0),
            new EnergyWorkload("10FFF", 1000.0),
        };

        List<BarrierEvaluationInput> inputs = new List<BarrierEvaluationInput>
        {
            new BarrierEvaluationInput(
                "PrimaryWall", BarrierRole.Primary, AreaClass.Controlled, OccupancyCategory.FullOccupancy,
                0.25, new PrimaryDistances(5.0), null),
            new BarrierEvaluationInput(
                "SideWall", BarrierRole.Secondary, AreaClass.Uncontrolled, OccupancyCategory.Corridor,
                1.0, null, new SecondaryDistances(5.0, 1.0, 4.0, 90.0, 400.0)),
        };

        return new LinacShieldingInput(geometry, MachineCatalog.TrueBeam, workloads, inputs);
    }
}
