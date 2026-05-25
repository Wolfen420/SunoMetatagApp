# Suno Metatag Section Editor — Design

**Date:** 2026-05-25
**Status:** Approved with r2 revisions — UX details pinned 2026-05-25 in response to r3-cycle FRONTEND/UX advisory.
**Owner:** Planner (Claude)
**Target repo:** `j:\SunoMetatagApp\` (sibling to `j:\SunoSongSetup\`)
**Revision history:**
- 2026-05-25 r1 — Initial section-editor design, user-approved.
- 2026-05-25 r2 — Added broadcast count badge (§5.12), pinned section-reorder ▲/▼ in v1 (§5.3), pinned glyph + label arm toggle (§5.5), replaced DispatcherTimer-arm-hint with reset-on-state-change (§5.9), pinned initial-focus implementation strategy (§5.1), chip × → ✕ (§5.4). Triggered by `ai/ENGINE_REVIEW.md` r3 advisory (3 HIGH + 3 MEDIUM blockers).

---

## 0. Why a new design

The earlier "scratch" design treated the lyric area as a single text buffer and inserted tags at the caret. That mental model put the user in charge of structure. This design treats a Suno prompt as **a list of sections, each section being a chip-row of tags plus a lyric textbox**. The app owns structure; the user owns content. It also kills the caret-position complexity (the r1 review's biggest blocker class) entirely.

This document supersedes the scratch design. The implementation plan tied to it (`docs/plans/2026-05-25-suno-metatag-scratch.md`) is deprecated; the active plan is `docs/plans/2026-05-25-suno-metatag-section-editor.md`.

## 1. Overview

A single-window Windows desktop utility for assembling Suno prompts. Three resizable columns:

- **Left (~30%):** read-only preview of the assembled prompt + "Copy all" button.
- **Middle (~40%):** vertically scrollable stack of **sections**. Each section is `chip-row + lyric textbox`. An arm toggle controls whether tag clicks land in this section.
- **Right (~30%):** tag picker — search box, category dropdown, scrollable wrap-grid of tag buttons (~115 starter tags).

No file open/save. No persistence between launches. Scratch space, but structured.

```
┌──────────────┬─────────────────────────┬──────────────┐
│ [Copy all]   │ ┌─────────────────────┐ │ [Search…]    │
│              │ │ ◉ Section   [×]     │ │ [Category ▾] │
│ [Guitar]     │ │ ┌─────────────────┐ │ │              │
│ [Powerful]   │ │ │[Guitar×][Power×]│ │ │ [Verse]      │
│ Song here…   │ │ └─────────────────┘ │ │ [Chorus]     │
│ It's lyrics… │ │ ┌─────────────────┐ │ │ [Whispered]  │
│              │ │ │Song here looks  │ │ │ [Powerful]   │
│              │ │ │like this        │ │ │ …            │
│              │ │ └─────────────────┘ │ │              │
│              │ ├─────────────────────┤ │              │
│              │ │ ○ Section   [×]     │ │              │
│              │ │ …                   │ │              │
│              │ └─────────────────────┘ │              │
│              │      [+ Add section]    │              │
└──────────────┴─────────────────────────┴──────────────┘
```

## 2. Goals & non-goals

### v1 goals

- Three-pane layout: preview / section stack / tag picker, with GridSplitters between.
- Sections are first-class: add, delete, arm/disarm, hold chip-row + lyric textbox.
- Tag click → tag appended to chip-row of every armed section.
- Tag chips can be reordered (◀/▶ buttons on each chip, visible on hover) and removed (× on each chip).
- Preview pane shows the assembled prompt live (recomputed on every change) and has a "Copy all" button that puts the assembled text on the clipboard.
- Tag picker UX from the prior r2 spec carries over: search-as-you-type, category dropdown, ~115 starter tags from bundled `tags.json`, error banner if load fails.
- Self-contained `.exe`, no installer.

### Non-goals (v1)

- Saving / loading prompt files.
- Persisting prompts across launches.
- Drag-and-drop reordering of either chips or sections (◀/▶ chips only; sections in add-order).
- Section types (Verse/Chorus/Bridge as a structured field). Sections are untyped; the user uses tags like `[Verse]` to mark structure.
- Multi-document / multi-window.
- Dark theme.
- Hotkeys.
- Favorites / recents.

All v2+ items captured in §11 backlog.

## 3. Tech stack

Same as the previous design:

- WPF on .NET 8.
- `CommunityToolkit.Mvvm` (`ObservableObject`, `RelayCommand`, source-gen properties).
- `System.Text.Json` (BCL) for `tags.json` loading.
- xUnit for tests (no FluentAssertions).
- `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` for distribution.

## 4. Repo & project structure

```
j:\SunoMetatagApp\
  SunoMetatagApp.sln
  src\
    SunoMetatagApp\
      SunoMetatagApp.csproj                          (WPF, .NET 8)
      App.xaml / App.xaml.cs
      MainWindow.xaml / .xaml.cs                     (thin code-behind: Copy command only)
      ViewModels\
        MainViewModel.cs
        TagViewModel.cs
      Models\
        TagDefinition.cs
        Section.cs                                   (new — observable, holds Tags + Lyrics + IsArmed)
      Services\
        TagService.cs                                (LoadAll, DistinctCategories, Filter)
        PreviewBuilder.cs                            (new — pure Build(sections, newline))
      NullToCollapsedConverter.cs                    (error banner visibility)
      StringIsNotEmptyConverter.cs                   (tooltip suppression on tag-picker buttons)
      ArmedToGlyphConverter.cs                       (new — bool → ◉/○)
      Resources\
        tags.json                                    (CopyToOutput=PreserveNewest)
  tests\
    SunoMetatagApp.Tests\                            (xUnit)
      TagServiceTests.cs                             (parsing + DistinctCategories)
      TagServiceFilterTests.cs                       (filter semantics)
      SectionTests.cs                                (new — section observable behavior)
      PreviewBuilderTests.cs                         (new — preview rendering)
      MainViewModelTests.cs                          (rewritten for sections)
  docs\
    specs\
      2026-05-25-suno-metatag-scratch-design.md      (DEPRECATED, retained for history)
      2026-05-25-suno-metatag-section-editor-design.md (this file)
    plans\
      2026-05-25-suno-metatag-scratch.md             (DEPRECATED, retained for history)
      2026-05-25-suno-metatag-section-editor.md      (active plan)
    BACKLOG.md
  README.md
  .gitignore
