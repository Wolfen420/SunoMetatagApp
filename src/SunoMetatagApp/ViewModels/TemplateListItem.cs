using SunoMetatagApp.Models;

namespace SunoMetatagApp.ViewModels;

// v1.20 (B-028): wrapper exposed to the XAML TemplateComboBox for grouped
// rendering of built-in vs user-defined templates. IsUserDefined drives the
// per-item × delete-affordance visibility (DataTrigger in MainWindow.xaml).
// Group drives the CollectionViewSource grouping headers.
public sealed class TemplateListItem
{
    public SongTemplate Template { get; }
    public bool IsUserDefined { get; }

    public string Name => Template.Name;
    public string Group => IsUserDefined ? "My Templates" : "Built-in Templates";

    public TemplateListItem(SongTemplate template, bool isUserDefined)
    {
        Template = template;
        IsUserDefined = isUserDefined;
    }
}
