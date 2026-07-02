# 06 — Physics Engine

Takes a confirmed `GeometryResult` + NCRP/AERB constants + physicist site inputs, and
produces the 9-barrier + door result. Depends on `Aran.Model` only (doc 01). A validated
prototype exists (numbers reproduced against an independent reference); it must be re-homed
as `Aran.Shielding` consuming the model — not the discarded hardcoded seed. *(D15, D16)*

## Barriers (patient-relative, D05)
Left Primary · Right Primary · Secondary behind machine · Secondary Left · Secondary Right ·
Ceiling Primary · Ceiling Secondary · Control Console · Entrance Door.

## Photon barrier math
- Primary / leakage: `B = P·d² / (W·U·T)`; `n = −log10(B)`; `t = TVL1 + (n−1)·TVLe`.
- Patient scatter: `B = P·d_sca²·d_sec²·(400/F) / (a(θ)·W·T)`.
- Head leakage (secondary): `B = P·d_sec² / (L_f·W·U·T)`.

## Maze-door — Photon (policy B, D06)
Compute for **every energy present**, then sum:
- `H_S`  (primary scatter off room surfaces)
- `H_LS` (leakage scattered)
- `H_ps` (patient scatter — **kept for all energies**, D06)
- `H_LT` (leakage transmitted through the inner maze wall)
→ `H_G` → `H_Tot` (photon).

## Maze-door — Neutron (added when max energy ≥ 10 MV, D08/D09)
- `φ_A = βQ_n/(4πd1²) + 5.4βQ_n/(2πS_r) + 1.3Q_n/(2πS_r)` (Eq 2.16)
- `TVD = 2.06·√S1`; `H_n,D = 2.4×10⁻¹⁵·φ_A·√(S0/S1)·[1.64·10^(−d2/1.9) + 10^(−d2/TVD)]` (Wu–McGinley)
- Capture gamma: `h_φ = K·φ_A·10^(−d2/3.9)`, `K = 6.9×10⁻¹⁶`.
- Door total = photon `H_Tot` + `H_n` + `H_cg`, compared to the worker goal with door
  occupancy T = 1/8.

## Constants (standards — the only non-drawing inputs)
- **Concrete TVL** (D12): primary B.2 (15 MV 44/41, 6 MV 37/33 cm); leakage B.7 (15 MV 36/33,
  6 MV 34/29 cm); patient scatter B.5a. Density 2.35 g/cm³.
- **Coefficients** α0/αz/α1/a(θ) from **NCRP tables** (D07).
- **Neutron** (D08): 2300CD @18 MV proxy, Q_n 0.95×10¹², β 1.0, f 0.25, Wu–McGinley.
- **Door materials** (D10): Pb TVL 6.1 cm, BPE TVL 4.5 cm.
- **Design goals / occupancy** (D11): 20/400 µSv/wk; walls T=1, door 1/8, console 1.

## Physicist site inputs (not from the drawing)
W, W_L (**FLAGGED — verify, drives result linearly**, D14), U_G, P.

## Status
Method: **DECIDED**. Re-homing into `Aran.Shielding` against the real `Aran.Model` is
gated on doc 03 (O04).
