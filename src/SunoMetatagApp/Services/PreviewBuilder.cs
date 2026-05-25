using System.Collections.Generic;
using System.Linq;
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

public static class PreviewBuilder
{
    public static string Build(IReadOnlyList<Section> sections, string newline)
    {
        if (sections is null || sections.Count == 0) return string.Empty;
        var nonEmpty = sections
            .Where(s => !string.IsNullOrWhiteSpace(s.Lyrics))
            .Select(s => s.Lyrics);
        return string.Join(newline + newline, nonEmpty).TrimEnd('\r', '\n');
    }
}
