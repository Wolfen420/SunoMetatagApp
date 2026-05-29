# SunoMetatagApp v1.19 — Implementation Plan (B-027)

**Spec:** [`2026-05-29-suno-metatag-v1.19-sortorder-stacked-auto-reorder.md`](../specs/2026-05-29-suno-metatag-v1.19-sortorder-stacked-auto-reorder.md)
**Lead r1 verdict:** `APPROVED (PASS-WITH-NOTES)` 2026-05-29 with 3 LOW absorptions: (1) ratify Genre=99 sorting to end for this slice; (2) document the `LastIndexOf('[')` merge-target invariant inline in the new reorder block; (3) record positional-arity wording precision (5-parameter `TagDefinition` record). All three absorbed at T1.

## Task sequence

| Task | Description |
|---|---|
| **T1** | Primary commit: edit `ViewModels/MainViewModel.cs` `InsertTagStacked` to add §3.7 reorder block between §3.5 and §3.6, including multi-line invariant comment per absorption #2 and explicit Genre/unknown-token semantics per absorption #1. Add v1.19 spec + plan docs (with absorption #3 positional-arity precision applied). |
| **T2** | Secondary commit: new `tests/SunoMetatagApp.Tests/MainViewModelInsertTagStackedAutoReorderTests.cs` with U1-U8 tests using a Sample tag array via the explicit 5-parameter `TagDefinition` constructor form (per absorption #3). |
| **T3** | `dotnet test` → expect 179/179 green. |
| **T4** | Dev smoke `timeout 8 dotnet run` → expect `EXIT=124`. |
| **T5** | `dotnet publish` + publish smoke → expect `EXIT=124`. |
| **T6** | USER REVIEW S1-S6. |
| **T7** | Wiki: `[[sunometatag-app]]` v1.18 → v1.19 + new subsection; `[[sunometatag-tag-library]]` v1.17 SortOrder section deferral-lifted note + Genre=99-to-end ratification. |
| **T8** | RESULT packet + archive entry 41 + EXECUTION_LOG.md append + USER ACTION NEEDED for Lead closeout. |

## Files modified (T1)

- `src/SunoMetatagApp/ViewModels/MainViewModel.cs` (§3.7 reorder block, ~25 lines added to `InsertTagStacked`)
- `docs/specs/2026-05-29-suno-metatag-v1.19-sortorder-stacked-auto-reorder.md` (new)
- `docs/plans/2026-05-29-suno-metatag-v1.19-sortorder-stacked-auto-reorder.md` (new — this file)

## Files modified (T2)

- `tests/SunoMetatagApp.Tests/MainViewModelInsertTagStackedAutoReorderTests.cs` (new — U1-U8 tests)

## Expected publish artifact delta

Non-zero `publish/SunoMetatagApp.exe` delta vs v1.18 from the new IL in `InsertTagStacked` (§3.7 block adds ~25 lines including a local function). `tags.json` byte-identical to v1.18 publish (v1.19 does not modify tags.json). `prompts.json` byte-identical to all cycles since v1.9.
