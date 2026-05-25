using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SunoMetatagApp.Models;

public sealed partial class Section : ObservableObject
{
    [ObservableProperty] private string _lyrics = "";
    [ObservableProperty] private bool _isArmed = true;

    public ObservableCollection<TagDefinition> Tags { get; } = new();

    [RelayCommand]
    private void RemoveTag(TagDefinition? tag)
    {
        if (tag != null) Tags.Remove(tag);
    }

    [RelayCommand]
    private void MoveTagLeft(TagDefinition? tag)
    {
        if (tag is null) return;
        var i = Tags.IndexOf(tag);
        if (i > 0) Tags.Move(i, i - 1);
    }

    [RelayCommand]
    private void MoveTagRight(TagDefinition? tag)
    {
        if (tag is null) return;
        var i = Tags.IndexOf(tag);
        if (i >= 0 && i < Tags.Count - 1) Tags.Move(i, i + 1);
    }
}
