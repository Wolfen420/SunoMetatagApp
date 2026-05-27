# SunoMetatagApp v1.9 — Spec (B-SUNO-008b Full Prompt Library Curation)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-008b / v1.9 — Full curation of the awesome-suno-prompts corpus on top of the v1.8 mechanism + seed
- **Type:** **Pure data curation slice** — zero source-code changes; zero schema changes; zero UI changes; mirrors v1.4 / v1.5 / v1.6 "content-only release" discipline
- **Decision packet authority:** Lead-ratified `D-2026-05-27-B-SUNO-008-scope-phasing` Option A — v1.9 is the explicit continuation slice (B-SUNO-008b) named in the decision's `consequences[]` ("Full corpus curation from awesome-suno-prompts (~136 prompts) is deferred to follow-on B-SUNO-008 continuation slice with decision-table workflow")
- **Working baseline:** `master` tip `25f8f2e` (v1.8 closeout = B-SUNO-008a PASS)
- **Specialist activation forecast:** FRONTEND/UX (discoverability check at 136-entry density on the existing UI surface); ENGINE remains out of scope (no schema / no source-code changes; still deterministic JSON read)
- **Source-of-truth:** `https://github.com/naqashmunir21/awesome-suno-prompts` (license CC0-1.0; default branch `main`; **commit SHA `e1d1247bd26f896127011d3bbc2ba8599d54960d`** — same SHA captured at v1.8; no upstream changes between v1.8 and v1.9 capture)

## 1. Goal

Replace the v1.8 16-entry seed corpus with the **full curated 136-entry corpus** from awesome-suno-prompts via a per-row ADD/SKIP decision table. v1.9 closes the parent backlog item `B-SUNO-008` (Add curated pre-made prompt library) by completing the second of the two phases declared in `D-2026-05-27-B-SUNO-008-scope-phasing` Option A:

- **B-SUNO-008a (v1.8, shipped):** mechanism + 16-entry seed.
- **B-SUNO-008b (this slice, v1.9):** full curation, replacing seed with the complete 136-entry corpus.

**Resolves backlog item:** parent B-SUNO-008 retires at v1.9 closeout.

## 2. Scope

### What this slice covers

- **`Resources/prompts.json` replaced** with 136 entries drawn from all 8 source files via the ADD/SKIP decision table. The 16 v1.8 seed entries are re-imported as part of the 136 ADDs (their bodies are identical to source).
- **Decision artifact:** new `docs/reference/B-SUNO-008b-decision-table.md` — 136-row per-entry table (ADD or SKIP, with rationale). Lead-ratifiable artifact. Mirrors v1.4 (`B-SUNO-005-decision-table.md`), v1.5 (`B-SUNO-006-decision-table.md`), v1.6 (`B-SUNO-007-decision-table.md`) format exactly.
- **Source-of-truth refresh:** `docs/reference/awesome-suno-prompts-source-2026-05-27-v1.9.md` — new immutable evidence file capturing the full 136-entry verbatim corpus + per-entry source-anchor URLs + SubGenre / Energy / SunoVersion / UseCase / NotableFeature field extractions. v1.8's `awesome-suno-prompts-source-2026-05-27.md` is preserved as historical evidence for the seed cycle.
- **Test updates** (no new test file; in-place edits to `PromptServiceTests.cs`):
  - **P1** count: `16` → `136`.
  - **P3** assertion: "each genre has exactly 2 entries" → per-genre **minimum** count per the source-file distribution (Pop ≥21, Rock ≥18, EDM ≥17, Hip-Hop ≥16, Indie ≥18, Jazz-Blues ≥18, R&B-Soul ≥15, Country ≥13). Source-distribution-driven assertion; SKIP rows in the decision table can reduce these by 0-N.
  - **P4-P7** carry unchanged.
  - **P5 [Theory]** carries unchanged (still 8 inline genre rows; per-genre energy split trivially satisfied at 13-21 entries/genre).
  - **P8 [Fact]** (NEW): Spot-check that 5 known high-utility entries are present (one per cardinal genre: `"Modern Pop Anthem (Female Vocals)"`, `"Epic Arena Anthem"`, `"Big Room House Anthem"`, `"Modern Trap Anthem"`, `"Classic Big Band Swing"`).
