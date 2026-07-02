# 01 — Architecture

## Projects (target)
| Project | Role | Depends on |
|---|---|---|
| `src/Aran.Model` | Host-agnostic output contract (geometry, barriers, sources, dimensions, provenance). | nothing (no Chuvadi) |
| `src/Aran.Extraction` | Nine-stage pipeline: PDF → vectors/text → semantic features → geometry result. | Chuvadi 3.15.0, Aran.Model |
| `src/Aran.Shielding` *(new)* | Physics engine: 9 barriers + maze/door + neutron; NCRP/AERB constants. | Aran.Model only |
| `src/Aran.Report` *(new)* | Verification canvas + eLORA `.docx` report generation. | Aran.Model, Aran.Shielding |
| `src/Aran.App` *(new)* | Orchestration + UI: upload plan+section → confirm canvas → report. | all of the above |
| `tests/*`, `examples/Aran.ExtractionHarness` | Tests + console runner. | — |

*(D16: the three new projects are additions; they must not touch or duplicate existing
`Aran.Model` / `Aran.Extraction` until that source is seen — rule 9.)*

## Data flow
```
plan.pdf + section.pdf
      │  (Chuvadi: line segments, text runs + positions, layers, page rotation)
      ▼
Aran.Extraction  ── 9 stages ──▶  Aran.Model.GeometryResult
      │                               (distances, areas, S0/S1, S_r, F, heights,
      │                                each with provenance = drawn/computed + unconfirmed)
      ▼
Verification canvas ── one-tap physicist confirm ──▶ confirmed GeometryResult
      │
      ▼
Aran.Shielding  ◀── NCRP/AERB constants + physicist site inputs (W, W_L, U_G, P)
      │  ShieldingResult (9 barriers + door photon + neutron)
      ▼
Aran.Report ──▶ canvas figure + eLORA .docx
```

## Layering rules
- `Aran.Model` is pure contracts — no Chuvadi, no physics, no UI. Portable.
- `Aran.Shielding` depends on `Aran.Model` **only** — it takes a `GeometryResult` and
  constants, and knows nothing about PDFs. (This is why the physics can be validated in
  isolation.)
- Only `Aran.Extraction` touches Chuvadi.
- `Aran.App` is the only project that wires everything and holds UI.

## OPEN
- Exact new-project names/boundaries (`Aran.Shielding` vs folding into an existing project) —
  confirm once `Aran.Model` is seen. *(O04)*
