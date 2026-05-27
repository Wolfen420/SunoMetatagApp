# SunoMetatagApp v1.7 — Search Normalization (B-SUNO-009)

- **Authored:** 2026-05-27
- **Type:** Source-code change slice (focused `TagService.Filter` enhancement)
- **Parent backlog:** B-SUNO-009 (Hyphen/space-insensitive search normalization). Surfaced by v1.5 USER REVIEW S6 PASS-WITH-CONCERN (`kpop` did not match `[K-Pop]`); kept as scoped follow-on through v1.5 + v1.6 closeouts.
- **Source files modified:** [`src/SunoMetatagApp/Services/TagService.cs`](../../src/SunoMetatagApp/Services/TagService.cs) (lines 79-84 + add private helper)
- **Implementation plan:** [`docs/plans/2026-05-27-suno-metatag-v1.7-search-normalization.md`](../plans/2026-05-27-suno-metatag-v1.7-search-normalization.md)
- **Precedent shape:** First source-code slice after 3 consecutive content slices (v1.4 / v1.5 / v1.6). Closest prior-shape precedent: **v1.3** (B-SUNO-004 stacked syntax) — focused source-code change with new test class + USER REVIEW smoke matrix.

## 1. Goal

Make `TagService.Filter` search **hyphen/space-insensitive** so that typing `kpop` surfaces `[K-Pop]`, `poprock` surfaces `[Pop-Rock]`, `lofi` surfaces both `[Lo-Fi]` (Genre) and `[Effect: Lo-fi]` (Effect), `drumandbass` surfaces `[Drum and Bass]`, etc.

The fix:
- Add a `private static string NormalizeForSearch(string s)` helper to `TagService` that strips `-` and ` ` from a string.
- Apply normalization to **both** the query AND each compare target (Label, Bracket) in `searchMatches` before the case-insensitive `Contains` check.
- Preserve case-insensitive comparison semantics (`StringComparison.OrdinalIgnoreCase`) already in use.

This is a **strict superset behavior change**: every query/target pair that matched under the v1.4-v1.6 literal-substring behavior still matches under v1.7 normalization; new matches surface for hyphenated/spaced source labels typed without their punctuation. No regression class introduced.

## 2. Non-goals

- **No** tags.json changes. The 331 entries are unchanged.
- **No** new categories, no new entries, no new MERGE/SKIP rows.
- **No** schema extension to `TagDefinition` (no aliases, no description fields).
- **No** `TagService.DistinctCategories` or `TagService.LoadAll` changes.
- **No** UI / theme / ViewModel changes. Picker search box wiring at [`MainViewModel.cs`](../../src/SunoMetatagApp/ViewModels/MainViewModel.cs) calls `TagService.Filter`; no change needed at the call site.
- **No** broader fuzzy-search (Levenshtein, prefix tries, etc.). v1.7 is **narrow**: hyphen + space normalization only. Future B-SUNO-010+ could explore fuzzy if user demand surfaces.
- **No** alias schema landing (`B-008` remains pending; 24 cumulative MERGE rows still data-only until B-008 ships).
- **No** carry-over backlog item resolution. B-026/B-027/B-028/B-SUNO-NNN/mojibake/`×` glyph all continue to Lead reconciliation.

## 3. Normalization rules

### 3.1 What gets normalized

**Stripped characters:** `-` (hyphen) and ` ` (single ASCII space). Both characters are removed from the query AND from each target compare field (Label, Bracket) before case-insensitive `Contains` evaluation.

**Not stripped:** brackets (`[`/`]`), colons (`:`), ampersands (`&`), apostrophes (`'`), commas (`,`), or any other punctuation. Rationale: only hyphens and spaces have surfaced as user-perceived friction in v1.5 USER S6; broader stripping could create unexpected matches.

### 3.2 Comparison semantics preserved

- Case-insensitive (`StringComparison.OrdinalIgnoreCase`) — unchanged from v1.4-v1.6.
- Substring match (`Contains`) — unchanged from v1.4-v1.6.
- Match against `Label` OR `Bracket` — unchanged from v1.4-v1.6.
- Empty/null search returns all (after category filter) — unchanged from v1.4-v1.6.

### 3.3 Concrete normalization examples

