# SunoMetatagApp v1.20 — User-Defined Template Persistence (B-028)

**Date:** 2026-05-29
**Backlog:** B-028 (Lead-designated next planner item via v1.19 closeout target_item)
**Cycle predecessor:** v1.19 (B-027 SortOrder-based stacked-tag auto-reorder) closeout `APPROVED (PASS)` 2026-05-29
**Status:** Spec — implementation landed at v1.20 T1 commit

---

## Scope

Lift v1.18's explicitly-deferred non-goal: persist user-defined `SongTemplate` entries to `%APPDATA%\SunoMetatagApp\templates.json` alongside the 4 hardcoded built-ins shipped at v1.18. Add Save + Delete affordances; built-ins remain read-only.

## Contract

### Storage

- **Path:** `%APPDATA%\SunoMetatagApp\templates.json` (resolved via `Environment.SpecialFolder.ApplicationData`).
- **Schema:** flat JSON array of `{ "name": string, "sectionTypes": string[] }`. No version field (YAGNI).
- **Creation:** directory and file created on first save; absent until then.
- **Atomicity:** writes go to `templates.json.tmp` then `File.Move(.tmp → .json, overwrite: true)` to avoid partial-write corruption.
- **Load defensiveness:** missing file → empty list; malformed JSON → empty list (caught and swallowed); entries with empty `name` or empty `sectionTypes` are silently skipped.

### Built-in vs user-defined separation

- Built-in templates (4 v1.18 hardcoded entries: `Standard Pop`, `Simple Ballad`, `Rock / EDM`, `Rap / Hip-Hop`) remain authoritative in `Models/SongTemplates.BuiltIns`.
- User templates are mutable, loaded from disk on construction, persisted on every Save/Delete.
- Combined `Templates : ObservableCollection<TemplateListItem>` view exposes both groups; XAML `CollectionViewSource` groups them by `TemplateListItem.Group` ("Built-in Templates" / "My Templates").
- Built-in templates are NOT deletable: the × button affordance is `Visibility="Collapsed"` by default, with a `DataTrigger` revealing it only when `IsUserDefined="True"`.

### UX affordances (ratified pre-r1)

