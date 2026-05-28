namespace SunoMetatagApp.Models;

public sealed record TagDefinition(
    string Category,
    string Label,
    string Bracket,
    string? Description = null,
    int SortOrder = 99);
