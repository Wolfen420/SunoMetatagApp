# Suno Metatag v1.1 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor the v1 SunoMetatagApp from a section-editor with chip-row broadcast to an inline-only model where tag clicks insert `[Bracket]` at the caret position in whichever lyric textbox has focus. No chip row, no arm toggle, no broadcast badge. Simpler data model, fewer LOC, smaller test surface.

**Architecture:** Light MVVM unchanged in shape. The `Section` model becomes a single-property class (`Lyrics`). `MainViewModel` tracks `FocusedSection` + `FocusedCaretPosition` + `FocusedSelectionLength` via View focus events. `InsertTag` modifies `FocusedSection.Lyrics` via `string.Insert` at the tracked caret, then raises `CaretRestoreRequested` for the View to set `TextBox.SelectionStart` back to the new position.

**Tech Stack:** Unchanged — WPF, .NET 8, C# 12, CommunityToolkit.Mvvm, System.Text.Json, xUnit.

**Reference spec:** [`docs/specs/2026-05-25-suno-metatag-v1.1-inline-tag-insertion.md`](../specs/2026-05-25-suno-metatag-v1.1-inline-tag-insertion.md) (spec at **r2**).

**Plan revision:** **r2 (2026-05-25)** — incorporates spec r2 resolutions (HIGH-1 LostFocus defer-clear, HIGH-2 focused-affordance + dim picker, MEDIUM-1 wiki rename-and-supersede, MEDIUM-2 move-boundary `CanExecute`). Test count 30 → 31; smoke matrix 5 → 6 cases; backlog adds B-024 (syntax highlighting) and retires B-016 (arm hint moot) at v1.1 closeout.

**Supersedes:** [`docs/plans/2026-05-25-suno-metatag-section-editor.md`](2026-05-25-suno-metatag-section-editor.md) (v1, executed and closed).

**Closes:** B-023 in [`docs/BACKLOG.md`](../BACKLOG.md).

**Prerequisites:** v1 shipped and on `main` of `j:\SunoMetatagApp\`. .NET 8 SDK present.

---

## Notes for the implementer

- **All commands assume CWD = `j:\SunoMetatagApp\`** unless stated otherwise.
- **Test framework:** xUnit with plain `Assert`, no FluentAssertions (carries over from v1).
- **Commit style:** Conventional commits (`refactor:`, `feat:`, `test:`, `chore:`, `docs:`). One commit per task unless noted.
- **The big refactor lives in Task 1** as one coordinated commit so the project compiles + tests stay green at every commit boundary. Splitting it would leave intermediate commits in a non-buildable state.
- **No new NuGet packages.** All dependencies from v1 stay.
- **Files deleted** (Task 2): `src/SunoMetatagApp/ArmedToGlyphConverter.cs`, `src/SunoMetatagApp/CountGreaterThanOneToBoolConverter.cs`. Their references in `App.xaml` and `MainWindow.xaml` ResourceDictionary entries also go.

---

## Test count: 31 (was 58 in v1; was 30 in r1)

Breakdown by file:
- `TagServiceTests.cs` — 5 (unchanged)
- `TagServiceFilterTests.cs` — 7 (unchanged)
- `SectionTests.cs` — 2 (simplified from 8)
- `PreviewBuilderTests.cs` — 7 (simplified from 10)
- `MainViewModelTests.cs` — **10** (rewritten from 28; r2 added one test for "two consecutive InsertTag with different FocusedSection lands in correct sections")

Smoke matrix: **6 cases** (down from 11 in v1; r2 added rapid-fire tag clicks case).

---

## Task 0 — Pre-flight (no commit)

**Goal:** Confirm v1 main is green before refactor begins.

- [ ] `git status` — clean working tree (or note pending B-023 BACKLOG entry).
- [ ] `git log --oneline -n 20` — verify 15 v1 commits land per `ai/EXECUTION_LOG.md`.
- [ ] `dotnet build` — green.
- [ ] `dotnet test` — 58 passing.

**Exit criteria:** v1 baseline confirmed.

---

## Task 1 — Coordinated refactor commit (`refactor: collapse to inline-only section model for v1.1`)

**Goal:** All production + test code changes for v1.1 land in one coordinated commit. Project compiles and tests pass at commit boundary.

### 1a. Simplify `Section` model

Edit `src/SunoMetatagApp/Models/Section.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace SunoMetatagApp.Models;

