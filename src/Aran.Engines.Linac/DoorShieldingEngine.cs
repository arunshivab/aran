using System;
using System.Collections.Generic;
using System.Globalization;
using Aran.Machines;

namespace Aran.Engines.Linac;

/// <summary>
/// Evaluates the dose equivalent at the maze door and, when the bare dose exceeds
/// the design goal, sizes a lead/BPE/lead sandwich door.
///
/// Photon path (all energies): HS Eq 2.9, HLS Eq 2.10, Hps Eq 2.11, HLT Eq 2.12,
/// HG = f·HS+HLS+Hps+HLT Eq 2.13, HTot = 2.64·HG Eq 2.14.
///
/// Neutron path (>10 MV under NCRP, ≥10 MV under AERB): neutron fluence φA Eq 2.16,
/// capture gamma Hcg Eq 2.15+2.17, neutron dose Hn,D via Wu–McGinley Eq 2.19+2.20,
/// Hn Eq 2.21, total HW = HTot+Hcg+Hn Eq 2.22.
///
/// Door shielding (only when HW > P): lead TVL 6.1 cm (3.6 MeV capture gamma,
/// NCRP 151 §2.4.3); BPE TVL 4.5 cm (neutrons, NCRP 151 §2.4.3).
/// </summary>
public sealed class DoorShieldingEngine
{
    private const double KCapture = 6.9e-16;   // Sv m² per neutron (NCRP 151 Eq 2.15)
    private const double WuMcGinleyConst = 2.4e-15; // Sv n^-1 m² (NCRP 151 Eq 2.19)
    private const double WuExp1Const = 1.64;    // coefficient in fast-neutron term (Eq 2.19)
    private const double WuTvd1 = 1.9;          // m, fast-neutron TVD (Eq 2.19)
    private const double EmpiricalFactor = 2.64; // Eq 2.14
    private const double LeakageFraction = 1e-3; // Lf = 0.1 % per IEC
    private const double LeadTvlCm = 6.1;       // cm, 3.6 MeV capture gamma (NCRP §2.4.3)
    private const double BpeTvlCm = 4.5;        // cm, neutrons (NCRP §2.4.3)
    private const double Log10Of2 = 0.30102999566;

    /// <summary>Evaluates the maze door under a single shielding standard.</summary>
    /// <param name="input">The LINAC shielding input (supplies machine + workloads).</param>
    /// <param name="maze">The confirmed maze run.</param>
    /// <param name="standard">The standard; must be confirmed.</param>
    /// <returns>The door evaluation.</returns>
    public DoorEvaluation Evaluate(LinacShieldingInput input, MazeRun maze, ShieldingStandard standard)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(maze);
        ArgumentNullException.ThrowIfNull(standard);
        if (!standard.IsConfirmed)
        {
            throw new InvalidOperationException(
                "Standard '" + standard.Name + "' is not confirmed.");
        }

        CitedValue designGoal = standard.DesignGoalSvPerWeek(maze.ProtectedClass);
        CitedValue occupancy = standard.Occupancy(maze.Occupancy);
        CitedValue u = new CitedValue(maze.UseFactorG, "physicist-specified use factor for Wall G");

        List<DoorComponentResult> components = new List<DoorComponentResult>();
        double hTot = 0.0;
        double hCg = 0.0;
        double hN = 0.0;

