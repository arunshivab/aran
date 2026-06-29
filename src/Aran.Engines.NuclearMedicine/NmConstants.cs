namespace Aran.Engines.NuclearMedicine;

/// <summary>
/// Physical and regulatory constants for nuclear medicine shielding calculations.
/// All values are taken from the AERB Technical Guidance for Nuclear Medicine
/// Facilities and AAPM TG-108 (Madsen et al., Med. Phys. 33(1), 2006).
/// </summary>
public static class NmConstants
{
    // -----------------------------------------------------------------------
    // PET / F-18  (AAPM TG-108 Table II and §Factors Affecting Dose Rates)
    // -----------------------------------------------------------------------

    /// <summary>
    /// F-18 effective dose rate constant Γ (µSv·m²·MBq⁻¹·h⁻¹).
    /// Source: AAPM TG-108 Table II / ANSI/ANS-6.1.1-1991.
    /// AERB NM guidance §B.2 adopts this value.
    /// </summary>
    public const double F18DoseRateConstant = 0.143;

    /// <summary>
    /// Patient body attenuation factor for 511 keV annihilation photons (dimensionless).
    /// Effective patient dose rate = Γ × A × 0.36.
    /// Source: AAPM TG-108 §Patient attenuation (mean of direct measurements).
    /// </summary>
    public const double F18PatientAttenuationFactor = 0.36;

    /// <summary>
    /// Effective patient dose rate constant = Γ × attenuation factor
    /// (µSv·m²·MBq⁻¹·h⁻¹).
    /// Source: AAPM TG-108 Eq 2 coefficient.
    /// </summary>
    public const double F18EffectiveDoseRate = F18DoseRateConstant * F18PatientAttenuationFactor; // 0.05148

    /// <summary>F-18 physical half-life (minutes).</summary>
    public const double F18HalfLifeMin = 109.8;

    /// <summary>
    /// AERB default administered activity for PET (MBq).
    /// Source: AERB NM guidance §B.2 (370 MBq per patient).
    /// </summary>
    public const double AerbPetAdministeredActivityMbq = 370.0;

    /// <summary>
    /// AERB default uptake time for PET (minutes).
    /// Source: AERB NM guidance §B.2 (45 minutes).
    /// </summary>
    public const double AerbPetUptakeTimeMin = 45.0;

    /// <summary>
    /// AERB default imaging time for PET (minutes).
    /// Source: AERB NM guidance §B.2 (30 minutes).
    /// </summary>
    public const double AerbPetImagingTimeMin = 30.0;

    /// <summary>
    /// Bladder voiding factor applied to imaging room calculation:
    /// approximately 15% of administered activity is excreted before imaging.
    /// Source: AAPM TG-108 §Imaging Room Calculation (factor 0.85).
    /// </summary>
    public const double VoidingFactor = 0.85;

    /// <summary>
    /// AERB maximum patients per week per PET unit.
    /// Source: AERB NM guidance §A.7.
    /// </summary>
    public const int AerbPetMaxPatientsPerWeek = 120;

    // -----------------------------------------------------------------------
    // Broad-beam 511 keV Archer model fitting parameters (Table V, TG-108)
    // -----------------------------------------------------------------------

    /// <summary>Archer model α for lead at 511 keV (cm⁻¹). Source: AAPM TG-108 Table V.</summary>
    public const double ArcherAlphaLead = 1.543;

    /// <summary>Archer model β for lead at 511 keV (cm⁻¹). Source: AAPM TG-108 Table V.</summary>
    public const double ArcherBetaLead = -0.4408;

    /// <summary>Archer model γ for lead at 511 keV. Source: AAPM TG-108 Table V.</summary>
    public const double ArcherGammaLead = 2.136;

    /// <summary>Archer model α for concrete (2.35 g/cm³) at 511 keV (cm⁻¹). Source: AAPM TG-108 Table V.</summary>
    public const double ArcherAlphaConcrete = 0.1539;