```

**Removed compared to the scratch design:** `Services/InsertionRules.cs`, `tests/SunoMetatagApp.Tests/InsertionRulesTests.cs`. No caret-position math anywhere.

## 5. UI behavior — detailed

### 5.1 Window

- Title: "Suno Metatag Editor"
- Default size: 1300×750, resizable, minimum 900×500.
- **Initial focus implementation (r2 — pinned).** `MainWindow.xaml.cs` subscribes to its own `Loaded` event. The handler uses `Dispatcher.BeginInvoke(...DispatcherPriority.Loaded)` to defer until layout is complete, then walks the visual tree from the named `SectionsHost` `ItemsControl` to find the first section's lyric `TextBox` (the only `TextBox` descendant of the first `ContentPresenter` whose binding path is `Lyrics`) and calls `.Focus()` on it. If no section exists (load-error ctor), the handler is a no-op.

### 5.2 Preview pane (left)

- A read-only multi-line `TextBox` (not a `TextBlock` — needed for selection/copy).
- Two-way `OneWay` bound to `MainViewModel.PreviewText`.
- "Copy all" button sticky at the top.
- Font: `Consolas 12pt`, monospace, predictable line wrapping.
- `VerticalScrollBarVisibility="Auto"`.

### 5.3 Section stack (middle)

- A `ScrollViewer` containing a named `ItemsControl` (`x:Name="SectionsHost"`) bound to `MainViewModel.Sections`.
- "+ Add section" button is sticky at the bottom of the column (never scrolls off).
- Each section renders as a bordered card with three rows:
  1. **Toolbar row:** arm toggle (text + glyph per §5.5) on the left; the literal text "Section" next to it; `▲` / `▼` move buttons (r2 — pinned in v1, see below) and `×` delete button on the right.
  2. **Chip row:** a `WrapPanel` of chips inside a bordered light-background area, min-height ~32px so it's obvious even when empty.
  3. **Lyric textbox:** `AcceptsReturn=True`, `TextWrapping=Wrap`, `MinLines=6`, `MaxLines=12`, `VerticalScrollBarVisibility=Auto`, font `Consolas 13pt`.

**Section reorder (r2 — pinned in v1).** Each section's toolbar carries `▲` (move up) and `▼` (move down) buttons immediately to the left of the `×` delete button. They are bound to `MainViewModel.MoveSectionUpCommand` / `MoveSectionDownCommand` with the section as parameter; both commands use `ObservableCollection<Section>.Move`. `▲` is disabled on the topmost section; `▼` is disabled on the bottommost. Rationale for v1 inclusion (not backlog): without reorder, the only way to move a section up the stack is to delete-and-re-add downstream sections, which destroys their chips and lyrics — that's a destructive workaround for an everyday songwriting move. Drag-and-drop section reorder remains backlog (B-003).

### 5.4 Chips

Each chip in a section's chip-row is a small bordered control with:

- The bracket text (e.g. `[Guitar]`).
- A `◀` button (move-left in the chip row), tooltip "Move left".
- A `▶` button (move-right), tooltip "Move right".
- A `✕` (U+2715, r2 — replaces the `×` U+00D7 multiplication-sign idiom) button (remove), tooltip "Remove".

The three small buttons are **always visible** in v1 (the "hover only" idea would be a visual-polish backlog item — for v1 they're always-on for discoverability). Each button has a `ToolTip` so the glyph alone is not load-bearing.

Disabled state:
- `◀` disabled when the chip is the first in its row.
- `▶` disabled when the chip is the last.
- `✕` always enabled.

### 5.5 Arm toggle (r2 — glyph + label)

- A `ToggleButton` bound to `Section.IsArmed`.
- Content is `"◉ Armed"` when armed (true) or `"○ Disarmed"` when disarmed (false), via `ArmedToGlyphConverter` (the converter returns the combined string). The text label removes the dependency on glyph rendering and adds a verbal anchor for screen readers and low-vision users.
- Tooltip: "Armed — tag clicks add to this section" / "Disarmed — tag clicks skip this section".
- New sections start armed.

### 5.6 Tag picker (right)

Unchanged from the r2 scratch design:

- Search `TextBox` with "Search tags…" placeholder, bound to `SearchText`.
- Category `ComboBox` bound to `SelectedCategory` (default `"All"`).
- Scrollable wrap-grid of tag buttons inside a `ScrollViewer` with `KeyboardNavigation.TabNavigation="Once"` + `DirectionalNavigation="Contained"`.
- Each tag button: content = `Bracket`; tooltip suppressed when `Description` is null/empty (via `StringIsNotEmptyConverter` on `ToolTipService.IsEnabled`).
- Error banner above the search box when `LoadError` is non-null.
- Empty-results `TextBlock`: "No tags match" when `FilteredTags.Count == 0`.

### 5.7 Initial state

- One armed section pre-created on launch (empty tags, empty lyrics).
- Tag picker shows all ~115 tags.
- Preview pane is empty (no tags + no lyrics in the one section → filtered out → empty preview).
- Focus is in the first section's lyric textbox.

### 5.8 Empty-section policy

- The user can never remove the last section. If `Sections.Count == 1` and they click `×` on it, the click is a no-op (button is disabled when only one section exists).
- Adding a section always appends to the bottom of the stack. New section's text box gets keyboard focus.

### 5.9 Tag-click flow (r2 — arm-hint clears on state change, not on timer)

1. Find all `Section`s where `IsArmed == true`.
2. If empty: set `ShowArmHint = true`. No tags are added. **No timer.** The hint persists until *either* (a) the next `InsertTag` call has at least one armed section, in which case `ShowArmHint = false` and that tag is appended, OR (b) any `Section.IsArmed` transitions to `true`, in which case `MainViewModel` clears `ShowArmHint = false` immediately. This means the hint disappears the moment the user fixes the problem.
3. Otherwise: set `ShowArmHint = false` (idempotent if already false); for each armed section, append the clicked `TagDefinition` to `section.Tags`. Duplicates allowed.

### 5.10 Section delete confirmation

When the user clicks `×` on a section:

- If the section is empty (no tags, no lyrics): delete immediately, no confirmation.
- If the section has any content: show a small modal `MessageBox` — "Delete this section? Its tags and lyrics will be lost." with OK / Cancel. OK removes; Cancel is a no-op.

### 5.11 Splitters

Two `GridSplitter`s between the three columns, both `Width="6"` with `Cursor="SizeWE"`.

### 5.12 Broadcast scope badge (r2 — pinned)

Because new sections start armed (§5.7) and tag clicks broadcast to *every* armed section (§5.9), a user with N armed sections may inadvertently apply a tag to all N at once. To keep the broadcast scope visible, the right pane shows a small status line **immediately above the tag-button grid** (between the category dropdown and the buttons):

> *"Will apply to N section(s)"*

Where N is `MainViewModel.ArmedSectionCount` (computed from `Sections.Count(s => s.IsArmed)` and recomputed whenever any `Section.IsArmed` changes or when `Sections` changes).

- When N = 0, the badge is hidden (the arm-hint banner already covers that state).
- When N ≥ 1, the badge is visible with subtle styling (foreground `#666`, small font), updating live.

