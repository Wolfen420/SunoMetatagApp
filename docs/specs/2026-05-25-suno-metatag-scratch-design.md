> [!warning] **DEPRECATED — superseded 2026-05-25.**
> This "scratch" design (single text buffer + insert-at-caret) was retired in favor of the section-editor model after a Lead Reviewer `NEEDS_REVISION` cycle. The active spec is now [`2026-05-25-suno-metatag-section-editor-design.md`](2026-05-25-suno-metatag-section-editor-design.md). The matching implementation plan is [`../plans/2026-05-25-suno-metatag-section-editor.md`](../plans/2026-05-25-suno-metatag-section-editor.md). This file is retained for history.

# Suno Metatag Scratch — Design (deprecated)

**Date:** 2026-05-25
**Status:** DEPRECATED. Approved with revisions on 2026-05-25 (r2), then superseded the same day by the section-editor design.
**Owner:** Planner (Claude)
**Target repo:** `j:\SunoMetatagApp\` (new, sibling to `j:\SunoSongSetup\`)
**Revision history:**
- 2026-05-25 r1 — Initial design, user-approved.
- 2026-05-25 r2 — Added §5.1 (UX behavior decisions: focus, tab nav, empty state, splitter, button style, virtualization), §8.1 (post-insertion caret advancement rule), §6.2 (tooltip null-handling). Triggered by `ai/ENGINE_REVIEW.md` HIGH/MEDIUM findings.

---

## 1. Overview

A single-window Windows desktop utility for assembling Suno AI prompt lyrics. The user pastes (or types) lyric text into a large editor, then clicks tag buttons in a side panel to insert Suno metatags (e.g., `[Verse]`, `[Chorus]`, `[Whispered]`) at the current cursor position. The app is a **temporary scratch space**: no file open, no save, no persistence between launches. Everything lives in the text box for the duration of the session.

The point is to make adding metatags faster and less error-prone than typing them by hand.

## 2. Goals & non-goals

### v1 goals

- Single window, opens straight to a usable scratch editor.
- Right-side panel exposes 500+ Suno metatags, organized by category, searchable.
- Click a tag → it inserts at the cursor on its own line; cursor lands on the next line ready for lyrics.
- Tag list ships as a bundled, hand-editable `tags.json`.
- Self-contained `.exe`; no installer.

### Non-goals (v1)

- Saving or loading lyric files.
- Persisting lyric text across launches.
- Favorites / recently used.
- Dark theme.
- Hotkeys for top-N tags.
- Selection-wrap insertion behavior.
- Auto-update of `tags.json` from web sources.
- Multi-window / multi-document.

These are captured as backlog items (see §11) and are intentionally excluded from v1.

## 3. Tech stack

- **Framework:** WPF on .NET 8.
- **MVVM:** `CommunityToolkit.Mvvm` (`ObservableObject`, `RelayCommand`, source-gen properties). No DI container.
- **JSON:** built-in `System.Text.Json`.
- **Tests:** xUnit + FluentAssertions in a separate `tests/` project.
- **Packaging:** `dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true` produces a single `SunoMetatagApp.exe` plus `tags.json` next to it.

## 4. Repo & project structure

```
j:\SunoMetatagApp\
  SunoMetatagApp.sln
  src\
    SunoMetatagApp\
      SunoMetatagApp.csproj           (WPF, .NET 8)
      App.xaml / App.xaml.cs
      MainWindow.xaml / .xaml.cs      (thin code-behind; caret insertion only)
      ViewModels\
        MainViewModel.cs
        TagViewModel.cs
      Services\
        TagService.cs
      Models\
        TagDefinition.cs
      Resources\
        tags.json                     (CopyToOutput=PreserveNewest)
  tests\
    SunoMetatagApp.Tests\             (xUnit)
  docs\
    specs\
      2026-05-25-suno-metatag-scratch-design.md   (this file)
    BACKLOG.md                        (seeded from §11)
  README.md
  .gitignore
