# SunoMetatagApp v1.5 — Genre Taxonomy Import (B-SUNO-006)

- **Authored:** 2026-05-27
- **Type:** Content-only data curation slice (zero source-code changes)
- **Parent backlog:** B-SUNO-006 (Add genre/style taxonomy tags from Suno genre reference)
- **Source artifact:** [`docs/reference/suno-genre-source-2026-05-27.md`](../reference/suno-genre-source-2026-05-27.md) (immutable WebFetch capture of `https://sunoaiwiki.com/resources/2024-05-03-list-of-music-genres-and-styles/`)
- **Curation artifact:** [`docs/reference/B-SUNO-006-decision-table.md`](../reference/B-SUNO-006-decision-table.md) (planner draft; lead-ratified at r1 approval)
- **Implementation plan:** [`docs/plans/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md`](../plans/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md)
- **Workflow precedent:** B-SUNO-005 / v1.4 (content-only release; ADD/MERGE/SKIP decision table; new SFX category). v1.5 mirrors that structure for the new **Genre** category.

## 1. Goal

Import the Suno community wiki's "List of Music Genres and Styles" taxonomy as a curated **new `Genre` category** in `tags.json`, growing the library from **199 → 286 entries** (planner draft 86 ADD + 1 specialist-LOW-1 absorbed at T0 for `[Avant-garde]` sibling parent in §A; exact count verified by T2 grep-recount discipline).

This is a **content curation slice**, identical in shape to v1.4:
- No source-code changes (no `MainWindow.xaml`, no `MainViewModel.cs`, no `TagService.cs`).
- One new top-level category: **Genre** (currently zero entries).
- Decision-table workflow: every source row → ADD / MERGE / SKIP with rationale.
- New content-coverage tests for the Genre category.
- No alias/synonym schema (deferred to **B-008** alongside MERGE rows from v1.4).

## 2. Non-goals

- **No** alias/synonym schema. Cross-section duplicates (`Punk rock`, `K-pop`) are SKIP-as-internal-duplicate with the canonical occurrence going to ADD, OR are added once and listed in the decision table's MERGE column for future B-008 alias resolution. Whichever path the decision table chooses, **no `aliases:` JSON field is added** in v1.5.
- **No** UI restyling. The Genre category renders with the existing v1.2 visual theme (Suno-styled chip rows, same picker button styles).
- **No** code changes to the picker dropdown / category filter. `TagService` already handles arbitrary new categories; the new Genre rows surface automatically through the category-filter combobox by virtue of existing `category` field semantics.
- **No** insertion-behavior changes. Plain click and Shift+click (v1.3) work identically on Genre entries.
- **No** existing tag mutations. All 199 v1.4 entries remain verbatim — no renames, no removals, no recategorizations.
- **No** changes to bracket convention. Genre tags use `[Title Case]` like all other categories.

## 3. Curation rules

### 3.1 Source authority

`docs/reference/suno-genre-source-2026-05-27.md` is the **immutable evidence layer**. All ADD/MERGE/SKIP claims in the decision table cite source section + raw label.

### 3.2 Bracket form (canonical Title-Case)

All Genre entries use bracket form `[Title Case]` regardless of source capitalization. **Title-Case rule:** capitalize each word except mid-word conjunctions (`and`, `or`, `the`) and prepositions (`of`, `in`) per common US English title-case convention. Hyphenated words capitalize both segments (`[K-Pop]`, `[Singer-Songwriter]`, `[Dance-Pop]`). Acronyms preserve their established casing (`[R&B]`, `[UK Drill]`). Examples:

| Source label | Bracket form in tags.json |
|---|---|
| `Electroacoustic` | `[Electroacoustic]` |
| `Industrial music` | `[Industrial]` (suffix "music" dropped — see §3.4) |
| `Chicago blues` | `[Chicago Blues]` |
| `Rhythm and blues` | `[R&B]` (canonical abbreviation — see §3.5) OR `[Rhythm and Blues]` (decision-table adjudication) |
| `K-pop` | `[K-Pop]` |
| `Drum and bass` | `[Drum and Bass]` |
| `Bebop` | `[Bebop]` |

