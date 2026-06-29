using Aran.Engines.Linac;
using Aran.Machines;
using Aran.Model;
using Aran.Report.Linac;

namespace Aran.App.Services;

/// <summary>
/// Stateless service that produces report PDFs from AppSession results.
/// Output capped at 2 MB per project constraint.
/// </summary>
public sealed class ReportBuilder
{
    private const int MaxOutputBytes = 2 * 1024 * 1024;

    public void BuildLinacReports(AppSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        MachineModel machine = MachineCatalog.FindByName(session.MachineName)
            ?? throw new InvalidOperationException("Unknown machine: " + session.MachineName);

        LinacShieldingInput input = BuildInput(session, machine);
        MazeRun mazeRun = EngineRunner.BuildMazeRun(session);
        LinacReportGenerator gen = new LinacReportGenerator();

        if (session.NcrpWallResult is not null && session.NcrpDoorResult is not null)
        {
            LinacReportInput ncrpInput = new LinacReportInput(
                session.FacilityName, session.FacilityAddress, session.FacilityFloor,
                session.DrawingReference, session.PreparedBy, session.EloraRpId,
                session.PreparedDate, machine, input, mazeRun,
                session.NcrpWallResult, session.NcrpDoorResult);
            byte[] pdf = gen.Generate(ncrpInput);
            session.NcrpReportBytes = pdf.Length <= MaxOutputBytes ? pdf : null;
        }

        if (session.AerbWallResult is not null && session.AerbDoorResult is not null)
        {
            LinacReportInput aerbInput = new LinacReportInput(
                session.FacilityName, session.FacilityAddress, session.FacilityFloor,
                session.DrawingReference, session.PreparedBy, session.EloraRpId,
                session.PreparedDate, machine, input, mazeRun,
                session.AerbWallResult, session.AerbDoorResult);
            byte[] pdf = gen.Generate(aerbInput);
            session.AerbLinacReportBytes = pdf.Length <= MaxOutputBytes ? pdf : null;
        }
    }

    private static LinacShieldingInput BuildInput(AppSession session, MachineModel machine)
    {
        ShieldingGeometryModel geometry = session.ConfirmedModel
            ?? throw new InvalidOperationException("Canvas not confirmed.");

        List<EnergyWorkload> workloads = new List<EnergyWorkload>();
        foreach (KeyValuePair<string, double> kv in session.Workloads)
        {
            workloads.Add(new EnergyWorkload(kv.Key, kv.Value));
        }

        List<BarrierEvaluationInput> barriers = new List<BarrierEvaluationInput>();
        foreach (Aran.Model.Barrier b in geometry.Barriers)
        {
            barriers.Add(new BarrierEvaluationInput(
                b.Id, BarrierRole.Secondary, AreaClass.Uncontrolled,
                OccupancyCategory.FullOccupancy, 1.0, null,
                new SecondaryDistances(3.0, 1.0, 3.0, 90.0, 400.0)));
        }

        return new LinacShieldingInput(geometry, machine, workloads, barriers);
    }
}