```

## 5. Window & UI layout

- **Title:** "Suno Metatag Scratch"
- **Default size:** 1100×700, resizable, minimum 700×450.
- **Root layout:** two-column `Grid` with a 4px `GridSplitter`. Left ≈70% (min flex), right ≈30% (min 280px).

### Left pane — editor

- Plain WPF `TextBox` (not `RichTextBox` — no styling needed, simpler caret model).
- `AcceptsReturn=True`, `AcceptsTab=True`, `TextWrapping=Wrap`, `VerticalScrollBarVisibility=Auto`.
- Font: `Consolas 13pt` (monospace, predictable layout for lyric structure).
- Two-way bound to `MainViewModel.LyricText`.

### Right pane — tag picker (top to bottom)

1. **Search `TextBox`** — placeholder "Search tags…", two-way bound to `MainViewModel.SearchText`. Filter is case-insensitive substring over `Label` and `Bracket`.
2. **Category `ComboBox`** — items: `"All"` plus distinct categories from `tags.json`. Bound to `MainViewModel.SelectedCategory`. Default `"All"`.
3. **Scrollable `ItemsControl`** — `ItemsPanel` = `WrapPanel`, wrapped in a `ScrollViewer`. `ItemsSource` = `MainViewModel.FilteredTags`. Each item renders as a `Button`:
   - `Content` = `Bracket` (e.g. `[Verse]`).
   - `ToolTip` = `Description` (if present).
   - `Command` = `MainViewModel.InsertTagCommand`; `CommandParameter` = the `TagViewModel`.

### What is **not** in v1

- No menu bar, no toolbar, no status bar, no settings dialog.

## 5.1 UX behavior decisions (r2)

The following behaviors are **part of v1**, locked here so the implementation plan can encode them exactly. Each was flagged HIGH or MEDIUM in the r1 FRONTEND/UX advisory.

### Initial focus

- Window root sets `FocusManager.FocusedElement="{Binding ElementName=LyricEditor}"`.
- This places the keyboard caret in the editor immediately after the window opens. Matches the user's stated workflow ("paste lyrics, then click tags").
- No `Focus()` call in code-behind is needed.

### Keyboard tab navigation in the tag panel

- The `ScrollViewer` wrapping the tag-button `ItemsControl` sets:
  - `KeyboardNavigation.TabNavigation="Once"`
  - `KeyboardNavigation.DirectionalNavigation="Contained"`
- Behavior: pressing `Tab` from the search box treats the entire button grid as **one** tab stop. Inside the panel, arrow keys navigate between buttons. Pressing `Tab` again leaves the panel as a unit and continues to the next focusable control.
- Rationale: prevents the 115+ (eventually 500+) tag buttons from forming a giant tab-trap. Keeps keyboard access without making `Tab` unusable.

### Empty-filter placeholder

- When `FilteredTags.Count == 0`, a centered `TextBlock` with text "No tags match" appears inside the tag-grid area (replacing the empty space).
- Implemented via a `DataTrigger` on `FilteredTags.Count` value `0` in the placeholder's `Style`.
- Foreground `#666` (acceptable contrast on white for v1; flagged in BACKLOG for accessibility hardening).

### GridSplitter

- `Width="6"` (up from initial 4px draft — 4px is below comfortable hit target).
- `Cursor="SizeWE"` for explicit affordance.

### Tag-button visual treatment

- v1 ships with default WPF button chrome. **Accepted as a known v1 limitation.**
- A polished visual treatment is queued as BACKLOG item `B-013 — Tag button visual treatment`.

### Virtualization at 500+ scale

- v1's tag panel is **not** virtualized. `WrapPanel` does not virtualize, and `ItemsControl` is non-virtualizing by default.
- This is acceptable at the v1 seed of ~115 tags. At the user's hand-extended 500+ target, scroll/keystroke perf may degrade.
- **Trigger condition** for backlog promotion: BACKLOG item `B-011 — Virtualize tag panel` ships when the user reports perceptible scroll stutter, search-keystroke lag, or when the tag count exceeds 300. Until then v1 stays simple.

## 6. Tag data model

### `tags.json` shape

Flat array of tag objects (category is a property, not a nesting level):

```json
[
  {
    "category": "Structure",
    "label": "Verse",
    "bracket": "[Verse]",
    "description": "Standard verse section."
  },
  {
    "category": "Vocal",
    "label": "Whispered",
    "bracket": "[Whispered]",
    "description": "Soft, intimate delivery."
  },
  {
    "category": "Effect",
    "label": "Reverb: Hall",
    "bracket": "[Effect: Reverb: Hall]"
  }
]
```

**Field rules**

