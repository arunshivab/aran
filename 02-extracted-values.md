# TrueBeam Vault — NCRP 151 Maze-Door Shielding Values

**Plan:** `6_10_15MV_6FFF_10FFF_TRUEBEAM_WITH_FFF_SVC_L_A_ROOM.pdf` (TrueBeam vault, right hall)
**Section:** `SECTION_-_B_-_B__(Length).pdf` (maze height only)
**Status:** all distances and areas below are confirmed and drawn on `ALL_VALUES.png`.

---

## 1. Common basis (how the plan was read)

| Item | Value | How obtained |
|---|---|---|
| Source PDF page | 1001 × 1417 pt, `/Rotate 270` | `pdfinfo` |
| Raster used | `plan-1.png`, 2953 × 2086 px | `pdftoppm -png -r 150` |
| Scale | **0.1136 px/mm** at 150 dpi | from a labelled plan dimension; 1 px = 8.803 mm |
| Distance rule | `m = hypot(Δx, Δy) px ÷ 0.1136 ÷ 1000` | applied to every distance |

**Confirmed reference points (render pixels):**

| Point | Pixel | Meaning |
|---|---|---|
| target (T) | (1244, 709) | X-ray source, 1 m west of ISO along beam axis (SAD) |
| ISO | (1358, 709) | isocentre |
| inner face / R / mid A0 | (1767, 709) | central-axis strike on East primary wall (Side-B); 3583 mm from ISO |
| outer face | (2046, 709) | inner face + wall thickness 2457 mm |
| inner maze corner | (1649, 1199) | graze corner at maze mouth |
| b | (1614, 1344) | in-air point on maze centreline (y = 1344) |
| A1 wall | x = 1881, y = 1123 → 1438 | vault-end cap face |
| mid A_1 | (1881, 1281) | midpoint of the A_1 segment |
| L corner | (1881, 1438) | outer maze wall ∩ A1 wall |
| door | (822, 1658) | maze door centre |
| left maze wall | x ≈ 703 | far chamfer wall |

---

## 2. NCRP central-beam-axis distances (all on y = 709)

All four lie on the central beam axis; the labelled values are the NCRP-derived figures.

### d_sca = 1.000 m — target → ISO (SAD)
- Source-to-isocentre distance (1 m from ISO; varies per machine). Drawn T → ISO.

### d_pp = 3.583 m — ISO → primary wall inner face
- Perpendicular ISO → Side-B inner face. Matches the printed plan dimension **3583 mm** (ISO → East wall). Drawn ISO → inner face (R). Pixel check: 409 px → 3.60 m.

### d_h = 4.583 m — target → first reflection surface
- Perpendicular ISO-to-wall distance **plus 1 m**: 1 + d_pp = 1 + 3.583. Drawn T → inner face. Pixel check: 523 px → 4.60 m.

### d_pri = 7.040 m — target → primary wall outer face
- 1 (target→ISO) + d_pp (3.583) + wall thickness (2.457) = 7.040. Drawn T → outer face. Pixel check: 802 px → 7.06 m.

---

## 3. Maze / door distances (confirmed)

### d_L = 9.59 m — ISO → door (leakage)
- Straight line ISO (1358,709) → door (822,1658) = hypot(536, 949) = 1090 px → 9.59 m.

### d_sec = 6.82 m — ISO → middle of A_1 (secondary)
- ISO (1358,709) → mid A_1 (1881,1281) = hypot(523, 572) = 775 px → 6.82 m.
- Corrected from the earlier 6.03 m (which was ISO→b); endpoint is now the **middle of A_1**.

### d_r = 5.75 m — R → b (grazing the inner maze edge)
- R (1767,709) → b (1614,1344) along the line grazing the inner maze corner (1649,1199) = 653 px → 5.75 m. b is in air on the maze centreline.

### d_z = 9.74 m — b → door (maze centreline)
- Polyline b → (822,1344) → door (822,1658): 792 + 314 = 1106 px → 9.74 m.

### d_zz = 10.34 m — maze length (wall-to-wall)
- Far-left chamfer wall (x≈703) → A1 face (x≈1881) = 1178 px → 10.34 m; matches the printed "10340" dimension.

---

## 4. Areas

Maze height **5.39 m** (§5) is used for the two maze cross-sections.