        foreach (BeamMode mode in input.Machine.Modes)
        {
            WorkloadValue workload = standard.Workload(input.Machine, mode, input.Workloads);
            bool isNeutronEnergy = IsNeutronEnergy(mode.NominalMv, standard);

            // --- photon components per mode ---
            DoorComponentResult hs = BuildHs(maze, mode, workload, u, designGoal, occupancy);
            DoorComponentResult hls = BuildHls(maze, mode, workload, u, designGoal, occupancy);
            DoorComponentResult hps = BuildHps(maze, mode, workload, u, designGoal, occupancy);
            DoorComponentResult hlt = BuildHlt(maze, mode, workload, u, designGoal, occupancy);
            components.Add(hs);
            components.Add(hls);
            components.Add(hps);
            components.Add(hlt);

            double hG = maze.PatientTransmissionF * hs.DoseSvPerWeek
                      + hls.DoseSvPerWeek
                      + hps.DoseSvPerWeek
                      + hlt.DoseSvPerWeek;
            hTot += EmpiricalFactor * hG;   // sum over modes; 2.64·HG per mode

            // --- neutron components (only when energy crosses standard threshold) ---
            if (isNeutronEnergy)
            {
                if (maze.Neutron is null)
                {
                    throw new ArgumentException(
                        "Mode " + mode.Name + " (" + mode.NominalMv + " MV) requires neutron geometry " +
                        "under " + standard.Name + " (threshold ≥10 MV), but MazeRun.Neutron is null.",
                        nameof(maze));
                }

                NeutronSource? neutronSource = NeutronCatalog.ForMachine(input.Machine.Name);
                if (neutronSource is null && mode.NominalMv == 10)
                {
                    neutronSource = NeutronCatalog.Generic10Mv;
                }

                if (neutronSource is null)
                {
                    throw new InvalidOperationException(
                        "No neutron source data for machine '" + input.Machine.Name +
                        "' at " + mode.NominalMv + " MV. Add an entry to NeutronCatalog.");
                }

                (DoorComponentResult cg, double hcgMode) = BuildHcg(maze, mode, workload, neutronSource);
                (DoorComponentResult hn, double hnMode) = BuildHn(maze, mode, workload, neutronSource);
                components.Add(cg);
                components.Add(hn);
                hCg += hcgMode;
                hN += hnMode;
            }
        }

        double bareDose = hTot + hCg + hN;
        double p = designGoal.Value * occupancy.Value;
        bool barePasses = bareDose <= p;

