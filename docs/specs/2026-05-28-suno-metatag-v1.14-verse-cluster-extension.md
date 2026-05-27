# SunoMetatagApp v1.14 — Add `[Verse 3]` through `[Verse 6]` Structure Tags — Spec

**Date:** 2026-05-28
**Slice:** B-SUNO-015 / v1.14 (Medium priority)
**Scope:** 4-entry data addition in `tags.json`. Zero code/behavior change.

## 1. Problem

`tags.json` currently includes `[Verse]`, `[Verse 1]`, and `[Verse 2]` as Structure-category entries (3-member Verse cluster). User request 2026-05-27 expanded the cluster to include `[Verse 3]`, `[Verse 4]`, `[Verse 5]`, `[Verse 6]` for songs with longer verse structures.

User request acceptance language: *"`tags.json` includes `[Verse 3]`, `[Verse 4]`, `[Verse 5]`, and `[Verse 6]` in the Structure category, with picker visibility and insertion behavior matching existing `[Verse]`, `[Verse 1]`, and `[Verse 2]` tags."*

## 2. Mechanism

Single source edit in `src/SunoMetatagApp/Resources/tags.json` — insert 4 new entries between existing `[Verse 2]` (line 5) and `[Pre-Chorus]` (line 6) entries, matching the existing 4-field flat schema:

```json
{ "category": "Structure", "label": "Verse 3",          "bracket": "[Verse 3]" },
{ "category": "Structure", "label": "Verse 4",          "bracket": "[Verse 4]" },
{ "category": "Structure", "label": "Verse 5",          "bracket": "[Verse 5]" },
{ "category": "Structure", "label": "Verse 6",          "bracket": "[Verse 6]" },
```

Source order doesn't affect picker rendering (v1.11 alphabetical sort overrides) but keeping the Verse cluster contiguous in source improves readability for future maintenance.

## 3. Picker render order after v1.14

Per v1.11 `OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase)`, the Verse cluster renders in lexical order:

```
[Verse 1]
[Verse 2]
[Verse 3]
[Verse 4]
[Verse 5]
[Verse 6]
[Verse]
```

Bare `[Verse]` sorts AFTER all `[Verse <number>]` entries because space (`0x20`) < `]` (`0x5D`) in ordinal comparison. This is the documented v1.11 lexical-not-numeric behavior; v1.14 extends the visible cluster from 3 to 7 entries but doesn't change the sort contract.

## 4. Counts after v1.14

| Metric | v1.13 baseline | v1.14 target | Delta |
|---|---|---|---|
| Total `tags.json` entries | 331 | 335 | +4 |
| Structure-category entries | 34 | 38 | +4 |
| Verse-cluster size (entries starting `[Verse`) | 3 | 7 | +4 |

## 5. Test additions

Test addition in `tests/SunoMetatagApp.Tests/TagServiceSunoaiwikiMetatagListTests.cs`:

- **H7** `[Theory]` `H7_ExtendedVerseCluster_PresentInStructure` with 4 `[InlineData]` cases asserting `[Verse 3]`, `[Verse 4]`, `[Verse 5]`, `[Verse 6]` are present in the Structure category via `LoadProductionTagsJson()`. Mirrors the H6 pattern. Naming continues the H1-H6 series per Lead absorption #2 at r1.

Test count forecast: 132 → 136 (one [Theory] with 4 cases counts as 4 xUnit test executions).

## 6. Parity boundaries (what does NOT change)

- `TagDefinition` 4-field record unchanged.
- `TagService.LoadAll` / `DistinctCategories` / `Filter` unchanged.
- `prompts.json` byte-identical to v1.13 (zero references to `[Verse 3]`-`[Verse 6]` confirmed via grep).
- `MainViewModel` unchanged.
- v1.7 search normalization, v1.10 picker-pane focus preservation, v1.11 alphabetical ordering, v1.12 chip-pill colors, v1.13 default-category-Structure — all preserved.
- v1.11 "Pill ordering" wiki documentation remains accurate; v1.14 wiki updates extend the cluster-size narrative without modifying the lexical-vs-numeric caveat.

## 7. Validation

USER REVIEW S1-S6 (see r1 plan packet §7.2):
- S1 (primary): new entries visible on app open per v1.13 default-category-Structure
- S2: insert behavior parity for `[Verse 3]`
- S3: v1.3 stacked-syntax regression-gate
- S4: v1.7 search normalization composition
- S5: v1.10 picker-pane focus regression-gate
- S6: v1.13 default-category regression-gate

## 8. Rollback

Two-commit revert: `git revert <T2-sha> <T1-sha>`. tags.json returns to 331 entries. Tests return to 132.

## 9. Related

- `[[sunometatag-tag-library]]` — architecture page; v1.14 wiki updates refresh Structure count (34 → 38), total count (331 → 335), Verse-cluster size (3 → 7), and H7 entry in test-coverage list. Lead absorption #1 directs FULL refresh of all Verse-cluster/count references.
- `[[sunometatag-app]]` — feature page; v1.14 subsection lands at T7.
- v1.11 "Pill ordering" subsection — referenced unchanged; documented caveat still accurate.
