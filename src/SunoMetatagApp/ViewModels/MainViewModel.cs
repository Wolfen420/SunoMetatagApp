using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;

namespace SunoMetatagApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<TagDefinition> _allTags;

    public ObservableCollection<Section> Sections { get; } = new();
    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedCategory = "All";
    [ObservableProperty] private string _lyricText = string.Empty;
    [ObservableProperty] private IReadOnlyList<TagViewModel> _filteredTags = Array.Empty<TagViewModel>();
    [ObservableProperty] private string _previewText = string.Empty;
    [ObservableProperty] private string? _loadError;
    [ObservableProperty] private bool _showArmHint;
    [ObservableProperty] private int _armedSectionCount;

    public event EventHandler? CopyRequested;

    public MainViewModel(IReadOnlyList<TagDefinition> tags)
    {
        _allTags = tags;
        Categories = BuildCategories(tags);
        SelectedCategory = "All";
        FilteredTags = ComputeFiltered();
        Sections.CollectionChanged += OnSectionsChanged;
        AddSection();
    }

    public MainViewModel(string loadError)
    {
        _allTags = Array.Empty<TagDefinition>();
        Categories = new[] { "All" };
        SelectedCategory = "All";
        FilteredTags = Array.Empty<TagViewModel>();
        LoadError = loadError;
        Sections.CollectionChanged += OnSectionsChanged;
    }

    [RelayCommand]
    private void AddSection() => Sections.Add(new Section());

    [RelayCommand]
    private void RemoveSection(Section? section)
    {
        if (section is null) return;
        if (Sections.Count <= 1) return;
        Sections.Remove(section);
    }

    [RelayCommand]
    private void MoveSectionUp(Section? section)
    {
        if (section is null) return;
        var i = Sections.IndexOf(section);
        if (i > 0) Sections.Move(i, i - 1);
    }

    [RelayCommand]
    private void MoveSectionDown(Section? section)
    {
        if (section is null) return;
        var i = Sections.IndexOf(section);
        if (i >= 0 && i < Sections.Count - 1) Sections.Move(i, i + 1);
    }

    [RelayCommand]
    private void InsertTag(TagViewModel? tag)
    {
        if (tag is null) return;
        var armed = Sections.Where(s => s.IsArmed).ToList();
        if (armed.Count == 0)
        {
            ShowArmHint = true;
            return;
        }
        ShowArmHint = false;
        foreach (var s in armed)
            s.Tags.Add(tag.Definition);
    }

    [RelayCommand]
    private void CopyPreview() => CopyRequested?.Invoke(this, EventArgs.Empty);

    partial void OnSearchTextChanged(string value) => FilteredTags = ComputeFiltered();
    partial void OnSelectedCategoryChanged(string value) => FilteredTags = ComputeFiltered();

    private void OnSectionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (Section s in e.NewItems) SubscribeToSection(s);
        if (e.OldItems != null)
            foreach (Section s in e.OldItems) UnsubscribeFromSection(s);
        RecomputePreview();
        RecomputeArmedCount();
    }

    private void SubscribeToSection(Section s)
    {
        s.PropertyChanged += OnSectionPropertyChanged;
        s.Tags.CollectionChanged += OnSectionTagsChanged;
    }

    private void UnsubscribeFromSection(Section s)
    {
        s.PropertyChanged -= OnSectionPropertyChanged;
        s.Tags.CollectionChanged -= OnSectionTagsChanged;
    }

    private void OnSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Section.Lyrics))
        {
            RecomputePreview();
        }
        else if (e.PropertyName == nameof(Section.IsArmed))
        {
            RecomputeArmedCount();
            if (sender is Section s && s.IsArmed) ShowArmHint = false;
        }
    }

    private void OnSectionTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => RecomputePreview();

    private void RecomputePreview()
        => PreviewText = PreviewBuilder.Build(Sections.ToList(), Environment.NewLine);

    private void RecomputeArmedCount()
        => ArmedSectionCount = Sections.Count(s => s.IsArmed);

    private static IReadOnlyList<string> BuildCategories(IEnumerable<TagDefinition> tags)
    {
        var distinct = TagService.DistinctCategories(tags);
        var list = new List<string>(distinct.Count + 1) { "All" };
        list.AddRange(distinct);
        return list;
    }

    private IReadOnlyList<TagViewModel> ComputeFiltered() =>
        TagService.Filter(_allTags, SearchText, SelectedCategory)
                  .Select(t => new TagViewModel(t))
                  .ToList();
}
