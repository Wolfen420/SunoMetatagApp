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
