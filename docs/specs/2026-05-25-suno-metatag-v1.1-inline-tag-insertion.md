# Suno Metatag v1.1 — Inline Tag Insertion (Design)

**Date:** 2026-05-25
**Status:** r2 — draft revised in response to FRONTEND/UX `ADVISORY_NEEDS_REVISION` 2026-05-25 (4 blockers: 2 HIGH + 2 MEDIUM).
**Owner:** Planner (Claude)
**Target repo:** `j:\SunoMetatagApp\`
**Supersedes:** `2026-05-25-suno-metatag-section-editor-design.md` (v1, shipped). v1 design is preserved as historical record with a deprecation banner.

**Revision history:**
- 2026-05-25 r1 — Initial inline-tag-insertion design (user-pinned via AskUserQuestion: hybrid → inline-only after chip-row consequence flagged; no broadcast; focus-required; `[Tag]` literal at caret).
- 2026-05-25 r2 — Specialist `ADVISORY_NEEDS_REVISION` resolved: HIGH-1 `LostKeyboardFocus` defer-clear (§5.5, §10), HIGH-2 focused-section affordance + dim-when-no-focus picker (§5.3, §5.4, §10), MEDIUM-1 wiki rename-and-supersede pinned (§13), MEDIUM-2 move-boundary disabling via `RelayCommand(CanExecute=...)` (§5.2, §9). LOW items folded: implicit `Focusable="False"` style (§10), test count 9 → 10 (§11), smoke cases 5 → 6 (§11), backlog adds B-024 syntax highlighting.

---

## 0. Why v1.1 supersedes v1

v1 shipped a section-editor model with a **chip row above** each section's lyric textbox. Tag clicks from the picker broadcast across every armed section, appending chips to each chip row. The preview emitted chip-row tags as separate bracket lines above the lyrics.

Smoke testing exposed one gap: **there is no way to place a tag inside the middle of a lyric block.** Tags can only appear above the lyrics, in pre-set blocks. A user who wants `Walking down the [Guitar] street` cannot express that.

The user evaluated three v1.1 approaches and pinned the design via AskUserQuestion (2026-05-25):

1. **Tag placement model:** Inline `[Tag]` tokens inside the lyric textbox. No chip row.
2. **Arm requirement:** None. Focus on a lyric textbox = intent. No arm state at all.
3. **No-focus fallback:** Do nothing. Tag clicks only work when a lyric textbox is focused.
4. **Chip row fate:** Removed entirely. Section becomes lyric textbox + toolbar only.
5. **Insertion format:** `[Tag]` literal at the caret. Caret lands after the closing `]`. User adds whitespace manually.

v1.1 is **simpler than v1**, not more complex. The Section domain model shrinks; the MainViewModel sheds half its state; two converters are deleted; the XAML loses the chip row, arm toggle, broadcast badge, and arm hint.

---

## 1. Overview

A single-window WPF utility. Three resizable columns, same as v1:

- **Left (~30%):** read-only preview + Copy all button. Same as v1.
- **Middle (~40%):** vertically scrollable stack of **sections**. Each section is just `toolbar (▲ ▼ ×) + lyric textbox`. No chip row, no arm toggle.
- **Right (~30%):** tag picker — search, category dropdown, button grid. No broadcast badge, no arm hint.

The user assembles a Suno prompt by typing lyrics in section textboxes and clicking tags from the picker to insert `[Bracket]` tokens at the caret position in whichever section is currently focused.

```
┌──────────────┬──────────────────────────┬──────────────┐
│              │ ┌────────────────────┐   │              │
│   Preview    │ │ ▲ ▼ ×              │   │   Search     │
│  (read-only) │ ├────────────────────┤   │   [______]   │
│              │ │ ┌────────────────┐ │   │              │
│ [Verse]      │ │ │ [Verse]        │ │   │   Category   │
│ Walking down │ │ │ Walking down   │ │   │   [All  ▾]   │
│ the [Guitar] │ │ │ the [Guitar]   │ │   │              │
│ street       │ │ │ street ⌷       │ │   │   [Verse]    │
│              │ │ │ Feeling like   │ │   │   [Chorus]   │
│ [Chorus]     │ │ │ a beat         │ │   │   [Guitar]   │
│ Heart starts │ │ │ [Drums]        │ │   │   [Drums]    │
│ to pound     │ │ │ Heart starts   │ │   │   [Powerful] │
│              │ │ │ to pound       │ │   │   ...        │
│              │ │ └────────────────┘ │   │              │
│              │ └────────────────────┘   │              │
│              │                          │              │
│ [Copy all]   │ [+ Add section]          │              │
└──────────────┴──────────────────────────┴──────────────┘
```

No file open/save. No persistence. Scratch space.

---

## 2. Goals (v1.1)

- Inline `[Tag]` insertion at the caret in whichever lyric textbox is focused.
- Single-section targeting (no broadcast).
- Section-level structure (multiple sections, reorderable) preserved from v1.
- Simpler data model and code surface than v1.
- All v1 unit-test coverage migrated to the new model; behavioral guarantees preserved where applicable.

## 3. Non-goals (still v2+)

Unchanged from v1: persistence ([[backlog#B-005]]), favorites ([[backlog#B-001]]), themes ([[backlog#B-002]]), drag-drop reorder ([[backlog#B-003]]), hotkeys ([[backlog#B-004]]), aliases ([[backlog#B-008]]), accessibility hardening ([[backlog#B-014]]), tag library expansion to 500+ (content-side follow-on).

## 4. Architecture

Light MVVM, same as v1, but the surface is smaller:

- **Models:** `TagDefinition` (unchanged), `Section` (**simplified** — only `Lyrics` property).
- **Services:** `TagService` (unchanged), `PreviewBuilder` (**simplified** — only emits lyrics, blank line between non-empty sections).
- **ViewModels:** `MainViewModel` (**rewritten** — drops arm/chip state, adds focus tracking + caret position), `TagViewModel` (unchanged).
- **Converters:** `NullToCollapsedConverter` (unchanged), `StringIsNotEmptyConverter` (unchanged). **`ArmedToGlyphConverter` and `CountGreaterThanOneToBoolConverter` are deleted.**
- **View:** `MainWindow.xaml` (redesigned — no chip row, no arm toggle, no broadcast badge, no arm hint), `MainWindow.xaml.cs` (focus tracking + caret restore handlers).
- **App entry:** `App.xaml.cs` (unchanged structurally).

## 5. UI Behavior

### 5.1 Initial focus

Same mechanism as v1 §5.1. `MainWindow.OnLoaded` uses `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)` to walk the visual tree from the named `SectionsHost` `ItemsControl` to the first section's lyric `TextBox` and calls `.Focus()`. The user can immediately start typing or clicking tags.

### 5.2 Section toolbar

Per section:
- **▲ Move up** — `ObservableCollection<T>.Move` to swap with prior section. **Disabled when section is at index 0**, via `RelayCommand(CanExecute = nameof(CanMoveSectionUp))` predicate that greys out the button.
- **▼ Move down** — `Move` to swap with next section. **Disabled when section is at the last index**, via `RelayCommand(CanExecute = nameof(CanMoveSectionDown))` predicate.
- **× Delete** — confirms via `MessageBox` only if the section's `Lyrics` is non-empty; otherwise deletes immediately. (Same as v1 §5.6, simplified to drop the "has tags" check.)

The `CanExecute` predicates are re-evaluated when `Sections.CollectionChanged` fires (add/remove/move). WPF `CommandManager.RequerySuggested` handles the requery automatically for button click sources; the VM additionally calls `MoveSectionUpCommand.NotifyCanExecuteChanged()` and `MoveSectionDownCommand.NotifyCanExecuteChanged()` from the `Sections.CollectionChanged` handler to ensure prompt re-evaluation when the collection reshapes (this addresses cases where `CommandManager` requery is suppressed).

Boundary disabling fully addresses the v1 closeout PASS-WITH-NOTES "section move boundary enablement" item. Buttons grey out at boundaries instead of no-opping when clicked.

### 5.3 Lyric textbox + focused-section affordance

Multi-line, word-wrapped, vertical scroll on overflow. Min height of ~120px (one or two lines visible without scrolling); grows with content up to a section-level cap then scrolls.

Bound to `Section.Lyrics` with `UpdateSourceTrigger=PropertyChanged` so the live preview updates per keystroke.

The textbox supports:
- Standard text editing (type, select, delete, copy, paste).
- Inline `[Tag]` insertion at the caret position via tag-picker clicks (§5.5).
- Mouse and keyboard caret movement (the VM tracks position via focus + selection events).

**Focused-section affordance (r2 — resolves HIGH-2 part 1).** The section's outer `Border` brush + thickness change when the section's lyric textbox has keyboard focus:

- **Unfocused:** `BorderBrush="LightGray"`, `BorderThickness="1"`.
- **Focused:** `BorderBrush="SteelBlue"`, `BorderThickness="2"`.

Implemented via a `Style.Triggers` on `Border` binding to `IsKeyboardFocusWithin` on the parent `DockPanel` (or via a `DataTrigger` reading `vm.FocusedSection == this section`). The visual signal makes "which section receives the next tag click" discoverable. Cheap (~6 lines of XAML); no new VM state needed beyond the existing `FocusedSection` property.

### 5.4 Tag picker + dim-when-no-focus affordance

Same shape as v1:
- Search textbox (case-insensitive prefix match on label and bracket).
- Category dropdown (auto-populated from `tags.json` plus "All" sentinel).
- Scrollable wrap-grid of tag buttons. Each button shows the label text; full tag description in tooltip when present.
- Empty-state placeholder: "No tags match" when `FilteredTags.Count == 0`.

**No broadcast badge.** Inline insertion targets exactly one section (the focused one), so the badge is unnecessary.

**Tag buttons set `Focusable="False"`** so clicking them does not steal focus from the lyric textbox. This is the load-bearing constraint that lets caret tracking work. (r2: applied via a tag-picker-scoped implicit `<Style TargetType="Button">` in the picker's `Resources`, not per-button. See §10 XAML. Prevents future tag-picker UI variants from silently breaking caret tracking.)

**Dim-when-no-focus affordance (r2 — resolves HIGH-2 part 2).** The tag-picker buttons render at reduced opacity (0.55) and show a tooltip *"Click in a lyric textbox first, then click a tag."* when `FocusedSection == null`. When a lyric textbox is focused (`FocusedSection != null`), the buttons render at full opacity (1.0) with their normal tag-description tooltip.

Implemented via a `FocusedSectionIsNullConverter` that maps `Section?` to either `0.55` (null) or `1.0` (non-null), bound on the `Opacity` property of each tag-picker button. The tooltip swap uses a `DataTrigger` on the same converter result. Together, the affordance makes the focus-required behavior discoverable instead of silent.

### 5.5 Inline insertion behavior

The contract:

```
on TagButton click:
    if no lyric TextBox is focused:
        do nothing
    else:
        let section = the Section whose textbox has focus
        let caret = the SelectionStart of that textbox at click time
        let selectionLength = the SelectionLength of that textbox at click time
        let bracket = the clicked tag's Bracket (e.g., "[Guitar]")
        section.Lyrics = section.Lyrics[..caret] + bracket + section.Lyrics[caret + selectionLength..]
        focused TextBox.SelectionStart = caret + bracket.Length
        focused TextBox.SelectionLength = 0
        focused TextBox stays focused