public partial class Section : ObservableObject
{
    [ObservableProperty] private string _lyrics = string.Empty;
}
```

Removed: `Tags`, `IsArmed`, `RemoveTagCommand`, `MoveTagLeftCommand`, `MoveTagRightCommand`, the chip-row collection subscription wiring.

### 1b. Simplify `PreviewBuilder`

Edit `src/SunoMetatagApp/Services/PreviewBuilder.cs`:

```csharp
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

public static class PreviewBuilder
{
    public static string Build(IReadOnlyList<Section> sections, string newline)
    {
        if (sections is null || sections.Count == 0) return string.Empty;
        var nonEmpty = sections
            .Where(s => !string.IsNullOrWhiteSpace(s.Lyrics))
            .Select(s => s.Lyrics);
        return string.Join(newline + newline, nonEmpty).TrimEnd('\r', '\n');
    }
}
```

Rules: walk sections in order, skip whitespace-only, join with one blank line between non-empty sections, trim trailing CR/LF.

### 1c. Rewrite `MainViewModel`

Edit `src/SunoMetatagApp/ViewModels/MainViewModel.cs` to match the spec §9 contract.

Key additions:
- `[ObservableProperty] private Section? _focusedSection;`
- `[ObservableProperty] private int _focusedCaretPosition;`
- `[ObservableProperty] private int _focusedSelectionLength;`
- `public event EventHandler<int>? CaretRestoreRequested;`
- New `InsertTag(TagDefinition?)` semantics:

```csharp
[RelayCommand]
private void InsertTag(TagDefinition? tag)
{
    if (tag is null || FocusedSection is null) return;
    var section = FocusedSection;
    var lyrics = section.Lyrics ?? string.Empty;
    var caret = Math.Clamp(FocusedCaretPosition, 0, lyrics.Length);
    var selLen = Math.Clamp(FocusedSelectionLength, 0, lyrics.Length - caret);
    var bracket = tag.Bracket;
    section.Lyrics = lyrics[..caret] + bracket + lyrics[(caret + selLen)..];
    FocusedCaretPosition = caret + bracket.Length;
    FocusedSelectionLength = 0;
    CaretRestoreRequested?.Invoke(this, FocusedCaretPosition);
}
```

Subscriptions: keep `Sections.CollectionChanged` and each Section's `PropertyChanged` (for `Lyrics`) for `RecomputePreview`. Drop all chip-row and arm-state subscription code.

Move boundary disabling via `CanExecute` predicates (r2 — resolves MEDIUM-2):

```csharp
[RelayCommand(CanExecute = nameof(CanMoveSectionUp))]
private void MoveSectionUp(Section section)
{
    var i = Sections.IndexOf(section);
    Sections.Move(i, i - 1);
}
private bool CanMoveSectionUp(Section? section) =>
    section is not null && Sections.IndexOf(section) > 0;

