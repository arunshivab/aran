# Aran — Geometry Extraction Methodology (general, file-agnostic)

This describes how every shielding value is derived from an AutoCAD-exported plan PDF.
Nothing here is hardcoded to one drawing — each number is measured from the drawing's
own scale and its own printed dimensions.

## Pipeline
1. **Render** the plan PDF page to a raster (currently `pdftoppm -r 150`). Capture page
   rotation (this sheet is `/Rotate 270`).
2. **Scale** — derive px/mm from any labelled dimension on the sheet (here 0.1136 px/mm
   at 150 dpi; 1 px = 8.803 mm). Method is general; the value re-derives per drawing.
3. **Feature identification** — locate ISO, primary walls, maze walls, corners, door.
   *(Currently human-confirmed; see roadmap for automation.)*
4. **Measure** — distance = hypot(Δx,Δy) px ÷ (px/mm) ÷ 1000; areas from width×height;
   geometry constructions (sight lines, projections) are plain analytic geometry.
5. **Cross-check** every pixel measurement against the drawing's printed dimension; the
   printed value is authoritative when they differ (auto-detection is unreliable near
   dimension lines, stipple, chamfers).

## Construction rules used
- **px → mm**: divide pixel distance by the scale.
- **Projection to a wall face**: field size × (distance_target→face / distance_target→ISO).
- **First-reflection / point b**: ray from reflection point grazing the inner maze corner,
  intersected with the maze centreline.
- **Location A (neutron)**: ray from ISO grazing the **vault-side** corner of the inner maze
  wall, intersected with the maze centreline.
- **Room surface area S_r**: 2(L·W + L·H + W·H) on the internal envelope.

## What is general vs. what still needs a human
- **General (file-agnostic):** render, scale-from-dimension, px→mm, all geometry math,
  area/surface-area, sight-line/projection constructions.
- **Not yet automated:** *feature identification* (which point is ISO, which line is the
  primary wall, which corner to graze). Today this is confirmed visually. The drawing's
  vector geometry + dimension text are inside the PDF and can be parsed directly — that is
  the path to removing the human step (see roadmap).