### A_0 = 3.36 m² — beam area on the primary barrier
- 40×40 cm field at ISO projected to the inner face: side = 0.40 × 4.583 / 1.0 = 1.833 m → 1.833² ≈ 3.36 m².
- (Beam diagonal projection 40√2 = 56.57 cm at ISO, projected to outer face ×7.040 = 3.982 m → max primary-barrier width needed.)

### A_1 = 14.9 m² — vault-end (maze-end) wall area
- A_1 segment on the A1 wall (x=1881): one end at the **L corner** (1881,1438), the other where the **end-to-end maze diagonal** meets the A1 wall (1881,1123). Length ≈ 2.77 m. Area = 2.77 × 5.39 = **14.9 m²**.

### A_z = 8.35 m² — maze cross-section (perpendicular to A_1)
- From mid A_0 (1767,709): one edge at (1591,1438) (line to inner maze corner extended to outer wall), the other (1767,1438) (perpendicular dropped from mid A_0). Length 1.55 m. Area = 1.55 × 5.39 = **8.35 m²**. A_1 and A_z meet at the L corner.

---

## 5. Maze height (from Section B-B)
- "5390" vertical dimension on Section B-B; "2560" maze-width confirms the matching cross-section (plan width 2557 mm). Maze height = **5.39 m**.

---

## 6. First reflection point
- On Primary Side-B (East wall), **3583 mm from ISO** = (1767, 709). (Earlier south-wall placement corrected to Side-B.)

---

## 7. Primary-barrier width check (confirmed)

Worst-case projected field, compared at the **inner face** (where the primary block sits).

- Max projected field = 40×40 field rotated 45° → diagonal = 40√2 = **56.57 cm** at ISO.
- Magnified to the inner face: × (d_h / d_sca) = 4.583 / 1.0 = 4.583 → **2.593 m**.
- + 0.30 m each side → **required width = 3.193 m**.
- **Actual primary block width = 3.686 m** (1843 + 1843 mm; chain 1740+1843+1843+2048 = 7474 = plan overall dim).
- **Result: 3.686 > 3.193 → adequate, ~0.25 m spare each side.**
- (Outer-face version, for reference: ×7.040 = 3.982 m projected, +0.60 = 4.582 m. Not the governing comparison since the block is at the inner face.)
- Figure: `primary_wall_width.png`.

---

## 8. Pending values — not yet determined

Mapped to the NCRP 151 maze-door equations (photon Eq 2.9–2.14; neutron/high-energy Eq 2.15–2.18). Required because this is a **15 MV** machine, so neutron + capture-gamma terms apply.

### 8a. Reflection & scatter coefficients (NCRP App B tables)
- **α0** — reflection coeff at first scattering surface A0 (energy + 45° incidence). Table B.8.
- **αz** — reflection coeff for 2nd reflection off maze surface Az (≈0.5 MeV). Table B.8.
- **α1** — reflection coeff off Wall G: 1.4 MeV (6 MV) / 1.5 MeV (10 MV) for leakage scatter; 0.5 MeV for patient scatter. Table B.8a.
- **a(θ)** — patient scatter fraction at angle θ. Table B.4.

### 8b. Workload / use / design goal
- **W** — primary workload (Gy/week).
- **W_L** — leakage workload (Gy/week).
- **U_G** — use factor for Wall G (gantry orientation).
- **P** — shielding design goal outside the door (the limit we solve to).
- **f** — primary fraction through patient ≈ 0.25 for 6–10 MV (40×40 field, 40³ phantom).

### 8c. Other photon inputs
- **F** — field area at mid-depth of patient at 1 m (cm²) — for H_ps (Eq 2.11).
- **B** — transmission factor of inner maze Wall Z along the oblique d_L path — for H_LT (Eq 2.12). Needs Wall-Z thickness + photon TVL.
- **L_f** = 1×10⁻³ (known constant, leakage ratio).
- Door **photon TVL** at 0.2 MeV broad-beam — for door thickness.