- **No schema changes.** Same 9 base + 2 forward-compat fields as v1.8. Tags and Difficulty remain `null`.
- **No UI changes.** PromptBrowserPane + genre filter + ListView + detail panel + Copy + attribution footer all unchanged.
- **No source-code changes.** PromptDefinition record + PromptService + App.xaml.cs + MainWindow.xaml + MainViewModel.cs + .csproj all unchanged.

### What this slice explicitly does NOT cover

- **No schema extensions** — `Tags` / `Difficulty` / hypothetical `BpmValue` / `KeyValue` / `EnergyDescriptor` fields all deferred. Forward-compat slots remain unused.
- **No UI affordance additions** — no free-text search inside prompt bodies or titles; no SubGenre secondary filter; no insert-as-section-set; no append-to-active-section. Genre filter ComboBox stays as the only narrowing mechanism.
- **No body normalization** — bodies are stored verbatim from the source's triple-backtick blocks. No trimming, no BPM/Key extraction into fields, no line-ending normalization beyond what JSON serialization implies.
- **No `tags.json` changes** — 30,421 bytes / 331 entries unchanged.
- **No carry-over backlog reconciliation** — B-026 / B-027 / B-028 / B-SUNO-NNN / `decisions/suno-visual-language.md:6` mojibake / section-delete `×` glyph still pending Lead-discretion (11+ cycles unaddressed).
- **No newly-added user backlog items handling** — B-SUNO-010 / B-SUNO-011 / B-SUNO-012 / B-SUNO-013 all queued separately; B-SUNO-012 (High priority) Lead-discretion to schedule after v1.9 closeout.

## 3. Approach (Mechanism)

### 3.1 Decision-table-driven curation

