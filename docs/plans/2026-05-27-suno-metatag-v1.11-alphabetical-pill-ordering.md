# SunoMetatagApp v1.11 — Implementation Plan (B-SUNO-011 Alphabetical Tag-Pill Ordering)

- **Spec:** `j:\SunoMetatagApp\docs\specs\2026-05-27-suno-metatag-v1.11-alphabetical-pill-ordering.md`
- **Approved packet:** `ai/PLAN.md` B-SUNO-011 / v1.11 r1 (Lead Reviewer 2026-05-27, `APPROVED (PASS-WITH-NOTES)` with 3 LOW absorption notes carried in `target_item`).
- **Working baseline:** `master` tip `c1ac316` (v1.10 closeout PASS, B-SUNO-012 retired).
- **Test baseline:** 127/127 green.

## Tasks T0-T8

### T0 — Baseline check

- Verify `master` tip is `c1ac316`.
- `dotnet build` → green.
- `dotnet test tests/SunoMetatagApp.Tests --no-build` → 127/127 green.
- Working-tree state: pre-existing modified `README.md` + untracked `.tmp/` + untracked `docs/reference/B-SUNO-007b-suno-meta-tags-database-decision-table-2026-05-27.md` (carry-overs from v1.9/v1.10 cycles, out of v1.11 scope).
- v1.11 commits MUST use targeted `git add`.

### T1 — Primary commit (mechanism + docs)

Single primary commit containing:

1. **`src/SunoMetatagApp/Services/TagService.cs`:**
   - Append `.OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase)` to the existing `.Where(...)` return chain in the `Filter` method.
   - Add a short comment explaining: alphabetical sort by raw Bracket, case-insensitive ordinal, applies after filter, sort uses display text not v1.7-normalized form, lexical-not-numeric caveat.

2. **`docs/specs/2026-05-27-suno-metatag-v1.11-alphabetical-pill-ordering.md`** — v1.11 spec (already drafted).

3. **`docs/plans/2026-05-27-suno-metatag-v1.11-alphabetical-pill-ordering.md`** — this implementation plan.

Expected commit shape:
- 1 file modified in `src/`.
- 2 new files in `docs/specs/` and `docs/plans/`.
- `dotnet build` green post-edit.
- `dotnet test --no-build` returns 127/127 green (no test changes yet; existing tests are set-based, not order-based).

### T2 — Secondary commit (test additions)

Add O1, O2, O3 tests to `tests/SunoMetatagApp.Tests/TagServiceFilterTests.cs`:

```csharp
// O1 — v1.11 (B-SUNO-011): Filter returns entries in alphabetical-by-Bracket
// order across the full library (All category, empty search). Spot-checks
// relative order of a few well-known entries.
[Fact]
public void O1_Filter_AllCategoryEmptySearch_ReturnsAlphabeticalByBracket()
{
    var tags = TagService.LoadAll(Path);  // production tags.json
    var result = TagService.Filter(tags, null, "All").ToList();

    // Verify result is sorted by Bracket OrdinalIgnoreCase.
    var sorted = result.OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase).ToList();
    Assert.Equal(sorted.Select(t => t.Bracket), result.Select(t => t.Bracket));
}

// O2 — v1.11 (B-SUNO-011): Filter returns category-filtered entries in
// alphabetical order, including prefix-form entries. Verifies [Mood: *]
// cluster sorts internally by post-colon word.
[Fact]
public void O2_Filter_MoodCategory_ReturnsAlphabeticalIncludingPrefixForm()
{
    var tags = TagService.LoadAll(Path);
    var result = TagService.Filter(tags, null, "Mood").ToList();

    Assert.NotEmpty(result);
    var sorted = result.OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase).ToList();
    Assert.Equal(sorted.Select(t => t.Bracket), result.Select(t => t.Bracket));

    // Verify prefix-form entries are present + alphabetical within prefix.
    var moodEntries = result.Where(t => t.Bracket.StartsWith("[Mood:")).ToList();
    Assert.NotEmpty(moodEntries);
    var moodSorted = moodEntries.OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase).ToList();
    Assert.Equal(moodSorted.Select(t => t.Bracket), moodEntries.Select(t => t.Bracket));
}

// O3 — v1.11 (B-SUNO-011) — Lead absorption #3: Filter applies OrdinalIgnoreCase
// to a synthetic mixed-case input set, returning deterministic case-folded order.
// Synthetic test isolates the comparer behavior from production tags.json content.
[Fact]
public void O3_Filter_MixedCaseSynthetic_ReturnsOrdinalIgnoreCaseOrder()
{
    var synthetic = new[]
    {
        new TagDefinition("X", "z-banana", "[BANANA]"),
        new TagDefinition("X", "z-cherry", "[Cherry]"),
        new TagDefinition("X", "z-apple", "[apple]"),
    };
    var result = TagService.Filter(synthetic, null, "All").ToList();
    Assert.Equal(new[] { "[apple]", "[BANANA]", "[Cherry]" },
                 result.Select(t => t.Bracket).ToArray());
}
```

Verify `Path` constant for production `tags.json` is accessible to the test file (check existing `TagServiceFilterTests.cs` for the established constant name).

Expected test count post-T2: **130/130 green** (127 v1.10 baseline + 3 new).

### T4 — Dev smoke launch

`timeout 6 dotnet run --no-build --project src/SunoMetatagApp` → expect `EXIT=124`.

### T5 — Publish artifact rebuild + smoke launch

`dotnet publish ...` standard command. Expect `publish/tags.json` byte-identical to v1.10 (30,421 B); `publish/prompts.json` byte-identical to v1.10 (75,743 B); `publish/SunoMetatagApp.exe` minor delta from v1.10 (small TagService.cs source diff).

`timeout 6 ./publish/SunoMetatagApp.exe` → expect `EXIT=124`.

### T6 — USER REVIEW S1-S6

Surface S1-S6 matrix (spec §5.2) to the user. Critical cases:
- **S2** — visual confirmation of alphabetical order, with explicit Verse-triplet validation per Lead absorption #1 (`[Verse 1]`, `[Verse 2]` before `[Verse]`).
- **S5** — preview-pane → pills dim regression-gate? No — v1.11 doesn't touch the v1.10 picker-pane focus model. S5 in v1.11 is the picker-pane focus preserve regression-gate from v1.10 (click SearchBox → pills stay bright; click `[Verse]` → inserts into focused lyric).

### T7 — Wiki updates

Land 2 wiki updates in `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\`:

1. **`architecture/sunometatag-tag-library.md`** — new "Pill ordering (v1.11)" subsection documenting the sort contract + lexical-vs-numeric caveat (Lead absorption #2). Refresh `last_confirmed` + `review_due` frontmatter.
2. **`features/sunometatag-app.md`** — title bump v1.10 → v1.11; new `## v1.10 → v1.11 (2026-05-27)` subsection at the top.

### T8 — Consolidate execution log + rewrite ai/PLAN.md as RESULT packet

- Append v1.11 entry to `j:\SunoSongSetup\ai\EXECUTION_LOG.md` covering T0-T8.
- Archive v1.11 r1 plan packet as Archive entry 25 in `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md` (retention rule).
- Rewrite `j:\SunoSongSetup\ai\PLAN.md` as v1.11 RESULT packet.
