# SunoMetatagApp v1.8 — Spec (B-SUNO-008a Prompt Library Mechanism + Seed)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-008a / v1.8 — Prompt library data model + service + UI surface + 16-prompt seed corpus
- **Type:** **Mechanism-first slice** (first major feature addition since v1.3 stacked-syntax B-SUNO-004; first new data model since original v1)
- **Decision packet authority:** Lead-ratified `D-2026-05-27-B-SUNO-008-scope-phasing` Option A (phased: v1.8 mechanism + small seed; full ~136-prompt curation deferred to B-SUNO-008b in v1.9+)
- **Working baseline:** `master` tip `df367ea` (v1.7 closeout)
- **Specialist activation forecast:** FRONTEND/UX (UI surface design + insert semantics); ENGINE out of scope (deterministic JSON load; no concurrency/persistence/generation-semantic implications)
- **Source-of-truth:** `https://github.com/naqashmunir21/awesome-suno-prompts` (license CC0-1.0; default branch `main`; 8 genre files in `prompts/` directory)

## 1. Goal

Ship a **bounded mechanism-introduction slice** for the curated pre-made prompt library:

1. New `PromptDefinition` record with the fields needed by awesome-suno-prompts source structure plus forward-compat extension fields.
2. New `prompts.json` resource file (parallel discipline to `tags.json`); ~16 hand-picked seed entries (2 per genre × 8 genres).
3. New `PromptService` (load + filter; mirrors `TagService` patterns).
4. New `PromptBrowserPane` UI surface — right-pane collapsible side panel (USER-confirmed UI placement per pre-spec brainstorm 2026-05-27).
5. Copy-to-clipboard insertion as the primary v1.8 path; multi-section insertion deferred to v1.9.
6. Inline + footer attribution to the source repo (CC0; credit anyway per project hygiene).
7. Tests P1-P7 (mirrors v1.6 H-style + v1.7 N-style content coverage).
8. USER REVIEW S1-S8 covering toggle, browse, copy, attribution link, and regression of existing v1.7 tag picker.

**Resolves backlog item:** B-SUNO-008 (mechanism + seed only; full ~136-prompt curation continues in B-SUNO-008b at v1.9+).

## 2. Scope

### What this slice covers

- **New data model:** `PromptDefinition` record (described in §3.1).
- **New resource file:** `Resources/prompts.json` with 16 entries (seed corpus; §3.4).
- **New service:** `Services/PromptService.cs` (load + filter; §3.2).
- **New UI surface:** `Views/PromptBrowserPane.xaml` (+ code-behind) docked right of preview pane; toggle via toolbar button (§3.3).
- **Toolbar / `MainWindow.xaml` wiring:** add toggle button + bind to `MainViewModel.IsPromptBrowserVisible`.
- **New ViewModel binding:** `MainViewModel.Prompts` (filtered) + `IsPromptBrowserVisible` (bool toggle) + selected-prompt state for inline expand.
- **Copy-to-clipboard action:** `PromptViewModel.CopyCommand` (or equivalent) that puts `Body` on `Clipboard.SetText`.
- **Attribution surface:** inline footer in `PromptBrowserPane` ("Prompts from awesome-suno-prompts (CC0)" + clickable link) + optional `SourceUrl` field on each `PromptDefinition`.
- **Tests:** new `tests/SunoMetatagApp.Tests/PromptServiceTests.cs` with P1-P7 (§6.1).
- **Documentation:** this spec + plan in `j:\SunoMetatagApp\docs\`. New wiki page `[[sunometatag-prompt-library]]` (architecture); bump `[[sunometatag-app]]` v1.7 → v1.8.

### What this slice explicitly does NOT cover

- **No full corpus curation.** ~120-200+ prompts in the source repo remain unimported; B-SUNO-008b at v1.9+ runs the full decision table.
- **No insert-as-section-set action.** Multi-section auto-creation from prompt structure is deferred to v1.9 informed by user signal.
- **No append-to-active-section action.** Pasted content goes wherever user pastes; no targeted insertion in v1.8.
- **No prompt search/filter beyond genre.** Free-text search inside prompt bodies is deferred to v1.9+ if user friction surfaces.
- **No prompt-to-tag cross-referencing.** Stacked-syntax detection inside prompt bodies for tag highlighting deferred.
- **No PromptDefinition schema additions beyond forward-compat `Tags` + `Difficulty`.** Other fields can be added in v1.9 if curation reveals need.
- **No B-SUNO-010 / B-SUNO-011 / B-SUNO-012 / B-SUNO-013 work** (queued separately; B-SUNO-012 High priority remains Lead-discretion to schedule).
- **No v1.2/v1.3 carry-over reconciliation** (B-026 / B-027 / B-028 / B-SUNO-NNN / `decisions/suno-visual-language.md:6` mojibake / `×` glyph — 10+ cycles unaddressed, Lead-discretion).
- **No `ai/REVIEW.md` retention-cap cleanup** (at 20 entries; Lead-owned).
- **No changes to `tags.json` or existing tag picker** (regression S7 in USER REVIEW confirms preservation).

## 3. Approach (Mechanism)

### 3.1 `PromptDefinition` record

New file: `src/SunoMetatagApp/Models/PromptDefinition.cs`.

```csharp
namespace SunoMetatagApp.Models;

