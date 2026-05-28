using System.Collections.Generic;

namespace SunoMetatagApp.Models;

// v1.18 (B-025): canonical song-structure template — a named list of SectionType
// strings that LoadTemplateCommand consumes to pre-populate the section stack.
public sealed record SongTemplate(string Name, IReadOnlyList<string> SectionTypes);
