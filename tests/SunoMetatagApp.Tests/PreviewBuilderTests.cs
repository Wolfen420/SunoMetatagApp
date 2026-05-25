using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

public class PreviewBuilderTests
{
    private const string NL = "\n";

    private static Section MakeSection(string lyrics) => new Section { Lyrics = lyrics };

    [Fact]
    public void Build_NoSections_ReturnsEmpty()
        => Assert.Equal("", PreviewBuilder.Build(new List<Section>(), NL));

    [Fact]
    public void Build_SingleSection_LyricsOnly_RendersLyricsAsIs()
    {
        var s = MakeSection("just lyrics");
        Assert.Equal("just lyrics", PreviewBuilder.Build(new[] { s }, NL));
    }

    [Fact]
    public void Build_TwoSections_SeparatedByOneBlankLine()
    {
        var s1 = MakeSection("l1");
        var s2 = MakeSection("l2");
        Assert.Equal("l1\n\nl2", PreviewBuilder.Build(new[] { s1, s2 }, NL));
    }

    [Fact]
    public void Build_WhitespaceOnlySection_IsSkipped()
    {
        var s1 = MakeSection("l1");
        var sMid = MakeSection("   \n\t");
        var s2 = MakeSection("l2");
        Assert.Equal("l1\n\nl2", PreviewBuilder.Build(new[] { s1, sMid, s2 }, NL));
    }

    [Fact]
    public void Build_TrimsTrailingNewlines()
    {
        var s = MakeSection("ends with newline\n\n");
        Assert.Equal("ends with newline", PreviewBuilder.Build(new[] { s }, NL));
    }

    [Fact]
    public void Build_HandlesCrLfNewlineParameter()
    {
        var s1 = MakeSection("l1");
        var s2 = MakeSection("l2");
        Assert.Equal("l1\r\n\r\nl2", PreviewBuilder.Build(new[] { s1, s2 }, "\r\n"));
    }

    [Fact]
    public void Build_LyricsWithInlineBracketTokens_PassesThroughVerbatim()
    {
        var s = MakeSection("Walking the [Guitar] street\n[Drums]\nfeels right");
        Assert.Equal(
            "Walking the [Guitar] street\n[Drums]\nfeels right",
            PreviewBuilder.Build(new[] { s }, NL));
    }
}
