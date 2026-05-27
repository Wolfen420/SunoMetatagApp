# SunoMetatagApp v1.6 — sunoaiwiki Metatag List Reconciliation (B-SUNO-007)

- **Authored:** 2026-05-27
- **Type:** Content-only data curation slice (zero source-code changes)
- **Parent backlog:** B-SUNO-007 (Curate metatag expansion from Suno metatag reference list)
- **Source artifact:** [`docs/reference/suno-metatag-list-source-2026-05-27.md`](../reference/suno-metatag-list-source-2026-05-27.md) (immutable WebFetch capture of `https://sunoaiwiki.com/resources/2024-05-13-list-of-metatags/`; 81 items across 5 sections)
- **Curation artifact:** [`docs/reference/B-SUNO-007-decision-table.md`](../reference/B-SUNO-007-decision-table.md) (planner draft; lead-ratified at r1 approval)
- **Implementation plan:** [`docs/plans/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md`](../plans/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md)
- **Workflow precedent:** B-SUNO-005 / v1.4 (cheat-sheet reconciliation) + B-SUNO-006 / v1.5 (genre taxonomy); v1.6 reconciles a third source against the cumulative 286-entry library.

## 1. Goal

Import the sunoaiwiki metatag list as curated additions to existing categories via ADD/MERGE/SKIP decision-table workflow (v1.4 + v1.5 precedent). Grow `tags.json` from **286 → 331 entries** (final: 45 ADD / 8 MERGE / 30 SKIP / 83 decision rows from 81 source items + 2 split rows; planner draft was 46/29, T1 self-correction removed Clapping ADD after collision detection with existing v1.4 SFX entry). **No new top-level categories.** Extend 5 existing categories: Vocal (+5), Instrument (+4), Production (+2), SFX (+14 after Clapping correction), Genre (+20). Defer alias schema to **B-008** (consistent with v1.4 + v1.5 MERGE-as-no-op contract; total cumulative MERGE rows grow from 16 → 24).

## 2. Non-goals

