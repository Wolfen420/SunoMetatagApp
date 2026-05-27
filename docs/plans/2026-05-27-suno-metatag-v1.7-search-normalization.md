# SunoMetatagApp v1.7 — Implementation Plan (B-SUNO-009 Search Normalization)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-009 / v1.7 — Hyphen/space-insensitive search normalization in `TagService.Filter`
- **Type:** Source-code change (no tags.json mutations; no UI changes)
- **Spec:** [`docs/specs/2026-05-27-suno-metatag-v1.7-search-normalization.md`](../specs/2026-05-27-suno-metatag-v1.7-search-normalization.md)
- **Precedent shape:** v1.3 (B-SUNO-004, commits `0305c73` / `4b72352` / `c74f082` / `ec9e19f`) — focused source-code change with new test class + USER REVIEW smoke matrix. **Different shape from v1.4/v1.5/v1.6 content slices** (no decision table, no immutable source capture).
- **Working baseline:** `master` tip `30e3b52` (v1.6 closeout)

## Task list (T0-T8)

### T0 — Baseline + planning artifacts landed
- Confirm `git status` clean on `j:\SunoMetatagApp\` `master`; `git log -1 --oneline` should show `30e3b52` (v1.6 closeout tip).
- Land 2 doc artifacts as **untracked** files (committed at T1):
  - `docs/specs/2026-05-27-suno-metatag-v1.7-search-normalization.md`
  - `docs/plans/2026-05-27-suno-metatag-v1.7-search-normalization.md`
- Run `dotnet build` + `dotnet test` — should show **94 tests green** (v1.6 baseline at HEAD `30e3b52`).
- **Absorption edits:** apply any pre-T1 absorption items from Lead/Specialist r1 review here.

### T1 — Source-code change in `TagService.cs`

Edit `src/SunoMetatagApp/Services/TagService.cs`:

1. Add `private static string NormalizeForSearch(string s)` helper inside the `TagService` class:
   ```csharp
   private static string NormalizeForSearch(string s) =>
       s.Replace("-", "", StringComparison.Ordinal)
        .Replace(" ", "", StringComparison.Ordinal);
   ```

2. Update `searchMatches` closure at lines 79-84 to use the normalized comparison:
   ```csharp
   bool searchMatches(TagDefinition t)
   {
       if (string.IsNullOrEmpty(search)) return true;
       var normalizedSearch = NormalizeForSearch(search);
       return NormalizeForSearch(t.Label).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
           || NormalizeForSearch(t.Bracket).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);
   }
   ```

3. Verify `dotnet build` green.
4. Verify `dotnet test --no-build` shows 94/94 still green (baseline maintained — strict superset semantics).

**Commit boundary:** primary commit with `TagService.cs` change + 2 doc artifacts.
  - Suggested message: `B-SUNO-009 / v1.7: hyphen/space-insensitive search normalization in TagService.Filter`

### T2 — Add content-coverage tests
- Create `tests/SunoMetatagApp.Tests/TagServiceSearchNormalizationTests.cs` with **N1-N6** per spec §6.1.
- Use the same `LoadProductionTagsJson()` helper pattern from v1.4/v1.5/v1.6 precedent.
- N4 implemented as `[Theory]` with 8 inline data rows (mirrors v1.5 G3 / v1.6 H3 [Theory] precedent).
- Run `dotnet test` — expect **>= 100 tests green** (94 baseline + 5 [Fact] N1/N2/N3/N5/N6 + 8 [Theory] N4 inline = 107 expected).

**Commit boundary:** secondary commit with new test file only.
  - Suggested message: `B-SUNO-009 / v1.7: 6 content-coverage tests for search normalization (N1-N6)`

### T3 — Test pass verification
- `dotnet test --no-build` — confirm 107/107 green at secondary-commit tip.
- Cross-verify with v1.6 baseline still passing (no regression class).

### T4 — Dev smoke launch
- `timeout 6 dotnet run --no-build --project src/SunoMetatagApp` — expect `EXIT=124` (timeout-killed WPF GUI) with no exception output before timeout.
- Search-box affordance unchanged visually; test that typing `kpop` (no hyphen) surfaces `[K-Pop]` interactively.

### T5 — Publish artifact rebuild
- `dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish`
- Expected `publish/SunoMetatagApp.exe` size: ~146 MB (identical to v1.3-v1.6 footprint; tags.json unchanged so `publish/tags.json` also identical at 30,421 bytes / 331 entries).
- Smoke-launch `publish/SunoMetatagApp.exe`; verify no startup exception.

### T6 — USER REVIEW manual smoke matrix
- Surface `USER REVIEW NEEDED` header to user with **S1-S8** per spec §6.2.
- Required response format: 8-row PASS/FAIL table OR free-text confirmation.
- Target: 8/8 PASS. **S2 is the critical case** — `kpop` (no hyphen) → `[K-Pop]` should now surface. Resolves v1.5 PASS-WITH-CONCERN class.

### T7 — Wiki updates landed in-cycle
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\features\sunometatag-app.md`:
  - Title bump: v1.6 → v1.7.
  - Add subsection: `## v1.6 → v1.7 (2026-05-27)` describing the search normalization (TagService.Filter change; N1-N6 tests; resolves v1.5 S6 PASS-WITH-CONCERN class).
  - Refresh `last_confirmed: 2026-05-27`.
  - Extend `sources` frontmatter.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\architecture\sunometatag-tag-library.md`:
  - **Update** "Literal-substring search (no hyphen/space normalization)" subsection at lines 181-205:
    - Rename to "Search normalization (hyphen/space-insensitive, v1.7+)" or similar.
    - Rewrite content to describe the v1.7 behavior (normalization helper + strict-superset semantics).
    - Remove the "B-SUNO-009 implementation note" since the slice has landed.
    - Note that 24 cumulative MERGE rows would still benefit from B-008 alias schema for cross-category aliases that aren't hyphen/space variants (e.g., `[Whisper]` → `[Whispered]`, `[Audience Cheering]` → `[Crowd Cheering]`).
  - Add `TagService.cs` change reference to Source paths.
  - Refresh `last_confirmed: 2026-05-27`.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md`:
  - Prepend Archive entry 17 (B-SUNO-009 r1 plan packet content — will happen during T8).
  - Verify Archive entry 16 (v1.6 RESULT) is already prepended at r1 draft time.

