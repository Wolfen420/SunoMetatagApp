namespace SunoMetatagApp.Models;

public sealed record PromptDefinition(
    string Genre,
    string SubGenre,
    string Title,
    string Body,
    string? UseCase = null,
    string? SunoVersion = null,
    int? Energy = null,
    string? NotableFeature = null,
    string? SourceUrl = null,
    string[]? Tags = null,
    string? Difficulty = null);
