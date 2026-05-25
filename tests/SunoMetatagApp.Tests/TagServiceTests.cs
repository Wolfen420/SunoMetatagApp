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
