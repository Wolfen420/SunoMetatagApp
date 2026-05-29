using System;
using System.IO;
using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.Services;
using SunoMetatagApp.ViewModels;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.20 (B-028): Save/Delete user-defined template coverage on MainViewModel.
// Each test instantiates VM with an injected UserTemplateService pointing to a
// fresh temp directory so persistence tests can verify file-side effects.
public class MainViewModelSaveDeleteTemplateTests
{
    private static readonly TagDefinition[] Sample =
    {
        new("Structure", "Verse", "[Verse]"),
    };

    private static (MainViewModel vm, UserTemplateService svc, string path) CreateVm()
    {
        var path = Path.Combine(Path.GetTempPath(),
                                "SunoMetatagApp-tests-" + Guid.NewGuid().ToString("N"),
                                "templates.json");
        var svc = new UserTemplateService(path);
        var vm = new MainViewModel(Sample, Array.Empty<PromptDefinition>(), svc);
        return (vm, svc, path);
    }

    // W1: SaveCurrentAsTemplate adds the template to UserTemplates.
    [Fact]
    public void W1_SaveCurrentAsTemplate_AddsToUserTemplates()
    {
        var (vm, _, _) = CreateVm();
        vm.Sections[0].SectionType = "Verse";
        vm.AddSectionCommand.Execute(null);
        vm.Sections[^1].SectionType = "Chorus";

        vm.SaveCurrentAsTemplateCommand.Execute("MyTest");

        Assert.Single(vm.UserTemplates);
        Assert.Equal("MyTest", vm.UserTemplates[0].Name);
        Assert.Equal(new[] { "Verse", "Chorus" }, vm.UserTemplates[0].SectionTypes.ToArray());
    }

    // W2: SaveCurrentAsTemplate persists the template to disk via the service.
    [Fact]
    public void W2_SaveCurrentAsTemplate_PersistsToDisk()
    {
        var (vm, svc, path) = CreateVm();
        vm.Sections[0].SectionType = "Verse";
        vm.SaveCurrentAsTemplateCommand.Execute("DiskTest");

        Assert.True(File.Exists(path));
        var reloaded = svc.LoadAll();
        Assert.Single(reloaded);
        Assert.Equal("DiskTest", reloaded[0].Name);
        Assert.Equal(new[] { "Verse" }, reloaded[0].SectionTypes.ToArray());
    }

    // W3: SaveCurrentAsTemplate with empty/whitespace/null name is no-op.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void W3_SaveCurrentAsTemplate_EmptyName_Noop(string? name)
    {
        var (vm, _, path) = CreateVm();
        vm.Sections[0].SectionType = "Verse";
        vm.SaveCurrentAsTemplateCommand.Execute(name);
        Assert.Empty(vm.UserTemplates);
        Assert.False(File.Exists(path));
    }

    // W4: SaveCurrentAsTemplate with no non-empty SectionTypes is no-op.
    [Fact]
    public void W4_SaveCurrentAsTemplate_NoSectionTypes_Noop()
    {
        var (vm, _, path) = CreateVm();
        // baseline Sections[0].SectionType is "" per v1.18 default
        vm.SaveCurrentAsTemplateCommand.Execute("NoSections");
        Assert.Empty(vm.UserTemplates);
        Assert.False(File.Exists(path));
    }

    // W5: SaveCurrentAsTemplate with existing name replaces in place.
    [Fact]
    public void W5_SaveCurrentAsTemplate_DuplicateName_Replaces()
    {
        var (vm, _, _) = CreateVm();
        vm.Sections[0].SectionType = "Verse";
        vm.SaveCurrentAsTemplateCommand.Execute("Dup");
        Assert.Single(vm.UserTemplates);
        Assert.Equal(new[] { "Verse" }, vm.UserTemplates[0].SectionTypes.ToArray());

        // change sections, save again with same name
        vm.AddSectionCommand.Execute(null);
        vm.Sections[^1].SectionType = "Chorus";
        vm.SaveCurrentAsTemplateCommand.Execute("Dup");
        Assert.Single(vm.UserTemplates);
        Assert.Equal(new[] { "Verse", "Chorus" }, vm.UserTemplates[0].SectionTypes.ToArray());
    }

