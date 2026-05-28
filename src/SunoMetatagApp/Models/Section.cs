using CommunityToolkit.Mvvm.ComponentModel;

namespace SunoMetatagApp.Models;

public sealed partial class Section : ObservableObject
{
    [ObservableProperty] private string _lyrics = "";
    // v1.18 (B-025): canonical section label set by LoadTemplateCommand from
    // SongTemplate.SectionTypes. Default empty for sections created via plain
    // AddSection() (no template). NOT included in OnSectionPropertyChanged's
    // RecomputePreview filter — SectionType is metadata, not preview content.
    [ObservableProperty] private string _sectionType = "";
}
