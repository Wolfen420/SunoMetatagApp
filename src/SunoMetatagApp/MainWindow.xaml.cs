using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class MainWindow : Window
{
    private TextBox? _currentFocusedTextBox;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnWindowLoaded;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
        {
            oldVm.CopyRequested -= OnCopyRequested;
            oldVm.CaretRestoreRequested -= OnCaretRestoreRequested;
        }
        if (e.NewValue is MainViewModel newVm)
        {
            newVm.CopyRequested += OnCopyRequested;
            newVm.CaretRestoreRequested += OnCaretRestoreRequested;
        }
    }

    private void OnCopyRequested(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            Clipboard.SetText(vm.PreviewText ?? string.Empty);
        }
    }

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

    // Focus tracking: lyric TextBox gained keyboard focus.
    private void LyricTextBox_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not TextBox tb || tb.DataContext is not Section section) return;
        if (DataContext is not MainViewModel vm) return;

        vm.FocusedSection = section;
        vm.FocusedCaretPosition = tb.SelectionStart;
        vm.FocusedSelectionLength = tb.SelectionLength;
        _currentFocusedTextBox = tb;
    }

    // r2 (resolves HIGH-1): defer-clear FocusedSection when focus leaves to anywhere
    // outside the lyric-textbox set. Deferred via Dispatcher.BeginInvoke so a
    // Focusable=False button click (which can briefly fire LostFocus for non-keyboard
    // focus types) does not trip the clear. The lambda re-checks before clearing:
    // if GotKeyboardFocus already moved FocusedSection to another lyric textbox,
    // or focus is now on any other lyric textbox, leave the new state alone.
    //
    // v1.10 (B-SUNO-012): third race-cancel check — if keyboard focus moved into
    // the tag-picker pane (SearchBox, Category ComboBox, scrollable pill area,
    // ComboBox dropdown popup), treat it as "still working with the focused
    // section" and skip the clear. Restores tag-pill clickability without forcing
    // the user back into the lyric textbox.
    private void LyricTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not TextBox tb || tb.DataContext is not Section section) return;
        if (DataContext is not MainViewModel vm) return;

        var sectionAtLossTime = section;
        var tbAtLossTime = tb;
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            if (vm.FocusedSection != sectionAtLossTime) return;

            if (Keyboard.FocusedElement is DependencyObject focused
                && IsAncestorOf(TagPickerPane, focused))
            {
                return;
            }

            if (Keyboard.FocusedElement is TextBox focusedTb &&
                focusedTb.DataContext is Section)
            {
                return;
            }

            vm.FocusedSection = null;
            vm.FocusedCaretPosition = 0;
            vm.FocusedSelectionLength = 0;
            if (ReferenceEquals(_currentFocusedTextBox, tbAtLossTime))
                _currentFocusedTextBox = null;
        }));
    }

    // v1.10 (B-SUNO-012): walk parent chain from `descendant` toward the visual root,
    // returning true if `ancestor` is encountered. Walks the visual tree first
    // (standard WPF parent chain); falls back to the logical tree at each step
    // where VisualTreeHelper.GetParent returns null, to bridge Popup/Adorner
    // boundaries (e.g., a ComboBoxItem inside an open ComboBox dropdown whose
    // visual chain is rooted in a separate PopupRoot HwndSource).
    private static bool IsAncestorOf(DependencyObject ancestor, DependencyObject? descendant)
    {
        var cur = descendant;
        while (cur != null)
        {
            if (ReferenceEquals(cur, ancestor)) return true;
            var next = VisualTreeHelper.GetParent(cur) ?? LogicalTreeHelper.GetParent(cur);
            if (ReferenceEquals(next, cur)) break;
            cur = next;
        }
        return false;
    }

    // Keep caret + selection tracked while focus stays on this textbox.
    private void LyricTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb || tb.DataContext is not Section section) return;
        if (DataContext is not MainViewModel vm) return;
        if (vm.FocusedSection != section) return;

        vm.FocusedCaretPosition = tb.SelectionStart;
        vm.FocusedSelectionLength = tb.SelectionLength;
    }

    // After InsertTag mutates Section.Lyrics, the bound TextBox.Text update propagates
    // via the dispatcher; defer the caret-restore so it lands after Text has updated.
    private void OnCaretRestoreRequested(object? sender, int newCaretPosition)
    {
        var tb = _currentFocusedTextBox;
        if (tb is null) return;
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            tb.Focus();
            var clamped = Math.Clamp(newCaretPosition, 0, tb.Text?.Length ?? 0);
            tb.SelectionStart = clamped;
            tb.SelectionLength = 0;
        }));
    }

    // v1.3 (B-SUNO-004): modifier-aware picker click router.
    // Reads Keyboard.Modifiers at Click event time (after mouse-up) and routes
    // to InsertTagStackedCommand (Shift held) or InsertTagCommand (plain).
    // Picker button is Focusable=False, so this does not move textbox focus or
    // disturb v1.1's defer-clear contract (see wiki/risks/focus-flip-stale-insert).
    private void TagPickerButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.DataContext is not ViewModels.TagViewModel tag) return;
        if (DataContext is not MainViewModel vm) return;

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            vm.InsertTagStackedCommand.Execute(tag);
        else
            vm.InsertTagCommand.Execute(tag);
    }

    private void DeleteSectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.CommandParameter is not Section section) return;
        if (DataContext is not MainViewModel vm) return;

        bool hasContent = !string.IsNullOrEmpty(section.Lyrics);
        if (hasContent)
        {
            var result = MessageBox.Show(
                "Delete this section? Its lyrics will be lost.",
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
