using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SunoMetatagApp.Views;

public partial class PromptBrowserPane : UserControl
{
    public PromptBrowserPane()
    {
        InitializeComponent();
    }

    private void AttributionHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true,
            });
        }
        catch
        {
            // best-effort hyperlink launch; do not crash the app if the shell refuses
        }
        e.Handled = true;
    }
}
