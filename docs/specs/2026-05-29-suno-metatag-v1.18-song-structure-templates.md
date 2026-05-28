# SunoMetatagApp v1.18 — Song Structure Templates (B-025)

**Date:** 2026-05-29
**Backlog:** `B-025` (Open, Medium, Owner: Planner)
**Slice type:** Schema + model + ViewModel + UI — adds 4 hardcoded built-in song-structure templates with a Load Template ComboBox above the section stack and a `SectionType` label inside each section card.

## Acceptance (from `docs/BACKLOG.md`)

> Add a Load Template control above sections; selecting a template confirms before clearing non-empty lyrics, rebuilds sections in canonical template order (Standard Pop, Simple Ballad, Rock / EDM, Rap / Hip-Hop), defines `record SongTemplate(string Name, IReadOnlyList<string> SectionTypes)`, adds `LoadTemplateCommand(SongTemplate template)` to `MainViewModel`, and loads templates via existing `AddSection()` / `RemoveSection()` while setting a new `SectionType` string on each section.

## Mechanism summary

- **New `Models/SongTemplate.cs`:** `public sealed record SongTemplate(string Name, IReadOnlyList<string> SectionTypes);`
- **New `Models/SongTemplates.cs`:** `public static class SongTemplates { public static IReadOnlyList<SongTemplate> BuiltIns { get; } = [...]; }` — 4 hardcoded templates.
- **`Models/Section.cs`:** add `[ObservableProperty] private string _sectionType = "";` field. Default empty for sections created via plain `AddSection()` without a template.
- **`ViewModels/MainViewModel.cs`:**
  - Add `public IReadOnlyList<SongTemplate> BuiltInTemplates { get; } = SongTemplates.BuiltIns;`
  - Add `[RelayCommand] private void LoadTemplate(SongTemplate? template)` — null/empty-guard, then `Sections.Clear()`, then per `sectionType` in `template.SectionTypes` call `AddSection()` and set `Sections[^1].SectionType = sectionType`.
- **`MainWindow.xaml`:**
  - Inside middle `<DockPanel Grid.Column="2">`: new `<Grid DockPanel.Dock="Top">` containing a `<ComboBox x:Name="TemplateComboBox" ItemsSource="{Binding BuiltInTemplates}" DisplayMemberPath="Name" SelectionChanged="TemplateComboBox_SelectionChanged" />` plus an overlay `<TextBlock IsHitTestVisible="False" Text="Load template…">` visible only when `SelectedItem == null` (DataTrigger pattern per Lead absorption #1 — mirrors v1.16 search-box placeholder pattern).
  - Section card toolbar restructured from `<StackPanel HorizontalAlignment="Right">` to `<DockPanel>` with right-anchored `<StackPanel>` (▲ ▼ ×) and a left-aligned `<TextBlock Text="{Binding SectionType}">` visible only when SectionType non-empty (DataTrigger inverse of placeholder pattern).
- **`MainWindow.xaml.cs`:** new `TemplateComboBox_SelectionChanged` handler — guards against empty AddedItems and wrong type; reads selected `SongTemplate`; checks `vm.Sections.Any(s => !string.IsNullOrEmpty(s.Lyrics))` → shows `MessageBox.Show` (matches `DeleteSectionButton_Click` pattern) if true; on confirm invokes `vm.LoadTemplateCommand.Execute(template)`; always resets `cb.SelectedIndex = -1` so user can re-select the same template later.

## Lead absorptions resolved at T1

1. **Add explicit ComboBox placeholder affordance for discoverability** — overlay TextBlock with text `"Load template…"` and DataTrigger on `SelectedItem == null` (mirrors v1.16 SearchBox placeholder TextBlock pattern at `MainWindow.xaml` line 176-192, inverted to show when empty). Applied at T1.
2. **Preserve explicit non-goals** — no user-defined template persistence to `%APPDATA%\SunoMetatagApp\templates.json` (explicitly deferred per BACKLOG Notes); no unrelated ordering/engine-surface expansions (`TagService.Filter`, `MainViewModel.InsertTagStacked`, v1.11/v1.17 contracts all unchanged). Documented in spec/commits/RESULT.

## Built-in template content

| Template | SectionTypes | Count |
|---|---|---|
| **Standard Pop** | Intro, Verse 1, Pre-Chorus, Chorus, Verse 2, Pre-Chorus, Chorus, Bridge, Chorus, Outro | 10 |
| **Simple Ballad** | Intro, Verse 1, Chorus, Verse 2, Chorus, Bridge, Outro | 7 |
| **Rock / EDM** | Intro, Verse 1, Chorus, Verse 2, Chorus, Drop, Chorus, Outro | 8 |
| **Rap / Hip-Hop** | Intro, Verse 1, Hook, Verse 2, Hook, Verse 3, Hook, Outro | 8 |

## Non-changes (preserved contracts)

- `TagService.Filter`, `MainViewModel.InsertTagStacked`, `tags.json`, `TagDefinition.SortOrder` — unchanged.
- `PromptService`, `PromptDefinition`, `prompts.json` — unchanged.
- v1.10 picker-pane focus, v1.11 alphabetical, v1.13 default-Structure, v1.14 Verse 3-6, v1.15 Atlas Ideaverse, v1.16 search-clear, v1.17 SortOrder banner — all unchanged.
- `Themes/SunoTokens.xaml` + `Themes/SunoStyles.xaml` — unchanged (ComboBox reuses `SunoComboBox`; SectionType label reuses default typography).
- User-defined template persistence to `%APPDATA%\SunoMetatagApp\templates.json` — explicitly DEFERRED per BACKLOG Notes.

## Validation

- **Test count: 160 → 171** (160 baseline + 11 new MainViewModelLoadTemplateTests: T1 + 4 InlineData T2 + T3-T8). New test file `MainViewModelLoadTemplateTests.cs` matches existing `MainViewModelInsertTagStackedTests.cs` naming precedent.
- USER REVIEW S1-S6: primary template load + confirmation flow + cancel preserves + re-selection + section toolbar layout + multi-cycle regression-gates.
- Smoke gates: dev `dotnet run` + publish `dotnet publish ... -p:PublishSingleFile=true` + publish exe smoke (8 s timeout each).

## Rollback

Two-commit revert: `git revert T2-sha T1-sha` returns to v1.17 closeout tip `d4fa19b`. New files (SongTemplate.cs, SongTemplates.cs, MainViewModelLoadTemplateTests.cs, v1.18 spec/plan) deleted. Section.cs returns to 1-field; MainViewModel.cs returns to no LoadTemplateCommand; MainWindow.xaml returns to v1.17-shaped layout; MainWindow.xaml.cs returns to no TemplateComboBox_SelectionChanged.
