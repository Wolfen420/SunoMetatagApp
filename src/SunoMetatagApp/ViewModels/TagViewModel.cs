using SunoMetatagApp.Models;

namespace SunoMetatagApp.ViewModels;

public sealed class TagViewModel
{
    public TagViewModel(TagDefinition definition) { Definition = definition; }
    public TagDefinition Definition { get; }
    public string Label        => Definition.Label;
    public string Bracket      => Definition.Bracket;
    public string Category     => Definition.Category;
    public string? Description => Definition.Description;
}
