# Aran — Documentation

This folder is the single source of truth for Aran's purpose, architecture, and
decisions. It exists so design is settled and recorded *before* code is written —
not reconstructed turn-by-turn.

## Layout

| Path | What it holds |
|---|---|
| `decisions.md` | One running log of every design/architecture decision: what, why, status. |
| `design/00-purpose-scope.md` | What Aran is, the universal-extraction mandate, non-goals, definition-of-done. |
| `design/01-architecture.md` | Projects, layering, and the PDF → extraction → model → engine → report data flow. |
| `design/02-extraction-pipeline.md` | The nine extraction stages: per-stage input, output, responsibility, status. |
| `design/03-data-model.md` | `Aran.Model` contract requirements (types TBD — see rule-9 note inside). |
| `design/04-orientation-independence.md` | Anchor-to-ISO frame; rotation/mirror; patient-relative Left/Right. |
| `design/05-chuvadi-interface.md` | What extraction consumes from Chuvadi (API TBD — see rule-1 note inside). |
| `design/06-physics-engine.md` | Nine-barrier + maze/door + neutron method and NCRP/AERB constants. |
| `design/07-report-and-canvas.md` | Verification canvas (one-tap confirm) and the .docx/eLORA report. |
| `design/08-delivery-protocol.md` | Full-files-only delivery cycle (extract/copy/build/run/git/cleanup). |
| `shielding/` | Domain reference (the extraction methodology, extracted values, prior decisions, roadmap), verbatim. |

## The systematic process (how we work here)

1. Walk the design topics in order, `00 → 07`.
2. For each topic: discuss in chat → resolve the **OPEN** items → record the outcome in
   `decisions.md` **and** fill the matching `design/` doc.
3. **No code is written for an area until its design doc is agreed.**
4. Traceability: every implementation ← a design doc ← a numbered decision in `decisions.md`.

## Status legend used throughout

- **DECIDED** — settled; implement to this.
- **OPEN** — needs a decision before the dependent code can be written.
- **FLAGGED** — decided but carries a risk/assumption a physicist must verify.
- **BLOCKED** — cannot be written without source we haven't seen (Aran.Model / Chuvadi API).
