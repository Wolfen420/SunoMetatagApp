# SunoMetatagApp v1.13 — Default Category Dropdown to Structure — Spec

**Date:** 2026-05-28
**Slice:** B-SUNO-014 / v1.13 (Medium priority)
**Scope:** 3-line behavioral default-state change in `MainViewModel`. Zero data/style change.

## 1. Problem

The category dropdown above the tag-picker pills currently defaults to `"All"` on app load, showing the full filtered tag corpus (~331 entries alphabetized per v1.11). Section/Structure tags (`[Verse]`, `[Chorus]`, `[Bridge]`, etc.) are the most-commonly inserted on first use because they anchor the song skeleton, but users must manually open the dropdown and select `"Structure"` to scope the picker to them.

User request (2026-05-27, B-SUNO-014 acceptance language): *"On app load, the category dropdown defaults to `Structure` (not `All`), and the initial pill list reflects Structure-filtered tags while preserving existing search/filter behavior after user changes."*

## 2. Mechanism

Two source edits in `src/SunoMetatagApp/ViewModels/MainViewModel.cs`:

1. **Field initializer (line 28)** changes from `"All"` to `"Structure"`:
   ```csharp
   [ObservableProperty] private string _selectedCategory = "Structure";
   ```
2. **Main constructor assignment (line 55)** changes from `"All"` to `"Structure"`:
   ```csharp
   SelectedCategory = "Structure";
   ```
   This fires the CommunityToolkit.Mvvm-generated partial method `OnSelectedCategoryChanged` → `FilteredTags = ComputeFiltered()`.
3. **Error-state constructor (line 68): UNCHANGED.** Keep `SelectedCategory = "All"` because `Categories = new[] { "All" }` only in that branch — assigning a value not present in `Categories` would render a blank ComboBox selection.

## 3. Why two changes and not one

The CommunityToolkit.Mvvm source generator emits a partial property `SelectedCategory` from `_selectedCategory`. The field initializer (line 28) sets the *initial* backing-field value but does NOT fire `OnSelectedCategoryChanged` (no setter has run yet). The constructor's explicit assignment (line 55) DOES fire `OnSelectedCategoryChanged` → `FilteredTags = ComputeFiltered()`. Without updating line 55, FilteredTags would carry the pre-change initial state. Both edits together produce the correct net behavior; updating only line 28 would not be sufficient for the initial FilteredTags to be Structure-filtered.

## 4. Initial state after v1.13

After v1.13 changes, the initial app-load state has:
- `SelectedCategory = "Structure"`
- `FilteredTags = ComputeFiltered() = TagService.Filter(_allTags, "", "Structure")` = the 34 Structure-category entries (`[Bridge]`, `[Build]`, `[Chorus]`, `[Coda]`, `[Drop]`, `[Hook]`, `[Intro]`, `[Outro]`, `[Pre-Chorus]`, `[Refrain]`, `[Verse]`, `[Verse 1]`, `[Verse 2]`, etc.) sorted alphabetically per v1.11 `OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase)`.

## 5. Parity boundaries (what does NOT change)

- `BuildCategories` — `["All", ...distinct]` shape unchanged.
- `ComputeFiltered` — logic unchanged.
- `OnSelectedCategoryChanged` — partial method unchanged.
- `tags.json` + `prompts.json` — byte-identical to v1.12 closeout.
- `MainWindow.xaml` ComboBox `SelectedItem` binding — unchanged.
- v1.7 search normalization, v1.10 picker-pane focus preservation, v1.11 alphabetical ordering, v1.12 chip-pill colors — all preserved.
- Error-state constructor — `Categories = new[] { "All" }; SelectedCategory = "All"` invariant explicitly preserved.

## 6. Validation

Test additions in `tests/SunoMetatagApp.Tests/MainViewModelTests.cs`:

- **D1** — Normal constructor: `Assert.Equal("Structure", vm.SelectedCategory)` and FilteredTags contains only Structure-category entries (using `Sample` which has 2 Structure + 1 Vocal entry → 2 Structure-filtered).
- **D2** — Error constructor: `Assert.Equal("All", vm.SelectedCategory)` (regression-gate for the explicit non-change on line 68).

Test count forecast: 130 → 132.

USER REVIEW S1-S6 (see r1 plan packet §7.2):
- S1 (primary): default state on app load shows "Structure" + Structure-filtered pills
- S2: switch/switch-back stability
- S3: search composition inside Structure
- S4: switch to other category
- S5: v1.10 picker-pane focus regression-gate
- S6: v1.3 stacked syntax regression-gate

## 7. Rollback

Two-commit revert: `git revert <T2-sha> <T1-sha>`. Tests return to 130.

## 8. Related

- `[[sunometatag-app]]` — feature page; v1.13 subsection lands at T7.
- `[[focus-flip-stale-insert]]` — risk page; no interaction (focus tracking independent of category default).
- B-SUNO-015 — queued for v1.14 (Verse 3-6 tags) per Lead direction at v1.12 closeout.
