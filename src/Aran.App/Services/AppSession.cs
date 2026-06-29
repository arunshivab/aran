using Aran.Engines.Linac;
using Aran.Extraction;
using Aran.Model;

namespace Aran.App.Services;

/// <summary>
/// Scoped service that holds all state flowing through the five-page workflow:
/// Upload → Canvas → InputForm → Run → Download.
/// One instance per browser connection.
/// </summary>
public sealed class AppSession
{
    // ── Step 1: Upload ────────────────────────────────────────────────────────

    /// <summary>Raw bytes of the uploaded plan PDF.</summary>
    public byte[]? PlanPdfBytes { get; set; }

    /// <summary>Original file name of the uploaded plan PDF.</summary>
    public string? PlanFileName { get; set; }

    /// <summary>Raw bytes of the uploaded section PDF (optional).</summary>
    public byte[]? SectionPdfBytes { get; set; }

    /// <summary>The extraction result produced by the pipeline.</summary>
    public ExtractionResult? ExtractionResult { get; set; }

    // ── Step 2: Canvas ────────────────────────────────────────────────────────

    /// <summary>The physicist-confirmed geometry model.</summary>
    public ShieldingGeometryModel? ConfirmedModel { get; set; }

    /// <summary>Whether the canvas confirmation step has been completed.</summary>
    public bool CanvasConfirmed { get; set; }

    // ── Step 3: InputForm ─────────────────────────────────────────────────────

    /// <summary>Facility name.</summary>
    public string FacilityName { get; set; } = string.Empty;

    /// <summary>Facility address.</summary>
    public string FacilityAddress { get; set; } = string.Empty;

    /// <summary>Floor description.</summary>
    public string FacilityFloor { get; set; } = string.Empty;

    /// <summary>Drawing reference.</summary>
    public string DrawingReference { get; set; } = string.Empty;

    /// <summary>Prepared-by physicist name.</summary>
    public string PreparedBy { get; set; } = string.Empty;

    /// <summary>eLORA RP registration number.</summary>
    public string EloraRpId { get; set; } = string.Empty;

    /// <summary>Date of preparation.</summary>
    public DateOnly PreparedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Selected machine model name from MachineCatalog.</summary>
    public string MachineName { get; set; } = string.Empty;

    /// <summary>Workload values per beam-mode name (cGy/wk or mAmin/wk).</summary>
    public Dictionary<string, double> Workloads { get; set; } = new();

    /// <summary>Confirmed maze run input (pre-populated from extraction stage 8 when available).</summary>
    public MazeRunFormValues MazeRun { get; set; } = new();

    /// <summary>Whether the input form has been submitted.</summary>
    public bool InputConfirmed { get; set; }

    // ── Step 4: Run ───────────────────────────────────────────────────────────

    /// <summary>NCRP 151 wall shielding result.</summary>
    public LinacShieldingResult? NcrpWallResult { get; set; }

    /// <summary>AERB wall shielding result.</summary>
    public LinacShieldingResult? AerbWallResult { get; set; }

    /// <summary>NCRP 151 door evaluation.</summary>
    public DoorEvaluation? NcrpDoorResult { get; set; }

    /// <summary>AERB door evaluation.</summary>
    public DoorEvaluation? AerbDoorResult { get; set; }

    /// <summary>Whether the run has completed.</summary>
    public bool RunCompleted { get; set; }

    // ── Step 5: Download ─────────────────────────────────────────────────────

    /// <summary>Generated NCRP 151 report PDF bytes (≤ 2 MB).</summary>
    public byte[]? NcrpReportBytes { get; set; }

    /// <summary>Generated AERB LINAC report PDF bytes (≤ 2 MB).</summary>
    public byte[]? AerbLinacReportBytes { get; set; }

    /// <summary>Generated AERB NM report PDF bytes, or null when NM not applicable.</summary>
    public byte[]? AerbNmReportBytes { get; set; }

    /// <summary>Resets the session to initial state.</summary>
    public void Reset()
    {
        PlanPdfBytes = null;
        PlanFileName = null;
        SectionPdfBytes = null;
        ExtractionResult = null;
        ConfirmedModel = null;
        CanvasConfirmed = false;
        FacilityName = string.Empty;
        FacilityAddress = string.Empty;
        FacilityFloor = string.Empty;
        DrawingReference = string.Empty;
        PreparedBy = string.Empty;
        EloraRpId = string.Empty;
        PreparedDate = DateOnly.FromDateTime(DateTime.Today);
        MachineName = string.Empty;
        Workloads = new Dictionary<string, double>();
        MazeRun = new MazeRunFormValues();
        InputConfirmed = false;
        NcrpWallResult = null;
        AerbWallResult = null;
        NcrpDoorResult = null;
        AerbDoorResult = null;
        RunCompleted = false;
        NcrpReportBytes = null;
        AerbLinacReportBytes = null;
        AerbNmReportBytes = null;
    }
}

/// <summary>Flat form values for maze run geometry, pre-populated from extraction.</summary>
public sealed class MazeRunFormValues
{
    public double DhM { get; set; } = 5.0;
    public double DrM { get; set; } = 3.5;
    public double DzM { get; set; } = 4.0;
    public double DsecM { get; set; } = 5.0;
    public double DzzM { get; set; } = 8.5;
    public double DscaM { get; set; } = 1.0;
    public double DlM { get; set; } = 6.0;
    public double A0M2 { get; set; } = 0.5;
    public double A1M2 { get; set; } = 5.0;
    public double AzM2 { get; set; } = 6.0;
    public double InnerWallTransmissionB { get; set; } = 1e-4;
    public double ScatterAngleDeg { get; set; } = 90.0;
    public double FieldAreaCm2 { get; set; } = 400.0;
    public double PatientTransmissionF { get; set; } = 0.25;
    public bool HasNeutron { get; set; }
    public double D1M { get; set; } = 6.4;
    public double D2M { get; set; } = 8.5;
    public double RoomSurfaceM2 { get; set; } = 236.0;
    public double S0M2 { get; set; } = 9.2;
    public double S1M2 { get; set; } = 8.4;
}
