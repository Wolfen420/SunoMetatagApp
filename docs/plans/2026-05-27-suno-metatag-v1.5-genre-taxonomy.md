# SunoMetatagApp v1.5 — Implementation Plan (B-SUNO-006 Genre Taxonomy)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-006 / v1.5 — Import sunoaiwiki music genres list as a new `Genre` category
- **Type:** Content-only data curation (zero source-code changes)
- **Spec:** [`docs/specs/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md`](../specs/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md)
- **Source:** [`docs/reference/suno-genre-source-2026-05-27.md`](../reference/suno-genre-source-2026-05-27.md) (immutable)
- **Decision table:** [`docs/reference/B-SUNO-006-decision-table.md`](../reference/B-SUNO-006-decision-table.md) (lead-ratified at r1 approval)
- **Precedent:** v1.4 (B-SUNO-005, commits `301c672` / `fadb7b9` / `42c5a28`); v1.3 (B-SUNO-004, commits `0305c73` / `4b72352` / `c74f082` / `ec9e19f`)
- **Working baseline:** `master` tip `42c5a28` (v1.4 closeout)

## Task list (T0-T8)

### T0 — Baseline + research artifacts landed
- Confirm `git status` clean on `j:\SunoMetatagApp\` `master`; `git log -1 --oneline` should show `42c5a28` (v1.4 hygiene).
- Land 4 doc/reference artifacts as **untracked** files (committed at T1):
  - `docs/specs/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md` (this slice's spec)
  - `docs/plans/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md` (this file)
  - `docs/reference/suno-genre-source-2026-05-27.md` (already landed as part of r1 planner draft)
  - `docs/reference/B-SUNO-006-decision-table.md` (planner draft — to be lead-ratified at r1 review)
- Run `dotnet build` + `dotnet test` — should show **53 tests green** (v1.4 baseline).
- **Absorption edits:** apply any pre-T1 absorption items from Lead/Specialist r1 review here (mirroring v1.4 T0 absorption of MEDIUM 1 / LOW 2).

### T1 — Apply decision table to `tags.json`
- Hand-edit `src/SunoMetatagApp/Resources/tags.json` to append all ADD rows from the decision table.
- Insertion strategy: **append a new `Genre` block** after the existing SFX block (preserving v1.2 category ordering convention: Structure → Vocal → Instrument → Mood → Effect → Production → SFX → Genre).
- All ADD rows use canonical Title-Case `[Title Case]` bracket form per spec §3.2 / §3.4 / §3.5.
- Preserve all 199 existing entries verbatim.
- Validate JSON parses (`dotnet test` would surface any malformed JSON via `TagService` startup).
- **Commit boundary:** primary commit with `tags.json` + 4 doc artifacts.
  - Suggested message: `B-SUNO-006 / v1.5: import sunoaiwiki genre taxonomy (+~75 Genre entries)`

### T2 — Add content-coverage tests
- Create `tests/SunoMetatagApp.Tests/TagServiceGenreTaxonomyTests.cs` with **G1-G6** per spec §7.1.
- Use the same `LoadProductionTagsJson()` helper pattern from `TagServiceCheatSheetTests.cs` (v1.4 precedent):
  ```csharp
  private static IReadOnlyList<TagDefinition> LoadProductionTagsJson()
  {
      var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
      return TagService.LoadAll(path);
  }
  ```
- Ensure tests reference production `tags.json` (copied to `AppContext.BaseDirectory` via project file's `<None Include>` copy directive — already set up in v1.4).
- **T2 grep-recount discipline:** before committing, run `grep -c '"category": *"Genre"'` on `src/SunoMetatagApp/Resources/tags.json` and compare to decision-table grand-total. If off-by-N, file a hygiene commit (mirroring v1.4 `42c5a28`).
- Run `dotnet test` — expect **70 tests green** (53 baseline + 5 [Fact] + 12 [Theory] inline rows for G3; the planner draft said 59 expecting G3 as a single [Fact], but G3 is data-driven so each search-term counts separately).
- **Commit boundary:** secondary commit with new test file only.
  - Suggested message: `B-SUNO-006 / v1.5: 6 content-coverage tests for Genre taxonomy (G1-G6)`

### T3 — Test pass verification
- `dotnet test --no-build` — confirm 70/70 green at the secondary-commit tip.
- If off-by-N caught at T2 grep-recount, this is where the hygiene commit lands (T2.5):
  - Update spec / plan / decision-table totals.
  - **Commit boundary:** tertiary hygiene commit.
  - Suggested message: `B-SUNO-006 / v1.5: hygiene — correct stale totals after grep recount`

### T4 — Dev smoke launch
- `dotnet run --no-build --project src/SunoMetatagApp` (or `dotnet run` from `src/SunoMetatagApp/`).
- App should launch within 4 seconds.
- No `JsonException`, `TagLoadException`, `XamlParseException` in console.
- Genre category visible in category-filter combobox.

### T5 — Publish artifact rebuild
- `dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish`
- Expected `publish/SunoMetatagApp.exe` size: ~146 MB (same as v1.3 / v1.4 baseline).
- Expected `publish/tags.json` size: ~21-22 KB (v1.4 was 19,146 bytes for 199 entries; +~75 entries adds ~2-3 KB).
- Smoke-launch `publish/SunoMetatagApp.exe`; verify Genre category visible.

### T6 — USER REVIEW manual smoke matrix
- Surface `USER REVIEW NEEDED` header to user with **S1-S8** per spec §7.2.
- Required response format: 8-row PASS/FAIL table OR free-text confirmation listing any FAILs.
- Target: **8/8 PASS first try** (precedent: v1.3 + v1.4 both PASSed first round).

### T7 — Wiki updates landed in-cycle
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\features\sunometatag-app.md`:
  - Title bump: v1.4 → v1.5.
  - Add subsection: `## v1.4 → v1.5 (2026-05-27)` describing the Genre category import.
  - Refresh `last_confirmed: 2026-05-27`.
  - Extend `sources`, `related`, `tags` frontmatter as needed.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\architecture\sunometatag-tag-library.md`:
  - Extend 7-category breakdown to **8** with Genre row.
  - Add §3.4 / §3.5 canonicalization rules (music suffix removal; abbreviation policy).
  - Append reconciliation history table row for B-SUNO-006.
  - Refresh `last_confirmed: 2026-05-27`.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md`:
  - Prepend Archive entry 12 (B-SUNO-006 r1 plan packet content — will happen during T8a step).

