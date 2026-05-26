# Implementation Plan — SunoMetatagApp v1.3 (Stacked Metatag Syntax)

**Spec:** `docs/specs/2026-05-26-suno-metatag-v1.3-stacked-syntax.md`
**Backlog item:** B-SUNO-004
**Date:** 2026-05-26
**Baseline commit:** `574ebd6` (v1.2 tip on `master` — note: prior plan packet said "main", actual branch is `master`)
**Estimated scope:** 4–6 commits; ~80 LOC source + ~170 LOC tests; zero visual-layer change; zero new wiki pages (1 wiki page extended).
**Specialist r1 LOWs absorbed in spec/plan before T1** (per Lead directive 2026-05-26):
- LOW 1 (spec §6.1 dead-code `openBetween` removed; gap-length short-circuit added)
- LOW 2 (spec §6.1 → `[RelayCommand] private void` per v1.1 toolkit pattern; `InsertTagStackedCommand` auto-generated)
- LOW 3 (spec §2.4 mixed-mode fallback explicit; new test T15)
- LOW 4 (spec §2.3.1 modifier-release timing note)
- LOW 5 (spec §5 empty-bracket non-goal; new test T16)
- LOW 6 (spec §6.3.1 `AutomationProperties.HelpText` on picker-scoped Style)
- LOW 7 (spec §2.3.1 Sticky Keys compatibility note)
- LOW 8 (spec §6.4 + §7.3 S13 → **pinned at T7 dev-smoke verification gate** below)

## Pre-execution baseline

- `j:\SunoMetatagApp\` `main` at commit `574ebd6` (v1.2 closeout fix-combobox).
- `dotnet build` green; `dotnet test` 31/31 passing.
- `publish/SunoMetatagApp.exe` reflects v1.2 visual theme; v1.3 will re-publish at T8.
- v1.1 focus model + v1.2 visual layer must be preserved verbatim.

## Task list

### T0 — Verify clean baseline

- `git status` on `j:\SunoMetatagApp\` shows clean working tree.
- `git log -1 --oneline` confirms tip is `574ebd6 fix: full ComboBox template override` (or later if subsequent slices have shipped — confirm with `git log`).
- `dotnet build` green.
- `dotnet test` 31/31 passing.

**No commit at T0.** Baseline verification only.

### T1 — Add `InsertTagStacked` public method to `MainViewModel`

File: `src/SunoMetatagApp/ViewModels/MainViewModel.cs`

Add the `InsertTagStacked(TagViewModel?)` public method per spec §6.1 immediately after the existing `InsertTag` RelayCommand method. Implementation lifted verbatim from spec §6.1.

Optionally also add `[RelayCommand]` attribute wrapper to expose `InsertTagStackedCommand` (per spec §11 decision 3 — planner-default: expose both for future hotkey binding). If `[RelayCommand]` is used on the method, rename the method to `InsertTagStacked` (it already is) and the toolkit generates `InsertTagStackedCommand`.

**Commit message:** `feat: add InsertTagStacked for Shift+click stack-into-bracket (B-SUNO-004)`

**Expected delta:** ~50 LOC added, 0 deleted.

### T2 — Add unit tests for `InsertTagStacked`

File: `tests/SunoMetatagApp.Tests/MainViewModelInsertTagStackedTests.cs` (new)

Cover the 14 cases from spec §7.1. Test pattern matches existing `MainViewModelTests.cs`:

```csharp
public class MainViewModelInsertTagStackedTests
{
    private static MainViewModel CreateVm()
    {
        var tags = new[]
        {
            new TagDefinition("Verse", "[Verse]", "Structure", null),
            new TagDefinition("Chorus", "[Chorus]", "Structure", null),
            new TagDefinition("Bridge", "[Bridge]", "Structure", null),
            new TagDefinition("Drums", "[Drums]", "Instrument", null),
        };
        return new MainViewModel(tags);
    }

    private static TagViewModel Tag(string bracket) =>
        new(new TagDefinition(bracket.Trim('[', ']'), bracket, "Test", null));