Rationale: the broadcast model is the user's stated intent ("if we want to add to multiple boxes"). The badge surfaces the broadcast scope without changing the click semantics, addressing the r3 advisory's friction concern without contradicting the design intent.

## 6. Tag data model

Unchanged from the scratch design:

```json
[
  { "category": "Structure", "label": "Verse",   "bracket": "[Verse]" },
  { "category": "Vocal",     "label": "Whisper", "bracket": "[Whispered]", "description": "Soft, intimate delivery." }
]
```

```csharp
public sealed record TagDefinition(
    string Category,
    string Label,
    string Bracket,
    string? Description = null);
```

`TagService` carries over verbatim: `LoadAll(path)`, `DistinctCategories(tags)`, `Filter(tags, search, category)`.

`tags.json` ships with the same ~115 starter entries and descriptions seeded for ambiguous tags (from the r2 plan).

## 7. Section model

```csharp
public sealed partial class Section : ObservableObject
{
    [ObservableProperty] private string _lyrics = "";
    [ObservableProperty] private bool _isArmed = true;

    public ObservableCollection<TagDefinition> Tags { get; } = new();

    [RelayCommand]
    private void RemoveTag(TagDefinition? tag)
    {
        if (tag != null) Tags.Remove(tag);
    }

    [RelayCommand]
    private void MoveTagLeft(TagDefinition? tag)
    {
        if (tag == null) return;
        var i = Tags.IndexOf(tag);
        if (i > 0) Tags.Move(i, i - 1);
    }

    [RelayCommand]
    private void MoveTagRight(TagDefinition? tag)
    {
        if (tag == null) return;
        var i = Tags.IndexOf(tag);
        if (i >= 0 && i < Tags.Count - 1) Tags.Move(i, i + 1);
    }
}
```