Per v1.4-v1.6 precedent, a per-row decision table at `docs/reference/B-SUNO-008b-decision-table.md` adjudicates every source entry as ADD or SKIP. **MERGE is not applicable** to discrete creative prompts (prompts aren't deduplicated by canonical name the way tags are; each is a discrete creative entry).

Decision-table row format:

```
| # | Genre | SubGenre | Title | Decision | Energy (parsed) | Rationale |
```

Expected distribution (planner forecast pre-T1):
- **136 source entries** across 8 genre files.
- **Expected ADDs: 134-136** — substantially all entries; the source corpus is well-curated and free of obvious commercial-link bodies (footer commercial CTAs are separate sections, not prompts).
- **Expected SKIPs: 0-2** — only if an entry's Body is structurally malformed or duplicates another entry in the same source file (unlikely; planner audit at T1).
- **Energy parsing:** non-numeric descriptors (`"Variable"`, `"11/10 (off the scale)"`) → Energy = `null`. P5 [Theory] already permits null on the ballad/chill anchor; per-genre split is trivially satisfied at 13-21 entries/genre. Planner forecast: ~2 null-Energy entries (`pop.md::Art Pop Avant-Garde` Energy=`"Variable"`, `pop.md::Hyperpop Chaotic Energy` Energy=`"11/10 (off the scale)"`).

### 3.2 Body content discipline (unchanged from v1.8)

Each entry's `Body` field stores the verbatim contents of the source's triple-backtick block. Surrounding markdown is stripped:

- H3 heading (`### <Title>`) → `Title` field.
- Triple-backtick block contents (between ` ``` ` markers) → `Body` field, line-for-line.
- `**Use Case:** ...` → `UseCase` field.
- `**Suno Version:** ...` → `SunoVersion` field.
- `**Energy:** ...` → `Energy` field (parsed integer or null).
- `**Notable Feature:** ...` → `NotableFeature` field.

The "Custom Prompts" / `songaifarm.com` commercial cross-promotion sections at the end of each genre file are not standalone prompts and are not imported. The "Production Tips" sections are reference material, not prompts, and are not imported.

### 3.3 SubGenre extraction (unchanged from v1.8)

The H2 section heading directly above each H3 entry (`## <SubGenre>`) becomes the `SubGenre` field. v1.9 carries the source's verbatim SubGenre strings (e.g., `"Upbeat Dance Pop"`, `"Stadium Rock Anthems"`, `"Grunge/Alternative"`).

### 3.4 SourceUrl anchor extraction (unchanged from v1.8)

Per-prompt `SourceUrl` is the GitHub auto-generated H3 slug:

```
https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/<file>.md#<title-slug>
```

GitHub H3 slug rules: lowercase, strip special characters (parentheses, `&`, `+`, `/`, etc.), replace spaces with hyphens.

### 3.5 Attribution policy (unchanged from v1.8)

CC0-1.0 doesn't require attribution. Two voluntary surfaces preserved from v1.8:
- Per-prompt `SourceUrl` field (data-only; not surfaced as a clickable link in v1.9 UI — same as v1.8).
- App-level footer hyperlink in `PromptBrowserPane` ("Prompts from awesome-suno-prompts (CC0)").

## 4. Risks

| # | Risk | Severity | Mitigation |
|---|---|---|---|
| **R1** | Decision-table drafting time at T1 (136 rows) | Low | Planner has all 8 source files staged in `.tmp/awesome-suno-prompts-snapshot/` from v1.8 cycle; per-row entry already inventoried by H3. Table is mechanical extraction not editorial choice. |
| **R2** | Per-entry transcription error (typo in Body) | Low | Planner verifies via grep that random sampled entries match source verbatim. P6 test asserts non-empty Body. |
| **R3** | Energy parsing edge case (non-numeric descriptors) | Informational | Documented in spec §3.1: ~2 entries → Energy=null. P5 [Theory] already permits null. |
| **R4** | UI scrollability with 136 entries vs 16 | Low | ListView already uses `ScrollViewer.VerticalScrollBarVisibility="Auto"` (landed in v1.8). USER REVIEW S2 verifies smooth scroll. |
| **R5** | Genre filter usability at 17 entries/genre average | Low | v1.8 already shipped the filter mechanism. USER REVIEW S3 verifies (Pop should now show 21 entries, not 2). |
| **R6** | P3 count assertion changes shape (exactly 2 → minimum N) | Low | Documented in §2; the change reflects source-distribution-driven semantics. Planner explicit about why the assertion shape changed. |
| **R7** | Test count drift across P1/P3 adjustments + new P8 | Informational | Forecast: 125 v1.8 → ~127 v1.9 (P1 count change is in-place; P3 assertion shape change is in-place; P5 unchanged; new P8 adds 1 test). |
| **R8** | Body strings carry GitHub-flavored Markdown that JSON serializes oddly | Low | v1.8 already shipped 16 verbatim bodies via `JsonSerializer` with `AllowTrailingCommas`; same serializer handles all 136. Visual check at T6 USER REVIEW S5 (copy-paste end-to-end). |
| **R9** | Lead override on SKIP/ADD rows at r1 | Low | Decision-table format makes overrides cheap (single-row edits). Planner default-pass forecast: ≥134 ADDs / ≤2 SKIPs. |
| **R10** | B-SUNO-012 (High priority) ordering tension | Informational | Lead-discretion per Q9 carry. Plan packet documents the alternative without scheduling it. |

## 5. Test Plan

### 5.1 Automated content-coverage tests (v1.8 P1-P7 + P8 new = 125 → ~127)

In-place edits to `tests/SunoMetatagApp.Tests/PromptServiceTests.cs`:

| Test | v1.8 | v1.9 | Change |
|---|---|---|---|
| **P1** | `LoadAll` returns 16 | `LoadAll` returns 136 (or whatever the decision-table ADD count is) | count number updated |
| **P2** | DistinctGenres returns 8 ordinal-equal | unchanged | none |
| **P3** | Each genre has **exactly** 2 entries | Each genre has **at least** N entries per source-file distribution (Pop ≥21, Rock ≥18, EDM ≥17, Hip-Hop ≥16, Indie ≥18, Jazz-Blues ≥18, R&B-Soul ≥15, Country ≥13) | assertion shape changes from `==2` to `>= source-distributed minimum`; minimums adjusted downward if SKIP rows reduce them |
| **P4** | All titles unique | unchanged | none |
| **P5** [Theory × 8] | Per-genre ≥1 high-energy ≥7 AND ≥1 ballad/chill ≤6 or null | unchanged | none — trivially satisfied at 13-21 entries/genre |
| **P6** | All Bodies non-empty | unchanged | none |
| **P7** | Forward-compat Tags/Difficulty null-tolerant | unchanged | none |
| **P8** (NEW) | Spot-check 5 known high-utility entries present (Modern Pop Anthem (Female Vocals), Epic Arena Anthem, Big Room House Anthem, Modern Trap Anthem, Classic Big Band Swing) | new [Fact] | 1 new test |

Expected total: **125 v1.8 baseline → ~127 v1.9**:
- 111 v1.7 baseline preserved
- 14 v1.8 P-series tests preserved (with P1/P3 in-place edits)
- 1 new P8

Net: 111 + 14 + 1 = **126** (or 127 if P3 split per-genre as 8 [Theory] rows — planner default: keep P3 as 1 [Fact] iterating all 8 genres).

### 5.2 USER REVIEW manual smoke matrix (S1-S8)

| # | Step | v1.8 Expected | v1.9 Expected |
|---|---|---|---|
| **S1** | Launch `publish/SunoMetatagApp.exe` | Browser hidden | unchanged |
| **S2** | Click `Prompts` toolbar | Pane slides in; All + 8 genres; **16 entries** | Pane slides in; All + 8 genres; **136 entries** (smooth scroll on full list) |
| **S3** | Genre filter → `Pop` | 2 Pop entries (Modern Pop Anthem + Piano-Driven Power Ballad) | **21 Pop entries** (full Pop section from source) |
| **S4** | Click any prompt row | Detail panel shows full content + Copy | unchanged |
| **S5** (CRITICAL) | Copy → paste in Notepad | Verbatim Body; "Copied!" auto-clears | unchanged; verbatim Body across any of the new 120 entries |
| **S6** | Click attribution footer | Browser opens source repo | unchanged |
| **S7** | Close pane; tag picker `kpop` → `[K-Pop]` | v1.7 search normalization regression check | unchanged |
| **S8** | Visual consistency | unchanged | unchanged |

PASS criterion: **8/8 PASS** forecast (mirrors v1.8 first-try-PASS). **S5 critical case** is the same end-to-end Body-verbatim verification; with 120 new bodies in scope, randomly sampling 2-3 different bodies during S5 is a useful expansion.

### 5.3 Rollback plan

Single-direction revert per v1.4-v1.6 precedent: `git revert <commit-hash(es)>` restores v1.8 closeout tip `25f8f2e` cleanly. `prompts.json` reverts to 16-entry seed; new decision-table doc and v1.9 source-capture removed. PromptDefinition / PromptService / PromptBrowserPane / MainViewModel unchanged so no regression risk.

## 6. Implementation Surfaces Touched

### 6.1 Source files (UNCHANGED — no edits)

- `src/SunoMetatagApp/Models/PromptDefinition.cs`
- `src/SunoMetatagApp/Services/PromptService.cs`
- `src/SunoMetatagApp/Views/PromptBrowserPane.xaml(.cs)`
- `src/SunoMetatagApp/ViewModels/MainViewModel.cs`
- `src/SunoMetatagApp/App.xaml.cs`
- `src/SunoMetatagApp/MainWindow.xaml`
- `src/SunoMetatagApp/SunoMetatagApp.csproj`

### 6.2 Resource files (REPLACED)

- `src/SunoMetatagApp/Resources/prompts.json` — 16 entries → 136 entries (or ADD-count after Lead-ratified SKIPs).

### 6.3 Test files (IN-PLACE EDITS)

- `tests/SunoMetatagApp.Tests/PromptServiceTests.cs`:
  - P1 count `16` → `136`.
  - P3 assertion shape `==2` → `>= source-distributed minimum`.
  - P8 [Fact] added.

### 6.4 Documentation files (NEW)

- `docs/specs/2026-05-27-suno-metatag-v1.9-prompt-library-curation.md` — this spec
- `docs/plans/2026-05-27-suno-metatag-v1.9-prompt-library-curation.md` — implementation plan
- `docs/reference/B-SUNO-008b-decision-table.md` — 136-row decision artifact
- `docs/reference/awesome-suno-prompts-source-2026-05-27-v1.9.md` — immutable source capture (SHA `e1d1247` re-confirmed; full 136-entry verbatim)

### 6.5 Wiki updates (`j:\SunoSongSetup\.SunoSongSetup-wiki\`)

- `wiki/features/sunometatag-app.md` — title bump v1.8 → v1.9; new `## v1.8 → v1.9 (2026-05-27)` subsection (mechanism unchanged; corpus expansion 16 → 136; B-SUNO-008 parent retires); `last_confirmed: 2026-05-27`; `sources` extended.
- `wiki/architecture/sunometatag-prompt-library.md` — "Seed corpus (v1.8)" section refactored to "Corpus (v1.9, full)"; reconciliation-history row added; `last_confirmed: 2026-05-27`.
- `wiki/reference/ai-plan-archive.md` — Archive entry 20 (v1.8 RESULT) already prepended at this drafting; Entry 21 (v1.9 r1 plan) will land at T8 RESULT closeout.

### 6.6 Surfaces explicitly NOT touched

- `src/SunoMetatagApp/Resources/tags.json` (30,421 B / 331 entries unchanged)
- `src/SunoMetatagApp/Services/TagService.cs` (v1.7 search normalization preserved)
- `src/SunoMetatagApp/Models/TagDefinition.cs` (schema unchanged)
- Existing v1.2-v1.8 visual theme tokens (no new tokens)

## 7. Wiki update commitment (closeout)

```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-prompt-library]], [[ai-plan-archive]]
```

All landed in-cycle; no queued-exception declarations. `wiki_sync_status: PASS` expected.

## 8. Open Decisions for Lead Ratification

| # | Decision | Planner default | §ref |
|---|---|---|---|
| **Q1** | Scope = full 136 in one cycle, replacing seed? | YES — user-confirmed pre-spec via brainstorm 2026-05-27 | §1, §2 |
| **Q2** | No schema changes (Tags/Difficulty stay null)? | YES — user-confirmed pre-spec; pure data slice discipline | §2, §3 |
| **Q3** | No UI changes (no free-text search, no SubGenre filter, no insert-as-section-set)? | YES — user-confirmed pre-spec; data-only v1.9 | §2 |
| **Q4** | Decision-table format per v1.4-v1.6 precedent? | YES — established project pattern | §3.1, §6.4 |
| **Q5** | Non-numeric Energy → null (P5 permits)? | YES — ~2 entries affected; P5 [Theory] already permits null | §3.1 |
| **Q6** | P3 assertion shape change (==2 → >= source-distributed minimum)? | YES — necessary because source genre counts vary (Pop 21, Country 13) | §5.1 |
| **Q7** | New P8 [Fact] for 5-entry presence spot-check? | YES — sanity gate for catastrophic curation regression | §5.1 |
| **Q8** | Re-import the 16 v1.8 seed entries as part of the 136 ADDs (drop-and-replace prompts.json)? | YES — cleanest "replace seed" semantic; bodies are identical to source so no data drift risk | §1, §2 |
| **Q9** | B-SUNO-012 (High priority) scheduling — before or after v1.9 retires B-SUNO-008 parent? | Defer to Lead; spec §2 documents as queued separately | §2 |
| **Q10** | New source-capture doc (v1.9 variant) vs reuse v1.8's (same SHA)? | YES — new v1.9 source-capture with full 136-entry verbatim is cleaner audit artifact even though commit SHA unchanged | §6.4 |

## 9. B-SUNO-008 Parent Milestone Retirement (v1.9 closeout commitment)

v1.9 closes the parent backlog item B-SUNO-008 (Add curated pre-made prompt library) per Lead's decision `D-2026-05-27-B-SUNO-008-scope-phasing`:

- **B-SUNO-008a (v1.8):** mechanism + seed ✓ shipped 2026-05-27.
- **B-SUNO-008b (v1.9, this slice):** full curation, replacing seed.

After v1.9 closeout, B-SUNO-008 retires from `docs/BACKLOG.md`. Future prompt-library work (free-text search, schema additions like `BpmValue`, insert-as-section-set, append-to-active-section, prompt-to-tag cross-referencing) would be new backlog items spawned post-v1.9 by user direction, not children of B-SUNO-008.

## 10. Conclusion

v1.9 is a **pure data curation slice** that completes the second half of Lead's decision-packet-ratified Option A scope-phasing for B-SUNO-008. Zero source-code changes; zero schema changes; zero UI changes. The mechanism shipped in v1.8 (B-SUNO-008a) handles 136 entries with no architectural amplification; v1.9 simply replaces the seed corpus with the full curated set.

Risk profile is bounded: a decision-table-driven curation cycle is the most-precedented shape in this project (v1.4 / v1.5 / v1.6 all used the same pattern with even larger source-row counts in some cases). The 136 source entries are well-curated upstream (CC0-1.0 awesome-suno-prompts repo), free of commercial-link Body content (commercial CTAs live in separate footer sections), and structurally consistent (every entry has Title + Body in triple-backtick + Use Case label + Suno Version label + Energy label + optional Notable Feature label).

**Expected closeout:** APPROVED (PASS or PASS-WITH-NOTES); USER REVIEW S1-S8 8/8 PASS forecast (would be **seventh** consecutive USER-REVIEW-first-try-PASS across v1.3 → v1.9); `wiki_sync_status: PASS`; B-SUNO-008 parent milestone retires.