```

Edge cases:
- **Selection range present:** clicking a tag replaces the selected text with the bracket (same as typing replaces selection).
- **Empty textbox + caret at position 0:** inserts at start; caret lands after `]`.
- **Caret at end of text:** inserts at end; caret lands after `]`.
- **Multi-line content, caret on line 3:** inserts at character position; no special handling of newlines.

**Focus-loss rule (r2 — resolves HIGH-1).** When keyboard focus moves *off* a lyric textbox to anywhere outside the section's lyric-textbox set (search box, category dropdown, preview pane, another window, etc.), the View's `LyricTextBox_LostKeyboardFocus` handler clears `vm.FocusedSection = null` (and resets `_currentFocusedTextBox` to null). The clear is **deferred via `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)`** so that quick focus-flips caused by `Focusable="False"` button clicks (which don't disturb keyboard focus but may briefly fire `LostFocus` for non-keyboard focus types under some conditions) do not trip the clear.

The deferred clear is canceled if, before it fires, `LyricTextBox_GotKeyboardFocus` fires on another lyric textbox in the same section stack (i.e., user moves keyboard focus to a different lyric textbox). The `GotFocus` handler sets `vm.FocusedSection` to the new section, so the pending clear lambda checks whether `vm.FocusedSection` still equals the original section before clearing — if it changed (because `GotFocus` already ran), the clear is a no-op.

This produces the deterministic UX: tag clicks insert only when a lyric textbox currently has focus; clicking outside any lyric textbox visibly removes the focused-section affordance + dims the picker, so the next tag click is a no-op (with the dim-state tooltip explaining why). No stale-insert path.

### 5.6 Add section

Bottom of middle column. Full-width `+ Add section` button. Appends a new empty `Section` to `Sections`. Focus moves to the new section's textbox.

### 5.7 Preview pane

Read-only `TextBox` (selectable, copyable). Live-updates on every change to any section's `Lyrics`. Copy all button at the bottom puts `PreviewText` on the clipboard.

No structural rendering of tags. The preview is **literally the concatenation of section lyrics** with one blank line between non-empty sections. Inline `[Tag]` tokens appear in the preview exactly where the user typed/inserted them.

### 5.8 Tag-load error banner

Unchanged from v1 §5.11. Corrupt `tags.json` → app launches in degraded mode with a copyable error banner in the right pane; sections still editable but picker is empty.

---

## 6. Tag data model

Unchanged from v1 §6. `Resources/tags.json` is the same ~115-tag file produced by v1 Task 10.

`TagDefinition`:
```csharp
public sealed record TagDefinition(string Category, string Label, string Bracket, string? Description = null);
```

`TagService` unchanged:
- `LoadAll(string path) → IReadOnlyList<TagDefinition>`
- `DistinctCategories(IReadOnlyList<TagDefinition>) → IReadOnlyList<string>` (with "All" prepended)
- `Filter(tags, search, category) → IEnumerable<TagDefinition>`
- `TagLoadException` for file/JSON errors

---

## 7. Section model (v1.1)

```csharp
public partial class Section : ObservableObject
{
    [ObservableProperty] private string _lyrics = string.Empty;
}
```

That is the entire model. No `Tags` collection, no `IsArmed`, no chip-row commands. The Section is just a named bucket of multiline text that the VM tracks in an `ObservableCollection<Section>`.

Properties dropped from v1:
- `Tags` (`ObservableCollection<TagDefinition>`)
- `IsArmed` (bool)
- `RemoveTagCommand`, `MoveTagLeftCommand`, `MoveTagRightCommand`

---

## 8. PreviewBuilder (v1.1)

```csharp
public static class PreviewBuilder
{
    public static string Build(IReadOnlyList<Section> sections, string newline)
    {
        var nonEmpty = sections
            .Where(s => !string.IsNullOrWhiteSpace(s.Lyrics))
            .Select(s => s.Lyrics);
        return string.Join(newline + newline, nonEmpty).TrimEnd('\r', '\n');
    }
}
```

Rules:
- Walk sections in order.
- Skip sections where `Lyrics` is null, empty, or whitespace.
- Join non-empty sections with **one blank line** (`newline + newline`) between them.
- Trim trailing `\r` and `\n` at end of output.
- Inline `[Tag]` tokens in lyrics pass through verbatim; no bracket-detection or special rendering.

The `newline` parameter remains injectable for testability (`"\n"` in tests, `Environment.NewLine` at runtime).

---

## 9. MainViewModel (v1.1)

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<TagDefinition> _allTags;

    public ObservableCollection<Section> Sections { get; }
    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedCategory = "All";
    [ObservableProperty] private IReadOnlyList<TagDefinition> _filteredTags = Array.Empty<TagDefinition>();
    [ObservableProperty] private string _previewText = string.Empty;
    [ObservableProperty] private string? _loadError;

    [ObservableProperty] private Section? _focusedSection;
    [ObservableProperty] private int _focusedCaretPosition;
    [ObservableProperty] private int _focusedSelectionLength;

    public event EventHandler? CopyRequested;
    public event EventHandler<int>? CaretRestoreRequested;

    [RelayCommand] private void AddSection() { /* ... */ }
    [RelayCommand] private void RemoveSection(Section section) { /* ... */ }

    // r2: CanExecute predicates replace no-op index guards. Buttons grey out at boundaries.
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
    private bool CanMoveSectionDown(Section? section) =>
        section is not null && Sections.IndexOf(section) is var i &&
        i >= 0 && i < Sections.Count - 1;

    // Wired from Sections.CollectionChanged so move-buttons re-evaluate when collection reshapes.
    private void OnSectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // ... existing subscribe/unsubscribe + RecomputePreview() ...
        MoveSectionUpCommand.NotifyCanExecuteChanged();
        MoveSectionDownCommand.NotifyCanExecuteChanged();
    }
    [RelayCommand] private void InsertTag(TagDefinition? tag)
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
    [RelayCommand] private void CopyPreview() { CopyRequested?.Invoke(this, EventArgs.Empty); }
}
```