| Query (raw) | Query (normalized) | Target Bracket (raw) | Target Bracket (normalized) | Match (case-insensitive)? |
|---|---|---|---|---|
| `kpop` | `kpop` | `[K-Pop]` | `[KPop]` | ✓ new in v1.7 |
| `k-pop` | `kpop` | `[K-Pop]` | `[KPop]` | ✓ already worked in v1.6 |
| `K Pop` | `KPop` | `[K-Pop]` | `[KPop]` | ✓ new in v1.7 |
| `poprock` | `poprock` | `[Pop-Rock]` | `[PopRock]` | ✓ new in v1.7 |
| `lofi` | `lofi` | `[Lo-Fi]` | `[LoFi]` | ✓ new in v1.7 (also matches `[Effect: Lo-fi]`) |
| `verse` | `verse` | `[Verse]` | `[Verse]` | ✓ already worked |
| `electric guitar` | `electricguitar` | `[Electric Guitar]` | `[ElectricGuitar]` | ✓ already worked (via raw substring) AND new normalized path |
| `electricguitar` | `electricguitar` | `[Electric Guitar]` | `[ElectricGuitar]` | ✓ new in v1.7 |
| `drumandbass` | `drumandbass` | `[Drum and Bass]` | `[DrumandBass]` | ✓ new in v1.7 |
| `r&b` | `r&b` | `[R&B]` | `[R&B]` | ✓ already worked (ampersand not stripped) |

### 3.4 What does NOT change

- Searching `verse` for `[Verse]` — still works (was already a literal substring match).
- Searching `mood: e` for `[Mood: Euphoric]` — still works (space-normalized to `mood:e` matches normalized `[Mood:Euphoric]` substring).
- Searching `[k-pop]` (with brackets) for `[K-Pop]` — still works (normalized `[kpop]` substring matches normalized `[KPop]` case-insensitive).
- Category filter — unchanged. Searching `kpop` with category `Genre` filter still returns only Genre matches.

## 4. Acceptance criteria

1. **Source change landed:** `src/SunoMetatagApp/Services/TagService.cs` has a private static `NormalizeForSearch` helper + updated `searchMatches` closure using it.
2. **All 94 v1.6 baseline tests still green** (no regression on prior behavior).
3. **New content-coverage tests pass:** 6 new tests (N1-N6, see §6.1); total `>= 100` green.
4. **Build green:** `dotnet build` clean.
5. **Publish smoke:** `publish/SunoMetatagApp.exe` rebuilt and launches without exception.
6. **USER REVIEW PASS:** 8-case manual smoke matrix (see §6.2) returns 8/8 PASS. Specific carry-forward verification of v1.5 S6 / v1.6 R10 cases.
7. **No data file mutations:** `git diff tags.json` shows zero changes.
8. **No XAML/ViewModel mutations:** `git diff` against v1.6 tip (`30e3b52`) shows source-code changes confined to `Services/TagService.cs` and the new test file.

## 5. Risks

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| **R1** | Performance regression on Filter calls — normalization runs every keystroke against 331 entries. | Low | At 331 entries × ~10-char avg Label/Bracket, normalization is O(N·M) ≈ 3300 character ops per keystroke = microseconds. WPF debouncing on TextBox typing already amortizes. No mitigation needed; verify at T4 smoke. |
| **R2** | False-positive matches: `[Drum and Bass]` becomes `drumandbass` after normalization — could match unexpected queries like searching for "andba". | Low | Substring-match-against-normalized-target is consistent with prior semantics; user query "andba" would also have been a substring of the raw `[Drum and Bass]`. No new false-positive class introduced; just expanded match surface. |
| **R3** | `[Effect: Lo-fi]` becomes `[Effect:Lo-fi]` normalized (colon adjacent to "Lo-fi"). Searching `lofi` matches both `[Lo-Fi]` Genre AND `[Effect: Lo-fi]` Effect — is the cross-category coexistence intent preserved? | Low | Both entries surface in search results; user filters by category dropdown to disambiguate. This is the v1.6 §3.3 ADD-new-category coexistence policy working as designed. |
| **R4** | Search becomes "too permissive" — users may expect literal matching for precise lookups. | Low | The change is purely a search expansion; users typing exact source labels (with hyphens/spaces) get the same results. Power users seeking precise lookup can still type the full hyphenated form. Worst case: surface friction prompts a future v1.8 toggle/escape mechanism. |
| **R5** | "Search performance" subjective acceptability — does the search "feel" different to users? | Low | T6 USER REVIEW tests with concrete cases. v1.5 S6 PASS-WITH-CONCERN was the documented user-friction case; v1.7 specifically resolves it. |
| **R6** | Could affect any other surface that calls `TagService.Filter`. | Low | `MainViewModel.FilteredTags` is the only call site (verified by grep). Picker UI binding to `FilteredTags` is unchanged. |
| **R7** | Test threshold confusion — H1/G1/C1 count tests reference tags.json sizes, not test results. v1.7 adds tests but doesn't change tags.json; H1/G1/C1 remain satisfied. | Informational | No threshold needs adjustment. |

