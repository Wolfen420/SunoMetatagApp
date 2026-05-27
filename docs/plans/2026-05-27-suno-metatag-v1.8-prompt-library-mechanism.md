# SunoMetatagApp v1.8 — Implementation Plan (B-SUNO-008a Prompt Library Mechanism + Seed)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-008a / v1.8 — Prompt library mechanism + 16-prompt seed corpus
- **Type:** Mechanism-first slice (new data model + new service + new UI surface + small seed)
- **Spec:** [`docs/specs/2026-05-27-suno-metatag-v1.8-prompt-library-mechanism.md`](../specs/2026-05-27-suno-metatag-v1.8-prompt-library-mechanism.md)
- **Decision packet authority:** Lead-ratified `D-2026-05-27-B-SUNO-008-scope-phasing` Option A
- **Precedent shape:** v1.2 visual redesign (new XAML pane + ViewModel binding additions) + v1.3 stacked syntax (mechanism-first source-code slice) — different shape from v1.4-v1.7
- **Working baseline:** `master` tip `df367ea` (v1.7 closeout)

## Task list (T0-T8)

### T0 — Baseline + planning artifacts landed

- Confirm `git status` clean on `j:\SunoMetatagApp\` `master`; `git log -1 --oneline` should show `df367ea` (v1.7 closeout tip).
- Land 3 doc artifacts as **untracked** files (committed at T1):
  - `docs/specs/2026-05-27-suno-metatag-v1.8-prompt-library-mechanism.md`
  - `docs/plans/2026-05-27-suno-metatag-v1.8-prompt-library-mechanism.md`
  - `docs/reference/awesome-suno-prompts-source-2026-05-27.md` (immutable source capture; built at T1 after planner selects 16 seed entries)
- Run `dotnet build` + `dotnet test` — should show **111 tests green** (v1.7 baseline at HEAD `df367ea`).
- **Absorption edits:** apply any pre-T1 absorption items from Lead/Specialist r1 review here.

### T1 — Data model + service + resource + source-capture (primary commit)

Order of operations within T1:

1. **Read all 8 `prompts/<genre>.md` files** via `gh api repos/naqashmunir21/awesome-suno-prompts/contents/prompts/<genre>.md --jq '.content' | base64 -d > /tmp/<genre>.md` (or equivalent). Capture commit SHA at fetch time.
2. **Apply selection criteria** (spec §3.4): 2 prompts per genre × 8 = 16. 1 high-energy + 1 ballad/chill per genre. Prefer BPM/Key annotations + UseCase fields. Avoid same SubGenre twice per genre. Avoid commercial-link bodies.
3. **Write `docs/reference/awesome-suno-prompts-source-2026-05-27.md`** with:
   - Repo metadata (license CC0-1.0, default branch `main`, commit SHA at fetch time)
   - File listing of `prompts/` (8 entries with sizes)
   - For each of the 16 selected prompts: source-anchor URL + raw body verbatim
4. **Write `src/SunoMetatagApp/Models/PromptDefinition.cs`** per spec §3.1 (record with 9 fields + 2 forward-compat).
5. **Write `src/SunoMetatagApp/Services/PromptService.cs`** per spec §3.2 (`LoadAll` / `DistinctGenres` / `Filter(by genre)`).
6. **Write `src/SunoMetatagApp/Resources/prompts.json`** with the 16 selected entries.
7. **Update `src/SunoMetatagApp/SunoMetatagApp.csproj`** to add `prompts.json` `CopyToOutput="Always"` rule.
8. **Update `src/SunoMetatagApp/App.xaml.cs`** to wire `PromptService.LoadAll(promptsPath)` at startup (parallel to `TagService.LoadAll`).
9. **Verify `dotnet build` green.**
10. **Verify `dotnet test --no-build` shows 111/111 still green** (baseline maintained — no UI/test changes yet; ViewModel + UI come at T2).

**Commit boundary:** primary commit with `PromptDefinition.cs` + `PromptService.cs` + `prompts.json` + `.csproj` edit + `App.xaml.cs` edit + 3 doc artifacts.
  - Suggested message: `B-SUNO-008a / v1.8: PromptDefinition model + PromptService + 16-entry seed prompts.json`

### T2 — UI surface (secondary commit)

Order of operations within T2:

1. **Write `src/SunoMetatagApp/Views/PromptBrowserPane.xaml`** per spec §3.3:
   - Header bar (title + close button)
   - Genre filter `ComboBox`
   - Prompt list `ListView` with row template (Title + SubGenre + Energy badge)
   - Expand/select panel with Body + UseCase + SunoVersion + NotableFeature + Copy button
   - Attribution footer with clickable hyperlink
2. **Write `src/SunoMetatagApp/Views/PromptBrowserPane.xaml.cs`** (minimal code-behind; mostly XAML-bound).
3. **Update `src/SunoMetatagApp/MainWindow.xaml`**:
   - Add new `Grid.ColumnDefinition` for the prompt browser pane (default-collapsed)
   - Add toolbar button (icon: bookmark/scroll glyph) bound to toggle command
4. **Update `src/SunoMetatagApp/ViewModels/MainViewModel.cs`**:
   - Add `IsPromptBrowserVisible : bool` property
   - Add `Prompts : ObservableCollection<PromptDefinition>` (filtered)
   - Add `SelectedPrompt : PromptDefinition?` for inline expand
   - Add `SelectedPromptGenre : string?` for genre filter binding
   - Add `[RelayCommand] TogglePromptBrowser()` method
   - Add `[RelayCommand] CopyPromptBody(PromptDefinition prompt)` method that calls `Clipboard.SetText(prompt.Body)` (wrap in try-catch per R3)
   - On constructor: load prompts via `PromptService.LoadAll` + populate initial `Prompts` collection
5. **Verify `dotnet build` green** (XAML markup compile may catch surface issues here).
6. **Verify `dotnet test --no-build` 111/111 still green** (no test changes; ViewModel additions don't affect existing tests).

**Commit boundary:** secondary commit with `PromptBrowserPane.xaml` + `.cs` + `MainWindow.xaml` toggle + `MainViewModel.cs` additions.
  - Suggested message: `B-SUNO-008a / v1.8: PromptBrowserPane.xaml + MainWindow toggle wiring + MainViewModel bindings`

### T3 — Content-coverage tests (tertiary commit)

Order of operations within T3:

1. **Create `tests/SunoMetatagApp.Tests/PromptServiceTests.cs`** with **P1-P7** per spec §6.1.
2. P5 implemented as `[Theory]` with 8 inline data rows (one per genre) per spec.
3. Use `Path.Combine(AppContext.BaseDirectory, "prompts.json")` helper pattern (mirror `LoadProductionTagsJson` from `TagServiceCheatSheetTests`).
4. Run `dotnet test` — expect **>= 120 tests green** (111 baseline + 4 [Fact] P1/P3/P4/P6 + 8 [Theory] P5 inline + 1 [Fact] P7 + 1 [Fact] P2 = 14 = 125 expected; minor count drift depending on exact assertion granularity).

**Commit boundary:** tertiary commit with new test file only.
  - Suggested message: `B-SUNO-008a / v1.8: 7 content-coverage tests for PromptService (P1-P7)`

### T4 — Dev smoke launch

- `timeout 6 dotnet run --no-build --project src/SunoMetatagApp` — expect `EXIT=124` (timeout-killed WPF GUI) with no exception output before timeout.
- Default state: prompt browser hidden (S1 forecast); existing v1.7 layout visually unchanged.

### T5 — Publish artifact rebuild

- `dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish`
- Expected `publish/SunoMetatagApp.exe` size: ~146 MB ± small footprint increase (new XAML view + new resource + new service add a few KB).
- Expected `publish/prompts.json` size: ~5-15 KB (16 entries × ~500-1000 bytes per entry).
- Expected `publish/tags.json` size: identical to v1.7 (30,421 bytes; zero changes).
- Smoke-launch `publish/SunoMetatagApp.exe`; verify no startup exception.

### T6 — USER REVIEW manual smoke matrix

- Surface `USER REVIEW NEEDED` header to user with **S1-S8** per spec §5.2.
- Required response format: 8-row PASS/FAIL table OR free-text confirmation.
- Target: 8/8 PASS. **S5 is the critical case** — copy-to-clipboard works end-to-end into Notepad/VSCode paste. **S2 is the discoverability case** — toggle button surfaces the new pane.

### T7 — Wiki updates landed in-cycle

- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\features\sunometatag-app.md`:
  - Title bump: v1.7 → v1.8
  - Add subsection: `## v1.7 → v1.8 (2026-05-27)` describing the prompt library mechanism + 16-prompt seed + UI surface + decision packet ratification.
  - Refresh `last_confirmed: 2026-05-27`.
  - Extend `sources` frontmatter.
  - Add `[[sunometatag-prompt-library]]` to `related`.
