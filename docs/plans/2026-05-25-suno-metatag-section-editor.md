# Suno Metatag Section Editor — Implementation Plan

> **DEPRECATED 2026-05-25** — v1 plan was executed and shipped (15 commits on main, 58/58 unit tests passing, 11/11 smoke pass on published exe). Closed `APPROVED (PASS-WITH-NOTES)` on the result-cycle review. Active plan for next slice: [`2026-05-25-suno-metatag-v1.1.md`](2026-05-25-suno-metatag-v1.1.md). This file is preserved as historical record.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a single-window WPF desktop utility that lets the user assemble a Suno prompt as a stack of structured sections (each = chip-row of tags + lyric textbox), with a live preview pane on the left and a searchable/categorized tag picker on the right.

**Architecture:** Light MVVM. Pure-logic core (`TagService`, `PreviewBuilder`) is fully unit-tested. The View hosts a 3-column layout with two `GridSplitter`s. `MainViewModel` owns an `ObservableCollection<Section>`; the preview is recomputed on any change via a pure `PreviewBuilder.Build` call. The only View-coupled code is a tiny code-behind for clipboard copy and the delete-confirmation `MessageBox`.

**Tech Stack:** WPF, .NET 8, C# 12, `CommunityToolkit.Mvvm` (NuGet), `System.Text.Json` (BCL), xUnit (tests).

**Reference spec:** `docs/specs/2026-05-25-suno-metatag-section-editor-design.md` (this repo). **Spec is at r2** — implementation plan revised in-place to honor §5.1 (initial-focus walker), §5.3 (section ▲/▼), §5.4 (chip ✕), §5.5 (arm toggle text+glyph), §5.9 (arm-hint reset on state change), §5.12 (broadcast scope badge), §10 (subscription-leak tests).

**Replaces:** The earlier `docs/plans/2026-05-25-suno-metatag-scratch.md` (deprecated; see its banner).

**Prerequisites:** .NET 8 SDK installed (`dotnet --version` shows 8.x). PowerShell on Windows for shell commands. Git installed.

---

## Notes for the implementer

- **All commands assume CWD = `j:\SunoMetatagApp\`** unless stated otherwise.
- **Test framework:** Plain xUnit, no FluentAssertions.
- **Line endings:** WPF `TextBox` uses `\r\n` on Windows. `PreviewBuilder.Build` takes a `newline` parameter so tests can use `"\n"` portably while the real app passes `Environment.NewLine`. The boundary-trim loop accepts both `'\r'` and `'\n'`.
- **Commit style:** Conventional commits (`feat:`, `test:`, `chore:`, `docs:`). One commit per task unless a task says otherwise.
- **No caret math anywhere.** This plan deliberately removes `InsertionRules` and its tests; if you find a previous-cycle copy of those files, delete them.

## What changed in r2 (2026-05-25)

Six blockers from `ai/ENGINE_REVIEW.md` r3 advisory addressed in-place. No new tasks, no task renumbering.

- **Task 9 (MainViewModel)** — adds `MoveSectionUp`/`MoveSectionDown` commands; adds `ArmedSectionCount` observable property; reworks arm-hint to reset on state change (no `DispatcherTimer`); adds two subscription-leak guard tests. **Total tests now 52** (was 45): MainViewModelTests goes from 15 → 22.
- **Task 11 (Converters)** — `ArmedToGlyphConverter` now returns `"◉ Armed"` / `"○ Disarmed"` (text + glyph instead of glyph alone).
- **Task 13b (Section stack XAML)** — adds `▲` and `▼` buttons to each section's toolbar (before the `×` delete button) bound to `MoveSectionUpCommand`/`MoveSectionDownCommand`; replaces chip `×` with `✕` (U+2715); names the `ItemsControl` `x:Name="SectionsHost"` so the focus walker can find it.
- **Task 13c (Tag picker XAML)** — adds a *"Will apply to N section(s)"* badge between the category dropdown and the button grid.
- **Task 13d (Preview + code-behind)** — adds `Window_Loaded` handler that walks the visual tree from `SectionsHost` to the first section's lyric `TextBox` and calls `.Focus()`. Subscribes via `Loaded` event in XAML.
- **Task 15 (BACKLOG)** — retires B-006 (section reorder is in v1 now); adds B-020 (debounce preview), B-021 (inline confirm), B-022 (preview cursor styling). 21 total entries.

---

## Task 0: Repo bootstrap

**Files:**
- Create: `j:\SunoMetatagApp\.gitignore`
- Create: `j:\SunoMetatagApp\README.md`

(Same as the prior plan's Task 0.)

- [ ] **Step 1: Initialize git repo**

```powershell
git init
git config user.email "wolfen231@gmail.com"
git config user.name "Jason Spencer"
```

- [ ] **Step 2: Create `.gitignore`**

Write `j:\SunoMetatagApp\.gitignore`:
```
# Build output
bin/
obj/
out/
publish/
*.user
*.suo

# Rider / Visual Studio
.idea/
.vs/
*.DotSettings.user

# OS
Thumbs.db
.DS_Store
```

- [ ] **Step 3: Create `README.md`**

Write `j:\SunoMetatagApp\README.md`:
````markdown
# Suno Metatag Editor

A single-window WPF utility for assembling Suno AI prompts as structured sections. Each section is a row of tag chips above a lyric textbox; the left pane shows a live preview you can copy to Suno.

Scratch space — no save/load.

## Requirements

- Windows 10 / 11
- .NET 8 SDK to build (`dotnet --version` → `8.x`)
- No SDK needed to *run* the published exe (self-contained)

## Build

```powershell
dotnet build
```

## Run

```powershell
dotnet run --project src/SunoMetatagApp
```

## Test

```powershell
dotnet test
```

## Publish (single-file self-contained exe)

```powershell
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

Output: `publish\SunoMetatagApp.exe` + `publish\tags.json`. Copy the folder anywhere; double-click the exe.

## Editing tags

`tags.json` ships next to the exe. Edit it to add, remove, or rename tags. The app reads it once at startup; restart to pick up changes.

## Design

See [`docs/specs/2026-05-25-suno-metatag-section-editor-design.md`](docs/specs/2026-05-25-suno-metatag-section-editor-design.md).
````

- [ ] **Step 4: Commit**

```powershell
git add .gitignore README.md docs/
git commit -m "chore: initial repo scaffolding with section-editor design spec"
```

---

## Task 1: Solution and project scaffolding

