using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.5 (B-SUNO-006): content-coverage tests for genre taxonomy import.
// Spec: docs/specs/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md §7.1.
// G3 search list extended per specialist plan-phase advisory LOW 3 (2026-05-27)
// to cover unusual canonicalizations (Muzak, K-Pop, R&B, Heavy Metal, Avant-garde).
public class TagServiceGenreTaxonomyTests
{
    private static IReadOnlyList<TagDefinition> LoadProductionTagsJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
        return TagService.LoadAll(path);
    }

    // G1 -- after v1.5 reconciliation, tags.json holds at least 270 entries
    // (199 v1.4 baseline + 87 Genre ADDs per Lead-ratified decision table
    // including T0 LOW 1 absorption for [Avant-garde] sibling parent).
    [Fact]
    public void G1_LoadAll_LoadsExpectedMinimumCount()
    {
        var tags = LoadProductionTagsJson();
        Assert.True(
            tags.Count >= 270,
            $"Expected >= 270 tags after B-SUNO-006, got {tags.Count}.");
    }

    // G2 -- new Genre category lands with at least 70 entries
    // (planner-conservative threshold; actual 87 after LOW 1 absorption).
    [Fact]
    public void G2_GenreCategory_HasExpectedMinimumCount()
    {
        var tags = LoadProductionTagsJson();
        var genreCount = tags.Count(t => t.Category == "Genre");
        Assert.True(
            genreCount >= 70,
            $"Expected >= 70 Genre entries, got {genreCount}.");
    }

    // G3 -- representative entries from each canonicalization class are
    // searchable end-to-end via Filter. Extended per specialist LOW 3.
    [Theory]
    [InlineData("Rock", "[Rock]")]
    [InlineData("Jazz", "[Jazz]")]
    [InlineData("Hip Hop", "[Hip Hop]")]
    [InlineData("Electronic", "[Electronic]")]
    [InlineData("Reggae", "[Reggae]")]
    [InlineData("Bebop", "[Bebop]")]
    [InlineData("Bossa Nova", "[Bossa Nova]")]
    [InlineData("Muzak", "[Muzak]")]            // §D.2 parenthetical canonical
    [InlineData("K-Pop", "[K-Pop]")]            // §I.4 hyphen Title-Case
    [InlineData("R&B", "[R&B]")]                // §B.5 abbreviation canonical
    [InlineData("Heavy Metal", "[Heavy Metal]")] // §L.3 Metal section
    [InlineData("Avant-garde", "[Avant-garde]")] // §A.0a LOW 1 sibling parent
    public void G3_Filter_FindsRepresentativeGenreEntries(string searchTerm, string expectedBracket)
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, searchTerm, "All").ToList();
        Assert.Contains(hits, t => t.Bracket == expectedBracket);
    }

    // G4 -- category filter for Genre returns ONLY Genre entries (no bleed
    // from Structure/Vocal/Instrument/Mood/Effect/Production/SFX).
    [Fact]
    public void G4_Filter_ByGenreCategory_ReturnsOnlyGenreEntries()
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, "", "Genre").ToList();
        Assert.NotEmpty(hits);
        Assert.All(hits, t => Assert.Equal("Genre", t.Category));
    }

    // G5 -- defensive: no two entries share a bracket string. Catches
    // copy-paste errors at T1 and accidental Genre-vs-existing collisions.
    // Equivalent to v1.4 C5 but explicitly carried forward to guard the
    // larger 286-entry surface.
    [Fact]
    public void G5_LoadAll_NoBracketCollisionsAcrossAllCategories()
    {
        var tags = LoadProductionTagsJson();
        var duplicates = tags
            .GroupBy(t => t.Bracket)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        Assert.Empty(duplicates);
    }

    // G6 -- defensive: all 8 expected top-level categories are present and
    // non-empty (extends v1.4 C6 from 7 to 8 with new Genre category).
    [Fact]
    public void G6_AllExpectedCategories_NonEmpty()
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
}
