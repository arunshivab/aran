# Aran — Decisions Log

One running log. Newer decisions may supersede older ones (noted explicitly).
Status: DECIDED · OPEN · FLAGGED · BLOCKED.

| # | Area | Decision | Why | Status |
|---|---|---|---|---|
| D01 | Mandate | Aran extracts every geometric value **from the drawing's vectors + dimension text**, never from memory or a hardcoded seed. Manual entry is not the primary path. | A seeded/typed app is just a spreadsheet; the product is automated extraction. | DECIDED |
| D02 | Input PDFs | Client-generated AutoCAD PDFs are **uncontrolled** — no guaranteed layers, dimension-entity type, orientation, or naming. Assume nothing; degrade to geometry + human-confirm. | Clients export however they like. | DECIDED |
| D03 | Confirm model | Extraction is **candidate + confirm**. Every model element already carries `Provenance` + `IsConfirmed`. | Safety check while removing manual measurement. | DECIDED |
| D04 | Section input | A **section PDF is always supplied** (same cut through Ceiling-Primary + maze; direction fixed, name may vary). Heights come from it. | Plan view has no height; removes maze-height guessing. | DECIDED |
| D05 | Barrier naming | Patient-relative **Left/Right** (HFS), never compass. Report must contain the 9 barriers. | Orientation-independent physicist convention. | DECIDED |
| D06 | Photon-maze | **Option B:** compute H_S/H_LS/H_ps/H_LT→H_G→H_Tot for every energy; add neutron+capture-gamma when max ≥ 10 MV. **H_ps kept for all energies.** | Works for any machine (incl. 6 MV-only); safe-side. | DECIDED (supersedes "a(θ) ignored >10 MV" in shielding/03) |
| D07 | Coefficients | α0/αz/α1/a(θ) from **NCRP standard tables**, not per-drawing angles. | Portable across drawings. | DECIDED |
| D08 | Neutron | 2300CD @18 MV proxy; Q_n 0.95×10¹²; Wu–McGinley (TVD=2.06√S1); β 1.0, f 0.25; Kersey omitted. | shielding/03. | DECIDED |
| D09 | Neutron threshold | ≥ 10 MV triggers neutron; 10 MV → 1800 @10 MV (Q_n 0.06×10¹², H0 0.04). | AERB. | DECIDED |
| D10 | Door materials | Pb TVL 6.1 cm (capture γ); BPE TVL 4.5 cm (neutron). | shielding/02 §9. | DECIDED |
| D11 | Goals/occupancy | P 20/400 µSv/wk; walls T=1, door 1/8, console 1 (NCRP B.1). | AERB + NCRP B.1. | DECIDED |
| D12 | Concrete TVLs | Primary B.2 (15 MV 44/41, 6 MV 37/33); leakage B.7 (36/33, 34/29); scatter B.5a; 2.35 g/cc. | NCRP 151. | DECIDED |
| D13 | Scale | mm/unit from any labelled dimension; printed value authoritative. Existing ScaleCalibrationStage already does 2-dimension consensus. | Auto-detect unreliable near dim lines. | DECIDED |
| D14 | Workload | AERB 1.0e5/0.5e5; institutional 300×80×5=1.2e5 split 2:1. **W_L=1500 cGy/wk drives result linearly — verify.** | Two bases; W_L uncertain. | FLAGGED |
| D15 | Report format | Deliverable is the **.docx/eLORA** report. The standalone HTML report + hardcoded-geometry "app" zip were the wrong artifact, discarded. | eLORA needs docx; seed app contradicted D01. | DECIDED |
| D16 | Repo integration | The physics engine (`Aran.Engines.Linac`), report (`Aran.Report.Linac`), machines (`Aran.Machines`) and app (`Aran.App`) **already exist** on main. Round 1 **extends** them (section-height extraction, gantry L/R, stage renames) and does not duplicate them; new work is additive. | Reconciled to the real multi-modality repo, not a 2-project snapshot. | DECIDED (supersedes the earlier "new projects" wording) |
| D17 | Delivery | **Full files only** (UTF-8 no-BOM/LF); the 6-step cycle in `design/08`. Aran repo `C:\Users\aruns\Documents\aran`; downloads via Downloads. | Restates rule 9 + fixes the workflow. | DECIDED |
| D18 | Section→height | Height MUST come from parsing the section PDF, replacing `vaultHeightMm = 3000.0` (SourceDetectionStage.cs:147). A section-parse stage/input is required. | Root cause of the "absurd report"; D04. | DECIDED |
| D19 | Drawing-derived maze | Replace hardcoded `a0M2 = 0.5` (SourceDetectionStage.cs:149) and the `dr`/`dzz` proxies (:124-134) with values derived from the drawing (reflection-ray construction, shielding/01). | D01 — from the drawing. | DECIDED |
| D20 | Gantry detection | Gantry is **mandatory** in every PDF. Detect via **block + primary-beam envelope** (primary signal), **"GANTRY" text** (rare, secondary), else **primary-beam alone** → assign the two sides and prompt **"Do you want to switch?"** on the canvas. | Robust across uncontrolled client PDFs (D02); candidate+confirm (D03). | DECIDED |
| D21 | Left/Right frame | **Gantry side = patient head** (HFS); opposite = feet. **Patient-left = 90° counter-clockwise (CCW) from the ISO→gantry direction, in plan view.** Head-up example: gantry at top ⇒ Left wall on the plan-LEFT. Left wall = wall toward patient-left. | Orientation-independent L/R (D05); resolves O03. | DECIDED |
| D22 | Heights home | New **`SectionGeometry`** record we own (maze height, ISO→ceiling, ISO→floor, slab thicknesses), populated by a section-parse step. Replaces `vaultHeightMm = 3000.0` (SourceDetectionStage.cs:147). | O08(a); D04/D18. | DECIDED |
| D23 | Shielding geometry | **DROPPED.** `MazeRun` / `MazePhotonGeometry` / `MazeNeutronGeometry` (`Aran.Engines.Linac/DoorInputs.cs`) already carry the full physics input set (S0, S1, d1, d2, room surface, areas, transmission). No new `ShieldingGeometry` record — the existing `MazeRun` is used. | Would have duplicated `MazeRun`. | DROPPED |
| D24 | Stage rename | Rename stage 7 `DimensionAssociationStage` → room-labelling; stage 8 `SourceDetectionStage` → maze-tracer / source detection. Touches `ExtractionPipeline` wiring + tests. | Names must match behaviour (O09). | DECIDED |
| D25 | Multi-vault | Sheets may contain more than one vault. v1 processes **one physicist-selected vault** (candidate isocentres surfaced when >1). Full **multi-vault reporting** (both bunkers in one run) is deferred to **v2**. | Keeps gantry/L-R unambiguous in v1. | DECIDED (v2 deferral) |

## OPEN items (to close as we walk 00 → 07)

| # | Area | Question |
|---|---|---|
| O01 | 02/05 | Dimension encoding in uncontrolled client PDFs — associative dimension entities vs plain text-near-line. Existing ScaleCalibrationStage uses text-near-segment; confirm this holds across clients. |
| O06 | 07 | `.docx` generation without a new NuGet dependency — via Chuvadi (PDF) or an in-repo writer. |

*Closed: O02/O03 → D20/D21 (gantry + CCW L/R); O07 → D21; O08 → D22; O09 → D24; O10 → D23; O04 → design/03; O05 → design/05.*

**Remaining OPEN:** O01 (dimension encoding across clients), O06 (.docx generation mechanism).
