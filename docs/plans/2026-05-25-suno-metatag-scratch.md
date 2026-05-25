> [!warning] **DEPRECATED — superseded 2026-05-25.**
> This plan implemented the "scratch" caret-editor design, which has been retired in favor of the section-editor model. The active plan is [`2026-05-25-suno-metatag-section-editor.md`](2026-05-25-suno-metatag-section-editor.md). **Do not execute the tasks below.** This file is retained for history.

# Suno Metatag Scratch — Implementation Plan (deprecated)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a single-window WPF desktop utility that lets the user paste/edit lyric text and insert Suno metatags (e.g., `[Verse]`, `[Chorus]`) at the cursor with one click from a searchable, categorized side panel.

**Architecture:** Light MVVM. Pure-logic core (`TagService`, `InsertionRules`) is fully unit-tested; the only View-coupled code is a tiny code-behind in `MainWindow.xaml.cs` that handles `TextBox.CaretIndex`. Tag list lives in a bundled `tags.json`. No DI container, no persistence, no file I/O on the lyric buffer.

**Tech Stack:** WPF, .NET 8, C# 12, `CommunityToolkit.Mvvm` (NuGet), `System.Text.Json` (BCL), xUnit (tests).

**Reference spec:** `docs/specs/2026-05-25-suno-metatag-scratch-design.md` (in this repo). **Spec is at r2** — implementation plan revised in-place to honor §5.1 (UX behavior decisions), §6.2 (tooltip null-handling), and §8.1 (post-insertion caret advancement).

**Prerequisites:** .NET 8 SDK installed (`dotnet --version` shows 8.x). PowerShell on Windows for shell commands. Git installed.

---

## Notes for the implementer

- **All commands assume CWD = `j:\SunoMetatagApp\`** unless stated otherwise.
- **Test framework:** Plain xUnit, no FluentAssertions. The spec mentioned FluentAssertions but plain xUnit is sufficient for this scope and avoids the v8+ licensing change.
- **Line endings:** WPF `TextBox` uses `\r\n` on Windows. `BuildInsertion` takes a `newline` parameter so tests can use `"\n"` and the View passes `Environment.NewLine`. Detection of "preceded by newline" checks for `'\n'` at `caret-1` (works for both `\n` and `\r\n` since both end in `\n`). Detection of "followed by newline" checks for `'\r'` or `'\n'` at `caret`.
- **Commit style:** Conventional commits (`feat:`, `test:`, `chore:`, `docs:`). One commit per task unless a task explicitly says otherwise.

## What changed in r2 (2026-05-25)

The r1 plan was reviewed and returned `NEEDS_REVISION` by the Lead Reviewer with a FRONTEND/UX advisory verdict of `ADVISORY_NEEDS_REVISION`. This plan now incorporates the spec's r2 UX decisions (§5.1, §6.2, §8.1):

- **Task 9** — seed `tags.json` now populates `description` for ambiguous tags. Added a `StringIsNotEmptyConverter` (Task 11b Step 2.5) to suppress empty tooltips.
- **Task 11a** — `GridSplitter` widened to 6px with `Cursor="SizeWE"`. Window now sets `FocusManager.FocusedElement` to put initial focus in the editor.
- **Task 11b** — `ScrollViewer` sets `KeyboardNavigation.TabNavigation="Once"` and `DirectionalNavigation="Contained"`. New "No tags match" placeholder added under the button grid. Tag button's `ToolTipService.IsEnabled` is data-bound through `StringIsNotEmptyConverter`.
- **Task 12** — post-insertion caret math now advances past the immediately-following `\r` and/or `\n` per spec §8.1. Added a 7th manual smoke case for "caret at end of mid-document line."
- **Task 14** — BACKLOG seeded with 17 items (B-001..B-017) instead of 10. New items: B-011 virtualization, B-012 filter cost, B-013 button visuals, B-014 screen-reader names, B-015 clear-search, B-016 splitter persistence, B-017 category tooltip.

No new tasks added; no task numbering changes. Task code blocks edited in place.

---

## Task 0: Repo bootstrap

**Files:**
- Create: `j:\SunoMetatagApp\.gitignore`
- Create: `j:\SunoMetatagApp\README.md`

- [ ] **Step 1: Initialize git repo**

Run (from `j:\SunoMetatagApp\`):
```powershell
git init
git config user.email "wolfen231@gmail.com"
git config user.name "Jason Spencer"
```
Expected: `Initialized empty Git repository in j:/SunoMetatagApp/.git/`

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

- [ ] **Step 3: Create initial `README.md`**

Write `j:\SunoMetatagApp\README.md`:
````markdown
# Suno Metatag Scratch

A single-window WPF utility for assembling Suno AI prompt lyrics. Paste lyrics in the left pane; click metatag buttons in the right pane to insert Suno tags (`[Verse]`, `[Chorus]`, …) at the cursor.

Temporary scratch space — no save/load.

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

`tags.json` ships next to the exe. Edit it to add, remove, or rename tags. The app reads the file once at startup; restart the app to pick up changes.

## Design

See [`docs/specs/2026-05-25-suno-metatag-scratch-design.md`](docs/specs/2026-05-25-suno-metatag-scratch-design.md).
````

- [ ] **Step 4: Commit**

```powershell
git add .gitignore README.md docs/
git commit -m "chore: initial repo scaffolding with design spec"
```

Expected: A commit containing `.gitignore`, `README.md`, and the pre-existing `docs/specs/2026-05-25-suno-metatag-scratch-design.md`.

---

## Task 1: Solution and project scaffolding

**Files:**
- Create: `j:\SunoMetatagApp\SunoMetatagApp.sln`
- Create: `j:\SunoMetatagApp\src\SunoMetatagApp\SunoMetatagApp.csproj`
- Create: `j:\SunoMetatagApp\tests\SunoMetatagApp.Tests\SunoMetatagApp.Tests.csproj`

- [ ] **Step 1: Create the solution and projects via `dotnet new`**

```powershell
dotnet new sln -n SunoMetatagApp
dotnet new wpf -o src/SunoMetatagApp -n SunoMetatagApp -f net8.0
dotnet new xunit -o tests/SunoMetatagApp.Tests -n SunoMetatagApp.Tests -f net8.0
dotnet sln add src/SunoMetatagApp/SunoMetatagApp.csproj
dotnet sln add tests/SunoMetatagApp.Tests/SunoMetatagApp.Tests.csproj
dotnet add tests/SunoMetatagApp.Tests reference src/SunoMetatagApp
```

- [ ] **Step 2: Add `CommunityToolkit.Mvvm` to the main project**

```powershell
dotnet add src/SunoMetatagApp package CommunityToolkit.Mvvm
```

- [ ] **Step 3: Delete the xUnit default placeholder test file**

```powershell
Remove-Item tests/SunoMetatagApp.Tests/UnitTest1.cs -ErrorAction SilentlyContinue
```

- [ ] **Step 4: Verify build and tests run**

```powershell
dotnet build
dotnet test
```
Expected: `Build succeeded`; `Passed!  - Failed: 0, Passed: 0, Skipped: 0` (zero tests is fine — file deleted).

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

Write `src/SunoMetatagApp/Models/TagDefinition.cs`:
```csharp
namespace SunoMetatagApp.Models;

