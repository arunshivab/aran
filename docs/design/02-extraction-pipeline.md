# 02 — Extraction Pipeline

Confirmed from the uploaded `src/Aran.Extraction` (rule 1). Line numbers cite the file.

## Contract
- `IExtractionStage` (IExtractionStage.cs): `string Name {get;}`, `void Execute(ExtractionContext)`.
- `ExtractionContext` (ExtractionContext.cs): `Document` (PdfDocument), `PageIndex`, `LayerMap`,
  `DisplayList` (PageDisplayList?), `Segments` (IReadOnlyList<LineSegment>), `Texts`
  (IReadOnlyList<TextRun>), `Classification` (LayerClassification?), `Scale` (ScaleCalibration?),
  `WallLoops`, `RoomPolygons`, **`MazeGeometry` (ExtractedMazeGeometry?)** (:73), `Model`
  (ModelBuilder), `Diagnostics`, `Report(severity, stage, message)`.
- `ExtractionResult(ShieldingGeometryModel Model, IReadOnlyList<Diagnostic> Diagnostics)`.
- `ExtractionPipeline.CreateDefault()` wires nine stages in order (ExtractionPipeline.cs:24);
  `Run` isolates per-stage failures as diagnostics (:43).

## The nine stages — TRUE current state
| # | Class | What its name says | What it ACTUALLY does | State |
|---|---|---|---|---|
| 1 | LoadStage | Load | Load (segments, texts, display list) | OK |
| 2 | LayerClassificationStage | Layer classification | classify segments/texts | first cut |
| 3 | ScaleCalibrationStage | Scale calibration | scale from dimension text, 2-dim consensus, printed value authoritative (ScaleCalibrationStage.cs:64,73) | OK (good) |
| 4 | WallReconstructionStage | Wall reconstruction | wall loops via WallGraph | first cut |
| 5 | RoomDetectionStage | Room detection | room polygons | first cut |
| 6 | MaterialClassificationStage | Material classification | material/density | first cut |
| 7 | **DimensionAssociationStage** | "Dimension association" | **Room labelling** (PiP of label text → RoomFunction) — does NOT associate dimensions (DimensionAssociationStage.cs:9) | misnamed |
| 8 | **SourceDetectionStage** | "Source detection" | **Maze tracer** — ISO/vault/maze/doors → ExtractedMazeGeometry (SourceDetectionStage.cs:9) | defective (see below) |
| 9 | AssembleStage | Assemble | build ShieldingGeometryModel | OK |

## Defects found in stage 8 (the "absurd report" root cause, in code)
- **`vaultHeightMm = 3000.0; // typical ceiling height`** (SourceDetectionStage.cs:147) — height is
  **hardcoded**; A1, Az, S1 derive from it (:150-152). No section PDF is read anywhere.
- **`a0M2 = 0.5;`** (:149) — A0 hardcoded ("typical"), not measured.
- `dr`, `dzz` are proxies `dh + mazeWidth/2` (:124-134), not the reflection-ray construction.
- ISO detection = text run containing `"ISO"` (:172) — not robust to arbitrary client labelling.
- `dsca = 1000.0` (:137) — NCRP convention, acceptable.

## What this forces (recorded as decisions/opens)
- **Section parsing is required** and must feed the height (D18) — replaces line 147.
- The tracer's hardcoded A0 and proxy dr/dzz must become drawing-derived (D19).
- ISO/frame detection must be orientation-independent (doc 04, O02/O03).
- Stage naming tech-debt (7 = room labelling, 8 = maze tracer) — rename decision (O09).
- `ExtractedMazeGeometry` field set vs physics needs S0/d1/d2 (O10).

## Construction rules (target, from shielding/01)
px→mm via scale; primary-wall projection = field × (target→face / target→ISO); first-reflection
point b from the grazing ray on the inner maze corner ∩ maze centreline; location A (neutron)
from the ISO ray grazing the vault-side inner-maze corner ∩ centreline; S_r on the internal
envelope (needs height ⇒ section).
