# SunoMetatagApp v1.9 — Implementation Plan (B-SUNO-008b Full Prompt Library Curation)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-008b / v1.9 — Full curation; replace 16-entry seed with 136-entry corpus
- **Type:** Pure data curation slice (mirror v1.4 / v1.5 / v1.6 content-only release discipline)
- **Spec:** [`docs/specs/2026-05-27-suno-metatag-v1.9-prompt-library-curation.md`](../specs/2026-05-27-suno-metatag-v1.9-prompt-library-curation.md)
- **Decision packet authority:** Lead-ratified `D-2026-05-27-B-SUNO-008-scope-phasing` Option A — v1.9 is the explicit B-SUNO-008b continuation
- **Working baseline:** `master` tip `25f8f2e` (v1.8 closeout = B-SUNO-008a PASS)

## Task list (T0-T8)

### T0 — Baseline + planning artifacts staged

- Confirm `git status` clean (or accounted-for) on `j:\SunoMetatagApp\` `master`; `git log -1 --oneline` should show `25f8f2e` (v1.8 closeout tip).
- Land 4 doc artifacts as **untracked** files (committed at T1):
  - `docs/specs/2026-05-27-suno-metatag-v1.9-prompt-library-curation.md`
  - `docs/plans/2026-05-27-suno-metatag-v1.9-prompt-library-curation.md`
  - `docs/reference/B-SUNO-008b-decision-table.md` (136 rows; built at T1 after planner enumerates per-row decisions)
  - `docs/reference/awesome-suno-prompts-source-2026-05-27-v1.9.md` (full 136-entry immutable evidence; built at T1 after planner finalizes selections; commit SHA `e1d1247` re-confirmed)
- Run `dotnet build` + `dotnet test` — should show **125 tests green** (v1.8 baseline at HEAD `25f8f2e`).
- **Absorption edits:** apply any pre-T1 absorption items from Lead/Specialist r1 review here.

### T1 — Decision table + new prompts.json + source-capture (primary commit)

Order of operations within T1:

1. **Re-enumerate all 136 source entries** by H3 from the 8 genre files (already staged in `.tmp/awesome-suno-prompts-snapshot/` from v1.8 cycle; v1.9 captures the same SHA `e1d1247`).
2. **Write `docs/reference/B-SUNO-008b-decision-table.md`** with 136 rows:
   - Columns: `# | Genre | SubGenre | Title | Decision (ADD/SKIP) | Energy (parsed) | Rationale`
   - Per-row ADD or SKIP determination. Planner default forecast: ≥134 ADDs, ≤2 SKIPs (only for structurally malformed bodies or rare in-source duplicates).
   - Energy column: parsed `int` for entries with `N/10` format; `null` for `"Variable"` / `"11/10 (off the scale)"` (~2 entries).
3. **Write `docs/reference/awesome-suno-prompts-source-2026-05-27-v1.9.md`** with:
   - Repo metadata (license CC0-1.0, default branch `main`, commit SHA `e1d1247bd26f896127011d3bbc2ba8599d54960d` re-confirmed)
   - File listing of `prompts/` (8 entries with sizes; identical to v1.8 capture)
   - For each of the 136 ADD-decided entries: source-anchor URL + raw triple-backtick block body + extracted UseCase / SunoVersion / Energy / NotableFeature labels
4. **Replace `src/SunoMetatagApp/Resources/prompts.json`** with 136 entries (or ADD-count after SKIPs):
   - Per-entry fields per v1.8 schema: `genre`, `subGenre`, `title`, `body`, `useCase`, `sunoVersion`, `energy`, `notableFeature`, `sourceUrl`.
   - `tags` and `difficulty` remain absent (null at deserialize time).
   - Body strings retain the verbatim triple-backtick block contents from source with `\n` line endings as JSON string escapes.
5. **Verify `dotnet build` green.**
6. **Verify `dotnet test --no-build` reports failures on the v1.8 P1/P3 assertions** (expected — they assert `==16` and `==2`; v1.9 P1/P3 must change). Failures here are the trigger for T2 in-place test edits.

**Commit boundary:** primary commit with new `prompts.json` (replacing v1.8 seed) + `docs/reference/B-SUNO-008b-decision-table.md` + `docs/reference/awesome-suno-prompts-source-2026-05-27-v1.9.md` + spec + plan.
  - Suggested message: `B-SUNO-008b / v1.9: replace 16-entry seed with 136-entry curated corpus + decision table`
  - Build green; tests expected to fail on P1/P3 (T2 fixes them).

