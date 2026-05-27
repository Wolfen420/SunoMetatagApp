using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.8 (B-SUNO-008a): content-coverage tests for the prompt library seed corpus.
// Loads the production prompts.json from the test project's build output (copied
// via project-reference to main project's CopyToOutput on prompts.json).
// Spec: docs/specs/2026-05-27-suno-metatag-v1.8-prompt-library-mechanism.md S6.1.
public class PromptServiceTests
{
    private static IReadOnlyList<PromptDefinition> LoadProductionPromptsJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "prompts.json");
        return PromptService.LoadAll(path);
    }

    // P1 -- LoadAll returns 16 entries (2 per genre x 8 genres = 16 seed corpus).
    [Fact]
    public void P1_LoadAll_Returns16Entries()
    {
        var prompts = LoadProductionPromptsJson();
        Assert.Equal(16, prompts.Count);
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

    // P3 -- Each of the 8 genres has exactly 2 entries (2-per-genre distribution).
    [Fact]
    public void P3_EachGenreHasExactly2Entries()
    {
        var prompts = LoadProductionPromptsJson();
        var byGenre = prompts.GroupBy(p => p.Genre).ToList();
        Assert.Equal(8, byGenre.Count);
        foreach (var group in byGenre)
        {
            Assert.True(
                group.Count() == 2,
                $"Genre '{group.Key}' has {group.Count()} entries; expected 2.");
        }
    }

    // P4 -- All 16 entries have unique Title (defense against duplicate-import bugs).
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

    // P6 -- All 16 entries have non-empty Body field (required for copy-to-clipboard).
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

    // P7 -- Forward-compat fields (Tags, Difficulty) tolerated as null in seed
    //       (no exception thrown by LoadAll). v1.8 seed leaves both null; v1.9
    //       curation can populate without re-import.
    [Fact]
    public void P7_ForwardCompatFieldsTolerateNull()
    {
        var prompts = LoadProductionPromptsJson();
        Assert.All(prompts, p =>
        {
            // Reading these properties on the loaded record must not throw.
            // v1.8 seed deliberately leaves them null.
            _ = p.Tags;
            _ = p.Difficulty;
        });
        // Confirm at least one entry has both null (seed contract).
        Assert.Contains(prompts, p => p.Tags is null && p.Difficulty is null);
    }
}
