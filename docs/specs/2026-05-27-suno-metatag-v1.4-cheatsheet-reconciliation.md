# SunoMetatagApp v1.4 — Cheat-Sheet vs tags.json Reconciliation

**Status:** draft (B-SUNO-005 r1)
**Date:** 2026-05-27
**Behavioral version impact:** v1.4 (content-only release; **no** code, theme, focus-model, or behavioral changes).
**Source-of-truth cheat sheet:** [`docs/reference/suno-cheat-sheet-2026-05-26.md`](../reference/suno-cheat-sheet-2026-05-26.md) (user-pasted Google Doc content 2026-05-27).
**Source-of-truth baseline:** [`src/SunoMetatagApp/Resources/tags.json`](../../src/SunoMetatagApp/Resources/tags.json) — 124 entries across 6 categories (Structure / Vocal / Instrument / Mood / Effect / Production).
**Decision model:** ADD / MERGE / SKIP per cheat-sheet entry (user-chosen 2026-05-27).

## 1. Motivation

`tags.json` was hand-seeded with ~115 entries at v1 ship; current baseline is 124 entries. The user-provided Suno cheat sheet (~112 entries) contains canonical Suno-AI metatags that are not yet represented in `tags.json` — particularly in three areas:

1. **Sound effects** (~50 SFX entries): `[Beeping]`, `[Birdsong]`, `[Footsteps]`, `[Gunshot]`, `[Thunder]`, etc. tags.json has none of these.
2. **Atmosphere / mood variants**: `[Eerie Whispers]`, `[Ghostly Echoes]`, `[Ominous Drone]`, `[Spectral Melody]`, `[Tense Underscore]`. tags.json has only generic `[Mood: X]` and `[Atmosphere: X]` prefix forms.
3. **Dynamic / progression**: `[Building Intensity]`, `[Climactic]`, `[Orchestral Build]`, `[Stripped Back]`, `[Layered Arrangement]`. tags.json has only `[Swell]`, `[Crescendo]`, `[Decrescendo]`.

Adding curated subsets unblocks fuller-fidelity Suno prompt composition without forcing users to manually type bracket tokens that should be one-click pills.

## 2. Decision model

Per cheat-sheet entry, exactly one of three decisions applies:

- **ADD** — cheat-sheet entry is genuinely new to `tags.json`. Land as a new `TagDefinition` JSON object with category mapping per §3.
- **MERGE** — cheat-sheet entry has duplicate semantics with an existing `tags.json` entry. **For v1.4 the MERGE decision is a no-tags.json-mutation acknowledgement** — the existing entry remains canonical; the cheat-sheet alternative name is noted in the decision-table rationale for documentation only. Alias-as-data-schema support is deferred to **B-008** (Tag aliases / synonyms) in the v1 BACKLOG.md.
- **SKIP** — cheat-sheet entry is intentionally excluded. Reasons may include: too narrow, out-of-scope category, malformed bracket text (e.g., slash characters), or planner-default redundancy with v1.3 stacked syntax (`[Outro | Powerful]` expressed inline instead of as a new `[Powerful Outro]` entry).

