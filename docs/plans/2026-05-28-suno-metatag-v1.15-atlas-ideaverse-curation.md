# SunoMetatagApp v1.15 — Atlas Ideaverse Metatag Database Curation — Plan

**Date:** 2026-05-28
**Companion to:** `docs/specs/2026-05-28-suno-metatag-v1.15-atlas-ideaverse-curation.md`
**Workflow packet:** `ai/PLAN.md` (v1.15 r1 plan — Lead `APPROVED (PASS-WITH-NOTES)` 2026-05-28 with 2 LOW absorptions).

## Tasks

| # | Task | Acceptance |
|---|---|---|
| T0 | Wiki context refresh + decision-table inconsistency analysis + grep verification (zero prompts.json references; H1-H7 test impact). | Done at planning time. |
| T1 | Primary commit: (a) fix decision-table Summary breakdown 35/12/7 → 39/10/5 per absorption #1; (b) edit `Resources/tags.json` insert 39 new entries grouped by category (Mood 13 / Production 10 / Effect 8 / Instrument 5 / Vocal 3); (c) commit previously-untracked decision-table file from working tree → tracked; (d) create spec + plan docs. Targeted `git add`. | Single commit `B-SUNO-007b / v1.15 T1: Atlas Ideaverse metatag database curation (39 ADDs)`. |
| T2 | Secondary commit: (a) add `H8_AtlasIdeaverseMetatagDatabase_PresentInExpectedCategory` `[Theory]` with 10 `[InlineData]` (5 category representatives + 5 borderline-decision verifiers); (b) extend `H2_ExtendedCategoryCountsMet` `[Theory]` with Mood (>=34) and Effect (>=27) rows + bump Production 6 → 16 per absorption #2; (c) refresh H5 stale comment 335 → 374. | One commit `B-SUNO-007b / v1.15 T2: H8 + H2 extension tests`. Test count 136 → 148. |
| T3 | Build + test gate. | 148/148 green. |
| T4 | Dev smoke. | `TIMEOUT_EXIT=124`. |
| T5 | Publish rebuild + publish smoke. | `TIMEOUT_EXIT=124`. tags.json delta ~+3.3 KB; prompts.json byte-identical to v1.14. |
| T6 | USER REVIEW S1-S6. | User-confirmed PASS. |
| T7 | Wiki updates — `[[sunometatag-tag-library]]` full refresh (count 335 → 374, per-category-row refresh, new v1.15 Atlas Ideaverse subsection, H8 in test-coverage list, H1/H5 count refreshes), `[[sunometatag-app]]` v1.15 subsection. | Pages updated; `last_confirmed=2026-05-28`. |
| T8 | Closeout — Archive entry 33 to plan archive; consolidated EXECUTION_LOG entry; rewrite `ai/PLAN.md` as RESULT. | RESULT packet per template. |

## Lead r1 absorptions (PASS-WITH-NOTES)

1. **Fix decision-table Summary breakdown** — 35/12/7 → 39/10/5 to match row-level authoritative breakdown, done BEFORE committing the source artifact. Reconciliation note added to the file's Summary section.
2. **Extend H2 category-threshold coverage to include Mood and Effect** — added `[InlineData("Mood", 34)]` + `[InlineData("Effect", 27)]` to existing 5-row H2 [Theory], plus bumped Production 6 → 16 to reflect post-v1.15 actual.

## Pre-flight checks (T0)

- `tags.json` baseline counts: Mood 21, Effect 19, Vocal 45, Instrument 36, Production 6, SFX 63, Genre 107, Structure 38 (total 335).
- Decision-table inconsistency: file Summary stated 35/12/7 but row count is 39/10/5. Reconciled per absorption #1.
- `prompts.json` grep: zero references to the 39 new brackets.
- Existing 136-test suite: H1 `>= 320` passes (335→374); H2 5 categories unaffected by Mood/Effect additions; H4 categories non-empty unaffected; H5 uniqueness unaffected (new entries new+unique); H6 fixed brackets unaffected; H7 Verse-cluster unaffected.

## Rollback

Two-commit revert: `git revert <T2-sha> <T1-sha>`. Tests return to 136.
