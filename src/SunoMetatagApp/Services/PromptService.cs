using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

public sealed class PromptLoadException : Exception
{
    public PromptLoadException(string message, Exception? inner = null) : base(message, inner) { }
}

public static class PromptService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IReadOnlyList<PromptDefinition> LoadAll(string path)
    {
        if (!File.Exists(path))
            throw new PromptLoadException($"prompts.json not found at '{path}'.");

        string json;
        try { json = File.ReadAllText(path); }
        catch (Exception ex)
        {
            throw new PromptLoadException($"Could not read prompts.json at '{path}': {ex.Message}", ex);
        }

        List<PromptDto>? dtos;
        try { dtos = JsonSerializer.Deserialize<List<PromptDto>>(json, JsonOpts); }
        catch (JsonException ex)
        {
            throw new PromptLoadException($"prompts.json is not valid JSON: {ex.Message}", ex);
        }

        if (dtos is null)
            throw new PromptLoadException("prompts.json deserialized to null.");

        var result = new List<PromptDefinition>(dtos.Count);
        for (int i = 0; i < dtos.Count; i++)
        {
            var d = dtos[i];
            if (string.IsNullOrWhiteSpace(d.Genre))
                throw new PromptLoadException($"prompts.json entry {i}: missing required field 'genre'.");
            if (string.IsNullOrWhiteSpace(d.Title))
                throw new PromptLoadException($"prompts.json entry {i}: missing required field 'title'.");
            if (string.IsNullOrWhiteSpace(d.Body))
                throw new PromptLoadException($"prompts.json entry {i}: missing required field 'body'.");

            result.Add(new PromptDefinition(
                Genre: d.Genre!,
                SubGenre: d.SubGenre ?? string.Empty,
                Title: d.Title!,
                Body: d.Body!,
                UseCase: d.UseCase,
                SunoVersion: d.SunoVersion,
                Energy: d.Energy,
                NotableFeature: d.NotableFeature,
                SourceUrl: d.SourceUrl,
                Tags: d.Tags,
                Difficulty: d.Difficulty));
        }
        return result;
    }

    public static IReadOnlyList<string> DistinctGenres(IEnumerable<PromptDefinition> prompts) =>
        prompts.Select(p => p.Genre)
               .Distinct(StringComparer.Ordinal)
               .OrderBy(g => g, StringComparer.Ordinal)
               .ToList();

    public static IEnumerable<PromptDefinition> Filter(
        IEnumerable<PromptDefinition> prompts,
        string? genre)
    {
        if (string.IsNullOrEmpty(genre) || genre.Equals("All", StringComparison.Ordinal))
            return prompts;

        return prompts.Where(p => p.Genre.Equals(genre, StringComparison.Ordinal));
    }

    private sealed class PromptDto
    {
        [JsonPropertyName("genre")]          public string? Genre { get; set; }
        [JsonPropertyName("subGenre")]       public string? SubGenre { get; set; }
        [JsonPropertyName("title")]          public string? Title { get; set; }
        [JsonPropertyName("body")]           public string? Body { get; set; }
        [JsonPropertyName("useCase")]        public string? UseCase { get; set; }
        [JsonPropertyName("sunoVersion")]    public string? SunoVersion { get; set; }
        [JsonPropertyName("energy")]         public int? Energy { get; set; }
        [JsonPropertyName("notableFeature")] public string? NotableFeature { get; set; }
        [JsonPropertyName("sourceUrl")]      public string? SourceUrl { get; set; }
        [JsonPropertyName("tags")]           public string[]? Tags { get; set; }
        [JsonPropertyName("difficulty")]     public string? Difficulty { get; set; }
    }
}