### T2 — Test count + P3 assertion shape edits (secondary commit)

Order of operations within T2:

1. **In-place edit `tests/SunoMetatagApp.Tests/PromptServiceTests.cs`:**
   - P1: change `Assert.Equal(16, prompts.Count)` → `Assert.Equal(136, prompts.Count)` (or the actual ADD-count after T1 SKIPs are known).
   - P3: change "each genre has exactly 2 entries" to per-genre minimum count assertion. Suggested implementation:
     ```csharp
     var expectedMin = new Dictionary<string, int>
     {
         { "Pop", 21 }, { "Rock", 18 }, { "EDM", 17 }, { "Hip-Hop", 16 },
         { "Indie", 18 }, { "Jazz-Blues", 18 }, { "R&B-Soul", 15 }, { "Country", 13 },
     };
     foreach (var (genre, min) in expectedMin)
     {
         var count = prompts.Count(p => p.Genre.Equals(genre, StringComparison.Ordinal));
         Assert.True(count >= min, $"Genre '{genre}' has {count} entries; expected >= {min}.");
     }
     ```
   - P3 minimums adjust downward by any SKIP rows from the decision table (Lead will ratify the table at r1; planner default forecast is full source-distribution minimums).
2. **Add new P8 [Fact]** asserting 5 high-utility entries are present:
   ```csharp
   [Fact]
   public void P8_KnownHighUtilityEntriesPresent()
   {
       var prompts = LoadProductionPromptsJson();
       string[] expected =
       {
           "Modern Pop Anthem (Female Vocals)",
           "Epic Arena Anthem",
           "Big Room House Anthem",
           "Modern Trap Anthem",
           "Classic Big Band Swing",
       };
       foreach (var title in expected)
           Assert.Contains(prompts, p => p.Title.Equals(title, StringComparison.Ordinal));
   }
   ```
3. **Verify `dotnet test` reports ~127 green** (111 v1.7 baseline + 14 v1.8 P1-P7 with P1/P3 edited + 1 new P8 = 126; or 127 if P3 split into per-genre rows).

**Commit boundary:** secondary commit with `PromptServiceTests.cs` in-place edits + P8 addition.
  - Suggested message: `B-SUNO-008b / v1.9: update P1 count + P3 assertion shape + add P8 high-utility presence test`

### T3 — (No tertiary commit needed)

v1.4 / v1.5 / v1.6 each shipped in 2-3 commits. v1.9 ships in 2 (T1 primary + T2 secondary). No new UI / source-code / additional tests beyond P8. **Tertiary skipped.**

### T4 — Dev smoke launch

- `timeout 6 dotnet run --no-build --project src/SunoMetatagApp` — expect `EXIT=124` (timeout-killed WPF GUI) with no exception output before timeout.
- Default state: prompt browser still hidden (no UI change); existing v1.8 layout unchanged.
- Optional: a quick interactive launch (not timeout-killed) to manually verify the 136-entry list scrolls cleanly is encouraged at T6 USER REVIEW.

### T5 — Publish artifact rebuild

- `dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish`
- Expected `publish/SunoMetatagApp.exe` size: ~146 MB ± small footprint increase (no DLL deltas).
- Expected `publish/prompts.json` size: **~50-90 KB** (136 entries × ~500-700 bytes per entry; v1.8 was 9,528 B for 16 entries, so linear extrapolation gives ~81 KB).
- Expected `publish/tags.json` size: identical to v1.7/v1.8 (30,421 bytes; zero changes).
- Smoke-launch `publish/SunoMetatagApp.exe`; verify no startup exception.

### T6 — USER REVIEW manual smoke matrix

- Surface `USER REVIEW NEEDED` header with **S1-S8** per spec §5.2.
- Required response format: 8-row PASS/FAIL table OR free-text confirmation.
- Target: 8/8 PASS first try (would be **seventh** consecutive USER-REVIEW-first-try-PASS across v1.3 → v1.9).
- **S2 is the discoverability case** — verify the full 136-entry list renders and scrolls cleanly.
- **S3 is the genre-filter case** — verify Pop filter shows 21 entries (not 2).
- **S5 is the critical case** — copy-to-clipboard end-to-end; randomly sample 2-3 different bodies from the new 120 entries.

### T7 — Wiki updates landed in-cycle

- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\features\sunometatag-app.md`:
  - Title bump: v1.8 → v1.9
  - Add subsection: `## v1.8 → v1.9 (2026-05-27)` describing the corpus expansion (16 → 136), parent B-SUNO-008 retirement, and decision-table-driven curation discipline carry-over from v1.4-v1.6.
  - Refresh `last_confirmed: 2026-05-27`.
  - Extend `sources` frontmatter.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\architecture\sunometatag-prompt-library.md`:
  - Rename "Seed corpus (v1.8)" section to "Corpus (v1.9, full)".
  - Add reconciliation-history row noting the 16 → 136 transition.
  - Refresh `last_confirmed: 2026-05-27`.
  - Note: `Energy` field documentation already covers `int?` nullability; v1.9 exercises the null case for ~2 entries.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md`:
  - Archive entry 20 (v1.8 RESULT) already prepended at this drafting (2026-05-27).
  - Archive entry 21 (v1.9 r1 plan) will be prepended at T8 ai/PLAN.md → RESULT rewrite during v1.9 closeout.

### T8 — Workflow packet maintenance

- **T8a:** Archive entry 21 (v1.9 r1 plan packet body) prepended at T8 ai/PLAN.md → RESULT rewrite.
- **T8b:** Append v1.9 execution entry to `j:\SunoSongSetup\ai\EXECUTION_LOG.md`.
- **T8c:** Rewrite `j:\SunoSongSetup\ai\PLAN.md` from r1 plan packet → RESULT packet (post-execution).

## Working-tree state at each commit boundary

| Commit | Files | Validation |
|---|---|---|
| (working tree pre-T1) | 4 doc artifacts untracked (spec + plan + decision-table + v1.9 source-capture) | `git status` shows 4 untracked .md files; build green; 125/125 tests green (v1.8 baseline) |
| **Primary (T1)** | `Resources/prompts.json` replaced (16 → 136) + 4 doc artifacts | `dotnet build` green; `dotnet test` expected failure on P1/P3 (assertion mismatch; fixed at T2) |
| **Secondary (T2)** | `tests/SunoMetatagApp.Tests/PromptServiceTests.cs` in-place edits (P1 count, P3 assertion shape) + P8 [Fact] | `dotnet test` ~127 green (125 v1.8 baseline + P1 count change in-place + P3 assertion shape change in-place + 1 new P8 = ~127) |

## Open r1 risks for Lead/Specialist review

These overlap with spec §4 risks:

1. **Test count drift forecast** (R7): planner forecast is `~127` but exact number depends on whether P3 is implemented as 1 [Fact] iterating 8 genres (=1 result) or split as 8 [Theory] rows (=8 results). Planner default: keep P3 as 1 [Fact] for minimal test-count delta.
2. **SKIP density at T1** (§3.1): planner forecast is 0-2 SKIPs based on visual audit during v1.8 cycle. Lead may want explicit pre-T1 lock on SKIP rationale.
3. **Body verbatim line-ending discipline** (R8): JSON serializer handles `\n` escapes; planner ensures source's UNIX line endings are preserved as `\n` in JSON strings (not `\r\n`).
4. **B-SUNO-012 (High priority) Lead override path** (R10): Lead may want to schedule B-SUNO-012 before v1.9 retires B-SUNO-008 parent. Decision-packet workflow would be required for a re-sequence.

## Specialist activation forecast

- **ENGINE:** out of scope. No source-code changes; `PromptService.LoadAll` continues to do deterministic JSON read; no concurrency, persistence, generation-semantic implications.
- **FRONTEND/UX:** activation expected. Specialist scrutiny anticipated on:
  - Discoverability of 136 entries via Genre filter only (no free-text search; user-confirmed via brainstorm 2026-05-27).
  - Pop filter showing 21 entries vs 2 — visual density check.
  - ListView vertical scroll behavior with 136 rows.
  - Detail panel responsiveness when selecting any of the new 120 entries (some bodies are longer; 180-DIP `MaxHeight` may need adjustment).
  - Copy-to-clipboard performance with longer bodies (low risk; Clipboard.SetText is sub-millisecond).
  - Whether the genre filter should default to a non-`All` value to reduce first-load cognitive load (planner default: `"All"` matches v1.8).

## Result-cycle wiki commitment

Closeout will declare:
```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-prompt-library]], [[ai-plan-archive]]
```

`wiki_sync_status: PASS` expected.
