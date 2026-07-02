# 05 — Chuvadi Interface

Confirmed from **Aran's own usage** (public aran repo), never the Chuvadi repo (rule).
Line numbers cite the confirming file in `src/Aran.Extraction`.

## Namespaces Aran consumes
- `Chuvadi.Pdf.Documents` — `PdfDocument` (ExtractionContext.cs:4,34).
- `Chuvadi.Pdf.Rendering.DisplayList` — `PageDisplayList`, `LineSegment`, `TextRun`,
  `PdfPageExtensions`, `LineSegmentExtraction` (ExtractionContext.cs:5; LoadStage.cs:2).

## Entry points (how Aran loads a page) — LoadStage.cs
- `PdfPageExtensions.BuildDisplayList(PdfDocument document, int pageIndex)` → `PageDisplayList` (:19)
- `PdfPageExtensions.GetTextRuns(PdfDocument document, int pageIndex)` → `IReadOnlyList<TextRun>` (:21)
- `LineSegmentExtraction.ExtractLineSegments(PageDisplayList, double tolerance)` → `IReadOnlyList<LineSegment>` (:22)
- `LineSegmentExtraction.DefaultFlattenTolerance` (double) (:22)
- `PageDisplayList.Count` (int) (:26)

## Member surface actually used (confirmed by usage)
- **`TextRun`** (class/record): `.Unicode` (string?) (ScaleCalibrationStage.cs:44; SourceDetectionStage.cs:172);
  `.BoundingBox` with `.X`, `.Y`, `.Width`, `.Height` (ScaleCalibrationStage.cs:117-118;
  DimensionAssociationStage.cs:106-107).
- **`LineSegment`** (struct — used as `LineSegment?` with `.Value`, ScaleCalibrationStage.cs:49,55):
  page-space `.X0 .Y0 .X1 .Y1` (ScaleCalibrationStage.cs:123-124); raw-space
  `.RawX0 .RawY0 .RawX1 .RawY1` (ScaleCalibrationStage.cs:140-141; SourceDetectionStage.cs:257-258).
- **`PageDisplayList`**: `.Count`. (Deeper op-walking members not yet needed; consult the
  package XML docs in `localpackages/` if a future stage must walk operators.)

## Not yet needed / to confirm only if required
- Op-level walking of `PageDisplayList` (individual `Do`/`S`/`f` operators), page `/Rotate`,
  and optional-content layer names as surfaced by Chuvadi. If a new stage needs these, confirm
  the exact members from `localpackages/` (DLL + XML docs) — **not** the Chuvadi repo. *(O05 residual)*

## Status
Interface for the existing pipeline: **CONFIRMED** (no longer BLOCKED). Anything beyond the
members above is confirmed from the package before use.
