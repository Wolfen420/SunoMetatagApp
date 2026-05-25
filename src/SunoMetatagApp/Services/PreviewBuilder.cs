using System.Collections.Generic;
using System.Linq;
using System.Text;
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

public static class PreviewBuilder
{
    public static string Build(IReadOnlyList<Section> sections, string newline)
    {
        var sb = new StringBuilder();
        var rendered = sections
            .Where(s => s.Tags.Count > 0 || !string.IsNullOrEmpty(s.Lyrics))
            .ToList();

        for (int i = 0; i < rendered.Count; i++)
        {
            var s = rendered[i];

            foreach (var tag in s.Tags)
                sb.Append(tag.Bracket).Append(newline);

            if (!string.IsNullOrEmpty(s.Lyrics))
                sb.Append(s.Lyrics);

            if (i < rendered.Count - 1)
            {
                while (sb.Length > 0 && (sb[sb.Length - 1] == '\n' || sb[sb.Length - 1] == '\r'))
                    sb.Length--;
                sb.Append(newline).Append(newline);
            }
        }

        while (sb.Length > 0 && (sb[sb.Length - 1] == '\n' || sb[sb.Length - 1] == '\r'))
            sb.Length--;

        return sb.ToString();
    }
}
