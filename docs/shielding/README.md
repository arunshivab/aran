# Aran

**Aran** (அரண் — "rampart, fortified wall") reads a vector AutoCAD facility-layout
PDF, analyses the geometry, runs radiation-shielding physics, and prepares an
AERB-compliant shielding report. It targets radiotherapy and nuclear-medicine
installations (LINAC, HDR brachytherapy, CT simulator and PET/gamma).

Aran is built on the Chuvadi PDF library and runs as a local-first .NET 10
application. Each analysis is a self-contained project; nothing is trusted until
a physicist confirms it on the verification canvas.

## Status

Extraction-stage skeleton. The pipeline loads a layout, classifies CAD layers,
calibrates scale and reconstructs barriers; room, material, dimension and source
stages are stubbed. Every extracted element carries a provenance and is marked
unconfirmed until reviewed.

## Projects

- `src/Aran.Model` - host-agnostic output contract (no Chuvadi dependency).
- `src/Aran.Extraction` - the nine-stage extraction pipeline (Chuvadi 3.15.0).
- `tests/Aran.Extraction.Tests` - unit tests (xUnit + FluentAssertions).
- `examples/Aran.ExtractionHarness` - console runner over a real layout PDF.

## Pipeline stages

1. Load - build display list, extract line segments and text runs. (implemented)
2. Layer classification - bucket segments and texts by layer role. (implemented)
3. Scale calibration - derive mm-per-unit from dimension numbers. (implemented, first cut)
4. Wall reconstruction - pair parallel wall strokes into barriers. (implemented, first cut)
5. Room detection - close walls into rooms. (stub)
6. Material classification - assign material and density. (stub)
7. Dimension association - bind dimension values to spans. (stub)
8. Source detection - find isocentres and beam axes. (stub)
9. Assemble - finalise the draft model. (implemented)

## Prerequisites

- .NET SDK 10.0.301 (pinned in `global.json`).
- Chuvadi 3.15.0 packages. Flat-copy the `.nupkg` files into `localpackages/`
  (the `chuvadi-local` feed in `nuget.config`). Never `dotnet nuget push`.

## Build, test, run

```
dotnet build Aran.slnx -c Release
dotnet test  tests/Aran.Extraction.Tests/Aran.Extraction.Tests.csproj -c Release
dotnet run   --project examples/Aran.ExtractionHarness -c Release -- <layout.pdf> 0 <outputDir>
```

## Licence

Proprietary. See `LICENSE.txt`.


# Aran — Shielding Documentation

Documentation for the TrueBeam vault shielding extraction + calculation. Suggested repo
location: `docs/shielding/` in `arunshivab/aran`.

## Contents
- **docs/01-extraction-methodology.md** — how values are derived from any AutoCAD plan PDF
  (general method; what's automated vs. human-confirmed today).
- **docs/02-extracted-values.md** — every distance, area, and value with derivation + pixel
  cross-checks (the canonical value reference).
- **docs/03-decisions-log.md** — finalized decisions from both prior chats (machine proxy,
  neutron method, design goals, geometry).
- **docs/04-roadmap-automation.md** — staged plan to remove the manual step and reach a
  fully automated PDF→report pipeline.
- **Door_Maze_Shielding_Calculation.md** — the computed maze-door dose using the precise
  extracted geometry.

## Headline
Precise extraction matters: the correct maze length (d2 = 11.84 m) gives a maze-door dose of
~5.3 µSv/wk vs the prior rough estimate of 115 µSv/wk. This is the whole case for automating
the extraction rather than typing approximate values.