[RelayCommand(CanExecute = nameof(CanMoveSectionDown))]
private void MoveSectionDown(Section section)
{
    var i = Sections.IndexOf(section);
    Sections.Move(i, i + 1);
}
private bool CanMoveSectionDown(Section? section)
{
    if (section is null) return false;
    var i = Sections.IndexOf(section);
    return i >= 0 && i < Sections.Count - 1;
}
```

The `RelayCommand` source generator emits `IRelayCommand` properties (`MoveSectionUpCommand`, `MoveSectionDownCommand`) that include `NotifyCanExecuteChanged()` methods. In the existing `OnSectionsCollectionChanged` handler (which already drives `RecomputePreview`), add the requery hop so move-buttons re-evaluate when the collection reshapes:

```csharp
private void OnSectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
{
    // ... existing per-item subscribe/unsubscribe + RecomputePreview() ...
    MoveSectionUpCommand.NotifyCanExecuteChanged();
    MoveSectionDownCommand.NotifyCanExecuteChanged();
}
```

WPF's `CommandManager.RequerySuggested` covers most user-initiated requeries (focus changes, mouse clicks), but `ObservableCollection.Move` doesn't trigger it; the explicit notify keeps button enable-state correct when sections reorder. **This fully closes the v1 closeout PASS-WITH-NOTES "section move boundary enablement" item.** Buttons grey out at boundaries instead of no-opping when clicked.

### 1d. Rewrite section template + tag-picker template in `MainWindow.xaml`

Remove:
- The chip row (`WrapPanel` with `<Border>`+`<TextBlock>`+`◀`+`▶`+`✕` per chip).
- The arm toggle button on the section toolbar.
- The broadcast badge in the right pane.
- The arm hint banner.
- The `ArmedToGlyphConverter` and `CountGreaterThanOneToBoolConverter` static-resource declarations.

Section template — r2 — with focused-border affordance (HIGH-2 part 1) and CanExecute-driven `▲`/`▼` button disabling (MEDIUM-2):

```xml
<DataTemplate DataType="{x:Type m:Section}">
    <Border Margin="0,0,0,8" Padding="6">
        <Border.Style>
            <Style TargetType="Border">
                <Setter Property="BorderBrush" Value="LightGray" />
                <Setter Property="BorderThickness" Value="1" />
                <Style.Triggers>
                    <DataTrigger Binding="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=DataContext.FocusedSection}" Value="{Binding}">
                        <Setter Property="BorderBrush" Value="SteelBlue" />
                        <Setter Property="BorderThickness" Value="2" />
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Border.Style>
        <DockPanel>
            <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" HorizontalAlignment="Right">
                <!-- Command bindings carry CanExecute behavior automatically; buttons grey at boundaries -->
                <Button Content="▲" Command="{Binding DataContext.MoveSectionUpCommand, RelativeSource={RelativeSource AncestorType=Window}}" CommandParameter="{Binding}" ToolTip="Move section up" />
                <Button Content="▼" Command="{Binding DataContext.MoveSectionDownCommand, RelativeSource={RelativeSource AncestorType=Window}}" CommandParameter="{Binding}" ToolTip="Move section down" />
                <Button Content="×" Click="DeleteSectionButton_Click" CommandParameter="{Binding}" ToolTip="Delete section" />
            </StackPanel>
            <TextBox
                Text="{Binding Lyrics, UpdateSourceTrigger=PropertyChanged}"
                AcceptsReturn="True"
                TextWrapping="Wrap"
                MinHeight="120"
                VerticalScrollBarVisibility="Auto"
                GotKeyboardFocus="LyricTextBox_GotFocus"
                LostKeyboardFocus="LyricTextBox_LostFocus"
                SelectionChanged="LyricTextBox_SelectionChanged" />
        </DockPanel>
    </Border>
</DataTemplate>
```

Tag-picker — r2 — with **implicit Focusable=False style** (LOW item) and **dim-when-no-focus affordance** (HIGH-2 part 2):

```xml
<ScrollViewer x:Name="TagPickerScroll">
    <ScrollViewer.Resources>
        <Style TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
            <Setter Property="Focusable" Value="False" />
            <Setter Property="Opacity" Value="1.0" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=DataContext.FocusedSection}" Value="{x:Null}">
                    <Setter Property="Opacity" Value="0.55" />
                    <Setter Property="ToolTip" Value="Click in a lyric textbox first, then click a tag." />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </ScrollViewer.Resources>
    <ItemsControl ItemsSource="{Binding FilteredTags}">
        <ItemsControl.ItemTemplate>
            <DataTemplate>
                <Button
                    Content="{Binding Label}"
                    Command="{Binding DataContext.InsertTagCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                    CommandParameter="{Binding}"
                    ToolTip="{Binding Description}"
                    ToolTipService.IsEnabled="{Binding Description, Converter={StaticResource StringIsNotEmptyConverter}}" />
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

