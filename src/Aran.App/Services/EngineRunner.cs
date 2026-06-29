using Aran.Engines.Linac;
using Aran.Machines;
using Aran.Model;

namespace Aran.App.Services;

/// <summary>
/// Stateless service that wires AppSession inputs into the shielding engines
/// and writes results back. Called from the Run page.
/// </summary>
public sealed class EngineRunner
{
    public void Run(AppSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        MachineModel machine = MachineCatalog.FindByName(session.MachineName)
            ?? throw new InvalidOperationException("Unknown machine: " + session.MachineName);

        ShieldingGeometryModel geometry = session.ConfirmedModel
            ?? throw new InvalidOperationException("Canvas step not confirmed.");

        List<EnergyWorkload> workloads = new List<EnergyWorkload>();
        foreach (KeyValuePair<string, double> kv in session.Workloads)
        {
            workloads.Add(new EnergyWorkload(kv.Key, kv.Value));
        }

        List<BarrierEvaluationInput> barriers = BuildBarrierInputs(geometry);
        LinacShieldingInput input = new LinacShieldingInput(geometry, machine, workloads, barriers);
        MazeRun mazeRun = BuildMazeRun(session);

        LinacShieldingEngine wallEngine = new LinacShieldingEngine();
        DoorShieldingEngine doorEngine = new DoorShieldingEngine();
        Ncrp151Standard ncrp = Standards.Ncrp151 with { IsConfirmed = true };
        AerbStandard aerb = Standards.Aerb with { IsConfirmed = true };

        session.NcrpWallResult = wallEngine.Evaluate(input, ncrp);
        session.AerbWallResult = wallEngine.Evaluate(input, aerb);
        session.NcrpDoorResult = doorEngine.Evaluate(input, mazeRun, ncrp);
        session.AerbDoorResult = doorEngine.Evaluate(input, mazeRun, aerb);
        session.RunCompleted = true;
    }

    private static List<BarrierEvaluationInput> BuildBarrierInputs(ShieldingGeometryModel geometry)
    {
        List<BarrierEvaluationInput> list = new List<BarrierEvaluationInput>();
        foreach (Aran.Model.Barrier b in geometry.Barriers)
        {
            // All unconfirmed barriers default to secondary; physicist corrects on canvas
            list.Add(new BarrierEvaluationInput(
                b.Id, BarrierRole.Secondary, AreaClass.Uncontrolled,
                OccupancyCategory.FullOccupancy, 1.0, null,
                new SecondaryDistances(3.0, 1.0, 3.0, 90.0, 400.0)));
        }

        return list;
    }

    /// <summary>Builds a MazeRun from session form values. Shared with ReportBuilder.</summary>
    public static MazeRun BuildMazeRun(AppSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        MazeRunFormValues f = session.MazeRun;

        MazePhotonGeometry photon = new MazePhotonGeometry(
            f.DhM, f.DrM, f.DzM, f.DsecM, f.DzzM, f.DscaM, f.DlM,
            f.A0M2, f.A1M2, f.AzM2,
            f.InnerWallTransmissionB,
            f.ScatterAngleDeg, f.ScatterAngleDeg, f.ScatterAngleDeg);

        MazeNeutronGeometry? neutron = f.HasNeutron
            ? new MazeNeutronGeometry(f.D1M, f.D2M, f.RoomSurfaceM2, f.S0M2, f.S1M2)
            : null;

        return new MazeRun(
            "Door1",
            AreaClass.Controlled,
            OccupancyCategory.VaultDoor,
            0.25,
            f.PatientTransmissionF,
            f.FieldAreaCm2,
            f.ScatterAngleDeg,
            photon,
            neutron);
    }
}
