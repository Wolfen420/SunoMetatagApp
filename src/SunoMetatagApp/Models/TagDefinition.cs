using System.Collections.Generic;

namespace SunoMetatagApp.Models;

public sealed record TagDefinition(
    string Category,
    string Label,
    string Bracket,
    string? Description = null,
    int SortOrder = 99,
    IReadOnlyList<string>? Aliases = null);