    [Fact]
    public void InsertTagStacked_AfterBracket_MergesIntoExisting()
    {
        var vm = CreateVm();
        var section = vm.Sections[0];
        section.Lyrics = "[Verse]";
        vm.FocusedSection = section;
        vm.FocusedCaretPosition = 7;

        vm.InsertTagStacked(Tag("[Chorus]"));

        Assert.Equal("[Verse | Chorus]", section.Lyrics);
        Assert.Equal(17, vm.FocusedCaretPosition);  // after the new ]
    }

    // ... 13 more tests per spec §7.1 table
}
```

**Commit message:** `test: 14 unit tests covering InsertTagStacked merge + fallback paths (B-SUNO-004)`

**Expected delta:** ~150 LOC added, 0 deleted. Test count 31 → 45.

### T3 — Run full test suite

- `dotnet build` from `j:\SunoMetatagApp\`.
- `dotnet test` from `j:\SunoMetatagApp\`.
- Expected: **47/47 passing** (31 existing + 14 new).

**Commit:** none at T3 (no source changes); validation only.

**If tests fail:** debug the merge algorithm in T1 and re-commit. Adjust spec §6.1 if algorithm needs correction (specialist re-review may be appropriate before proceeding to T4).

### T4 — Add `TagPickerButton_Click` handler in code-behind

File: `src/SunoMetatagApp/MainWindow.xaml.cs`

Add the handler per spec §6.2 in the code-behind, placed near the other event handlers (e.g., after `OnCaretRestoreRequested`):

```csharp
private void TagPickerButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is not Button btn) return;
    if (btn.DataContext is not TagViewModel tag) return;
    if (DataContext is not MainViewModel vm) return;

    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        vm.InsertTagStacked(tag);
    else
        vm.InsertTagCommand.Execute(tag);
}
```

**Commit message:** `feat: route Shift+click on tag picker to InsertTagStacked (B-SUNO-004)`

**Expected delta:** ~12 LOC added, 0 deleted.

### T5 — Wire XAML button to `TagPickerButton_Click`

File: `src/SunoMetatagApp/MainWindow.xaml` (lines 229–233)

Replace the `Command`/`CommandParameter` binding with `Click="TagPickerButton_Click"` per spec §6.3:

```xml
<DataTemplate>
    <Button Content="{Binding Bracket}"
            ToolTip="{Binding Description}"
            ToolTipService.IsEnabled="{Binding Description, Converter={StaticResource StringIsNotEmptyConverter}}"
            Click="TagPickerButton_Click" />