public sealed record TagDefinition(
    string Category,
    string Label,
    string Bracket,
    string? Description = null);
```

- [ ] **Step 2: Verify it compiles**

```powershell
dotnet build
```
Expected: `Build succeeded`.

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

- [ ] **Step 1: Write the failing tests**

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
              { "category": "Structure", "label": "Verse",  "bracket": "[Verse]" },
              { "category": "Vocal",     "label": "Whisper","bracket": "[Whispered]", "description": "Soft." }
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
        // Missing "bracket"
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

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test
```
Expected: FAIL — `TagService` and `TagLoadException` do not exist yet.

- [ ] **Step 3: Implement `TagService.LoadAll` and `TagLoadException`**

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
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            throw new TagLoadException($"Could not read tags.json at '{path}': {ex.Message}", ex);
        }

        List<TagDto>? dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<TagDto>>(json, JsonOpts);
        }
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
        [JsonPropertyName("category")]  public string? Category { get; set; }
        [JsonPropertyName("label")]     public string? Label { get; set; }
        [JsonPropertyName("bracket")]   public string? Bracket { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

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

**Files:**
- Modify: `src/SunoMetatagApp/Services/TagService.cs`
- Modify: `tests/SunoMetatagApp.Tests/TagServiceTests.cs`

- [ ] **Step 1: Add the failing test**

Append to `tests/SunoMetatagApp.Tests/TagServiceTests.cs`, inside the existing `TagServiceTests` class:
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

- [ ] **Step 2: Run tests to verify the new one fails**

```powershell
dotnet test
```
Expected: FAIL — `DistinctCategories` is not defined.

- [ ] **Step 3: Implement `DistinctCategories`**

Add inside `TagService` class in `src/SunoMetatagApp/Services/TagService.cs` (after `LoadAll`):
```csharp
    public static IReadOnlyList<string> DistinctCategories(IEnumerable<TagDefinition> tags) =>
        tags.Select(t => t.Category)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToList();
```

- [ ] **Step 4: Run tests**

```powershell
dotnet test
```
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

- [ ] **Step 1: Write the failing tests**

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

    [Fact]
    public void Filter_AllCategory_EmptySearch_ReturnsEverything()
    {
        var result = TagService.Filter(Sample, search: null, category: "All").ToList();
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Filter_NullCategory_EmptySearch_ReturnsEverything()
    {
        var result = TagService.Filter(Sample, search: "", category: null).ToList();
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Filter_SpecificCategory_ReturnsOnlyThatCategory()
    {
        var result = TagService.Filter(Sample, search: null, category: "Vocal").ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal("Vocal", t.Category));
    }

    [Fact]
    public void Filter_Search_MatchesLabel_CaseInsensitive()
    {
        var result = TagService.Filter(Sample, search: "whisp", category: "All").ToList();
        Assert.Single(result);
        Assert.Equal("Whisper", result[0].Label);
    }

    [Fact]
    public void Filter_Search_MatchesBracket_CaseInsensitive()
    {
        // Typing "[ver" should find "[Verse]" via the bracket field.
        var result = TagService.Filter(Sample, search: "[VER", category: "All").ToList();
        Assert.Single(result);
        Assert.Equal("[Verse]", result[0].Bracket);
    }

    [Fact]
    public void Filter_CategoryAndSearch_AreAndCombined()
    {
        // "v" matches Verse (label) and Whisper (no), Belt (no — no v). Should match Verse from Structure only when category=Structure.
        var result = TagService.Filter(Sample, search: "v", category: "Structure").ToList();
        Assert.Single(result);
        Assert.Equal("Verse", result[0].Label);
    }

    [Fact]
    public void Filter_EmptyResults_DoesNotThrow()
    {
        var result = TagService.Filter(Sample, search: "zzzzzzzz", category: "All").ToList();
        Assert.Empty(result);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test
```
Expected: FAIL — `Filter` is not defined.

- [ ] **Step 3: Implement `Filter`**

Add inside `TagService` class in `src/SunoMetatagApp/Services/TagService.cs` (after `DistinctCategories`):
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

```powershell
dotnet test
```
Expected: PASS, 12 tests total (5 from before + 7 new).

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/Services/TagService.cs tests/SunoMetatagApp.Tests/TagServiceFilterTests.cs
git commit -m "feat: TagService.Filter with category+search AND semantics"
```

---

## Task 6: `InsertionRules.BuildInsertion` (TDD)

**Files:**
- Create: `tests/SunoMetatagApp.Tests/InsertionRulesTests.cs`
- Create: `src/SunoMetatagApp/Services/InsertionRules.cs`

- [ ] **Step 1: Write the failing tests**

Write `tests/SunoMetatagApp.Tests/InsertionRulesTests.cs`:
```csharp
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

public class InsertionRulesTests
{
    // All tests pass "\n" as the newline so assertions stay portable.
    private const string NL = "\n";

    [Fact]
    public void Insertion_EmptyDocument_InsertsBracketOnly()
    {
        var s = InsertionRules.BuildInsertion("", 0, "[Verse]", NL);
        Assert.Equal("[Verse]", s);
    }

    [Fact]
    public void Insertion_AtStartOfDocument_NoLeadingNewline()
    {
        var s = InsertionRules.BuildInsertion("hello", 0, "[Verse]", NL);
        Assert.Equal("[Verse]\n", s);
    }

    [Fact]
    public void Insertion_AtEndOfDocument_NoTrailingNewline()
    {
        var s = InsertionRules.BuildInsertion("hello", 5, "[Verse]", NL);
        Assert.Equal("\n[Verse]", s);
    }

    [Fact]
    public void Insertion_AtStartOfLine_NoLeadingNewline()
    {
        // Caret right after the first '\n' — start of line 2.
        var s = InsertionRules.BuildInsertion("hello\nworld", 6, "[Verse]", NL);
        Assert.Equal("[Verse]\n", s);
    }

    [Fact]
    public void Insertion_AtEndOfLine_NoTrailingNewline()
    {
        // Caret at index 5 (between "hello" and the '\n' that follows).
        var s = InsertionRules.BuildInsertion("hello\nworld", 5, "[Verse]", NL);
        Assert.Equal("\n[Verse]", s);
    }

    [Fact]
    public void Insertion_MidLine_AddsBothNewlines()
    {
        // Caret in the middle of "hello".
        var s = InsertionRules.BuildInsertion("hello", 2, "[Verse]", NL);
        Assert.Equal("\n[Verse]\n", s);
    }

    [Fact]
    public void Insertion_OnBlankLineBetweenContent_OmitsBoth()
    {
        // "a\n\nb": caret at index 2 (on the blank line, between two newlines).
        var s = InsertionRules.BuildInsertion("a\n\nb", 2, "[Verse]", NL);
        Assert.Equal("[Verse]", s);
    }

    [Fact]
    public void Insertion_HandlesCrLf_OnWindowsStyleText()
    {
        // "a\r\nb": caret at index 3 (start of "b"). Preceded by '\n', so leading omitted.
        var s = InsertionRules.BuildInsertion("a\r\nb", 3, "[Verse]", "\r\n");
        Assert.Equal("[Verse]\r\n", s);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test
```
Expected: FAIL — `InsertionRules` is not defined.

- [ ] **Step 3: Implement `BuildInsertion`**

Write `src/SunoMetatagApp/Services/InsertionRules.cs`:
```csharp
namespace SunoMetatagApp.Services;

public static class InsertionRules
{
    /// <summary>
    /// Build the exact text to splice into <paramref name="fullText"/> at <paramref name="caret"/>
    /// so that <paramref name="bracket"/> ends up on its own line, with newlines omitted where the
    /// surrounding text already provides a line break (or where caret is at a document boundary).
    /// </summary>
    public static string BuildInsertion(string fullText, int caret, string bracket, string newline)
    {
        bool atStart = caret <= 0;
        bool atEnd   = caret >= fullText.Length;

        // Preceded by a newline: works for both "\n" and "\r\n" because both end in '\n'.
        bool precededByNewline = !atStart && fullText[caret - 1] == '\n';

        // Followed by a newline: accept '\n' or '\r' so that on Windows ("\r\n") we still detect the line break.
        bool followedByNewline = !atEnd && (fullText[caret] == '\n' || fullText[caret] == '\r');

        bool omitLeading  = atStart || precededByNewline;
        bool omitTrailing = atEnd   || followedByNewline;

        string leading  = omitLeading  ? "" : newline;
        string trailing = omitTrailing ? "" : newline;
        return leading + bracket + trailing;
    }
}
```

- [ ] **Step 4: Run tests**

```powershell
dotnet test
```
Expected: PASS, 20 tests total.

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/Services/InsertionRules.cs tests/SunoMetatagApp.Tests/InsertionRulesTests.cs
git commit -m "feat: InsertionRules.BuildInsertion with boundary and CRLF handling"
```

---

## Task 7: `TagViewModel` (no tests — trivial wrapper)

**Files:**
- Create: `src/SunoMetatagApp/ViewModels/TagViewModel.cs`

- [ ] **Step 1: Write the wrapper**

Write `src/SunoMetatagApp/ViewModels/TagViewModel.cs`:
```csharp
using SunoMetatagApp.Models;

namespace SunoMetatagApp.ViewModels;

public sealed class TagViewModel
{
    public TagViewModel(TagDefinition definition)
    {
        Definition = definition;
    }

    public TagDefinition Definition { get; }

    public string Label       => Definition.Label;
    public string Bracket     => Definition.Bracket;
    public string Category    => Definition.Category;
    public string? Description => Definition.Description;
}
```

- [ ] **Step 2: Verify build**

```powershell
dotnet build
```
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```powershell
git add src/SunoMetatagApp/ViewModels/TagViewModel.cs
git commit -m "feat: add TagViewModel binding wrapper"
```

---

## Task 8: `MainViewModel` (TDD on filter refresh)

**Files:**
- Create: `tests/SunoMetatagApp.Tests/MainViewModelTests.cs`
- Create: `src/SunoMetatagApp/ViewModels/MainViewModel.cs`

- [ ] **Step 1: Write the failing tests**

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
    public void Ctor_FilteredTags_StartsWithEverything()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal(3, vm.FilteredTags.Count);
    }

    [Fact]
    public void ChangingSelectedCategory_RecomputesFilteredTags()
    {
        var vm = new MainViewModel(Sample);
        vm.SelectedCategory = "Structure";
        Assert.Equal(2, vm.FilteredTags.Count);
        Assert.All(vm.FilteredTags, t => Assert.Equal("Structure", t.Category));
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
    public void InsertTagCommand_RaisesInsertRequestedWithDefinition()
    {
        var vm = new MainViewModel(Sample);
        TagDefinition? captured = null;
        vm.InsertRequested += (_, def) => captured = def;

        var tagVm = vm.FilteredTags.First(t => t.Label == "Verse");
        vm.InsertTagCommand.Execute(tagVm);

        Assert.NotNull(captured);
        Assert.Equal("[Verse]", captured!.Bracket);
    }

    [Fact]
    public void LoadError_NullByDefault()
    {
        var vm = new MainViewModel(Sample);
        Assert.Null(vm.LoadError);
    }

    [Fact]
    public void LoadErrorCtor_SetsLoadErrorAndEmptyCategories()
    {
        var vm = new MainViewModel(loadError: "tags.json not found.");
        Assert.Equal("tags.json not found.", vm.LoadError);
        Assert.Equal(new[] { "All" }, vm.Categories);
        Assert.Empty(vm.FilteredTags);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test
```
Expected: FAIL — `MainViewModel` is not defined.

- [ ] **Step 3: Implement `MainViewModel`**

Write `src/SunoMetatagApp/ViewModels/MainViewModel.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;

namespace SunoMetatagApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<TagDefinition> _allTags;

    public MainViewModel(IReadOnlyList<TagDefinition> tags)
    {
        _allTags = tags;
        Categories = BuildCategories(tags);
        SelectedCategory = "All";
        FilteredTags = ComputeFiltered();
    }

    /// <summary>Construct a degraded view-model used when tags.json failed to load.</summary>
    public MainViewModel(string loadError)
    {
        _allTags = System.Array.Empty<TagDefinition>();
        Categories = new[] { "All" };
        SelectedCategory = "All";
        FilteredTags = System.Array.Empty<TagViewModel>();
        LoadError = loadError;
    }

    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private string _lyricText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<TagViewModel> _filteredTags = System.Array.Empty<TagViewModel>();

    [ObservableProperty]
    private string? _loadError;

    partial void OnSearchTextChanged(string value)        => FilteredTags = ComputeFiltered();
    partial void OnSelectedCategoryChanged(string value)  => FilteredTags = ComputeFiltered();

    public event EventHandler<TagDefinition>? InsertRequested;

    [RelayCommand]
    private void InsertTag(TagViewModel? tag)
    {
        if (tag is null) return;
        InsertRequested?.Invoke(this, tag.Definition);
    }

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

- [ ] **Step 4: Run tests**

```powershell
dotnet test
```
Expected: PASS, 27 tests total.

- [ ] **Step 5: Commit**

```powershell
git add src/SunoMetatagApp/ViewModels/MainViewModel.cs tests/SunoMetatagApp.Tests/MainViewModelTests.cs
git commit -m "feat: add MainViewModel with reactive filtering and insert command"
```

---

## Task 9: Seed initial `tags.json`

**Files:**
- Create: `src/SunoMetatagApp/Resources/tags.json`
- Modify: `src/SunoMetatagApp/SunoMetatagApp.csproj`

This task seeds a curated starter set drawn from `hookgenius.app/learn/suno-metatags-complete-list/` and `openmusicprompt.com/blog/suno-ai-metatags-guide`, deduped and mapped to the spec's starter categories. Aim for ~100 tags; user can hand-extend to 500+.

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

Open `src/SunoMetatagApp/SunoMetatagApp.csproj` and add this `<ItemGroup>` block immediately before the closing `</Project>` tag:
```xml
  <ItemGroup>
    <None Update="Resources\tags.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
      <Link>tags.json</Link>
    </None>
  </ItemGroup>
```

The `<Link>` rename means the file lands directly as `tags.json` in the output, not as `Resources\tags.json`.

- [ ] **Step 3: Build and verify the file is copied**

```powershell
dotnet build
Test-Path src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json
```
Expected: `True`.

- [ ] **Step 4: Commit**

```powershell
git add src/SunoMetatagApp/Resources/tags.json src/SunoMetatagApp/SunoMetatagApp.csproj
git commit -m "feat: bundle starter tags.json (~115 tags) with CopyToOutput"
```

---

## Task 10: `App.xaml` startup wiring

**Files:**
- Modify: `src/SunoMetatagApp/App.xaml`
- Modify: `src/SunoMetatagApp/App.xaml.cs`

The default WPF template's `App.xaml` sets `StartupUri="MainWindow.xaml"`. We override `OnStartup` to construct the ViewModel from `TagService` and assign it as the window's `DataContext`. If loading fails we use the degraded `MainViewModel(loadError)` constructor so the window still opens with an error banner.

- [ ] **Step 1: Replace `App.xaml`**

Replace contents of `src/SunoMetatagApp/App.xaml`:
```xml
<Application x:Class="SunoMetatagApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:SunoMetatagApp">
    <Application.Resources>
    </Application.Resources>
</Application>
```

Note: no `StartupUri` — we construct the window in code.

- [ ] **Step 2: Replace `App.xaml.cs`**

Replace contents of `src/SunoMetatagApp/App.xaml.cs`:
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

- [ ] **Step 3: Verify build**

```powershell
dotnet build
```
Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```powershell
git add src/SunoMetatagApp/App.xaml src/SunoMetatagApp/App.xaml.cs
git commit -m "feat: wire App.OnStartup to load tags and construct MainViewModel"
```

---

## Task 11a: `MainWindow.xaml` layout — left pane (editor) and grid skeleton

**Files:**
- Modify: `src/SunoMetatagApp/MainWindow.xaml`

- [ ] **Step 1: Replace `MainWindow.xaml`**

Replace contents of `src/SunoMetatagApp/MainWindow.xaml`:
```xml
<Window x:Class="SunoMetatagApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:SunoMetatagApp"
        xmlns:vm="clr-namespace:SunoMetatagApp.ViewModels"
        Title="Suno Metatag Scratch"
        Width="1100" Height="700"
        MinWidth="700" MinHeight="450"
        WindowStartupLocation="CenterScreen"
        FocusManager.FocusedElement="{Binding ElementName=LyricEditor}"
        d:DataContext="{d:DesignInstance Type=vm:MainViewModel}"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="7*" MinWidth="320" />
            <ColumnDefinition Width="6" />
            <ColumnDefinition Width="3*" MinWidth="280" />
        </Grid.ColumnDefinitions>

        <!-- Left pane: lyric editor -->
        <TextBox Grid.Column="0"
                 x:Name="LyricEditor"
                 Text="{Binding LyricText, UpdateSourceTrigger=PropertyChanged}"
                 AcceptsReturn="True"
                 AcceptsTab="True"
                 TextWrapping="Wrap"
                 VerticalScrollBarVisibility="Auto"
                 HorizontalScrollBarVisibility="Auto"
                 FontFamily="Consolas"
                 FontSize="13"
                 Padding="8" />

        <!-- Splitter -->
        <GridSplitter Grid.Column="1"
                      Width="6"
                      Cursor="SizeWE"
                      HorizontalAlignment="Stretch"
                      VerticalAlignment="Stretch"
                      Background="#DDD" />

        <!-- Right pane: tag picker -->
        <Grid Grid.Column="2" Margin="6">
            <TextBlock Text="Tag picker pane (filled in Task 11b)"
                       HorizontalAlignment="Center"
                       VerticalAlignment="Center"
                       Foreground="#888" />
        </Grid>
    </Grid>
</Window>
```

- [ ] **Step 2: Build and run, eyeball the layout**

```powershell
dotnet build
dotnet run --project src/SunoMetatagApp
```
Expected: Window opens, ~1100×700. Left pane shows an empty editable text area in Consolas. **Caret is blinking in the left editor immediately on open** (initial focus). Right pane shows the placeholder text. The grid splitter between the two panes is 6px wide, shows a horizontal-resize cursor on hover, and is easy to grab. Close the window.

- [ ] **Step 3: Commit**

```powershell
git add src/SunoMetatagApp/MainWindow.xaml
git commit -m "feat: MainWindow grid skeleton with editor, focused-on-startup, 6px splitter"
```

---

## Task 11b: `MainWindow.xaml` right pane (search, category, button grid, error banner)

**Files:**
- Modify: `src/SunoMetatagApp/MainWindow.xaml`

- [ ] **Step 1: Replace the right-pane `<Grid>` block**

In `src/SunoMetatagApp/MainWindow.xaml`, replace the placeholder right-pane `<Grid Grid.Column="2" ...>...</Grid>` with:
```xml
        <!-- Right pane: tag picker -->
        <Grid Grid.Column="2" Margin="6">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />  <!-- Error banner -->
                <RowDefinition Height="Auto" />  <!-- Search -->
                <RowDefinition Height="Auto" />  <!-- Category -->
                <RowDefinition Height="*" />     <!-- Button grid -->
            </Grid.RowDefinitions>

            <!-- Error banner: visible only when LoadError is set -->
            <Border Grid.Row="0"
                    Background="#FFE5E5"
                    BorderBrush="#C00"
                    BorderThickness="1"
                    Padding="6"
                    Margin="0,0,0,6"
                    Visibility="{Binding LoadError, Converter={StaticResource NullToCollapsedConverter}}">
                <DockPanel>
                    <Button DockPanel.Dock="Right"
                            Content="Copy"
                            Padding="6,2"
                            Click="CopyErrorButton_Click"
                            ToolTip="Copy error message to clipboard" />
                    <TextBlock Text="{Binding LoadError}"
                               Foreground="#900"
                               TextWrapping="Wrap"
                               VerticalAlignment="Center" />
                </DockPanel>
            </Border>

            <!-- Search box -->
            <TextBox Grid.Row="1"
                     x:Name="SearchBox"
                     Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                     Margin="0,0,0,4"
                     Padding="4"
                     ToolTip="Search tags (case-insensitive)" />
            <TextBlock IsHitTestVisible="False"
                       Grid.Row="1"
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

            <!-- Category dropdown -->
            <ComboBox Grid.Row="2"
                      ItemsSource="{Binding Categories}"
                      SelectedItem="{Binding SelectedCategory}"
                      Margin="0,0,0,6"
                      Padding="4" />

            <!-- Scrollable button grid + empty-state placeholder -->
            <Grid Grid.Row="3">
                <ScrollViewer VerticalScrollBarVisibility="Auto"
                              HorizontalScrollBarVisibility="Disabled"
                              KeyboardNavigation.TabNavigation="Once"
                              KeyboardNavigation.DirectionalNavigation="Contained">
                    <ItemsControl ItemsSource="{Binding FilteredTags}">
                        <ItemsControl.ItemsPanel>
                            <ItemsPanelTemplate>
                                <WrapPanel Orientation="Horizontal" />
                            </ItemsPanelTemplate>
                        </ItemsControl.ItemsPanel>
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Button Content="{Binding Bracket}"
                                        ToolTip="{Binding Description}"
                                        ToolTipService.IsEnabled="{Binding Description, Converter={StaticResource StringIsNotEmptyConverter}}"
                                        Margin="2"
                                        Padding="6,2"
                                        Command="{Binding DataContext.InsertTagCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                        CommandParameter="{Binding}" />
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>

                <!-- "No tags match" placeholder shown when FilteredTags is empty -->
                <TextBlock Text="No tags match"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Top"
                           Margin="0,12,0,0"
                           Foreground="#666"
                           IsHitTestVisible="False">
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

- [ ] **Step 2: Register the `NullToCollapsedConverter` resource**

In the same `MainWindow.xaml`, add a `<Window.Resources>` block immediately after the opening `<Window ...>` tag and before `<Grid>`:
```xml
    <Window.Resources>
        <local:NullToCollapsedConverter x:Key="NullToCollapsedConverter" />
        <local:StringIsNotEmptyConverter x:Key="StringIsNotEmptyConverter" />
    </Window.Resources>
```

- [ ] **Step 3: Create the converters**

Write `src/SunoMetatagApp/NullToCollapsedConverter.cs`:
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

Write `src/SunoMetatagApp/StringIsNotEmptyConverter.cs`:
```csharp
using System;
using System.Globalization;
using System.Windows.Data;

namespace SunoMetatagApp;

/// <summary>
/// Returns true if the bound string is non-null and non-empty. Used to gate
/// ToolTipService.IsEnabled so buttons with no Description show no tooltip at all.
/// </summary>
public sealed class StringIsNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

- [ ] **Step 4: Add the `CopyErrorButton_Click` handler stub**

This handler will be filled in Task 12; for now, add an empty stub so the XAML compiles. In `src/SunoMetatagApp/MainWindow.xaml.cs`, replace the existing class body so it looks like:
```csharp
using System.Windows;

namespace SunoMetatagApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel vm && !string.IsNullOrEmpty(vm.LoadError))
        {
            Clipboard.SetText(vm.LoadError);
        }
    }
}
```

- [ ] **Step 5: Build and run, eyeball the layout**

```powershell
dotnet build
dotnet run --project src/SunoMetatagApp
```
Expected: Window opens; **caret is in the left editor on startup**. Right pane shows a search box (with "Search tags…" placeholder), a category dropdown with "All" selected, and a wrapping grid of ~115 metatag buttons in the scrollable area below. Buttons render their bracket text (e.g. `[Verse]`).

Keyboard checks:
- From the editor, pressing **Tab** moves to the search box. Pressing Tab again moves to the category combo. Pressing Tab again enters the tag panel as a **single** stop (not button-by-button). Inside the panel, arrow keys move between buttons. Pressing Tab again leaves the panel.

Hover checks:
- Hover `[Effect: Sidechain]` → tooltip "Volume ducking pumped to the kick — classic EDM/house feel." appears.
- Hover `[Verse]` → **no tooltip** appears at all (empty tooltips suppressed via `ToolTipService.IsEnabled`).

Filter checks:
- Type `zzzzz` in the search box → button grid is empty, **"No tags match"** placeholder appears centered near the top of the panel.
- Clear the search box → grid repopulates, placeholder disappears.
- Selecting a category filters too. Buttons clicking does nothing yet (Task 12). Close the window.

- [ ] **Step 6: Commit**

```powershell
git add src/SunoMetatagApp/MainWindow.xaml src/SunoMetatagApp/MainWindow.xaml.cs src/SunoMetatagApp/NullToCollapsedConverter.cs src/SunoMetatagApp/StringIsNotEmptyConverter.cs
git commit -m "feat: MainWindow right pane with search, category, button grid, empty-state, tooltip suppression, contained tab navigation"
```

---

## Task 12: Caret-aware insertion in `MainWindow.xaml.cs`

**Files:**
- Modify: `src/SunoMetatagApp/MainWindow.xaml.cs`

Subscribe to `MainViewModel.InsertRequested` and perform the caret-aware insertion using `InsertionRules.BuildInsertion`.

- [ ] **Step 1: Replace `MainWindow.xaml.cs`**

Replace contents of `src/SunoMetatagApp/MainWindow.xaml.cs`:
```csharp
using System;
using System.Windows;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
            oldVm.InsertRequested -= OnInsertRequested;
        if (e.NewValue is MainViewModel newVm)
            newVm.InsertRequested += OnInsertRequested;
    }

    private void OnInsertRequested(object? sender, TagDefinition tag)
    {
        var caret = LyricEditor.CaretIndex;
        var fullText = LyricEditor.Text ?? string.Empty;
        var inserted = InsertionRules.BuildInsertion(fullText, caret, tag.Bracket, Environment.NewLine);
        var newText = fullText.Insert(caret, inserted);

        // Splice into the bound property so the ViewModel stays in sync.
        if (DataContext is MainViewModel vm)
        {
            vm.LyricText = newText;
        }

        // Per spec §8.1: land caret on the line *after* the bracket.
        // If BuildInsertion already added a trailing newline, caret + inserted.Length
        // is already past it. If it omitted the trailing newline (because a newline
        // was already there), advance over at most one \r\n / \r / \n now.
        int newCaret = caret + inserted.Length;
        if (newCaret < newText.Length && newText[newCaret] == '\r') newCaret++;
        if (newCaret < newText.Length && newText[newCaret] == '\n') newCaret++;

        LyricEditor.CaretIndex = newCaret;
        LyricEditor.Focus();
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

- [ ] **Step 2: Build and run, smoke test**

```powershell
dotnet build
dotnet run --project src/SunoMetatagApp
```

Manual smoke test in the running app (all 7 cases must pass):

1. **Mid-text insert.** Type "hello world" in the editor. Click between "hello" and "world", then click `[Verse]`. The tag should appear on its own line, splitting the text. Cursor should land on the new blank line below the tag, ready to type.
2. **Start-of-document insert.** Place cursor at the very start, click `[Chorus]`. Should insert at the top with a newline after but no newline before. Cursor lands on the line after `[Chorus]`.
3. **End-of-document insert.** Place cursor at the very end, click `[Bridge]`. Should insert at the bottom with a newline before but no newline after. Cursor lands at the end (after `[Bridge]`).
4. **End of mid-document line (new — added in r2 per spec §8.1).** Type three lines: `line one` ⏎ `line two` ⏎ `line three`. Place cursor at the **end of `line one`** (just before the line break, before `line two`). Click `[Verse]`. Expected: `[Verse]` appears on its own line between `line one` and `line two`. **Cursor must land at the start of `line two`** — i.e., on the line *after* `[Verse]`, not still on the bracket's line. If cursor lands at end of the bracket's line, the post-insertion advancement rule failed.
5. **Search filter.** Type "ver" in the search box. The button grid should narrow to a few results.
6. **Category filter.** Select "Vocal" in the category dropdown. Should show only Vocal tags. Reset category to "All", clear search.
7. **Empty filter.** Type "zzzzz" in the search box. Button grid empty; "No tags match" placeholder visible. Clear search; placeholder disappears.

Close the window.

- [ ] **Step 3: Commit**

```powershell
git add src/SunoMetatagApp/MainWindow.xaml.cs
git commit -m "feat: caret-aware tag insertion via InsertRequested event"
```

---

## Task 13: Verify error-banner path

**Files:** (test only — no source changes)

- [ ] **Step 1: Temporarily rename `tags.json` in the build output**

```powershell
dotnet build
Move-Item src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json `
          src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json.bak
```

- [ ] **Step 2: Run and verify the error banner appears**

```powershell
dotnet run --project src/SunoMetatagApp --no-build
```

Expected: Window opens, red banner at top of right pane reads "tags.json not found at '…\tags.json'.", "Copy" button works (paste somewhere to verify the message is on the clipboard), button grid is empty, editor still accepts input. Close.

- [ ] **Step 3: Restore the file**

```powershell
Move-Item src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json.bak `
          src/SunoMetatagApp/bin/Debug/net8.0-windows/tags.json
```

- [ ] **Step 4: No commit (no source changes).**

---

## Task 14: Seed `docs/BACKLOG.md`

**Files:**
- Create: `j:\SunoMetatagApp\docs\BACKLOG.md`

- [ ] **Step 1: Write the backlog**

Write `j:\SunoMetatagApp\docs\BACKLOG.md`:
```markdown
# SunoMetatagApp Backlog

Open work for v2+. v1 scope is locked by [`specs/2026-05-25-suno-metatag-scratch-design.md`](specs/2026-05-25-suno-metatag-scratch-design.md).

Roughly prioritized — top first.

## B-001 — Favorites / recently-used tags

**Status:** open
**Priority:** high
**Source:** v1 design §11
**Acceptance:** A "Pinned" section appears at the top of the tag panel showing user-pinned tags. Right-click a tag → "Pin". Persisted to `%APPDATA%\SunoMetatagApp\favorites.json` across launches. A separate "Recent" section shows the last 10 distinct tags used in this session.
**Notes:** Decide pin/unpin UX (right-click vs Shift-click vs an explicit pin icon on each button).

## B-002 — Dark theme

**Status:** open
**Priority:** medium
**Source:** v1 design §11
**Acceptance:** Light/dark toggle in a new (small) settings menu. Choice persisted to `%APPDATA%\SunoMetatagApp\settings.json`. All controls — editor, panel, error banner — re-themed correctly.

## B-003 — Selection-wrap insertion

**Status:** open
**Priority:** medium
**Source:** v1 design §11; alternate insertion mode considered during brainstorming
**Acceptance:** If text is selected when a tag is clicked, the tag wraps the selection (e.g. selected text becomes the body of a `[Verse] … [Verse]` block). Off by default; togglable in settings.
**Notes:** Decide on the closing-tag convention (Suno does not use closing tags; this is a user-comfort feature, not a literal Suno semantics feature).

## B-004 — Hotkeys for top-N tags

**Status:** open
**Priority:** medium
**Source:** v1 design §11
**Acceptance:** `Ctrl+1` … `Ctrl+9` insert the first 9 pinned tags (depends on B-001). Hotkey labels render on the buttons.

## B-005 — Persist lyric text across launches

**Status:** open
**Priority:** medium
**Source:** v1 design §11
**Acceptance:** Lyric text is saved to `%APPDATA%\SunoMetatagApp\last-session.txt` on close and restored on open. A "Clear" button in a small toolbar wipes the buffer and saved file.
**Notes:** Trade-off — adds disk I/O that the "temporary scratch space" v1 explicitly avoided. Validate desire first.

## B-006 — Reload `tags.json` without restart

**Status:** open
**Priority:** low
**Source:** Surfaced during brainstorming (spec §6 "Live-edit behavior")
**Acceptance:** A "Reload tags" item in a small menu or button re-reads `tags.json` from disk. UI updates immediately. Errors during reload show the same error banner as startup.

## B-007 — Auto-update `tags.json` from a known URL

**Status:** open
**Priority:** low
**Source:** v1 design §11
**Acceptance:** A "Check for tag updates" action pulls from a configured URL (default: TBD), shows a diff preview ("X new tags, Y renamed, Z removed"), confirms, then writes the merged `tags.json` next to the exe. Backup the previous file.

## B-008 — Per-category insertion rules (inline vs. own-line)

**Status:** open
**Priority:** low
**Source:** v1 design §11
**Acceptance:** `tags.json` can opt a tag into inline (no newlines) insertion via an `inline: true` field. Vocal/style/effect tags typically inline; structure tags stay on their own line. Default remains on its own line.

## B-009 — Tag aliases / search synonyms

**Status:** open
**Priority:** low
**Source:** v1 design §11
**Acceptance:** `tags.json` accepts an optional `aliases: [...]` field per tag. Search matches against `label`, `bracket`, and `aliases` (case-insensitive substring). Aliases do not render visually.

## B-010 — Retry `musci.io/blog/suno-tags` as a seed source

**Status:** open
**Priority:** low
**Source:** Spec §12 deferred / open question
**Acceptance:** When the URL becomes reachable (currently 500s), re-run the seeding work and merge new tags into `Resources/tags.json`. Document the merge result in a commit message.

## B-011 — Virtualize tag panel

**Status:** open
**Priority:** medium (trigger-based)
**Source:** Spec §5.1 + FRONTEND/UX advisory (MEDIUM)
**Acceptance:** Tag panel uses a virtualizing layout that maintains the wrap-grid appearance and stays smooth (no perceptible scroll stutter, no search-keystroke lag) at 500+ tags. Options at the time of work: `VirtualizingStackPanel` (loses wrap), `VirtualizingWrapPanel` (third-party), or custom.
**Trigger condition:** ship when tag count > 300 OR the user reports stutter/lag. v1 intentionally non-virtualized.

## B-012 — Cache TagViewModel per TagDefinition

**Status:** open
**Priority:** low
**Source:** FRONTEND/UX advisory (MEDIUM)
**Acceptance:** `MainViewModel.ComputeFiltered` reuses existing `TagViewModel` wrappers instead of allocating new ones on every search-keystroke. Filter result remains correct; allocations on `OnSearchTextChanged` drop materially. Verified with a debug-build allocation count check or comment.

## B-013 — Tag button visual treatment

**Status:** open
**Priority:** medium
**Source:** Spec §5.1 + FRONTEND/UX advisory (MEDIUM)
**Acceptance:** A `Style x:Key="TagButtonStyle"` (flat background, 1px border, subtle hover, optional category color hint) replaces default WPF chrome on the tag buttons. Light mode only (dark mode is B-002). Defined once in `Window.Resources` or a `Themes/` file.

## B-014 — Screen-reader naming

**Status:** open
**Priority:** low
**Source:** FRONTEND/UX advisory (LOW)
**Acceptance:** Every interactive control (editor, search box, category combo, each tag button, splitter) has an `AutomationProperties.Name` set so screen readers announce a useful label instead of the class name.

## B-015 — Clear-search / reset-filters affordance

**Status:** open
**Priority:** low
**Source:** FRONTEND/UX advisory (LOW)
**Acceptance:** Either a small `✕` button inside (or next to) the search box clears the text on click, or pressing `Esc` while focus is in the search box clears it. Category resets to "All" alongside (or via separate "Reset filters" affordance).

## B-016 — Persist splitter position

**Status:** open
**Priority:** low
**Source:** FRONTEND/UX advisory (LOW)
**Acceptance:** Splitter position saved to `%APPDATA%\SunoMetatagApp\layout.json` on close, restored on open. Default position remains 70/30 if no saved value exists.

## B-017 — Category dropdown tooltip

**Status:** open
**Priority:** trivial
**Source:** FRONTEND/UX advisory (LOW)
**Acceptance:** Category `ComboBox` has `ToolTip="Filter by category"`. One-line change.
```

- [ ] **Step 2: Commit**

```powershell
git add docs/BACKLOG.md
git commit -m "docs: seed BACKLOG.md from v1 design out-of-scope list"
```

---

## Task 15: Publish, verify single-file exe, document

**Files:**
- Modify: `README.md` (only if the publish steps reveal something worth noting)

- [ ] **Step 1: Final test pass before publishing**

```powershell
dotnet test
```
Expected: PASS, 27 tests.

- [ ] **Step 2: Publish self-contained single-file**

```powershell
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```
Expected: Build succeeded; output in `publish\`. Verify:
```powershell
Get-ChildItem publish | Select-Object Name, Length
Test-Path publish/SunoMetatagApp.exe
Test-Path publish/tags.json
```
Expected: `SunoMetatagApp.exe` present (~80 MB), `tags.json` present.

- [ ] **Step 3: Smoke test the published exe**

Double-click `publish\SunoMetatagApp.exe` (or run from PowerShell).

Verify:
- Window opens.
- Right pane shows ~115 tag buttons across categories.
- Typing in search filters them.
- Selecting a category filters them.
- Clicking `[Verse]` with the cursor in the empty editor inserts `[Verse]\r\n` and places the cursor on the new line.
- Typing more text, clicking `[Chorus]` mid-text inserts it on its own line.

Close the window.

- [ ] **Step 4: Commit `publish/` exclusion** *(only if `publish/` was tracked)*

If `git status` shows the `publish/` folder as untracked, no commit is needed — it is already covered by `.gitignore`. Skip this step.

- [ ] **Step 5: Final commit (optional)**

If you tweaked `README.md` during smoke testing:
```powershell
git add README.md
git commit -m "docs: note any publish quirks discovered during smoke test"
```

Otherwise no commit — the work is complete.

---

## Done criteria

- [ ] `dotnet test` passes with 27 tests.
- [ ] `dotnet run --project src/SunoMetatagApp` opens the working app.
- [ ] All 6 manual smoke-test cases from Task 12 Step 2 pass.
- [ ] Error-banner path (Task 13) verified.
- [ ] `publish/SunoMetatagApp.exe` runs as a standalone self-contained app.
- [ ] `docs/BACKLOG.md` lists v2+ items.
- [ ] Git history shows ~14 conventional-commit commits, one per task.
