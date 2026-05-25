using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;
using Xunit;

namespace SunoMetatagApp.Tests;

public class MainViewModelTests
{
    private static readonly TagDefinition[] Sample =
    {
        new("Structure", "Verse",   "[Verse]"),
        new("Structure", "Chorus",  "[Chorus]"),
        new("Vocal",     "Whisper", "[Whispered]"),
    };

    [Fact]
    public void Ctor_PopulatesCategoriesWithAllPrefix()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal(new[] { "All", "Structure", "Vocal" }, vm.Categories);
        Assert.Equal("All", vm.SelectedCategory);
    }

    [Fact]
    public void Ctor_StartsWithOneArmedEmptySection()
    {
        var vm = new MainViewModel(Sample);
        Assert.Single(vm.Sections);
        Assert.True(vm.Sections[0].IsArmed);
        Assert.Equal("", vm.Sections[0].Lyrics);
        Assert.Empty(vm.Sections[0].Tags);
    }

    [Fact]
    public void Ctor_PreviewText_StartsEmpty()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal("", vm.PreviewText);
    }

    [Fact]
    public void AddSection_AppendsArmedSection()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.Sections.Count);
        Assert.True(vm.Sections[1].IsArmed);
    }

    [Fact]
    public void RemoveSection_OnLastSection_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        Assert.Single(vm.Sections);
        vm.RemoveSectionCommand.Execute(vm.Sections[0]);
        Assert.Single(vm.Sections);
    }

    [Fact]
    public void RemoveSection_OnMiddleSection_Removes()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(3, vm.Sections.Count);
        var middle = vm.Sections[1];
        vm.RemoveSectionCommand.Execute(middle);
        Assert.Equal(2, vm.Sections.Count);
        Assert.DoesNotContain(middle, vm.Sections);
    }

    [Fact]
    public void InsertTag_WithOneArmedSection_AppendsToThatSection()
    {
        var vm = new MainViewModel(Sample);
        var tagVm = new TagViewModel(Sample[0]);
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Single(vm.Sections[0].Tags);
        Assert.Equal("[Verse]", vm.Sections[0].Tags[0].Bracket);
    }

    [Fact]
    public void InsertTag_WithMultipleArmedSections_AppendsToAll()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var tagVm = new TagViewModel(Sample[1]);
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Equal("[Chorus]", vm.Sections[0].Tags[0].Bracket);
        Assert.Equal("[Chorus]", vm.Sections[1].Tags[0].Bracket);
    }

    [Fact]
    public void InsertTag_WithNoArmedSections_DoesNotMutate_AndSetsShowArmHint()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].IsArmed = false;
        var tagVm = new TagViewModel(Sample[0]);
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Empty(vm.Sections[0].Tags);
        Assert.True(vm.ShowArmHint);
    }

    [Fact]
    public void InsertTag_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        var tagVm = new TagViewModel(Sample[0]);
        vm.InsertTagCommand.Execute(tagVm);
        Assert.Equal("[Verse]", vm.PreviewText);
    }

    [Fact]
    public void LyricsChange_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].Lyrics = "hello";
        Assert.Equal("hello", vm.PreviewText);
    }

    [Fact]
    public void RemoveTagOnSection_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        var t = Sample[0];
        vm.Sections[0].Tags.Add(t);
        Assert.Equal("[Verse]", vm.PreviewText);
        vm.Sections[0].RemoveTagCommand.Execute(t);
        Assert.Equal("", vm.PreviewText);
    }

    [Fact]
    public void ChangingSearchText_RecomputesFilteredTags()
    {
        var vm = new MainViewModel(Sample);
        vm.SearchText = "whisp";
        Assert.Single(vm.FilteredTags);
        Assert.Equal("Whisper", vm.FilteredTags[0].Label);
    }

    [Fact]
    public void ChangingSelectedCategory_RecomputesFilteredTags()
    {
        var vm = new MainViewModel(Sample);
        vm.SelectedCategory = "Vocal";
        Assert.Single(vm.FilteredTags);
        Assert.Equal("Whisper", vm.FilteredTags[0].Label);
    }

    [Fact]
    public void LoadErrorCtor_HasEmptyCategoriesAndSections()
    {
        var vm = new MainViewModel("tags.json not found.");
        Assert.Equal("tags.json not found.", vm.LoadError);
        Assert.Equal(new[] { "All" }, vm.Categories);
        Assert.Empty(vm.Sections);
        Assert.Empty(vm.FilteredTags);
    }

    // ---- r2 additions ----

    [Fact]
    public void Ctor_ArmedSectionCount_IsOne()
    {
        var vm = new MainViewModel(Sample);
        Assert.Equal(1, vm.ArmedSectionCount);
    }

    [Fact]
    public void AddSection_IncrementsArmedSectionCount()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.ArmedSectionCount);
    }

    [Fact]
    public void RemoveSection_DecrementsArmedSectionCount_WhenRemovedWasArmed()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.ArmedSectionCount);
        vm.RemoveSectionCommand.Execute(vm.Sections[1]);
        Assert.Equal(1, vm.ArmedSectionCount);
    }

    [Fact]
    public void IsArmedChange_UpdatesArmedSectionCount()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.ArmedSectionCount);
        vm.Sections[0].IsArmed = false;
        Assert.Equal(1, vm.ArmedSectionCount);
    }

    [Fact]
    public void IsArmedTransitionToTrue_ClearsShowArmHint()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].IsArmed = false;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0]));
        Assert.True(vm.ShowArmHint);
        vm.Sections[0].IsArmed = true;
        Assert.False(vm.ShowArmHint);
    }

    [Fact]
    public void InsertTag_WithArmedSection_ClearsShowArmHint()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].IsArmed = false;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0]));
        Assert.True(vm.ShowArmHint);
        vm.Sections[0].IsArmed = true;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0]));
        Assert.False(vm.ShowArmHint);
    }

    [Fact]
    public void MoveSectionUp_AtTop_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionUpCommand.Execute(s0);
        Assert.Equal(s0, vm.Sections[0]);
        Assert.Equal(s1, vm.Sections[1]);
    }

    [Fact]
    public void MoveSectionUp_SwapsWithPrevious()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionUpCommand.Execute(s1);
        Assert.Equal(s1, vm.Sections[0]);
        Assert.Equal(s0, vm.Sections[1]);
    }

    [Fact]
    public void MoveSectionDown_AtBottom_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionDownCommand.Execute(s1);
        Assert.Equal(s0, vm.Sections[0]);
        Assert.Equal(s1, vm.Sections[1]);
    }

    [Fact]
    public void MoveSectionDown_SwapsWithNext()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];
        vm.MoveSectionDownCommand.Execute(s0);
        Assert.Equal(s1, vm.Sections[0]);
        Assert.Equal(s0, vm.Sections[1]);
    }

    [Fact]
    public void MoveSection_RecomputesPreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        vm.Sections[0].Lyrics = "first";
        vm.Sections[1].Lyrics = "second";
        Assert.StartsWith("first", vm.PreviewText);
        vm.MoveSectionDownCommand.Execute(vm.Sections[0]);
        Assert.StartsWith("second", vm.PreviewText);
    }

    [Fact]
    public void AfterRemoveSection_MutatingRemovedSectionLyrics_DoesNotChangePreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var doomed = vm.Sections[1];
        vm.RemoveSectionCommand.Execute(doomed);
        var before = vm.PreviewText;
        doomed.Lyrics = "should be ignored";
        Assert.Equal(before, vm.PreviewText);
    }

    [Fact]
    public void AfterRemoveSection_AddingTagToRemovedSection_DoesNotChangePreviewText()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var doomed = vm.Sections[1];
        vm.RemoveSectionCommand.Execute(doomed);
        var before = vm.PreviewText;
        doomed.Tags.Add(Sample[0]);
        Assert.Equal(before, vm.PreviewText);
    }
}
