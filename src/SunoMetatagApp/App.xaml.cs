using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using SunoMetatagApp.ViewModels;

namespace SunoMetatagApp;

public partial class App : Application
{
    public static IReadOnlyList<PromptDefinition> LoadedPrompts { get; private set; }
        = Array.Empty<PromptDefinition>();

    public static string? PromptLoadError { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var tagsPath = Path.Combine(AppContext.BaseDirectory, "tags.json");
        var promptsPath = Path.Combine(AppContext.BaseDirectory, "prompts.json");

        try
        {
            LoadedPrompts = PromptService.LoadAll(promptsPath);
        }
        catch (PromptLoadException ex)
        {
            PromptLoadError = ex.Message;
        }

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