| Field         | Type   | Required | Notes                                                    |
|---------------|--------|----------|----------------------------------------------------------|
| `category`    | string | yes      | Drives the dropdown. Easy to add new categories.         |
| `label`       | string | yes      | Short human label used for search matching.              |
| `bracket`     | string | yes      | **Exact text inserted**, brackets included.              |
| `description` | string | no       | Tooltip text only.                                       |

### C# model

```csharp
public sealed record TagDefinition(
    string Category,
    string Label,
    string Bracket,
    string? Description = null);
```

### Starter categories

`Structure`, `Vocal`, `Instrument`, `Mood`, `Effect`, `Production`, `Genre`. Implementation may add more from the seed sources.

### Why flat, not nested

- Search filters cleanly across all tags with one LINQ pass.
- No nesting to keep balanced when hand-editing.
- New metadata fields (e.g., `aliases`, `version`) can be added without restructuring.

### Seeding

Implementation work (not design): combine and dedupe tags from `hookgenius.app/learn/suno-metatags-complete-list/` (~64) and `openmusicprompt.com/blog/suno-ai-metatags-guide` (~127), tagging each with one of the starter categories. Retry `musci.io/blog/suno-tags` periodically (currently 500s) and merge what it adds. Expected output: ~150–300 tags after dedupe, growable to 500+ by hand.

### Live-edit behavior

`tags.json` is read **once at startup**. Edits made while the app is running take effect on the next launch. v1 has no in-app reload action; this is acceptable because the file is the user's own and changes are uncommon. (A reload button is a candidate backlog item if hand-editing becomes frequent.)

## 6.2 Tooltip null-handling (r2)

`Button.ToolTip="{Binding Description}"` with a `null` `Description` will still produce a tooltip-on-hover with empty content in WPF. v1 handles this two ways, layered:

- **Data-side fix:** the v1 seed `tags.json` populates `description` for ambiguous tags (`[Effect: Sidechain]`, `[Atmosphere: Cyberpunk]`, `[Callback: Chorus melody]`, etc.). Self-explanatory tags (`[Verse]`, `[Chorus]`) keep `description` omitted.
- **UI-side fix:** the button's tooltip is suppressed when `Description` is null/empty via `ToolTipService.IsEnabled` data-bound through a `StringIsNotEmptyConverter`. No empty tooltip box appears on hover.

## 7. Components & data flow

Three classes do the work; everything else is XAML.

### `TagService` (stateless utility)

- `IReadOnlyList<TagDefinition> LoadAll(string path)` — reads and deserializes `tags.json`.
- `IReadOnlyList<string> DistinctCategories(IEnumerable<TagDefinition> tags)` — for the ComboBox; sorted, distinct.
- `IEnumerable<TagDefinition> Filter(IEnumerable<TagDefinition> tags, string? search, string? category)` — pure function. Returns tags where:
  - `category` matches (or `category` is `null` / `"All"`), **AND**
  - `search` is null/empty **OR** `Label` or `Bracket` contains `search` (case-insensitive).

### `MainViewModel` (`ObservableObject`)

Observable properties:

- `string LyricText` — two-way bound to the editor.
- `string SearchText` — bound to the search box.
- `string SelectedCategory` — bound to the ComboBox; default `"All"`.
- `IReadOnlyList<string> Categories` — populated once at startup.
- `IReadOnlyList<TagViewModel> FilteredTags` — recomputed when `SearchText` or `SelectedCategory` changes.
- `string? LoadError` — non-null → error banner visible.

Commands:

- `InsertTagCommand(TagViewModel tag)` — raises an `InsertRequested` event the View handles (see §8).

### `TagViewModel` (thin wrapper)

Wraps a `TagDefinition` and exposes `Bracket`, `Label`, `Description` for binding/tooltip. No logic.

### `MainWindow.xaml.cs` (intentionally tiny)

Only thing it does: subscribes to `MainViewModel.InsertRequested` and performs caret-aware insertion into the editor. See §8.

### Startup flow

```
App.OnStartup
   └─ MainWindow ctor
        └─ new MainViewModel(TagService.LoadAll("tags.json"))
             ├─ Categories = ["All", ...DistinctCategories(tags)]
             ├─ FilteredTags = all tags wrapped in TagViewModel
             └─ DataContext = this ViewModel
```

### Tag-click flow