(Same as prior plan's Task 1.)

- [ ] **Step 1: Create solution + projects**

```powershell
dotnet new sln -n SunoMetatagApp
dotnet new wpf -o src/SunoMetatagApp -n SunoMetatagApp -f net8.0
dotnet new xunit -o tests/SunoMetatagApp.Tests -n SunoMetatagApp.Tests -f net8.0
dotnet sln add src/SunoMetatagApp/SunoMetatagApp.csproj
dotnet sln add tests/SunoMetatagApp.Tests/SunoMetatagApp.Tests.csproj
dotnet add tests/SunoMetatagApp.Tests reference src/SunoMetatagApp
```

- [ ] **Step 2: Add `CommunityToolkit.Mvvm`**

```powershell
dotnet add src/SunoMetatagApp package CommunityToolkit.Mvvm
```

- [ ] **Step 3: Delete default xUnit placeholder**

```powershell
Remove-Item tests/SunoMetatagApp.Tests/UnitTest1.cs -ErrorAction SilentlyContinue
```

- [ ] **Step 4: Verify build and test**

```powershell
dotnet build
dotnet test
```
Expected: `Build succeeded`; `Passed: 0`.

- [ ] **Step 5: Commit**

```powershell
git add .
git commit -m "chore: scaffold WPF main project and xUnit tests project"
```

---

## Task 2: `TagDefinition` model

**Files:**
- Create: `src/SunoMetatagApp/Models/TagDefinition.cs`

- [ ] **Step 1: Write the model**

```csharp
namespace SunoMetatagApp.Models;

public sealed record TagDefinition(
    string Category,
    string Label,
    string Bracket,
    string? Description = null);
```

- [ ] **Step 2: Verify build**

```powershell
dotnet build
```

- [ ] **Step 3: Commit**

```powershell
git add src/SunoMetatagApp/Models/TagDefinition.cs
git commit -m "feat: add TagDefinition model"
```

---

## Task 3: `TagService.LoadAll` (TDD)

**Files:**
- Create: `tests/SunoMetatagApp.Tests/TagServiceTests.cs`
- Create: `src/SunoMetatagApp/Services/TagService.cs`

- [ ] **Step 1: Write failing tests**

Write `tests/SunoMetatagApp.Tests/TagServiceTests.cs`:
```csharp
using System.IO;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

public class TagServiceTests
{
    private static string WriteTempJson(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"tags-{System.Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void LoadAll_ParsesValidFile()
    {
        var path = WriteTempJson("""
            [
              { "category": "Structure", "label": "Verse",   "bracket": "[Verse]" },
              { "category": "Vocal",     "label": "Whisper", "bracket": "[Whispered]", "description": "Soft." }
            ]
            """);

        var tags = TagService.LoadAll(path);

        Assert.Equal(2, tags.Count);
        Assert.Equal(new TagDefinition("Structure", "Verse", "[Verse]"), tags[0]);
        Assert.Equal(new TagDefinition("Vocal", "Whisper", "[Whispered]", "Soft."), tags[1]);
    }

    [Fact]
    public void LoadAll_ThrowsWithClearMessage_OnMalformedJson()
    {
        var path = WriteTempJson("not json at all");
        var ex = Assert.Throws<TagLoadException>(() => TagService.LoadAll(path));
        Assert.Contains("tags.json", ex.Message);
    }

    [Fact]
    public void LoadAll_ThrowsWithClearMessage_OnMissingRequiredField()
    {
        var path = WriteTempJson("""
            [ { "category": "Structure", "label": "Verse" } ]
            """);

        var ex = Assert.Throws<TagLoadException>(() => TagService.LoadAll(path));
        Assert.Contains("bracket", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadAll_ThrowsWithClearMessage_WhenFileMissing()
    {
        var ex = Assert.Throws<TagLoadException>(() => TagService.LoadAll("Z:\\definitely\\missing.json"));
        Assert.Contains("not found", ex.Message, System.StringComparison.OrdinalIgnoreCase);
    }
}
```

- [ ] **Step 2: Run tests — verify they fail**

```powershell
dotnet test
```
Expected: FAIL — `TagService` and `TagLoadException` not defined.

- [ ] **Step 3: Implement**

Write `src/SunoMetatagApp/Services/TagService.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

public sealed class TagLoadException : Exception
{
    public TagLoadException(string message, Exception? inner = null) : base(message, inner) { }
}

public static class TagService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IReadOnlyList<TagDefinition> LoadAll(string path)
    {
        if (!File.Exists(path))
            throw new TagLoadException($"tags.json not found at '{path}'.");

        string json;
        try { json = File.ReadAllText(path); }
        catch (Exception ex)
        {
            throw new TagLoadException($"Could not read tags.json at '{path}': {ex.Message}", ex);
        }

        List<TagDto>? dtos;
        try { dtos = JsonSerializer.Deserialize<List<TagDto>>(json, JsonOpts); }
        catch (JsonException ex)
        {
            throw new TagLoadException($"tags.json is not valid JSON: {ex.Message}", ex);
        }

        if (dtos is null)
            throw new TagLoadException("tags.json deserialized to null.");

        var result = new List<TagDefinition>(dtos.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            var d = dtos[i];
            if (string.IsNullOrWhiteSpace(d.Category))
                throw new TagLoadException($"tags.json entry {i}: missing required field 'category'.");
            if (string.IsNullOrWhiteSpace(d.Label))
                throw new TagLoadException($"tags.json entry {i}: missing required field 'label'.");
            if (string.IsNullOrWhiteSpace(d.Bracket))
                throw new TagLoadException($"tags.json entry {i}: missing required field 'bracket'.");

            result.Add(new TagDefinition(d.Category!, d.Label!, d.Bracket!, d.Description));
        }
        return result;
    }

    private sealed class TagDto
    {
        [JsonPropertyName("category")]    public string? Category { get; set; }
        [JsonPropertyName("label")]       public string? Label { get; set; }
        [JsonPropertyName("bracket")]     public string? Bracket { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }
}
```

- [ ] **Step 4: Run tests — verify pass**

```powershell
dotnet test
```
Expected: PASS, 4 tests.

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/Services/TagService.cs tests/SunoMetatagApp.Tests/TagServiceTests.cs
git commit -m "feat: add TagService.LoadAll with validation and clear errors"
```

---

## Task 4: `TagService.DistinctCategories` (TDD)

- [ ] **Step 1: Add failing test**

Append to `tests/SunoMetatagApp.Tests/TagServiceTests.cs`:
```csharp
    [Fact]
    public void DistinctCategories_ReturnsSortedDistinct()
    {
        var tags = new[]
        {
            new TagDefinition("Vocal",     "A", "[A]"),
            new TagDefinition("Structure", "B", "[B]"),
            new TagDefinition("Vocal",     "C", "[C]"),
            new TagDefinition("Effect",    "D", "[D]"),
        };
        var categories = TagService.DistinctCategories(tags);
        Assert.Equal(new[] { "Effect", "Structure", "Vocal" }, categories);
    }
```

- [ ] **Step 2: Run tests — verify new one fails**

```powershell
dotnet test
```

- [ ] **Step 3: Implement**

Add inside `TagService` (after `LoadAll`):
```csharp
    public static IReadOnlyList<string> DistinctCategories(IEnumerable<TagDefinition> tags) =>
        tags.Select(t => t.Category)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToList();
```

- [ ] **Step 4: Run tests**

Expected: PASS, 5 tests.

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/Services/TagService.cs tests/SunoMetatagApp.Tests/TagServiceTests.cs
git commit -m "feat: TagService.DistinctCategories returns sorted distinct"
```

---

## Task 5: `TagService.Filter` (TDD)

**Files:**
- Create: `tests/SunoMetatagApp.Tests/TagServiceFilterTests.cs`
- Modify: `src/SunoMetatagApp/Services/TagService.cs`

- [ ] **Step 1: Write failing tests**

Write `tests/SunoMetatagApp.Tests/TagServiceFilterTests.cs`:
```csharp
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

public class TagServiceFilterTests
{
    private static readonly TagDefinition[] Sample =
    {
        new("Structure", "Verse",   "[Verse]"),
        new("Structure", "Chorus",  "[Chorus]"),
        new("Vocal",     "Whisper", "[Whispered]"),
        new("Vocal",     "Belt",    "[Belted]"),
        new("Effect",    "Reverb",  "[Effect: Reverb: Hall]"),
    };

    [Fact] public void Filter_AllCategory_EmptySearch_ReturnsEverything()
        => Assert.Equal(5, TagService.Filter(Sample, null, "All").Count());

    [Fact] public void Filter_NullCategory_EmptySearch_ReturnsEverything()
        => Assert.Equal(5, TagService.Filter(Sample, "", null).Count());

    [Fact]
    public void Filter_SpecificCategory_ReturnsOnlyThatCategory()
    {
        var r = TagService.Filter(Sample, null, "Vocal").ToList();
        Assert.Equal(2, r.Count);
        Assert.All(r, t => Assert.Equal("Vocal", t.Category));
    }

    [Fact]
    public void Filter_Search_MatchesLabel_CaseInsensitive()
    {
        var r = TagService.Filter(Sample, "whisp", "All").ToList();
        Assert.Single(r);
        Assert.Equal("Whisper", r[0].Label);
    }

    [Fact]
    public void Filter_Search_MatchesBracket_CaseInsensitive()
    {
        var r = TagService.Filter(Sample, "[VER", "All").ToList();
        Assert.Single(r);
        Assert.Equal("[Verse]", r[0].Bracket);
    }

    [Fact]
    public void Filter_CategoryAndSearch_AreAndCombined()
    {
        var r = TagService.Filter(Sample, "v", "Structure").ToList();
        Assert.Single(r);
        Assert.Equal("Verse", r[0].Label);
    }

    [Fact]
    public void Filter_EmptyResults_DoesNotThrow()
        => Assert.Empty(TagService.Filter(Sample, "zzzzz", "All"));
}
```

- [ ] **Step 2: Run tests — verify they fail**

```powershell
dotnet test
```

- [ ] **Step 3: Implement**

Add inside `TagService`:
```csharp
    public static IEnumerable<TagDefinition> Filter(
        IEnumerable<TagDefinition> tags,
        string? search,
        string? category)
    {
        bool categoryMatches(TagDefinition t) =>
            string.IsNullOrEmpty(category) ||
            category.Equals("All", StringComparison.Ordinal) ||
            t.Category.Equals(category, StringComparison.Ordinal);

        bool searchMatches(TagDefinition t)
        {
            if (string.IsNullOrEmpty(search)) return true;
            return t.Label.Contains(search, StringComparison.OrdinalIgnoreCase)
                || t.Bracket.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        return tags.Where(t => categoryMatches(t) && searchMatches(t));
    }
```

- [ ] **Step 4: Run tests**

Expected: PASS, 12 tests.

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/Services/TagService.cs tests/SunoMetatagApp.Tests/TagServiceFilterTests.cs
git commit -m "feat: TagService.Filter with category+search AND semantics"
```

---

## Task 6: `Section` model (TDD)

**Files:**
- Create: `tests/SunoMetatagApp.Tests/SectionTests.cs`
- Create: `src/SunoMetatagApp/Models/Section.cs`

- [ ] **Step 1: Write failing tests**

Write `tests/SunoMetatagApp.Tests/SectionTests.cs`:
```csharp
using System.Collections.Specialized;
using SunoMetatagApp.Models;
using Xunit;

namespace SunoMetatagApp.Tests;

public class SectionTests
{
    private static TagDefinition Tag(string name) =>
        new("Test", name, $"[{name}]");

    [Fact]
    public void NewSection_DefaultsToArmed_WithEmptyLyricsAndNoTags()
    {
        var s = new Section();
        Assert.True(s.IsArmed);
        Assert.Equal("", s.Lyrics);
        Assert.Empty(s.Tags);
    }

    [Fact]
    public void RemoveTag_RemovesGivenTag()
    {
        var s = new Section();
        var t = Tag("A");
        s.Tags.Add(t);
        s.RemoveTagCommand.Execute(t);
        Assert.Empty(s.Tags);
    }

    [Fact]
    public void RemoveTag_NullTag_DoesNothing()
    {
        var s = new Section();
        s.Tags.Add(Tag("A"));
        s.RemoveTagCommand.Execute(null);
        Assert.Single(s.Tags);
    }

    [Fact]
    public void MoveTagLeft_AtFirstPosition_DoesNothing()
    {
        var s = new Section();
        var a = Tag("A"); var b = Tag("B");
        s.Tags.Add(a); s.Tags.Add(b);
        s.MoveTagLeftCommand.Execute(a);
        Assert.Equal(a, s.Tags[0]);
        Assert.Equal(b, s.Tags[1]);
    }

    [Fact]
    public void MoveTagLeft_SwapsWithPrevious()
    {
        var s = new Section();
        var a = Tag("A"); var b = Tag("B");
        s.Tags.Add(a); s.Tags.Add(b);
        s.MoveTagLeftCommand.Execute(b);
        Assert.Equal(b, s.Tags[0]);
        Assert.Equal(a, s.Tags[1]);
    }

    [Fact]
    public void MoveTagRight_AtLastPosition_DoesNothing()
    {
        var s = new Section();
        var a = Tag("A"); var b = Tag("B");
        s.Tags.Add(a); s.Tags.Add(b);
        s.MoveTagRightCommand.Execute(b);
        Assert.Equal(a, s.Tags[0]);
        Assert.Equal(b, s.Tags[1]);
    }

    [Fact]
    public void MoveTagRight_SwapsWithNext()
    {
        var s = new Section();
        var a = Tag("A"); var b = Tag("B");
        s.Tags.Add(a); s.Tags.Add(b);
        s.MoveTagRightCommand.Execute(a);
        Assert.Equal(b, s.Tags[0]);
        Assert.Equal(a, s.Tags[1]);
    }

    [Fact]
    public void Tags_CollectionChangedFires_OnAdd()
    {
        var s = new Section();
        int fires = 0;
        s.Tags.CollectionChanged += (_, _) => fires++;
        s.Tags.Add(Tag("A"));
        Assert.Equal(1, fires);
    }
}
```

- [ ] **Step 2: Run tests — verify they fail**

```powershell
dotnet test
```

- [ ] **Step 3: Implement `Section`**

Write `src/SunoMetatagApp/Models/Section.cs`:
```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SunoMetatagApp.Models;

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
        if (tag is null) return;
        var i = Tags.IndexOf(tag);
        if (i > 0) Tags.Move(i, i - 1);
    }

    [RelayCommand]
    private void MoveTagRight(TagDefinition? tag)
    {
        if (tag is null) return;
        var i = Tags.IndexOf(tag);
        if (i >= 0 && i < Tags.Count - 1) Tags.Move(i, i + 1);
    }
}
```

- [ ] **Step 4: Run tests**

Expected: PASS, 20 tests total (12 + 8).

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/Models/Section.cs tests/SunoMetatagApp.Tests/SectionTests.cs
git commit -m "feat: add Section model with Tags collection and chip commands"
```

---

## Task 7: `PreviewBuilder` (TDD)

**Files:**
- Create: `tests/SunoMetatagApp.Tests/PreviewBuilderTests.cs`
- Create: `src/SunoMetatagApp/Services/PreviewBuilder.cs`

- [ ] **Step 1: Write failing tests**

