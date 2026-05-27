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

    // O1 -- v1.11 (B-SUNO-011): Filter returns entries in alphabetical-by-Bracket
    // order (case-insensitive ordinal) across the full sample ("All" category,
    // empty search). Sort is applied AFTER filter as a single LINQ pipeline.
    [Fact]
    public void O1_Filter_AllCategoryEmptySearch_ReturnsAlphabeticalByBracket()
    {
        var result = TagService.Filter(Sample, null, "All").ToList();
        var expected = new[] { "[Belted]", "[Chorus]", "[Effect: Reverb: Hall]", "[Verse]", "[Whispered]" };
        Assert.Equal(expected, result.Select(t => t.Bracket).ToArray());
    }

    // O2 -- v1.11 (B-SUNO-011): Filter returns category-filtered entries in
    // alphabetical order, including prefix-form bracket entries
    // ([Mood: Aggressive] < [Mood: Euphoric] < [Mood: Nostalgic]) because the
    // shared [Mood:  prefix is identical and the post-colon word drives order.
    [Fact]
    public void O2_Filter_CategoryFilter_ReturnsAlphabeticalIncludingPrefixForm()
    {
        var moods = new[]
        {
            new TagDefinition("Mood", "Nostalgic",  "[Mood: Nostalgic]"),
            new TagDefinition("Mood", "Aggressive", "[Mood: Aggressive]"),
            new TagDefinition("Mood", "Euphoric",   "[Mood: Euphoric]"),
        };
        var result = TagService.Filter(moods, null, "Mood").ToList();
        var expected = new[] { "[Mood: Aggressive]", "[Mood: Euphoric]", "[Mood: Nostalgic]" };
        Assert.Equal(expected, result.Select(t => t.Bracket).ToArray());
    }

    // O3 -- v1.11 (B-SUNO-011) Lead r1 absorption #3: case-mixed synthetic
    // ordering test. Verifies StringComparer.OrdinalIgnoreCase folds case
    // before comparison so a/A, b/B, c/C sort by letter regardless of casing.
    [Fact]
    public void O3_Filter_MixedCaseSynthetic_ReturnsOrdinalIgnoreCaseOrder()
    {
        var synthetic = new[]
        {
            new TagDefinition("X", "z-banana", "[BANANA]"),
            new TagDefinition("X", "z-cherry", "[Cherry]"),
            new TagDefinition("X", "z-apple",  "[apple]"),
        };
        var result = TagService.Filter(synthetic, null, "All").ToList();
        var expected = new[] { "[apple]", "[BANANA]", "[Cherry]" };
        Assert.Equal(expected, result.Select(t => t.Bracket).ToArray());
    }
}
