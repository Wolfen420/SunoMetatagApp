# SunoMetatagApp v1.17 — Implementation Plan (B-026)

**Spec:** [`2026-05-28-suno-metatag-v1.17-tag-role-sort-order.md`](../specs/2026-05-28-suno-metatag-v1.17-tag-role-sort-order.md)
**Lead r1 verdict:** `APPROVED (PASS-WITH-NOTES)` 2026-05-28 with 2 LOW absorptions: (1) correct plan test-count forecast to 160 (not 154 from r1 §6.3 — already self-corrected in r1 §8, applied authoritatively here); (2) preserve explicit non-goal that `Filter`/`InsertTagStacked` ordering is unchanged in this slice. Both absorbed at the spec authoring step.

## Task sequence

| Task | Description |
|---|---|
| **T1** | Primary commit: stage carry-over (`TagDefinition.cs` 5-field record, `tags.json` per-entry sortOrder); edit `TagService.cs` (TagDto SortOrder nullable + LoadAll 5-arg); edit `MainWindow.xaml` (new RowDefinition Row=3 banner + pill grid Row=3 → Row=4); add v1.17 spec + plan docs. |
| **T2** | Secondary commit: S1 [Theory] (7 InlineData per-category mapping) + S2 [Fact] Genre=99 default + S3 [Fact] JSON-missing-field default. |
| **T3** | `dotnet test` → expect 160/160 green. |
| **T4** | Dev smoke `timeout 8 dotnet run` → expect `EXIT=124`. |
| **T5** | `dotnet publish` + publish smoke → expect `EXIT=124`. |
| **T6** | USER REVIEW S1-S6. |
| **T7** | Wiki: `[[sunometatag-app]]` v1.16 → v1.17 + new subsection; `[[sunometatag-tag-library]]` SortOrder field documentation + test coverage refresh. |
| **T8** | RESULT packet + archive entry 37 + EXECUTION_LOG.md append + USER ACTION NEEDED for Lead closeout. |

## Files modified (T1)

- `src/SunoMetatagApp/Models/TagDefinition.cs` (carry-over staged)
- `src/SunoMetatagApp/Resources/tags.json` (carry-over staged)
- `src/SunoMetatagApp/Services/TagService.cs` (TagDto + LoadAll 5-arg)
- `src/SunoMetatagApp/MainWindow.xaml` (RowDefinitions + new banner + pill grid Row=4)
- `docs/specs/2026-05-28-suno-metatag-v1.17-tag-role-sort-order.md` (new)
- `docs/plans/2026-05-28-suno-metatag-v1.17-tag-role-sort-order.md` (new — this file)

## Files modified (T2)

- `tests/SunoMetatagApp.Tests/TagServiceSunoaiwikiMetatagListTests.cs` (S1 + S2 added — uses existing `LoadProductionTagsJson` helper)
- `tests/SunoMetatagApp.Tests/TagServiceTests.cs` (S3 added — or new test file if not present)

## Expected publish artifact delta

Non-zero `publish/SunoMetatagApp.exe` delta vs v1.16 from the TagService + XAML changes. `tags.json` reflects the carry-over data (same as v1.16's publish artifact, since v1.16's publish already had the carry-over baked in via working tree). `prompts.json` byte-identical to all cycles since v1.9.