The `▲`/`▼` button greying happens automatically because the bound `RelayCommand`'s `CanExecute` predicate returns false at boundaries — WPF's `Button.IsEnabled` flows from the bound command's `CanExecute` result.

### 1e. Rewrite code-behind in `MainWindow.xaml.cs`

Add:
- `private TextBox? _currentFocusedTextBox;` field.
- `LyricTextBox_GotFocus(object, KeyboardFocusChangedEventArgs)` — set VM's FocusedSection/CaretPosition/SelectionLength; cache `_currentFocusedTextBox`.
- **`LyricTextBox_LostFocus(object, KeyboardFocusChangedEventArgs)` — r2 (resolves HIGH-1):** defer-clear `FocusedSection` to null when focus leaves to anywhere outside any lyric textbox. Defer via `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)` so `Focusable=False` button clicks don't trip the clear. The deferred lambda re-checks `vm.FocusedSection` and `Keyboard.FocusedElement` before clearing — if focus moved to a different lyric textbox in the meantime (which means `GotKeyboardFocus` already ran), leave the new state alone. Full code:

  ```csharp
  private void LyricTextBox_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
  {
      if (sender is not TextBox tb || tb.DataContext is not Section section) return;
      if (DataContext is not MainViewModel vm) return;

      var sectionAtLossTime = section;
      var tbAtLossTime = tb;
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
      {
          if (vm.FocusedSection != sectionAtLossTime) return; // GotFocus already ran on a different lyric textbox
          if (Keyboard.FocusedElement is TextBox focusedTb &&
              focusedTb.DataContext is Section)
          {
              return; // Focus is now on another lyric textbox; let its GotFocus drive state
          }
          // Focus moved outside any lyric textbox. Clear so next tag click is a deterministic no-op.
          vm.FocusedSection = null;
          vm.FocusedCaretPosition = 0;
          vm.FocusedSelectionLength = 0;
          if (ReferenceEquals(_currentFocusedTextBox, tbAtLossTime))
              _currentFocusedTextBox = null;
      }));
  }
  ```

- `LyricTextBox_SelectionChanged(object, RoutedEventArgs)` — update VM's CaretPosition/SelectionLength while focus stays on this textbox.
- Subscribe to `MainViewModel.CaretRestoreRequested` in `OnDataContextChanged` (mirror pattern of `CopyRequested`).
- `OnCaretRestoreRequested(object?, int)` — uses `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)` to set `_currentFocusedTextBox.SelectionStart` after the bound `Lyrics` change propagates.

Keep:
- `OnDataContextChanged` — extend to wire up the new event.
- `OnWindowLoaded` + `FindFirstLyricTextBox` + `FindLyricsTextBox` — unchanged.
- `OnCopyRequested` — unchanged.
- `DeleteSectionButton_Click` — simplify the "has content" check from `section.Tags.Count > 0 || !string.IsNullOrEmpty(section.Lyrics)` to just `!string.IsNullOrEmpty(section.Lyrics)`.
- `CopyErrorButton_Click` — unchanged.

### 1f. Simplify `SectionTests.cs`

Delete most tests; keep:
- `Default_Lyrics_IsEmpty`
- `Setting_Lyrics_RaisesPropertyChanged`

### 1g. Simplify `PreviewBuilderTests.cs`

Keep simplified versions of:
- `Build_WithEmptyList_ReturnsEmptyString`
- `Build_WithSingleSection_ReturnsLyrics`
- `Build_WithTwoSections_JoinsWithBlankLine`
- `Build_SkipsWhitespaceOnlySections`
- `Build_TrimsTrailingNewlines`
- `Build_WithCrlfNewline_PreservesNewlineStyle`
- `Build_LyricsWithInlineBracketTokens_PassThroughVerbatim` (new — proves inline `[Tag]` tokens are not specially rendered)