- Create NEW `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\architecture\sunometatag-prompt-library.md` (architecture page parallel to `[[sunometatag-tag-library]]`):
  - Frontmatter: `type: architecture`, `claim_state: active`, `last_confirmed: 2026-05-27`, etc.
  - Section: "Library file location" (Resources/prompts.json)
  - Section: "Schema" (PromptDefinition record)
  - Section: "PromptService API"
  - Section: "PromptBrowserPane UI surface"
  - Section: "Attribution policy"
  - Section: "Seed corpus (v1.8)"
  - Section: "Source paths"
  - Section: "Related: [[sunometatag-tag-library]], [[sunometatag-app]]"
- Edit `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md`:
  - Prepend Archive entry 18 (v1.7 RESULT body — will happen during T8).
  - Verify Archive entry 17 (B-SUNO-009 r1) is already prepended at v1.7 closeout.

### T8 — Workflow packet maintenance

- **T8a:** Archive entry 18 (v1.7 RESULT) prepended at T8 ai/PLAN.md → RESULT rewrite.
- **T8b:** Append v1.8 execution entry to `j:\SunoSongSetup\ai\EXECUTION_LOG.md`.
- **T8c:** Rewrite `j:\SunoSongSetup\ai\PLAN.md` from r1 plan packet → RESULT packet (post-execution).