### 8d. Neutron + capture-gamma inputs (15 MV)
- **Location A** — point on the maze centerline where the isocenter is just visible (must be marked on the plan).
- **d1** — ISO → Location A (Eq 2.16, 2.18).
- **d2** — Location A → door (Eq 2.15, 2.18).
- **d_E (your term) — distance to the back wall.** Likely maps to part of the neutron path (d2 or a back-wall leg); confirm exact definition.
- **S0** — inner maze entrance cross-sectional area (Kersey, Eq 2.18).
- **S1** — cross-sectional area along the maze (Kersey, Eq 2.18). (S0/S1 ratio is what the formula uses.)
- **S_r** — total surface area of the treatment room (m²) (Eq 2.16) — computable from the plan + heights.
- **Q_n** — neutron source strength, n per Gy at iso, for 15 MV TrueBeam. Table B.9.
- **β** — neutron head-transmission factor (1 for Pb, 0.85 for W head shielding).
- **H0** — total neutron dose-equiv at d0 = 1.41 m (Table B.9).
- **d0** = 1.41 m (known); **K** = 6.9×10⁻¹⁶ Sv·m² (known); **TVD** ≈ 3.9 m for 15 MV (known).
- Modified Kersey (NCRP §2.4.2.2.2) parameters, if that variant is used instead.

### 8e. Door design outputs (after the above)
- **H_S, H_LS, H_ps, H_LT** → **H_G** (Eq 2.13) → **H_Tot** (Eq 2.14).
- **H_cg** (Eq 2.17) and **H_n,D** (Eq 2.18) for the neutron path.
- Required **door transmission** = P / H_Tot, then door **thickness** (photon 0.2 MeV; capture-gamma 3.6 MeV; photoneutron BPE/borated layer).

---

## 9. Neutron / maze plan-derived values (determined)

All read off the plan + Section B-B (the Table values in §8a/§8d remain).

| Quantity | Value | Source / construction |
|---|---|---|
| F (field area, mid-depth at 1 m) | 1600 cm² | 40×40, fixed by TrueBeam model (F/400 = 4 in Eq 2.11) |
| Location A | (1853, 1344) px | ISO sight line grazing the **vault-side** corner of the inner maze wall at (1649, 1082) → maze centerline |
| d1 (ISO → A) | 7.09 m | straight line, hypot(495, 635) px |
| d2 (A → door) | 11.84 m | along maze: 9.08 m corridor + 2.76 m down to door |
| S1 (cross-section along maze) | 13.78 m² | corridor width 2.557 m × height 5.39 m |
| S0 (inner maze entrance, throat) | 11.04 m² | throat width 2.048 m (1649→1881) × height 5.39 m |
| S0 / S1 | 0.80 | for Kersey Eq 2.18 |
| Vault internal height (at ISO) | 3.453 m | Section B-B: ISO→ceiling 2138 + ISO→floor 1315 |
| Vault envelope (E-W × N-S) | 9.214 × 6.866 m | secondary-wall internal; E-W narrows to 7.166 at primary barriers |
| **S_r (total room surface area)** | **237.6 m²** | box of secondary envelope: 2(LW + LH + WH), L=9.214, W=6.866, H=3.453 |

**Knowns (constants):** d0 = 1.41 m · K = 6.9×10⁻¹⁶ Sv·m² · TVD ≈ 3.9 m (15 MV) · L_f = 1×10⁻³ · f ≈ 0.25.

### Machine / neutron table values (FINALIZED — prior decision)
- **TrueBeam 15X → Varian 2300CD @ 18 MV** (conservative proxy): **Q_n = 0.95×10¹² n/Gy** (Followill et al. 2003, Table B.9).
- **H0 not tabulated** for 2300CD → **Kersey cross-check omitted** (noted for later); **Wu–McGinley (Eq 2.19, TVD = 2.06·√S1) is the reported neutron value**.
- **β = 1.0** (conservative). **f = 0.25**.
- **10 MV mode → Varian 1800 @ 10 MV**: Q_n = 0.06×10¹², H0 = 0.04 mSv/Gy. Rule: **10 MV and above include the neutron treatment** (AERB ≥10 threshold).
- Door materials: lead TVL **6.1 cm** (3.6 MeV capture gamma); BPE TVL **4.5 cm** (neutrons); lead/BPE/lead sandwich.
- **Patient scatter a(θ): ignored above 10 MV** (H_ps drops out for 15X).

### Still pending — site/design inputs (physicist-set)
- W, W_L, U_G, P (design goal at door); B (Wall-Z transmission along d_L).
- Photon reflection coefficients α0, αz, α1 (only if photon-maze components are retained for ≤10 MV modes) — pick from B.8a/B.8b once the reflection angles are taken from the geometry.