State dropped from v1:
- `ShowArmHint` (bool)
- `ArmedSectionCount` (int)
- Arm-hint reset logic
- Chip-row event subscriptions (no chips)

State added in v1.1:
- `FocusedSection` (`Section?`) — the Section whose lyric textbox currently has keyboard focus.
- `FocusedCaretPosition` (`int`) — `SelectionStart` of that textbox at the last focus or selection change.
- `FocusedSelectionLength` (`int`) — `SelectionLength` of that textbox at the last selection change.
- `CaretRestoreRequested` (event) — fired after `InsertTag` so the View can set `TextBox.SelectionStart` back to the new caret position.

Subscriptions still required:
- `Sections.CollectionChanged` — trigger `RecomputePreview()`.
- For each `Section`, `PropertyChanged` on `Lyrics` — trigger `RecomputePreview()`.
- Matched subscribe/unsubscribe on add/remove. **Subscription-leak guard test** carried forward from v1 (the structural part — without `Tags`, the leak surface is smaller, but the discipline still matters).

Two constructors (same pattern as v1):
- `MainViewModel(IReadOnlyList<TagDefinition> tags)` — normal startup with seeded sections (one empty section).
- `MainViewModel(string loadError)` — degraded ctor; sets `LoadError` and instantiates one empty section so the user can still type.