Write `tests/SunoMetatagApp.Tests/PreviewBuilderTests.cs`:
```csharp
using System.Collections.Generic;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

public class PreviewBuilderTests
{
    private const string NL = "\n";

    private static TagDefinition Tag(string name) => new("Test", name, $"[{name}]");

    private static Section Section(string lyrics, params TagDefinition[] tags)
    {
        var s = new Section { Lyrics = lyrics };
        foreach (var t in tags) s.Tags.Add(t);
        return s;
    }

    [Fact]
    public void Build_NoSections_ReturnsEmpty()
        => Assert.Equal("", PreviewBuilder.Build(new List<Section>(), NL));

    [Fact]
    public void Build_SingleEmptySection_ReturnsEmpty()
        => Assert.Equal("", PreviewBuilder.Build(new[] { new Section() }, NL));

    [Fact]
    public void Build_SingleSection_TagsAndLyrics_RendersTagsThenLyrics()
    {
        var s = Section("Song here\nIt's lyrics", Tag("Guitar"), Tag("Powerful"));
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("[Guitar]\n[Powerful]\nSong here\nIt's lyrics", result);
    }

    [Fact]
    public void Build_SingleSection_TagsOnly_RendersTagsNoTrailingBlank()
    {
        var s = Section("", Tag("Outro"));
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("[Outro]", result);
    }

    [Fact]
    public void Build_SingleSection_LyricsOnly_RendersLyricsAsIs()
    {
        var s = Section("just lyrics");
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("just lyrics", result);
    }

    [Fact]
    public void Build_TwoSections_SeparatedByOneBlankLine()
    {
        var s1 = Section("l1", Tag("A"));
        var s2 = Section("l2", Tag("B"));
        var result = PreviewBuilder.Build(new[] { s1, s2 }, NL);
        Assert.Equal("[A]\nl1\n\n[B]\nl2", result);
    }

    [Fact]
    public void Build_MiddleSectionEmpty_SkippedFromOutput()
    {
        var s1 = Section("l1", Tag("A"));
        var sMid = new Section(); // empty
        var s2 = Section("l2", Tag("B"));
        var result = PreviewBuilder.Build(new[] { s1, sMid, s2 }, NL);
        Assert.Equal("[A]\nl1\n\n[B]\nl2", result);
    }

    [Fact]
    public void Build_SectionLyricsEndingInNewline_NormalizedAtBoundary()
    {
        var s1 = Section("l1\n", Tag("A"));
        var s2 = Section("l2", Tag("B"));
        var result = PreviewBuilder.Build(new[] { s1, s2 }, NL);
        // Trailing \n on s1 is trimmed at the between-sections boundary; one blank line remains.
        Assert.Equal("[A]\nl1\n\n[B]\nl2", result);
    }

    [Fact]
    public void Build_PreservesTagOrder_WithinSection()
    {
        var s = Section("l", Tag("B"), Tag("A"), Tag("C"));
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("[B]\n[A]\n[C]\nl", result);
    }

    [Fact]
    public void Build_HandlesCrLfNewlineParameter()
    {
        var s = Section("l", Tag("A"));
        var result = PreviewBuilder.Build(new[] { s }, "\r\n");
        Assert.Equal("[A]\r\nl", result);
    }
}
```

- [ ] **Step 2: Run tests — verify they fail**

```powershell
dotnet test
```

- [ ] **Step 3: Implement**

Write `src/SunoMetatagApp/Services/PreviewBuilder.cs`:
```csharp
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

public static class PreviewBuilder
{
    public static string Build(IReadOnlyList<Section> sections, string newline)
    {
        var sb = new StringBuilder();
        var rendered = sections
            .Where(s => s.Tags.Count > 0 || !string.IsNullOrEmpty(s.Lyrics))
            .ToList();

        for (int i = 0; i < rendered.Count; i++)
        {
            var s = rendered[i];

            foreach (var tag in s.Tags)
                sb.Append(tag.Bracket).Append(newline);

            if (!string.IsNullOrEmpty(s.Lyrics))
                sb.Append(s.Lyrics);

            if (i < rendered.Count - 1)
            {
                while (sb.Length > 0 && (sb[sb.Length - 1] == '\n' || sb[sb.Length - 1] == '\r'))
                    sb.Length--;
                sb.Append(newline).Append(newline);
            }
        }

        while (sb.Length > 0 && (sb[sb.Length - 1] == '\n' || sb[sb.Length - 1] == '\r'))
            sb.Length--;

        return sb.ToString();
    }
}
```

- [ ] **Step 4: Run tests**

Expected: PASS, 30 tests total (20 + 10).

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/Services/PreviewBuilder.cs tests/SunoMetatagApp.Tests/PreviewBuilderTests.cs
git commit -m "feat: PreviewBuilder renders sections with one-blank-line separator and boundary trim"
```

---

## Task 8: `TagViewModel` (no tests — trivial wrapper)

**Files:**
- Create: `src/SunoMetatagApp/ViewModels/TagViewModel.cs`

- [ ] **Step 1: Write the wrapper**

```csharp
using SunoMetatagApp.Models;

namespace SunoMetatagApp.ViewModels;

public sealed class TagViewModel
{
    public TagViewModel(TagDefinition definition) { Definition = definition; }
    public TagDefinition Definition { get; }
    public string Label        => Definition.Label;
    public string Bracket      => Definition.Bracket;
    public string Category     => Definition.Category;
    public string? Description => Definition.Description;
}
```

- [ ] **Step 2: Build**

```powershell
dotnet build
```

- [ ] **Step 3: Commit**

```powershell
git add src/SunoMetatagApp/ViewModels/TagViewModel.cs
git commit -m "feat: add TagViewModel binding wrapper"
```

---

## Task 9: `MainViewModel` (TDD)

**Files:**
- Create: `tests/SunoMetatagApp.Tests/MainViewModelTests.cs`
- Create: `src/SunoMetatagApp/ViewModels/MainViewModel.cs`

- [ ] **Step 1: Write failing tests**

Write `tests/SunoMetatagApp.Tests/MainViewModelTests.cs`:
```csharp
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;
using Xunit;

namespace SunoMetatagApp.Tests;

public class MainViewModelTests
{
    private static readonly TagDefinition[] Sample =
    {
        new("Structure", "Verse",   "[Verse]"),
        new("Structure", "Chorus",  "[Chorus]"),
        new("Vocal",     "Whisper", "[Whispered]"),
    };

    [Fact]
    public void Ctor_PopulatesCategoriesWithAllPrefix()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal(new[] { "All", "Structure", "Vocal" }, vm.Categories);
        Assert.Equal("All", vm.SelectedCategory);
    }

    [Fact]
    public void Ctor_StartsWithOneArmedEmptySection()
    {
        var vm = new MainViewModel(Sample);
        Assert.Single(vm.Sections);
        Assert.True(vm.Sections[0].IsArmed);
        Assert.Equal("", vm.Sections[0].Lyrics);
        Assert.Empty(vm.Sections[0].Tags);
    }

    [Fact]
    public void Ctor_PreviewText_StartsEmpty()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal("", vm.PreviewText);
    }

    [Fact]
    public void AddSection_AppendsArmedSection()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.Sections.Count);
        Assert.True(vm.Sections[1].IsArmed);
    }

    [Fact]
    public void RemoveSection_OnLastSection_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        Assert.Single(vm.Sections);
        vm.RemoveSectionCommand.Execute(vm.Sections[0]);
        Assert.Single(vm.Sections);
    }

    [Fact]
    public void RemoveSection_OnMiddleSection_Removes()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(3, vm.Sections.Count);
        var middle = vm.Sections[1];
        vm.RemoveSectionCommand.Execute(middle);
        Assert.Equal(2, vm.Sections.Count);
        Assert.DoesNotContain(middle, vm.Sections);
    }

    [Fact]
    public void InsertTag_WithOneArmedSection_AppendsToThatSection()
    {
        var vm = new MainViewModel(Sample);
        var tagVm = new TagViewModel(Sample[0]); // [Verse]
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Single(vm.Sections[0].Tags);
        Assert.Equal("[Verse]", vm.Sections[0].Tags[0].Bracket);
    }

    [Fact]
    public void InsertTag_WithMultipleArmedSections_AppendsToAll()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null); // second section, also armed
        var tagVm = new TagViewModel(Sample[1]); // [Chorus]
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Equal("[Chorus]", vm.Sections[0].Tags[0].Bracket);
        Assert.Equal("[Chorus]", vm.Sections[1].Tags[0].Bracket);
    }

    [Fact]
    public void InsertTag_WithNoArmedSections_DoesNotMutate_AndSetsShowArmHint()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].IsArmed = false;
        var tagVm = new TagViewModel(Sample[0]);
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Empty(vm.Sections[0].Tags);
        Assert.True(vm.ShowArmHint);
    }

    [Fact]
    public void InsertTag_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        var tagVm = new TagViewModel(Sample[0]); // [Verse]
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Equal("[Verse]", vm.PreviewText);
    }

    [Fact]
    public void LyricsChange_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].Lyrics = "hello";
        Assert.Equal("hello", vm.PreviewText);
    }

    [Fact]
    public void RemoveTagOnSection_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        var t = Sample[0];
        vm.Sections[0].Tags.Add(t);
        Assert.Equal("[Verse]", vm.PreviewText);
        vm.Sections[0].RemoveTagCommand.Execute(t);
        Assert.Equal("", vm.PreviewText);
    }

    [Fact]
    public void ChangingSearchText_RecomputesFilteredTags()
    {
        var vm = new MainViewModel(Sample);
        vm.SearchText = "whisp";
        Assert.Single(vm.FilteredTags);
        Assert.Equal("Whisper", vm.FilteredTags[0].Label);
    }

    [Fact]
    public void ChangingSelectedCategory_RecomputesFilteredTags()
    {
        var vm = new MainViewModel(Sample);
        vm.SelectedCategory = "Vocal";
        Assert.Single(vm.FilteredTags);
        Assert.Equal("Whisper", vm.FilteredTags[0].Label);
    }

    [Fact]
    public void LoadErrorCtor_HasEmptyCategoriesAndSections()
    {
        var vm = new MainViewModel("tags.json not found.");
        Assert.Equal("tags.json not found.", vm.LoadError);
        Assert.Equal(new[] { "All" }, vm.Categories);
        Assert.Empty(vm.Sections);
        Assert.Empty(vm.FilteredTags);
    }

    // ---- r2 additions ----

    [Fact]
    public void Ctor_ArmedSectionCount_IsOne()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal(1, vm.ArmedSectionCount);
    }

    [Fact]
    public void AddSection_IncrementsArmedSectionCount()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.ArmedSectionCount);
    }

    [Fact]
    public void RemoveSection_DecrementsArmedSectionCount_WhenRemovedWasArmed()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.ArmedSectionCount);
        vm.RemoveSectionCommand.Execute(vm.Sections[1]);
        Assert.Equal(1, vm.ArmedSectionCount);
    }

    [Fact]
    public void IsArmedChange_UpdatesArmedSectionCount()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.ArmedSectionCount);
        vm.Sections[0].IsArmed = false;
        Assert.Equal(1, vm.ArmedSectionCount);
    }

    [Fact]
    public void IsArmedTransitionToTrue_ClearsShowArmHint()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].IsArmed = false;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0]));
        Assert.True(vm.ShowArmHint);
        vm.Sections[0].IsArmed = true;
        Assert.False(vm.ShowArmHint);
    }

    [Fact]
    public void InsertTag_WithArmedSection_ClearsShowArmHint()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].IsArmed = false;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0]));
        Assert.True(vm.ShowArmHint);
        vm.Sections[0].IsArmed = true;
        // ShowArmHint already cleared by the state transition; verify next click keeps it false.
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0]));
        Assert.False(vm.ShowArmHint);
    }

    [Fact]
    public void MoveSectionUp_AtTop_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionUpCommand.Execute(s0);
        Assert.Equal(s0, vm.Sections[0]);
        Assert.Equal(s1, vm.Sections[1]);
    }

    [Fact]
    public void MoveSectionUp_SwapsWithPrevious()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionUpCommand.Execute(s1);
        Assert.Equal(s1, vm.Sections[0]);
        Assert.Equal(s0, vm.Sections[1]);
    }

    [Fact]
    public void MoveSectionDown_AtBottom_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionDownCommand.Execute(s1);
        Assert.Equal(s0, vm.Sections[0]);
        Assert.Equal(s1, vm.Sections[1]);
    }

    [Fact]
    public void MoveSectionDown_SwapsWithNext()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionDownCommand.Execute(s0);
        Assert.Equal(s1, vm.Sections[0]);
        Assert.Equal(s0, vm.Sections[1]);
    }

    [Fact]
    public void MoveSection_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        vm.Sections[0].Lyrics = "first";
        vm.Sections[1].Lyrics = "second";
        Assert.StartsWith("first", vm.PreviewText);
        vm.MoveSectionDownCommand.Execute(vm.Sections[0]);
        Assert.StartsWith("second", vm.PreviewText);
    }

    [Fact]
    public void AfterRemoveSection_MutatingRemovedSectionLyrics_DoesNotChangePreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var doomed = vm.Sections[1];
        vm.RemoveSectionCommand.Execute(doomed);
        var before = vm.PreviewText;
        doomed.Lyrics = "should be ignored";
        Assert.Equal(before, vm.PreviewText);
    }

    [Fact]
    public void AfterRemoveSection_AddingTagToRemovedSection_DoesNotChangePreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var doomed = vm.Sections[1];
        vm.RemoveSectionCommand.Execute(doomed);
        var before = vm.PreviewText;
        doomed.Tags.Add(Sample[0]);
        Assert.Equal(before, vm.PreviewText);
    }
}
```

- [ ] **Step 2: Run tests — verify they fail**

```powershell
dotnet test
```

- [ ] **Step 3: Implement `MainViewModel`**

Write `src/SunoMetatagApp/ViewModels/MainViewModel.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;