</DataTemplate>
```

Required sub-steps:
- Add a Style-level Setter `<Setter Property="ToolTip" Value="Click to insert; Shift+click to stack into nearest bracket." />` to the picker-scoped Style (around line 213) per spec §6.4. The DataTrigger override for FocusedSection==null still wins when no focus is held; the per-button local `ToolTip` binding to `Description` still wins for tags with descriptions.
- **(Absorbed from specialist LOW 6)** Add `<Setter Property="AutomationProperties.HelpText" Value="Click to insert. Shift+click to stack into the nearest bracket on the current line." />` to the same picker-scoped Style. UI Automation announces this regardless of `ToolTipService.IsEnabled` precedence — surfaces the Shift+click affordance to screen readers + assistive tech.

**Commit message:** `feat: wire tag picker button Click to TagPickerButton_Click handler (B-SUNO-004)`

**Expected delta:** ~6 LOC delta (2 removed Command lines, ~4 added — Click attr + optional Setter).

### T6 — Build + regression test

- `dotnet build` green.
- `dotnet test` 47/47 passing (31 + 16).
- No XAML parse errors at compile time.

**Commit:** none at T6 (validation only).

### T7 — Dev smoke-launch (caught Margin defect lesson from v1.2)

- `dotnet run --project src/SunoMetatagApp --no-build` from `j:\SunoMetatagApp\`.
- App should launch without `XamlParseException`. Verify in the dev console:
  - No exceptions on startup.
  - Picker buttons clickable.
  - Plain click inserts `[Tag]` at caret as in v1.2.
  - Shift+click on a focused lyric textbox with `[Verse]` content merges into `[Verse | Chorus]`.
  - Shift+click on empty/bracket-free line falls back to plain insert.

**Specialist LOW 8 verification gate (PIN — must verify before T8 publish):**
- Hover any **description-less** tag button (e.g. `[Verse]`, which has no `Description` in `tags.json`) without focus on any lyric textbox AND with focus on a lyric textbox.
- **Expected:** Style-level tooltip "Click to insert; Shift+click to stack into nearest bracket." appears on hover when no focus, AND the same tooltip appears (or the dim-mode override) per the DataTrigger when focus is present.
- **Hypothesis test:** if `ToolTipService.IsEnabled="{Binding Description, Converter=...}"` evaluates to `false` for the description-less tag, and this suppresses the Style-level Setter's tooltip too, the user will see NO tooltip at all on description-less tags.
- **If hint does NOT appear:** apply spec §6.4 Option A (remove `ToolTipService.IsEnabled` from per-button XAML), Option B (restructure binding), or Option C (rely on AutomationProperties.HelpText for accessibility + accept reduced visual discoverability). Choose minimal fix; commit as `fix: ...`; re-smoke.
- **If hint DOES appear:** no action needed — Style Setter delivers as designed.
- Document outcome in the RESULT packet §2 deviations table.

**If smoke-launch fails:** debug + fix in code (e.g., XAML parse error from missing `xmlns` or typo) and re-commit. Do not proceed to T8 until smoke-launch is clean AND LOW 8 is verified.

**Commit:** none at T7 unless a fix is needed.

### T8 — Publish single-file self-contained exe

```
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true -o publish
```

Verify:
- `publish/SunoMetatagApp.exe` exists (~147 MB).
- `publish/tags.json` exists (CopyToOutput).
- Launch `publish/SunoMetatagApp.exe` and confirm it opens without error.

**Commit:** none at T8 (publish output is in `.gitignore` per project convention; the source has already been committed at T1–T5).

### T9 — USER REVIEW (12-case smoke matrix)

Hand off the published exe to the user with the smoke matrix from spec §7.3 (12 cases):

- 6 v1.1 carry-over (plain-click semantics: focus, insert at caret, copy, modal-delete, error-banner)
- 6 v1.3 new (Shift+click stack, multi-stack, fallback, line-scope, dim affordance held, visual layer intact)

**Surface header:** `USER REVIEW NEEDED` with numbered steps + per-case PASS/FAIL response format.

**Branch logic:**
- All 12 PASS → proceed to T10.
- Any FAIL → diagnose, fix, re-publish, re-smoke. Iterate until PASS. Per the v1.2 user-iteration discipline, document each round in the RESULT packet `§2 Deviations` table.

### T10 — Wiki updates

After USER REVIEW PASS:

1. **UPDATE** `.SunoSongSetup-wiki/wiki/architecture/sunometatag-inline-editor.md`:
   - Add new section "Stacked-syntax insertion (Shift+click)" after "Focus tracking + caret restoration".
   - Document the merge algorithm (caret-inside-bracket case + walk-left-for-bracket case + fallback).
   - Document the code-behind click router as the modifier-detection point.
   - Refresh `last_confirmed: 2026-05-26`, `updated: 2026-05-26`.

2. **UPDATE** `.SunoSongSetup-wiki/wiki/features/sunometatag-app.md`:
   - Bump title to "(v1.3 — Stacked Metatag Syntax on Inline + Visual)".
   - Add new "v1.2 → v1.3 (2026-05-26)" subsection above the existing v1.1→v1.2 subsection.
   - Refresh `last_confirmed: 2026-05-26`, `updated: 2026-05-26`.

3. **No new wiki pages.**

### T11 — Rewrite `ai/PLAN.md` as RESULT packet

- Archive the B-SUNO-004 r1 plan packet (or latest approved revision) as Archive entry 9 in `.SunoSongSetup-wiki/wiki/reference/ai-plan-archive.md` (archive-before-edit discipline).
- Append v1.3 execution entry to `ai/EXECUTION_LOG.md`.
- Rewrite `ai/PLAN.md` as the v1.3 RESULT packet using the v1.2 RESULT template structure (17 sections).
- Include the standard handoff sections (USER ACTION NEEDED with Step 1 Specialist + Step 2 Lead instruction texts).

## Validation gates

| Gate | Check | Block on failure? |
|---|---|---|
| T0 | Baseline green (build + 31 tests) | Yes |
| T3 | 45 tests passing (31 + 16 new) | Yes (debug + re-commit) |
| T6 | 45 tests passing post-code-behind change | Yes |
| T7 | Dev smoke-launch (no XAML parse errors, basic click semantics work) | Yes |
| T8 | Publish exe builds + launches | Yes |
| T9 | USER REVIEW PASS on 12-case matrix | Yes (iterate until PASS) |
| T10 | Wiki pages updated with lifecycle frontmatter | Yes |
| T11 | RESULT packet written + archive entry 9 prepended + EXECUTION_LOG appended | Yes |

## Rollback plan

If at any point the v1.3 mechanism cannot be made to work reliably:

1. Hard reset `j:\SunoMetatagApp\` `main` to baseline commit `574ebd6` (v1.2 closeout).
2. v1.2 visual + behavioral state restored exactly.
3. Surface failure to user via USER ACTION NEEDED with diagnosis + recommendation (e.g., redesign merge algorithm, alternate UX trigger, defer to v2).

Rollback is **safe and reversible** because:
- v1.3 changes are confined to two source files + one test file in the `SunoMetatagApp` sibling repo.
- No `tags.json` content changes.
- No theme/visual changes.
- No new dependencies.
- SunoSongSetup workflow files (`ai/PLAN.md`, etc.) are non-git but easily restorable from archive.

## Risks

| Risk | Likelihood | Mitigation |
|---|---|---|
| Merge algorithm has a corner case that breaks user expectation | Medium | 14 unit tests + 6 smoke cases enumerate boundary conditions; spec §4 documents 14 edge-case rows. |
| Replacing Command binding with Click handler breaks an unforeseen MVVM test | Low | No existing unit test references the XAML binding directly; tests use the VM directly. Verify with `dotnet test`. |
| Shift+click modifier detection fails on non-US keyboards / accessibility setups | Low | `Keyboard.Modifiers` is layout-independent in WPF; ModifierKeys.Shift maps to any Shift key. |
| Future hotkey work (B-004) collides with the Click-handler approach | Low | `InsertTagStackedCommand` exposed alongside the public method (planner-default decision §11.3). |
| In-cycle scope creep (visual tweaks, additional UX) | Medium | Explicit non-goals in spec §5; this plan does not include any visual changes. |

## Specialist activation rationale

**FRONTEND/UX activation recommended** for v1.3 plan-phase review:

- New user-facing interaction (Shift+click) that affects discoverability, predictability, and accessibility.
- Modifier-key semantics for buttons is a non-trivial design surface (modifier discoverability, no-bracket fallback affordance, no visual preview of merge target).
- Edge-case enumeration is broad; specialist should verify the 14-case table is complete or call out missing scenarios.

**ENGINE specialty NOT activated** — no geometry, determinism-sensitive, or export-contract changes.

## Wiki impact forecast

- **UPDATE** [[sunometatag-inline-editor]] (new "Stacked-syntax insertion (Shift+click)" section)
- **UPDATE** [[sunometatag-app]] (v1.2→v1.3 subsection + title bump)
- **APPEND** [[ai-plan-archive]] (Archive entry 9 at closeout)
- **APPEND** [[ai-engine-review-archive]] / [[ai-review-archive]] if retention rule triggers (planner does not own these archive append decisions for the live packets)
- **NEW pages:** none

## Open scope decisions (carried from spec §11)

For Specialist + Lead Reviewer ratification at plan-phase review:

1. Backlog ID namespace: keep `B-SUNO-NNN` or remap to `B-028`?
2. Wiki extension vs new page: extend [[sunometatag-inline-editor]] or create [[sunometatag-stacked-syntax]]?
3. Optional `InsertTagStackedCommand` (RelayCommand wrapper): expose or omit?
4. Style-level Shift+click tooltip Setter: add or skip?

Planner defaults: keep, extend, expose, add. Lead may override at plan-phase APPROVED-with-notes.
