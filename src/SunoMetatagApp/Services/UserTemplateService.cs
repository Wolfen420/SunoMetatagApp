using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SunoMetatagApp.Models;

namespace SunoMetatagApp.Services;

// v1.20 (B-028): persistence layer for user-defined song-structure templates.
// Schema is a flat array of {name, sectionTypes[]} entries. Stored at
// %APPDATA%\SunoMetatagApp\templates.json by default; constructor-injected path
// for testability. LoadAll is defensive — returns empty list on missing file or
// malformed JSON so the app remains usable. SaveAll writes atomically via a
// .tmp file + File.Move(overwrite:true).
public sealed class UserTemplateService
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string TemplatesPath { get; }

    public UserTemplateService(string templatesPath)
    {
        TemplatesPath = templatesPath;
    }

    public static UserTemplateService CreateDefault() =>
        new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SunoMetatagApp",
            "templates.json"));

    public IReadOnlyList<SongTemplate> LoadAll()
    {
        if (!File.Exists(TemplatesPath)) return Array.Empty<SongTemplate>();
        try
        {
            var json = File.ReadAllText(TemplatesPath);
            var dtos = JsonSerializer.Deserialize<UserTemplateDto[]>(json, _opts)
                       ?? Array.Empty<UserTemplateDto>();
            return dtos
                .Where(d => !string.IsNullOrWhiteSpace(d.Name)
                            && d.SectionTypes is { Count: > 0 })
                .Select(d => new SongTemplate(d.Name!, d.SectionTypes!.ToList()))
                .ToList();
        }
        catch
        {
            return Array.Empty<SongTemplate>();
        }
    }

    public void SaveAll(IReadOnlyList<SongTemplate> templates)
    {
        var dir = Path.GetDirectoryName(TemplatesPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var dtos = templates.Select(t => new UserTemplateDto
        {
            Name = t.Name,
            SectionTypes = t.SectionTypes.ToList(),
        }).ToArray();

        var tempPath = TemplatesPath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(dtos, _opts));
        File.Move(tempPath, TemplatesPath, overwrite: true);
    }
}