```
Button Click
   → InsertTagCommand(tagVm)
      → MainViewModel raises InsertRequested(tagVm.Definition)
         → MainWindow code-behind inserts at caret + refocuses editor
```

### Filter flow

```
SearchText or SelectedCategory changes
   → OnPropertyChanged
      → MainViewModel.RefreshFilteredTags()
         → FilteredTags = TagService.Filter(...).Select(t => new TagViewModel(t))
```

## 8. Insertion behavior

### Contract

When a tag button is clicked, insert the tag's `Bracket` text into the editor **at the current caret position**, on its own line. Cursor lands on the newly-created blank line below the tag, ready for lyrics. The editor regains focus immediately.

### Trim rules

The naïve insertion `"\n" + bracket + "\n"` produces ugly double blank lines at document boundaries. The actual rule:

- If caret is at index 0 **OR** the character immediately before the caret is `\n` → **omit** the leading `\n`.
- If caret is at the end of the text **OR** the character at the caret is `\n` → **omit** the trailing `\n`.
- Both conditions can apply at once (caret on a blank line between two existing lines). When both apply, the inserted string is the bracket alone — no newlines added. The blank line becomes the bracket's line.
- After insertion, caret position = original caret + length of the inserted string. This places the cursor on the line **after** the bracket.

### Testable seam

Extract a pure helper:

```csharp
public static string BuildInsertion(string fullText, int caret, string bracket);
```

Returns the exact string to insert (with the trim rules applied). The code-behind calls this, splices it into `LyricText`, and updates `CaretIndex`. The helper has no WPF dependencies and lives in the `SunoMetatagApp` core project so it is unit-testable.

### Why caret math stays in code-behind

`TextBox.CaretIndex` is a View concern. Abstracting it behind an `ICaretEditor` would add an interface, a fake, and View-side wiring for one line of useful logic — net negative on simplicity. The pure-logic part (`BuildInsertion`) is in the ViewModel/core layer and is fully tested; only the `Text = ...; CaretIndex = ...` two-liner lives in the View.

## 8.1 Post-insertion caret advancement (r2)

Naïvely setting `caret_new = caret_old + inserted.Length` fails the spec's "cursor lands on the line below the bracket" promise in the `omitTrailing=true` case. When the caret was at end-of-mid-document-line (so `BuildInsertion` omitted the trailing newline because a `\r\n`/`\n` already followed), `inserted` ends with `]`, not with a newline; `caret_new` lands immediately after `]`, still on the bracket's own line.

### Rule

After splicing `inserted` into the text and computing `caret_new = caret_old + inserted.Length`:

- If the character at `caret_new` is `\r`, advance `caret_new` by 1.
- If the character at `caret_new` (after the optional advance above) is `\n`, advance `caret_new` by 1.

This advances past at most one line break (`\r\n`, `\r`, or `\n`) so the caret ends up on the line **after** the bracket regardless of whether the trailing newline was added or was pre-existing.

### Testability

The advance rule is two short conditionals in `MainWindow.xaml.cs`; it is verified end-to-end by the manual smoke case **"caret at end of mid-document line"** added to the implementation plan's Task 12 Step 2.

### Why this is in the spec

The advisory found this is the one place where the otherwise pure-logic design leaks into the View. Pinning the rule in the spec keeps the View code mechanical instead of judgment-laden.

## 9. Error handling

| Failure                                | Behavior                                                                                                                                             |
|----------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| `tags.json` missing                    | `LoadError = "tags.json not found at <path>"`. Red banner above search box with a "Copy" button. Editor still works; tag panel is empty.             |
| `tags.json` malformed                  | Same banner; message includes the exception's `Message` (not full stack — keep user-readable).                                                       |
| `Filter()` exception                   | Caught at the ViewModel boundary; `LoadError` set; filtering bypassed (show all). Should only happen on a bug.                                       |
| `BuildInsertion` / caret math throws   | **No try/catch.** Bounds are deterministic; if this throws it's a real bug and should surface in the debugger.                                       |
| Editor I/O                             | None — no file operations on the lyric buffer.                                                                                                       |

## 10. Testing strategy