- `Tags` is `ObservableCollection<TagDefinition>` so XAML's `ItemsControl` reflects add/remove/move live.
- Chip-row commands live on the `Section` itself (not on `MainViewModel`) because each chip's context is a tag inside a known section. This makes XAML binding straightforward (`AncestorType=ItemsControl` to reach the section).

## 8. PreviewBuilder

Pure function. Lives in `Services/PreviewBuilder.cs`. Signature:

```csharp
public static class PreviewBuilder
{
    public static string Build(IReadOnlyList<Section> sections, string newline);
}
```

### Rendering rule

For each section in order:

1. Skip if `Tags.Count == 0` **and** `string.IsNullOrEmpty(Lyrics)`. (Empty sections don't appear in the preview.)
2. Append each tag's `Bracket` on its own line (with a trailing `newline` after each).
3. Append `Lyrics` verbatim (no transformation, including any newlines the user typed).

Between rendered sections, insert exactly one blank line: trim any trailing `\r` / `\n` from the accumulator, then append `newline + newline`.

After the final section, trim trailing `\r` / `\n` from the accumulator so the preview has no trailing blank line.

### Examples

Single section, tags=`[Guitar],[Powerful]`, lyrics=`"Song here\nIt's lyrics"`:

```
[Guitar]
[Powerful]
Song here
It's lyrics
```

Three sections: section 2 empty (no tags, no lyrics):

```
[Guitar]
[Powerful]
v1 lyrics

[Whispered]
v3 lyrics
```

(Section 2 is filtered out; sections 1 and 3 are separated by one blank line.)

Section with tags but no lyrics:

```
[Outro]
```

(Just the bracket, no trailing blank line if it's the only/last section.)

### Testability

`PreviewBuilder.Build` is a pure function with no WPF or VM dependencies. Unit tests pass `"\n"` as `newline` for portable assertions. Test coverage (10 cases) listed in §10.

## 9. MainViewModel

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<TagDefinition> _allTags;

    public ObservableCollection<Section> Sections { get; } = new();
    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _selectedCategory = "All";
    [ObservableProperty] private IReadOnlyList<TagViewModel> _filteredTags = Array.Empty<TagViewModel>();
    [ObservableProperty] private string _previewText = "";
    [ObservableProperty] private string? _loadError;
    [ObservableProperty] private bool _showArmHint;
    [ObservableProperty] private int _armedSectionCount;  // r2 — drives broadcast badge

    // Commands (RelayCommand source-gen):
    //   AddSectionCommand          → AddSection()
    //   RemoveSectionCommand       → RemoveSection(Section)
    //   MoveSectionUpCommand       → MoveSectionUp(Section)        (r2)
    //   MoveSectionDownCommand     → MoveSectionDown(Section)      (r2)
    //   InsertTagCommand           → InsertTag(TagViewModel)
    //   CopyPreviewCommand         → CopyPreview() (raises CopyRequested event; View handles clipboard)
}
```

### Construction

```csharp
public MainViewModel(IReadOnlyList<TagDefinition> tags)
{
    _allTags = tags;
    Categories = BuildCategories(tags);
    SelectedCategory = "All";
    FilteredTags = ComputeFiltered();
    Sections.CollectionChanged += OnSectionsChanged;
    AddSection(); // start with one armed section
}

// Degraded ctor when tags.json failed to load
public MainViewModel(string loadError) { ... }
```

### Preview recompute wiring

When a `Section` is added to `Sections`:

- Subscribe to `section.PropertyChanged` (recompute preview when `Lyrics` changes; also recompute `ArmedSectionCount` and reset `ShowArmHint` when `IsArmed` changes).
- Subscribe to `section.Tags.CollectionChanged` (recompute when tags add/remove/move).

When a `Section` is removed: **unsubscribe.** Verified by `MainViewModelTests.AfterRemoveSection_MutatingRemovedSectionLyrics_DoesNotChangePreviewText`.

On the `Sections` collection change itself: recompute preview AND recompute `ArmedSectionCount`.

`RecomputePreview()` calls `PreviewBuilder.Build(Sections.ToList(), Environment.NewLine)` and assigns to `PreviewText`.

`RecomputeArmedCount()` sets `ArmedSectionCount = Sections.Count(s => s.IsArmed)`.

### Tag insertion (r2 — no timer; reset on state change)

```csharp
[RelayCommand]
private void InsertTag(TagViewModel? tag)
{
    if (tag is null) return;
    var armed = Sections.Where(s => s.IsArmed).ToList();
    if (armed.Count == 0)
    {
        ShowArmHint = true;
        return;
    }
    ShowArmHint = false;
    foreach (var s in armed)
        s.Tags.Add(tag.Definition);
}
```

The hint also clears when any `Section.IsArmed` transitions to `true` (handled in the section-property-changed handler):

```csharp
private void OnSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(Section.Lyrics))
        RecomputePreview();
    else if (e.PropertyName == nameof(Section.IsArmed))
    {
        RecomputeArmedCount();
        if (sender is Section s && s.IsArmed) ShowArmHint = false;
    }
}
```

No `DispatcherTimer`, no `FlashArmHint` helper.

### Section ops (r2 — adds MoveSectionUp/Down)

```csharp
[RelayCommand]
private void AddSection()
{
    Sections.Add(new Section());
}

