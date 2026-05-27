# SunoMetatagApp v1.12 — Chip-Pill Background/BorderBrush Color-State Parity — Plan

**Date:** 2026-05-27
**Companion to:** `docs/specs/2026-05-27-suno-metatag-v1.12-chip-pill-parity.md`
**Workflow packet:** `ai/PLAN.md` (v1.12 r1 plan — Lead `APPROVED (PASS-WITH-NOTES)` 2026-05-27 with 3 LOW absorptions).

## Tasks

| # | Task | Acceptance |
|---|---|---|
| T0 | Wiki context refresh — `[[sunometatag-visual-theme]]`, `[[focus-flip-stale-insert]]`, `[[sunometatag-app]]` + Lead r1 absorption text. | Done at planning time. |
| T1 | Primary commit: edit `Themes/SunoStyles.xaml` (`SunoTagPill` 6 token-ref swaps + new `IsKeyboardFocused` Trigger + clarified comment per absorption #1), edit `Themes/SunoTokens.xaml` (delete 3 Colors + 3 Brushes + obsolete comment), create this plan doc + companion spec doc. Targeted `git add`. | Single commit `B-SUNO-010 / v1.12 T1: chip-pill Background/BorderBrush color-state parity`. |
| T2 | (Skipped) No secondary test commit — visual-styling-only change. | Rationale in plan + RESULT. |
| T3 | Build + test gate. | 130/130 green. |
| T4 | Dev smoke. | `EXIT=124`. |
| T5 | Publish rebuild + publish smoke. | `EXIT=124`; `tags.json` + `prompts.json` byte-identical to v1.11 closeout. |
| T6 | USER REVIEW S1-S6 with explicit S3 sub-checks (absorption #2). | User-confirmed PASS. |
| T7 | Wiki updates landed in-cycle, including new "Parity boundaries (v1.12)" subsection in `[[sunometatag-visual-theme]]` (absorption #3). | Pages updated; `last_confirmed=2026-05-27`. |
| T8 | Archive r1 plan to plan archive; consolidated EXECUTION_LOG entry; rewrite `ai/PLAN.md` as RESULT. | RESULT packet covers §1-§11 per template. |

## Lead r1 absorptions (PASS-WITH-NOTES)

1. **Clarify parity wording as Background/BorderBrush parity** — applied in style comment, spec, plan, wiki updates, and RESULT.
2. **Explicit S3 pressed-state sub-check** — sub-checks (a) hover lighter purple, (b) press dark (#3A3A48 not deep fuchsia) made explicit at T6.
3. **Document parity-boundary details in v1.12 visual-theme wiki updates** — new "Parity boundaries (v1.12)" subsection at T7 explicitly lists parity (Background, BorderBrush, base/hover/pressed/focus) vs divergent (Foreground, shape, font size, margin).

## Pre-flight grep result (T0)

`SunoAccentPill*` tokens referenced only by the 6 swap targets in `SunoTagPill`. Zero external consumers. Safe to delete.

## Rollback

`git revert <T1-sha>`. Tests stay at 130/130. Single-commit slice.
