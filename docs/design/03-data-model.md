# 03 — Data Model (Aran.Model)

Read from the uploaded source (rule 1). Line numbers cite the confirming file.

## Aran.Model — the confirmed contract (namespace `Aran.Model`)
All extracted elements are **immutable records** carrying `Provenance` + `IsConfirmed`
(candidate-and-confirm is built into the model).

- **Enums** (ModelEnums.cs): `Provenance` {FromLayer, FromDimensionText, TextureMatched,
  GeometryInferred, Manual} (:4); `RoomFunction` {Unknown, TreatmentRoom, ControlRoom, Maze,
  Corridor, Office, Toilet, UtilityRoom, PublicArea, AdjacentClinical, UnoccupiedArea} (:23);
  `AreaClass` {Unknown, Controlled, Uncontrolled} (:60); `BarrierMaterial` {Unknown, Concrete,
  Brick, NaturalEarth, Lead, Steel} (:73); `MachineType` {Unknown, LinacTrueBeam, LinacHalcyon,
  LinacTomo, CyberKnife, Telecobalt, GammaKnife, HdrBrachy, LdrBrachyCs137, Simulator,
  CtSimulator, PetCt} (:95).
- **Geometry** (Geometry.cs): `PointMm(double X, double Y)` readonly record struct (:8);
  `LineMm(PointMm A, PointMm B)` (:13); `Polyline(IReadOnlyList<PointMm> Points)` (:17);
  `Polygon(IReadOnlyList<PointMm> Vertices)` (:21). **Millimetre space.**
- **ScaleCalibration** (:11): `(double MillimetresPerUnit, double Confidence, bool CrossChecked, Provenance Provenance)`.
- **RadiationSource** (:16): `(string Id, PointMm Isocentre, IReadOnlyList<LineMm> BeamAxes,
  MachineType Machine, IReadOnlyList<int> EnergiesMv, Provenance, bool IsConfirmed)`.
- **Barrier** (:16): `(string Id, Polyline CentrelineMm, double ThicknessMm, BarrierMaterial Material,
  double? DensityGramsPerCc, string? RoomAId, string? RoomBId, Provenance, bool IsConfirmed)`.
- **Room** (:15): `(string Id, Polygon BoundaryMm, string? Label, RoomFunction SuggestedFunction,
  double? OccupancyT, AreaClass Classification, Provenance, bool IsConfirmed)`.
- **ShieldingGeometryModel** (:15): `(ScaleCalibration? Scale, IReadOnlyList<Room> Rooms,
  IReadOnlyList<Barrier> Barriers, IReadOnlyList<RadiationSource> Sources)` + static `Empty` (:22).

## Aran.Extraction — supporting types the stages use
- `RawPoint(double X, double Y)` readonly record struct (WallGraph.cs:11) — raw drawing units.
- `WallLoop(IReadOnlyList<RawPoint> Points, bool IsClosed)` (WallGraph.cs:18).
- `RoomPolygon(WallLoop Loop, double AreaRawSq, string? Label, RoomFunction Function)` (RoomPolygon.cs:13).
- **`ExtractedMazeGeometry`** (RoomPolygon.cs:40) — the derived maze contract, in mm:
  VaultPolygon, MazePolygon, IsocentreRaw, Inner/OuterDoorCentreRaw, **DhMm, DzMm, DrMm,
  DsecMm, DzzMm, DscaMm, DlMm, A0M2, A1M2, AzM2, MazeCrossM2 (=S1)**, Diagnostics. Comment
  says it is "Converted to `MazeRun` by the UI/orchestration layer" — **`MazeRun` is not
  defined anywhere** (comment-only); that orchestration layer is what our shielding work builds.

## Gaps this model has vs. our physics needs (design decisions required)
1. **No patient-relative Left/Right role on `Barrier`** — only centreline/thickness/material.
   L/R (D05) must be assigned downstream from the source frame (doc 04). *(O07)*
2. **`Room` is 2-D only** (`BoundaryMm` polygon) — **no height**. Heights (maze height, vault
   height = ISO→ceiling + ISO→floor, slab thicknesses) from the section (D04) have **no home**
   in the current model. *(O08)*
3. **`ExtractedMazeGeometry` is missing physics inputs**: it has `MazeCrossM2` (S1) but **no S0
   (throat)**, and **no distinct neutron path d1/d2** (only Dsec/Dl). The physics engine (doc 06)
   needs S0, S1, d1, d2. Reconcile the field set. *(O10)*
4. **`A0M2`, and the maze height, are currently hardcoded in the tracer** (see doc 02) — the
   contract is fine, but its producer fabricates values. *(D18, D19)*

## Design decision
The three new shielding projects consume `Aran.Model` + `ExtractedMazeGeometry` (confirmed).
Any concept missing above is added as an **additive** type (new file), or as extra fields on a
new record we own — **never** by editing the existing model from assumption (rule 9). The exact
choice per gap is O07/O08/O10, decided as we walk 03→06.