The MERGE-as-no-op semantics deliberately avoid two risks:
- **Breaking renames.** Renaming `[Whispered]` (current) to `[Whisper]` (cheat sheet's spelling) would break any saved prompts referencing `[Whispered]`. v1.4 keeps existing brackets intact.
- **Schema scope creep.** Real alias support requires a `TagDefinition.Aliases` field + `TagService.Filter` lookup + search-text matching. That's a v2 concern (B-008 backlog item).

## 3. Category model

### 3.1 Existing categories (preserved)

`Structure`, `Vocal`, `Instrument`, `Mood`, `Effect`, `Production` — unchanged. All MERGE-as-no-op acknowledgements stay in their existing category; new ADDs map per §3.3.

### 3.2 New category proposed: `SFX`

**Reason:** the cheat sheet's Section D contains ~50 pure sound-effect entries (`[Beeping]`, `[Footsteps]`, `[Gunshot]`, `[Thunder]`, etc.) that:
- Don't fit `Effect` (which is for production/post effects like Reverb, Distortion).
- Don't fit `Production` (which is for engineering directives like BPM, Heavy Bass).
- Don't fit `Mood` (semantically distinct from atmospheric mood).

A new top-level `SFX` category cleanly hosts these. No code changes required — `MainViewModel.BuildCategories` already derives the category list from `TagService.DistinctCategories(tags)`, so adding a new category to `tags.json` automatically populates the ComboBox.

**Lead-ratifiable alternative:** subsume SFX under `Effect` with category rename to `Effects/SFX` or similar. Planner-default: separate `SFX` category for taxonomy clarity.

### 3.3 Category mapping rules for ADD entries

| Cheat sheet section | Canonical target category | Rationale |
|---|---|---|
| B.1 Song Structure | Structure | matches existing |
| B.2 Instrumental (solos, breaks) | Structure | existing `[Guitar Solo]` already in Structure; preserve pattern |
| B.3 Vocal | Vocal | matches existing |
| B.4 Specific Elements (composite tags) | (planner-default ADD; some go to Structure category as variants — see §4) | per-entry decision |
| B.5 Atmosphere and Mood | Mood | extends existing Mood category |
| B.6 Dynamic and Progression | Effect | matches existing `[Swell]`, `[Crescendo]`, `[Decrescendo]` Effect entries |
| C — Perfect Studio Vocals | Vocal | matches existing |
| D — Sound Effects (pure SFX) | **SFX (new)** | per §3.2 |
| D — production overlap | Effect (MERGE with existing `[Effect: *]`) | naming-convention consistency |
| D — atmosphere variants | SFX (planner-default) | `[Nighttime Atmosphere]`, `[Daytime Atmosphere]`, `[Natural Ambience]` are ambient-environment SFX rather than mood directives. Lead-ratifiable: could go to Mood instead. |

### 3.4 Naming convention reconciliation

`tags.json` currently uses **two coexisting forms**:
- **Bare brackets:** `[Verse]`, `[Whispered]`, `[Acoustic Guitar]`, `[Swell]`, `[Crescendo]`.
- **Prefixed brackets:** `[Mood: Euphoric]`, `[Atmosphere: Dreamy]`, `[Effect: Lo-fi]`, `[Energy: Building]`, `[Voice: Auto-tune]`, `[Tempo: 128 BPM]`, `[Callback: Chorus melody]`.

The cheat sheet uses **bare brackets only** — that's the canonical Suno-AI form.

**Default for new ADDs:** bare bracket form (matches Suno canonical + cheat-sheet convention).

**Exception — category-neighbor consistency:** when adding to an existing category that uniformly uses prefix form (e.g., `Effect` category where existing entries are `[Effect: Lo-fi]`, `[Effect: Reverb: Hall]`, `[Effect: Delay: Ping-pong]`, `[Effect: Distortion]`, etc.), new ADDs match the neighbor convention to avoid mixed-naming-within-category. Concrete cases in decision table §D.2: `[Flanger Effects]` cheat-sheet entry lands as `[Effect: Flanger]`; `[Vinyl Record Sounds]` lands as `[Effect: Vinyl Crackle]`. This was specialist-flagged at r1 (LOW 2 absorbed at T0) — the §3.4 default and the §D.2 ADD rows are consistent under the exception clause.

**Existing entries are NOT migrated.** `[Mood: Euphoric]`, `[Atmosphere: Dreamy]`, etc. stay verbatim — naming-convention refactor is a v2+ item.

**Three coexisting forms post-v1.4** (specialist LOW 7 documentation):
- Bare canonical: `[Verse]`, `[Birdsong]`, `[Climactic]`, `[Eerie Whispers]`
- Prefix form: `[Mood: Euphoric]`, `[Effect: Flanger]`
- Compound bare: `[Catchy Hook]`, `[Tense Underscore]`, `[Spectral Melody]`

Searchability is unaffected (`TagService.Filter` matches against `Label` text). UI presentation is unaffected (pill content is `Bracket` string verbatim). Users browsing a single category may see mixed conventions — this is the honest cost of the no-rename stance and is documented in the new [[sunometatag-tag-library]] wiki page.

**Lead-ratifiable alternative:** force all new ADDs to use prefix form OR migrate existing prefix forms to bare. Planner-default rejects both as out of v1.4 scope (migration is a separate refactor slice; uniform-prefix imposes more naming churn than the cheat sheet endorses).

## 4. Composite "Specific Elements" — ADD vs v1.3 stacked-syntax inline

Cheat sheet Section B.4 (Specific Elements, 6 entries) contains composite tags whose semantics can be expressed via v1.3 Shift+click stacked syntax:

| Cheat sheet entry | v1.3 stacked alternative | ADD as separate? |
|---|---|---|
| `[Catchy Hook]` | `[Hook \| Catchy]` (existing tags) | **ADD** — distinct enough; `[Catchy]` as standalone is not in tags.json |
| `[Emotional Bridge]` | `[Bridge \| Emotional]` | **ADD** — `[Emotional]` not standalone |
| `[Powerful Outro]` | `[Outro \| Powerful]` (both exist standalone in tags.json) | **SKIP** — express via v1.3 stacked syntax; redundant entry |
| `[Soft Intro]` | `[Intro \| Soft]` (both exist) | **SKIP** — redundant |
| `[Melodic Interlude]` | `[Interlude \| Melodic]` ([Melodic] not standalone) | **ADD** |
| `[Percussion Break]` | `[Break \| Hand Percussion]` (close match but not exact) | **ADD** |

Planner-default rationale: when both standalone parts already exist in tags.json, prefer v1.3 stacked syntax over redundant entry; when one part is new, ADD the composite for one-click access.

## 5. Decision table

The full per-entry decision table lives in [`docs/reference/B-SUNO-005-decision-table.md`](../reference/B-SUNO-005-decision-table.md) — landed as a separate file because of size (~110 rows). Summary counts:

| Decision | Count | Notes |
|---|---|---|
| ADD | 74 | Mostly Section D.1 SFX (48) + Section B.5 Atmosphere (7) + Section B.6 Dynamic (5) + Section B.2-B.4 misc (12) + Section D.2 production effects (2) |
| MERGE | 15 | No-tags.json mutation; documentation only |
| SKIP | 21 | Intentional exclusion + redundant-with-v1.3-stacked + already-exists in tags.json |

**Result:** tags.json grows from **124 → 198 entries**. Categories: 6 existing + 1 new (SFX) = **7 categories**.

The decision table is Lead-ratifiable at plan-phase review; rows are concrete enough to apply mechanically at T1 execution.

## 6. Architecture impact

### 6.1 Data-only changes

- `Resources/tags.json` — appended entries (no schema change, no removals, no renames). Preserve existing entries verbatim.
- `MainViewModel.BuildCategories` — already derives categories dynamically; no code change needed for the new `SFX` category.
- `TagService.LoadAll`, `TagService.DistinctCategories`, `TagService.Filter` — already handle arbitrary tag content; no code change needed.

### 6.2 No code changes

This is a content-only release. Zero edits to:
- `MainViewModel.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `Section.cs`, `PreviewBuilder.cs`, `TagDefinition.cs`, `TagViewModel.cs`, `TagService.cs`, `App.xaml`, `App.xaml.cs`.
- `Themes/SunoTokens.xaml`, `Themes/SunoStyles.xaml`.
- All converter files.
- All 47 existing unit tests (they pass on the new tags.json by virtue of testing behaviors, not specific tag counts).

### 6.3 No wiki schema changes

- [[sunometatag-app]] — title bump to v1.4 + new "v1.3 → v1.4" subsection documenting tag library expansion. No architecture changes.
- [[sunometatag-inline-editor]] — unchanged (behavior identical).
- **New page (proposed):** [[sunometatag-tag-library]] — describes the tag library structure (categories, conventions, the v1.4 reconciliation that landed the cheat-sheet additions). Lead-ratifiable: extend [[sunometatag-app]] instead of new page. Planner-default: new page for separation of feature record vs reference content.

## 7. Test plan

### 7.1 Existing test preservation

All 47 unit tests (31 v1 baseline + 16 v1.3 stacked-syntax) must continue to pass. None test specific tag counts; they test behavioral contracts (insert, merge, filter, etc.) — so adding tags is non-breaking.

### 7.2 New tests for v1.4

| # | Test name | Purpose |
|---|---|---|
| C1 | `TagService_LoadAll_LoadsExpectedCount` | After v1.4 reconciliation, `TagService.LoadAll(path).Count >= 198` (planner-proposed total per decision-table §Totals row; updated if Lead ratifies different decision-table rows). |
| C2 | `TagService_DistinctCategories_IncludesSFX` | New "SFX" category appears in `DistinctCategories` result when the new tags.json is loaded. |
| C3 | `TagService_Filter_FindsNewSFXTag` | Search for "Birdsong" returns the new `[Birdsong]` entry; search-text matching across new entries works. |
| C4 | `TagService_Filter_ByNewSFXCategory` | Filter `SelectedCategory = "SFX"` returns only the new SFX entries; existing Effect/Mood tags don't bleed in. |
| C5 | `TagService_LoadAll_NoBracketCollisions` | All `Bracket` strings in the new tags.json are unique (defensive — guards against accidental dup during table application). |
| C6 | `TagService_LoadAll_AllNewEntriesHaveCategory` | Defensive — no new entry has empty/null category field. |

### 7.3 Manual smoke matrix (USER REVIEW)

| # | Scenario | Pass criteria |
|---|---|---|
| S1 | Launch app; check the category dropdown | New "SFX" entry appears alongside Structure/Vocal/Instrument/Mood/Effect/Production. |
| S2 | Select "SFX" category | Picker fills with ~50 SFX pills (Beeping, Footsteps, etc.). Visual layer unchanged from v1.3. |
| S3 | Plain click + Shift+click on a new SFX tag (e.g. `[Birdsong]`) | Plain inserts `[Birdsong]` at caret (v1.1 behavior); Shift+click merges into nearest bracket on line (v1.3 behavior). All 47 tests still cover this; smoke confirms it works on new tags. |
| S4 | Search "build" with category "All" | Returns `[Building Intensity]` (new), `[Build]` (existing), `[Build-up]` (existing), `[Euphoric Build]` (new), `[Orchestral Build]` (new). |
| S5 | Search "drum" with category "All" | Returns `[Drum Solo]` (new), `[Drum Break]` (existing), `[Drum Fill]` (existing), `[Electronic Drums]` (existing), `[Percussion Break]` (new). |
| S6 | Copy preview output with mixed new + existing tags | Output renders verbatim with both new SFX and existing structure tags. |
| S7 | All v1.3 carry-over smoke (13 cases) still pass | No regression on v1.1 inline / v1.2 visual / v1.3 stacked-syntax behaviors. |
| S8 | Existing prompts saved with old tags (if any) still work | Plain insert of existing `[Mood: Euphoric]` still works — no rename impact. |

## 8. Non-goals

- **No code changes** beyond `tags.json` content (and any new unit tests in the `Tests/` project).
- **No `TagDefinition.Aliases` schema extension.** Alias support is B-008 (deferred).
- **No rename of existing entries.** `[Mood: Euphoric]`, `[Whispered]`, etc. stay verbatim.
- **No migration of prefix-form entries to bare form.** `[Mood: X]` stays as `[Mood: X]`.
- **No removal of existing entries.** Even if cheat sheet doesn't reference an existing tag, it stays.
- **No genre tag expansion** — that's B-SUNO-006 (separate slice; sunoaiwiki genres URL).
- **No deeper metatag reference reconciliation** — that's B-SUNO-007 (separate slice; sunoaiwiki metatags URL).
- **No prompt library import** — that's B-SUNO-008 (separate slice; awesome-suno-prompts URL).
- **No tags.json schema/format change.** Existing JSON array of `{ category, label, bracket, description? }` records preserved verbatim.

## 9. Open scope decisions (for Lead ratification at plan-phase review)

1. **New category name: `SFX`** — accept, or prefer alternative (e.g., `Sound Effects`, `Effects`, `Ambient`)?
2. **Atmosphere-variant assignment:** `[Nighttime Atmosphere]`, `[Daytime Atmosphere]`, `[Natural Ambience]` — go to new `SFX` category (planner-default) or to existing `Mood` category?
3. **Composite-tag ADD/SKIP defaults** (§4): planner-defaulted 4 ADDs (Catchy Hook, Emotional Bridge, Melodic Interlude, Percussion Break) and 2 SKIPs (Powerful Outro, Soft Intro). Lead-ratify or override?
4. **Decision table file location:** `docs/reference/B-SUNO-005-decision-table.md` (separate file, planner-default) or inline in plan packet (would balloon plan packet to ~800 lines)?
5. **`[Echo/Delay]` handling:** SKIP (slash in bracket is malformed Suno syntax) or ADD as `[Echo]` + `[Delay]` separately?
6. **Wiki: new [[sunometatag-tag-library]] page** vs extending [[sunometatag-app]] only? Planner-default: new page for separation of concerns.
7. **Test counts:** v1.4 adds 6 new tests (C1–C6); existing 47 preserved. Lead-ratify count or push for more/less coverage on the content side?

## 10. Migration risk

- **JSON syntax errors during decision-table application.** Risk: trailing commas, missing quotes, bracket-collision typos. **Mitigation:** apply decision table via deterministic JSON-serialization (write code to read tags.json, append entries, re-serialize) OR hand-edit + run `dotnet build` after each batch to catch JSON parse errors at app startup. T0 baseline check confirms the new tags.json loads cleanly via `TagService.LoadAll` before USER REVIEW.
- **Bracket collisions** (two entries with identical `bracket` string). **Mitigation:** test C5 explicitly checks all brackets are unique. If a collision is detected, the colliding entry is moved to SKIP at decision-table review.
- **Category-name typos** (e.g., `"SFX "` with trailing space vs `"SFX"`). **Mitigation:** decision table uses single canonical spelling; test C6 verifies all entries have non-empty category.
- **Filter/search performance** on a larger tag library (198 entries vs 124). **Mitigation:** virtualization is B-011 in v1 backlog; v1.4 ships at 198 which is well below the 300-tag threshold for B-011 trigger. No performance concern flagged.

## 11. Non-functional contracts

- **Determinism:** decision-table application is pure JSON manipulation; fully deterministic.
- **No I/O changes:** no new file paths, no network access; tags.json continues to live at `src/SunoMetatagApp/Resources/tags.json` with `CopyToOutput` rule.
- **Backwards compatibility:** all existing entries preserved verbatim; no rename / removal / format change. Any saved prompts referencing existing tags continue to work.

## 12. Source paths

- `j:\SunoMetatagApp\src\SunoMetatagApp\Resources\tags.json` (target; appended)
- `j:\SunoMetatagApp\docs\reference\suno-cheat-sheet-2026-05-26.md` (source; immutable)
- `j:\SunoMetatagApp\docs\reference\B-SUNO-005-decision-table.md` (new; ~120 rows × ADD/MERGE/SKIP × rationale)
- `j:\SunoMetatagApp\tests\SunoMetatagApp.Tests\TagServiceCheatSheetTests.cs` (new; 6 C1–C6 tests)