public sealed record PromptDefinition(
    string Genre,                   // Required: "Pop" | "Rock" | "EDM" | "Hip-Hop" | "Indie" | "Jazz-Blues" | "R&B-Soul" | "Country"
    string SubGenre,                // Required: e.g., "Upbeat Dance Pop", "Stadium Rock"
    string Title,                   // Required: e.g., "Modern Pop Anthem (Female Vocals)"
    string Body,                    // Required: multi-line prompt text (copy target)
    string? UseCase = null,         // Optional: e.g., "TikTok viral hits, summer anthems"
    string? SunoVersion = null,     // Optional: "V5" | "V4.5" | "Both"
    int? Energy = null,             // Optional: 0-10 rating
    string? NotableFeature = null,  // Optional: free-text annotation
    string? SourceUrl = null,       // Optional: link back to anchor in awesome-suno-prompts
    string[]? Tags = null,          // Forward-compat (NOT exercised in v1.8 seed)
    string? Difficulty = null       // Forward-compat (NOT exercised in v1.8 seed)
);
```

Forward-compat fields (`Tags`, `Difficulty`) carry the specialist advisory recommendation to plan for v1.9 expansion without schema churn. v1.8 seed entries leave these `null`; the schema accepts them so v1.9 entries can populate without a re-import.

### 3.2 `PromptService` API

New file: `src/SunoMetatagApp/Services/PromptService.cs`.

Mirrors `TagService` shape:

```csharp
public sealed class PromptLoadException : Exception { ... }

public static class PromptService
{
    public static IReadOnlyList<PromptDefinition> LoadAll(string path) { ... }

    public static IReadOnlyList<string> DistinctGenres(IEnumerable<PromptDefinition> prompts) { ... }