### 1h. Rewrite `MainViewModelTests.cs`

10 tests (r2 — added test 9 from specialist LOW item):

1. `AddSection_AppendsSectionToCollection`
2. `RemoveSection_RemovesFromCollection`
3. `MoveSectionUp_CanExecute_AtIndexZero_ReturnsFalse` (r2 — CanExecute predicate is the disabling mechanism; assert false at index 0)
4. `MoveSectionDown_CanExecute_AtLastIndex_ReturnsFalse` (r2)
5. `MoveSectionUp_FromMiddle_SwapsWithPrior`
6. `InsertTag_WithNoFocusedSection_DoesNothing`
7. `InsertTag_WithFocusedSection_InsertsBracketAtCaret`
8. `InsertTag_WithSelectionRange_ReplacesSelectionWithBracket`
9. **`InsertTag_TwoConsecutiveCallsWithDifferentFocusedSections_LandsInCorrectSections` (r2 — new):** Set `FocusedSection = section1`, call `InsertTag(tag1)`, verify section1.Lyrics gains `[tag1]`. Then set `FocusedSection = section2`, call `InsertTag(tag2)`, verify section2.Lyrics gains `[tag2]` and section1.Lyrics stays as-is. Pins the per-call section-targeting contract against any future regression of the focus-tracking → InsertTag wiring.
10. `AfterRemoveSection_MutatingRemovedSectionLyrics_DoesNotChangePreviewText` (subscription leak guard, carried over from v1)

### 1i. Validate

- [ ] `dotnet build` — green.
- [ ] `dotnet test` — **31 passing**, 0 failing (r2 — was 30 in r1; added focus-targeting test).

### 1j. Commit

```
git add -A
git commit -m "refactor: collapse to inline-only section model for v1.1 (closes B-023)"
```

**Exit criteria:** Test count = 30, all green. Project structure matches spec.

---

## Task 2 — Delete obsolete converters (`chore: drop arm/broadcast converters`)

If Task 1 didn't already remove them:

- [ ] `git rm src/SunoMetatagApp/ArmedToGlyphConverter.cs`
- [ ] `git rm src/SunoMetatagApp/CountGreaterThanOneToBoolConverter.cs`
- [ ] Verify no remaining references in any `.xaml` (search for `ArmedToGlyph` and `CountGreaterThanOne`).
- [ ] `dotnet build` — green.
- [ ] Commit:
  ```
  git commit -m "chore: delete ArmedToGlyphConverter and CountGreaterThanOneToBoolConverter"
  ```

If Task 1 handled deletion as part of its commit, mark Task 2 as no-op and move on.

---

## Task 3 — Manual run validation (`feat: dev validation pass`)

No commit.

- [ ] `dotnet run --project src/SunoMetatagApp` — app launches.
- [ ] Initial focus lands in first section's lyric textbox (verify by typing immediately).
- [ ] Type some lyrics. Click a tag from the picker. Bracket inserts at caret. Caret lands after `]`. Textbox stays focused.
- [ ] Click in the middle of existing text. Click a tag. Bracket inserts at that position.
- [ ] Select a word. Click a tag. Bracket replaces selection.
- [ ] Tab out of the lyric textbox (or click in the search textbox). Click a tag. **Nothing should happen** (no focus = no insertion).
- [ ] Close the app.

**Exit criteria:** All five mid-task checks pass.

---

## Task 4 — Publish single-file exe (`chore: publish v1.1`)

