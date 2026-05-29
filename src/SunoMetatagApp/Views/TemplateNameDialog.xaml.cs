using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SunoMetatagApp.Views;

// v1.20 (B-028): modal name-input dialog for the Save-as-Template flow. Returns
// trimmed non-empty name via Result, or null on cancel/empty input. Mirrors
// Suno dark-theme tokens. Enter key submits (IsDefault on OK); Esc cancels
// (IsCancel on Cancel via KeyDown handler).
public partial class TemplateNameDialog : Window
{
    public string? Result { get; private set; }

    public TemplateNameDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => NameTextBox.Focus();
    }

    public static string? Prompt(Window owner, string title = "Save Template", string defaultName = "")
    {
        var dlg = new TemplateNameDialog { Owner = owner, Title = title };
        dlg.NameTextBox.Text = defaultName;
        if (!string.IsNullOrEmpty(defaultName))
            dlg.NameTextBox.SelectAll();
        return dlg.ShowDialog() == true ? dlg.Result : null;
    }

    private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        OkButton.IsEnabled = !string.IsNullOrWhiteSpace(NameTextBox.Text);
    }

    private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            DialogResult = false;
            return;
        }
        Result = name;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
