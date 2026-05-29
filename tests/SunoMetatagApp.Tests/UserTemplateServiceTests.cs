using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.20 (B-028): UserTemplateService coverage — load missing/malformed/valid
// files, sanitize empty entries, save+roundtrip, atomic-write directory
// creation. All tests use temp-directory paths for isolation; each test
// instance gets a unique temp path. Cleanup deferred to OS (Path.GetTempPath
// rotation) — explicit cleanup would race with parallel xUnit runs.
public class UserTemplateServiceTests
{
    private static string NewTempPath() =>
        Path.Combine(Path.GetTempPath(),
                     "SunoMetatagApp-tests-" + Guid.NewGuid().ToString("N"),
                     "templates.json");

    // V1: Constructor with non-existent file path returns empty list, no throw.
    [Fact]
    public void V1_LoadAll_MissingFile_ReturnsEmpty()
    {
        var svc = new UserTemplateService(NewTempPath());
        var result = svc.LoadAll();
        Assert.Empty(result);
    }

    // V2: Malformed JSON returns empty list, does not throw.
    [Fact]
    public void V2_LoadAll_MalformedJson_ReturnsEmpty()
    {
        var path = NewTempPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "not valid json {{{");
        var svc = new UserTemplateService(path);
        var result = svc.LoadAll();
        Assert.Empty(result);
    }

    // V3: Valid JSON with 2 templates round-trips correctly.
    [Fact]
    public void V3_LoadAll_ValidJson_ReturnsTemplates()
    {
        var path = NewTempPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, """
            [
              { "name": "Alpha", "sectionTypes": ["Verse 1", "Chorus"] },
              { "name": "Beta",  "sectionTypes": ["Intro", "Hook", "Outro"] }
            ]
            """);
        var svc = new UserTemplateService(path);
        var result = svc.LoadAll();
        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha", result[0].Name);
        Assert.Equal(new[] { "Verse 1", "Chorus" }, result[0].SectionTypes.ToArray());
        Assert.Equal("Beta", result[1].Name);
        Assert.Equal(new[] { "Intro", "Hook", "Outro" }, result[1].SectionTypes.ToArray());
    }

    // V4: Entry with empty name is skipped silently.
    [Fact]
    public void V4_LoadAll_SkipsEmptyName()
    {
        var path = NewTempPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, """
            [
              { "name": "",      "sectionTypes": ["Verse"] },
              { "name": "Valid", "sectionTypes": ["Chorus"] }
            ]
            """);
        var svc = new UserTemplateService(path);
        var result = svc.LoadAll();
        Assert.Single(result);
        Assert.Equal("Valid", result[0].Name);
    }

    // V5: Entry with empty SectionTypes is skipped.
    [Fact]
    public void V5_LoadAll_SkipsEmptySectionTypes()
    {
        var path = NewTempPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, """
            [
              { "name": "Empty",  "sectionTypes": [] },
              { "name": "Valid",  "sectionTypes": ["Chorus"] }
            ]
            """);
        var svc = new UserTemplateService(path);
        var result = svc.LoadAll();
        Assert.Single(result);
        Assert.Equal("Valid", result[0].Name);
    }

    // V6: SaveAll creates the parent directory if it does not exist.
    [Fact]
    public void V6_SaveAll_CreatesDirectory()
    {
        var path = NewTempPath();
        Assert.False(File.Exists(path));
        Assert.False(Directory.Exists(Path.GetDirectoryName(path)!));
        var svc = new UserTemplateService(path);
        svc.SaveAll(new[] { new SongTemplate("Test", new[] { "Verse" }) });
        Assert.True(File.Exists(path));
    }

    // V7: SaveAll writes via .tmp + Move; .tmp must not remain after Move.
    [Fact]
    public void V7_SaveAll_AtomicViaTempFile()
    {
        var path = NewTempPath();
        var svc = new UserTemplateService(path);
        svc.SaveAll(new[] { new SongTemplate("Test", new[] { "Verse" }) });
        Assert.True(File.Exists(path));
        Assert.False(File.Exists(path + ".tmp"));
    }

    // V8: SaveAll → LoadAll roundtrip preserves Name and SectionTypes exactly.
    [Fact]
    public void V8_SaveAll_Roundtrip()
    {
        var path = NewTempPath();
        var svc = new UserTemplateService(path);
        var input = new[]
        {
            new SongTemplate("Roundtrip Alpha", new[] { "Intro", "Verse 1", "Chorus", "Outro" }),
            new SongTemplate("Roundtrip Beta",  new[] { "Verse", "Hook" }),
        };
        svc.SaveAll(input);
        var result = svc.LoadAll();
        Assert.Equal(2, result.Count);
        Assert.Equal("Roundtrip Alpha", result[0].Name);
        Assert.Equal(input[0].SectionTypes, result[0].SectionTypes);
        Assert.Equal("Roundtrip Beta", result[1].Name);
        Assert.Equal(input[1].SectionTypes, result[1].SectionTypes);
    }
}