```
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

- [ ] Verify `publish/SunoMetatagApp.exe` exists (~150 MB).
- [ ] Verify `publish/tags.json` exists.
- [ ] **Smoke-launch the published exe before USER REVIEW.** Run `publish\SunoMetatagApp.exe` and confirm the window appears with default UI rendered. If launch fails with `XamlParseException` or similar, the published exe surfaces parse-time XAML defects that unit tests cannot catch (XAML is parsed at `Window.Show()`, not at compile or test time). Diagnose by running `dotnet run --project src/SunoMetatagApp --no-build` to capture the stderr trace, then fix and republish before gating to USER REVIEW. This step is *new in v1.1 post-closeout* — v1.1 r2 spec §10 had an illegal `DataTrigger Value="{Binding}"` markup that compiled clean and tested clean but crashed at launch; the only signal was running the published exe.
- [ ] No commit (publish artifacts are gitignored per v1).

---

## Task 5 — Manual smoke matrix on published exe (USER REVIEW NEEDED)

Run `publish/SunoMetatagApp.exe`. Verify each case:

### Case 1 — Initial focus + focused-affordance + move-boundary disable (r2)
Launch app. Caret should be in the first section's lyric textbox. The section's border should be **SteelBlue with 2px thickness** (focused affordance). Typing immediately should land text in that section.

Check the section toolbar: `▲` button should be **greyed out / disabled** (CanExecute false at index 0). Click `+ Add section`. Now two sections. Click into section 2's textbox. Section 2's border becomes SteelBlue; section 1's returns to LightGray. Section 2's `▼` button should be greyed (last index).

### Case 2 — Inline insertion at caret
Type "Walking down the street". Click between "the" and "street" (or use arrow keys). Click `[Guitar]` tag. Verify: `Walking down the[Guitar] street`. Caret should be between `]` and space.

### Case 3 — Selection replacement
Type "Walking down the street". Select "the". Click `[Powerful]` tag. Verify: `Walking down [Powerful] street`. Caret should be after `]`.

### Case 4 — Focus-loss + dim affordance + no-op (r2 — was just no-op)
With a lyric textbox focused (border SteelBlue), click in the **search textbox** in the right pane. Verify:
- The lyric section's border returns to **LightGray** (focused affordance cleared).
- Tag-picker buttons render at **~55% opacity** (visibly dimmer than normal).
- Hover any tag-picker button: tooltip reads **"Click in a lyric textbox first, then click a tag."**
- Click any tag-picker button: **nothing happens** — no insertion into the previously-focused lyric textbox at its stale caret, no chip anywhere, no error.

Click back into the lyric textbox. Verify the border returns to SteelBlue and the picker buttons return to full opacity.

### Case 5 — Multi-section preview
Click "+ Add section". Type lyrics in both sections (different content). Insert tags inline in both. Verify the preview pane shows both sections separated by a blank line, with inline tag tokens preserved verbatim. Click "Copy all". Paste somewhere to verify clipboard.

### Case 6 — Rapid-fire tag clicks (r2 — new, surfaces Dispatcher ordering edge cases)
With a lyric textbox focused, type "AB" and place the caret between A and B. Within one second, click three different tags in succession (e.g., `[Guitar]` → `[Drums]` → `[Powerful]`). Verify:
- All three brackets land at the moving caret position in click order.
- Final text reads `A[Guitar][Drums][Powerful]B` (caret after the third `]`).
- No tag is lost, duplicated, or inserted at a stale caret position.
- Textbox stays focused; border stays SteelBlue throughout.

If this case fails (out-of-order insertion, lost clicks, or wrong caret), the `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)` caret-restore pattern has a timing issue — escalate before approval.

**Result format:** Reply with PASS/FAIL per case + any observations.

---

## Task 6 — Update `docs/BACKLOG.md` (`docs: backlog updates for v1.1 release`)

r2: three edits in one commit.

- [ ] Replace the B-023 entry with the retired form:
  ```
  ## B-023 — *(retired — hybrid inline tag insertion shipped in v1.1)*
  ```
- [ ] Replace the B-016 entry (permanent arm-hint) with the retired form, since v1.1 has no arm state:
  ```
  ## B-016 — *(retired — arm-toggle removed in v1.1; hint is moot)*
  ```
- [ ] B-024 was added at planning time (r2 pre-execution); confirm it's present in the form below. If missing, add:
  ```
  ## B-024 — Syntax-highlight `[Tag]` tokens in lyric textbox
  **Status:** open · **Priority:** low
  **Source:** v1.1 r2 FRONTEND/UX advisory (LOW)
  **Acceptance:** Inline `[Tag]` bracket tokens in the lyric textbox render with a distinct color (e.g., SteelBlue) so they visually separate from user lyrics. Requires `RichTextBox` or AvalonEdit; plain `TextBox` doesn't support inline coloring. Trigger to ship: user reports difficulty visually scanning lyrics with many inline tags.
  ```
- [ ] Re-check the file. Other backlog items unchanged (B-001..B-022 minus retired B-006/B-016, plus B-023 retired and B-024 added — 21 open + 3 retired).
- [ ] Commit:
  ```
  git commit -m "docs: retire B-023 (shipped) and B-016 (moot); seed B-024 (syntax highlighting)"
  ```

---

## Task 7 — Update wiki pages with rename + supersede (r2 — resolves MEDIUM-1)

In `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\`:

### 7a. Create new architecture page

- [ ] **Create `architecture/sunometatag-inline-editor.md`** with frontmatter:
  ```yaml
  title: SunoMetatagApp Inline-Editor Architecture
  type: architecture
  status: active
  summary: v1.1 inline-tag-insertion architecture. Section model = Lyrics only; tag clicks insert [Bracket] at caret in focused lyric textbox; no chip row, no broadcast.
  sources:
    - j:\SunoMetatagApp\docs\specs\2026-05-25-suno-metatag-v1.1-inline-tag-insertion.md (r2)
    - j:\SunoMetatagApp\src\SunoMetatagApp\ (post-v1.1 main)
  project_paths:
    - j:\SunoMetatagApp\src\SunoMetatagApp\
  related:
    - "[[sunometatag-app]]"
    - "[[sunometatag-sibling-repo]]"
    - "[[focus-flip-stale-insert]]"
  supersedes: "[[sunometatag-section-editor]]"
  created: 2026-05-25
  updated: 2026-05-25
  confidence: high
  owner: planner
  last_confirmed: 2026-05-25
  review_due: 2026-08-25
  claim_state: active
  tags: [sla, architecture, sunometatag, wpf, mvvm]
  cssclasses: [architecture-note]
  ```
- [ ] Body: Overview (inline-only model, simpler than v1), Layers (Models / Services / VMs / Converters — note 2 v1 converters deleted), Reactive recompute (now driven only by Lyrics changes), View (focused-border affordance, dim-when-no-focus, implicit Focusable=False style), Code-behind (initial-focus walker carried over; LostFocus defer-clear new), App startup (unchanged).

### 7b. Mark old architecture page as superseded

- [ ] **Edit `architecture/sunometatag-section-editor.md`** (do NOT delete; preserve v1 chip-row claims as historical record):
  - Add at top of file (above the title `#`): `> [!warning] Superseded — see [[sunometatag-inline-editor]]`
  - In frontmatter, add: `superseded_by: "[[sunometatag-inline-editor]]"` and change `claim_state: active` → `claim_state: superseded`.

