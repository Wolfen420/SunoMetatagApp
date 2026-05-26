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
    [ObservableProperty] private IReadOnlyList<TagViewModel> _filteredTags = Array.Empty<TagViewModel>();
    [ObservableProperty] private string _previewText = string.Empty;
    [ObservableProperty] private string? _loadError;

    [ObservableProperty] private Section? _focusedSection;
    [ObservableProperty] private int _focusedCaretPosition;
    [ObservableProperty] private int _focusedSelectionLength;

    public event EventHandler? CopyRequested;
    public event EventHandler<int>? CaretRestoreRequested;

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
        AddSection();
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

    [RelayCommand(CanExecute = nameof(CanMoveSectionUp))]
    private void MoveSectionUp(Section? section)
    {
        if (section is null) return;
        var i = Sections.IndexOf(section);
        if (i > 0) Sections.Move(i, i - 1);
    }

    private bool CanMoveSectionUp(Section? section)
    {
        if (section is null) return false;
        return Sections.IndexOf(section) > 0;
    }

    [RelayCommand(CanExecute = nameof(CanMoveSectionDown))]
    private void MoveSectionDown(Section? section)
    {
        if (section is null) return;
        var i = Sections.IndexOf(section);
        if (i >= 0 && i < Sections.Count - 1) Sections.Move(i, i + 1);
    }

    private bool CanMoveSectionDown(Section? section)
    {
        if (section is null) return false;
        var i = Sections.IndexOf(section);
        return i >= 0 && i < Sections.Count - 1;
    }

    [RelayCommand]
    private void InsertTag(TagViewModel? tag)
    {
        if (tag is null || FocusedSection is null) return;
        var section = FocusedSection;
        var lyrics = section.Lyrics ?? string.Empty;
        var caret = Math.Clamp(FocusedCaretPosition, 0, lyrics.Length);
        var selLen = Math.Clamp(FocusedSelectionLength, 0, Math.Max(0, lyrics.Length - caret));
        var bracket = tag.Bracket;
        section.Lyrics = lyrics[..caret] + bracket + lyrics[(caret + selLen)..];
        FocusedCaretPosition = caret + bracket.Length;
        FocusedSelectionLength = 0;
        CaretRestoreRequested?.Invoke(this, FocusedCaretPosition);
    }

    // v1.3 (B-SUNO-004): Shift+click on a tag in the picker invokes this command.
    // Walks left from the caret on the current line for a complete [...] bracket
    // (or detects caret-inside-bracket); appends " | <inner-name>" before its ].
    // Falls back to plain InsertTag when no merge target exists on the current line.
    // See docs/specs/2026-05-26-suno-metatag-v1.3-stacked-syntax.md §3 + §6.1.
    [RelayCommand]
    private void InsertTagStacked(TagViewModel? tag)
    {
        if (tag is null || FocusedSection is null) { InsertTag(tag); return; }
        var section = FocusedSection;
        var lyrics = section.Lyrics ?? string.Empty;
        var caret = Math.Clamp(FocusedCaretPosition, 0, lyrics.Length);

        // 3.1 line bounds
        int lineStart = caret == 0 ? 0 : lyrics.LastIndexOf('\n', caret - 1) + 1;
        int lineEnd = lyrics.IndexOf('\n', caret);
        if (lineEnd < 0) lineEnd = lyrics.Length;

        int? appendAt = null;

        // 3.2 caret-inside-bracket
        if (caret > lineStart)
        {
            int lastOpen = lyrics.LastIndexOf('[', caret - 1);
            if (lastOpen >= lineStart)
            {
                int closeAfter = lyrics.IndexOf(']', lastOpen + 1);
                int gapLen = caret - (lastOpen + 1);
                bool noCloseBeforeCaret = gapLen <= 0
                    || lyrics.IndexOf(']', lastOpen + 1, gapLen) < 0;
                if (noCloseBeforeCaret && closeAfter >= caret && closeAfter <= lineEnd)
                {
                    appendAt = closeAfter;
                }
            }
        }

        // 3.3 walk-left for a complete [...] block
        if (appendAt is null && caret > lineStart)
        {
            int closeIdx = lyrics.LastIndexOf(']', caret - 1);
            while (closeIdx >= lineStart)
            {
                int openIdx = lyrics.LastIndexOf('[', closeIdx - 1);
                if (openIdx >= lineStart) { appendAt = closeIdx; break; }
                if (closeIdx == lineStart) break;
                closeIdx = lyrics.LastIndexOf(']', closeIdx - 1);
            }
        }

        // 3.4 fallback to plain insert (selection IS replaced per plain-insert semantics)
        if (appendAt is null) { InsertTag(tag); return; }

        // 3.5 append
        var innerName = (tag.Bracket ?? string.Empty).Trim('[', ']');
        var insertText = " | " + innerName;
        section.Lyrics = lyrics.Insert(appendAt.Value, insertText);

        // 3.6 caret landing: position past the bracket's ']'
        FocusedCaretPosition = appendAt.Value + insertText.Length + 1;
        FocusedSelectionLength = 0;
        CaretRestoreRequested?.Invoke(this, FocusedCaretPosition);
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
        MoveSectionUpCommand.NotifyCanExecuteChanged();
        MoveSectionDownCommand.NotifyCanExecuteChanged();
    }

    private void SubscribeToSection(Section s)
    {
        s.PropertyChanged += OnSectionPropertyChanged;
    }

    private void UnsubscribeFromSection(Section s)
    {
        s.PropertyChanged -= OnSectionPropertyChanged;
    }

    private void OnSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Section.Lyrics))
        {
            RecomputePreview();
        }
    }

    private void RecomputePreview()
        => PreviewText = PreviewBuilder.Build(Sections.ToList(), Environment.NewLine);

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
