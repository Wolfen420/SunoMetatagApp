using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.18 (B-025): LoadTemplate command coverage — built-in template surface,
// per-template SectionTypes count, clear-and-rebuild flow, SectionType set in
// template order, empty Lyrics after load, null no-op, second-load replacement,
// and default-empty SectionType regression-gate for plain AddSection().
public class MainViewModelLoadTemplateTests
{
    private static readonly TagDefinition[] Sample =
    {
        new("Structure", "Verse", "[Verse]"),
    };

    // T1: BuiltInTemplates exposes the 4 hardcoded templates with expected names.
    [Fact]
    public void T1_BuiltInTemplates_ContainsFourExpectedNames()
    {
        var vm = new MainViewModel(Sample);
        var names = vm.BuiltInTemplates.Select(t => t.Name).ToList();
        Assert.Equal(4, names.Count);
        Assert.Contains("Standard Pop", names);
        Assert.Contains("Simple Ballad", names);
        Assert.Contains("Rock / EDM", names);
        Assert.Contains("Rap / Hip-Hop", names);
    }

    // T2: each built-in template has the expected SectionTypes count.
    [Theory]
    [InlineData("Standard Pop",   10)]
    [InlineData("Simple Ballad",   7)]
    [InlineData("Rock / EDM",      8)]
    [InlineData("Rap / Hip-Hop",   8)]
    public void T2_BuiltInTemplate_HasExpectedSectionCount(string templateName, int expectedCount)
    {
        var template = SongTemplates.BuiltIns.Single(t => t.Name == templateName);
        Assert.Equal(expectedCount, template.SectionTypes.Count);
    }

    // T3: LoadTemplate clears existing sections and rebuilds to template count.
    [Fact]
    public void T3_LoadTemplate_ClearsAndRebuildsSectionStackToTemplateCount()
    {
        var vm = new MainViewModel(Sample);
        Assert.Single(vm.Sections);  // baseline from MainViewModel ctor

        var pop = SongTemplates.BuiltIns.Single(t => t.Name == "Standard Pop");
        vm.LoadTemplateCommand.Execute(pop);

        Assert.Equal(10, vm.Sections.Count);
    }

    // T4: LoadTemplate sets SectionType on each new section in template order.
    [Fact]
    public void T4_LoadTemplate_SetsSectionTypeInTemplateOrder()
    {
        var vm = new MainViewModel(Sample);
        var ballad = SongTemplates.BuiltIns.Single(t => t.Name == "Simple Ballad");
        vm.LoadTemplateCommand.Execute(ballad);

        Assert.Equal(ballad.SectionTypes.Count, vm.Sections.Count);
        for (int i = 0; i < ballad.SectionTypes.Count; i++)
            Assert.Equal(ballad.SectionTypes[i], vm.Sections[i].SectionType);
    }

    // T5: LoadTemplate leaves Lyrics empty on every new section
    // (no auto-fill from the template).
    [Fact]
    public void T5_LoadTemplate_LeavesLyricsEmptyOnAllNewSections()
    {
        var vm = new MainViewModel(Sample);
        var pop = SongTemplates.BuiltIns.Single(t => t.Name == "Standard Pop");
        vm.LoadTemplateCommand.Execute(pop);

        Assert.All(vm.Sections, s => Assert.Equal("", s.Lyrics));
    }

    // T6: LoadTemplate with null template is a no-op (defensive guard).
    [Fact]
    public void T6_LoadTemplate_WithNull_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        var before = vm.Sections.Count;
        vm.LoadTemplateCommand.Execute(null);
        Assert.Equal(before, vm.Sections.Count);
    }

    // T7: Second load fully replaces sections from the first load.
    [Fact]
    public void T7_LoadTemplate_SecondLoad_ReplacesFirstSections()
    {
        var vm = new MainViewModel(Sample);
        var pop = SongTemplates.BuiltIns.Single(t => t.Name == "Standard Pop");
        var hipHop = SongTemplates.BuiltIns.Single(t => t.Name == "Rap / Hip-Hop");

        vm.LoadTemplateCommand.Execute(pop);
        Assert.Equal(10, vm.Sections.Count);
        Assert.Equal("Intro", vm.Sections[0].SectionType);

        vm.LoadTemplateCommand.Execute(hipHop);
        Assert.Equal(hipHop.SectionTypes.Count, vm.Sections.Count);
        Assert.Equal("Intro", vm.Sections[0].SectionType);
        Assert.Equal("Hook", vm.Sections[2].SectionType);  // 3rd entry per Rap/Hip-Hop
    }

    // T8: Section default SectionType is empty string (regression-gate for
    // sections created via AddSection() not via a template).
    [Fact]
    public void T8_AddSection_WithoutTemplate_SectionTypeIsEmpty()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal("", vm.Sections[0].SectionType);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal("", vm.Sections[^1].SectionType);
    }
}