namespace SunoMetatagApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<TagDefinition> _allTags;

    public ObservableCollection<Section> Sections { get; } = new();
    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedCategory = "All";
    [ObservableProperty] private string _lyricText = string.Empty; // unused; retained for forward-compat
    [ObservableProperty] private IReadOnlyList<TagViewModel> _filteredTags = Array.Empty<TagViewModel>();
    [ObservableProperty] private string _previewText = string.Empty;
    [ObservableProperty] private string? _loadError;
    [ObservableProperty] private bool _showArmHint;
    [ObservableProperty] private int _armedSectionCount;

    public event EventHandler? CopyRequested;

    public MainViewModel(IReadOnlyList<TagDefinition> tags)
    {
        _allTags = tags;
        Categories = BuildCategories(tags);
        SelectedCategory = "All";
        FilteredTags = ComputeFiltered();
        Sections.CollectionChanged += OnSectionsChanged;
        AddSection();
    }

    public MainViewModel(string loadError)
    {
        _allTags = Array.Empty<TagDefinition>();
        Categories = new[] { "All" };
        SelectedCategory = "All";
        FilteredTags = Array.Empty<TagViewModel>();
        LoadError = loadError;
        Sections.CollectionChanged += OnSectionsChanged;
        // No starter section in the load-error ctor.
    }

    [RelayCommand]
    private void AddSection() => Sections.Add(new Section());

    [RelayCommand]
    private void RemoveSection(Section? section)
    {
        if (section is null) return;
        if (Sections.Count <= 1) return;
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

    [RelayCommand]
    private void CopyPreview() => CopyRequested?.Invoke(this, EventArgs.Empty);

    partial void OnSearchTextChanged(string value) => FilteredTags = ComputeFiltered();
    partial void OnSelectedCategoryChanged(string value) => FilteredTags = ComputeFiltered();

    private void OnSectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (Section s in e.NewItems) SubscribeToSection(s);
        if (e.OldItems != null)
            foreach (Section s in e.OldItems) UnsubscribeFromSection(s);
        RecomputePreview();
        RecomputeArmedCount();
    }

    private void SubscribeToSection(Section s)
    {
        s.PropertyChanged += OnSectionPropertyChanged;
        s.Tags.CollectionChanged += OnSectionTagsChanged;
    }

    private void UnsubscribeFromSection(Section s)
    {
        s.PropertyChanged -= OnSectionPropertyChanged;
        s.Tags.CollectionChanged -= OnSectionTagsChanged;
    }

    private void OnSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Section.Lyrics))
        {
            RecomputePreview();
        }
        else if (e.PropertyName == nameof(Section.IsArmed))
        {
            RecomputeArmedCount();
            if (sender is Section s && s.IsArmed) ShowArmHint = false;
        }
    }

    private void OnSectionTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => RecomputePreview();

    private void RecomputePreview()
        => PreviewText = PreviewBuilder.Build(Sections.ToList(), Environment.NewLine);

    private void RecomputeArmedCount()
        => ArmedSectionCount = Sections.Count(s => s.IsArmed);

    private static IReadOnlyList<string> BuildCategories(IEnumerable<TagDefinition> tags)
    {
        var distinct = TagService.DistinctCategories(tags);
        var list = new List<string>(distinct.Count + 1) { "All" };
        list.AddRange(distinct);
        return list;
    }

    private IReadOnlyList<TagViewModel> ComputeFiltered() =>
        TagService.Filter(_allTags, SearchText, SelectedCategory)
                  .Select(t => new TagViewModel(t))
                  .ToList();
}
```

**Note on the PreviewText test assertions:** the tests use `"\n"`-based literals (`"[Verse]"`, `"hello"`, `"[Verse]"`) so the assertions pass on Windows even though `Environment.NewLine` is `"\r\n"` — only the *between-sections* and *intra-section* newlines are affected, and the test cases here are all single-section. The earlier `PreviewBuilderTests.cs` covers the multi-section/CRLF cases explicitly.

- [ ] **Step 4: Run tests**

Expected: PASS, 52 tests total (30 + 22).

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/ViewModels/MainViewModel.cs tests/SunoMetatagApp.Tests/MainViewModelTests.cs
git commit -m "feat: MainViewModel with section-based model, preview recompute, arm-targeted tag insert, section reorder, leak guards"
```

---

## Task 10: Seed `tags.json`

**Files:**
- Create: `src/SunoMetatagApp/Resources/tags.json`
- Modify: `src/SunoMetatagApp/SunoMetatagApp.csproj`

Carry over the r2 starter JSON from the prior plan — ~115 tags with descriptions populated for ambiguous ones. (Identical content; reproduced here so this plan is self-contained.)

- [ ] **Step 1: Write `tags.json`**