[RelayCommand]
private void RemoveSection(Section? section)
{
    if (section is null) return;
    if (Sections.Count <= 1) return;     // never delete the last one
    Sections.Remove(section);
}

[RelayCommand]
private void MoveSectionUp(Section? section)
{
    if (section is null) return;
    var i = Sections.IndexOf(section);
    if (i > 0) Sections.Move(i, i - 1);
}

[RelayCommand]
private void MoveSectionDown(Section? section)
{
    if (section is null) return;
    var i = Sections.IndexOf(section);
    if (i >= 0 && i < Sections.Count - 1) Sections.Move(i, i + 1);
}
```

The XAML binds:
- The `×` button's `IsEnabled` to a converter on `Sections.Count` so it disables when there's only one section.
- The `▲` button's `IsEnabled` to a multi-binding that returns true when the section is not at index 0.
- The `▼` button's `IsEnabled` similarly true when the section is not at the last index.

The delete-confirmation modal is a View concern, not VM logic — `MainWindow.xaml.cs` intercepts the command via a hook or wraps the button click. (Alternative: VM raises a `DeleteConfirmationRequested` event the View answers; simpler to put a `MessageBox.Show` in the click handler.) **Chosen approach:** View intercepts in code-behind for the modal, then calls the VM command on confirmation.

### Copy command

```csharp
public event EventHandler? CopyRequested;