### T8 — Workflow packet maintenance
- **T8a:** Archive v1.4 RESULT packet as Archive entry 12 in `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md`. (Already done as part of r1 planner draft — see archival action below for the B-SUNO-006 r1 plan packet itself when it's later converted to RESULT.)
- **T8b:** Append v1.5 execution entry to `j:\SunoSongSetup\ai\EXECUTION_LOG.md`.
- **T8c:** Rewrite `j:\SunoSongSetup\ai\PLAN.md` from r1 plan packet → RESULT packet (post-execution).

## Working-tree state at each commit boundary

| Commit | Files | Validation |
|---|---|---|
| (working tree pre-T1) | spec + plan + decision-table + source untracked | `git status` shows 4 untracked .md files; build green |
| **Primary (T1)** | `tags.json` + 4 doc artifacts | `dotnet build` green; `dotnet test` 53/53 green |
| **Secondary (T2)** | New `TagServiceGenreTaxonomyTests.cs` | `dotnet test` 70/70 green |
| **Hygiene (T2.5, optional)** | spec + plan + decision-table totals if grep-recount finds off-by-N | `dotnet test` 59/59 still green; threshold `>= 70` Genre / `>= 270` total remains satisfied |

## Open r1 risks for Lead/Specialist review

These overlap with spec §11 open questions. Listed here so Lead can ratify or override in r1 verdict:

1. **Decision-table grand-total** is a planner draft. v1.4 precedent: planner draft off-by-one caught at T2. Same risk applies; mitigation is the test-threshold `>= N` pattern.
2. **Bracket-form canonicalization** in §3.4 / §3.5 is a planner default (Drop "music" suffix; `R&B` for "Rhythm and blues"). Lead may override.
3. **Section heading inclusion** (`[Rock]`, `[Pop]`, etc.) is a planner default. Lead may override to "sub-styles only" or "headings only".
4. **No `aliases:` JSON field** — alias schema explicitly deferred to **B-008** (consistent with v1.4 precedent).
5. **No source-code changes.** Picker dropdown / chip rows / search filter all work automatically with new Genre rows by virtue of existing `category` field semantics.

## Specialist activation forecast

- **ENGINE:** out of scope. No code changes, no geometry, no determinism-affecting surface.
- **FRONTEND/UX:** activation expected (mirrors v1.4 activation level). Specialist scrutiny on:
  - Genre category alphabetical position in combobox (acceptable per v1.2 logic; not a regression).
  - Chip-row density when filtered to Genre (~75 chips; existing rendering tested at SFX scale of 49 in v1.4 and shows acceptable scroll behavior).
  - Search affordance for genre queries (existing search filter already handles arbitrary category data).
  - Discoverability of new category for existing users.

## Result-cycle wiki commitment (per CLAUDE.md wiki-sync gate)

Closeout will declare:
```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-tag-library]], [[ai-plan-archive]]
```

`wiki_sync_status: PASS` is the expected closeout state.
