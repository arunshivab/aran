# Aran — Roadmap to Full Automation

Goal: feed any AutoCAD radiotherapy plan PDF (+ section PDF) and get the shielding report
with **no manual value entry**. The math is already general; the gap is *feature identification*.

## Phase 0 — Today (human-confirmed extraction)
- Raster render + scale-from-dimension + manual/visual identification of ISO, walls, maze,
  corners. All geometry math and the door/maze physics engine work end-to-end.
- **Status:** geometry for this vault fully extracted and verified; door/maze dose computed.

## Phase 1 — Parse the PDF as vector + text (remove the raster guesswork)
- AutoCAD PDFs carry true line geometry and dimension **text with positions**.
- Extract: line segments, polylines, hatches, text runs (value + x,y), layers if present.
- Outcome: scale and every printed dimension obtained exactly, no px rounding.

## Phase 2 — Semantic identification (the real automation)
- **ISO**: text "ISO CENTER" + the beam-axis crosshair; the dashed beam-envelope lines
  converge here. Anchor everything to it.
- **Primary walls**: thickest barriers on the beam axis; beam-envelope dashed lines strike them.
- **Maze**: the labelled "MAZE" corridor; centreline from its two long faces.
- **Inner maze corners** (maze-side for b, vault-side for A): the wall-end corners flanking
  the throat; pick by side.
- **Door**: leaf-arc symbol + "DOOR" text at the maze end.
- Each identification emits a candidate the engine can render for one-tap human confirmation
  (keeps a safety check while removing the manual measurement).

## Phase 3 — Section drawing auto-read
- From the section PDF: maze height, vault height at ISO (sum of ISO→ceiling + ISO→floor),
  ceiling/floor slab thicknesses. Match the section's "MAZE" / "ISO CENTER" labels.

## Phase 4 — Engine wiring (Aran.App Canvas)
- Extraction stage outputs the full value set (distances, areas, S0/S1, S_r, F) → the
  existing `MazeRun` / engine input. Canvas overlays every value on the plan for verification.
- Machine catalog supplies Q_n/H0/β (TrueBeam→2300CD). Physicist sets only W, W_L, U_G, P.

## Phase 5 — Report generation
- Door/maze + walls computed; report (.docx) emitted with full traces, adequacy table,
  door sizing. Matches the eLORA format already established.

## Definition of done
- Drop in plan PDF + section PDF → values auto-extracted, shown on canvas for one-tap confirm,
  physicist enters only site/workload constants → full AERB/eLORA report out. No manual geometry.
