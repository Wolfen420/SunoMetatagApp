# SunoMetatagApp v1.21 — ALIAS Resolution Follow-on (B-SUNO-007c)

**Date:** 2026-05-30
**Backlog:** B-SUNO-007c (Lead-designated next planner item via v1.20 closeout target_item)
**Cycle predecessor:** v1.20 (B-028 user-defined template persistence) closeout `APPROVED (PASS)` 2026-05-30
**Lead r1 verdict on this cycle:** `APPROVED (PASS)` clean variant 2026-05-30 — no absorptions required

---

## Scope

Lift the v1.15 (B-SUNO-007b) ALIAS deferral. The 10 deferred ALIAS rows mapping short forms to existing canonical prefix-form entries become search-only aliases via a new optional `aliases: string[]` field on `TagDefinition`. Pill rendering unchanged; insertion unchanged; only the search-match check is extended.

## The 10 v1.15 ALIAS mappings

| # | Alias short form | Canonical target |
|---|---|---|
| 1 | `[Aggressive]` | `[Mood: Aggressive]` |
| 2 | `[Building Energy]` | `[Energy: Building]` |
| 3 | `[Dreamy]` | `[Atmosphere: Dreamy]` |
| 4 | `[Euphoric]` | `[Mood: Euphoric]` |
| 5 | `[Explosive]` | `[Energy: Explosive]` |
| 6 | `[High Energy]` | `[Mood: High Energy]` |
| 7 | `[Melancholic]` | `[Mood: Melancholic]` |
| 8 | `[Melismatic]` | `[Melisma]` |
| 9 | `[Nostalgic]` | `[Mood: Nostalgic]` |
| 10 | `[Romantic]` | `[Mood: Romantic]` |

## Contract

### Schema

`TagDefinition` gains a 6th positional parameter:

```csharp
public sealed record TagDefinition(
    string Category,
    string Label,
    string Bracket,
    string? Description = null,
    int SortOrder = 99,
    IReadOnlyList<string>? Aliases = null);
```

`Aliases = null` is the default — preserves existing 3-param and 5-param constructor call sites.

`TagDto` (in `TagService.cs`) gains:

```csharp
[JsonPropertyName("aliases")] public List<string>? Aliases { get; set; }
```

Missing JSON field → null → treated as no aliases for search-match purposes.

### Search-match extension

`TagService.Filter.searchMatches` extends from 2-tier (Label, Bracket) to 3-tier (Label, Bracket, **Aliases**):

```csharp
if (NormalizeForSearch(t.Label).Contains(normalizedSearch, ...)
    || NormalizeForSearch(t.Bracket).Contains(normalizedSearch, ...))
    return true;
var aliases = t.Aliases ?? Array.Empty<string>();
for (int i = 0; i < aliases.Count; i++)
{
    if (NormalizeForSearch(aliases[i]).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
        return true;
}
return false;
```

Aliases use the **same `NormalizeForSearch` hyphen/space-strip** as Label and Bracket (consistent with v1.7 B-SUNO-009 contract). `Array.Empty<string>()` is a zero-allocation singleton.

### Pill list

The v1.11 alphabetical ordering by canonical `Bracket` is preserved. Alias matches are an additional inclusion path — the canonical entry appears at most once in the result regardless of which match path triggered.

## Data updates (`Resources/tags.json`)

10 canonical entries gain `"aliases": ["[<short form>]"]`:

- Line 56: `[Melisma]` → `["[Melismatic]"]`
- Line 112: `[Mood: Euphoric]` → `["[Euphoric]"]`
- Line 113: `[Mood: Melancholic]` → `["[Melancholic]"]`
- Line 114: `[Mood: Aggressive]` → `["[Aggressive]"]`
- Line 115: `[Mood: Nostalgic]` → `["[Nostalgic]"]`
- Line 118: `[Mood: Romantic]` → `["[Romantic]"]`
- Line 119: `[Mood: High Energy]` → `["[High Energy]"]`
- Line 121: `[Atmosphere: Dreamy]` → `["[Dreamy]"]`
- Line 124: `[Energy: Explosive]` → `["[Explosive]"]`
- Line 125: `[Energy: Building]` → `["[Building Energy]"]`

All other entries unchanged (no `aliases` field; deserializes to null).

## Validation

### Test coverage added (v1.21 T2)

- `tests/SunoMetatagApp.Tests/TagServiceAliasFilterTests.cs` — A1-A8 covering: alias short-form match, normalization consistency, null/empty aliases default, all 10 v1.15 mappings findable, alphabetical sort preservation, no duplicates from alias match, LoadAll populates Aliases correctly from JSON.

Anticipated test count: 199 → ~207-209.

### Smoke

- Dev smoke + publish smoke: expected `EXIT=124` both.
- Publish exe: expected small positive delta from new IL + ~200-400 bytes tags.json growth.
- USER REVIEW S1-S6 per plan §6.4.

## Explicit non-changes

- `MainViewModel.InsertTag` / `InsertTagStacked` — all v1.19 §3.x logic byte-unchanged.
- `TagService.Filter` v1.11 alphabetical pill-LIST ordering preserved exactly.
- `TagDefinition` 3-param / 5-param constructor call sites preserved via positional defaults.
- No UI changes (no `MainWindow.xaml` / `MainWindow.xaml.cs` / `Themes/*` touches).
- No PreviewBuilder / Copy path changes — typed-in-lyrics short forms NOT rewritten.
- v1.19 SortOrder lookup for typed short forms NOT extended to consult aliases (documented future-cycle candidate).
- `prompts.json` / `PromptService` / `PromptDefinition` — unchanged.
- `Models/Section.cs` / `Models/SongTemplate.cs` / `Models/SongTemplates.cs` / `Models/UserTemplateDto.cs` / `Services/UserTemplateService.cs` — v1.18 + v1.20 template surface preserved.
- README.md carry-over — separate docs cycle.

## Acceptance

- v1.15 ALIAS deferral lifted for all 10 deferred rows.
- Data-driven extensibility: future ALIAS additions need only edit `tags.json`.
- All 199 v1.20 tests + ~7-8 new A tests green.
- All v1.20-and-prior contracts preserved.
