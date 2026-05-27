# SunoMetatagApp v1.14 — Add `[Verse 3]` through `[Verse 6]` Structure Tags — Plan

**Date:** 2026-05-28
**Companion to:** `docs/specs/2026-05-28-suno-metatag-v1.14-verse-cluster-extension.md`
**Workflow packet:** `ai/PLAN.md` (v1.14 r1 plan — Lead `APPROVED (PASS-WITH-NOTES)` 2026-05-28 with 2 LOW absorptions).

## Tasks

| # | Task | Acceptance |
|---|---|---|
| T0 | Wiki context refresh — `[[sunometatag-tag-library]]`, `[[sunometatag-app]]` + Lead r1 absorption text. Pre-flight greps: no `prompts.json` Verse 3-6 references; H1-H6 existing test impact analysis. | Done at planning time. |
| T1 | Primary commit: edit `Resources/tags.json` (4 new entries inserted after `[Verse 2]`), create this plan doc + companion spec doc with absorbed `H7_*` naming. Targeted `git add`. | Single commit `B-SUNO-015 / v1.14 T1: add [Verse 3]-[Verse 6] Structure tags`. |
| T2 | Secondary commit: add `H7_ExtendedVerseCluster_PresentInStructure` `[Theory]` test with 4 `[InlineData]` cases in `TagServiceSunoaiwikiMetatagListTests.cs` (absorption #2 — H7 naming continues H1-H6 series). Optionally update H5 `// 331` comment to `// 335` for accuracy. | One commit `B-SUNO-015 / v1.14 T2: H7 Verse-cluster presence test`. Test count 132 → 136. |
| T3 | Build + test gate. | 136/136 green. |
| T4 | Dev smoke. | `TIMEOUT_EXIT=124`. |
| T5 | Publish rebuild + publish smoke. | `TIMEOUT_EXIT=124`. tags.json size delta ~+260 B; prompts.json byte-identical to v1.13. |
| T6 | USER REVIEW S1-S6. | User-confirmed PASS. |
| T7 | Wiki updates — `[[sunometatag-tag-library]]` FULL refresh of all Verse-cluster/count references (absorption #1: summary line "331 entries" → "335 entries", category-table Structure row count "34" → "38", Pill-ordering Verse-triplet → Verse-cluster of 7 narrative, test-coverage list adds H7, source-paths note count update). `[[sunometatag-app]]` v1.14 subsection. | Pages updated; `last_confirmed=2026-05-28`. |
| T8 | Closeout — Archive entry 31 to plan archive; consolidated EXECUTION_LOG entry; rewrite `ai/PLAN.md` as RESULT. | RESULT packet per template. |

## Lead r1 absorptions (PASS-WITH-NOTES)

1. **T7 wiki refresh — FULL** — not spot-update; refresh all Verse-cluster/count references in `[[sunometatag-tag-library]]` (frontmatter summary, total count, Structure-row count, Pill-ordering subsection narrative, test-coverage list with new H7 entry, source-paths note).
2. **Test naming — H7_ continuation** — use `H7_ExtendedVerseCluster_PresentInStructure` to continue the H1-H6 series naming convention. Plan and spec documents already use H7_ naming; the original ai/PLAN.md r1 packet's V1_ naming will be noted as superseded in the RESULT.

## Pre-flight checks (T0)

- `grep` on `prompts.json` confirms zero references to `[Verse 3]`-`[Verse 6]` — no prompt-corpus impact.
- `grep` on test files confirms only the H5 code comment references "331" (assertion uses uniqueness, not exact count). H1 uses `>= 320` lower bound. H2 doesn't test Structure. H4/H6 unaffected.
- tags.json schema: 4-field flat record matching existing `[Verse 1]` / `[Verse 2]` entries.

## Rollback

Two-commit revert: `git revert <T2-sha> <T1-sha>`. Tests return to 132.
