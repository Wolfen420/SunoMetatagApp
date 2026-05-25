using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnWindowLoaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
            oldVm.CopyRequested -= OnCopyRequested;
        if (e.NewValue is MainViewModel newVm)
            newVm.CopyRequested += OnCopyRequested;
    }

    private void OnCopyRequested(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            Clipboard.SetText(vm.PreviewText ?? string.Empty);
        }
    }

    // Initial focus walker — defer to DispatcherPriority.Loaded so item containers exist,
    // then walk the visual tree from SectionsHost to the first section's lyric TextBox.
    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            new Action(() =>
            {
                var firstLyric = FindFirstLyricTextBox();
                firstLyric?.Focus();
            }));
    }

    private TextBox? FindFirstLyricTextBox()
    {
        if (SectionsHost.Items.Count == 0) return null;
        var container = SectionsHost.ItemContainerGenerator.ContainerFromIndex(0) as DependencyObject;
        if (container is null) return null;
        return FindLyricsTextBox(container);
    }

    private static TextBox? FindLyricsTextBox(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBox tb)
            {
                var expr = tb.GetBindingExpression(TextBox.TextProperty);
                if (expr?.ParentBinding.Path?.Path == "Lyrics")
                    return tb;
            }
            var found = FindLyricsTextBox(child);
            if (found is not null) return found;
        }
        return null;
    }

    private void DeleteSectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not Section section) return;
        if (DataContext is not MainViewModel vm) return;

        bool hasContent = section.Tags.Count > 0 || !string.IsNullOrEmpty(section.Lyrics);
        if (hasContent)
        {
            var result = MessageBox.Show(
                "Delete this section? Its tags and lyrics will be lost.",
                "Confirm delete",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.OK) return;
        }

        vm.RemoveSectionCommand.Execute(section);
    }

    private void CopyErrorButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && !string.IsNullOrEmpty(vm.LoadError))
        {
            Clipboard.SetText(vm.LoadError);
        }
    }
}
