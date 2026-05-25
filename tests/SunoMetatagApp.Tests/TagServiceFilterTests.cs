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
