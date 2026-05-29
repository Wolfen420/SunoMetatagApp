# SunoMetatagApp v1.19 — SortOrder-Based Stacked-Tag Auto-Reorder (B-027)

**Date:** 2026-05-29
**Backlog:** `B-027` (Lead-designated via v1.18 closeout `target_item`; Lead to add formal BACKLOG row at v1.19 closeout reviewer-memory updates).
**Slice type:** Behavioral — `MainViewModel.InsertTagStacked` post-append auto-reorder. Activates the canonical SortOrder sequence v1.17 wired but deferred.

## Acceptance (from Lead's v1.18 closeout `target_item`)

> "Plan B-027 in ai/PLAN.md as the next backlog slice (SortOrder-based stacked-tag auto-reorder)."

Derived contract: when `InsertTagStacked`'s merge-target branch completes (§3.5 append), the resulting `[tag | tag | tag]` content is auto-reordered by each token's canonical `TagDefinition.SortOrder` (Structure=1, Vocal=2, Instrument=3, Mood=4, Effect=5, SFX=6, Production=7). Stable sort preserves user-typed order within same-SortOrder groups. Genre tokens (`SortOrder=99` default per v1.17 canonical mapping) and unknown tokens (no matching `TagDefinition` by Label or stripped Bracket) sort to the end. v1.11 alphabetical pill-picker LIST ordering is unaffected — v1.19 only touches the inline `[...]` bracket content produced by stacking.

## Mechanism summary

- **Single edit point:** `ViewModels/MainViewModel.cs` `InsertTagStacked` method. New §3.7 block placed AFTER the existing §3.5 append and BEFORE the §3.6 caret-landing update. Original §3.6 still applies when the reorder is a no-op (already canonical).
- **Algorithm:** locate `[openIdx, closeIdx]` of the just-merged bracket (closeIdx = `appendAt.Value + insertText.Length`; openIdx = `LastIndexOf('[', closeIdx - 1)` — invariant documented inline per absorption #2); extract `content` between `[` and `]`; `Split('|')` + `Trim` + drop empties; per-token `SortOrderOf(token)` two-tier lookup via `_allTags` (Label first, then bracket-stripped, default 99); stable `OrderBy(SortOrderOf)`; rejoin with ` \| `; replace bracket content via `section.Lyrics = current[..(newOpenIdx+1)] + rejoined + current[newCloseIdx..]`; re-derive `FocusedCaretPosition` past the new bracket length.
- **No-op short-circuit:** `if (!sorted.SequenceEqual(tokens, StringComparer.Ordinal))` skips the property reassignment when the reorder is identity — avoids spurious PropertyChanged + RecomputePreview cycles.

## Lead absorptions resolved at T1

1. **Explicit Genre=99 ratification** — Genre tokens fall to the canonical `SortOrder=99` default per v1.17 mapping (Genre intentionally omitted from the BACKLOG B-026 canonical mapping per `[[sunometatag-tag-library]]`). In v1.19's reorder algorithm, Genre tokens therefore sort to the END of the bracket alongside unknown tokens (custom user text with no matching `TagDefinition`). Verified by U5 test (`[Rock | Distorted]` + Shift+Drums → `[Drums | Distorted | Rock]`). This is the v1.19 ratified semantics; any future change to Genre's position would require a BACKLOG B-026 canonical mapping update (with corresponding tags.json data revision) rather than a v1.19 algorithm change.
2. **`LastIndexOf('[')` merge-target invariant documented inline** — multi-line comment in §3.7 explains that §3.2/§3.3 guarantee `appendAt` points into a complete `[...]` block on the current line with no intervening `[` or `]` between the matching open `[` and the original `]`. After §3.5 inserts `insertText` immediately before the original `]`, the new `]` shifts to index `appendAt.Value + insertText.Length` and the matching `[` remains the closest `[` strictly to the left. `LastIndexOf('[', newCloseIdx - 1)` therefore locates it correctly.
3. **Record positional-arity precision** — `TagDefinition` is a `sealed record` with **5 positional parameters** (Category, Label, Bracket, Description, SortOrder) — explicit per the v1.17 schema. In spec/RESULT/wiki language, references to "the 5-argument constructor" specifically mean the `record` positional constructor with all 5 parameters supplied. Existing test fixtures that use the 3-parameter form (e.g., `new("Structure", "Verse", "[Verse]")`) rely on Description=`null` and SortOrder=`99` positional defaults. The v1.19 U-tests use the explicit 5-parameter form to seed SortOrder values for meaningful reorder coverage.

## Non-changes (preserved contracts)

- `TagService.Filter` v1.11 alphabetical-by-Bracket pill-LIST ordering — unchanged.
- `TagService.LoadAll` + `TagDefinition` + `tags.json` — unchanged (v1.19 only CONSUMES the SortOrder data that v1.17 wired).
- `MainViewModel.InsertTag` (non-stacked single-bracket insert) — unchanged.
- `MainViewModel.InsertTagStacked` §3.1-§3.6 — preserved exactly; §3.7 is purely additive.
- `MainWindow.xaml` + `MainWindow.xaml.cs` — unchanged. No UI affordance, no markup change, no new handler.
- `Themes/SunoTokens.xaml` + `Themes/SunoStyles.xaml` — unchanged.
- v1.10 picker-pane focus, v1.13 default-Structure, v1.14 Verse 3-6, v1.15 Atlas Ideaverse, v1.16 search-clear, v1.17 SortOrder banner, v1.18 Song Structure Templates — all unchanged.
- Existing T1-T16 stacked-syntax tests in `MainViewModelInsertTagStackedTests.cs` — stable no-op under v1.19 because their Sample uses the 3-parameter `TagDefinition` constructor form → all entries default to `SortOrder=99` → reorder is a stable identity.

## Validation

- **Test count: 171 → 179** (+8 U-tests: U1 2-token reorder, U2 3-token reorder, U3 stable sort within same SortOrder, U4 unknown token to end, U5 Genre to end, U6 fresh-bracket no-reorder, U7 already-sorted-bracket new-token canonical slot, U8 mid-group insertion).
- New test file `tests/SunoMetatagApp.Tests/MainViewModelInsertTagStackedAutoReorderTests.cs` (separate from existing `MainViewModelInsertTagStackedTests.cs` so its Sample can use the explicit 5-parameter form for explicit per-tag SortOrder values).
- USER REVIEW S1-S6: primary auto-reorder + 3-token + existing-order preservation + unknown-token-at-end + multi-cycle regression-gates + fresh-bracket fallback preserved.
- Smoke gates: dev `dotnet run` + publish `dotnet publish ... -p:PublishSingleFile=true` + publish exe smoke (8 s timeout each).

## Rollback

Two-commit revert: `git revert T2-sha T1-sha` returns to v1.18 closeout tip `3971a21`. `MainViewModel.cs` returns to v1.18-shape `InsertTagStacked` (§3.7 block removed); new test file deleted; tests return to 171.