### 3.3 Section-heading-as-Genre policy

The **14 top-level section headings** (Avant-garde & Experimental, Blues, Country, …, Regional Music) are themselves valid Genre tags in addition to their sub-styles. Adopted policy:

- Add section headings as top-level Genre entries (e.g., `[Rock]`, `[Pop]`, `[Country]`, `[Jazz]`, `[Blues]`, `[Hip Hop]`, `[Folk]`, `[Electronic]`, `[Metal]`, `[Punk]`, `[R&B]`, `[Soul]` if separately desired, `[Easy Listening]`, `[Experimental]`).
- Skip section headings that are awkward as labels (`Regional Music` → SKIP as too generic; expand its 4 super-labels and 4 paren'd sub-styles as standalone entries instead).
- Skip headings that overlap with existing categories on `tags.json` semantic surfaces (none in v1.5; no existing `Genre` entries to collide with).

### 3.4 "music" suffix removal (canonical bare form)

Source entries ending in " music" (e.g., `Industrial music`, `Noise music`, `Progressive music`, `Psychedelic music`, `House music`, `Trance music`, `Celtic music`, `Gospel music`) are canonicalized by **dropping the suffix**, yielding `[Industrial]`, `[Noise]`, `[Progressive]`, `[Psychedelic]`, `[House]`, `[Trance]`, `[Celtic]`, `[Gospel]`. Rationale: bracket density in metatag use is the norm, and `[Industrial]` reads identically to `[Industrial music]` in Suno-generation context. Decision-table column documents per-row whether suffix removed or preserved.

### 3.5 Established-abbreviation canonicalization

A small set of entries have widely-used short-form conventions:

| Raw source | Canonical bracket form | Rationale |
|---|---|---|
| `Rhythm and blues` | `[R&B]` | "R&B" is the universal label in modern music tooling; matches sectional heading "R&B & Soul". |
| `Drum and bass` | `[Drum and Bass]` (decision-table may also add `[DnB]` ADD) | Long form is searchable; abbreviated form is well-known. |
| `Hip Hop` (section heading and prefix) | `[Hip Hop]` (two words, space) | Consistent with sectional heading. |

The decision table is the source-of-truth for per-row canonicalization decisions.

### 3.6 Internal duplicate handling (within source)

The source contains 3 known internal duplicates (planner observation in `suno-genre-source-2026-05-27.md`):

1. **`Punk rock`** in `Rock` ∩ `Punk` → ADD once as `[Punk Rock]`; decision-table notes both sections in rationale.
2. **`K-pop`** in `Pop` ∩ `Regional Music → Asian music` → ADD once as `[K-Pop]`; both sections cited.
3. **`Soft rock`** in `Easy Listening` ∩ implied Rock (not literally duplicated; Rock section has `Classic rock`, `Hard rock` etc.) → ADD as `[Soft Rock]` once with `Easy Listening` as primary section.

### 3.7 Regional Music expansion

The `Regional Music` section uses parenthetical sub-styles (`Brazilian music (Samba, Bossa nova)`). Adopted policy:

- ADD each parenthetical sub-style as its own Genre tag: `[Samba]`, `[Bossa Nova]`, `[Reggae]`, `[Dancehall]`, `[Afrobeat]`, `[Highlife]`, `[J-Pop]`.
- SKIP the regional super-labels (`Brazilian music`, `Caribbean music`, `African music`, `Asian music`) as too coarse for direct prompt use. Suno users prefer `[Reggae]` over `[Caribbean music]`.
- `K-pop` from this section is already covered by `Pop` section ADD; SKIP-as-internal-dup here.

### 3.8 MERGE-as-no-op convention (continued from v1.4)

If the decision table identifies any source row as semantically equivalent to an existing v1.4 tag (currently appears to be **zero** since no Genre category exists), the row is **MERGE** without adding to `tags.json`. MERGE rationale is recorded in the decision table for future **B-008** alias-schema work. **No `aliases:` JSON field is added in v1.5.**

## 4. Decision-table format

The decision table follows v1.4 precedent and ships as `docs/reference/B-SUNO-006-decision-table.md`. Columns:

| Column | Purpose |
|---|---|
| Section | Source section name (verbatim) |
| Source label | Source row verbatim |
| Decision | `ADD` / `MERGE` / `SKIP` |
| Target category | Always `Genre` for ADD rows; blank otherwise |
| Bracket form | Canonical `[Title Case]` if ADD |
| Rationale | One-liner: why ADD/MERGE/SKIP; cite source-internal duplicates, "music" suffix, abbreviation policy, etc. |

Sub-totals per section; grand totals at file end. **T2 grep-recount discipline** (v1.4 precedent): planner draft totals must be verified by `grep -c '"category": *"Genre"'` after T1 commit; any off-by-N caught at T2 triggers a hygiene commit. Mirror v1.4 hygiene commit pattern (`42c5a28`).

## 5. Acceptance criteria (B-SUNO-006 close conditions)

1. **Source-of-truth file landed:** `docs/reference/suno-genre-source-2026-05-27.md` exists and is unmodified post-T0.
2. **Decision table landed:** `docs/reference/B-SUNO-006-decision-table.md` exists with every source row adjudicated.
3. **tags.json grown:** `Genre` category has at least the count claimed by decision-table grand-total; full file parses cleanly.
4. **Content-coverage tests pass:** 6 new tests (G1-G6, see §7.1); existing 53 tests still pass; total `>= 59` green.
5. **Build green:** `dotnet build` clean.
6. **Publish smoke:** `publish/SunoMetatagApp.exe` rebuilt and launches without `JsonException` / `TagLoadException` / `XamlParseException`.
7. **USER REVIEW PASS:** 8-case manual smoke matrix (see §7.2) returns 8/8 PASS.
8. **No source-code mutations:** `git diff` against v1.4 tip (`42c5a28`) shows zero changes outside `tags.json`, `docs/`, and the new test file.

## 6. Risks

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| R1 | Decision-table grand totals off-by-N (precedent: v1.4 D.1 had +1 off-by-one). | Low | T2 grep-recount + hygiene commit pattern; tests use `>= N` thresholds, not equality, so off-by-N within margin won't fail tests. |
| R2 | Bracket-form casing inconsistencies (`K-pop` vs `K-Pop`, `J-Pop`, etc.). | Low | §3.2 + §3.5 explicitly enumerate canonical forms; decision-table column captures every per-row choice. |
| R3 | Section heading semantic overlap with sub-styles (`[Rock]` parent vs `[Hard Rock]`, `[Classic Rock]` children). | Low | §3.3 keeps both; users typically use sub-styles for specific prompts and parent for generic. No code change required. |
| R4 | Suno tag-library user expectations differ from sunoaiwiki list (some users may expect genres absent from this list: `Lo-Fi`, `Vaporwave`, `Synthwave`, `Phonk`, `Drill (non-UK)`, etc.). | Medium | Document scope: v1.5 imports the **sunoaiwiki list as-is**, not a comprehensive taxonomy. Future B-SUNO-007 (curate metatag expansion) explicitly addresses additional sources. Acceptance is "covers sunoaiwiki source"; not "covers all Suno-usable genres". |
| R5 | Genre tags clutter search results for non-genre queries (e.g., filtering "Rock" might now return many entries). | Low | Existing category-filter combobox (v1.2) lets users filter to `Genre` only; structural fit is good. |
| R6 | `[Soft Rock]` (Easy Listening) vs `[Rock]` parent confusion. | Low | Decision-table rationale documents Easy Listening sourcing; UI behavior unaffected. |
| R7 | Re-running WebFetch later may return different content if sunoaiwiki revises the page. | Low | Immutable source file is dated 2026-05-27; future captures get new dated files and explicit supersession. |

## 7. Test plan

### 7.1 Automated content-coverage tests (NEW: G1-G6)

New test file: `tests/SunoMetatagApp.Tests/TagServiceGenreTaxonomyTests.cs`.

| Test | Assertion | Notes |
|---|---|---|
| **G1** | `tags.json` entry count `>= 270` (planner-conservative; actual post-LOW-1 absorption: 286). | Mirrors v1.4 C1 pattern. |
| **G2** | A `Genre` category exists with entry count `>= 70`. | New category coverage. Actual after LOW 1 absorption: 87. |
| **G3** | At least one well-known label is searchable from each canonicalization class: `Rock`, `Jazz`, `Hip Hop`, `Electronic`, `Reggae`, `Bebop`, `Bossa Nova`, **`Muzak`** (parenthetical-canonical §D.2), **`K-Pop`** (hyphen-Title-Case §I.4), **`R&B`** (abbreviation-canonical §B.5), **`Heavy Metal`** (Metal section §L.3), **`Avant-garde`** (LOW 1 sibling parent §A.0a). All 12 representative entries searchable. | Mirrors v1.4 C3 pattern; extended per B-SUNO-006 specialist plan-phase advisory LOW 3 (2026-05-27) to cover unusual canonicalizations. |
| **G4** | Genre-category filter returns only Genre entries (no Structure/Vocal/Mood/Effect/Instrument/Production/SFX bleed). | Mirrors v1.4 C4 (SFX isolation). |
| **G5** | No `[...]` bracket collisions between Genre entries and existing 199 v1.4 entries. (Existing tags have NO genre overlap, so this is a regression guard against accidental rename in some other category landing in Genre's space.) | Mirrors v1.4 C5. |
| **G6** | All 8 categories non-empty (`Structure`, `Vocal`, `Instrument`, `Mood`, `Effect`, `Production`, `SFX`, `Genre`). | Mirrors v1.4 C6 (extends to 8). |

Existing 53 v1.4 tests **all still pass** (53/53 → 53/53 + 6 new = 59/59 expected).

### 7.2 USER REVIEW manual smoke matrix (S1-S8, single round target)

| # | Step | Expected outcome |
|---|---|---|
| **S1** | Launch `publish/SunoMetatagApp.exe`; open category-filter dropdown. | Genre category visible (alphabetically between Effect and Instrument, OR at a sensible position per `TagService` ordering). |
| **S2** | Filter to `Genre`; verify chip-row populates. | At least ~70 Genre chips appear; quick visual count plausibility check. |
| **S3** | Plain-click `[Rock]` in some section's editor. | `[Rock]` inserted at caret per v1.1 / v1.3 mechanism; insertion-behavior unchanged. |
| **S4** | Shift+click `[Hard Rock]` into same line as `[Rock]`. | Stacked as `[Rock \| Hard Rock]` per v1.3 mechanism; unchanged. |
| **S5** | Search "rock" in picker. | All rock-related entries surface (`[Rock]`, `[Hard Rock]`, `[Classic Rock]`, `[Indie Rock]`, `[Punk Rock]`, `[Soft Rock]`, `[Country Rock]`, `[Folk Rock]`, `[Alternative Rock]`); no irrelevant entries. |
| **S6** | Search "k-pop" or "kpop". | `[K-Pop]` surfaces. |
| **S7** | Copy preview verbatim. | Renders all inserted Genre tags with correct bracket form. |
| **S8** | v1.3 carry-over: Shift+click into bracket-end with existing `[Mood]` content. | Stacked syntax still works; no regression. |

PASS criterion: 8/8 PASS in a single round (target; precedent: v1.3 + v1.4 both passed first try).

## 8. Implementation surfaces touched

### 8.1 Data files
- `src/SunoMetatagApp/Resources/tags.json` — **append** Genre entries after SFX block (preserving the v1.2 category ordering convention). Exact count from decision-table grand-total.
- `publish/tags.json` — re-published with the same content at T5.

### 8.2 Test files
- `tests/SunoMetatagApp.Tests/TagServiceGenreTaxonomyTests.cs` — new (G1-G6).

### 8.3 Doc files
- `docs/specs/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md` — this file.
- `docs/plans/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md` — implementation plan.
- `docs/reference/suno-genre-source-2026-05-27.md` — immutable source capture (T0).
- `docs/reference/B-SUNO-006-decision-table.md` — curation decisions (T1).

### 8.4 Wiki updates (`j:\SunoSongSetup\.SunoSongSetup-wiki\`)
- `wiki/features/sunometatag-app.md` — version bump v1.4 → v1.5; new "v1.4 → v1.5 (2026-05-27)" subsection. `last_confirmed` refresh.
- `wiki/architecture/sunometatag-tag-library.md` — extend 7-category breakdown to 8 with new Genre category, new naming rules (§3.4 "music" suffix removal, §3.5 abbreviation canonicalization). Reconciliation history table appended.
- `wiki/reference/ai-plan-archive.md` — Archive entry 12 prepended (B-SUNO-006 r1 plan packet → result packet conversion).

### 8.5 Surfaces explicitly NOT touched
- `src/SunoMetatagApp/MainWindow.xaml`
- `src/SunoMetatagApp/MainWindow.xaml.cs`
- `src/SunoMetatagApp/ViewModels/MainViewModel.cs`
- `src/SunoMetatagApp/ViewModels/TagViewModel.cs`
- `src/SunoMetatagApp/Services/TagService.cs` (already category-agnostic)
- Any other `.cs` / `.xaml` file
- `wiki/architecture/sunometatag-inline-editor.md` (v1.3 mechanism unchanged)
- `wiki/architecture/sunometatag-visual-theme.md` (v1.2 visual theme unchanged)

## 9. Rollback plan

Single-commit revert. All changes ship in **2-3 commits** on `master`:
- Primary commit: `tags.json` + 4 doc artifacts (mirroring v1.4 `301c672`).
- Secondary commit: G1-G6 test file (mirroring v1.4 `fadb7b9`).
- Optional T2 hygiene commit if grep-recount finds off-by-N totals (mirroring v1.4 `42c5a28`).

`git revert <primary-commit>` restores the v1.4 closeout tip (`42c5a28`) cleanly. Tests written in the secondary commit reference the new Genre category, so a clean revert sequence is `git revert <test-commit> && git revert <primary-commit>`.

## 10. Wiki update commitment (closeout)

Closeout will declare `Wiki updates landed:` with these specific pages:
- `[[sunometatag-app]]` — updated v1.4 → v1.5
- `[[sunometatag-tag-library]]` — extended for Genre category and new canonicalization rules
- `[[ai-plan-archive]]` — Archive entry 12 prepended

No queued-exception declarations; all landed in-cycle. No `wiki_sync_status: FAIL` risk.

## 11. Open questions (planner draft — surfaced for Lead/Specialist)

| # | Question | Planner default |
|---|---|---|
| **Q1** | Should section headings (`[Rock]`, `[Pop]`, etc.) be added as Genre tags alongside their sub-styles? | **YES** — added for searchability (§3.3). |
| **Q2** | Drop "music" suffix from canonicalization (`[Industrial]` vs `[Industrial music]`)? | **YES** — bare-form (§3.4). |
| **Q3** | `Rhythm and blues` → `[R&B]` or `[Rhythm and Blues]`? | `[R&B]` — universal short-form (§3.5). |
| **Q4** | `Drum and bass` → keep long form only, or also add `[DnB]`? | Long form only; defer `[DnB]` alias to B-008. |
| **Q5** | Internal duplicates (`Punk rock`, `K-pop`): single canonical ADD or dual ADD? | **Single ADD** with multi-section citation in rationale (§3.6). |
| **Q6** | Regional super-labels (`Brazilian music`, `Caribbean music`, etc.): ADD as parents or SKIP? | **SKIP** — too coarse (§3.7). |
| **Q7** | Test threshold `>= N`: tight (decision-table grand-total exactly) or loose (planner-conservative)? | **Loose** — `>= 270` for total / `>= 70` for Genre (mirroring v1.4 `>= 198` precedent which allowed 199 to pass). |

These are planner-default proposals. Lead may override any during r1 review.

## 12. Conclusion

v1.5 is a **straight content-only release** in the v1.4 mold: ~70-80 new Genre entries, one new top-level category, 6 new tests, zero source-code mutations, mirrored task structure (T0-T8). Risk profile is **low** because v1.3 + v1.4 already validated this curation pattern end-to-end with first-try USER REVIEW PASSes on both. The Genre import is purely additive — no existing tag/category/behavior is touched.