        DoorShielding? shielding = barePasses ? null : SizeDoor(bareDose, p, hCg, hN);
        return new DoorEvaluation(maze.DoorId, standard.Name, bareDose, p, barePasses, shielding, components);
    }

    // HS: primary scattered from Wall G (Eq 2.9)
    private DoorComponentResult BuildHs(MazeRun maze, BeamMode mode, WorkloadValue wl,
        CitedValue u, CitedValue p, CitedValue t)
    {
        MazePhotonGeometry g = maze.Photon;
        AlbedoLookup a0 = ReflectionTables.Alpha0Primary(mode.NominalMv, g.Alpha0ReflectionDeg);
        AlbedoLookup az = ReflectionTables.AlphaZSecondSurface(g.AlphaZReflectionDeg);
        ScatterFractionLookup sf = Ncrp151Tables.ScatterFraction(mode.NominalMv, maze.ScatterAngleDegrees);
        double num = wl.PrimaryGyPerWeek * u.Value * a0.Alpha * g.A0 * az.Alpha * g.Az;
        double den = g.Dh * g.Dr * g.Dz;
        double hs = den > 0 ? num / (den * den) : 0;
        List<string> notes = new List<string>(wl.Notes);
        foreach (string n in a0.Notes) { notes.Add(n); }
        foreach (string n in az.Notes) { notes.Add(n); }
        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Primary scatter at door (Eq 2.9)",
                "HS = W*UG*α0*A0*αz*Az / (dh*dr*dz)^2",
                "HS = " + Fmt(wl.PrimaryGyPerWeek) + "*" + Fmt(u.Value) + "*" + Fmt(a0.Alpha) +
                    "*" + Fmt(g.A0) + "*" + Fmt(az.Alpha) + "*" + Fmt(g.Az) +
                    " / (" + Fmt(g.Dh) + "*" + Fmt(g.Dr) + "*" + Fmt(g.Dz) + ")^2",
                new CalculationTerm[]
                {
                    new CalculationTerm("W", wl.PrimaryGyPerWeek, "Gy/wk", wl.Citation),
                    new CalculationTerm("UG", u.Value, "", u.Citation),
                    new CalculationTerm("α0", a0.Alpha, "", a0.Citation),
                    new CalculationTerm("A0", g.A0, "m²", "layout"),
                    new CalculationTerm("αz", az.Alpha, "", az.Citation),
                    new CalculationTerm("Az", g.Az, "m²", "layout"),
                    new CalculationTerm("dh", g.Dh, "m", "layout"),
                    new CalculationTerm("dr", g.Dr, "m", "layout"),
                    new CalculationTerm("dz", g.Dz, "m", "layout"),
                },
                new CalculationTerm("HS", hs, "Sv/wk", "NCRP 151 Eq 2.9")),
        };
        return new DoorComponentResult(DoorComponentKind.PrimaryScatterHs, mode.Name, mode.NominalMv, hs, steps, notes);
    }

    // HLS: leakage scattered from Wall G (Eq 2.10)
    private DoorComponentResult BuildHls(MazeRun maze, BeamMode mode, WorkloadValue wl,
        CitedValue u, CitedValue p, CitedValue t)
    {
        MazePhotonGeometry g = maze.Photon;
        AlbedoLookup a1 = ReflectionTables.Alpha1LeakageScatter(mode.NominalMv, g.Alpha1ReflectionDeg);
        double num = LeakageFraction * wl.LeakageGyPerWeek * u.Value * a1.Alpha * g.A1;
        double den = g.Dsec * g.Dzz;
        double hls = den > 0 ? num / (den * den) : 0;
        List<string> notes = new List<string>(wl.Notes);
        foreach (string n in a1.Notes) { notes.Add(n); }
        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Leakage scatter at door (Eq 2.10)",
                "HLS = Lf*WL*UG*α1*A1 / (dsec*dzz)^2",
                "HLS = " + Fmt(LeakageFraction) + "*" + Fmt(wl.LeakageGyPerWeek) + "*" + Fmt(u.Value) +
                    "*" + Fmt(a1.Alpha) + "*" + Fmt(g.A1) +
                    " / (" + Fmt(g.Dsec) + "*" + Fmt(g.Dzz) + ")^2",
                new CalculationTerm[]
                {
                    new CalculationTerm("Lf", LeakageFraction, "", "IEC 2002 (0.1%)"),
                    new CalculationTerm("WL", wl.LeakageGyPerWeek, "Gy/wk", wl.Citation),
                    new CalculationTerm("UG", u.Value, "", u.Citation),
                    new CalculationTerm("α1", a1.Alpha, "", a1.Citation),
                    new CalculationTerm("A1", g.A1, "m²", "layout"),
                    new CalculationTerm("dsec", g.Dsec, "m", "layout"),
                    new CalculationTerm("dzz", g.Dzz, "m", "layout"),
                },
                new CalculationTerm("HLS", hls, "Sv/wk", "NCRP 151 Eq 2.10")),
        };
        return new DoorComponentResult(DoorComponentKind.LeakageScatterHls, mode.Name, mode.NominalMv, hls, steps, notes);
    }

    // Hps: patient scattered (Eq 2.11)
    private DoorComponentResult BuildHps(MazeRun maze, BeamMode mode, WorkloadValue wl,
        CitedValue u, CitedValue p, CitedValue t)
    {
        MazePhotonGeometry g = maze.Photon;
        ScatterFractionLookup sf = Ncrp151Tables.ScatterFraction(mode.NominalMv, maze.ScatterAngleDegrees);
        AlbedoLookup a1 = ReflectionTables.Alpha1PatientScatter(g.Alpha1ReflectionDeg);
        double num = sf.Fraction * wl.PrimaryGyPerWeek * u.Value * (maze.FieldAreaCm2 / 400.0) * a1.Alpha * g.A1;
        double den = g.Dsca * g.Dsec * g.Dzz;
        double hps = den > 0 ? num / (den * den * den) : 0;
        List<string> notes = new List<string>(wl.Notes);
        foreach (string n in sf.Notes) { notes.Add(n); }
        foreach (string n in a1.Notes) { notes.Add(n); }
        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Patient scatter at door (Eq 2.11)",
                "Hps = a(θ)*W*UG*(F/400)*α1*A1 / (dsca*dsec*dzz)^2",
                "Hps = " + Fmt(sf.Fraction) + "*" + Fmt(wl.PrimaryGyPerWeek) + "*" + Fmt(u.Value) +
                    "*(" + Fmt(maze.FieldAreaCm2) + "/400)*" + Fmt(a1.Alpha) + "*" + Fmt(g.A1) +
                    " / (" + Fmt(g.Dsca) + "*" + Fmt(g.Dsec) + "*" + Fmt(g.Dzz) + ")^2",
                new CalculationTerm[]
                {
                    new CalculationTerm("a(θ)", sf.Fraction, "", sf.Citation),
                    new CalculationTerm("W", wl.PrimaryGyPerWeek, "Gy/wk", wl.Citation),
                    new CalculationTerm("UG", u.Value, "", u.Citation),
                    new CalculationTerm("F", maze.FieldAreaCm2, "cm²", "layout"),
                    new CalculationTerm("α1", a1.Alpha, "", a1.Citation),
                    new CalculationTerm("A1", g.A1, "m²", "layout"),
                    new CalculationTerm("dsca", g.Dsca, "m", "layout"),
                    new CalculationTerm("dsec", g.Dsec, "m", "layout"),
                    new CalculationTerm("dzz", g.Dzz, "m", "layout"),
                },
                new CalculationTerm("Hps", hps, "Sv/wk", "NCRP 151 Eq 2.11")),
        };
        return new DoorComponentResult(DoorComponentKind.PatientScatterHps, mode.Name, mode.NominalMv, hps, steps, notes);
    }

    // HLT: leakage transmitted through inner maze wall (Eq 2.12)
    private DoorComponentResult BuildHlt(MazeRun maze, BeamMode mode, WorkloadValue wl,
        CitedValue u, CitedValue p, CitedValue t)
    {
        MazePhotonGeometry g = maze.Photon;
        double num = LeakageFraction * wl.LeakageGyPerWeek * u.Value * g.InnerWallTransmissionB;
        double hlt = g.DL > 0 ? num / (g.DL * g.DL) : 0;
        List<string> notes = new List<string>(wl.Notes);
        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Leakage through inner maze wall (Eq 2.12)",
                "HLT = Lf*WL*UG*B / dL^2",
                "HLT = " + Fmt(LeakageFraction) + "*" + Fmt(wl.LeakageGyPerWeek) + "*" + Fmt(u.Value) +
                    "*" + Fmt(g.InnerWallTransmissionB) + " / " + Fmt(g.DL) + "^2",
                new CalculationTerm[]
                {
                    new CalculationTerm("Lf", LeakageFraction, "", "IEC 2002 (0.1%)"),
                    new CalculationTerm("WL", wl.LeakageGyPerWeek, "Gy/wk", wl.Citation),
                    new CalculationTerm("UG", u.Value, "", u.Citation),
                    new CalculationTerm("B", g.InnerWallTransmissionB, "", "inner wall transmission"),
                    new CalculationTerm("dL", g.DL, "m", "layout"),
                },
                new CalculationTerm("HLT", hlt, "Sv/wk", "NCRP 151 Eq 2.12")),
        };
        return new DoorComponentResult(DoorComponentKind.LeakageTransmissionHlt, mode.Name, mode.NominalMv, hlt, steps, notes);
    }

    // Hcg: neutron capture gamma (Eq 2.15–2.17)
    private (DoorComponentResult Result, double Dose) BuildHcg(
        MazeRun maze, BeamMode mode, WorkloadValue wl, NeutronSource neutron)
    {
        MazeNeutronGeometry ng = maze.Neutron!;
        double phiA = NeutronFluencePhiA(ng, neutron);
        double tvd = mode.NominalMv <= 15 ? 3.9 : 5.4; // NCRP §2.4.2.1
        double hphi = KCapture * phiA * Math.Pow(10.0, -ng.D2 / tvd);
        double hcg = wl.LeakageGyPerWeek * hphi;
        List<string> notes = new List<string>(wl.Notes)
        {
            "TVD = " + Fmt(tvd) + " m (" + (mode.NominalMv <= 15 ? "15 MV" : "18–25 MV") + ", NCRP 151 §2.4.2.1).",
            neutron.Citation,
        };
        List<CalculationStep> steps = new List<CalculationStep>
        {
            PhiAStep(ng, neutron, phiA),
            new CalculationStep(
                "Capture gamma dose per isocenter gray (Eq 2.15)",
                "hφ = K*φA*10^(-d2/TVD)",
                "hφ = " + Fmt(KCapture) + "*" + Fmt(phiA) + "*10^(-" + Fmt(ng.D2) + "/" + Fmt(tvd) + ")",
                new CalculationTerm[]
                {
                    new CalculationTerm("K", KCapture, "Sv·m²/neutron", "NCRP 151 Eq 2.15"),
                    new CalculationTerm("φA", phiA, "n/m²/Gy", "Eq 2.16"),
                    new CalculationTerm("d2", ng.D2, "m", "layout"),
                    new CalculationTerm("TVD", tvd, "m", "NCRP 151 §2.4.2.1"),
                },
                new CalculationTerm("hφ", hphi, "Sv/Gy", "NCRP 151 Eq 2.15")),
            new CalculationStep(
                "Weekly capture gamma dose (Eq 2.17)",
                "Hcg = WL*hφ",
                "Hcg = " + Fmt(wl.LeakageGyPerWeek) + "*" + Fmt(hphi),
                new CalculationTerm[]
                {
                    new CalculationTerm("WL", wl.LeakageGyPerWeek, "Gy/wk", wl.Citation),
                    new CalculationTerm("hφ", hphi, "Sv/Gy", "Eq 2.15"),
                },
                new CalculationTerm("Hcg", hcg, "Sv/wk", "NCRP 151 Eq 2.17")),
        };
        DoorComponentResult result = new DoorComponentResult(
            DoorComponentKind.CaptureGammaHcg, mode.Name, mode.NominalMv, hcg, steps, notes);
        return (result, hcg);
    }

    // Hn: neutron dose, Wu–McGinley (Eq 2.19–2.21)
    private (DoorComponentResult Result, double Dose) BuildHn(
        MazeRun maze, BeamMode mode, WorkloadValue wl, NeutronSource neutron)
    {
        MazeNeutronGeometry ng = maze.Neutron!;
        double phiA = NeutronFluencePhiA(ng, neutron);
        double tvd = 2.06 * Math.Sqrt(ng.S1);    // Eq 2.20
        double ratio = ng.S0 / ng.S1;
        double hnD = WuMcGinleyConst * phiA * ratio *
                     (WuExp1Const * Math.Pow(10.0, -ng.D2 / WuTvd1) + Math.Pow(10.0, -ng.D2 / tvd));  // Eq 2.19
        double hn = wl.LeakageGyPerWeek * hnD;   // Eq 2.21
        List<string> notes = new List<string>(wl.Notes)
        {
            "Wu–McGinley method (2003), NCRP 151 Eq 2.19. Kersey cross-check omitted: " +
            "H0 not tabulated for selected neutron reference unit (" + neutron.Citation + ").",
        };
        List<CalculationStep> steps = new List<CalculationStep>
        {
            PhiAStep(ng, neutron, phiA),
            new CalculationStep(
                "Maze TVD for neutrons (Eq 2.20)",
                "TVD = 2.06*√S1",
                "TVD = 2.06*√" + Fmt(ng.S1),
                new CalculationTerm[] { new CalculationTerm("S1", ng.S1, "m²", "layout") },
                new CalculationTerm("TVD", tvd, "m", "NCRP 151 Eq 2.20")),
            new CalculationStep(
                "Neutron dose per isocenter gray (Eq 2.19, Wu–McGinley)",
                "Hn,D = 2.4e-15*φA*(S0/S1)*(1.64*10^(-d2/1.9) + 10^(-d2/TVD))",
                "Hn,D = 2.4e-15*" + Fmt(phiA) + "*(" + Fmt(ng.S0) + "/" + Fmt(ng.S1) + ")*" +
                    "(1.64*10^(-" + Fmt(ng.D2) + "/1.9) + 10^(-" + Fmt(ng.D2) + "/" + Fmt(tvd) + "))",
                new CalculationTerm[]
                {
                    new CalculationTerm("φA", phiA, "n/m²/Gy", "Eq 2.16"),
                    new CalculationTerm("S0", ng.S0, "m²", "layout"),
                    new CalculationTerm("S1", ng.S1, "m²", "layout"),
                    new CalculationTerm("d2", ng.D2, "m", "layout"),
                    new CalculationTerm("TVD", tvd, "m", "Eq 2.20"),
                },
                new CalculationTerm("Hn,D", hnD, "Sv/Gy", "NCRP 151 Eq 2.19")),
            new CalculationStep(
                "Weekly neutron dose (Eq 2.21)",
                "Hn = WL*Hn,D",
                "Hn = " + Fmt(wl.LeakageGyPerWeek) + "*" + Fmt(hnD),
                new CalculationTerm[]
                {
                    new CalculationTerm("WL", wl.LeakageGyPerWeek, "Gy/wk", wl.Citation),
                    new CalculationTerm("Hn,D", hnD, "Sv/Gy", "Eq 2.19"),
                },
                new CalculationTerm("Hn", hn, "Sv/wk", "NCRP 151 Eq 2.21")),
        };
        DoorComponentResult result = new DoorComponentResult(
            DoorComponentKind.NeutronHn, mode.Name, mode.NominalMv, hn, steps, notes);
        return (result, hn);
    }

    // φA shared between Hcg and Hn (Eq 2.16)
    private static double NeutronFluencePhiA(MazeNeutronGeometry ng, NeutronSource neutron)
    {
        double direct = neutron.Beta * neutron.QnPerGray / (4.0 * Math.PI * ng.D1 * ng.D1);
        double scattered = 5.4 * neutron.Beta * neutron.QnPerGray / (2.0 * Math.PI * ng.RoomSurfaceAreaM2);
        double thermal = 1.3 * neutron.QnPerGray / (2.0 * Math.PI * ng.RoomSurfaceAreaM2);
        return direct + scattered + thermal;
    }

    private static CalculationStep PhiAStep(MazeNeutronGeometry ng, NeutronSource neutron, double phiA)
    {
        return new CalculationStep(
            "Neutron fluence at inner maze point (Eq 2.16)",
            "φA = β*Qn/(4π*d1²) + 5.4*β*Qn/(2π*Sr) + 1.3*Qn/(2π*Sr)",
            "φA = " + Fmt(neutron.Beta) + "*" + Fmt(neutron.QnPerGray) +
                "/(4π*" + Fmt(ng.D1) + "²) + 5.4*" + Fmt(neutron.Beta) + "*" + Fmt(neutron.QnPerGray) +
                "/(2π*" + Fmt(ng.RoomSurfaceAreaM2) + ") + 1.3*" + Fmt(neutron.QnPerGray) +
                "/(2π*" + Fmt(ng.RoomSurfaceAreaM2) + ")",
            new CalculationTerm[]
            {
                new CalculationTerm("β", neutron.Beta, "", "head shielding material"),
                new CalculationTerm("Qn", neutron.QnPerGray, "n/Gy", neutron.Citation),
                new CalculationTerm("d1", ng.D1, "m", "layout"),
                new CalculationTerm("Sr", ng.RoomSurfaceAreaM2, "m²", "layout"),
            },
            new CalculationTerm("φA", phiA, "n/m²/Gy", "NCRP 151 Eq 2.16"));
    }

    // Door sizing: lead for capture gamma, BPE for neutrons
    private static DoorShielding SizeDoor(double bareDose, double p,
        double hCgTotal, double hNTotal)
    {
        List<string> notes = new List<string>
        {
            "Lead TVL = 6.1 cm for 3.6 MeV capture gamma (NCRP 151 §2.4.3).",
            "BPE TVL = 4.5 cm for neutrons (NCRP 151 §2.4.3).",
        };
        double nLead = hCgTotal > 0 && p > 0 ? Math.Max(0, Math.Log10(hCgTotal / p)) : 0;
        double nBpe = hNTotal > 0 && p > 0 ? Math.Max(0, Math.Log10(hNTotal / p)) : 0;
        double leadMm = nLead * LeadTvlCm * 10.0;
        double bpeMm = nBpe * BpeTvlCm * 10.0;
        List<CalculationStep> steps = new List<CalculationStep>
        {
            new CalculationStep(
                "Lead thickness for capture gamma",
                "t_Pb = log10(Hcg/P) * TVL_Pb",
                "t_Pb = log10(" + Fmt(hCgTotal) + "/" + Fmt(p) + ") * " + Fmt(LeadTvlCm),
                new CalculationTerm[]
                {
                    new CalculationTerm("Hcg", hCgTotal, "Sv/wk", "above"),
                    new CalculationTerm("P", p, "Sv/wk", "standard"),
                    new CalculationTerm("TVL_Pb", LeadTvlCm, "cm", "NCRP 151 §2.4.3"),
                },
                new CalculationTerm("t_Pb", leadMm, "mm", "NCRP 151 §2.4.3")),
            new CalculationStep(
                "BPE thickness for neutrons",
                "t_BPE = log10(Hn/P) * TVL_BPE",
                "t_BPE = log10(" + Fmt(hNTotal) + "/" + Fmt(p) + ") * " + Fmt(BpeTvlCm),
                new CalculationTerm[]
                {
                    new CalculationTerm("Hn", hNTotal, "Sv/wk", "above"),
                    new CalculationTerm("P", p, "Sv/wk", "standard"),
                    new CalculationTerm("TVL_BPE", BpeTvlCm, "cm", "NCRP 151 §2.4.3"),
                },
                new CalculationTerm("t_BPE", bpeMm, "mm", "NCRP 151 §2.4.3")),
        };
        return new DoorShielding(leadMm, bpeMm, steps, notes);
    }

    private static bool IsNeutronEnergy(int mv, ShieldingStandard standard)
    {
        if (standard is AerbStandard)
        {
            return mv >= 10;
        }

        return mv > 10;  // NCRP: strictly >10 MV
    }

    private static string Fmt(double value)
    {
        if (value == 0.0) { return "0"; }
        double abs = Math.Abs(value);
        if (abs < 1e-3 || abs >= 1e5)
        {
            return value.ToString("0.###e+00", CultureInfo.InvariantCulture);
        }

        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
