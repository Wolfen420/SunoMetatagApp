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

            result.Add(new TagDefinition(d.Category!, d.Label!, d.Bracket!, d.Description));
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
            return t.Label.Contains(search, StringComparison.OrdinalIgnoreCase)
                || t.Bracket.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        return tags.Where(t => categoryMatches(t) && searchMatches(t));
    }

    private sealed class TagDto
    {
        [JsonPropertyName("category")]    public string? Category { get; set; }
        [JsonPropertyName("label")]       public string? Label { get; set; }
        [JsonPropertyName("bracket")]     public string? Bracket { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }
}