## Working-tree state at each commit boundary

| Commit | Files | Validation |
|---|---|---|
| (working tree pre-T1) | 3 doc artifacts untracked | `git status` shows 3 untracked .md files; build green; 111/111 tests green (v1.7 baseline) |
| **Primary (T1)** | `Models/PromptDefinition.cs` + `Services/PromptService.cs` + `Resources/prompts.json` + `.csproj` edit + `App.xaml.cs` edit + 3 doc artifacts | `dotnet build` green; `dotnet test` 111/111 green (no regression; no new tests yet) |
| **Secondary (T2)** | `Views/PromptBrowserPane.xaml(.cs)` + `MainWindow.xaml` + `MainViewModel.cs` | `dotnet build` green; `dotnet test` 111/111 green (no regression) |
| **Tertiary (T3)** | New `PromptServiceTests.cs` | `dotnet test` ≥ 124/124 green (111 + ~14 new) |

## Open r1 risks for Lead/Specialist review

These overlap with spec §4 risks:

1. **UI real-estate** (R2): Specialist may want a third UI mockup variant (e.g., docked-bottom or floating window). Decision-packet adjudication already happened; Lead may re-open at r1 if user surface friction surfaces.
2. **Attribution surface** (R5): Lead may prefer single-surface (footer only OR per-prompt only) — currently both. Lead-discretion to override.
3. **Forward-compat fields** (R4): Specialist may want `Tags` field exercised in v1.8 seed (e.g., 3-5 tags per prompt extracted from body descriptors). Planner default: null in seed; populated in v1.9 curation.
4. **Seed criteria at T1** (R1): If a chosen prompt body has fields not in `PromptDefinition`, planner substitutes per §3.4. Lead may want explicit pre-T1 lock on selection.

## Specialist activation forecast

- **ENGINE:** out of scope. `PromptService.LoadAll` is deterministic JSON read at startup (mirrors `TagService`); no concurrency, persistence, generation-semantic implications.
- **FRONTEND/UX:** activation expected. Specialist scrutiny anticipated on:
  - UI surface real-estate impact (R2).
  - Inline expand vs separate-panel reveal pattern in `PromptBrowserPane`.
  - Genre filter discoverability (one dropdown + no free-text search; deferred to v1.9).
  - Copy-to-clipboard UX (status feedback after copy; toast or button-state change?).
  - Attribution surface design (CC0 = optional but voluntarily added; whether one surface or both is right).
  - Default state of pane = hidden (preserves v1.7 layout for upgraders, but discoverability of new feature).
  - Visual consistency with existing v1.2-v1.7 theme.

## Result-cycle wiki commitment

Closeout will declare:
```
Wiki updates landed: [[sunometatag-app]], [[sunometatag-prompt-library]], [[ai-plan-archive]]
```

`wiki_sync_status: PASS` expected.