    // W6: SaveCurrentAsTemplate trims surrounding whitespace from the name.
    [Fact]
    public void W6_SaveCurrentAsTemplate_TrimsName()
    {
        var (vm, _, _) = CreateVm();
        vm.Sections[0].SectionType = "Verse";
        vm.SaveCurrentAsTemplateCommand.Execute("  Trimmed  ");
        Assert.Single(vm.UserTemplates);
        Assert.Equal("Trimmed", vm.UserTemplates[0].Name);
    }

    // W7: DeleteUserTemplate removes the entry and persists empty list.
    [Fact]
    public void W7_DeleteUserTemplate_RemovesAndPersists()
    {
        var (vm, svc, path) = CreateVm();
        vm.Sections[0].SectionType = "Verse";
        vm.SaveCurrentAsTemplateCommand.Execute("ToDelete");

        var item = vm.Templates.First(t => t.IsUserDefined && t.Name == "ToDelete");
        vm.DeleteUserTemplateCommand.Execute(item);

        Assert.Empty(vm.UserTemplates);
        Assert.True(File.Exists(path));
        Assert.Empty(svc.LoadAll());
    }

    // W8: DeleteUserTemplate on a built-in is a no-op (IsUserDefined=false guard).
    [Fact]
    public void W8_DeleteUserTemplate_BuiltIn_Noop()
    {
        var (vm, _, _) = CreateVm();
        var builtIn = vm.Templates.First(t => !t.IsUserDefined);
        var builtInTotalBefore = vm.BuiltInTemplates.Count;

        vm.DeleteUserTemplateCommand.Execute(builtIn);

        Assert.Equal(builtInTotalBefore, vm.BuiltInTemplates.Count);
        Assert.Equal(builtInTotalBefore, vm.Templates.Count(t => !t.IsUserDefined));
    }

    // W9: Templates collection lists built-ins first, then user templates,
    //     with correct Group property per item.
    [Fact]
    public void W9_TemplatesCollection_BuiltInsFirstThenUserDefined()
    {
        var (vm, _, _) = CreateVm();
        vm.Sections[0].SectionType = "Verse";
        vm.SaveCurrentAsTemplateCommand.Execute("U1");
        vm.AddSectionCommand.Execute(null);
        vm.Sections[^1].SectionType = "Chorus";
        vm.SaveCurrentAsTemplateCommand.Execute("U2");

        var builtInCount = vm.BuiltInTemplates.Count;
        Assert.Equal(builtInCount + 2, vm.Templates.Count);
        for (int i = 0; i < builtInCount; i++)
        {
            Assert.False(vm.Templates[i].IsUserDefined);
            Assert.Equal("Built-in Templates", vm.Templates[i].Group);
        }
        for (int i = builtInCount; i < vm.Templates.Count; i++)
        {
            Assert.True(vm.Templates[i].IsUserDefined);
            Assert.Equal("My Templates", vm.Templates[i].Group);
        }
    }

    // W10: LoadAll-on-construction populates UserTemplates from disk.
    [Fact]
    public void W10_Constructor_LoadsUserTemplatesFromService()
    {
        var path = Path.Combine(Path.GetTempPath(),
                                "SunoMetatagApp-tests-" + Guid.NewGuid().ToString("N"),
                                "templates.json");
        var svc = new UserTemplateService(path);
        svc.SaveAll(new[]
        {
            new SongTemplate("Preexisting", new[] { "Intro", "Verse 1", "Outro" }),
        });

        var vm = new MainViewModel(Sample, Array.Empty<PromptDefinition>(), svc);
        Assert.Single(vm.UserTemplates);
        Assert.Equal("Preexisting", vm.UserTemplates[0].Name);
        Assert.Contains(vm.Templates, t => t.IsUserDefined && t.Name == "Preexisting");
    }
}