---

## 10. View (XAML)

### Section template (r2 — focused-border affordance)

```xml
<DataTemplate DataType="{x:Type m:Section}">
    <Border Margin="0,0,0,8" Padding="6">
        <Border.Style>
            <Style TargetType="Border">
                <!-- Unfocused default -->
                <Setter Property="BorderBrush" Value="LightGray" />
                <Setter Property="BorderThickness" Value="1" />
                <Style.Triggers>
                    <!-- Focused: bound to this Section being the VM's FocusedSection -->
                    <DataTrigger Binding="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=DataContext.FocusedSection}" Value="{Binding}">
                        <Setter Property="BorderBrush" Value="SteelBlue" />
                        <Setter Property="BorderThickness" Value="2" />
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Border.Style>
        <DockPanel>
            <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,0,0,4">
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

The `▲` / `▼` buttons grey out automatically because the bound `RelayCommand`'s `CanExecute` predicate returns false at boundaries (§9). The `×` button is plain `Click` so it's always enabled (delete is always allowed; confirmation modal handles non-empty case).

### Tag picker (r2 — implicit Focusable=False style + dim-when-no-focus)

The tag picker `ScrollViewer` sets `Focusable="False"` for all buttons inside it via an implicit `<Style>` in its `Resources`, so future tag-picker UI variants can't silently break caret tracking by forgetting the per-button setting:

```xml
<ScrollViewer x:Name="TagPickerScroll">
    <ScrollViewer.Resources>
        <!-- HIGH-2 (r2): implicit Focusable=False on every Button in the picker.
             Load-bearing constraint for caret tracking; applied scope-wide so future
             additions (context menu, hotkey palette, pinned recents) inherit it. -->
        <Style TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
            <Setter Property="Focusable" Value="False" />
            <!-- HIGH-2 (r2): dim affordance when no lyric textbox is focused. -->
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
                <!-- Note: explicit ToolTip on the button is overridden by the Style's ToolTip
                     setter when FocusedSection is null (DataTrigger wins), giving the
                     "Click in a lyric textbox first" message. When focused, the per-button
                     description tooltip applies. -->
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</ScrollViewer>
```

### Code-behind handlers

```csharp
private void LyricTextBox_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
{
    if (sender is TextBox tb && tb.DataContext is Section section && DataContext is MainViewModel vm)
    {
        vm.FocusedSection = section;
        vm.FocusedCaretPosition = tb.SelectionStart;
        vm.FocusedSelectionLength = tb.SelectionLength;
        _currentFocusedTextBox = tb;
    }
}