[RelayCommand]
private void CopyPreview() => CopyRequested?.Invoke(this, EventArgs.Empty);
```

The View subscribes and calls `Clipboard.SetText(vm.PreviewText)`. Keeps `Clipboard` (a WPF/UI class) out of the VM and out of unit-test reach.

## 10. Testing strategy

xUnit, no FluentAssertions, plain `Assert.*`. Three test files inherited from v1 stay; three change/add.

### `TagServiceTests.cs` (unchanged from r2)

`LoadAll_ParsesValidFile`, `LoadAll_ThrowsWithClearMessage_OnMalformedJson`, `LoadAll_ThrowsWithClearMessage_OnMissingRequiredField`, `LoadAll_ThrowsWithClearMessage_WhenFileMissing`, `DistinctCategories_ReturnsSortedDistinct`.

### `TagServiceFilterTests.cs` (unchanged from r2)

`Filter_AllCategory_EmptySearch_ReturnsEverything`, `Filter_NullCategory_EmptySearch_ReturnsEverything`, `Filter_SpecificCategory_ReturnsOnlyThatCategory`, `Filter_Search_MatchesLabel_CaseInsensitive`, `Filter_Search_MatchesBracket_CaseInsensitive`, `Filter_CategoryAndSearch_AreAndCombined`, `Filter_EmptyResults_DoesNotThrow`.

### `SectionTests.cs` (new)

- `RemoveTag_RemovesGivenTag`
- `RemoveTag_NullTag_DoesNothing`
- `MoveTagLeft_AtFirstPosition_DoesNothing`
- `MoveTagLeft_SwapsWithPrevious`
- `MoveTagRight_AtLastPosition_DoesNothing`
- `MoveTagRight_SwapsWithNext`
- `Tags_CollectionChangedFires_OnAdd`

### `PreviewBuilderTests.cs` (new)

- `Build_NoSections_ReturnsEmpty`
- `Build_SingleEmptySection_ReturnsEmpty`
- `Build_SingleSection_TagsAndLyrics_RendersTagsThenLyrics`
- `Build_SingleSection_TagsOnly_RendersTagsNoTrailingBlank`
- `Build_SingleSection_LyricsOnly_RendersLyricsAsIs`
- `Build_TwoSections_SeparatedByOneBlankLine`
- `Build_MiddleSectionEmpty_SkippedFromOutput`
- `Build_SectionLyricsEndingInNewline_NormalizedAtBoundary`
- `Build_PreservesTagOrder_WithinSection`
- `Build_HandlesCrLfNewlineParameter`

### `MainViewModelTests.cs` (rewritten, r2 — 22 cases)

- `Ctor_PopulatesCategoriesWithAllPrefix`
- `Ctor_StartsWithOneArmedEmptySection`
- `Ctor_PreviewText_StartsEmpty`
- `Ctor_ArmedSectionCount_IsOne`
- `AddSection_AppendsArmedSection`
- `AddSection_IncrementsArmedSectionCount`
- `RemoveSection_OnLastSection_IsNoOp`
- `RemoveSection_OnMiddleSection_Removes`
- `RemoveSection_DecrementsArmedSectionCount_WhenRemovedWasArmed`
- `MoveSectionUp_AtTop_IsNoOp` *(r2)*
- `MoveSectionUp_SwapsWithPrevious` *(r2)*
- `MoveSectionDown_AtBottom_IsNoOp` *(r2)*
- `MoveSectionDown_SwapsWithNext` *(r2)*
- `MoveSection_RecomputesPreviewText` *(r2)*
- `InsertTag_WithOneArmedSection_AppendsToThatSection`
- `InsertTag_WithMultipleArmedSections_AppendsToAll`
- `InsertTag_WithNoArmedSections_DoesNotMutate_AndSetsShowArmHint`
- `InsertTag_WithArmedSection_ClearsShowArmHint` *(r2)*
- `IsArmedTransitionToTrue_ClearsShowArmHint` *(r2 — hint reset on state change)*
- `IsArmedChange_UpdatesArmedSectionCount` *(r2)*
- `InsertTag_RecomputesPreviewText`
- `LyricsChange_RecomputesPreviewText`
- `RemoveTagOnSection_RecomputesPreviewText`
- `ChangingSearchText_RecomputesFilteredTags`
- `ChangingSelectedCategory_RecomputesFilteredTags`
- `LoadErrorCtor_HasEmptyCategoriesAndSections`
- `AfterRemoveSection_MutatingRemovedSectionLyrics_DoesNotChangePreviewText` *(r2 — subscription-leak guard)*
- `AfterRemoveSection_AddingTagToRemovedSection_DoesNotChangePreviewText` *(r2 — subscription-leak guard, tags side)*

Total test count moves from 15 → **22** in this file, total project tests from 45 → **52**.

### Explicitly not tested

XAML layout, clipboard copy (View concern), the `DispatcherTimer` for the arm-hint (timing concern), modal confirmation dialog.

## 11. Backlog (v2+ items)

Seeded into `j:\SunoMetatagApp\docs\BACKLOG.md`. Priority order, rough.

1. **Favorites / recently-used tags** (B-001).
2. **Dark theme** (B-002).
3. **Drag-and-drop reorder** (B-003) — both chips and sections (full drag-drop). v1 ships ▲/▼ buttons for section reorder per §5.3; drag is the v2 polish.
4. **Hotkeys for arm/disarm and tag insert** (B-004).
5. **Persist prompt across launches** (B-005).
6. **In-app reload of `tags.json`** (B-007). *(B-006 retired — section reorder is now in v1.)*
7. **Tag aliases / synonyms** (B-008).
8. **Section type field (Verse/Chorus/Bridge as structured)** (B-009).
9. **Per-section "add tag" inline shortcut** (B-010).
10. **Virtualize the tag panel** (B-011) — trigger-based.
11. **Chip-row hover affordances** (B-012) — show ◀/▶/✕ only on hover.
12. **Tag button visual treatment** (B-013) — flat style.
13. **Screen-reader naming** (B-014).
14. **Splitter positions and column widths persisted** (B-015).
15. **Persistent dim arm-hint** (B-016) — alternative to the auto-clearing hint.
16. **Auto-update `tags.json`** (B-017).
17. **Retry `musci.io` as a seed source** (B-018).
18. **Tag chip drag-to-different-section** (B-019).
19. **Debounce preview recompute** (B-020 — r2) — 50ms `DispatcherTimer` debounce on `RecomputePreview` for very long lyrics / rapid typing.
20. **Inline delete-section confirm** (B-021 — r2) — replace the modal `MessageBox` with an inline "× → Delete?" two-click confirm.
21. **Preview pane cursor styling** (B-022 — r2) — `Cursor="Arrow"` on the read-only preview TextBox unless the user is actively selecting, to avoid the I-beam misleading users into thinking it's editable.

## 12. Deferred / open (r2 — most items pinned)

- ~~**Initial focus mechanics**~~ — **pinned in §5.1.** `MainWindow.xaml.cs` `Loaded` handler walks the visual tree from the named `SectionsHost` `ItemsControl` to the first section's lyric `TextBox` and calls `.Focus()`.
- ~~**Arm-hint visual / timing**~~ — **pinned in §5.9.** No timer; hint clears on the next successful tag insertion or when any section transitions to armed.
- **`MessageBox` vs inline confirm for delete:** v1 uses `MessageBox.Show`. Inline confirm is BACKLOG B-021. (Specialist suggested this as a LOW item; deferred is acceptable.)
- **Arm-hint placement:** spec promises *some* user-visible affordance when no sections are armed. Implementation places it at the top of the right pane above the search box (consistent with the error banner placement).

## 13. Workflow note

This design was produced via the `superpowers:brainstorming` skill, validated by the user, and supersedes the earlier "scratch" design after a Lead Reviewer `NEEDS_REVISION` cycle that exposed caret-editor model limitations. The implementation plan tied to this design is at `docs/plans/2026-05-25-suno-metatag-section-editor.md`. The plan packet for the SunoSongSetup multi-agent workflow is `ai/PLAN.md` r3.
