# Implementation Plan — SunoMetatagApp v1.4 (Cheat-Sheet Reconciliation)

**Spec:** `docs/specs/2026-05-27-suno-metatag-v1.4-cheatsheet-reconciliation.md`
**Decision table:** `docs/reference/B-SUNO-005-decision-table.md` (~120 rows; Lead-ratifiable at plan-phase review)
**Cheat-sheet source:** `docs/reference/suno-cheat-sheet-2026-05-26.md` (user-pasted 2026-05-27, immutable)
**Backlog item:** B-SUNO-005
**Date:** 2026-05-27
**Baseline commit:** `ec9e19f` (v1.3 closeout tip on `master`)
**Estimated scope:** 2 commits (T1 tags.json content + T2 6 new unit tests); zero source-code changes; 1 new wiki page; 75 new tag entries (as-landed; initial planner estimate was 74); tags.json grows 124 → 199.

## Pre-execution baseline

- `j:\SunoMetatagApp\` `master` tip = `ec9e19f` (v1.3 fix-XAML-rewire closeout).
- Working tree state at start: untracked spec/plan/reference files for B-SUNO-005 (will be committed at T1/T2 alongside source changes).
- `dotnet build` green; `dotnet test` 47/47 passing on v1.3 baseline.
- `publish/SunoMetatagApp.exe` reflects v1.3 build; will be re-published at T7 with v1.4 tag library.

## Task list

### T0 — Verify clean baseline + decision-table ratification

- `git status` on `j:\SunoMetatagApp\` shows clean working tree except for the untracked B-SUNO-005 spec/plan/cheat-sheet/decision-table files (these will be tracked at T1/T2).
- `git log -1 --oneline` confirms tip is `ec9e19f`.
- `dotnet build` green.
- `dotnet test` 47/47 passing.
- **Confirm** Lead-ratified decision table (`docs/reference/B-SUNO-005-decision-table.md`) is final — if Lead revised any rows during plan-phase review, ensure the file reflects the ratified version before T1.

**No commit at T0.** Baseline verification only.

### T1 — Apply ratified decision table to tags.json

File: `src/SunoMetatagApp/Resources/tags.json`

Per the ratified decision table, mechanically apply ADD entries (74 new objects per planner-proposed totals; may differ if Lead ratifies row overrides) to the end of the JSON array. **Do NOT touch existing entries** (lines 1–131 of current tags.json preserved verbatim). Insertion approach:

1. Read the existing tags.json (124 entries).
2. For each ADD row in the decision table (in document order: B.1 then B.2 then B.3 ... then C, then D):
   - Append `{ "category": "<target>", "label": "<label>", "bracket": "<bracket>" }` (description field optional; include only if decision table rationale explicitly suggests one).
3. Group ADDs by target category in the file structure: keep the existing structure-vocal-instrument-mood-effect-production order, then add an SFX section at the end.
4. Preserve the existing 1-line-per-entry JSON formatting and blank-line section separators.
5. Re-save tags.json.
6. Verify JSON parses cleanly: `dotnet build` (since the file is consumed at app startup, parsing happens via `JsonSerializer.Deserialize<TagDefinition[]>` in `TagService.LoadAll`).

Also commit:
- `docs/specs/2026-05-27-suno-metatag-v1.4-cheatsheet-reconciliation.md` (this slice's spec)
- `docs/plans/2026-05-27-suno-metatag-v1.4-cheatsheet-reconciliation.md` (this plan)
- `docs/reference/suno-cheat-sheet-2026-05-26.md` (cheat-sheet source-of-truth)
- `docs/reference/B-SUNO-005-decision-table.md` (Lead-ratified decision table)

**Commit message:** `feat(tags): apply ratified B-SUNO-005 cheat-sheet reconciliation (+85 entries, +SFX category)`

**Expected delta:** tags.json ~+85 lines; docs/ ~+~700 lines (spec + plan + reference + decision table); 0 deleted lines.

### T2 — Add v1.4 content-coverage unit tests

File: `tests/SunoMetatagApp.Tests/TagServiceCheatSheetTests.cs` (new)

Add 6 tests (C1–C6 per spec §7.2):

```csharp
public class TagServiceCheatSheetTests
{
    private static IReadOnlyList<TagDefinition> LoadTagsJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
        return TagService.LoadAll(path);
    }

    [Fact]
    public void C1_LoadAll_LoadsExpectedMinimumCount()
    {
        var tags = LoadTagsJson();
        Assert.True(tags.Count >= 198, $"Expected >= 198 tags after B-SUNO-005, got {tags.Count}.");
    }

    [Fact]
    public void C2_DistinctCategories_IncludesSFX()
    {
        var tags = LoadTagsJson();
        var categories = TagService.DistinctCategories(tags);
        Assert.Contains("SFX", categories);
    }

    [Fact]
    public void C3_Filter_FindsNewSFXTag_Birdsong()
    {
        var tags = LoadTagsJson();
        var hits = TagService.Filter(tags, "Birdsong", "All").ToList();
        Assert.Contains(hits, t => t.Bracket == "[Birdsong]");
    }

    [Fact]
    public void C4_Filter_BySFXCategory_ReturnsOnlySFXEntries()
    {
        var tags = LoadTagsJson();
        var hits = TagService.Filter(tags, "", "SFX").ToList();
        Assert.NotEmpty(hits);
        Assert.All(hits, t => Assert.Equal("SFX", t.Category));
    }

    [Fact]
    public void C5_LoadAll_NoBracketCollisions()
    {
        var tags = LoadTagsJson();
        var dupBrackets = tags.GroupBy(t => t.Bracket)
                              .Where(g => g.Count() > 1)
                              .Select(g => g.Key)
                              .ToList();
        Assert.Empty(dupBrackets);
    }

    [Fact]
    public void C6_LoadAll_AllEntriesHaveNonEmptyCategory()
    {
        var tags = LoadTagsJson();
        Assert.All(tags, t => Assert.False(string.IsNullOrWhiteSpace(t.Category)));
    }
}
```

Note: tests use `Path.Combine(AppContext.BaseDirectory, "tags.json")` — same pattern as existing `TagServiceTests.cs` if it uses one. Verify the existing TagServiceTests pattern at T2 start and align.

**Commit message:** `test: add 6 content-coverage tests for B-SUNO-005 tags.json expansion (C1-C6)`

**Expected delta:** ~+70 LOC test code; 0 deleted lines. Test count 47 → 53.

### T3 — Run full test suite

- `dotnet build` green.
- `dotnet test` from `j:\SunoMetatagApp\`.
- Expected: **53/53 passing** (31 v1 + 16 v1.3 + 6 v1.4).

**Commit:** none at T3 (validation only).

**If tests fail:** diagnose; likely root causes:
- JSON parse error in tags.json (trailing comma, unbalanced brackets) → fix tags.json; re-test.
- Bracket collision detected by C5 → review decision-table application; if a collision survived, treat as a planning oversight and move offending entry to SKIP. Re-test.
- Count mismatch on C1 → recount applied ADDs in the table vs file; ensure exact match.

### T4 — Dev smoke-launch

- `dotnet run --project src/SunoMetatagApp --no-build` from `j:\SunoMetatagApp\`.
- Verify in dev console:
  - No exceptions on startup (especially `JsonException` or `TagLoadException`).
  - Category dropdown contains `SFX` alongside Structure/Vocal/Instrument/Mood/Effect/Production.
  - Selecting `SFX` populates the picker with new SFX entries.
  - Plain click + Shift+click work on a new entry (e.g. `[Birdsong]`) per v1.1 + v1.3 behaviors.
  - Visual layer unchanged from v1.3 (fuchsia pills, violet focused border, dark theme).

**If smoke-launch fails:** debug + fix and re-commit. Do not proceed to T5 until clean.

**Commit:** none at T4 unless a fix is needed.

### T5 — Publish single-file self-contained exe

```
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true -o publish
```

Verify:
- `publish/SunoMetatagApp.exe` exists (~146 MB; size shouldn't grow appreciably because tags.json content is small).
- `publish/tags.json` exists (CopyToOutput; reflects v1.4 content).
- Launch published exe and confirm clean startup + SFX category visible.

**Commit:** none at T5 (publish output is in `.gitignore`).

### T6 — USER REVIEW (8-case smoke matrix)

Hand off the published exe to the user with the smoke matrix from spec §7.3 (S1–S8):

1. New SFX category appears in dropdown.
2. SFX category populates picker with new SFX pills.
3. Plain + Shift+click work on a new SFX tag.
4. Search "build" finds 5 entries (new + existing mix).
5. Search "drum" finds 5 entries (new + existing mix).
6. Copy preview output renders new + existing tags verbatim.
7. All v1.3 carry-over (13 smoke cases) still pass — no regression.
8. Existing prompts referencing old tags still work — backwards compat.

**Surface header:** `USER REVIEW NEEDED` with numbered steps + per-case PASS/FAIL response format.

**Branch logic:**
- All 8 PASS → proceed to T7.
- Any FAIL → diagnose, fix, re-publish, re-smoke. Document each round in the RESULT packet §2 Deviations table.

### T7 — Wiki updates

After USER REVIEW PASS:

1. **UPDATE** `.SunoSongSetup-wiki/wiki/features/sunometatag-app.md`:
   - Bump title to "(v1.4 — Suno Cheat-Sheet Reconciliation on Stacked + Visual + Inline)".
   - Add new "v1.3 → v1.4 (2026-05-27)" subsection above the v1.2→v1.3 subsection. Document the 74 ADDs, +1 new SFX category, MERGE-as-no-op semantics, deferred-alias deferral to B-008, and total tag count 124 → 198.
   - Refresh `last_confirmed: 2026-05-27`, `updated: 2026-05-27`.

2. **NEW page (proposed in spec §6.3):** `.SunoSongSetup-wiki/wiki/architecture/sunometatag-tag-library.md` — describes:
   - Categories model (7 categories post-v1.4).
   - Naming-convention mixed forms (bare canonical for new ADDs; existing `[Prefix: X]` preserved).
   - JSON schema (`{ category, label, bracket, description? }`).
   - Alias support status (deferred to B-008; MERGE is no-op for v1.4).
   - Reconciliation history (link to [[sunometatag-app]] v1.4 subsection + reference files in sibling repo).
   - Frontmatter: `type: architecture`, `status: validated`, `tags: [sla, architecture, sunometatag, content]`.
   - **Lead-ratifiable alternative:** skip new page; just extend [[sunometatag-app]]. If Lead rejects the new page at plan-phase, this task collapses to update-only.

### T8 — Rewrite ai/PLAN.md as RESULT packet

- Archive the B-SUNO-005 r1 plan packet (or latest approved revision) as Archive entry 11 in `.SunoSongSetup-wiki/wiki/reference/ai-plan-archive.md` (archive-before-edit discipline).
- Append v1.4 execution entry to `ai/EXECUTION_LOG.md`.
- Rewrite `ai/PLAN.md` as the v1.4 RESULT packet using the standard 17-section template.
- Include USER ACTION NEEDED with Step 1 Specialist + Step 2 Lead handoff text.

## Validation gates

| Gate | Check | Block on failure? |
|---|---|---|
| T0 | Baseline green (build + 47 tests) + decision table ratified | Yes |
| T3 | 53/53 tests passing | Yes |
| T4 | Dev smoke-launch (JSON parses, SFX category appears, no exceptions) | Yes |
| T5 | Publish exe builds + launches | Yes |
| T6 | USER REVIEW PASS on 8-case matrix | Yes (iterate until PASS) |
| T7 | Wiki pages updated with lifecycle frontmatter | Yes |
| T8 | RESULT packet written + Archive entry 11 + EXECUTION_LOG appended | Yes |

## Rollback plan

If at any point the v1.4 expansion cannot be made to work reliably:

1. `git revert <T1-commit-hash>` to undo the tags.json mutation. (T2 commit is just new tests, harmless to keep.)
2. v1.3 visual + behavioral + content state restored exactly.
3. Surface failure to user via USER ACTION NEEDED with diagnosis + recommendation (e.g., restage with smaller ADD batch, defer to v1.5 in separate batches, escalate decision-table rows that caused issues).

Rollback is **safe and fully reversible** because:
- v1.4 changes are confined to `tags.json` content + new test file + docs.
- No `Themes/`, no `Views/`, no `ViewModels/`, no source-code changes.
- No new dependencies.
- No package version bumps.

## Risks

| Risk | Likelihood | Mitigation |
|---|---|---|
| Decision-table row produces bracket collision with existing entry | Low (table reviewed at Lead plan-phase) | C5 test enforces uniqueness; treat colliding row as SKIP and re-run. |
| JSON parse error from hand-applied table | Medium (74 entries appended) | Apply in small batches per cheat-sheet section (B.2 → B.3 → B.4 → B.5 → B.6 → C → D.1 → D.2) with `dotnet build` after each batch to catch parse errors at JSON deserialization (T0 baseline check + per-batch verification). Specialist LOW 3 absorbed: hand-edit batched, not script-driven. |
| Performance regression on 209-tag library | Low (B-011 threshold is 300+ tags) | C1 test verifies count; smoke test confirms picker responsiveness. |
| User pushback on naming convention (bare vs prefix forms) at USER REVIEW | Low (planner-default is canonical Suno form per cheat sheet) | User can request rename in follow-on slice; v1.4 doesn't lock naming permanently. |
| Lead rejects new SFX category in plan-phase review | Low (planner-default is clean taxonomy) | If rejected, fall back to placing SFX entries under existing Effect category; rename target column in decision table; re-apply table. |

## Specialist activation rationale

**FRONTEND/UX activation recommended** for v1.4 plan-phase review:

- The decision table affects what tags users see + click; UX surface is broad.
- Naming convention (bare vs prefix forms) is a UX-discoverability question.
- New SFX category affects category dropdown ordering + searchability.
- Composite "Specific Elements" ADD/SKIP decisions (§4 of spec) affect whether users see redundant entries vs use v1.3 stacked syntax.

**ENGINE specialty NOT activated** — no geometry, determinism-sensitive, or export-contract changes; this is pure data curation.

## Wiki impact forecast

- **UPDATE** [[sunometatag-app]] (v1.3 → v1.4 subsection + title bump)
- **NEW (proposed)** [[sunometatag-tag-library]] (tag library reference page; planner-default; Lead can collapse to update-only if preferred)
- **APPEND** [[ai-plan-archive]] (Archive entry 11 at closeout)

**No other wiki pages will become stale.** v1.3 visual + stacked-syntax + focus-flip-stale-insert pages remain accurate (v1.4 is data-only).