private void LyricTextBox_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
{
    // r2 (resolves HIGH-1): defer-clear FocusedSection when focus leaves a lyric textbox.
    // The deferred clear cancels itself if GotKeyboardFocus on another lyric textbox
    // races ahead (which means user moved focus to a different lyric textbox, and the
    // new GotFocus already set FocusedSection to the new section).
    if (sender is not TextBox tb || tb.DataContext is not Section section) return;
    if (DataContext is not MainViewModel vm) return;

    var sectionAtLossTime = section;
    var tbAtLossTime = tb;
    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
    {
        // If GotKeyboardFocus already set FocusedSection to a different section,
        // the user moved within the lyric-textbox set — leave that new state alone.
        if (vm.FocusedSection != sectionAtLossTime) return;

        // If keyboard focus is now on any lyric textbox in this window, leave alone.
        // (GotFocus on the new one will sync state.)
        if (Keyboard.FocusedElement is TextBox focusedTb &&
            focusedTb.DataContext is Section)
        {
            return;
        }

        // Otherwise: focus moved outside any lyric textbox. Clear state so the
        // next tag click is a deterministic no-op + the dim affordance shows.
        vm.FocusedSection = null;
        vm.FocusedCaretPosition = 0;
        vm.FocusedSelectionLength = 0;
        if (ReferenceEquals(_currentFocusedTextBox, tbAtLossTime))
        {
            _currentFocusedTextBox = null;
        }
    }));
}

