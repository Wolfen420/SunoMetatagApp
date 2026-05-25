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
    public void AddSection_AppendsSectionToCollection()
    {
        var vm = new MainViewModel(Sample);
        Assert.Single(vm.Sections);
        vm.AddSectionCommand.Execute(null);
        Assert.Equal(2, vm.Sections.Count);
    }

    [Fact]
    public void RemoveSection_RemovesFromCollection()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var middle = vm.Sections[0];
        vm.RemoveSectionCommand.Execute(middle);
        Assert.Single(vm.Sections);
        Assert.DoesNotContain(middle, vm.Sections);
    }

    [Fact]
    public void MoveSectionUp_CanExecute_AtIndexZero_ReturnsFalse()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var first = vm.Sections[0];
        Assert.False(vm.MoveSectionUpCommand.CanExecute(first));
    }

    [Fact]
    public void MoveSectionDown_CanExecute_AtLastIndex_ReturnsFalse()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var last = vm.Sections[vm.Sections.Count - 1];
        Assert.False(vm.MoveSectionDownCommand.CanExecute(last));
    }

    [Fact]
    public void MoveSectionUp_FromMiddle_SwapsWithPrior()
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
    public void InsertTag_WithNoFocusedSection_DoesNothing()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].Lyrics = "hello";
        // FocusedSection stays null
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0]));
        Assert.Equal("hello", vm.Sections[0].Lyrics);
    }

    [Fact]
    public void InsertTag_WithFocusedSection_InsertsBracketAtCaret()
    {
        var vm = new MainViewModel(Sample);
        var s = vm.Sections[0];
        s.Lyrics = "Walking down the street";
        vm.FocusedSection = s;
        vm.FocusedCaretPosition = 16; // between "the" and " street"
        vm.FocusedSelectionLength = 0;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0])); // [Verse]
        Assert.Equal("Walking down the[Verse] street", s.Lyrics);
        Assert.Equal(16 + "[Verse]".Length, vm.FocusedCaretPosition);
    }

    [Fact]
    public void InsertTag_WithSelectionRange_ReplacesSelectionWithBracket()
    {
        var vm = new MainViewModel(Sample);
        var s = vm.Sections[0];
        s.Lyrics = "Walking down the street";
        vm.FocusedSection = s;
        vm.FocusedCaretPosition = 13;            // start of "the"
        vm.FocusedSelectionLength = 3;           // selects "the"
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[1])); // [Chorus]
        Assert.Equal("Walking down [Chorus] street", s.Lyrics);
        Assert.Equal(13 + "[Chorus]".Length, vm.FocusedCaretPosition);
        Assert.Equal(0, vm.FocusedSelectionLength);
    }

    [Fact]
    public void InsertTag_TwoConsecutiveCallsWithDifferentFocusedSections_LandsInCorrectSections()
    {
        var vm = new MainViewModel(Sample);
        vm.AddSectionCommand.Execute(null);
        var s0 = vm.Sections[0];
        var s1 = vm.Sections[1];

        vm.FocusedSection = s0;
        vm.FocusedCaretPosition = 0;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0])); // [Verse] into s0
        Assert.Equal("[Verse]", s0.Lyrics);
        Assert.Equal("", s1.Lyrics);

        vm.FocusedSection = s1;
        vm.FocusedCaretPosition = 0;
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[1])); // [Chorus] into s1
        Assert.Equal("[Verse]", s0.Lyrics);   // unchanged
        Assert.Equal("[Chorus]", s1.Lyrics);
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
}