## 6. Test plan

### 6.1 Automated content-coverage tests (NEW: N1-N6)

New test file: `tests/SunoMetatagApp.Tests/TagServiceSearchNormalizationTests.cs`.

| Test | Assertion | Notes |
|---|---|---|
| **N1** | `Filter("kpop", "All")` returns `[K-Pop]` Genre. | The v1.5 S6 PASS-WITH-CONCERN case. |
| **N2** | `Filter("poprock", "All")` returns `[Pop-Rock]` Genre. | v1.6 hyphenated entry. |
| **N3** | `Filter("lofi", "All")` returns BOTH `[Lo-Fi]` Genre AND `[Effect: Lo-fi]` Effect (cross-category coexistence). | Cross-category exhibit. |
| **N4** ([Theory], 8 inline rows) | Hyphen and space variants all match canonical bracket: `K Pop` → `[K-Pop]`; `kpop` → `[K-Pop]`; `K-Pop` → `[K-Pop]` (regression); `drumandbass` → `[Drum and Bass]`; `drum and bass` → `[Drum and Bass]` (regression); `electricguitar` → `[Electric Guitar]`; `singersongwriter` → `[Singer-Songwriter]`; `posthardcore` → `[Post-Hardcore]`. | All 9 hyphenated entries (6 from v1.5 + 3 from v1.6) covered + a few space-containing entries. |
| **N5** | `Filter("kpop", "Genre")` returns `[K-Pop]` and ONLY Genre-category entries (no Effect/Vocal bleed). | Category filter still works after normalization. |
| **N6** | `Filter("", "All")` returns all 331 entries (empty-search regression). | Edge case: empty search returns all. |

Existing 94 v1.6 baseline tests **all still pass** (no regression).

### 6.2 USER REVIEW manual smoke matrix (S1-S8)

| # | Step | Expected outcome |
|---|---|---|
| **S1** | Launch `publish/SunoMetatagApp.exe`; open category-filter dropdown; verify 8 categories with v1.6 counts | All unchanged from v1.6; visual theme unchanged |
| **S2** | Search box: type `kpop` (no hyphen) | `[K-Pop]` Genre surfaces (the v1.5 S6 PASS-WITH-CONCERN case — now PASS) |
| **S3** | Search box: clear; type `poprock` (no hyphen) | `[Pop-Rock]` Genre surfaces |
| **S4** | Search box: clear; type `lofi` (no hyphen) | BOTH `[Lo-Fi]` Genre AND `[Effect: Lo-fi]` Effect surface (cross-category coexistence) |
| **S5** | Search box: clear; type `drumandbass` (no spaces) | `[Drum and Bass]` Genre surfaces |
| **S6** | Search box: clear; type `singersongwriter` (no hyphen) | `[Singer-Songwriter]` Genre surfaces |
| **S7** | Search box: clear; type `verse` then click `[Verse]` to insert | Existing v1.1-v1.6 search behavior unchanged; plain-click insertion works |
| **S8** | Filter to Genre category; type `kpop` in search | Only `[K-Pop]` surfaces (category filter still applies; cross-category Effect entry not shown) |

PASS criterion: 8/8 PASS first round. **S2 is the explicit user-validation that B-SUNO-009 resolved the v1.5 PASS-WITH-CONCERN.**

## 7. Implementation surfaces touched

### 7.1 Source files
- `src/SunoMetatagApp/Services/TagService.cs` — add `NormalizeForSearch` helper + update `searchMatches` closure. Estimated ~8 lines net addition (1 helper method + 3 lines edited in `Filter`).

### 7.2 Test files
- `tests/SunoMetatagApp.Tests/TagServiceSearchNormalizationTests.cs` — new (N1-N6 with N4 as [Theory]).

