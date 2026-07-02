# 07 — Report & Canvas

## Verification canvas
- Overlay **every extracted value** (distances, areas, S0/S1, S_r, F, heights) on the actual
  plan render, plus the candidate features (ISO, primary band, maze corners, door).
- One-tap physicist **confirm/correct** per feature before any dose is computed. *(D03)*
- The canvas is the safety net that lets automation proceed without silent geometry errors.

## Report (eLORA `.docx`, D15)
- Established format: cover, facility table, methodology + formulae, per-barrier worked pages
  (formula → substitution → n → t → recommended vs proposed), maze-door section (photon
  components + neutron), adequacy summary, undertaking + signature block.
- Two workload bases (AERB vs institutional) differ only in W. *(D14)*
- Nine patient-relative barriers, minimum. *(D05)*
- Every value carries its provenance; estimated/standard inputs are flagged for review.

## OPEN
- **O06** — `.docx` generation in .NET without a new NuGet dependency: via **Chuvadi**
  (emit a PDF instead) or an in-repo docx writer. Decide before implementing the report.
- Canvas rendering surface (Aran.App UI tech) — confirm alongside doc 01 app project.

## Status
Format: **DECIDED** (docx/eLORA). Generation mechanism: **OPEN** (O06).
