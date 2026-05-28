# SunoMetatagApp v1.17 — Tag Role Sort Order (B-026)

**Date:** 2026-05-28
**Backlog:** `B-026` (Open, Medium, Owner: Planner)
**Slice type:** Schema + service + UI guidance — adds the canonical `SortOrder` role to every tag and surfaces the left-to-right stacking sequence in the picker when `SelectedCategory == "All"`.

## Acceptance (from `docs/BACKLOG.md`)

> Add optional `sortOrder` to `tags.json` entries with category mapping `Structure=1`, `Vocal=2`, `Instrument=3`, `Mood=4`, `Effect=5`, `SFX=6`, `Production=7` (default `99` when missing), add `SortOrder` to `Models/TagDefinition.cs` with default `99`, and show a subtle read-only guidance banner/divider in the picker when category is `All` (e.g., `1 Structure -> 2 Vocal -> 3 Instrument -> 4 Mood -> 5 Effect`); do not auto-reorder `InsertTagStacked` in this item.

## Mechanism summary

- **Carry-over reconciled into a clean commit (T1):** prior B-026 in-flight working-tree changes — `TagDefinition.cs` 5th positional field `int SortOrder = 99`, and `tags.json` `"sortOrder": N` field on every entry per the canonical mapping (Structure=1, Vocal=2, Instrument=3, Mood=4, Effect=5, SFX=6, Production=7; Genre defaults to 99) — staged and committed as part of this slice per Lead's v1.16 closeout mandate.
- **TagService deserialization wiring (T1):** added `[JsonPropertyName("sortOrder")] int? SortOrder` to private `TagDto`; updated `LoadAll` to pass `d.SortOrder ?? 99` to the 5-arg `new TagDefinition(...)` constructor. Both missing JSON field and explicit `null` coalesce to 99.
- **UI guidance banner (T1):** new `Grid.Row="3"` in `TagPickerPane` (`MainWindow.xaml`) containing a `<TextBlock>` with text `"1 Structure  →  2 Vocal  →  3 Instrument  →  4 Mood  →  5 Effect  →  6 SFX  →  7 Production"`. Visibility controlled by a single `DataTrigger Binding="{Binding SelectedCategory}" Value="All"` (default `Visibility="Collapsed"`; becomes `Visible` only when `SelectedCategory == "All"`). `IsHitTestVisible="False"` for read-only guidance. `TextAlignment="Center"`, `FontSize="{StaticResource SunoFontSizeSmall}"`, `Foreground="{StaticResource SunoTextSecondaryBrush}"`. Pill grid previously at `Grid.Row="3"` shifts to `Grid.Row="4"`.

## Non-changes (preserved contracts)

Explicit non-goal per Lead absorption #2: **`TagService.Filter` ordering behavior and `MainViewModel.InsertTagStacked` are unchanged in this slice.** v1.11 alphabetical-by-`Bracket` ordering remains authoritative; `SortOrder` is exposed on the model for future consumers but NOT consumed by Filter or InsertTagStacked at v1.17. Auto-reorder of stacked tokens is explicitly deferred per BACKLOG.

Other preserved contracts:
- v1.16 search-clear (×) control + SearchBox Padding override — unaffected by the `Grid.Row` renumber (verified at USER REVIEW S4).
- v1.15 Atlas Ideaverse 39-entry corpus, v1.14 Verse-cluster, v1.13 default-Structure, v1.12 chip-pill parity, v1.11 alphabetical ordering, v1.10 picker-pane focus preservation, v1.7 search normalization — unchanged.
- `PromptService`, `PromptDefinition`, `prompts.json` — unchanged (byte-identical).
- `Themes/SunoTokens.xaml` + `Themes/SunoStyles.xaml` — unchanged (banner reuses existing typography + color tokens).
- `MainViewModel.cs` — unchanged (banner uses direct DataTrigger on `SelectedCategory`; no new VM property).

## Validation

- **Test count: 151 → 160** (per Lead absorption #1; r1 plan §6.3 stated 154, corrected here to authoritative 160).
  - **S1** `[Theory]` with 7 `[InlineData]` rows (`Structure=1`, `Vocal=2`, `Instrument=3`, `Mood=4`, `Effect=5`, `SFX=6`, `Production=7`) asserting every entry in category X has `SortOrder == Y` via `LoadProductionTagsJson()`.
  - **S2** `[Fact]` asserting Genre category entries all have `SortOrder == 99` (default fallback for the deliberately-omitted mapping).
  - **S3** `[Fact]` synthetic JSON missing the `sortOrder` field → `LoadAll` defaults to 99.
- USER REVIEW S1-S6: banner visible at All; collapsed otherwise; layout integrity; v1.16 + multi-cycle regression-gates; sortOrder data-only (not consumed by Filter).
- Smoke gates: dev `dotnet run` + publish `dotnet publish ... -p:PublishSingleFile=true` + publish exe smoke (8 s timeout each).

## Rollback

Two-commit revert: `git revert T2-sha T1-sha` returns to v1.16 closeout tip `0074816`.
