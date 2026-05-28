using System.Collections.Generic;

namespace SunoMetatagApp.Models;

// v1.18 (B-025): hardcoded built-in templates per BACKLOG acceptance. Future
// backlog items may add user-defined templates persisted to
// %APPDATA%\SunoMetatagApp\templates.json (explicitly deferred per Notes).
public static class SongTemplates
{
    public static IReadOnlyList<SongTemplate> BuiltIns { get; } = new SongTemplate[]
    {
        new("Standard Pop", new[]
        {
            "Intro", "Verse 1", "Pre-Chorus", "Chorus",
            "Verse 2", "Pre-Chorus", "Chorus",
            "Bridge", "Chorus", "Outro",
        }),
        new("Simple Ballad", new[]
        {
            "Intro", "Verse 1", "Chorus",
            "Verse 2", "Chorus",
            "Bridge", "Outro",
        }),
        new("Rock / EDM", new[]
        {
            "Intro", "Verse 1", "Chorus",
            "Verse 2", "Chorus",
            "Drop", "Chorus", "Outro",
        }),
        new("Rap / Hip-Hop", new[]
        {
            "Intro", "Verse 1", "Hook",
            "Verse 2", "Hook",
            "Verse 3", "Hook", "Outro",
        }),
    };
}
