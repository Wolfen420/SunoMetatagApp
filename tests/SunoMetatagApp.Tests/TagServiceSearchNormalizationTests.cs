using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.7 (B-SUNO-009): content-coverage tests for hyphen/space-insensitive
// search normalization in TagService.Filter.
// Spec: docs/specs/2026-05-27-suno-metatag-v1.7-search-normalization.md sec 6.1.
// Resolves v1.5 PASS-WITH-CONCERN class (kpop -> [K-Pop] surfacing).
// N6 absorbs Lead PASS-WITH-NOTES item #2 (explicit empty-normalized-search
// edge-case coverage); N6 expanded from [Fact] to [Theory] vs plan §T2 to
// cover all empty-normalized variants.
public class TagServiceSearchNormalizationTests
{
    private static IReadOnlyList<TagDefinition> LoadProductionTagsJson()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "tags.json");
        return TagService.LoadAll(path);
    }

    // N1 -- empty/null search returns the full set (no normalization applied).
    [Fact]
    public void N1_Filter_EmptySearch_ReturnsAllEntries()
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, "", "All").ToList();
        Assert.Equal(tags.Count, hits.Count);

        var hitsNull = TagService.Filter(tags, null, "All").ToList();
        Assert.Equal(tags.Count, hitsNull.Count);
    }

    // N2 -- single-character query still matches (non-regression vs v1.6
    // literal-substring behavior). 'k' normalized = 'k'; should still hit
    // any entry whose normalized Label/Bracket contains 'k'.
    [Fact]
    public void N2_Filter_SingleCharacterQuery_StillMatches()
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, "k", "All").ToList();
        Assert.Contains(hits, t => t.Bracket == "[K-Pop]");
    }

    // N3 -- existing literal-substring queries continue to work
    // (strict-superset semantics: anything that matched pre-v1.7 still matches).
    [Fact]
    public void N3_Filter_LiteralSubstring_StillMatches()
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, "pop", "All").ToList();
        Assert.Contains(hits, t => t.Bracket == "[K-Pop]");
        Assert.Contains(hits, t => t.Bracket == "[Pop-Rock]");
    }

    // N4 -- representative normalization pairs across hyphen-strip, space-strip,
    // and mixed-case variants. Each row: a search that did NOT match pre-v1.7
    // now surfaces the expected bracket.
    [Theory]
    [InlineData("kpop", "[K-Pop]")]            // hyphen-strip + case
    [InlineData("poprock", "[Pop-Rock]")]      // hyphen-strip multi-word
    [InlineData("lofi", "[Lo-Fi]")]            // hyphen-strip Genre
    [InlineData("hiphop", "[Hip Hop]")]        // space-strip
    [InlineData("bossanova", "[Bossa Nova]")]  // space-strip multi-word
    [InlineData("K-Pop", "[K-Pop]")]           // with-hyphen literal (non-regression)
    [InlineData("k pop", "[K-Pop]")]           // space variant of hyphen target
    [InlineData("hip hop", "[Hip Hop]")]       // existing literal multi-word
    public void N4_Filter_NormalizesHyphenAndSpace(string searchTerm, string expectedBracket)
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, searchTerm, "All").ToList();
        Assert.Contains(hits, t => t.Bracket == expectedBracket);
    }

    // N5 -- case-insensitivity preserved across normalized comparison.
    // Both 'KPOP' (upper) and 'kpop' (lower) surface [K-Pop].
    [Fact]
    public void N5_Filter_NormalizedComparison_IsCaseInsensitive()
    {
        var tags = LoadProductionTagsJson();
        var upper = TagService.Filter(tags, "KPOP", "All").ToList();
        var lower = TagService.Filter(tags, "kpop", "All").ToList();
        Assert.Contains(upper, t => t.Bracket == "[K-Pop]");
        Assert.Contains(lower, t => t.Bracket == "[K-Pop]");
    }

    // N6 -- empty-normalized inputs ('-', ' ', '--', '   ') return the full set,
    // matching empty-search semantics rather than collapsing to Contains("")
    // which would also return all but via a surprising path. Absorbs Lead
    // PASS-WITH-NOTES item #2 explicit edge-case coverage.
    [Theory]
    [InlineData("-")]
    [InlineData(" ")]
    [InlineData("--")]
    [InlineData("   ")]
    public void N6_Filter_EmptyNormalizedSearch_ReturnsAllEntries(string searchTerm)
    {
        var tags = LoadProductionTagsJson();
        var hits = TagService.Filter(tags, searchTerm, "All").ToList();
        Assert.Equal(tags.Count, hits.Count);
    }
}
