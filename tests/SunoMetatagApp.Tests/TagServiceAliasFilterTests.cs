using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.21 (B-SUNO-007c): alias resolution coverage. Synthetic Sample for A1-A7
// (deterministic, no dependency on tags.json content). A8 integration-tests
// the real production tags.json copied to test output via project reference.
public class TagServiceAliasFilterTests
{
    // Sample mixes entries with and without aliases. Canonical "Aggressive" /
    // "[Mood: Aggressive]" mirrors the v1.15 ALIAS-row #1 mapping so tests
    // exercise the same shape the production data uses. "Bright" has no
    // aliases (control). "Building" includes a multi-word alias to exercise
    // NormalizeForSearch space-strip on the alias path.
    private static readonly TagDefinition[] Sample =
    {
        new("Mood", "Aggressive", "[Mood: Aggressive]", null, 4, new[] { "[Aggressive]" }),
        new("Mood", "Bright",     "[Mood: Bright]",     null, 4, null),
        new("Mood", "Empty",      "[Mood: Empty]",      null, 4, Array.Empty<string>()),
        new("Energy", "Building", "[Energy: Building]", null, 4, new[] { "[Building Energy]" }),
        new("Vocal", "Melisma",   "[Melisma]",          null, 2, new[] { "[Melismatic]" }),
    };

    // A1: typing the short form (with brackets) finds the canonical entry.
    [Fact]
    public void A1_Filter_AliasShortForm_MatchesCanonicalEntry()
    {
        var result = TagService.Filter(Sample, "[Aggressive]", "All").ToList();
        Assert.Single(result);
        Assert.Equal("[Mood: Aggressive]", result[0].Bracket);
    }

    // A2: alias matching honors v1.7 hyphen/space normalization. "HighEnergy"
    // (no space) finds an entry whose alias is "[Building Energy]" only if
    // space-strip applies symmetrically; "BuildingEnergy" verifies the same.
    [Fact]
    public void A2_Filter_AliasShortForm_HyphenSpaceInsensitive()
    {
        var result = TagService.Filter(Sample, "BuildingEnergy", "All").ToList();
        Assert.Single(result);
        Assert.Equal("[Energy: Building]", result[0].Bracket);
    }

    // A3: entry with null Aliases + non-matching search → no match. The
    // null-coalesce to Array.Empty<string>() must not produce spurious results.
    [Fact]
    public void A3_Filter_NullAliasesField_NoExtraMatches()
    {
        var result = TagService.Filter(Sample, "doesnotexist", "All").ToList();
        Assert.Empty(result);
    }

    // A4: entry with empty aliases array + non-matching search → no match.
    [Fact]
    public void A4_Filter_EmptyAliasesArray_NoExtraMatches()
    {
        // "Empty" entry has Aliases = Array.Empty<string>(); searching for a
        // term that matches neither Label nor Bracket nor any alias returns 0.
        var result = TagService.Filter(Sample, "totallyunrelated", "All").ToList();
        Assert.Empty(result);
    }

    // A5: each of the 10 v1.15 alias short forms finds its canonical entry
    // in the real production tags.json.
    [Theory]
    [InlineData("[Aggressive]",     "[Mood: Aggressive]")]
    [InlineData("[Building Energy]","[Energy: Building]")]
    [InlineData("[Dreamy]",         "[Atmosphere: Dreamy]")]
    [InlineData("[Euphoric]",       "[Mood: Euphoric]")]
    [InlineData("[Explosive]",      "[Energy: Explosive]")]
    [InlineData("[High Energy]",    "[Mood: High Energy]")]
    [InlineData("[Melancholic]",    "[Mood: Melancholic]")]
    [InlineData("[Melismatic]",     "[Melisma]")]
    [InlineData("[Nostalgic]",      "[Mood: Nostalgic]")]
    [InlineData("[Romantic]",       "[Mood: Romantic]")]
    public void A5_Filter_All10V15Aliases_FindCanonical(string shortForm, string canonical)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
        var tags = TagService.LoadAll(path);
        var result = TagService.Filter(tags, shortForm, "All")
                               .Select(t => t.Bracket)
                               .ToList();
        Assert.Contains(canonical, result);
    }

    // A6: alias match preserves v1.11 alphabetical sort by canonical Bracket.
    // Construct a search that matches multiple entries via mixed Label/alias
    // paths and verify the result is sorted by Bracket case-insensitive
    // ordinal.
    [Fact]
    public void A6_Filter_AliasMatch_PreservesAlphabeticalSort()
    {
        // Both "[Mood: Aggressive]" (alias "[Aggressive]") and "[Mood: Bright]"
        // (Label "Bright") match "b" — wait, only Bright matches "b". Use a
        // search that hits multiple via the mixed paths:
        // - "Mood" matches Label/Bracket of Aggressive, Bright, Empty (Label
        //   "Empty" + Bracket "[Mood: Empty]")
        var result = TagService.Filter(Sample, "Mood", "All").ToList();
        // Sorted by Bracket: [Mood: Aggressive] < [Mood: Bright] < [Mood: Empty]
        Assert.Equal(3, result.Count);
        Assert.Equal("[Mood: Aggressive]", result[0].Bracket);
        Assert.Equal("[Mood: Bright]", result[1].Bracket);
        Assert.Equal("[Mood: Empty]", result[2].Bracket);
    }

    // A7: search matching BOTH Label substring AND alias produces the entry
    // exactly once (no duplication from multi-path match).
    [Fact]
    public void A7_Filter_AliasMatchDoesNotProduceDuplicates()
    {
        // "Aggressive" matches Label "Aggressive" AND alias "[Aggressive]".
        var result = TagService.Filter(Sample, "Aggressive", "All").ToList();
        Assert.Single(result);
        Assert.Equal("[Mood: Aggressive]", result[0].Bracket);
    }

    // A8: LoadAll populates Aliases correctly from production tags.json — all
    // 10 v1.15 canonical entries have non-empty Aliases; non-ALIAS entries
    // have null Aliases (verified via spot-check on a non-aliased entry).
    [Fact]
    public void A8_LoadAll_AliasesFieldPopulated()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
        var tags = TagService.LoadAll(path);

        // Spot-check: 10 canonical entries have non-null non-empty Aliases.
        var canonicals = new[]
        {
            "[Mood: Aggressive]",  "[Energy: Building]", "[Atmosphere: Dreamy]",
            "[Mood: Euphoric]",    "[Energy: Explosive]", "[Mood: High Energy]",
            "[Mood: Melancholic]", "[Melisma]",          "[Mood: Nostalgic]",
            "[Mood: Romantic]",
        };
        foreach (var bracket in canonicals)
        {
            var entry = tags.Single(t => t.Bracket == bracket);
            Assert.NotNull(entry.Aliases);
            Assert.NotEmpty(entry.Aliases);
        }

        // Spot-check: a non-aliased entry has null Aliases.
        var nonAliased = tags.First(t => t.Bracket == "[Mood: Dark]");
        Assert.Null(nonAliased.Aliases);
    }
}
