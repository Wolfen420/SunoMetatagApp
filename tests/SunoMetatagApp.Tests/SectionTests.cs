using SunoMetatagApp.Models;
using Xunit;

namespace SunoMetatagApp.Tests;

public class SectionTests
{
    [Fact]
    public void Default_Lyrics_IsEmpty()
    {
        var s = new Section();
        Assert.Equal("", s.Lyrics);
    }

    [Fact]
    public void Setting_Lyrics_RaisesPropertyChanged()
    {
        var s = new Section();
        int fires = 0;
        s.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Section.Lyrics)) fires++;
        };
        s.Lyrics = "hello";
        Assert.Equal(1, fires);
        Assert.Equal("hello", s.Lyrics);
    }
}
