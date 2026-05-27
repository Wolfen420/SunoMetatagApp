using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.9 (B-SUNO-008b): content-coverage tests for the full curated prompt corpus.
// Loads the production prompts.json from the test project's build output (copied
// via project-reference to main project's CopyToOutput on prompts.json).
// Spec: docs/specs/2026-05-27-suno-metatag-v1.9-prompt-library-curation.md.
// Per-genre minimum counts match decision-table source-distribution maxima
// (0 SKIPs in B-SUNO-008b-decision-table.md).
public class PromptServiceTests
{
    private static IReadOnlyList<PromptDefinition> LoadProductionPromptsJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "prompts.json");
        return PromptService.LoadAll(path);
    }

    // P1 -- LoadAll returns 136 entries (full v1.9 curated corpus; 136 ADD / 0 SKIP
    //       per docs/reference/B-SUNO-008b-decision-table.md).
    [Fact]
    public void P1_LoadAll_Returns136Entries()
    {
        var prompts = LoadProductionPromptsJson();
        Assert.Equal(136, prompts.Count);
    }

    // P2 -- DistinctGenres returns the 8 source genres.
    [Fact]
    public void P2_DistinctGenres_Returns8Genres()
    {
        var prompts = LoadProductionPromptsJson();
        var genres = PromptService.DistinctGenres(prompts);
        Assert.Equal(8, genres.Count);
        var expected = new[] { "Country", "EDM", "Hip-Hop", "Indie", "Jazz-Blues", "Pop", "R&B-Soul", "Rock" };
        Assert.Equal(expected.OrderBy(g => g), genres.OrderBy(g => g));
    }

    // P3 -- Each genre meets its per-genre minimum from the v1.9 decision table
    //       (source-distribution maxima since 0 SKIPs in v1.9 curation).
    //       Minimums: Pop 21, Rock 18, EDM 17, Hip-Hop 16, Indie 18, Jazz-Blues 18,
    //       R&B-Soul 15, Country 13 (sum = 136).
    [Fact]
    public void P3_EachGenreMeetsPerGenreMinimum()
    {
        var prompts = LoadProductionPromptsJson();
        var expectedMin = new Dictionary<string, int>
        {
            { "Pop", 21 },
            { "Rock", 18 },
            { "EDM", 17 },
            { "Hip-Hop", 16 },
            { "Indie", 18 },
            { "Jazz-Blues", 18 },
            { "R&B-Soul", 15 },
            { "Country", 13 },
        };
        foreach (var (genre, min) in expectedMin)
        {
            var count = prompts.Count(p => p.Genre.Equals(genre, System.StringComparison.Ordinal));
            Assert.True(
                count >= min,
                $"Genre '{genre}' has {count} entries; expected >= {min}.");
        }
    }

    // P4 -- All 136 entries have unique Title (defense against duplicate-import bugs).
    [Fact]
    public void P4_AllTitlesUnique()
    {
        var prompts = LoadProductionPromptsJson();
        var distinctTitles = prompts.Select(p => p.Title).Distinct().Count();
        Assert.Equal(prompts.Count, distinctTitles);
    }

    // P5 -- Per-genre selection criteria: >=1 high-energy (Energy >= 7) AND
    //       >=1 ballad/chill (Energy <= 6 OR null) per genre. Theory with 8 rows.
    [Theory]
    [InlineData("Pop")]
    [InlineData("Rock")]
    [InlineData("EDM")]
    [InlineData("Hip-Hop")]
    [InlineData("Indie")]
    [InlineData("Jazz-Blues")]
    [InlineData("R&B-Soul")]
    [InlineData("Country")]
    public void P5_EachGenreHasHighAndLowEnergyAnchors(string genre)
    {
        var prompts = LoadProductionPromptsJson();
        var inGenre = prompts.Where(p => p.Genre.Equals(genre, System.StringComparison.Ordinal)).ToList();

        Assert.True(
            inGenre.Any(p => p.Energy is int e && e >= 7),
            $"Genre '{genre}' is missing a high-energy anchor (Energy >= 7).");
        Assert.True(
            inGenre.Any(p => p.Energy is null || p.Energy <= 6),
            $"Genre '{genre}' is missing a ballad/chill anchor (Energy <= 6 or null).");
    }

    // P6 -- All entries have non-empty Body field (required for copy-to-clipboard).
    [Fact]
    public void P6_AllEntriesHaveNonEmptyBody()
    {
        var prompts = LoadProductionPromptsJson();
        foreach (var p in prompts)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(p.Body),
                $"Prompt '{p.Title}' has empty Body.");
        }
    }

    // P7 -- Forward-compat fields (Tags, Difficulty) tolerated as null in corpus
    //       (no exception thrown by LoadAll). v1.9 corpus leaves both null; future
    //       cycles can populate without re-import.
    [Fact]
    public void P7_ForwardCompatFieldsTolerateNull()
    {
        var prompts = LoadProductionPromptsJson();
        Assert.All(prompts, p =>
        {
            // Reading these properties on the loaded record must not throw.
            // v1.9 corpus deliberately leaves them null.
            _ = p.Tags;
            _ = p.Difficulty;
        });
        // Confirm at least one entry has both null (corpus contract).
        Assert.Contains(prompts, p => p.Tags is null && p.Difficulty is null);
    }

    // P8 -- High-utility spot-check: five canonical entries from the v1.9 corpus
    //       must be present by exact Title match. Defends against silent
    //       re-import / parser regressions that would drop or rename entries.
    //       All 5 entries verified as ADD rows in the v1.9 decision table.
    [Fact]
    public void P8_HighUtilityEntriesPresent()
    {
        var prompts = LoadProductionPromptsJson();
        var requiredTitles = new[]
        {
            "Modern Pop Anthem (Female Vocals)",
            "Epic Arena Anthem",
            "Big Room House Anthem",
            "Modern Trap Anthem",
            "Classic Big Band Swing",
        };
        foreach (var title in requiredTitles)
        {
            Assert.True(
                prompts.Any(p => p.Title.Equals(title, System.StringComparison.Ordinal)),
                $"v1.9 corpus is missing required entry by Title: '{title}'.");
        }
    }
}
