using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;

namespace SunoMetatagApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<TagDefinition> _allTags;
    private readonly IReadOnlyList<PromptDefinition> _allPrompts;
    private readonly UserTemplateService _userTemplateService;
    private int _copyStatusToken;

    public ObservableCollection<Section> Sections { get; } = new();
    public IReadOnlyList<string> Categories { get; }
    public IReadOnlyList<string> PromptGenres { get; }
    public ObservableCollection<PromptDefinition> Prompts { get; } = new();
    // v1.18 (B-025): hardcoded built-in song-structure templates. Stable
    // read-only surface; v1.20 (B-028) supplements this with the mutable
    // UserTemplates collection persisted to %APPDATA%\SunoMetatagApp\templates.json.
    public IReadOnlyList<SongTemplate> BuiltInTemplates { get; } = SongTemplates.BuiltIns;
    // v1.20 (B-028): user-defined song-structure templates loaded from and
    // persisted to UserTemplateService. ObservableCollection so the combined
    // Templates view rebuilds on add/remove.
    public ObservableCollection<SongTemplate> UserTemplates { get; } = new();
    // v1.20 (B-028): combined surface bound to the XAML TemplateComboBox.
    // Built-ins first (IsUserDefined=false), then user templates (IsUserDefined=true);
    // CollectionViewSource in XAML groups them by TemplateListItem.Group.
    public ObservableCollection<TemplateListItem> Templates { get; } = new();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedCategory = "Structure";
    [ObservableProperty] private IReadOnlyList<TagViewModel> _filteredTags = Array.Empty<TagViewModel>();
    [ObservableProperty] private string _previewText = string.Empty;
    [ObservableProperty] private string? _loadError;

    [ObservableProperty] private Section? _focusedSection;
    [ObservableProperty] private int _focusedCaretPosition;
    [ObservableProperty] private int _focusedSelectionLength;

    [ObservableProperty] private bool _isPromptBrowserVisible;
    [ObservableProperty] private string _selectedPromptGenre = "All";
    [ObservableProperty] private PromptDefinition? _selectedPrompt;
    [ObservableProperty] private string? _promptCopyStatus;

    public event EventHandler? CopyRequested;
    public event EventHandler<int>? CaretRestoreRequested;

    public MainViewModel(IReadOnlyList<TagDefinition> tags)
        : this(tags, GetLoadedPrompts(), null)
    {
    }

    public MainViewModel(IReadOnlyList<TagDefinition> tags, IReadOnlyList<PromptDefinition> prompts)
        : this(tags, prompts, null)
    {
    }

    // v1.20 (B-028): primary constructor accepts an optional UserTemplateService
    // so tests can inject a temp-directory-backed instance. Production callers
    // pass null and get UserTemplateService.CreateDefault() resolving to
    // %APPDATA%\SunoMetatagApp\templates.json.
    public MainViewModel(IReadOnlyList<TagDefinition> tags,
                         IReadOnlyList<PromptDefinition> prompts,
                         UserTemplateService? userTemplateService)
    {
        _allTags = tags;
        _allPrompts = prompts;
        _userTemplateService = userTemplateService ?? UserTemplateService.CreateDefault();
        Categories = BuildCategories(tags);
        // v1.13 (B-SUNO-014): default to Structure category on app load so users see the most
        // common section tags first. Error-state constructor below intentionally keeps "All"
        // because its Categories list contains only "All".
        SelectedCategory = "Structure";
        FilteredTags = ComputeFiltered();
        PromptGenres = BuildPromptGenres(prompts);
        RefreshPrompts();
        Sections.CollectionChanged += OnSectionsChanged;
        foreach (var t in _userTemplateService.LoadAll()) UserTemplates.Add(t);
        UserTemplates.CollectionChanged += (_, _) => RebuildTemplatesCollection();
        RebuildTemplatesCollection();
        AddSection();
    }

    public MainViewModel(string loadError)
    {
        _allTags = Array.Empty<TagDefinition>();
        _allPrompts = GetLoadedPrompts();
        _userTemplateService = UserTemplateService.CreateDefault();
        Categories = new[] { "All" };
        SelectedCategory = "All";
        FilteredTags = Array.Empty<TagViewModel>();
        LoadError = loadError;
        PromptGenres = BuildPromptGenres(_allPrompts);
        RefreshPrompts();
        Sections.CollectionChanged += OnSectionsChanged;
        // v1.20 (B-028): error-state constructor does not LoadAll() — keeps the
        // error path fast and avoids file-I/O when the app already failed to
        // load tags. UserTemplates remains empty; Templates shows only built-ins.
        UserTemplates.CollectionChanged += (_, _) => RebuildTemplatesCollection();
        RebuildTemplatesCollection();
        AddSection();
    }

    private void RebuildTemplatesCollection()
    {
        Templates.Clear();
        foreach (var t in BuiltInTemplates) Templates.Add(new TemplateListItem(t, isUserDefined: false));
        foreach (var t in UserTemplates) Templates.Add(new TemplateListItem(t, isUserDefined: true));
    }

    private static IReadOnlyList<PromptDefinition> GetLoadedPrompts()
    {
        if (Application.Current is App app)
            return App.LoadedPrompts;
        return Array.Empty<PromptDefinition>();
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

    // v1.18 (B-025): load a built-in song structure template into the section
    // stack. The confirmation flow for the "non-empty lyrics will be lost" case
    // is handled by the View-side TemplateComboBox_SelectionChanged code-behind
    // handler (matches the existing DeleteSectionButton_Click + RemoveSectionCommand
    // pattern); this VM command performs only the rebuild logic. Sections.Clear()
    // bypasses RemoveSection's Count<=1 guard, which is anti-template-load.
    //
    // v1.20 (B-028, Lead absorption #3): signature kept as SongTemplate? to
    // preserve v1.18 test fixtures unchanged. The XAML now binds to
    // Templates : ObservableCollection<TemplateListItem>; the code-behind
    // SelectionChanged handler extracts TemplateListItem.Template before
    // invoking this command.
    [RelayCommand]
    private void LoadTemplate(SongTemplate? template)
    {
        if (template is null || template.SectionTypes.Count == 0) return;
        Sections.Clear();
        foreach (var sectionType in template.SectionTypes)
        {
            AddSection();
            Sections[^1].SectionType = sectionType;
        }
    }

    // v1.20 (B-028): capture current Sections[*].SectionType non-empty values as
    // a user-defined template, persist to UserTemplateService. Duplicate-name
    // detection lives in the code-behind handler (MessageBox.Show confirmation
    // pattern matching v1.18 DeleteSection). Empty-name and no-section-types
    // cases are guarded here as defensive early returns.
    [RelayCommand]
    private void SaveCurrentAsTemplate(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var trimmed = name.Trim();
        var sectionTypes = Sections
            .Select(s => s.SectionType ?? string.Empty)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
        if (sectionTypes.Count == 0) return;

        var existing = UserTemplates.FirstOrDefault(t =>
            string.Equals(t.Name, trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) UserTemplates.Remove(existing);
        UserTemplates.Add(new SongTemplate(trimmed, sectionTypes));
        _userTemplateService.SaveAll(UserTemplates);
    }

    // v1.20 (B-028): delete a user-defined template. Built-in templates are NOT
    // deletable — the IsUserDefined guard prevents accidental built-in removal
    // even if a malformed call site supplied a built-in TemplateListItem.
    // Persist after removal.
    [RelayCommand]
    private void DeleteUserTemplate(TemplateListItem? item)
    {
        if (item is null || !item.IsUserDefined) return;
        var match = UserTemplates.FirstOrDefault(t => ReferenceEquals(t, item.Template));
        if (match is null) return;
        UserTemplates.Remove(match);
        _userTemplateService.SaveAll(UserTemplates);
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

        // 3.7 (v1.19, B-027) auto-reorder bracket contents by canonical SortOrder.
        // Activates the canonical left-to-right stacking sequence (Structure=1,
        // Vocal=2, Instrument=3, Mood=4, Effect=5, SFX=6, Production=7) that
        // v1.17 wired through TagDefinition.SortOrder but deferred per BACKLOG.
        // Genre tokens (SortOrder=99 default) and unknown tokens (no matching
        // TagDefinition by Label or Bracket-stripped) sort to the end. Stable
        // OrderBy preserves user-typed order within same-SortOrder groups.
        //
        // Merge-target invariant: §3.5 inserted `insertText` at `appendAt.Value`
        // which was the position of the existing ']' on the merge target. After
        // the insert, the same ']' has shifted to index `appendAt.Value +
        // insertText.Length` (= newCloseIdx). The matching '[' of THIS bracket
        // is the closest '[' strictly to the left of newCloseIdx because §3.2
        // and §3.3 guaranteed we found a complete `[...]` block containing
        // appendAt — no intervening '[' or ']' on the same line between them.
        // LastIndexOf('[', newCloseIdx - 1) therefore locates the correct '['.
        {
            var current = section.Lyrics;
            int newCloseIdx = appendAt.Value + insertText.Length;
            int newOpenIdx = current.LastIndexOf('[', newCloseIdx - 1);
            if (newOpenIdx >= 0)
            {
                var content = current.Substring(newOpenIdx + 1, newCloseIdx - newOpenIdx - 1);
                var tokens = content.Split('|')
                                    .Select(t => t.Trim())
                                    .Where(t => t.Length > 0)
                                    .ToList();
                int SortOrderOf(string token) =>
                    _allTags.FirstOrDefault(t => string.Equals(t.Label, token, StringComparison.OrdinalIgnoreCase))?.SortOrder
                    ?? _allTags.FirstOrDefault(t => string.Equals(t.Bracket.Trim('[', ']'), token, StringComparison.OrdinalIgnoreCase))?.SortOrder
                    ?? 99;
                var sorted = tokens.OrderBy(SortOrderOf).ToList();
                if (!sorted.SequenceEqual(tokens, StringComparer.Ordinal))
                {
                    var rejoined = string.Join(" | ", sorted);
                    section.Lyrics = current[..(newOpenIdx + 1)] + rejoined + current[newCloseIdx..];
                    FocusedCaretPosition = newOpenIdx + 1 + rejoined.Length + 1;
                    FocusedSelectionLength = 0;
                    CaretRestoreRequested?.Invoke(this, FocusedCaretPosition);
                    return;
                }
            }
        }

        // 3.6 caret landing: position past the bracket's ']'
        FocusedCaretPosition = appendAt.Value + insertText.Length + 1;
        FocusedSelectionLength = 0;
        CaretRestoreRequested?.Invoke(this, FocusedCaretPosition);
    }

    [RelayCommand]
    private void CopyPreview() => CopyRequested?.Invoke(this, EventArgs.Empty);

    // v1.16 (B-SUNO-013): clear the SearchText to dismiss the active search query
    // without disturbing SelectedCategory or FocusedSection. The existing
    // OnSearchTextChanged partial method (below) recomputes FilteredTags when
    // SearchText changes — so this command needs no extra plumbing beyond
    // assigning the property.
    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    [RelayCommand]
    private void TogglePromptBrowser() => IsPromptBrowserVisible = !IsPromptBrowserVisible;

    [RelayCommand]
    private async Task CopyPromptBody(PromptDefinition? prompt)
    {
        if (prompt is null) return;

        try
        {
            Clipboard.SetText(prompt.Body);
            await SetCopyStatusAndClearAsync("Copied!");
        }
        catch
        {
            await SetCopyStatusAndClearAsync("Copy failed");
        }
    }

    private async Task SetCopyStatusAndClearAsync(string status)
    {
        var token = ++_copyStatusToken;
        PromptCopyStatus = status;
        try
        {
            await Task.Delay(1500);
        }
        catch
        {
            return;
        }
        // only clear if no newer copy occurred in the meantime
        if (_copyStatusToken == token)
            PromptCopyStatus = null;
    }

    partial void OnSearchTextChanged(string value) => FilteredTags = ComputeFiltered();
    partial void OnSelectedCategoryChanged(string value) => FilteredTags = ComputeFiltered();
    partial void OnSelectedPromptGenreChanged(string value) => RefreshPrompts();

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

    private static IReadOnlyList<string> BuildPromptGenres(IEnumerable<PromptDefinition> prompts)
    {
        var distinct = PromptService.DistinctGenres(prompts);
        var list = new List<string>(distinct.Count + 1) { "All" };
        list.AddRange(distinct);
        return list;
    }

    private void RefreshPrompts()
    {
        Prompts.Clear();
        foreach (var p in PromptService.Filter(_allPrompts, SelectedPromptGenre))
            Prompts.Add(p);

        // if currently selected prompt is filtered out, clear selection
        if (SelectedPrompt is { } sel && !Prompts.Contains(sel))
            SelectedPrompt = null;
    }
}
