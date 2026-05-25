using System;
using System.IO;
using System.Windows;
using SunoMetatagApp.Services;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var tagsPath = Path.Combine(AppContext.BaseDirectory, "tags.json");

        MainViewModel vm;
        try
        {
            var tags = TagService.LoadAll(tagsPath);
            vm = new MainViewModel(tags);
        }
        catch (TagLoadException ex)
        {
            vm = new MainViewModel(ex.Message);
        }

        var window = new MainWindow { DataContext = vm };
        window.Show();
    }
}