    public static IEnumerable<PromptDefinition> Filter(
        IEnumerable<PromptDefinition> prompts,
        string? genre) { ... }
}
```

Filter takes a single nullable `genre` argument (not a free-text search) — v1.8 surfaces only genre-filter; free-text search inside prompt bodies deferred to v1.9+.

### 3.3 `PromptBrowserPane` UI surface

New file: `src/SunoMetatagApp/Views/PromptBrowserPane.xaml` (+ code-behind).

**Placement:** Docked right-of-preview-pane in `MainWindow.xaml` as a `<Grid.Column>` cell. Width default ~280 DIPs; collapsible via `Visibility="{Binding IsPromptBrowserVisible, Converter=...}"`.

**Pane content (top-down):**

1. **Header bar** with title text "Prompts" + collapse `×` button.
2. **Genre filter `ComboBox`** populated from `PromptService.DistinctGenres` (8 entries + "All" sentinel; same pattern as tag-picker).
3. **Scrollable prompt `ListView`** bound to `MainViewModel.Prompts` (filtered). Each row shows:
   - **Title** (primary text)
   - **SubGenre** (secondary text, smaller)
   - **Energy badge** if `Energy` not null (e.g., "9/10")
4. **Expand-on-click reveal:** clicking a row expands inline (or selects + populates a footer panel) to show:
   - **Body** (read-only, monospace, multi-line `TextBox`)
   - **UseCase** (if present)
   - **SunoVersion** (if present)
   - **NotableFeature** (if present)
   - **"Copy" button** — primary action; copies `Body` to clipboard
5. **Attribution footer:** small text "Prompts from [awesome-suno-prompts](https://github.com/naqashmunir21/awesome-suno-prompts) (CC0)" with clickable hyperlink.

**Toggle wiring in `MainWindow.xaml`:**

- Add toolbar button (icon: bookmark/scroll glyph) bound to a `RelayCommand` that flips `MainViewModel.IsPromptBrowserVisible`.
- Default state: hidden (do not expand on first launch; existing v1.7 layout preserved for upgraders).

**Visual consistency:** All visual elements use existing v1.2-v1.7 theme tokens (dark background, accent colors, typography); no new theme tokens introduced.

### 3.4 Seed corpus (16 entries)

**Selection criteria** (planner picks at T1 by reading each `prompts/<genre>.md` table of contents):

- **2 entries per genre × 8 genres = 16 total.**
- **Per genre:** 1 high-energy / archetypal anchor (Energy 8-10) + 1 lower-energy / ballad/chill anchor (Energy ≤ 6 or "builds").
- **Quality bias:** prefer entries with explicit `BPM` + `Key` annotation in the body (better Suno utility); prefer entries with `Use Case` field present (more illustrative).
- **Diversity bias:** avoid 2 entries from the same SubGenre within the same Genre.
- **No T1-time discovery exception:** if a chosen entry's body or metadata is malformed, planner substitutes the next-best entry from the same SubGenre + documents in commit message.

**Per-genre seed forecast** (exact titles selected at T1):

| Genre | Source file | High-energy pick (rep.) | Lower-energy pick (rep.) |
|---|---|---|---|
| Pop | `prompts/pop.md` | "Modern Pop Anthem (Female Vocals)" | (TBD T1 from Emotional Ballads section) |
| Rock | `prompts/rock.md` | "Epic Arena Anthem" | "Power Ballad Rock" |
| EDM | `prompts/edm.md` | (TBD T1) | (TBD T1) |
| Hip-Hop | `prompts/hip-hop.md` | (TBD T1) | (TBD T1) |
| Indie | `prompts/indie.md` | (TBD T1) | (TBD T1) |
| Jazz-Blues | `prompts/jazz-blues.md` | (TBD T1) | (TBD T1) |
| R&B-Soul | `prompts/rnb-soul.md` | (TBD T1) | (TBD T1) |
| Country | `prompts/country.md` | (TBD T1) | (TBD T1) |

**Why TBD at spec time:** planner-default deferred to T1 execution after reading the actual TOC + body of each `.md` file in turn. Exact titles do not affect mechanism correctness; selection criteria are the spec-bound commitment.

### 3.5 Source-of-truth capture (immutable evidence)

Per v1.4-v1.6 import-as-source discipline, planner captures a snapshot of the awesome-suno-prompts repo state used for selection:

- **New file:** `docs/reference/awesome-suno-prompts-source-2026-05-27.md` (immutable WebFetch/gh-API capture).
- **Capture contents:**
  - Repo metadata: license (CC0-1.0), default branch (`main`), commit SHA at fetch time
  - File listing of `prompts/` (8 genre files with sizes)
  - For each of the 16 selected seed prompts: source-anchor URL + raw body verbatim
- **License compliance note:** CC0 dedication = no attribution legally required, but the file documents the project's voluntary credit policy.

### 3.6 Attribution policy

- **Inline (per-prompt):** optional `SourceUrl` field linking to anchor (e.g., `https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/pop.md#modern-pop-anthem-female-vocals`).
- **App-level:** attribution footer in `PromptBrowserPane` with clickable hyperlink to source repo.
- **No App About dialog change in v1.8.** A future v1.9+ may add a dedicated "Attribution" or "Credits" pane; deferred.

