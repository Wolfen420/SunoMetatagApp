# SunoMetatagApp v1.6 — Implementation Plan (B-SUNO-007 sunoaiwiki Metatag List Reconciliation)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-007 / v1.6 — Reconcile sunoaiwiki metatag list against post-v1.5 286-entry library
- **Type:** Content-only data curation (zero source-code changes)
- **Spec:** [`docs/specs/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md`](../specs/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md)
- **Source:** [`docs/reference/suno-metatag-list-source-2026-05-27.md`](../reference/suno-metatag-list-source-2026-05-27.md)
- **Decision table:** [`docs/reference/B-SUNO-007-decision-table.md`](../reference/B-SUNO-007-decision-table.md)
- **Precedent:** v1.5 (B-SUNO-006, commits `939e611` / `3ecdd5e` / `c9fe7dc`); v1.4 (B-SUNO-005, commits `301c672` / `fadb7b9` / `42c5a28`)
- **Working baseline:** `master` tip `c9fe7dc` (v1.5 closeout)

## Task list (T0-T8)

### T0 — Baseline + research artifacts landed
- Confirm `git status` clean on `j:\SunoMetatagApp\` `master`; `git log -1 --oneline` should show `c9fe7dc` (v1.5 closeout tip).
- Land 4 doc/reference artifacts as **untracked** files (committed at T1):
  - `docs/specs/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md`
  - `docs/plans/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md`
  - `docs/reference/suno-metatag-list-source-2026-05-27.md`
  - `docs/reference/B-SUNO-007-decision-table.md`
- Run `dotnet build` + `dotnet test` — should show **70 tests green** (v1.5 baseline at HEAD `c9fe7dc`).
- **Absorption edits:** apply any pre-T1 absorption items from Lead/Specialist r1 review here.

### T1 — Apply decision table to `tags.json`
- Hand-edit `src/SunoMetatagApp/Resources/tags.json` to append 45 ADD rows across 5 existing categories (Vocal +5, Instrument +4, Production +2, SFX +14, Genre +20). Planner draft was 46 ADDs; T1 self-correction removed Clapping ADD after bracket-collision detection with existing v1.4 `[Clapping]`.
- Insertion strategy: append each ADD at the **end of its target category block** to preserve v1.4 + v1.5 ordering convention. For Genre, append after the existing v1.5 block. For SFX, append after the existing v1.4 block. Etc.
- All ADD rows use canonical Title-Case `[Title Case]` bracket form per v1.5 §3.2 rules (inherited unchanged).
- Preserve all 286 existing entries verbatim.
- Validate JSON parses cleanly (`dotnet test` would surface any malformed JSON via `TagService` startup).
- **Commit boundary:** primary commit with `tags.json` + 4 doc artifacts.
  - Suggested message: `B-SUNO-007 / v1.6: reconcile sunoaiwiki metatag list (+46 entries across 5 categories)`

### T2 — Add content-coverage tests
- Create `tests/SunoMetatagApp.Tests/TagServiceSunoaiwikiMetatagListTests.cs` with **H1-H6** per spec §7.1.
- Use the same `LoadProductionTagsJson()` helper pattern from v1.4/v1.5 precedent.
- H3 implemented as `[Theory]` with 12 inline data rows (per v1.5 G3 [Theory] precedent; better isolation, xUnit-idiomatic).
- **T2 grep-recount discipline:** before committing, run:
  ```
  grep -c '"bracket":' src/SunoMetatagApp/Resources/tags.json    # expect 331 (planner draft was 332; Clapping SKIP correction landed at T1)
  grep -c '"category": "Vocal"' src/SunoMetatagApp/Resources/tags.json     # expect 45
  grep -c '"category": "Instrument"' src/SunoMetatagApp/Resources/tags.json # expect 36
  grep -c '"category": "Production"' src/SunoMetatagApp/Resources/tags.json # expect 6
  grep -c '"category": "SFX"' src/SunoMetatagApp/Resources/tags.json        # expect 63 (planner draft was 64; Clapping SKIP)
  grep -c '"category": "Genre"' src/SunoMetatagApp/Resources/tags.json      # expect 107
  ```
- If off-by-N, file hygiene commit (T2.5) per v1.4 `42c5a28` + v1.5 `c9fe7dc` precedent.
- Run `dotnet test` — expect **>= 81 tests green** (70 baseline + 5 [Fact] + 12 [Theory] H3 inline rows = 87 expected).
- **Commit boundary:** secondary commit with new test file only.
  - Suggested message: `B-SUNO-007 / v1.6: 6 content-coverage tests for sunoaiwiki metatag reconciliation (H1-H6)`

### T3 — Test pass verification
- `dotnet test --no-build` — confirm 87/87 green at secondary-commit tip (or applicable higher number after [Theory] expansion).
- If T2.5 hygiene commit lands, re-verify post-hygiene.

### T4 — Dev smoke launch
- `timeout 6 dotnet run --no-build --project src/SunoMetatagApp` — expect `EXIT=124` (timeout-killed WPF GUI) with no exception output before timeout.
- 5 extended categories visible in category-filter combobox.

### T5 — Publish artifact rebuild
- `dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish`
- Expected `publish/SunoMetatagApp.exe` size: ~146 MB (identical to v1.3/v1.4/v1.5 footprint).
- Expected `publish/tags.json` size: ~30 KB (~26.6 KB for 286 entries + ~3-4 KB for 46 ADDs).
- Smoke-launch `publish/SunoMetatagApp.exe`; verify no startup exception.

### T6 — USER REVIEW manual smoke matrix
- Surface `USER REVIEW NEEDED` header to user with **S1-S8** per spec §7.2.
- Required response format: 8-row PASS/FAIL table OR free-text confirmation.
- Target: 8/8 PASS or PASS-WITH-CONCERN (S6 carries forward v1.5 hyphen-insensitive limitation as expected known artifact).

### T7 — Wiki updates landed in-cycle
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\features\sunometatag-app.md`:
  - Title bump: v1.5 → v1.6.
  - Add subsection: `## v1.5 → v1.6 (2026-05-27)` describing the metatag-list reconciliation (46 ADDs, 8 MERGEs, 29 SKIPs, no new category).
  - Refresh `last_confirmed: 2026-05-27`.
  - Extend `sources` frontmatter.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\architecture\sunometatag-tag-library.md`:
  - Categories table counts updated (Vocal 40→45, Instrument 32→36, Production 4→6, SFX 49→64, Genre 87→107).
  - v1.6 row appended to Reconciliation history table.
  - v1.6 H1-H6 validation gates sub-section added.
  - Source paths extended with 4 new v1.6 files.
  - Refresh `last_confirmed: 2026-05-27`.
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md`:
  - Prepend Archive entry 15 (B-SUNO-007 r1 plan packet content — will happen during T8a step).
  - Verify Archive entry 14 (v1.5 RESULT packet) is already prepended at r1 draft time.

