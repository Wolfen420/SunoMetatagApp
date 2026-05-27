# SunoMetatagApp v1.10 — Implementation Plan (B-SUNO-012 Tag-Picker Pane Focus Preservation)

- **Spec:** `j:\SunoMetatagApp\docs\specs\2026-05-27-suno-metatag-v1.10-picker-focus-preservation.md`
- **Approved packet:** `ai/PLAN.md` B-SUNO-012 / v1.10 r1 (Lead Reviewer 2026-05-27, `APPROVED (PASS-WITH-NOTES)` with 4 LOW absorption notes carried in `target_item`).
- **Working baseline:** `master` tip `1e623ce` (v1.9 closeout = B-SUNO-008b PASS, parent B-SUNO-008 retired).
- **Test baseline:** 126/126 green.

## Tasks T0-T8

### T0 — Baseline check

- Verify `master` tip is `1e623ce`.
- `dotnet build` → green.
- `dotnet test tests/SunoMetatagApp.Tests --no-build` → 126/126 green.
- Verify working tree state: pre-existing modified `README.md` (out of v1.10 scope; carry-over from v1.9 RESULT §4 disclosure), untracked `.tmp/` (gitignored), untracked `docs/reference/B-SUNO-007b-suno-meta-tags-database-decision-table-2026-05-27.md` (out of v1.10 scope).
- v1.10 commits MUST use targeted `git add` to avoid bundling out-of-scope changes.

### T1 — Primary commit (mechanism + docs + Lead absorption #1, #3, #4)

Single primary commit containing:

1. **`src/SunoMetatagApp/MainWindow.xaml`:**
   - Line 143: change `<Grid Grid.Column="4" Margin="4">` → `<Grid Grid.Column="4" Margin="4" x:Name="TagPickerPane">`.
   - Line 130: change `LostKeyboardFocus="LyricTextBox_LostFocus"` → `LostKeyboardFocus="LyricTextBox_LostKeyboardFocus"` (absorption #3).

2. **`src/SunoMetatagApp/MainWindow.xaml.cs`:**
   - Rename method `LyricTextBox_LostFocus` → `LyricTextBox_LostKeyboardFocus` (absorption #3).
   - Add new static helper `IsAncestorOf(DependencyObject ancestor, DependencyObject? descendant)` with `VisualTreeHelper.GetParent` primary walk + `LogicalTreeHelper.GetParent` fallback for Popup boundaries (absorption #1 defensive option).
   - In `LyricTextBox_LostKeyboardFocus`'s deferred continuation, insert the new third race-cancel check between the existing `sectionAtLossTime` check and the lyric-textbox check.

3. **`docs/specs/2026-05-27-suno-metatag-v1.10-picker-focus-preservation.md`** — new spec file.

4. **`docs/plans/2026-05-27-suno-metatag-v1.10-picker-focus-preservation.md`** — this implementation plan.

Expected commit shape:
- 2 files modified in `src/`.
- 2 new files in `docs/specs/` and `docs/plans/`.
- `dotnet build` green post-edit.
- `dotnet test --no-build` returns 126/126 green (no test changes yet).

### T2 — Secondary commit (test additions)

Add F1 + F2 tests to `tests/SunoMetatagApp.Tests/MainViewModelTests.cs`:

```csharp
// F1 — v1.10 (B-SUNO-012): InsertTag routes to FocusedSection regardless of which
// UI element holds keyboard focus. Documents the VM-level invariant that v1.10's
// picker-pane defer-clear guard relies on: if FocusedSection is non-null, InsertTag
// always lands the bracket in that section's Lyrics, whatever the View's focus state.
[Fact]
public void F1_InsertTag_RoutesToFocusedSection_RegardlessOfViewFocusState()
{
    var tags = new[] { new TagDefinition("Tag", "[Verse]", "Category", null) };
    var vm = new MainViewModel(tags);
    var section = vm.Sections[0];
    vm.FocusedSection = section;
    vm.FocusedCaretPosition = 0;
    var verseTag = vm.FilteredTags.First(t => t.Bracket == "[Verse]");
    vm.InsertTagCommand.Execute(verseTag);
    Assert.Equal("[Verse]", section.Lyrics);
}

// F2 — v1.10 (B-SUNO-012): InsertTag with null FocusedSection is a no-op. Regression-
// gate against accidentally enabling insertion without a target — v1.10's picker-pane
// guard prevents premature null transitions but the InsertTag null-check remains the
// load-bearing defense.
[Fact]
public void F2_InsertTag_WithNullFocusedSection_IsNoOp()
{
    var tags = new[] { new TagDefinition("Tag", "[Chorus]", "Category", null) };
    var vm = new MainViewModel(tags);
    var section = vm.Sections[0];
    section.Lyrics = "existing";
    vm.FocusedSection = null;
    var chorusTag = vm.FilteredTags.First(t => t.Bracket == "[Chorus]");
    vm.InsertTagCommand.Execute(chorusTag);
    Assert.Equal("existing", section.Lyrics);
}
```

Expected test count post-T2: **128/128 green** (126 v1.9 baseline + 2 new).

If existing tests already cover these invariants exactly, collapse F1/F2 into a single new test documenting the v1.10 contract change. Final exact assertions verified at T2 commit time.

### T4 — Dev smoke launch

`timeout 6 dotnet run --no-build --project src/SunoMetatagApp` → expect `EXIT=124` (clean timeout, no startup exception).

### T5 — Publish artifact rebuild + smoke launch

`dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish` → produced `publish/SunoMetatagApp.exe` (~153 MB self-contained), `publish/prompts.json` 75,743 B (byte-identical to v1.9), `publish/tags.json` 30,421 B (byte-identical to v1.9).

`timeout 6 ./publish/SunoMetatagApp.exe` → expect `EXIT=124` clean.

### T6 — USER REVIEW S1-S8

Surface the S1-S8 matrix (spec §5.2) to the user. Critical cases:
- **S3** — user's reported workflow (focus lyric → click SearchBox → type → click pill).
- **S4** — ComboBox dropdown-open-pause (absorption #1) — exercises the logical-tree fallback for Popup boundary.
- **S5** — preview-pane → pills dim regression-gate (v1.1 r2 contract preservation).

### T7 — Wiki updates

Land 3 wiki updates in `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\`:

1. **`risks/focus-flip-stale-insert.md`** — extend with picker-pane exemption + v1.10 third-guard documentation. Optionally add a fourth named failure mode ("picker-pane-trap"). **Refresh frontmatter: `last_confirmed: 2026-05-27`, `review_due: 2026-11-27`** (absorption #4).
2. **`architecture/sunometatag-inline-editor.md`** — append a short paragraph under "Focus tracking + caret restoration" noting v1.10's third-guard semantics.
3. **`features/sunometatag-app.md`** — title bump v1.9 → v1.10; new `## v1.9 → v1.10 (2026-05-27)` subsection at the top.

### T8 — Consolidate execution log + rewrite ai/PLAN.md as RESULT packet

- Append v1.10 entry to `j:\SunoSongSetup\ai\EXECUTION_LOG.md` covering T0-T8.
- Archive v1.10 r1 plan packet as Archive entry 23 in `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\reference\ai-plan-archive.md` (retention rule).
- Rewrite `j:\SunoSongSetup\ai\PLAN.md` as v1.10 RESULT packet with §1-§12 + USER REVIEW outcomes table + USER ACTION NEEDED for Lead closeout.