xUnit + FluentAssertions, in `tests\SunoMetatagApp.Tests\`. Three files. Pin the pure logic; skip the UI.

### `TagServiceTests.cs`

- `LoadAll_ParsesValidFile`
- `LoadAll_ThrowsWithClearMessage_OnMalformedJson`
- `LoadAll_ThrowsWithClearMessage_OnMissingRequiredField`
- `DistinctCategories_ReturnsSortedDistinct`

### `TagServiceFilterTests.cs` (most important — this is what the user feels)

- `Filter_AllCategory_EmptySearch_ReturnsEverything`
- `Filter_SpecificCategory_ReturnsOnlyThatCategory`
- `Filter_Search_MatchesLabel_CaseInsensitive`
- `Filter_Search_MatchesBracket_CaseInsensitive` (typing `[ver` finds `[Verse]`)
- `Filter_CategoryAndSearch_AreAndCombined`
- `Filter_EmptyResults_DoesNotThrow`

### `InsertionRulesTests.cs`

- `Insertion_AtStartOfDocument_NoLeadingNewline`
- `Insertion_AtEndOfDocument_NoTrailingNewline`
- `Insertion_AtStartOfLine_NoLeadingNewline`
- `Insertion_AtEndOfLine_NoTrailingNewline`
- `Insertion_MidLine_AddsBothNewlines`
- `Insertion_EmptyDocument_InsertsBracketOnly`

### Explicitly not tested

XAML layout, MVVM property-change plumbing, the WPF caret API itself.

## 11. Backlog (v2+ items)

To be seeded into `j:\SunoMetatagApp\docs\BACKLOG.md` during repo bootstrap. Listed in rough priority order:

1. **Favorites / recently-used tags** (B-001) — pin frequently-used tags to a "Pinned" section at the top of the panel. Persist to `%APPDATA%`.
2. **Dark theme** (B-002) — light/dark toggle, persisted.
3. **Selection-wrap insertion** (B-003) — if text is selected when a tag is clicked, the tag wraps the selection.
4. **Hotkeys for top-N tags** (B-004) — `Ctrl+1`..`Ctrl+9` for the first 9 pinned tags.
5. **Persist lyric text across launches** (B-005) — save to `%APPDATA%` on close, restore on open. Add a "Clear" button.
6. **Reload `tags.json` without restart** (B-006) — in-app reload action.
7. **Auto-update `tags.json`** (B-007) — pull updates from a known URL with manual trigger.
8. **Per-category insertion rules** (B-008) — let `tags.json` opt some tags into inline (no newlines) insertion via an `inline: true` field.
9. **Tag aliases / search synonyms** (B-009) — `aliases: ["chorus", "hook"]` so users find tags by alternate names.
10. **Retry `musci.io/blog/suno-tags` as a seed source** (B-010) — currently 500s; merge when reachable.
11. **Virtualize tag panel** (B-011) — promote when tag count > 300 OR user reports scroll stutter / search-keystroke lag. Until then v1's plain `WrapPanel` is intentional.
12. **Filter recomputation cost optimization** (B-012) — cache `TagViewModel` per `TagDefinition` so search-keystroke filtering doesn't re-materialize every wrapper.
13. **Tag button visual treatment** (B-013) — replace default WPF chrome with a flat/subtle style. v1 ships default.
14. **Screen-reader naming** (B-014) — add `AutomationProperties.Name` across the panel for accessibility.
15. **Clear-search / reset-filters affordance** (B-015) — small `✕` button or `Esc` keybind.
16. **Persist splitter position** (B-016) — remember the user's chosen left/right split across launches.
17. **Category dropdown tooltip** (B-017) — small but useful discoverability win.

## 12. Deferred / open questions

- **`musci.io/blog/suno-tags` source** — currently returns HTTP 500 via WebFetch. Retry during implementation and either include it or note in seeding why it was skipped.
- **Initial git remote** — not specified. Sibling repo will be `git init`'d during implementation; pushing to a remote is out of scope for the design.
- **Code signing / distribution** — out of scope. Build output is an unsigned local exe.

## 13. Workflow note (internal)

This design was produced via the `superpowers:brainstorming` skill. The next step per that skill is the `superpowers:writing-plans` skill, which will produce the implementation plan. That plan will then be mirrored into `j:\SunoSongSetup\ai\PLAN.md` for review under the SunoSongSetup multi-agent workflow before any code is written in `j:\SunoMetatagApp\`.

The design doc is **not** committed to git yet — `j:\SunoMetatagApp\` is not a git repo; `git init` is one of the first tasks of the implementation plan.