### 7c. Update cross-references

- [ ] **Edit `features/sunometatag-app.md`:**
  - `related:` field: replace `[[sunometatag-section-editor]]` with `[[sunometatag-inline-editor]]`.
  - Body: refresh the "What landed" section with v1.1's inline-only behavior. Add a "v1 → v1.1 (2026-05-25)" subsection describing the model change.
  - Frontmatter: bump `last_confirmed: 2026-05-25`.
- [ ] **Edit `decisions/sunometatag-sibling-repo.md`:** `related:` field: replace `[[sunometatag-section-editor]]` with `[[sunometatag-inline-editor]]`. No body changes.

### 7d. Create new risk page (per specialist suggestion)

- [ ] **Create `risks/focus-flip-stale-insert.md`** documenting *why* `LyricTextBox_LostFocus` defer-clears `FocusedSection`:
  ```yaml
  title: Focus-Flip Stale-Insert Risk
  type: risk
  status: active
  summary: Without defer-clearing FocusedSection on LostKeyboardFocus, a user who focuses a lyric textbox then clicks into the search/preview textbox and clicks a tag sees the bracket inserted at the stale caret position in the previously-focused lyric textbox.
  sources:
    - ai/ENGINE_REVIEW.md (v1.1 r1 specialist HIGH-1 finding)
    - j:\SunoMetatagApp\src\SunoMetatagApp\MainWindow.xaml.cs (LyricTextBox_LostFocus)
  related:
    - "[[sunometatag-inline-editor]]"
  created: 2026-05-25
  updated: 2026-05-25
  confidence: high
  owner: planner
  last_confirmed: 2026-05-25
  review_due: 2026-08-25
  claim_state: active
  tags: [sla, risk, sunometatag, focus]
  cssclasses: [risk-note]
  ```
