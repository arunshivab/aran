using System.Collections.Generic;

namespace Aran.Engines.Linac;

/// <summary>The radiation component at the maze door.</summary>
public enum DoorComponentKind
{
    /// <summary>Primary beam scattered from Wall G (Eq 2.9).</summary>
    PrimaryScatterHs,

    /// <summary>Head-leakage scattered from Wall G (Eq 2.10).</summary>
    LeakageScatterHls,

    /// <summary>Patient-scattered radiation (Eq 2.11).</summary>
    PatientScatterHps,

    /// <summary>Leakage transmitted through inner maze wall (Eq 2.12).</summary>
    LeakageTransmissionHlt,

    /// <summary>Neutron capture gamma rays (Eq 2.15–2.17).</summary>
    CaptureGammaHcg,

    /// <summary>Direct neutrons at door (Eq 2.19–2.21, Wu–McGinley).</summary>
    NeutronHn,
}

/// <summary>
/// One component of the dose at the maze door for one beam mode.
/// </summary>
/// <param name="Kind">The radiation component.</param>
/// <param name="BeamModeName">The beam mode evaluated.</param>
/// <param name="EnergyMv">Nominal energy used for table lookups (MV).</param>
/// <param name="DoseSvPerWeek">The weekly dose equivalent (Sv week^-1).</param>
/// <param name="Steps">Calculation steps with formula and substituted trace.</param>
/// <param name="Notes">Caveats (energy substitutions, omitted terms, etc.).</param>
public sealed record DoorComponentResult(
    DoorComponentKind Kind,
    string BeamModeName,
    int EnergyMv,
    double DoseSvPerWeek,
    IReadOnlyList<CalculationStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>The sandwich door shielding required when the bare dose exceeds the design goal.</summary>
/// <param name="LeadMm">Lead thickness required (mm).</param>
/// <param name="BpeMm">Borated polyethylene thickness required (mm).</param>
/// <param name="Steps">Sizing calculation steps.</param>
/// <param name="Notes">Caveats.</param>
public sealed record DoorShielding(
    double LeadMm,
    double BpeMm,
    IReadOnlyList<CalculationStep> Steps,
    IReadOnlyList<string> Notes);

/// <summary>The door evaluation for one standard.</summary>
/// <param name="DoorId">The door identifier.</param>
/// <param name="StandardName">The standard applied.</param>
/// <param name="BareDoseSvPerWeek">Total weekly dose at the door with no shielding.</param>
/// <param name="DesignGoalSvPerWeek">The applicable design goal.</param>
/// <param name="BarePasses">Whether the bare dose is within the design goal.</param>
/// <param name="Shielding">The sandwich shielding sized when bare dose exceeds the goal; null when BarePasses is true.</param>
/// <param name="Components">All component results contributing to the total.</param>
public sealed record DoorEvaluation(
    string DoorId,
    string StandardName,
    double BareDoseSvPerWeek,
    double DesignGoalSvPerWeek,
    bool BarePasses,
    DoorShielding? Shielding,
    IReadOnlyList<DoorComponentResult> Components);