private void LyricTextBox_SelectionChanged(object sender, RoutedEventArgs e)
{
    if (sender is TextBox tb && DataContext is MainViewModel vm && tb.DataContext is Section section)
    {
        if (vm.FocusedSection == section)
        {
            vm.FocusedCaretPosition = tb.SelectionStart;
            vm.FocusedSelectionLength = tb.SelectionLength;
        }
    }
}

private void OnCaretRestoreRequested(object? sender, int newCaretPosition)
{
    if (_currentFocusedTextBox is null) return;
    var tb = _currentFocusedTextBox;
    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
    {
        tb.Focus();
        tb.SelectionStart = Math.Clamp(newCaretPosition, 0, tb.Text.Length);
        tb.SelectionLength = 0;
    }));
}
```

The `DispatcherPriority.Loaded` deferral lets the bound `Lyrics` change propagate to `TextBox.Text` before we set `SelectionStart`, since the binding update is async-ish.

---

## 11. Testing strategy

Target: 31 unit tests + 6 manual smoke cases (r2 — added one VM test and one smoke case from specialist LOW items; was 30 + 5 in r1).

| File | Tests | Notes |
|---|---|---|
| `TagServiceTests.cs` | 5 | Unchanged from v1. |
| `TagServiceFilterTests.cs` | 7 | Unchanged from v1. |
| `SectionTests.cs` | 2 | **Simplified.** Only `Lyrics` PropertyChanged + default value. No chip commands. |
| `PreviewBuilderTests.cs` | 7 | **Simplified.** Empty sections list, single section, two sections, skip whitespace-only, CRLF newline, trim trailing newlines, lyrics with inline `[Tag]` tokens pass through. |
| `MainViewModelTests.cs` | 10 | **Rewritten + r2 add.** Add/remove/move section, move boundaries (CanExecute reports false at index 0 / last index), InsertTag with no focus, InsertTag with focus, InsertTag with selection range replacement, **r2: two consecutive InsertTag with different FocusedSection lands in correct sections**, subscription leak guard (mutate removed section's Lyrics → preview unaffected). |

Manual smoke cases (run on published exe):
1. Initial focus lands in first section's lyric textbox. Section's border shows the SteelBlue focused affordance.
2. Click in textbox at mid-text, click a tag → `[Bracket]` inserted at caret, caret lands after `]`, textbox stays focused.
3. Select a word, click a tag → bracket replaces the selection.
4. **Focus-loss + no-focus dim affordance (r2 — was just no-op):** click in lyric textbox, then click in the search textbox. Section's border returns to LightGray (focused affordance clears). Tag-picker buttons dim to 0.55 opacity. Hover a button: tooltip reads *"Click in a lyric textbox first, then click a tag."* Click a tag: nothing happens (no insertion into stale section).
5. Two sections, type lyrics in both, preview shows them separated by a blank line. Inline tags pass through verbatim.
6. **r2 — Rapid-fire tag clicks.** With a lyric textbox focused, click three different tags within one second. All three brackets should land consecutively at the moving caret position, in click order, with no Dispatcher ordering glitches.

Move-boundary smoke is folded into case 1's setup: after add, observe that first section's `▲` button greys out (CanExecute false at index 0). After adding a second section and selecting it, observe that section 2's `▼` button greys out (last index).

---

## 12. Migration from v1

Since v1 has no persistence, there is no runtime data migration. The migration is purely in the codebase:

| v1 surface | v1.1 disposition |
|---|---|
| `Section.Tags` collection | Deleted |
| `Section.IsArmed` | Deleted |
| `Section.RemoveTagCommand` etc. | Deleted |
| `MainViewModel.ShowArmHint` | Deleted |
| `MainViewModel.ArmedSectionCount` | Deleted |
| `MainViewModel.InsertTagCommand` (broadcast logic) | Rewritten (focus-based) |
| `ArmedToGlyphConverter.cs` | Deleted |
| `CountGreaterThanOneToBoolConverter.cs` | Deleted |
| `PreviewBuilder.Build` (chip-line emission) | Rewritten (lyrics-only) |
| Chip row XAML, arm toggle XAML, broadcast badge XAML, arm hint XAML | Deleted |
| `MainWindow.xaml.cs` `OnWindowLoaded` focus walker | Kept (unchanged) |
| `MainWindow.xaml.cs` `DeleteSectionButton_Click` | Kept (simplified — drop the `has tags` check) |
| Test files | 3 simplified, 1 rewritten, 1 unchanged |

Same `tags.json` content. Same `App.xaml.cs` startup flow.

---

## 13. Backlog impact

- **B-023 is implemented by this slice** and should close on v1.1 release.
- **B-006 was retired in v1** (section reorder shipped); v1.1 keeps section reorder with **CanExecute-based** boundary disabling (closes the v1 PASS-WITH-NOTES item).
- **B-001 (favorites), B-002 (theme), B-003 (drag-drop), B-004 (hotkeys), B-005 (persistence)** unchanged.
- **B-016 (permanent arm hint)** is moot — v1.1 has no arm state. Should be retired at v1.1 closeout.
- **B-021 (inline delete-section confirm)** still open; modal MessageBox carries over.
- **B-024 (r2 — new):** Syntax-highlight `[Tag]` tokens in lyric textbox. Suggested by specialist LOW finding. Requires `RichTextBox` or AvalonEdit; plain `TextBox` doesn't support it. Out of scope for v1.1; seeded to backlog.
- New backlog items potentially introduced by v1.1 (defer to specialist review):
  - Inline tag remove affordance (right-click → remove `[Tag]` token at caret position?) — currently user just deletes the text manually.
  - Tag-around-selection mode (Suno supports `[Tag]Lyric content[/Tag]` style for some metatags; v1.1 only inserts open brackets).

### Wiki page supersession strategy (r2 — resolves MEDIUM-1)

The existing `wiki/architecture/sunometatag-section-editor.md` is **renamed** to `wiki/architecture/sunometatag-inline-editor.md` at v1.1 closeout, with full claim-lifecycle supersession per AGENTS.md:

1. **New page:** `wiki/architecture/sunometatag-inline-editor.md`
   - `type: architecture`, `status: active`, `claim_state: active`.
   - `supersedes: "[[sunometatag-section-editor]]"` in frontmatter.
   - New content describing the inline-insertion + focus-tracking architecture.
2. **Old page:** `wiki/architecture/sunometatag-section-editor.md`
   - Kept on disk with its v1 chip-row content preserved.
   - `superseded_by: "[[sunometatag-inline-editor]]"` added to frontmatter.
   - `claim_state: superseded`.
   - `> [!warning] Superseded — see [[sunometatag-inline-editor]]` callout at top.
3. **Cross-reference updates:**
   - `wiki/features/sunometatag-app.md` `related:` field: replace `[[sunometatag-section-editor]]` with `[[sunometatag-inline-editor]]`. Also bump `last_confirmed: 2026-05-25` + add a v1.1 "What changed" section.
   - `wiki/decisions/sunometatag-sibling-repo.md` `related:` field: same swap.
4. **New risk page:** `wiki/risks/focus-flip-stale-insert.md` documenting *why* the `LostKeyboardFocus` handler defer-clears (and what would have happened if it didn't — the stale-insert surprise specialist HIGH-1 named). Helps future maintainers who might "simplify" the handler away.

## 14. Out of scope for v1.1

- Persistence across launches (B-005).
- Favorites / recents (B-001).
- Dark theme (B-002).
- Drag-drop (B-003).
- Hotkeys (B-004).
- Section type field (B-009).
- Tag auto-update from URL (B-017).
- Cross-section chip drag (B-019) — moot, no chips.
- Inline `[/Tag]` close-bracket pairing or "tag around selection" mode.
- Tag library expansion past ~115.
- Accessibility hardening past v1.

## 15. Open design questions — r2 resolutions

The r1 spec carried 4 open design questions; all four were specialist blockers (or fed into them). r2 resolves each:

1. **Focus tracking via `KeyboardFocus` events vs `Keyboard.FocusedElement` at click time.**
   - **Resolution (r2):** Stay with `GotKeyboardFocus`/`LostKeyboardFocus` events for VM-tracked state, with the load-bearing addition that `LostKeyboardFocus` now defer-clears `FocusedSection` (§5.5). The alternative (`Keyboard.FocusedElement` at click time) was considered by the specialist; the trade-off is that it bypasses VM property tracking, making the model less MVVM-pure. Sticking with the event-based approach keeps the VM as the source of truth and lets unit tests cover the contract directly.
   - **Status:** Closed.

2. **Caret restore via `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)`.**
   - **Resolution (r2):** Keep the deferral pattern. It's a known-working WPF empirical pattern even if not contractually documented. The LOW specialist concern about the ordering being non-contractual is addressed by smoke case 6 (rapid-fire tag clicks) which surfaces any timing breakage. Alternative considered (synchronous code-behind insertion bypassing the VM property dance) was rejected — keeps insertion in the VM for testability.
   - **Status:** Closed.

3. **Tag click while focus is in the preview TextBox (read-only).**
   - **Resolution (r2):** Subsumed by HIGH-1 resolution. When the user focuses the preview TextBox, the lyric textbox's `LostKeyboardFocus` fires; the new defer-clear logic clears `FocusedSection` to null (preview TextBox is not a `Section`-bound TextBox). Tag click is then a no-op + dim affordance is visible.
   - **Status:** Closed (resolved by HIGH-1 mechanics).

4. **Tag click while focus is in the search textbox.**
   - **Resolution (r2):** Subsumed by HIGH-1 resolution. Search TextBox is not a `Section`-bound TextBox. After defer-clear fires, `FocusedSection == null`, tag click is no-op, dim affordance visible. **The stale-insert path the r1 spec acknowledged as "may be surprising" is now closed.**
   - **Status:** Closed (resolved by HIGH-1 mechanics).

### New r2 open questions (none HIGH)

None at r2 that block approval. LOW items addressed inline; MEDIUM specialist items pinned in §13 (wiki supersession) and §5.2 (move-boundary `CanExecute`).

---

## 16. Glossary

- **Section:** one (toolbar + lyric textbox) unit in the middle column. Owns a `Lyrics` string.
- **Focused section:** the Section whose lyric textbox currently has keyboard focus. Tracked on the VM.
- **Inline insertion:** clicking a tag in the picker writes `[Bracket]` at the focused textbox's caret position.
- **Tag bracket:** the canonical Suno-prompt form of a tag, e.g., `[Verse]`, `[Guitar]`, `[Powerful]`. Stored in `TagDefinition.Bracket`.
- **Preview:** the concatenated string of all non-empty sections' `Lyrics`, separated by blank lines. Live-updates per keystroke.