- [ ] Body: describe the stale-insert scenario, the defer-clear mitigation, why removing the defer-clear is dangerous, and the dim-affordance signal that backs it visually.

### 7e. Append v1.1 plan packet entry to archive

- [ ] **Edit `reference/ai-plan-archive.md`:** prepend a new "Archive entry 3 — SunoMetatagApp v1.1 r1 Plan Packet — superseded by r2 on 2026-05-25" with the r1 packet content (the v1.1 plan packet that returned NEEDS_REVISION). The r2 packet currently lives in `ai/PLAN.md` and archives at v1.1 RESULT closeout.

No commit in `j:\SunoMetatagApp\` (wiki lives in SunoSongSetup which is not a git repo). Just save the wiki edits.

---

## Task 8 — Final result packet (`ai/PLAN.md` rewrite)

After USER REVIEW (Task 5) returns PASS:

- [ ] Archive current v1.1 plan packet content from `ai/PLAN.md` into `.SunoSongSetup-wiki/wiki/reference/ai-plan-archive.md` (prepend as a new "Archive entry 3" before the existing entries; per the retention rule).
- [ ] Rewrite `j:\SunoSongSetup\ai\PLAN.md` as the v1.1 RESULT packet using `RESULT_REVIEW_TEMPLATE.md`.
- [ ] Surface USER ACTION NEEDED for routing to Specialist + Lead closeout.

---

## Rollback plan

Per-task rollback:
- **Task 1** — `git revert <task-1-commit>`. Brings v1 back fully.
- **Task 2** — restore from git: `git checkout <task-1-commit>~1 -- src/SunoMetatagApp/ArmedToGlyphConverter.cs src/SunoMetatagApp/CountGreaterThanOneToBoolConverter.cs`.
- **Tasks 3–7** — no production code change after Task 2.

Full rollback to v1: `git reset --hard <last-v1-commit>` (requires user authorization per project rules — destructive).

Repository nuke: `Remove-Item -Recurse -Force j:\SunoMetatagApp` (sibling repo, does not affect SunoSongSetup).

---

## Out of scope for this plan

- Inline `[/Tag]` close-bracket pairing or "tag around selection" mode. Optional v2 backlog item if specialist flags as desirable.
- Right-click on `[Tag]` token to remove it. v1.1 user just deletes the text via keyboard.
- Per-section caret memory across focus changes (v1.1 tracks current caret only; switching focus loses prior position).
- Anything in v1's existing backlog (B-001..B-022) other than the section move-boundary item already addressed in Task 1c.

---

## Summary

10 tasks (0–8 plus pre-flight). One coordinated refactor commit + small follow-on commits + USER REVIEW gate + result packet. v1.1 ships a simpler app than v1 by removing the chip-row metaphor entirely.