### T8 — Workflow packet maintenance
- **T8a:** Archive entry 14 (v1.5 RESULT) was prepended during this r1 plan packet draft. Archive entry 15 (this r1 plan packet body) prepended during T8 when ai/PLAN.md → RESULT rewrite occurs.
- **T8b:** Append v1.6 execution entry to `j:\SunoSongSetup\ai\EXECUTION_LOG.md`.
- **T8c:** Rewrite `j:\SunoSongSetup\ai\PLAN.md` from r1 plan packet → RESULT packet (post-execution).

## Working-tree state at each commit boundary

| Commit | Files | Validation |
|---|---|---|
| (working tree pre-T1) | spec + plan + decision-table + source untracked | `git status` shows 4 untracked .md files; build green |
| **Primary (T1)** | `tags.json` + 4 doc artifacts | `dotnet build` green; `dotnet test` 70/70 green (baseline maintained) |
| **Secondary (T2)** | New `TagServiceSunoaiwikiMetatagListTests.cs` | `dotnet test` 87/87 green |
| **Hygiene (T2.5, optional)** | spec + plan + decision-table totals if grep-recount finds off-by-N | tests still green; thresholds satisfied |

## Open r1 risks for Lead/Specialist review

These overlap with spec §11 open decisions. Listed here so Lead can ratify or override in r1 verdict:

1. **Cross-category collision policy** (§3.3): SKIP-canonical / MERGE-cross-cat / ADD-new-cat — Lead may simplify or refine.
2. **Split-row decisions** (§3.4): "Christian & Gospel" + "Dance & Electronic" split into multiple decision rows.
3. **"African" ADD breaks v1.5 §3.7 regional super-label SKIP precedent** — different exact-source-label (no "music" suffix).
4. **"Christian" / "Boy" / "Girl" / "Man" / "Woman" voice-type ADDs** — verbatim source per import-as-source discipline.
5. **`[Drums]` / `[Piano]` / `[Synth]` to Instrument** despite source's Style/Genre labeling.
6. **`[Censored]` / `[Silence]` to Production** placement choice.
7. **`[Lo-Fi]` Genre coexisting with `[Effect: Lo-fi]` Effect** — cross-category ADD example.
8. **Decision-table grand totals** are planner draft. v1.4 precedent: planner draft off-by-one caught at T2; v1.5 precedent: test-count drift caught at T2.5. Same mitigation strategy.
9. **B-SUNO-009 alternative** — Lead may override target_item at r1 to switch focus to source-code search-normalization slice instead of v1.6 content slice.

## Specialist activation forecast

- **ENGINE:** out of scope. No code changes, no geometry, no determinism.
- **FRONTEND/UX:** activation expected (mirrors v1.4 + v1.5 activation level). Specialist scrutiny anticipated on:
  - Cross-category collision rationale (Lo-Fi Genre vs Effect: Lo-fi; Drums Instrument vs Drum-Solo Structure).
  - Chip-row density when filtered to Genre (~107 chips post-v1.6, up from 87 in v1.5). Approaches but doesn't exceed B-011 300-tag virtualization trigger.
  - Search-affordance carry-over (S6 PASS-WITH-CONCERN from v1.5 explicitly expected to recur; B-SUNO-009 the explicit fix slice).
  - "Boy" / "Girl" voice-type adds — appropriateness for adult-music tool.
  - Discoverability of 46 new entries across 5 categories.

## Result-cycle wiki commitment

Closeout will declare:
```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-tag-library]], [[ai-plan-archive]]
```

`wiki_sync_status: PASS` is the expected closeout state.