Write `src/SunoMetatagApp/Resources/tags.json`:
```json
[
  { "category": "Structure", "label": "Intro",            "bracket": "[Intro]" },
  { "category": "Structure", "label": "Verse",            "bracket": "[Verse]" },
  { "category": "Structure", "label": "Verse 1",          "bracket": "[Verse 1]" },
  { "category": "Structure", "label": "Verse 2",          "bracket": "[Verse 2]" },
  { "category": "Structure", "label": "Pre-Chorus",       "bracket": "[Pre-Chorus]" },
  { "category": "Structure", "label": "Chorus",           "bracket": "[Chorus]" },
  { "category": "Structure", "label": "Post-Chorus",      "bracket": "[Post-Chorus]" },
  { "category": "Structure", "label": "Bridge",           "bracket": "[Bridge]" },
  { "category": "Structure", "label": "Outro",            "bracket": "[Outro]" },
  { "category": "Structure", "label": "End",              "bracket": "[End]" },
  { "category": "Structure", "label": "Instrumental",     "bracket": "[Instrumental]" },
  { "category": "Structure", "label": "Interlude",        "bracket": "[Interlude]" },
  { "category": "Structure", "label": "Break",            "bracket": "[Break]" },
  { "category": "Structure", "label": "Breakdown",        "bracket": "[Breakdown]" },
  { "category": "Structure", "label": "Build",            "bracket": "[Build]" },
  { "category": "Structure", "label": "Build-up",         "bracket": "[Build-up]" },
  { "category": "Structure", "label": "Drop",             "bracket": "[Drop]" },
  { "category": "Structure", "label": "Hook",             "bracket": "[Hook]" },
  { "category": "Structure", "label": "Refrain",          "bracket": "[Refrain]" },
  { "category": "Structure", "label": "Solo",             "bracket": "[Solo]" },
  { "category": "Structure", "label": "Guitar Solo",      "bracket": "[Guitar Solo]" },
  { "category": "Structure", "label": "Saxophone Solo",   "bracket": "[Saxophone Solo]" },
  { "category": "Structure", "label": "Drum Break",       "bracket": "[Drum Break]" },
  { "category": "Structure", "label": "Drum Fill",        "bracket": "[Drum Fill]" },
  { "category": "Structure", "label": "Fade In",          "bracket": "[Fade In]" },
  { "category": "Structure", "label": "Fade Out",         "bracket": "[Fade Out]" },

  { "category": "Vocal", "label": "Whispered",        "bracket": "[Whispered]",        "description": "Soft, intimate delivery." },
  { "category": "Vocal", "label": "Soft",             "bracket": "[Soft]" },
  { "category": "Vocal", "label": "Gentle",           "bracket": "[Gentle]" },
  { "category": "Vocal", "label": "Spoken",           "bracket": "[Spoken]" },
  { "category": "Vocal", "label": "Spoken Word",      "bracket": "[Spoken Word]" },
  { "category": "Vocal", "label": "Powerful",         "bracket": "[Powerful]" },
  { "category": "Vocal", "label": "Belted",           "bracket": "[Belted]" },
  { "category": "Vocal", "label": "Shouted",          "bracket": "[Shouted]" },
  { "category": "Vocal", "label": "Screamed",         "bracket": "[Screamed]" },
  { "category": "Vocal", "label": "Growled",          "bracket": "[Growled]" },
  { "category": "Vocal", "label": "Falsetto",         "bracket": "[Falsetto]" },
  { "category": "Vocal", "label": "Head Voice",       "bracket": "[Head Voice]" },
  { "category": "Vocal", "label": "Chest Voice",      "bracket": "[Chest Voice]" },
  { "category": "Vocal", "label": "Breathy",          "bracket": "[Breathy]" },
  { "category": "Vocal", "label": "Raspy",            "bracket": "[Raspy]" },
  { "category": "Vocal", "label": "Smooth",           "bracket": "[Smooth]" },
  { "category": "Vocal", "label": "Soulful",          "bracket": "[Soulful]" },
  { "category": "Vocal", "label": "Operatic",         "bracket": "[Operatic]" },
  { "category": "Vocal", "label": "Nasal",            "bracket": "[Nasal]" },
  { "category": "Vocal", "label": "Airy",             "bracket": "[Airy]" },
  { "category": "Vocal", "label": "Harmonies",        "bracket": "[Harmonies]" },
  { "category": "Vocal", "label": "Ad-libs",          "bracket": "[Ad-libs]" },
  { "category": "Vocal", "label": "Vocal Run",        "bracket": "[Vocal Run]" },
  { "category": "Vocal", "label": "Melisma",          "bracket": "[Melisma]" },
  { "category": "Vocal", "label": "Vibrato",          "bracket": "[Vibrato]" },
  { "category": "Vocal", "label": "Staccato",         "bracket": "[Staccato]" },
  { "category": "Vocal", "label": "Legato",           "bracket": "[Legato]" },
  { "category": "Vocal", "label": "Call and Response","bracket": "[Call and Response]" },
  { "category": "Vocal", "label": "Chant",            "bracket": "[Chant]" },
  { "category": "Vocal", "label": "Choir",            "bracket": "[Choir]" },
  { "category": "Vocal", "label": "Rapped",           "bracket": "[Rapped]" },
  { "category": "Vocal", "label": "Fast Rap",         "bracket": "[Fast Rap]" },
  { "category": "Vocal", "label": "Slow Flow",        "bracket": "[Slow Flow]" },
  { "category": "Vocal", "label": "Melodic Rap",      "bracket": "[Melodic Rap]" },
  { "category": "Vocal", "label": "Trap Flow",        "bracket": "[Trap Flow]" },
  { "category": "Vocal", "label": "Auto-tune",        "bracket": "[Voice: Auto-tune]" },

  { "category": "Instrument", "label": "Electric Guitar",  "bracket": "[Electric Guitar]" },
  { "category": "Instrument", "label": "Acoustic Guitar",  "bracket": "[Acoustic Guitar]" },
  { "category": "Instrument", "label": "Cello",            "bracket": "[Cello]" },
  { "category": "Instrument", "label": "Violin",           "bracket": "[Violin]" },
  { "category": "Instrument", "label": "Banjo",            "bracket": "[Banjo]" },
  { "category": "Instrument", "label": "Ukulele",          "bracket": "[Ukulele]" },
  { "category": "Instrument", "label": "Harp",             "bracket": "[Harp]" },
  { "category": "Instrument", "label": "Grand Piano",      "bracket": "[Grand Piano]" },
  { "category": "Instrument", "label": "Electric Piano",   "bracket": "[Electric Piano]" },
  { "category": "Instrument", "label": "Hammond Organ",    "bracket": "[Hammond Organ]" },
  { "category": "Instrument", "label": "Rhodes",           "bracket": "[Rhodes]" },
  { "category": "Instrument", "label": "Analog Synth",     "bracket": "[Analog Synth]" },
  { "category": "Instrument", "label": "Accordion",        "bracket": "[Accordion]" },
  { "category": "Instrument", "label": "Harpsichord",      "bracket": "[Harpsichord]" },
  { "category": "Instrument", "label": "Mellotron",        "bracket": "[Mellotron]" },
  { "category": "Instrument", "label": "808 Bass",         "bracket": "[808 Bass]" },
  { "category": "Instrument", "label": "Distorted Bass",   "bracket": "[Distorted Bass]" },
  { "category": "Instrument", "label": "Double Bass",      "bracket": "[Double Bass]" },
  { "category": "Instrument", "label": "Electronic Drums", "bracket": "[Electronic Drums]" },
  { "category": "Instrument", "label": "Hand Percussion",  "bracket": "[Hand Percussion]" },
  { "category": "Instrument", "label": "Timpani",          "bracket": "[Timpani]" },
  { "category": "Instrument", "label": "Congas",           "bracket": "[Congas]" },
  { "category": "Instrument", "label": "Shakers",          "bracket": "[Shakers]" },
  { "category": "Instrument", "label": "Tambourine",       "bracket": "[Tambourine]" },
  { "category": "Instrument", "label": "Trumpet",          "bracket": "[Trumpet]" },
  { "category": "Instrument", "label": "Flute",            "bracket": "[Flute]" },
  { "category": "Instrument", "label": "Clarinet",         "bracket": "[Clarinet]" },
  { "category": "Instrument", "label": "Harmonica",        "bracket": "[Harmonica]" },
  { "category": "Instrument", "label": "Trombone",         "bracket": "[Trombone]" },
  { "category": "Instrument", "label": "Oboe",             "bracket": "[Oboe]" },
  { "category": "Instrument", "label": "Bagpipes",         "bracket": "[Bagpipes]" },
  { "category": "Instrument", "label": "Didgeridoo",       "bracket": "[Didgeridoo]" },

  { "category": "Mood", "label": "Euphoric",      "bracket": "[Mood: Euphoric]" },
  { "category": "Mood", "label": "Melancholic",   "bracket": "[Mood: Melancholic]" },
  { "category": "Mood", "label": "Aggressive",    "bracket": "[Mood: Aggressive]" },
  { "category": "Mood", "label": "Nostalgic",     "bracket": "[Mood: Nostalgic]" },
  { "category": "Mood", "label": "Dark",          "bracket": "[Mood: Dark]" },
  { "category": "Mood", "label": "Chill",         "bracket": "[Mood: Chill]" },
  { "category": "Mood", "label": "Romantic",      "bracket": "[Mood: Romantic]" },
  { "category": "Mood", "label": "High Energy",   "bracket": "[Mood: High Energy]" },
  { "category": "Mood", "label": "Soulful",       "bracket": "[Mood: Soulful]" },
  { "category": "Mood", "label": "Dreamy",        "bracket": "[Atmosphere: Dreamy]",    "description": "Soft, hazy, otherworldly atmosphere." },
  { "category": "Mood", "label": "Cyberpunk",     "bracket": "[Atmosphere: Cyberpunk]", "description": "Neon-lit, gritty, synth-heavy futuristic vibe." },
  { "category": "Mood", "label": "Medieval",      "bracket": "[Atmosphere: Medieval]",  "description": "Period-appropriate instrumentation and modal feel." },
  { "category": "Mood", "label": "Explosive",     "bracket": "[Energy: Explosive]",     "description": "Sudden, high-impact bursts of energy." },
  { "category": "Mood", "label": "Building",      "bracket": "[Energy: Building]",      "description": "Steady rise in intensity toward a peak." },

  { "category": "Effect", "label": "Lo-fi",            "bracket": "[Effect: Lo-fi]",            "description": "Warm, degraded, vinyl-style sound." },
  { "category": "Effect", "label": "Reverb: Hall",     "bracket": "[Effect: Reverb: Hall]",     "description": "Spacious concert-hall reverb." },
  { "category": "Effect", "label": "Delay: Ping-pong", "bracket": "[Effect: Delay: Ping-pong]", "description": "Echo bouncing between left and right channels." },
  { "category": "Effect", "label": "Distortion",       "bracket": "[Effect: Distortion]" },
  { "category": "Effect", "label": "Sidechain",        "bracket": "[Effect: Sidechain]",        "description": "Volume ducking pumped to the kick — classic EDM/house feel." },
  { "category": "Effect", "label": "Bitcrusher",       "bracket": "[Effect: Bitcrusher]",       "description": "Reduced bit depth — gritty, lo-fi digital sound." },
  { "category": "Effect", "label": "Autopan",          "bracket": "[Effect: Autopan]",          "description": "Sound sweeps left-right automatically." },
  { "category": "Effect", "label": "Radio Filter",     "bracket": "[Effect: Radio Filter]",     "description": "Telephone/AM-radio band-pass tone." },
  { "category": "Effect", "label": "Texture: Grainy",  "bracket": "[Texture: Grainy]",          "description": "Sandy, particulate texture overlay." },
  { "category": "Effect", "label": "Swell",            "bracket": "[Swell]",                    "description": "Slow rise in volume." },
  { "category": "Effect", "label": "Crescendo",        "bracket": "[Crescendo]",                "description": "Build in intensity over time." },
  { "category": "Effect", "label": "Decrescendo",      "bracket": "[Decrescendo]",              "description": "Gradual reduction in intensity." },

  { "category": "Production", "label": "Heavy Bass",       "bracket": "[Heavy Bass]" },
  { "category": "Production", "label": "Scratches",        "bracket": "[Scratches]",              "description": "Turntable scratching." },
  { "category": "Production", "label": "Tempo: 128 BPM",   "bracket": "[Tempo: 128 BPM]",         "description": "Beats per minute (replace 128 as needed)." },
  { "category": "Production", "label": "Callback: Chorus", "bracket": "[Callback: Chorus melody]", "description": "Echo a previous chorus melodic motif here." }
]
```

- [ ] **Step 2: Add CopyToOutput rule to `.csproj`**

In `src/SunoMetatagApp/SunoMetatagApp.csproj`, add before the closing `</Project>`:
```xml
  <ItemGroup>
    <None Update="Resources\tags.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
      <Link>tags.json</Link>
    </None>
  </ItemGroup>
```

- [ ] **Step 3: Build and verify**

```powershell
dotnet build
Test-Path src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json
```
Expected: `True`.

- [ ] **Step 4: Commit**

```powershell
git add src/SunoMetatagApp/Resources/tags.json src/SunoMetatagApp/SunoMetatagApp.csproj
git commit -m "feat: bundle starter tags.json (~115 tags, ambiguous ones described) with CopyToOutput"
```

---

## Task 11: Converters

**Files:**
- Create: `src/SunoMetatagApp/NullToCollapsedConverter.cs`
- Create: `src/SunoMetatagApp/StringIsNotEmptyConverter.cs`
- Create: `src/SunoMetatagApp/ArmedToGlyphConverter.cs`
- Create: `src/SunoMetatagApp/CountGreaterThanOneToBoolConverter.cs`

These value converters back several XAML bindings in Task 13.

- [ ] **Step 1: NullToCollapsed (error banner visibility)**

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SunoMetatagApp;

public sealed class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

- [ ] **Step 2: StringIsNotEmpty (tooltip suppression on tag-picker buttons)**

```csharp
using System;
using System.Globalization;
using System.Windows.Data;

namespace SunoMetatagApp;

public sealed class StringIsNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

- [ ] **Step 3: ArmedToGlyph (glyph + text label, r2)**

```csharp
using System;
using System.Globalization;
using System.Windows.Data;

namespace SunoMetatagApp;

public sealed class ArmedToGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? "◉ Armed" : "○ Disarmed";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

- [ ] **Step 4: CountGreaterThanOneToBool (delete-button enable when >1 section)**

