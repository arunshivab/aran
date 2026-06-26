namespace Aran.Engines.Linac;

using Aran.Model;

/// <summary>
/// Photon-maze geometry quantities required for NCRP 151 Equations 2.9–2.12.
/// All distances in metres, areas in m².
/// </summary>
/// <param name="Dh">Target to first scattering surface (Wall G), perpendicular (m).</param>
/// <param name="Dr">First-reflection centre past inner maze edge to maze centreline point b (m).</param>
/// <param name="Dz">Centreline distance from point b to the door (m).</param>
/// <param name="Dsec">Target to maze centreline at Wall G (m); also used as patient-to-Wall-G distance.</param>
/// <param name="Dzz">Centreline distance along maze from scattering surface A1 to door (m).</param>
/// <param name="Dsca">Target to patient (m).</param>
/// <param name="DL">Target to maze door centre through inner maze wall (oblique, m).</param>
/// <param name="A0">Beam area at first scattering surface (m²).</param>
/// <param name="A1">Area of Wall G visible from the maze door (m²).</param>
/// <param name="Az">Cross-sectional area of maze inner entry projected onto the maze wall (m²).</param>
/// <param name="InnerWallTransmissionB">Transmission factor B for leakage through inner maze wall (Wall Z).</param>
/// <param name="Alpha0ReflectionDeg">Angle of reflection off the first surface A0 (degrees).</param>
/// <param name="AlphaZReflectionDeg">Angle of reflection off the second maze surface Az (degrees).</param>
/// <param name="Alpha1ReflectionDeg">Angle of reflection off Wall G for leakage/scatter (degrees).</param>
public sealed record MazePhotonGeometry(
    double Dh,
    double Dr,
    double Dz,
    double Dsec,
    double Dzz,
    double Dsca,
    double DL,
    double A0,
    double A1,
    double Az,
    double InnerWallTransmissionB,
    double Alpha0ReflectionDeg,
    double AlphaZReflectionDeg,
    double Alpha1ReflectionDeg);

/// <summary>
/// Neutron-maze geometry quantities for NCRP 151 Equations 2.16–2.21.
/// </summary>
/// <param name="D1">Isocenter to inner maze point A (m).</param>
/// <param name="D2">Inner maze point A to door (centreline, m).</param>
/// <param name="RoomSurfaceAreaM2">Total surface area of the treatment room (m²).</param>
/// <param name="S0">Cross-sectional area of the inner maze opening (m²).</param>
/// <param name="S1">Cross-sectional area along the maze body (m²).</param>
public sealed record MazeNeutronGeometry(
    double D1,
    double D2,
    double RoomSurfaceAreaM2,
    double S0,
    double S1);

/// <summary>
/// The confirmed maze run and door location, consumed by the door shielding engine.
/// Quantities are read from the physicist-confirmed plan and section drawings.
/// </summary>
/// <param name="DoorId">Identifier for the door, matching the geometry model.</param>
/// <param name="ProtectedClass">Area class of the space outside the door.</param>
/// <param name="Occupancy">Occupancy category of the space outside the door.</param>
/// <param name="UseFactorG">Use factor for beam directed at Wall G.</param>
/// <param name="PatientTransmissionF">
/// Fraction of primary beam transmitted through the patient (~0.25 for 6–10 MV).
/// </param>
/// <param name="ScatterAngleDegrees">Patient scatter angle toward the maze (degrees).</param>
/// <param name="FieldAreaCm2">Field area at mid-depth of patient at 1 m (cm²).</param>
/// <param name="Photon">Photon-maze geometry (required for all energies).</param>
/// <param name="Neutron">Neutron geometry; null for ≤10 MV under NCRP, required for ≥10 MV under AERB or >10 MV under NCRP.</param>
public sealed record MazeRun(
    string DoorId,
    AreaClass ProtectedClass,
    OccupancyCategory Occupancy,
    double UseFactorG,
    double PatientTransmissionF,
    double ScatterAngleDegrees,
    double FieldAreaCm2,
    MazePhotonGeometry Photon,
    MazeNeutronGeometry? Neutron);
