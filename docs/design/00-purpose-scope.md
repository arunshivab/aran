# 00 — Purpose & Scope

## What Aran is
Aran reads a client's AutoCAD radiotherapy vault-layout PDF **plus its section PDF**,
extracts all shielding geometry **from the drawings themselves**, runs the NCRP 151 /
AERB shielding physics, and emits an eLORA-format `.docx` report. It is local-first,
.NET 10, built on the Chuvadi PDF library.

## The mandate (non-negotiable)
- **Universal.** It must accept *any* client layout: any orientation, any rotation, maze
  running left-to-right or right-to-left, any mirroring, any naming. Clients generate these
  PDFs however they like and we have **no control** over them. *(D01, D02)*
- **From the drawing.** Every distance and area is measured from the drawing's vectors and
  its printed dimension text — never from memory, never from a hardcoded per-facility seed.
  The printed dimension is authoritative over pixel measurement. *(D01, D13)*
- **Standards are the only non-drawing inputs.** TVLs, albedos, scatter fractions, Q_n
  proxy, occupancy, design goals come from NCRP 151 / AERB and are fixed constants. *(D06–D13)*
- **Candidate + confirm.** Because client PDFs are uncontrolled, semantic identification
  emits candidates the physicist confirms on a canvas — never assume-and-proceed. *(D03)*

## Inputs
1. Plan PDF (vault layout, top-down).
2. Section PDF (same cut through Ceiling-Primary and the maze; direction fixed, name may
   vary) — supplies all heights. *(D04)*
3. Physicist-set site constants only: workload W, W_L, use factor U_G, design goal P.

## Outputs
1. Verification canvas — every extracted value overlaid on the plan for one-tap confirm.
2. eLORA `.docx` shielding report with the 9 patient-relative barriers, worked steps,
   adequacy table, and door sizing. *(D05, D15)*

## Non-goals
- Not a manual-entry calculator. If the physicist types every value, a spreadsheet suffices.
- Not tied to one facility, one machine, or one drawing convention.
- Does not invent geometry when the drawing is ambiguous — it asks (candidate + confirm).

## Definition of done
Drop in plan PDF + section PDF → geometry auto-extracted → shown on canvas for one-tap
confirmation → physicist enters only site/workload constants → full AERB/eLORA report out,
with **no manual geometry entry**.

## Status
Purpose & scope: **DECIDED**. Open dependencies live in later docs (extraction robustness,
model/Chuvadi contracts).
