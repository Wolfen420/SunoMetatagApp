using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.6 (B-SUNO-007): content-coverage tests for sunoaiwiki metatag list reconciliation.
// Spec: docs/specs/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md §7.1.
// Validates 45 ADD distribution across 5 existing categories + cross-category
// coexistence cases + sec 4 Structural-Tags 100% SKIP-as-canonical-present.
public class TagServiceSunoaiwikiMetatagListTests
{
    private static IReadOnlyList<TagDefinition> LoadProductionTagsJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
        return TagService.LoadAll(path);
    }

    // H1 -- after v1.6 reconciliation, tags.json holds at least 320 entries
    // (286 v1.5 baseline + 45 v1.6 ADDs after T1 Clapping self-correction).
    [Fact]
    public void H1_LoadAll_LoadsExpectedMinimumCount()
    {
        var tags = LoadProductionTagsJson();
        Assert.True(
            tags.Count >= 320,
            $"Expected >= 320 tags after B-SUNO-007, got {tags.Count}.");
    }

    // H2 -- the 5 extended categories grew per plan. Per-category extension
    // verification (post-T1-Clapping-self-correction thresholds).
    [Theory]
    [InlineData("Vocal", 45)]
    [InlineData("Instrument", 36)]
    [InlineData("Production", 6)]
    [InlineData("SFX", 63)]
    [InlineData("Genre", 107)]
    public void H2_ExtendedCategoryCountsMet(string category, int expectedMin)
    {
        var tags = LoadProductionTagsJson();
        var count = tags.Count(t => t.Category == category);
        Assert.True(
            count >= expectedMin,
            $"Expected >= {expectedMin} entries in '{category}', got {count}.");
    }

    // H3 -- representative new entries from each canonicalization class are
    // searchable end-to-end via Filter. Covers all 5 extended categories +
    // cross-category coexistence pairs from spec sec 3.3.
    [Theory]
    [InlineData("Barking", "[Barking]")]              // SFX new
    [InlineData("Phone Ringing", "[Phone Ringing]")]  // SFX cross-cat (distinct from Bell Ringing)
    [InlineData("Announcer", "[Announcer]")]          // Vocal new voice-type
    [InlineData("Female Narrator", "[Female Narrator]")] // Vocal multi-word
    [InlineData("Boy", "[Boy]")]                      // Vocal voice-type
    [InlineData("Girl", "[Girl]")]                    // Vocal voice-type
    [InlineData("Silence", "[Silence]")]              // Production new
    [InlineData("EDM", "[EDM]")]                      // Genre acronym
    [InlineData("Pop-Rock", "[Pop-Rock]")]            // Genre hyphen Title-Case
    [InlineData("Christmas", "[Christmas]")]          // Genre new
    [InlineData("Lo-Fi", "[Lo-Fi]")]                  // Genre cross-cat (coexists with Effect: Lo-fi)
    [InlineData("Drums", "[Drums]")]                  // Instrument cross-cat (vs Structure Drum Solo)
    public void H3_Filter_FindsRepresentativeNewEntries(string searchTerm, string expectedBracket)
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, searchTerm, "All").ToList();
        Assert.Contains(hits, t => t.Bracket == expectedBracket);
    }

    // H4 -- all 8 expected categories still non-empty (extends v1.5 G6).
    [Fact]
    public void H4_AllExpectedCategories_NonEmpty()
    {
        var tags = LoadProductionTagsJson();
        var expectedCategories = new[]
        {
            "Structure", "Vocal", "Instrument", "Mood",
            "Effect", "Production", "SFX", "Genre"
        };
        foreach (var category in expectedCategories)
        {
            var count = tags.Count(t => t.Category == category);
            Assert.True(count > 0, $"Expected non-empty category '{category}' but found 0 entries.");
        }
    }

    // H5 -- no bracket collisions across all 335 entries (extends v1.5 G5;
    // post-v1.14 count = 331 v1.6 baseline + 4 v1.14 Verse 3-6 entries).
    // This test catches the T1 Clapping collision that v1.4 C5 + v1.5 G5
    // also catch; redundant by design (defense in depth).
    [Fact]
    public void H5_LoadAll_NoBracketCollisionsAcrossAllCategories()
    {
        var tags = LoadProductionTagsJson();
        var duplicates = tags
            .GroupBy(t => t.Bracket)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        Assert.Empty(duplicates);
    }

    // H6 -- all 4 Structural source items (Chorus, Intro, Outro, Verse)
    // exist in Structure category. Verifies SKIP-as-canonical-present
    // decisions in sec 4 of decision table (100% overlap with v1 baseline).
    [Theory]
    [InlineData("[Chorus]")]
    [InlineData("[Intro]")]
    [InlineData("[Outro]")]
    [InlineData("[Verse]")]
    public void H6_StructuralSourceItems_PresentInStructure(string expectedBracket)
    {
        var tags = LoadProductionTagsJson();
        var entry = tags.SingleOrDefault(t => t.Bracket == expectedBracket);
        Assert.NotNull(entry);
        Assert.Equal("Structure", entry!.Category);
    }

    // H7 -- v1.14 (B-SUNO-015) Verse-cluster extension: [Verse 3] through
    // [Verse 6] added as Structure entries to match existing [Verse], [Verse 1],
    // [Verse 2]. Mirrors H6 pattern for the new bracket set. Per v1.11 lexical
    // sort, the cluster renders as [Verse 1] < [Verse 2] < [Verse 3] < ... <
    // [Verse 6] < [Verse] in the picker (space < ']' in ordinal comparison).
    [Theory]
    [InlineData("[Verse 3]")]
    [InlineData("[Verse 4]")]
    [InlineData("[Verse 5]")]
    [InlineData("[Verse 6]")]
    public void H7_ExtendedVerseCluster_PresentInStructure(string expectedBracket)
    {
        var tags = LoadProductionTagsJson();
        var entry = tags.SingleOrDefault(t => t.Bracket == expectedBracket);
        Assert.NotNull(entry);
        Assert.Equal("Structure", entry!.Category);
    }
}
