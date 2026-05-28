# SunoMetatagApp v1.16 — Implementation Plan (B-SUNO-013)

**Spec:** [`2026-05-28-suno-metatag-v1.16-search-clear-control.md`](../specs/2026-05-28-suno-metatag-v1.16-search-clear-control.md)
**Lead r1 verdict:** `APPROVED (PASS-WITH-NOTES)` 2026-05-28 (re-pass verified). One LOW absorption: reconcile `SunoIconButton MinWidth=28` vs clear-button Width/Padding math. Absorbed at T1 (see spec §"Lead absorption resolved at T1").

## Task sequence

| Task | Description |
|---|---|
| **T1** | Primary commit: `MainViewModel.cs` add `ClearSearchCommand` (after `CopyPreviewCommand`); `MainWindow.xaml` add right-anchored × Button inside search Grid with visibility DataTrigger + SearchBox `Padding="10,4,32,4"` override; add v1.16 spec doc + v1.16 plan doc. |
| **T2** | Secondary commit: `MainViewModelTests.cs` add C1 (clear sets empty), C2 (preserves SelectedCategory), C3 (filter recompute round-trip). |
| **T3** | `dotnet test` → expect 151/151 green. |
| **T4** | Dev smoke `timeout 6 dotnet run --no-build` → expect `TIMEOUT_EXIT=124`. |
| **T5** | `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish` + publish smoke `timeout 6 ./publish/SunoMetatagApp.exe`. |
| **T6** | USER REVIEW S1-S6 (see spec §Validation). |
| **T7** | Wiki updates: `[[sunometatag-app]]` v1.15 → v1.16 + new subsection. |
| **T8** | RESULT packet + archive entry 35 prepend + EXECUTION_LOG.md append + USER ACTION NEEDED for Lead closeout. |

## Files modified (T1)

- `src/SunoMetatagApp/ViewModels/MainViewModel.cs` (+8 lines including 5-line comment)
- `src/SunoMetatagApp/MainWindow.xaml` (+22 lines for Button block + Padding override)
- `docs/specs/2026-05-28-suno-metatag-v1.16-search-clear-control.md` (new)
- `docs/plans/2026-05-28-suno-metatag-v1.16-search-clear-control.md` (new — this file)

## Files modified (T2)

- `tests/SunoMetatagApp.Tests/MainViewModelTests.cs` (+3 [Fact] tests)

## Expected publish artifact delta

Non-zero publish exe size delta from v1.15 (153,591,732 B). Ends the three-cycle byte-identical streak. `tags.json` / `prompts.json` byte-identical to v1.15.
