# 08 — Delivery Protocol

Restates the Project-Instructions delivery rule so it lives with the project. *(D17)*

## Full files only
Every code/doc deliverable is a COMPLETE file (UTF-8 no-BOM, LF). Never snippets, diffs,
patches, or anchored-replace. (Rule 9.)

## The cycle for every change
1. Claude delivers complete files (zip) to the output area.
2. Arun downloads to the Downloads folder.
3. Claude gives PowerShell to: extract → copy into `C:\Users\aruns\Documents\aran` → delete
   any stale files → clean `bin`/`obj` → build → run.
4. Arun runs them and does the visual verification (his eyes are ground truth).
5. Claude gives git commands (feature branch → PR → CI: style + docs-up-to-date + build matrix → merge).
6. Claude gives cleanup commands.

## Guards
- Claude never assumes a delivered file was already copied (Rule 2).
- Before editing an existing repo file, Claude must have its current bytes from Arun, byte-
  compares the intended result, and shows the diff (Rule 9).
- New files (no version on main) are additions and need no prior upload; if a path might
  already exist, ask first.
