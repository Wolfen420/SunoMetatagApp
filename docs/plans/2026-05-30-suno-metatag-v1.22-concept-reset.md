# Implementation Plan — SunoMetatagApp v1.22 — Concept Reset (B-SUNO-016)

**Date:** 2026-05-30
**Spec:** `docs/specs/2026-05-30-suno-metatag-v1.22-concept-reset.md`
**Cycle predecessor:** v1.21 closeout `APPROVED (PASS)` 2026-05-30
**Lead r1 verdict:** `APPROVED (PASS-WITH-NOTES)` 2026-05-30 — one required absorption (precise egress wording) applied at T1

---

## Task sequence

### T1 — Combined docs commit (single-commit cycle per Q6 ratification)

Files created:
- `docs/concept-reset-2026-05-30.md` — BACKLOG-acceptance four-section packet
- `.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md` — NEW durable decision page (purple semantic — decisions folder)
- `docs/specs/2026-05-30-suno-metatag-v1.22-concept-reset.md` — spec doc
- `docs/plans/2026-05-30-suno-metatag-v1.22-concept-reset.md` — this plan doc

Files rewritten:
- `README.md` — closes long-standing working-tree carry-over with new "Local-only positioning" + "What this is NOT" sections + updated feature list + revised roadmap

Absorption #1 (precise egress wording) applied throughout all four artifacts: "no app-initiated API or network fetch" / "clipboard egress (primary)" / "local file I/O" / "user-initiated attribution hyperlink launch (potential)".

### T2 — Not used

Single-commit cycle. No test fixtures (no source changes). No code changes.

### T3 — Build + tests verification

`dotnet test tests/SunoMetatagApp.Tests` → expect **216/216** green (unchanged).

### T4 — Dev smoke

`timeout 8 dotnet run --project src/SunoMetatagApp --no-build` → expect `EXIT=124`.

### T5 — Publish + smoke

`dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish` → expect **byte-identical** artifacts to v1.21 (no source changes).
`timeout 8 ./publish/SunoMetatagApp.exe` → expect `EXIT=124`.

### T6 — USER REVIEW S1-S4

Per plan §6.3 — docs-only cycle has fewer scenarios than feature cycles:
- S1: concept-reset doc accuracy
- S2: README rewrite accuracy
- S3: wiki decision page durability
- S4: multi-cycle regression-gates intact (app still launches and v1.21 behaviors work)

### T7 — Wiki feature-log update

- `.SunoSongSetup-wiki/wiki/features/sunometatag-app.md` — v1.21 → v1.22 title bump + `updated/last_confirmed=2026-05-30` + new `## v1.21 → v1.22 (2026-05-30)` subsection covering: B-SUNO-016 concept-reset background, three deliverables, Measured audit findings, Lead absorption #1 resolution, multi-cycle regression-gates intact (no source changes), B-SUNO-016 closure, validation summary (216/216 + smokes + USER REVIEW 4/4 first try expected + byte-identical streak continues).

### T8 — Archive + RESULT + push

- `.SunoSongSetup-wiki/wiki/reference/ai-plan-archive.md` — Archive entry 47 (v1.22 r1 plan packet) prepended
- `ai/EXECUTION_LOG.md` — v1.22 entry appended
- `ai/PLAN.md` — replaced with v1.22 RESULT packet
- `git push origin master` — incremental push per new T8 step
- USER ACTION NEEDED surfaced for Lead closeout routing