```csharp
using System;
using System.Globalization;
using System.Windows.Data;

namespace SunoMetatagApp;

public sealed class CountGreaterThanOneToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int n && n > 1;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

- [ ] **Step 5: Build and commit**

```powershell
dotnet build
git add src/SunoMetatagApp/*Converter.cs
git commit -m "feat: add value converters (NullToCollapsed, StringIsNotEmpty, ArmedToGlyph, CountGreaterThanOneToBool)"
```

---

## Task 12: `App.xaml` startup wiring

**Files:**
- Modify: `src/SunoMetatagApp/App.xaml`
- Modify: `src/SunoMetatagApp/App.xaml.cs`

- [ ] **Step 1: Replace `App.xaml`**

```xml
<Application x:Class="SunoMetatagApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:SunoMetatagApp">
    <Application.Resources>
    </Application.Resources>
</Application>
```

- [ ] **Step 2: Replace `App.xaml.cs`**

```csharp
using System;
using System.IO;
using System.Windows;
using SunoMetatagApp.Services;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var tagsPath = Path.Combine(AppContext.BaseDirectory, "tags.json");

        MainViewModel vm;
        try
        {
            var tags = TagService.LoadAll(tagsPath);
            vm = new MainViewModel(tags);
        }
        catch (TagLoadException ex)
        {
            vm = new MainViewModel(ex.Message);
        }

        var window = new MainWindow { DataContext = vm };
        window.Show();
    }
}
```

- [ ] **Step 3: Build and commit**

```powershell
dotnet build
git add src/SunoMetatagApp/App.xaml src/SunoMetatagApp/App.xaml.cs
git commit -m "feat: wire App.OnStartup to load tags and construct MainViewModel"
```

---

## Task 13a: `MainWindow.xaml` — three-column grid skeleton

**Files:**
- Modify: `src/SunoMetatagApp/MainWindow.xaml`

- [ ] **Step 1: Replace `MainWindow.xaml`**

```xml
<Window x:Class="SunoMetatagApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:SunoMetatagApp"
        xmlns:vm="clr-namespace:SunoMetatagApp.ViewModels"
        xmlns:m="clr-namespace:SunoMetatagApp.Models"
        Title="Suno Metatag Editor"
        Width="1300" Height="750"
        MinWidth="900" MinHeight="500"
        WindowStartupLocation="CenterScreen"
        d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d">
    <Window.Resources>
        <local:NullToCollapsedConverter x:Key="NullToCollapsedConverter" />
        <local:StringIsNotEmptyConverter x:Key="StringIsNotEmptyConverter" />
        <local:ArmedToGlyphConverter x:Key="ArmedToGlyphConverter" />
        <local:CountGreaterThanOneToBoolConverter x:Key="CountGreaterThanOneToBoolConverter" />
    </Window.Resources>
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="3*" MinWidth="240" />
            <ColumnDefinition Width="6" />
            <ColumnDefinition Width="4*" MinWidth="360" />
            <ColumnDefinition Width="6" />
            <ColumnDefinition Width="3*" MinWidth="260" />
        </Grid.ColumnDefinitions>

        <!-- Left: preview pane (filled in Task 13d) -->
        <Grid Grid.Column="0" Margin="6">
            <TextBlock Text="Preview (filled in Task 13d)"
                       HorizontalAlignment="Center" VerticalAlignment="Center"
                       Foreground="#888" />
        </Grid>

        <GridSplitter Grid.Column="1" Width="6" Cursor="SizeWE"
                      HorizontalAlignment="Stretch" VerticalAlignment="Stretch"
                      Background="#DDD" />

        <!-- Middle: section stack (filled in Task 13b) -->
        <Grid Grid.Column="2" Margin="6">
            <TextBlock Text="Section stack (filled in Task 13b)"
                       HorizontalAlignment="Center" VerticalAlignment="Center"
                       Foreground="#888" />
        </Grid>

        <GridSplitter Grid.Column="3" Width="6" Cursor="SizeWE"
                      HorizontalAlignment="Stretch" VerticalAlignment="Stretch"
                      Background="#DDD" />

        <!-- Right: tag picker (filled in Task 13c) -->
        <Grid Grid.Column="4" Margin="6">
            <TextBlock Text="Tag picker (filled in Task 13c)"
                       HorizontalAlignment="Center" VerticalAlignment="Center"
                       Foreground="#888" />
        </Grid>
    </Grid>
</Window>
```

- [ ] **Step 2: Replace `MainWindow.xaml.cs` with a minimal stub**

```csharp
using System.Windows;

namespace SunoMetatagApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 3: Build, run, eyeball**

```powershell
dotnet build
dotnet run --project src/SunoMetatagApp
```
Expected: Window opens, ~1300×750, three columns with placeholder text and two draggable 6px splitters. Close.

- [ ] **Step 4: Commit**

```powershell
git add src/SunoMetatagApp/MainWindow.xaml src/SunoMetatagApp/MainWindow.xaml.cs
git commit -m "feat: MainWindow three-column skeleton with splitters"
```

---

## Task 13b: Section stack (middle column)

**Files:**
- Modify: `src/SunoMetatagApp/MainWindow.xaml`

- [ ] **Step 1: Replace the middle-column placeholder**

In `MainWindow.xaml`, replace the entire middle-column `<Grid Grid.Column="2" ...>...</Grid>` block with:

```xml
        <!-- Middle: section stack -->
        <DockPanel Grid.Column="2" Margin="6">
            <Button DockPanel.Dock="Bottom"
                    Content="+ Add section"
                    Margin="0,8,0,0" Padding="8,4"
                    HorizontalAlignment="Stretch"
                    Command="{Binding AddSectionCommand}" />

            <ScrollViewer VerticalScrollBarVisibility="Auto">
                <ItemsControl x:Name="SectionsHost" ItemsSource="{Binding Sections}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate DataType="{x:Type m:Section}">
                            <Border BorderBrush="#CCC" BorderThickness="1"
                                    CornerRadius="4" Margin="0,0,0,8" Padding="6"
                                    Background="White">
                                <Grid>
                                    <Grid.RowDefinitions>
                                        <RowDefinition Height="Auto" />
                                        <RowDefinition Height="Auto" />
                                        <RowDefinition Height="*" />
                                    </Grid.RowDefinitions>

                                    <!-- Toolbar row -->
                                    <DockPanel Grid.Row="0" Margin="0,0,0,4">
                                        <Button DockPanel.Dock="Right"
                                                Content="×"
                                                Padding="8,0"
                                                FontWeight="Bold"
                                                ToolTip="Delete this section"
                                                Click="DeleteSectionButton_Click"
                                                CommandParameter="{Binding}"
                                                IsEnabled="{Binding DataContext.Sections.Count, RelativeSource={RelativeSource AncestorType=Window}, Converter={StaticResource CountGreaterThanOneToBoolConverter}}" />
                                        <Button DockPanel.Dock="Right"
                                                Content="▼" Padding="6,0" Margin="0,0,3,0"
                                                FontSize="11"
                                                ToolTip="Move section down"
                                                Command="{Binding DataContext.MoveSectionDownCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                                CommandParameter="{Binding}" />
                                        <Button DockPanel.Dock="Right"
                                                Content="▲" Padding="6,0" Margin="0,0,3,0"
                                                FontSize="11"
                                                ToolTip="Move section up"
                                                Command="{Binding DataContext.MoveSectionUpCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                                CommandParameter="{Binding}" />
                                        <ToggleButton IsChecked="{Binding IsArmed}"
                                                      Content="{Binding IsArmed, Converter={StaticResource ArmedToGlyphConverter}}"
                                                      Padding="8,2"
                                                      FontSize="13"
                                                      ToolTip="Armed sections receive tag clicks" />
                                        <TextBlock Text="Section" Margin="8,0,0,0"
                                                   VerticalAlignment="Center" Foreground="#888" />
                                    </DockPanel>

                                    <!-- Chip row -->
                                    <Border Grid.Row="1" BorderBrush="#EEE" BorderThickness="1"
                                            Background="#F8F8F8" Padding="3" MinHeight="36"
                                            Margin="0,0,0,4">
                                        <ItemsControl ItemsSource="{Binding Tags}">
                                            <ItemsControl.ItemsPanel>
                                                <ItemsPanelTemplate><WrapPanel Orientation="Horizontal" /></ItemsPanelTemplate>
                                            </ItemsControl.ItemsPanel>
                                            <ItemsControl.ItemTemplate>
                                                <DataTemplate DataType="{x:Type m:TagDefinition}">
                                                    <Border Background="#E8E8FF" BorderBrush="#BBB" BorderThickness="1"
                                                            CornerRadius="3" Margin="2" Padding="3,1">
                                                        <StackPanel Orientation="Horizontal">
                                                            <TextBlock Text="{Binding Bracket}" Margin="2,0"
                                                                       VerticalAlignment="Center" />
                                                            <Button Content="◀" Padding="3,0" Margin="3,0,0,0"
                                                                    FontSize="9" MinWidth="18"
                                                                    Command="{Binding DataContext.MoveTagLeftCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}"
                                                                    CommandParameter="{Binding}"
                                                                    ToolTip="Move left" />
                                                            <Button Content="▶" Padding="3,0" Margin="1,0,0,0"
                                                                    FontSize="9" MinWidth="18"
                                                                    Command="{Binding DataContext.MoveTagRightCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}"
                                                                    CommandParameter="{Binding}"
                                                                    ToolTip="Move right" />
                                                            <Button Content="✕" Padding="4,0" Margin="2,0,2,0"
                                                                    FontWeight="Bold" MinWidth="18"
                                                                    Command="{Binding DataContext.RemoveTagCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}"
                                                                    CommandParameter="{Binding}"
                                                                    ToolTip="Remove" />
                                                        </StackPanel>
                                                    </Border>
                                                </DataTemplate>
                                            </ItemsControl.ItemTemplate>
                                        </ItemsControl>
                                    </Border>

                                    <!-- Lyric textbox -->
                                    <TextBox Grid.Row="2"
                                             Text="{Binding Lyrics, UpdateSourceTrigger=PropertyChanged}"
                                             AcceptsReturn="True"
                                             TextWrapping="Wrap"
                                             MinLines="6" MaxLines="12"
                                             VerticalScrollBarVisibility="Auto"
                                             FontFamily="Consolas" FontSize="13"
                                             Padding="6" />
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </ScrollViewer>
        </DockPanel>
```

- [ ] **Step 2: Add the `DeleteSectionButton_Click` handler to code-behind**

In `MainWindow.xaml.cs`, replace the class body:

```csharp
using System.Windows;
using System.Windows.Controls;
using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DeleteSectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not Section section) return;
        if (DataContext is not MainViewModel vm) return;

        bool hasContent = section.Tags.Count > 0 || !string.IsNullOrEmpty(section.Lyrics);
        if (hasContent)
        {
            var result = MessageBox.Show(
                "Delete this section? Its tags and lyrics will be lost.",
                "Confirm delete",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.OK) return;
        }

        vm.RemoveSectionCommand.Execute(section);
    }
}
```

- [ ] **Step 3: Build and run, eyeball**

```powershell
dotnet build
dotnet run --project src/SunoMetatagApp
```

Expected: Window opens with one section visible in the middle column. The section toolbar shows: `◉ Armed` toggle on the left, the literal "Section" label, then `▲` (disabled), `▼` (disabled), `×` (disabled — only one section). Below the toolbar is an empty chip row (light-gray bordered area, ~36px tall), then an empty Consolas textbox. Bottom of the middle column has a "+ Add section" button.

Manual checks:
- Click "+ Add section" → second section appears below; both delete buttons are now enabled; `▼` on section 1 enables; `▲` on section 2 enables.
- Click `▼` on section 1 → it swaps places with section 2. Click `▲` on it → swaps back.
- Click `×` on the empty section → it disappears immediately (no confirm, since it had no content).
- Click `◉ Armed` toggle on the remaining section → label changes to `○ Disarmed`. Click again → back to `◉ Armed`.
- Type in the lyric textbox of the section → text accepts, multi-line works.

Close the window.

- [ ] **Step 4: Commit**

```powershell
git add src/SunoMetatagApp/MainWindow.xaml src/SunoMetatagApp/MainWindow.xaml.cs
git commit -m "feat: section stack with chip row, arm toggle (◉/○ + label), ▲/▼ reorder, delete (with confirm), +Add button"
```

---

## Task 13c: Tag picker (right column)

**Files:**
- Modify: `src/SunoMetatagApp/MainWindow.xaml`

- [ ] **Step 1: Replace the right-column placeholder**

In `MainWindow.xaml`, replace the entire right-column `<Grid Grid.Column="4" ...>...</Grid>` block with:

```xml
        <!-- Right: tag picker -->
        <Grid Grid.Column="4" Margin="6">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" /> <!-- Error banner -->
                <RowDefinition Height="Auto" /> <!-- Arm hint -->
                <RowDefinition Height="Auto" /> <!-- Search -->
                <RowDefinition Height="Auto" /> <!-- Category -->
                <RowDefinition Height="Auto" /> <!-- Broadcast scope badge (r2) -->
                <RowDefinition Height="*" />    <!-- Buttons -->
            </Grid.RowDefinitions>

            <!-- Error banner -->
            <Border Grid.Row="0"
                    Background="#FFE5E5" BorderBrush="#C00" BorderThickness="1"
                    Padding="6" Margin="0,0,0,6"
                    Visibility="{Binding LoadError, Converter={StaticResource NullToCollapsedConverter}}">
                <DockPanel>
                    <Button DockPanel.Dock="Right"
                            Content="Copy" Padding="6,2"
                            Click="CopyErrorButton_Click"
                            ToolTip="Copy error message to clipboard" />
                    <TextBlock Text="{Binding LoadError}"
                               Foreground="#900" TextWrapping="Wrap"
                               VerticalAlignment="Center" />
                </DockPanel>
            </Border>

            <!-- Arm hint -->
            <Border Grid.Row="1"
                    Background="#FFF8E0" BorderBrush="#C90" BorderThickness="1"
                    Padding="4" Margin="0,0,0,6">
                <TextBlock Text="No section armed — toggle ◉ on a section to receive tag clicks."
                           Foreground="#660" TextWrapping="Wrap" />
                <Border.Style>
                    <Style TargetType="Border">
                        <Setter Property="Visibility" Value="Collapsed" />
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding ShowArmHint}" Value="True">
                                <Setter Property="Visibility" Value="Visible" />
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </Border.Style>
            </Border>

            <!-- Search box -->
            <Grid Grid.Row="2">
                <TextBox x:Name="SearchBox"
                         Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                         Margin="0,0,0,4" Padding="4"
                         ToolTip="Search tags (case-insensitive)" />
                <TextBlock IsHitTestVisible="False"
                           Text="Search tags…"
                           Margin="6,0,0,0"
                           VerticalAlignment="Center"
                           Foreground="#999">
                    <TextBlock.Style>
                        <Style TargetType="TextBlock">
                            <Setter Property="Visibility" Value="Collapsed" />
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding Text, ElementName=SearchBox}" Value="">
                                    <Setter Property="Visibility" Value="Visible" />
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </TextBlock.Style>
                </TextBlock>
            </Grid>

            <!-- Category dropdown -->
            <ComboBox Grid.Row="3"
                      ItemsSource="{Binding Categories}"
                      SelectedItem="{Binding SelectedCategory}"
                      Margin="0,0,0,6" Padding="4"
                      ToolTip="Filter by category" />

            <!-- Broadcast scope badge (r2) -->
            <TextBlock Grid.Row="4"
                       Text="{Binding ArmedSectionCount, StringFormat='Will apply to {0} section(s)'}"
                       Foreground="#666" FontSize="11"
                       Margin="2,0,0,4">
                <TextBlock.Style>
                    <Style TargetType="TextBlock">
                        <Setter Property="Visibility" Value="Visible" />
                        <Style.Triggers>
                            <DataTrigger Binding="{Binding ArmedSectionCount}" Value="0">
                                <Setter Property="Visibility" Value="Collapsed" />
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </TextBlock.Style>
            </TextBlock>

            <!-- Scrollable button grid + empty-state -->
            <Grid Grid.Row="5">
                <ScrollViewer VerticalScrollBarVisibility="Auto"
                              HorizontalScrollBarVisibility="Disabled"
                              KeyboardNavigation.TabNavigation="Once"
                              KeyboardNavigation.DirectionalNavigation="Contained">
                    <ItemsControl ItemsSource="{Binding FilteredTags}">
                        <ItemsControl.ItemsPanel>
                            <ItemsPanelTemplate><WrapPanel Orientation="Horizontal" /></ItemsPanelTemplate>
                        </ItemsControl.ItemsPanel>
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Button Content="{Binding Bracket}"
                                        ToolTip="{Binding Description}"
                                        ToolTipService.IsEnabled="{Binding Description, Converter={StaticResource StringIsNotEmptyConverter}}"
                                        Margin="2" Padding="6,2"
                                        Command="{Binding DataContext.InsertTagCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                        CommandParameter="{Binding}" />
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>
                <TextBlock Text="No tags match"
                           HorizontalAlignment="Center" VerticalAlignment="Top"
                           Margin="0,12,0,0" Foreground="#666" IsHitTestVisible="False">
                    <TextBlock.Style>
                        <Style TargetType="TextBlock">
                            <Setter Property="Visibility" Value="Collapsed" />
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding FilteredTags.Count}" Value="0">
                                    <Setter Property="Visibility" Value="Visible" />
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </TextBlock.Style>
                </TextBlock>
            </Grid>
        </Grid>
```

- [ ] **Step 2: Add `CopyErrorButton_Click` handler**

In `MainWindow.xaml.cs`, append inside the `MainWindow` class:

```csharp
    private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && !string.IsNullOrEmpty(vm.LoadError))
        {
            Clipboard.SetText(vm.LoadError);
        }
    }
```

- [ ] **Step 3: Build, run, eyeball**

```powershell
dotnet build
dotnet run --project src/SunoMetatagApp
```

Expected: Right column shows the search box, category dropdown, the broadcast-scope badge ("Will apply to 1 section(s)" given the single armed default section), and ~115 wrapping tag buttons.

Manual checks:
- Type "ver" in search → grid narrows. Type "zzzzz" → "No tags match" placeholder visible.
- Disarm the section (toggle `◉ Armed` → `○ Disarmed`). Badge **disappears**. Click any tag → the yellow "No section armed…" hint appears.
- Re-arm the section. Badge **reappears** with "Will apply to 1 section(s)". Hint **disappears immediately**.
- Click "+ Add section" in the middle column → badge reads "Will apply to 2 section(s)". Click `[Verse]` → chip lands in **both** sections' chip rows.

Close the window.

- [ ] **Step 4: Commit**

```powershell
git add src/SunoMetatagApp/MainWindow.xaml src/SunoMetatagApp/MainWindow.xaml.cs
git commit -m "feat: tag picker pane with broadcast badge, search, category, button grid, error banner, arm hint, empty state"
```

---

## Task 13d: Preview pane (left column) + Copy button wiring

**Files:**
- Modify: `src/SunoMetatagApp/MainWindow.xaml`
- Modify: `src/SunoMetatagApp/MainWindow.xaml.cs`

- [ ] **Step 1: Replace the left-column placeholder**

In `MainWindow.xaml`, replace the entire left-column `<Grid Grid.Column="0" ...>...</Grid>` block with:

```xml
        <!-- Left: preview pane -->
        <DockPanel Grid.Column="0" Margin="6">
            <Button DockPanel.Dock="Top"
                    Content="Copy all"
                    Padding="8,4" Margin="0,0,0,6"
                    HorizontalAlignment="Stretch"
                    Command="{Binding CopyPreviewCommand}"
                    ToolTip="Copy the assembled prompt to the clipboard" />

            <TextBox Text="{Binding PreviewText, Mode=OneWay}"
                     IsReadOnly="True"
                     AcceptsReturn="True"
                     TextWrapping="Wrap"
                     VerticalScrollBarVisibility="Auto"
                     FontFamily="Consolas" FontSize="12"
                     Padding="6"
                     Background="#FAFAFA" />
        </DockPanel>
```

- [ ] **Step 2: Subscribe to `CopyRequested`, add initial-focus walker (r2)**

In `MainWindow.xaml.cs`, replace the entire class body:

```csharp
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnWindowLoaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
            oldVm.CopyRequested -= OnCopyRequested;
        if (e.NewValue is MainViewModel newVm)
            newVm.CopyRequested += OnCopyRequested;
    }

    private void OnCopyRequested(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            Clipboard.SetText(vm.PreviewText ?? string.Empty);
        }
    }

    // r2 — initial focus walker.
    // Per spec §5.1: defer to DispatcherPriority.Loaded so item containers exist,
    // then walk the visual tree from SectionsHost to the first section's lyric TextBox.
    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() =>
            {
                var firstLyric = FindFirstLyricTextBox();
                firstLyric?.Focus();
            }));
    }

    private TextBox? FindFirstLyricTextBox()
    {
        if (SectionsHost.Items.Count == 0) return null;
        var container = SectionsHost.ItemContainerGenerator.ContainerFromIndex(0) as DependencyObject;
        if (container is null) return null;
        return FindLyricsTextBox(container);
    }

    private static TextBox? FindLyricsTextBox(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBox tb)
            {
                var expr = tb.GetBindingExpression(TextBox.TextProperty);
                if (expr?.ParentBinding.Path?.Path == "Lyrics")
                    return tb;
            }
            var found = FindLyricsTextBox(child);
            if (found is not null) return found;
        }
        return null;
    }

    private void DeleteSectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not Section section) return;
        if (DataContext is not MainViewModel vm) return;

        bool hasContent = section.Tags.Count > 0 || !string.IsNullOrEmpty(section.Lyrics);
        if (hasContent)
        {
            var result = MessageBox.Show(
                "Delete this section? Its tags and lyrics will be lost.",
                "Confirm delete",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.OK) return;
        }

        vm.RemoveSectionCommand.Execute(section);
    }

    private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && !string.IsNullOrEmpty(vm.LoadError))
        {
            Clipboard.SetText(vm.LoadError);
        }
    }
}
```

- [ ] **Step 3: Build, run, full smoke test**

```powershell
dotnet build
dotnet run --project src/SunoMetatagApp
```

Manual smoke test (all 11 cases must pass — r2 adds cases 0, 9, 10):

0. **Initial focus (r2).** On launch, **the caret is blinking in the first section's lyric textbox**. Start typing immediately without any click — text lands in that textbox.
1. **Single section, tags + lyrics.** With the default armed section, type a couple of lyric lines, then click `[Verse]`. The preview pane should immediately show `[Verse]` on its own line above the lyric text.
2. **Multiple tags in one section, preserve order.** Click `[Guitar]` then `[Powerful]`. Preview shows `[Guitar]` then `[Powerful]` then your lyric. Chip row shows `[Guitar◀▶✕][Powerful◀▶✕]` left-to-right.
3. **Chip reorder.** Click `▶` on the `[Guitar]` chip → it swaps with `[Powerful]`. Preview updates to `[Powerful]` then `[Guitar]` then lyric.
4. **Chip remove.** Click `✕` on the `[Powerful]` chip → it disappears, preview drops that line.
5. **Multi-section, multi-arm.** Click "+ Add section". Right pane badge updates to "Will apply to 2 section(s)". Type "v1" in section 1's textbox, "v2" in section 2's textbox. Click `[Chorus]` → chip lands in both chip rows; preview shows two sections separated by one blank line, each with `[Chorus]` above its lyric.
6. **Disarm one, click tag.** Disarm section 2 (toggle `◉ Armed` → `○ Disarmed`). Badge updates to "Will apply to 1 section(s)". Click `[Bridge]` → chip lands only in section 1. Preview updates accordingly.
7. **No armed sections.** Disarm both sections. Badge **disappears entirely**. Click any tag → yellow "no section armed" hint appears in the right pane; no tags added; preview unchanged.
8. **Hint clears on rearm (r2).** With the hint still showing, re-arm section 1. Hint **disappears immediately** (no timer wait). Badge reappears with "Will apply to 1 section(s)".
9. **Section reorder (r2).** Type distinct content in sections 1 and 2 so preview shows their order. Click `▼` on section 1. Sections swap; preview reorders to match. Click `▲` on it → swaps back.
10. **Copy all + delete confirm.** Click "Copy all" → preview text is on the clipboard. Paste into a text editor to verify. Click `×` on a section that has content → modal confirm appears; OK removes (preview updates); Cancel keeps it.

Close the window.

- [ ] **Step 4: Commit**

```powershell
git add src/SunoMetatagApp/MainWindow.xaml src/SunoMetatagApp/MainWindow.xaml.cs
git commit -m "feat: preview pane with live render and Copy-all clipboard support"
```

---

## Task 14: Verify error-banner path

- [ ] **Step 1: Rename tags.json in build output to simulate missing file**

```powershell
dotnet build
Move-Item src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json `
          src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json.bak
```

- [ ] **Step 2: Run and verify the banner**

```powershell
dotnet run --project src/SunoMetatagApp --no-build
```

Expected:
- Window opens, **red error banner** at top of right pane reads "tags.json not found at '…\tags.json'.".
- The button grid is empty.
- **The section stack is empty** (no starter section; load-error ctor doesn't create one). The preview pane is empty. The "+ Add section" button is still there and works — clicking it adds a section that can be edited normally (just no tag picker).
- The "Copy" button on the error banner copies the message to the clipboard.

Close the window.

- [ ] **Step 3: Restore the file**

```powershell
Move-Item src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json.bak `
          src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json
```

- [ ] **Step 4: No commit (no source changes).**

---

## Task 15: Seed `docs/BACKLOG.md`

**Files:**
- Create: `j:\SunoMetatagApp\docs\BACKLOG.md`

- [ ] **Step 1: Write the backlog**

Write `j:\SunoMetatagApp\docs\BACKLOG.md`:
```markdown
# SunoMetatagApp Backlog

Open work for v2+. v1 scope is locked by [`specs/2026-05-25-suno-metatag-section-editor-design.md`](specs/2026-05-25-suno-metatag-section-editor-design.md).

Roughly prioritized — top first.

## B-001 — Favorites / recently-used tags
**Status:** open · **Priority:** high
**Acceptance:** A "Pinned" group appears at the top of the tag picker; right-click a tag → Pin. Persisted to `%APPDATA%\SunoMetatagApp\favorites.json`.

## B-002 — Dark theme
**Status:** open · **Priority:** medium
**Acceptance:** Light/dark toggle in a small settings menu. Choice persisted. All controls re-themed.

## B-003 — Drag-and-drop reorder (chips and sections)
**Status:** open · **Priority:** medium
**Acceptance:** Drag a chip to a different position within its section's row, or to a different section's row. Drag a section's toolbar handle to reorder sections. v1 uses ◀/▶ on chips and add-order for sections.

## B-004 — Hotkeys
**Status:** open · **Priority:** medium
**Acceptance:** `Ctrl+N` adds a section. `Ctrl+A` arms all sections. `Ctrl+D` disarms all. `Ctrl+C` (when preview focused) copies all.

## B-005 — Persist prompt across launches
**Status:** open · **Priority:** medium
**Acceptance:** Sections + tags + lyrics serialized to `%APPDATA%\SunoMetatagApp\last-session.json` on close, restored on open. "Clear" button wipes everything.

## B-006 — *(retired — section reorder ships in v1 per spec §5.3)*

## B-007 — Reload `tags.json` without restart
**Status:** open · **Priority:** low
**Acceptance:** "Reload tags" action in a menu re-reads `tags.json`; updates the picker live; surfaces errors via the same banner.

## B-008 — Tag aliases / synonyms
**Status:** open · **Priority:** low
**Acceptance:** Optional `aliases: [...]` field on tags; search matches against label, bracket, and aliases.

## B-009 — Section type field
**Status:** open · **Priority:** low
**Acceptance:** Optional per-section "type" dropdown (Verse, Chorus, Bridge, …) separate from the tag chips. Auto-emits the matching tag in the preview.

## B-010 — Per-section "add tag" inline shortcut
**Status:** open · **Priority:** low
**Acceptance:** Type a tag name directly into a section's chip row (with autocomplete) instead of going to the right-pane picker.

## B-011 — Virtualize tag panel
**Status:** open · **Priority:** medium (trigger-based)
**Acceptance:** Tag panel uses a virtualizing host and stays smooth at 500+ tags. Trigger: tag count > 300 OR user-reported lag.

## B-012 — Chip-row hover affordances
**Status:** open · **Priority:** low
**Acceptance:** ◀/▶/× on chips appear only on hover; cleaner default look. v1 shows them always for discoverability.

## B-013 — Tag button visual treatment
**Status:** open · **Priority:** medium
**Acceptance:** `Style x:Key="TagButtonStyle"` replaces default WPF chrome with a flat, subtle hover style.

## B-014 — Screen-reader naming
**Status:** open · **Priority:** low
**Acceptance:** `AutomationProperties.Name` set across all interactive controls.

## B-015 — Persist splitter positions / column widths
**Status:** open · **Priority:** low
**Acceptance:** Saved to `%APPDATA%\SunoMetatagApp\layout.json`; restored on launch.

## B-016 — Permanent "no armed sections" hint
**Status:** open · **Priority:** low
**Acceptance:** When zero sections armed, a subtle persistent indicator near the tag picker; current v1 only flashes briefly.

## B-017 — Auto-update `tags.json` from URL
**Status:** open · **Priority:** low
**Acceptance:** "Check for tag updates" pulls from a configured URL, shows a diff preview, confirms, then writes the merged file. Backup the prior file.

## B-018 — Retry `musci.io/blog/suno-tags` as a seed source
**Status:** open · **Priority:** low
**Acceptance:** When the URL becomes reachable (currently 500s), re-run seeding and merge into `tags.json`.

## B-019 — Tag chip drag-to-different-section
**Status:** open · **Priority:** low
**Acceptance:** Drag a chip from section A's row to section B's row → tag moves between sections.

## B-020 — Debounce preview recompute (r2)
**Status:** open · **Priority:** low
**Source:** r3 FRONTEND/UX advisory (MEDIUM)
**Acceptance:** `RecomputePreview` runs at most once per ~50ms via `DispatcherTimer` debounce. Trigger to ship: user reports keystroke lag in very long lyrics (~10KB+).

## B-021 — Inline delete-section confirm (r2)
**Status:** open · **Priority:** low
**Source:** r3 FRONTEND/UX advisory (LOW)
**Acceptance:** Replace the modal `MessageBox` with an inline `×` → "Delete?" two-click confirm on the section toolbar. Cleaner, testable.

## B-022 — Preview pane cursor styling (r2)
**Status:** open · **Priority:** trivial
**Source:** r3 FRONTEND/UX advisory (LOW)
**Acceptance:** The read-only preview TextBox uses `Cursor="Arrow"` until the user starts selecting, then transitions to I-beam during selection only. Eliminates the "this looks editable" affordance.
```

- [ ] **Step 2: Commit**

```powershell
git add docs/BACKLOG.md
git commit -m "docs: seed BACKLOG.md from section-editor design out-of-scope list"
```

---

## Task 16: Publish, verify single-file exe

- [ ] **Step 1: Final test pass**

```powershell
dotnet test
```
Expected: PASS, 52 tests.

- [ ] **Step 2: Publish**

```powershell
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

Verify:
```powershell
Test-Path publish/SunoMetatagApp.exe
Test-Path publish/tags.json
```
Expected: `True`, `True`.

- [ ] **Step 3: Smoke test the published exe**

Double-click `publish\SunoMetatagApp.exe`.

Re-run all 8 manual cases from Task 13d Step 3. All should pass on the published exe just as they did under `dotnet run`. Pay special attention to:
- Clipboard works (Copy all + Copy on error banner).
- Section delete confirm dialog renders.
- Live preview updates on every keystroke and every tag/chip action.

Close the window.

- [ ] **Step 4: Final commit (if README needs notes)**

If you discovered any quirk worth noting in the README, add it and commit:
```powershell
git add README.md
git commit -m "docs: note publish quirks discovered during smoke test"
```

Otherwise no commit. Done.

---

## Done criteria

- [ ] `dotnet test` passes with **52 tests** (5 + 7 + 8 + 10 + 22).
- [ ] `dotnet run --project src/SunoMetatagApp` opens the working app with one armed section visible, **caret already in the lyric textbox**.
- [ ] All **11 manual smoke-test cases** from Task 13d Step 3 pass.
- [ ] Error-banner path (Task 14) verified.
- [ ] `publish/SunoMetatagApp.exe` runs as a standalone self-contained app.
- [ ] `docs/BACKLOG.md` lists **21 v2+ items** (B-001..B-022 with B-006 retired).
- [ ] Git history shows ~16 conventional-commit commits, one per task.
- [ ] No file named `InsertionRules.cs` or `InsertionRulesTests.cs` exists anywhere in the repo.