### 7.3 Doc files
- `docs/specs/2026-05-27-suno-metatag-v1.7-search-normalization.md` — this file.
- `docs/plans/2026-05-27-suno-metatag-v1.7-search-normalization.md` — implementation plan.

### 7.4 Wiki updates (`j:\SunoSongSetup\.SunoSongSetup-wiki\`)
- `wiki/features/sunometatag-app.md` — version bump v1.6 → v1.7; new "v1.6 → v1.7 (2026-05-27)" subsection. `last_confirmed` refresh.
- `wiki/architecture/sunometatag-tag-library.md` — Update "Literal-substring search (no hyphen/space normalization)" subsection: rename to "Search normalization (hyphen/space-insensitive, v1.7+)" or similar; rewrite content to describe the new behavior; remove the "B-SUNO-009 implementation note" since the slice has landed; add note that 24 cumulative MERGE rows would still benefit from B-008 alias schema for cross-category aliases that aren't hyphen/space variants.
- `wiki/reference/ai-plan-archive.md` — Archive entry 16 prepended at r1 draft (v1.6 RESULT) + entry 17 prepended at T8 (this r1 plan packet).

### 7.5 Surfaces explicitly NOT touched
- `src/SunoMetatagApp/Resources/tags.json` (zero changes)
- `src/SunoMetatagApp/MainWindow.xaml(.cs)`
- `src/SunoMetatagApp/ViewModels/MainViewModel.cs` (search-box binding unchanged)
- `src/SunoMetatagApp/Models/TagDefinition.cs` (schema unchanged)
- All other source files in `src/`

## 8. Rollback plan

Single-commit revert. v1.7 ships in **1-2 commits** on `master`:
- Primary commit: `TagService.cs` source change + spec + plan.
- Secondary commit: N1-N6 test file.

`git revert <primary-commit>` restores v1.6 closeout tip (`30e3b52`) cleanly. Search behavior immediately reverts to literal-substring matching.

## 9. Wiki update commitment (closeout)

Closeout will declare:
```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-tag-library]], [[ai-plan-archive]]
```

All landed in-cycle; no queued-exception declarations. `wiki_sync_status: PASS` expected.

## 10. Open Decisions for Lead Ratification

| # | Decision | Planner default | §ref |
|---|---|---|---|
| **Q1** | Normalize only `-` and ` ` characters? Or also strip other punctuation (`[`/`]`, `:`, etc.)? | Only `-` and ` `; broader stripping reserved for future fuzzy-search slice. | §3.1 |
| **Q2** | Normalize the SEARCH QUERY only, or also the TARGET (Label + Bracket)? | Both — needed for the v1.5 S6 case (`kpop` query → `[K-Pop]` target). | §3.1 |
| **Q3** | Make normalization configurable (toggle on/off via setting)? | NO — single behavior; future toggle can be added if user pressure surfaces. | (NOT in scope) |
| **Q4** | Performance: should `Filter` cache normalized Label+Bracket per `TagDefinition` to avoid re-normalizing each call? | NO — 331 entries × ~10 chars is negligible. Premature optimization. Could be added later if profiling shows hotspot. | R1 |
| **Q5** | Test threshold for N1-N6 — exact-equality or `>=` pattern? | Exact-equality for `Filter` return contents (assert that `[K-Pop]` IS in results); not count-thresholds. | §6.1 |
| **Q6** | B-SUNO-008 alternative path (Lead-noted as alternative in v1.6 closeout target_item) — should planner draft B-SUNO-008 in parallel, or sequence after v1.7 ships? | Sequence after v1.7. B-SUNO-009 is small and surgical (~10 LOC source change); ship cleanly before tackling B-SUNO-008 prompt library (larger curation slice). | §14 |

## 11. Conclusion

v1.7 is a **focused source-code change slice** — first source-code slice since v1.3. ~10 LOC net change to `TagService.cs` Filter method; 6 new content-coverage tests; zero `tags.json` mutations; zero UI/theme/ViewModel changes. Strict superset behavior change (no regression class). **Resolves the v1.5 S6 PASS-WITH-CONCERN class permanently** across all 9 hyphenated entries (6 v1.5 + 3 v1.6).

Risk profile is low because the change is small, isolated, well-tested at the unit level, and has a clean rollback path. The B-SUNO-008 prompt library alternative path is sequenced after — v1.8+ when v1.7 is closed.
