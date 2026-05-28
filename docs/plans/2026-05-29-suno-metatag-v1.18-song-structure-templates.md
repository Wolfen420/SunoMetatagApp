# SunoMetatagApp v1.18 — Implementation Plan (B-025)

**Spec:** [`2026-05-29-suno-metatag-v1.18-song-structure-templates.md`](../specs/2026-05-29-suno-metatag-v1.18-song-structure-templates.md)
**Lead r1 verdict:** `APPROVED (PASS-WITH-NOTES)` 2026-05-29 with 2 LOW absorptions: (1) add explicit ComboBox placeholder affordance "Load template…" via overlay TextBlock + DataTrigger on SelectedItem=null; (2) preserve explicit non-goals (no user-defined template persistence in this slice; no unrelated ordering/engine-surface expansions). Both absorbed at T1.

## Task sequence

| Task | Description |
|---|---|
| **T1** | Primary commit: new `Models/SongTemplate.cs` + `Models/SongTemplates.cs`; edit `Models/Section.cs` (SectionType field); edit `ViewModels/MainViewModel.cs` (BuiltInTemplates property + LoadTemplateCommand); edit `MainWindow.xaml` (ComboBox-in-Grid with overlay placeholder + section card toolbar restructure with SectionType label); edit `MainWindow.xaml.cs` (`using System.Linq;` + `TemplateComboBox_SelectionChanged` handler); add v1.18 spec + plan docs. |
| **T2** | Secondary commit: new `tests/SunoMetatagApp.Tests/MainViewModelLoadTemplateTests.cs` with T1-T8 tests (11 cases). |
| **T3** | `dotnet test` → expect 171/171 green. |
| **T4** | Dev smoke `timeout 8 dotnet run` → expect `EXIT=124`. |
| **T5** | `dotnet publish` + publish smoke → expect `EXIT=124`. |
| **T6** | USER REVIEW S1-S6. |
| **T7** | Wiki: `[[sunometatag-app]]` v1.17 → v1.18 + new subsection. |
| **T8** | RESULT packet + archive entry 39 + EXECUTION_LOG.md append + USER ACTION NEEDED for Lead closeout. |

## Files modified (T1)

- `src/SunoMetatagApp/Models/SongTemplate.cs` (new)
- `src/SunoMetatagApp/Models/SongTemplates.cs` (new)
- `src/SunoMetatagApp/Models/Section.cs` (SectionType field)
- `src/SunoMetatagApp/ViewModels/MainViewModel.cs` (BuiltInTemplates + LoadTemplateCommand)
- `src/SunoMetatagApp/MainWindow.xaml` (ComboBox with placeholder overlay + section card toolbar restructure)
- `src/SunoMetatagApp/MainWindow.xaml.cs` (using System.Linq + TemplateComboBox_SelectionChanged)
- `docs/specs/2026-05-29-suno-metatag-v1.18-song-structure-templates.md` (new)
- `docs/plans/2026-05-29-suno-metatag-v1.18-song-structure-templates.md` (new — this file)

## Files modified (T2)

- `tests/SunoMetatagApp.Tests/MainViewModelLoadTemplateTests.cs` (new — T1-T8 tests)

## Expected publish artifact delta

Non-zero `publish/SunoMetatagApp.exe` delta vs v1.17 from multiple new source files + new XAML markup + new code-behind handler. `tags.json` reflects v1.17 carry-over state (byte-identical to v1.17 publish). `prompts.json` byte-identical to all cycles since v1.9.
