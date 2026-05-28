# SunoMetatagApp v1.16 — Search Clear (×) Control (B-SUNO-013)

**Date:** 2026-05-28
**Backlog:** `B-SUNO-013` (Open, Low, Owner: Planner)
**Slice type:** Usability polish — inline clear-control affordance inside the tag-picker SearchBox.

## Acceptance (from `docs/BACKLOG.md`)

> Search input includes an inline clear (x) control that appears when text is present and clears the search text in one click without disrupting current category/filter context. UI polish/usability improvement; keyboard behavior should remain consistent.

## Mechanism summary

- **VM:** new `[RelayCommand] private void ClearSearch() => SearchText = string.Empty;` on `MainViewModel`. The existing `OnSearchTextChanged` partial recomputes `FilteredTags` on the property change, so no extra plumbing is needed.
- **XAML:** add a right-anchored `<Button>` inside the existing search `<Grid Grid.Row="1">` (`MainWindow.xaml` lines 170-193 area). Bound to `ClearSearchCommand`, content `×`, style `SunoIconButton`, `Focusable="False"`. Visibility controlled by a single `DataTrigger` that mirrors the placeholder TextBlock pattern but inverted (default Visible; `Value=""` collapses).
- **SearchBox Padding:** `Padding="10,4,32,4"` overrides the inherited `SunoTextBox` Padding to reserve right space for the clear button (28 DIP rendered width via `SunoIconButton MinWidth=28` + 4 DIP right margin).

## Non-changes (preserved contracts)

- v1.10 picker-pane focus-preservation (`MainWindow.xaml.cs` `LostKeyboardFocus` defer-clear guard with `IsAncestorOf(TagPickerPane, focused)` check) — double-guarded by `Focusable="False"` on the new button.
- v1.11 alphabetical pill ordering (`TagService.Filter`) — unchanged.
- v1.13 default-Structure category — unchanged.
- v1.14 Verse 3-6 — unchanged.
- v1.15 Atlas Ideaverse 39-entry corpus — unchanged.
- v1.12 chip-pill color-state parity (`SunoStyles.xaml` `SunoTagPill`) — unchanged.
- v1.7 hyphen-insensitive search composition — unchanged.

## Lead absorption resolved at T1

`SunoIconButton` style sets `MinWidth=28` (`SunoStyles.xaml` line 97). Original r1 plan called for `Width=24` on the local button, which would have been overridden by the style's MinWidth at render time — leaving the SearchBox right padding (`28` planned) flush against the actual rendered button (no right margin). Resolved by:

- dropping local `Width=24` / `Height=22` / `Padding=0` overrides on the button (let the style apply MinWidth=28 + `SunoPaddingIconButton=6,2`),
- raising SearchBox `Padding` right to **32** DIP = 28 button width + 4 right margin (`Margin="0,0,4,4"`).

## Validation

- 3 new `[Fact]` tests in `MainViewModelTests.cs` (C1 command sets empty, C2 SelectedCategory preserved, C3 filter recompute round-trip). Test count 148 → **151**.
- USER REVIEW S1-S6: visibility appears/clears, category preservation regression-gate, empty-state stability, v1.10 picker-pane focus regression-gate, v1.11/v1.13/v1.14/v1.15 prior-cycle regression-gates, keyboard-flow integrity.
- Smoke gates: dev `dotnet run` + publish `dotnet publish ... -p:PublishSingleFile=true` + publish exe smoke (6 s timeout each).

## Rollback

Two-commit revert: `git revert T2-sha T1-sha` returns to v1.15 closeout tip `2439dc5`.
