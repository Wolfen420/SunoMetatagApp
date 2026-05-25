using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

public class PreviewBuilderTests
{
    private const string NL = "\n";

    private static TagDefinition Tag(string name) => new("Test", name, $"[{name}]");

    private static Section MakeSection(string lyrics, params TagDefinition[] tags)
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
        var s = MakeSection("Song here\nIt's lyrics", Tag("Guitar"), Tag("Powerful"));
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("[Guitar]\n[Powerful]\nSong here\nIt's lyrics", result);
    }

    [Fact]
    public void Build_SingleSection_TagsOnly_RendersTagsNoTrailingBlank()
    {
        var s = MakeSection("", Tag("Outro"));
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("[Outro]", result);
    }

    [Fact]
    public void Build_SingleSection_LyricsOnly_RendersLyricsAsIs()
    {
        var s = MakeSection("just lyrics");
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("just lyrics", result);
    }

    [Fact]
    public void Build_TwoSections_SeparatedByOneBlankLine()
    {
        var s1 = MakeSection("l1", Tag("A"));
        var s2 = MakeSection("l2", Tag("B"));
        var result = PreviewBuilder.Build(new[] { s1, s2 }, NL);
        Assert.Equal("[A]\nl1\n\n[B]\nl2", result);
    }

    [Fact]
    public void Build_MiddleSectionEmpty_SkippedFromOutput()
    {
        var s1 = MakeSection("l1", Tag("A"));
        var sMid = new Section();
        var s2 = MakeSection("l2", Tag("B"));
        var result = PreviewBuilder.Build(new[] { s1, sMid, s2 }, NL);
        Assert.Equal("[A]\nl1\n\n[B]\nl2", result);
    }

    [Fact]
    public void Build_SectionLyricsEndingInNewline_NormalizedAtBoundary()
    {
        var s1 = MakeSection("l1\n", Tag("A"));
        var s2 = MakeSection("l2", Tag("B"));
        var result = PreviewBuilder.Build(new[] { s1, s2 }, NL);
        Assert.Equal("[A]\nl1\n\n[B]\nl2", result);
    }

    [Fact]
    public void Build_PreservesTagOrder_WithinSection()
    {
        var s = MakeSection("l", Tag("B"), Tag("A"), Tag("C"));
        var result = PreviewBuilder.Build(new[] { s }, NL);
        Assert.Equal("[B]\n[A]\n[C]\nl", result);
    }

    [Fact]
    public void Build_HandlesCrLfNewlineParameter()
    {
        var s = MakeSection("l", Tag("A"));
        var result = PreviewBuilder.Build(new[] { s }, "\r\n");
        Assert.Equal("[A]\r\nl", result);
    }
}
