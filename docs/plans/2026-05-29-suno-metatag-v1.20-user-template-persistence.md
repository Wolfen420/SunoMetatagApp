# Implementation Plan — SunoMetatagApp v1.20 — User-Defined Template Persistence (B-028)

**Date:** 2026-05-29
**Spec:** `docs/specs/2026-05-29-suno-metatag-v1.20-user-template-persistence.md`
**Cycle predecessor:** v1.19 closeout `APPROVED (PASS)` 2026-05-29
**Lead r1 verdict on this cycle:** `APPROVED (PASS-WITH-NOTES)` 2026-05-29 with 4 required absorptions (all resolved at T1/T2)

---

## Task sequence

### T1 — Primary implementation commit

Files created:
- `src/SunoMetatagApp/Models/UserTemplateDto.cs`
- `src/SunoMetatagApp/Services/UserTemplateService.cs`
- `src/SunoMetatagApp/ViewModels/TemplateListItem.cs`
- `src/SunoMetatagApp/Views/BindingProxy.cs` (Lead absorption #2)
- `src/SunoMetatagApp/Views/TemplateNameDialog.xaml` + `.xaml.cs`

Files extended:
- `src/SunoMetatagApp/ViewModels/MainViewModel.cs` — UserTemplates, Templates, RebuildTemplatesCollection, SaveCurrentAsTemplateCommand, DeleteUserTemplateCommand; new optional `UserTemplateService?` constructor parameter; LoadTemplate signature unchanged (Lead absorption #3).
- `src/SunoMetatagApp/MainWindow.xaml` — VmProxy + TemplatesView resources; restructured DockPanel with SaveTemplateButton + ComboBox; GroupStyle + ItemTemplate with conditional × delete affordance via DataTrigger.
- `src/SunoMetatagApp/MainWindow.xaml.cs` — SaveTemplateButton_Click, DeleteUserTemplateButton_PreviewMouseLeftButtonDown (Lead absorption #4), TemplateComboBox_SelectionChanged unwrap (Lead absorption #3).

Docs:
- `docs/specs/2026-05-29-suno-metatag-v1.20-user-template-persistence.md`
- `docs/plans/2026-05-29-suno-metatag-v1.20-user-template-persistence.md`

### T2 — Test fixtures commit

- `tests/SunoMetatagApp.Tests/UserTemplateServiceTests.cs` — V1-V8 service layer.
- `tests/SunoMetatagApp.Tests/MainViewModelSaveDeleteTemplateTests.cs` — W1-W10 VM layer.
- `tests/SunoMetatagApp.Tests/MainViewModelLoadTemplateTests.cs` — updated `CreateVm()` helper for temp-path UserTemplateService injection (Lead absorption #3 hygiene).

### T3 — Build + tests

`dotnet test tests/SunoMetatagApp.Tests` → expect 199/199 green.

### T4 — Dev smoke

`timeout 8 dotnet run --project src/SunoMetatagApp --no-build` → expect `EXIT=124`.

### T5 — Publish + smoke

`dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish` → exe expected non-zero delta from v1.19 153,599,924 B (new files = new IL + BAML).
`timeout 8 ./publish/SunoMetatagApp.exe` → expect `EXIT=124`.

### T6 — USER REVIEW S1-S6

Per spec acceptance:
- S1: Save user template (primary)
- S2: Load user template
- S3: Delete user template (verifies × works + selection guard)
- S4: Built-in templates retain read-only semantics (no × on built-ins)
- S5: Persistence across app restart
- S6: Multi-cycle regression-gates intact

### T7 — Wiki updates

- `[[sunometatag-app]]` v1.19 → v1.20 title bump + new subsection
- new `[[sunometatag-user-templates]]` architecture page
- `[[ai-plan-archive]]` archive entry 43 (v1.20 r1 plan packet)

### T8 — RESULT packet

Replace `ai/PLAN.md` with v1.20 RESULT packet; append `ai/EXECUTION_LOG.md` v1.20 entry; surface `USER ACTION NEEDED` for Lead closeout routing.
