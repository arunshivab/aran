# 04 — Orientation Independence

## Problem
Client PDFs arrive in any orientation: rotated sheets, maze left-to-right or right-to-left,
mirrored layouts. Nothing can key off screen up/down/left/right. *(D02)*

## Strategy: anchor to a local frame at the source
1. **Find the source** (ISO) and the **gantry** (doc 02, source-detection stage).
2. Build a **local frame** from ISO→gantry. All geometry is expressed in this frame, so
   rotation/mirroring of the sheet does not change any distance, area, or barrier role.
3. **Patient-relative Left/Right** comes from the frame under Head-First-Supine, not the
   screen. *(D05)*
4. Primary vs secondary on the lateral walls is by geometry (the thick beam-hit band is
   primary; the thinner lateral portions are secondary), independent of orientation.

## Gantry detection (mandatory input) *(D20)*
The gantry is mandatory in every PDF. Detect it with a priority cascade:
1. **Gantry block/symbol + primary-beam envelope together** — the best, primary signal.
2. **"GANTRY" text label** — rare, used if present.
3. **Fallback — primary beam alone:** assign the two sides as Left/Right from the beam, then
   on the verification canvas prompt **"Do you want to switch?"** (one-tap flip). This keeps a
   human in the loop when the block/text is absent (candidate + confirm, D03).

## Left/Right frame rule *(D21)*
- **Gantry side = the patient's HEAD** (HFS). The opposite side = the patient's feet. So the
  vector **ISO → gantry is the head direction.**
- **Patient-left = 90° counter-clockwise (CCW) from the ISO→gantry direction, in plan view.**
- The **Left wall** is the wall toward the patient's left; the **Right wall** toward the right.

### Worked example (state it this way in the report)
Gantry at the **top** of the plan ⇒ head points up ⇒ rotating the head-direction 90° CCW
points to the **plan-LEFT** ⇒ the **Left Primary Wall is the lateral wall on the plan-left**,
and the Right Primary Wall is on the plan-right. If the layout is mirrored or the gantry is at
the bottom/side, the same CCW rule re-derives L/R correctly with no special case — and the
canvas "switch?" affordance is the final human check.

## Why this is robust
- Distances/areas are frame-invariant (measured between identified features).
- Maze direction falls out of the maze centreline relative to the frame — "runs left" vs
  "runs right" is just a sign in the local frame, not a special case.

## Status
Frame and L/R rule: **DECIDED** (D20, D21). Detection implementation lands in the renamed
source-detection stage (D24), consuming the gantry per the cascade above.