## 4. Risks

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| **R1** | Scope creep at T1 — discovery of prompt-body fields not in `PromptDefinition` (e.g., source mentions "Key" + "BPM" inline in body; v1.8 doesn't parse). | Low | `Body` field stores the full prompt text verbatim. No parsing required in v1.8. Future v1.9+ may add `KeyValue` or `BpmValue` columns if user friction surfaces. |
| **R2** | UI real-estate impact — adding a ~280-DIP collapsible right panel may compress preview pane on smaller screens. | Low | Panel default-hidden; user toggles via toolbar button. Existing v1.7 UI default-state is unchanged for upgraders. Visual consistency check at USER REVIEW S8. |
| **R3** | Clipboard side effects — `Clipboard.SetText` invocation may fail on rare edge cases (clipboard locked by another process). | Low | Wrap in try-catch; surface `Status: "Copy failed"` to user. Standard WPF clipboard pattern. |
| **R4** | Forward-compat fields (`Tags`, `Difficulty`) included but not tested in v1.8 — test for tolerance only. | Informational | P7 [Fact] tests null-tolerance. v1.9 entries populating these fields will trigger new tests at curation time. |
| **R5** | Attribution interpretation — CC0 license doesn't require credit but project adds it voluntarily. User may want different surface (e.g., About dialog only). | Low | Two surfaces (inline + footer link) deliberately small; T7 USER REVIEW S6 verifies footer visibility + link works. Lead may override at r1 to single-surface only. |
| **R6** | Cross-references in source repo (e.g., "→ Custom Prompts" links to songaifarm.com) — these are commercial promotions. Need to ensure v1.8 does NOT propagate. | Low | Selection criteria pick prompts whose Body sections do not embed commercial links. `Body` field stores verbatim prompt text only (no surrounding markdown). T1 planner check + commit-message rationale. |
| **R7** | Lead may override Option A→B at r1 reading this spec. | Low | This spec is bounded to Option A scope per `aiDECISION.json` ratification. Lead override would require new decision packet. Specialist activation forecast assumes Option A; if Lead override, planner pivots to Option B-shaped r1. |

## 5. Test Plan

### 5.1 Automated content-coverage tests (NEW: P1-P7)

New test file: `tests/SunoMetatagApp.Tests/PromptServiceTests.cs`.

| Test | Assertion | Notes |
|---|---|---|
| **P1** | `PromptService.LoadAll("prompts.json")` returns 16 entries | Sanity load + count |
| **P2** | `DistinctGenres` returns 8 genres (Pop / Rock / EDM / Hip-Hop / Indie / Jazz-Blues / R&B-Soul / Country) | Genre coverage |
| **P3** | Each genre has exactly 2 entries (2 per genre × 8 = 16) | 2-per-genre distribution |
| **P4** | All 16 entries have unique `Title` (no Title collisions) | Defense against duplicate-import bugs |
| **P5** ([Theory], 8 inline rows) | Per-genre: ≥1 entry has `Energy >= 7` (high-energy anchor) AND ≥1 entry has `Energy <= 6` or `null` (ballad/chill anchor) | Selection criteria validation |
| **P6** | All 16 entries have non-empty `Body` field | Body field required |
| **P7** | Forward-compat fields (`Tags`, `Difficulty`) tolerated as `null` in loaded entries (no exception thrown) | Forward-compat schema test |

Expected automated total: **111 v1.7 baseline + 4 [Fact] (P1/P3/P4/P6) + 8 P5 [Theory] rows + 1 [Fact] (P7) = 124 expected.**

Note: P2 is [Fact] returning count of 8; if implemented as a single assertion = 1 test. If split per-genre = more.

### 5.2 USER REVIEW manual smoke matrix (S1-S8)

| # | Step | Expected outcome |
|---|---|---|
| **S1** | Launch `publish/SunoMetatagApp.exe`; verify default state: prompt browser hidden | Existing v1.7 layout preserved; no prompt browser visible |
| **S2** | Click "Prompts" toolbar button | Right-pane collapsible side panel slides into view; genre dropdown shows "All" + 8 genre options; prompt list shows 16 entries |
| **S3** | Genre filter dropdown → select "Pop" | List filters to 2 Pop entries |
| **S4** | Click a prompt row | Inline expand reveals Body + UseCase + SunoVersion + NotableFeature + "Copy" button |
| **S5** | Click "Copy" → switch to a text editor (Notepad / VSCode) → paste | Pasted text matches prompt Body verbatim |
| **S6** | Click attribution footer link | Default browser opens `https://github.com/naqashmunir21/awesome-suno-prompts` |
| **S7** | Close prompt browser; verify tag picker still works (search `kpop` → `[K-Pop]` surfaces) | Existing v1.7 search normalization preserved (regression check) |
| **S8** | Visual consistency: prompt browser uses v1.7 dark theme (background, accents, fonts) | No visual incongruity vs existing panes |

PASS criterion: **8/8 PASS** first try. Mechanism-only scope means low likelihood of USER REVIEW concerns; UI surface decisions are USER-confirmed before this spec landed.

### 5.3 Rollback plan

Single-commit revert. v1.8 ships in **2-3 commits** on `master`:
- Primary: `prompts.json` + `PromptDefinition` model + `PromptService` + spec + plan + source-capture doc
- Secondary: `PromptBrowserPane.xaml` + `.cs` + `MainWindow.xaml` toggle wiring + `MainViewModel.cs` binding additions
- Tertiary: `PromptServiceTests.cs` (P1-P7)

`git revert <primary> <secondary> <tertiary>` restores v1.7 closeout tip `df367ea` cleanly. New `Resources/prompts.json` removal also disposes seed data.

## 6. Implementation Surfaces Touched

### 6.1 Source files (NEW)

- `src/SunoMetatagApp/Models/PromptDefinition.cs`
- `src/SunoMetatagApp/Services/PromptService.cs`
- `src/SunoMetatagApp/Views/PromptBrowserPane.xaml` (+ `.xaml.cs`)
- `src/SunoMetatagApp/Resources/prompts.json`

### 6.2 Source files (MODIFIED)

- `src/SunoMetatagApp/MainWindow.xaml` — add right-pane column + toolbar toggle button
- `src/SunoMetatagApp/ViewModels/MainViewModel.cs` — add `IsPromptBrowserVisible`, `Prompts`, `SelectedPrompt` properties + toggle command + copy command
- `src/SunoMetatagApp/App.xaml.cs` — wire `PromptService.LoadAll(promptsPath)` at startup (parallel to `TagService.LoadAll`)
- `src/SunoMetatagApp/SunoMetatagApp.csproj` — add `prompts.json` `CopyToOutput` entry

### 6.3 Test files (NEW)

- `tests/SunoMetatagApp.Tests/PromptServiceTests.cs` (P1-P7)

### 6.4 Documentation files (NEW)

- `docs/specs/2026-05-27-suno-metatag-v1.8-prompt-library-mechanism.md` — this spec
- `docs/plans/2026-05-27-suno-metatag-v1.8-prompt-library-mechanism.md` — implementation plan
- `docs/reference/awesome-suno-prompts-source-2026-05-27.md` — immutable source capture

### 6.5 Wiki updates (`j:\SunoSongSetup\.SunoSongSetup-wiki\`)

- `wiki/features/sunometatag-app.md` — title bump v1.7 → v1.8; new "v1.7 → v1.8 (2026-05-27)" subsection; `last_confirmed: 2026-05-27`; sources extended
- `wiki/architecture/sunometatag-prompt-library.md` — NEW architecture page (parallel to `[[sunometatag-tag-library]]`)
- `wiki/reference/ai-plan-archive.md` — Archive entry 18 prepended at r1 draft (v1.7 RESULT body)

### 6.6 Surfaces explicitly NOT touched

- `src/SunoMetatagApp/Resources/tags.json` (zero changes; 331 entries unchanged)
- `src/SunoMetatagApp/Services/TagService.cs` (v1.7 search normalization preserved)
- `src/SunoMetatagApp/Models/TagDefinition.cs` (schema unchanged)
- Existing v1.2-v1.7 visual theme tokens (no new theme tokens; existing tokens reused)

## 7. Wiki update commitment (closeout)

Closeout will declare:

```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-prompt-library]], [[ai-plan-archive]]
```

All landed in-cycle; no queued-exception declarations. `wiki_sync_status: PASS` expected.

## 8. Open Decisions for Lead Ratification

| # | Decision | Planner default | §ref |
|---|---|---|---|
| **Q1** | UI placement = right-pane collapsible side panel? | YES (USER-confirmed via Visual Companion mockup picker 2026-05-27) | §3.3 |
| **Q2** | Copy-to-clipboard as the only v1.8 insert path? | YES (insert-as-section-set + append-to-active deferred to v1.9) | §1, §3.3 |
| **Q3** | Schema = separate `prompts.json` (not extending `tags.json`)? | YES (single-responsibility per resource; matches `TagDefinition` discipline) | §3.1 |
| **Q4** | Seed = 16 prompts (2 per genre × 8 genres)? | YES — matches specialist advisory recommendation | §3.4 |
| **Q5** | Forward-compat fields (`Tags`, `Difficulty`) in v1.8 schema? | YES — per specialist advisory; null-tolerated in seed; P7 tests | §3.1, §6.1 |
| **Q6** | Attribution = inline `SourceUrl` + footer link both surfaces? | YES (CC0 = optional, but project adds voluntary credit on both surfaces) | §3.6 |
| **Q7** | Default state of prompt browser pane = hidden? | YES (preserves v1.7 layout for upgraders; user discovers via toolbar) | §3.3 |
| **Q8** | Selection criteria for 16 seed entries (high-energy + ballad/chill per genre)? | Per §3.4 selection criteria; exact titles deferred to T1 | §3.4 |

## 9. B-SUNO-008b Continuation (v1.9+ forecast)

Per Lead's decision packet `consequences[]`:
> "Full corpus curation from awesome-suno-prompts (~136 prompts) is deferred to follow-on B-SUNO-008 continuation slice with decision-table workflow."

v1.9+ B-SUNO-008b shape:
- Full ~120-200+ prompt curation across 8 genre files
- ADD/MERGE/SKIP decision table per v1.4-v1.6 precedent
- Possible schema additions if curation reveals fields needed (e.g., `BpmValue`, `KeyValue` for sort/filter)
- Possible additional UI affordances (free-text search inside prompt bodies; insert-as-section-set; append-to-active-section)
- USER REVIEW matrix scaled accordingly

Lead may also choose to sequence other backlog items between v1.8 (this slice) and v1.9 (B-SUNO-008b):
- **B-SUNO-012** (section focus restoration; **High priority**) — only High-priority open item
- B-SUNO-010 / B-SUNO-011 / B-SUNO-013 (Low/Medium UI polish items)
- v1.2/v1.3 carry-overs (B-026 / B-027 / B-028 / B-SUNO-NNN / `decisions/suno-visual-language.md:6` mojibake / `×` glyph)

## 10. Conclusion

v1.8 is a **mechanism-introduction slice** — first new data model since v1 original section editor; first new UI surface since v1.2 visual redesign; first source-code slice with paired test class since v1.7. Per Lead's `D-2026-05-27-B-SUNO-008-scope-phasing` Option A ratification, v1.8 ships the prompt library mechanism + a small 16-prompt seed that validates end-to-end without committing to full ~136-prompt curation cost. Visual Companion mockup workflow USER-confirmed the right-pane collapsible side panel placement pre-spec.

Risk profile is bounded by Option A discipline: tiny seed, single UI surface, no insert-semantic experimentation. The pattern mirrors v1.3 stacked-syntax discipline (mechanism-first, content-second).

**Expected closeout:** APPROVED (PASS or PASS-WITH-NOTES); USER REVIEW S1-S8 8/8 PASS forecast; wiki_sync_status: PASS.