### T8 — Workflow packet maintenance
- **T8a:** Archive entry 16 (v1.6 RESULT) prepended during this r1 plan packet draft. Archive entry 17 prepended at T8 ai/PLAN.md → RESULT rewrite.
- **T8b:** Append v1.7 execution entry to `j:\SunoSongSetup\ai\EXECUTION_LOG.md`.
- **T8c:** Rewrite `j:\SunoSongSetup\ai\PLAN.md` from r1 plan packet → RESULT packet (post-execution).

## Working-tree state at each commit boundary

| Commit | Files | Validation |
|---|---|---|
| (working tree pre-T1) | spec + plan untracked | `git status` shows 2 untracked .md files; build green; 94/94 tests green (baseline) |
| **Primary (T1)** | `Services/TagService.cs` + 2 doc artifacts | `dotnet build` green; `dotnet test` 94/94 green (no regression on baseline) |
| **Secondary (T2)** | New `TagServiceSearchNormalizationTests.cs` | `dotnet test` 107/107 green |

## Open r1 risks for Lead/Specialist review

These overlap with spec §10 open decisions:

1. **Normalization scope** (§3.1): only `-` and ` `; not other punctuation. Lead may extend or narrow.
2. **Performance** (R1): re-normalizing target on every Filter call. At 331 entries this is microseconds; if Lead/specialist want caching, planner-default deferred to future slice.
3. **B-SUNO-008 alternative path** (Lead-noted as alternative in v1.6 closeout target_item): sequence after v1.7 ships. Lead may override to bundle or swap priority.

## Specialist activation forecast

- **ENGINE:** out of scope. `TagService.Filter` is deterministic; no concurrency, persistence, or generation-semantic implications.
- **FRONTEND/UX:** activation expected. Specialist scrutiny anticipated on:
  - User-facing search affordance change (strict-superset semantics).
  - Edge cases around `[Effect: Lo-fi]` cross-category coexistence with `[Lo-Fi]` Genre (both surface on `lofi` query — intended behavior per v1.6 §3.3 ADD-new-cat policy).
  - Test coverage adequacy of N1-N6 vs other potential edge cases.
  - Discoverability of the search-affordance change for existing users (no UI text change; behavior just becomes more permissive).

## Result-cycle wiki commitment

Closeout will declare:
```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-tag-library]], [[ai-plan-archive]]
```

`wiki_sync_status: PASS` expected.