- **Save:** dedicated `Save as Template…` button next to the Load Template ComboBox. Click opens `TemplateNameDialog` modal; on OK, duplicate-name confirmation via `MessageBox.Show` if the name already exists; then `SaveCurrentAsTemplateCommand` captures non-empty `Sections[*].SectionType` values into a new `SongTemplate` and persists.
- **Delete:** per-item × button visible only on user templates in the ComboBox dropdown; click executes `DeleteUserTemplateCommand` and persists; the click does NOT trigger a load of the underlying ComboBoxItem (selection-on-delete guard per Lead absorption #4).
- **Load:** unchanged from v1.18 — ComboBox `SelectionChanged` handler unwraps `TemplateListItem.Template` and invokes `LoadTemplateCommand(SongTemplate?)` (signature preserved per Lead absorption #3).

## Lead absorptions resolved at T1

1. **Save-button style resource key fix.** r1 plan referenced `SunoSecondaryButton` which does not exist in `Themes/SunoStyles.xaml`. Resolved by using the existing `SunoButton` base style key — appropriate visual weight for the unobtrusive Save action.
2. **ComboBox Popup-boundary command binding.** ComboBox dropdown items live in a separate visual/logical tree rooted in a PopupRoot, so `RelativeSource={AncestorType=Window}` from inside `ComboBox.ItemTemplate` resolves to null. Resolved by introducing `Views/BindingProxy.cs` (a `Freezable`-derived class holding a `Data` dependency property) declared as a `Window.Resources` entry: `<views:BindingProxy x:Key="VmProxy" Data="{Binding}" />`. The × Button's command binding hops through this proxy: `Command="{Binding Data.DeleteUserTemplateCommand, Source={StaticResource VmProxy}}"`. Freezable inheritance context propagates the resource into the popup tree.
3. **LoadTemplate parameter migration.** `MainViewModel.LoadTemplate` signature stays as `LoadTemplate(SongTemplate?)` — unchanged from v1.18. The XAML ComboBox `ItemsSource` is now bound to `TemplatesView` (a `CollectionViewSource` over `Templates : ObservableCollection<TemplateListItem>`), and the code-behind `TemplateComboBox_SelectionChanged` handler unwraps `TemplateListItem.Template` before invoking the command. All 8 v1.18 `MainViewModelLoadTemplateTests` continue to pass with their original `Execute(SongTemplate)` calls; only the VM construction in those tests was updated to inject a temp-path `UserTemplateService` for isolation from the developer's real `%APPDATA%\SunoMetatagApp\templates.json` state.
4. **Selection-on-delete guard.** Without intervention, clicking the × button inside a `ComboBoxItem` would cause the mouse-down event to bubble up to the item and trigger `SelectionChanged` → unintended template load. Resolved via a `PreviewMouseLeftButtonDown` handler on the × button that executes the delete command manually and sets `e.Handled = true`, stopping the routed event before it can reach the parent `ComboBoxItem`. Keyboard activation (Enter/Space on a focused × button) continues to work via the standard Command binding because it follows a different routed-event chain (KeyDown → Click → Command).

## Mechanism

### Files added (v1.20 T1)

- `src/SunoMetatagApp/Models/UserTemplateDto.cs` — internal JSON DTO with `Name` and `SectionTypes` properties (matches `SongTemplate` shape).
- `src/SunoMetatagApp/Services/UserTemplateService.cs` — instance service with constructor-injected `TemplatesPath`, `LoadAll()`, `SaveAll(...)`, and `CreateDefault()` factory.
- `src/SunoMetatagApp/ViewModels/TemplateListItem.cs` — wrapper carrying `Template`, `IsUserDefined`, `Name`, `Group`.
- `src/SunoMetatagApp/Views/BindingProxy.cs` — Freezable-derived proxy for Popup-boundary command binding (absorption #2).
- `src/SunoMetatagApp/Views/TemplateNameDialog.xaml` + `.xaml.cs` — modal name-input dialog with Enter/Esc keyboard handling, OK-enabled-on-nonempty, return `Result` string.

### Files extended (v1.20 T1)

- `src/SunoMetatagApp/ViewModels/MainViewModel.cs` — new 3-arg primary constructor accepting optional `UserTemplateService?`; new `_userTemplateService` field; new `UserTemplates : ObservableCollection<SongTemplate>` and `Templates : ObservableCollection<TemplateListItem>` collections; new `RebuildTemplatesCollection()` method; new `SaveCurrentAsTemplateCommand(string?)`; new `DeleteUserTemplateCommand(TemplateListItem?)`; `LoadTemplate(SongTemplate?)` signature unchanged. Error-state constructor also initializes the service (skips `LoadAll()` for fast-path).
- `src/SunoMetatagApp/MainWindow.xaml` — Window.Resources gains `VmProxy` BindingProxy and `TemplatesView` CollectionViewSource (with `PropertyGroupDescription` on `Group`); template area restructured from single `Grid` to `DockPanel` with right-docked `SaveTemplateButton` + ComboBox in fill area; ComboBox `ItemsSource` retargeted to `{StaticResource TemplatesView}`; new `<ComboBox.GroupStyle>` with `HeaderTemplate`; new `<ComboBox.ItemTemplate>` with `DockPanel` containing the × delete button (using `BindingProxy` for command binding + `DataTrigger` on `IsUserDefined` for visibility) and the template name TextBlock; existing placeholder TextBlock unchanged.
- `src/SunoMetatagApp/MainWindow.xaml.cs` — new `using SunoMetatagApp.Views;`; `TemplateComboBox_SelectionChanged` adapted to extract `TemplateListItem.Template`; new `SaveTemplateButton_Click` handler with `TemplateNameDialog.Prompt` + duplicate-name `MessageBox.Show` confirmation; new `DeleteUserTemplateButton_PreviewMouseLeftButtonDown` handler executing the command and setting `e.Handled = true`.

## Validation

### Test coverage added (v1.20 T2)

- `tests/SunoMetatagApp.Tests/UserTemplateServiceTests.cs` — V1-V8 covering missing-file, malformed-JSON, valid-JSON roundtrip, empty-name skip, empty-sectionTypes skip, directory creation, atomic-write `.tmp` cleanup, save→load roundtrip.
- `tests/SunoMetatagApp.Tests/MainViewModelSaveDeleteTemplateTests.cs` — W1-W10 covering Save adds to UserTemplates, Save persists, empty-name no-op (W3 [Theory] with 3 InlineData), no-section-types no-op, duplicate-name replacement, trimmed-name, Delete removes and persists, built-in Delete no-op, Templates collection ordering and Group property, constructor load from preexisting file.
- `tests/SunoMetatagApp.Tests/MainViewModelLoadTemplateTests.cs` — updated CreateVm helper to inject temp-path `UserTemplateService` (per absorption #3 test isolation); 8 v1.18 test cases otherwise unchanged.

Total test count: 179 (v1.19 baseline) + 8 V + 12 W (10 [Fact] + 1 [Theory] with 3 InlineData) = **199/199 green** in 111 ms (T3 measurement).

### Smoke

T4 dev smoke + T5 publish smoke deferred to execution log; both expected `EXIT=124`.

## Explicit non-changes

- `MainViewModel.InsertTag` / `InsertTagStacked` (all §3.x logic byte-unchanged from v1.19).
- `TagService.Filter` / `TagService.LoadAll` (v1.11 alphabetical pill-LIST ordering; v1.17 SortOrder loading).
- `tags.json` / `TagDefinition` / `prompts.json` / `PromptService` / `PromptDefinition`.
- `Models/SongTemplate.cs` record shape.
- `Models/SongTemplates.cs` BuiltIns content (4 hardcoded built-ins, unmodified).
- `Themes/SunoTokens.xaml` + `Themes/SunoStyles.xaml`.
- `Models/Section.cs` (SectionType field shape from v1.18).
- No template rename / edit (deferred).
- No template import / export (deferred).
- No JSON schema version (YAGNI for v1.20).
- README.md carry-over (separate docs cycle).

## Acceptance

- v1.18 deferral lifted; user-defined templates persist to `%APPDATA%\SunoMetatagApp\templates.json`.
- Save + Delete affordances landed; built-ins remain read-only.
- All 4 Lead r1 absorptions resolved.
- All 199 tests green.
- v1.19-and-prior contracts preserved (multi-cycle regression-gates intact).
