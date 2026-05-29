using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SunoMetatagApp.Models;

// v1.20 (B-028): JSON DTO for user-defined templates persisted to
// %APPDATA%\SunoMetatagApp\templates.json. Public surface remains SongTemplate;
// this DTO exists solely so UserTemplateService can round-trip mutable lists.
internal sealed class UserTemplateDto
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("sectionTypes")] public List<string>? SectionTypes { get; set; }
}
