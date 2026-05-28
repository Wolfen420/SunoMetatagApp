using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

public sealed class TagLoadException : Exception
{
    public TagLoadException(string message, Exception? inner = null) : base(message, inner) { }
}

public static class TagService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IReadOnlyList<TagDefinition> LoadAll(string path)
    {
        if (!File.Exists(path))
            throw new TagLoadException($"tags.json not found at '{path}'.");

        string json;
        try { json = File.ReadAllText(path); }
        catch (Exception ex)
        {
            throw new TagLoadException($"Could not read tags.json at '{path}': {ex.Message}", ex);
        }

        List<TagDto>? dtos;
        try { dtos = JsonSerializer.Deserialize<List<TagDto>>(json, JsonOpts); }
        catch (JsonException ex)
        {
            throw new TagLoadException($"tags.json is not valid JSON: {ex.Message}", ex);
        }

        if (dtos is null)
            throw new TagLoadException("tags.json deserialized to null.");

        var result = new List<TagDefinition>(dtos.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            var d = dtos[i];
            if (string.IsNullOrWhiteSpace(d.Category))
                throw new TagLoadException($"tags.json entry {i}: missing required field 'category'.");
            if (string.IsNullOrWhiteSpace(d.Label))
                throw new TagLoadException($"tags.json entry {i}: missing required field 'label'.");
            if (string.IsNullOrWhiteSpace(d.Bracket))
                throw new TagLoadException($"tags.json entry {i}: missing required field 'bracket'.");

            result.Add(new TagDefinition(d.Category!, d.Label!, d.Bracket!, d.Description, d.SortOrder ?? 99));
        }
        return result;
    }

    public static IReadOnlyList<string> DistinctCategories(IEnumerable<TagDefinition> tags) =>
        tags.Select(t => t.Category)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToList();

    public static IEnumerable<TagDefinition> Filter(
        IEnumerable<TagDefinition> tags,
        string? search,
        string? category)
    {
        bool categoryMatches(TagDefinition t) =>
            string.IsNullOrEmpty(category) ||
            category.Equals("All", StringComparison.Ordinal) ||
            t.Category.Equals(category, StringComparison.Ordinal);

        bool searchMatches(TagDefinition t)
        {
            if (string.IsNullOrEmpty(search)) return true;
            var normalizedSearch = NormalizeForSearch(search);
            if (normalizedSearch.Length == 0) return true;
            return NormalizeForSearch(t.Label).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                || NormalizeForSearch(t.Bracket).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);
        }

        // v1.11 (B-SUNO-011): alphabetical sort by raw Bracket text, case-insensitive ordinal.
        // Applies AFTER filter; sort uses display text (Bracket), not the v1.7-normalized search form.
        // Prefix-form entries (e.g., [Mood: Euphoric]) cluster by namespace because the [Namespace:
        // prefix is constant within each cluster; sub-order falls out to the post-colon human word.
        // Caveat: lexical (not numeric) — [Verse 1], [Verse 2], [Verse] order because space (0x20)
        // sorts before ] (0x5D). Stable per LINQ OrderBy contract.
        return tags
            .Where(t => categoryMatches(t) && searchMatches(t))
            .OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeForSearch(string s) =>
        s.Replace("-", "", StringComparison.Ordinal)
         .Replace(" ", "", StringComparison.Ordinal);

    private sealed class TagDto
    {
        [JsonPropertyName("category")]    public string? Category { get; set; }
        [JsonPropertyName("label")]       public string? Label { get; set; }
        [JsonPropertyName("bracket")]     public string? Bracket { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        // v1.17 (B-026): canonical role sortOrder (1 Structure, 2 Vocal, 3 Instrument,
        // 4 Mood, 5 Effect, 6 SFX, 7 Production). Nullable so a missing JSON field
        // coalesces to the 99 default at construction time (System.Text.Json would
        // otherwise yield 0 for a non-nullable int).
        [JsonPropertyName("sortOrder")]   public int? SortOrder { get; set; }
    }
}