- **No** alias/synonym schema (still deferred to B-008; v1.6 adds 8 new MERGE rows on top of cumulative 16 from v1.4).
- **No** hyphen/space-insensitive search normalization (that is **B-SUNO-009**, surfaced by v1.5 USER S6; explicitly scoped as alternative target_item at r1 review per Lead's v1.5 closeout directive — see §14 Q3).
- **No** UI / theme / behavioral changes. v1.1 inline + v1.2 visual + v1.3 stacked + v1.4 SFX + v1.5 Genre all preserved verbatim.
- **No** existing tag mutations. All 286 v1.5 entries preserved verbatim — no renames, no removals, no recategorizations.
- **No** new top-level category (unlike v1.4 SFX or v1.5 Genre). Library remains 8 categories.
- **No** changes to TagDefinition schema. Source-code zero-diff.

## 3. Curation rules

### 3.1 Source authority

[`docs/reference/suno-metatag-list-source-2026-05-27.md`](../reference/suno-metatag-list-source-2026-05-27.md) is the **immutable evidence layer**. All ADD/MERGE/SKIP claims in the decision table cite source section + raw label.

### 3.2 Canonicalization rules (inherited from v1.5, unchanged)

All v1.5 canonicalization rules from [[sunometatag-tag-library]] apply unchanged:
- Title-Case bracket form with mid-word conjunctions/prepositions lowercase; hyphens preserve both-segment Title-Case; acronyms preserve casing.
- "music" suffix removal (n/a in v1.6 source — no source labels end in " music").
- Established-abbreviation canonicalization (n/a in v1.6 source — no new abbreviations beyond `R&B` already covered in v1.5).
- Internal-duplicate handling (n/a — v1.6 source has no internal duplicates).
- Regional super-label policy (n/a — v1.6 source has no Regional section).
- Section-heading split (applied to 2 v1.6 source rows: "Christian & Gospel" and "Dance & Electronic" — see §3.4 below).

### 3.3 Cross-category collision policy

Many v1.6 source rows have existing semantic-equivalents in `tags.json` but in **different categories**. Adopted policy (planner default):

- **SKIP-as-canonical-present** when the source label already exists with the same bracket form (e.g., `Heavy metal` source label vs existing `[Heavy Metal]` in Genre — same bracket after Title-Case canonicalization).
- **MERGE-cross-category** when the source label exists in a different category as a semantically-related variant (e.g., source `Chill` labels as Style/Genre; existing `[Mood: Chill]` is in Mood category — different category, related semantics → MERGE for B-008 alias data).
- **ADD-new-category** when the source label categorizes the entry differently than the existing entry, AND adding a parallel entry would be useful (e.g., source `Lo-fi` as Style/Genre alongside existing `[Effect: Lo-fi]` as Effect — both useful as distinct bracket forms; ADD `[Lo-Fi]` to Genre coexisting with existing `[Effect: Lo-fi]`).

### 3.4 Split-row decisions (compound source labels)

Two v1.6 source rows in §5 carry compound labels with `&` separator. Adopted policy: each is **split into multiple decision-table rows** so each component is adjudicated independently.

- **"Christian & Gospel"** → split into row 13a (`Gospel` half → MERGE-as-canonical-present in v1.5 §J.3) + row 13b (`Christian` half → ADD `[Christian]` standalone). Mirrors v1.5 §J.0 "R&B & Soul" heading-split precedent.
- **"Dance & Electronic"** → split into row 16a (`Dance` half → ADD `[Dance]` standalone) + row 16b (`Electronic` half → SKIP-as-canonical-present in v1.5 §E.0).

These add 2 extra decision rows beyond the 81 source items (totaling 83 decision-table rows).

### 3.5 Source labels that overlap with v1.5 with different casing

Several v1.6 source labels match existing v1.5 entries except for capitalization (`Heavy metal` vs `[Heavy Metal]`, `Hip hop` vs `[Hip Hop]`, `J-pop` vs `[J-Pop]`, etc.). These are SKIP-as-canonical-present (v1.5 already chose the Title-Case canonical form per §3.2). No new MERGE rows because the underlying bracket form would be identical after applying v1.5 §3.2 Title-Case rule.

### 3.6 Source labels that overlap with v1.5 with different punctuation

One v1.6 source row matches an existing v1.5 entry with hyphenation difference: `Synth pop` (source, space) vs `[Synth-Pop]` (existing, hyphen). MERGE for B-008 alias data; existing entry stays canonical with hyphen.

## 4. Decision-table format

The decision table follows v1.4/v1.5 precedent and ships as `docs/reference/B-SUNO-007-decision-table.md`. Columns:

| Column | Purpose |
|---|---|
| Section | Source section name (verbatim from sunoaiwiki page) |
| Source label | Source row verbatim |
| Decision | `ADD` / `MERGE` / `SKIP` |
| Target category | Category for ADD rows; blank for SKIP/MERGE rows |
| Bracket form | Canonical `[Title Case]` if ADD |
| Rationale | One-liner per row: cite v1.5 canonicalization rules applied, existing-collision check result, source provenance |

Sub-totals per section; grand totals at file end. **T2 grep-recount discipline** (v1.4/v1.5 precedent): planner draft totals must be verified by `grep -c '"bracket":' src/SunoMetatagApp/Resources/tags.json` after T1 commit; any off-by-N caught at T2 triggers a hygiene commit.

## 5. Acceptance criteria (B-SUNO-007 close conditions)

1. **Source-of-truth file landed:** `docs/reference/suno-metatag-list-source-2026-05-27.md` exists and is unmodified post-T0.
2. **Decision table landed:** `docs/reference/B-SUNO-007-decision-table.md` exists with every source row adjudicated (83 rows from 81 source items + 2 split rows).
3. **tags.json grown:** 5 existing categories extended (Vocal +5, Instrument +4, Production +2, SFX +15, Genre +20 = 46 ADDs total); no new category; full file parses cleanly.
4. **Content-coverage tests pass:** 6 new tests (H1-H6, see §7.1); existing 70 tests still pass; total `>= 81` green (acknowledging 6 new + potential [Theory] expansion).
5. **Build green:** `dotnet build` clean.
6. **Publish smoke:** `publish/SunoMetatagApp.exe` rebuilt and launches without `JsonException` / `TagLoadException` / `XamlParseException`.
7. **USER REVIEW PASS:** 8-case manual smoke matrix (see §7.2) returns 8/8 PASS or PASS-WITH-CONCERN.
8. **No source-code mutations:** `git diff` against v1.5 tip (`c9fe7dc`) shows zero changes outside `tags.json`, `docs/`, and the new test file.

## 6. Risks

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| **R1** | Decision-table grand totals off-by-N (v1.4 precedent +1 off-by-one caught at T2; v1.5 had test-count drift caught at T2.5). | Low | T2 grep-recount + hygiene commit pattern; loose `>= N` thresholds for tests (H1 `>= 320`, H2 sum of category-extension assertions). |
| **R2** | Cross-category collision causes user confusion (e.g., `[Lo-Fi]` Genre vs `[Effect: Lo-fi]` Effect, `[Drums]` Instrument vs `[Drum Solo]` Structure). | Medium | Decision-table rationale documents per-row cross-category rationale. Two entries with different bracket forms are technically distinct in `tags.json` namespace; no actual collision. User-facing dropdown filters by category so confusion is bounded. |
| **R3** | "Boy" / "Girl" / "Man" / "Woman" voice-type ADDs could lower app's perceived professionalism if seen as overly generic. | Low | These are source labels verbatim from sunoaiwiki — "import-as-source" discipline applies. Decision-table rationale notes the source provenance. |
| **R4** | "African" ADD as Genre breaks v1.5 §3.7 regional super-label SKIP policy. | Low | v1.5 §3.7 SKIPped "African music" because of the music suffix + super-label-too-coarse rule. v1.6 source has bare "African" without "music" — different exact-source-label. ADD with rationale note documenting the v1.5 §3.7 policy exception. Lead may override at r1. |
| **R5** | "Christian" standalone ADD (from §5 row 13 split) may not be a canonical music-genre name; "Christian rock" / "Christian pop" are more common. | Low | Source has the compound label "Christian & Gospel" implying both halves are Genre-valid. Split-row rationale documents the v1.5 J.0 R&B+Soul precedent. Lead may override to SKIP or rename at r1. |
| **R6** | "Drums" / "Piano" / "Synth" added to Instrument category conflict with the source's labeling as Style/Genre. | Low | Source labeling appears imprecise (these are instruments, not genres); category-respect convention chooses Instrument. Decision-table rationale documents the source-vs-category disagreement. |
| **R7** | "Censored" / "Silence" added to Production category may be more appropriate as SFX. | Low | These are timing/production markers (silence) and post-production markers (censoring) — Production fits the existing schema better than SFX. Decision-table rationale documents. Lead-overridable. |
| **R8** | `[Whispers]` (plural SFX from §1 row 17) vs existing `[Whispered]` (vocal style) could confuse users. | Low | They're semantically distinct (sound effect of multiple whisperers vs a vocal-delivery style). Different brackets, different categories. Decision-table rationale documents the distinction. |
| **R9** | Cumulative MERGE row count for future B-008 reaches 24 (v1.4: 16 + v1.5: 0 + v1.6: 8) — adequate seed data for alias schema. | Informational | Not a risk; positive lineage. |

## 7. Test plan

### 7.1 Automated content-coverage tests (NEW: H1-H6)

New test file: `tests/SunoMetatagApp.Tests/TagServiceSunoaiwikiMetatagListTests.cs`.

| Test | Assertion | Notes |
|---|---|---|
| **H1** | `tags.json` entry count `>= 320` (planner-conservative; actual 331 after T1 Clapping self-correction). | Mirrors v1.4 C1, v1.5 G1 pattern. |
| **H2** | The 5 extended categories all grew per plan: Vocal `>= 45` (was 40, ADD +5), Instrument `>= 36` (was 32, +4), Production `>= 6` (was 4, +2), SFX `>= 63` (was 49, +14 after Clapping correction), Genre `>= 107` (was 87, +20). | Per-category extension verification. |
| **H3** | Representative new entries are searchable end-to-end via `TagService.Filter`: `Barking`, `Phone Ringing`, `Announcer`, `Female Narrator`, `Boy`, `Girl`, `Silence`, `EDM`, `Pop-Rock`, `Christmas`, `Lo-Fi`, `Drums` (12 representative entries across 5 extended categories). | Implemented as `[Theory]` with 12 inline rows (v1.5 G3 precedent). |
| **H4** | All 8 categories still non-empty (extends v1.5 G6 / v1.4 C6). | Regression guard. |
| **H5** | No bracket collisions across all 332 entries (extends v1.5 G5 / v1.4 C5). | Defensive uniqueness. |
| **H6** | All 4 Structural source items (Chorus, Intro, Outro, Verse) already exist in Structure category with their bracket forms — verifies SKIP-as-canonical-present decisions in §4 of decision table. | Specific to v1.6 SKIP discipline. |

Existing 70 v1.5 tests **all still pass** (70/70 → 70/70 + 6 new = expected `>= 81` if H3 [Theory] adds 12 inline rows: 70 + 5 [Fact] + 12 [Theory] = 87 test results).

### 7.2 USER REVIEW manual smoke matrix (S1-S8)

| # | Step | Expected outcome |
|---|---|---|
| **S1** | Launch `publish/SunoMetatagApp.exe`; verify category dropdown shows 8 categories with expected counts | All 8 categories present; v1.5 visual theme unchanged |
| **S2** | Filter to `SFX`; verify chip-row populates with at least 64 chips | `[Barking]`, `[Cough]`, `[Phone Ringing]`, etc. visible alongside v1.4 SFX entries |
| **S3** | Filter to `Genre`; verify ~107 chips | New ADDs (`[Lo-Fi]`, `[EDM]`, `[Christmas]`, `[Pop-Rock]`, `[Post-Hardcore]`, `[Christian]`, `[Dance]`) visible alongside v1.5 entries |
| **S4** | Plain-click `[Barking]` into section editor | `[Barking]` inserted; insertion behavior unchanged from v1.1 |
| **S5** | Shift+click `[Phone Ringing]` after existing `[Drum Break]` (or any bracket) | Stacked syntax `[Drum Break \| Phone Ringing]` works; v1.3 carry-over intact |
| **S6** | Search `lo-fi` and `lofi` | `lo-fi` returns both `[Lo-Fi]` (Genre) and `[Effect: Lo-fi]` (Effect); `lofi` returns nothing (B-SUNO-009 will fix this) |
| **S7** | Search `boy` | `[Boy]` (Vocal) surfaces; existing `[Boys Vocal]` if any also surfaces |
| **S8** | Copy preview verbatim; confirm v1.4 + v1.5 carry-over entries (`[Birdsong]`, `[Rock]`, `[K-Pop]`) all still work | No regression |

PASS criterion: 8/8 PASS (or PASS-WITH-CONCERN on S6 carrying forward v1.5 search-affordance limitation; that's expected and known). Single round target.

## 8. Implementation surfaces touched

### 8.1 Data files
- `src/SunoMetatagApp/Resources/tags.json` — **append** 46 entries distributed across 5 existing categories. Exact count from decision-table grand-total.
- `publish/tags.json` — re-published at T5.

### 8.2 Test files
- `tests/SunoMetatagApp.Tests/TagServiceSunoaiwikiMetatagListTests.cs` — new (H1-H6).

### 8.3 Doc files
- `docs/specs/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md` — this file.
- `docs/plans/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md` — implementation plan.
- `docs/reference/suno-metatag-list-source-2026-05-27.md` — immutable source capture (T0).
- `docs/reference/B-SUNO-007-decision-table.md` — curation decisions (T0).

### 8.4 Wiki updates (`j:\SunoSongSetup\.SunoSongSetup-wiki\`)
- `wiki/features/sunometatag-app.md` — version bump v1.5 → v1.6; new "v1.5 → v1.6 (2026-05-27)" subsection. `last_confirmed` refresh.
- `wiki/architecture/sunometatag-tag-library.md` — Categories table counts updated; new v1.6 reconciliation history row; cumulative MERGE rows for B-008 grow to 24; v1.6 H1-H6 validation gates added; source paths extended.
- `wiki/reference/ai-plan-archive.md` — Archive entry 14 prepended (v1.5 RESULT) + entry 15 prepended (this r1 plan packet → result conversion at T8).

### 8.5 Surfaces explicitly NOT touched
- `src/SunoMetatagApp/MainWindow.xaml(.cs)`
- `src/SunoMetatagApp/ViewModels/*.cs`
- `src/SunoMetatagApp/Services/TagService.cs` (literal-substring search semantics carry forward; B-SUNO-009 is the explicit alternative-or-follow-on for that fix)
- `src/SunoMetatagApp/Models/TagDefinition.cs` (no schema extension)
- All XAML / theme / converter files

## 9. Rollback plan

Single-commit revert. All changes ship in **2-3 commits** on `master`:
- Primary commit: `tags.json` + 4 doc artifacts (mirroring v1.4 `301c672` + v1.5 `939e611`).
- Secondary commit: H1-H6 test file (mirroring v1.4 `fadb7b9` + v1.5 `3ecdd5e`).
- Optional T2 hygiene commit if grep-recount finds off-by-N totals.

`git revert <primary-commit>` restores the v1.5 closeout tip (`c9fe7dc`) cleanly.

## 10. Wiki update commitment (closeout)

Closeout will declare:
```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-tag-library]], [[ai-plan-archive]]
```

All landed in-cycle; no queued-exception declarations expected. `wiki_sync_status: PASS` is the expected closeout state.

## 11. Open Decisions for Lead Ratification

Listed for explicit r1 ratification (planner defaults below; Lead may override any):

| # | Decision | Planner default | §ref |
|---|---|---|---|
| **Q1** | Cross-category collision policy: SKIP-canonical / MERGE-cross-cat / ADD-new-cat distinctions correct? | YES — three-rule policy in §3.3 | §3.3 |
| **Q2** | Split-row decisions for "Christian & Gospel" + "Dance & Electronic"? | YES — mirror v1.5 §J.0 heading-split precedent | §3.4 |
| **Q3** | `[African]` ADD despite v1.5 §3.7 regional super-label SKIP precedent? | YES — bare "African" without "music" suffix differs from v1.5's "African music" SKIP target | R4 |
| **Q4** | `[Christian]` standalone ADD (from compound `Christian & Gospel` split)? | YES — but Lead may prefer SKIP or `[Christian Rock]` alternative | R5 |
| **Q5** | `[Drums]` / `[Piano]` / `[Synth]` / `[Orchestra]` to Instrument (not Genre) despite source's Style/Genre labeling? | YES — category-respect over source-respect when source labeling appears imprecise | R6 |
| **Q6** | `[Censored]` / `[Silence]` to Production (not SFX or new category)? | YES — timing/production markers | R7 |
| **Q7** | `[Whispers]` SFX standalone (despite existing `[Whispered]` Vocal)? | YES — semantically distinct (sound effect vs vocal style) | R8 |
| **Q8** | `[Lo-Fi]` Genre ADD coexisting with `[Effect: Lo-fi]` Effect? | YES — different categories, different brackets, both useful | §3.3 example |
| **Q9** | `[Boy]` / `[Girl]` / `[Announcer]` / `[Female Narrator]` / `[Reporter]` voice-type ADDs to Vocal? | YES — verbatim from source per "import-as-source" discipline | R3 |
| **Q10** | Test threshold strategy: H1 `>= 320` (loose) for actual 332? | YES — same loose-threshold pattern as v1.4 (`>= 198` for 199) / v1.5 (`>= 270` for 286) | §7.1 |
| **Q11** | B-SUNO-009 (hyphen/space-insensitive search) as alternative target_item for Lead override? | NO — v1.6 primary path. Lead may override to switch focus, but planner default proceeds with v1.6 content slice. | §14 closeout |

## 12. Conclusion

v1.6 is a **straight content-only release** in the v1.4/v1.5 mold — 46 ADDs across 5 existing categories, 8 new MERGE rows for B-008 alias data, 29 SKIP-as-canonical-present decisions (heavily driven by Structural Tags overlap + v1.5 Genre overlap). Risk profile is **low** because v1.4 + v1.5 already validated this curation pattern end-to-end with first-try USER REVIEW PASSes on both. **No new categories** unlike v1.4 (SFX) or v1.5 (Genre); v1.6 deepens existing categories. The reconciliation methodology + canonicalization rules from v1.5 [[sunometatag-tag-library]] apply unchanged. **B-SUNO-009 surfaces as alternative scope** per Lead's v1.5 closeout target_item — see §14 closeout Q3; Lead may switch target at r1 if hyphen/space-insensitive search is higher priority than v1.6 content expansion.
