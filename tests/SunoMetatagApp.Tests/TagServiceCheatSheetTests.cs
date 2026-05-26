using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.4 (B-SUNO-005): content-coverage tests for cheat-sheet reconciliation.
// Loads the production tags.json from the test project's build output (copied
// via project-reference to main project's CopyToOutput on tags.json).
// Spec: docs/specs/2026-05-27-suno-metatag-v1.4-cheatsheet-reconciliation.md S7.2.
public class TagServiceCheatSheetTests
{
    private static IReadOnlyList<TagDefinition> LoadProductionTagsJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
        return TagService.LoadAll(path);
    }

    // C1 -- after v1.4 reconciliation, tags.json holds at least 198 entries
    // (124 v1.3 baseline + 74 ADD per Lead-ratified decision table).
    [Fact]
    public void C1_LoadAll_LoadsExpectedMinimumCount()
    {
        var tags = LoadProductionTagsJson();
        Assert.True(
            tags.Count >= 198,
            $"Expected >= 198 tags after B-SUNO-005, got {tags.Count}.");
    }

    // C2 -- new SFX category lands per spec S3.2 / decision table SS D.1.
    [Fact]
    public void C2_DistinctCategories_IncludesSFX()
    {
        var tags = LoadProductionTagsJson();
        var categories = TagService.DistinctCategories(tags);
        Assert.Contains("SFX", categories);
    }

    // C3 -- a known new SFX entry is searchable end-to-end via Filter.
    [Fact]
    public void C3_Filter_FindsNewSFXTag_Birdsong()
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, "Birdsong", "All").ToList();
        Assert.Contains(hits, t => t.Bracket == "[Birdsong]");
    }

    // C4 -- category filter for SFX returns ONLY SFX entries (no bleed).
    [Fact]
    public void C4_Filter_BySFXCategory_ReturnsOnlySFXEntries()
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, "", "SFX").ToList();
        Assert.NotEmpty(hits);
        Assert.All(hits, t => Assert.Equal("SFX", t.Category));
    }

    // C5 -- defensive: no two entries share a bracket string. Catches
    // copy-paste errors in the decision-table application at T1.
    [Fact]
    public void C5_LoadAll_NoBracketCollisions()
    {
        var tags = LoadProductionTagsJson();
        var duplicates = tags
            .GroupBy(t => t.Bracket)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        Assert.Empty(duplicates);
    }

    // C6 -- defensive: every entry has a non-empty category. Catches
    // missing-category typos in the decision-table application.
    [Fact]
    public void C6_LoadAll_AllEntriesHaveNonEmptyCategory()
    {
        var tags = LoadProductionTagsJson();
        Assert.All(tags, t => Assert.False(string.IsNullOrWhiteSpace(t.Category)));
    }
}