    /// <summary>Archer model β for concrete at 511 keV (cm⁻¹). Source: AAPM TG-108 Table V.</summary>
    public const double ArcherBetaConcrete = -0.1161;

    /// <summary>Archer model γ for concrete at 511 keV. Source: AAPM TG-108 Table V.</summary>
    public const double ArcherGammaConcrete = 2.0752;

    // -----------------------------------------------------------------------
    // Gamma Camera / SPECT  (AERB NM guidance §B.1)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tc-99m specific gamma-ray constant Γ (mR·h⁻¹·mCi⁻¹ at 1 m).
    /// Source: AERB NM guidance §B.1.2.
    /// </summary>
    public const double Tc99mGammaConstantMrHMci = 0.078;

    /// <summary>
    /// Tc-99m Γ converted to SI units (µSv·m²·MBq⁻¹·h⁻¹).
    /// Conversion: 0.078 mR·h⁻¹·mCi⁻¹ × (1 mSv/100 mR) × (37 MBq/mCi) × 1000 µSv/mSv = 0.02886
    /// </summary>
    public const double Tc99mDoseRateConstant = Tc99mGammaConstantMrHMci * 37.0 / 100.0;

    /// <summary>
    /// AERB adequate shielding for Gamma Camera: 23 cm brick (1.65 g/cm³).
    /// Source: AERB NM guidance §B.1.3.
    /// </summary>
    public const double AerbGcBrickMm = 230.0;

    /// <summary>
    /// AERB adequate shielding for Gamma Camera: 15 cm concrete (2.35 g/cm³).
    /// Source: AERB NM guidance §B.1.3.
    /// </summary>
    public const double AerbGcConcreteMm = 150.0;

    // -----------------------------------------------------------------------
    // HDT / I-131  (AERB NM guidance §B.3)
    // -----------------------------------------------------------------------

    /// <summary>
    /// I-131 specific gamma-ray constant (mR·h⁻¹·mCi⁻¹ at 1 m).
    /// Source: AERB NM guidance §B.3.1.
    /// </summary>
    public const double I131GammaConstantMrHMci = 0.22;

    /// <summary>
    /// I-131 Γ in SI units (µSv·m²·MBq⁻¹·h⁻¹).
    /// Conversion: 0.22 × 37 / 100 = 0.08140
    /// </summary>
    public const double I131DoseRateConstant = I131GammaConstantMrHMci * 37.0 / 100.0;

    /// <summary>
    /// Maximum activity handled per week for HDT I-131 (mCi).
    /// Source: AERB NM guidance §B.3.1 (300 mCi).
    /// </summary>
    public const double AerbHdtMaxActivityMci = 300.0;

    /// <summary>Maximum activity in MBq (300 mCi × 37 = 11100 MBq).</summary>
    public const double AerbHdtMaxActivityMbq = AerbHdtMaxActivityMci * 37.0;

    /// <summary>
    /// TVL for I-131 in concrete (cm).
    /// Source: AERB NM guidance §B.3.2.
    /// </summary>
    public const double I131TvlConcreteCm = 10.0;

    /// <summary>
    /// TVL for I-131 in lead (cm).
    /// Source: AERB NM guidance §B.3.2.
    /// </summary>
    public const double I131TvlLeadCm = 1.0;

    // -----------------------------------------------------------------------
    // AERB design goals (common to all NM modalities)
    // -----------------------------------------------------------------------

    /// <summary>AERB design goal for radiation workers (Sv/wk). Source: AERB NM guidance §A.3.</summary>
    public const double AerbControlledSvPerWk = 4.0e-4;

    /// <summary>AERB design goal for public (Sv/wk). Source: AERB NM guidance §A.3.</summary>
    public const double AerbUncontrolledSvPerWk = 2.0e-5;

    /// <summary>AERB occupancy factor T = 1 for all areas. Source: AERB NM guidance §A.5.</summary>
    public const double AerbOccupancyFactor = 1.0;
}
