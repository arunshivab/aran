namespace Aran.Model;

/// <summary>
/// The mapping from raw drawing units to real-world millimetres, together with
/// the confidence and provenance of that mapping.
/// </summary>
/// <param name="MillimetresPerUnit">Real-world millimetres per raw drawing unit.</param>
/// <param name="Confidence">A value in the range 0..1 indicating extraction confidence.</param>
/// <param name="CrossChecked">Whether the scale agreed across two independent dimensions.</param>
/// <param name="Provenance">How the calibration was derived.</param>
public sealed record ScaleCalibration(
    double MillimetresPerUnit,
    double Confidence,
    bool CrossChecked,
    Provenance Provenance);
