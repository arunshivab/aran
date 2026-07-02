# Aran — Decisions Log (both chats)

## Project
- Facility: Varian **TrueBeam SVC** (6/10/15 MV + 6FFF/10FFF), Index Medical College, Indore.
- Basement (Lower Ground), no beam stopper, right-angle maze. eLORA submission.
- References: AERB Technical Guidance, NCRP 151 (2005), IAEA SRS-47.

## Supreme rule
The app must **extract values automatically from the plan PDF** — manual entry is not the
primary path (if the physicist types every value, a spreadsheet suffices). Stop and ask at
every geometry fork; produce an image at every step for visual verification.

## Machine / neutron (finalized)
- **TrueBeam 15X → Varian 2300CD @ 18 MV** proxy (conservative): **Q_n = 0.95×10¹² n/Gy**.
- **H0 not tabulated** for 2300CD → **Kersey cross-check omitted**; **Wu–McGinley (Eq 2.19,
  TVD = 2.06·√S1) is the reported neutron value**.
- **β = 1.0** (conservative). **f = 0.25**. **10 MV and above include neutron** (AERB ≥10).
- 10 MV mode → Varian 1800 @ 10 MV: Q_n = 0.06×10¹², H0 = 0.04 mSv/Gy.
- Patient scatter a(θ) ignored above 10 MV.
- Door materials: lead TVL 6.1 cm (3.6 MeV capture gamma), BPE TVL 4.5 cm (neutrons).

## Design goals / workload
- P = **20 µSv/week** (public) / **400 µSv/week** (controlled).
- W_L = **1500 cGy/week** (prior report; **FLAGGED — verify, drives result linearly**).

## Geometry (this session, all visually confirmed by Arun)
- Scale 0.1136 px/mm; ISO (1358,709); primary inner face R (1767,709); wall 2457 mm.
- Distances: d_sca 1.0, d_pp 3.583, d_h 4.583, d_pri 7.040, d_L 9.59, d_sec 6.82,
  d_r 5.75, d_z 9.74, d_zz 10.34, **d1 7.09, d2 11.84**.
- Areas: A_0 3.36, A_1 14.9, A_z 8.35; **S0 11.04, S1 13.78, S0/S1 0.80**; F 1600 cm².
- Maze height 5.39 m; vault height 3.453 m; vault envelope 9.214 × 6.866 m; **S_r 237.6 m²**.
- Location A grazes the **vault-side** corner (1649,1082) → A (1853,1344).
- Primary-barrier width: required 3.193 m (inner face) vs actual 3.686 m → adequate.

## Open / to confirm
- W_L (workload) value. U_G use factor. B (Wall-Z transmission along d_L) if photon door governs.
