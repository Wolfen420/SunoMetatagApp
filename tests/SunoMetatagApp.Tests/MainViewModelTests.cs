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

    // V1 (v1.10 / B-SUNO-012): VM-level invariant relied on by the v1.10 picker-pane
    // defer-clear guard in MainWindow.xaml.cs. InsertTag routes to MainViewModel.
    // FocusedSection independently of any View-side keyboard-focus state — so when
    // the v1.10 guard preserves FocusedSection across a focus transition into the
    // tag-picker pane (SearchBox / Category ComboBox / pill grid), subsequent
    // tag-pill clicks insert into the last-focused lyric textbox as the user expects.
    // This test simulates the post-v1.10 path: FocusedSection set, then keyboard
    // focus conceptually elsewhere (no View state to simulate; the VM contract is
    // surface-agnostic), then InsertTag invoked.
    [Fact]
    public void V1_InsertTag_RoutesToFocusedSection_IndependentOfViewFocusState()
    {
        var vm = new MainViewModel(Sample);
        var s = vm.Sections[0];
        s.Lyrics = "before ";
        vm.FocusedSection = s;
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.FocusedSelectionLength = 0;

        // Simulates the v1.10 path: user typed in section, then clicked SearchBox
        // (in v1.9 this cleared FocusedSection; in v1.10 the picker-pane guard
        // preserves it). VM doesn't know about View focus — InsertTag operates
        // strictly on FocusedSection.
        vm.InsertTagCommand.Execute(new TagViewModel(Sample[0])); // [Verse]
        Assert.Equal("before [Verse]", s.Lyrics);
        Assert.Equal("before ".Length + "[Verse]".Length, vm.FocusedCaretPosition);
    }

    [Fact]
    public void D1_NormalConstructor_DefaultsToStructureCategory_AndFilteredTagsReflectStructureOnly()
    {
        // v1.13 (B-SUNO-014): default category dropdown to Structure so the picker
        // opens scoped to the most-common section tags. Sample has 2 Structure
        // entries ([Verse], [Chorus]) + 1 Vocal entry ([Whispered]) — initial
        // FilteredTags should contain only the 2 Structure entries.
        var vm = new MainViewModel(Sample);

        Assert.Equal("Structure", vm.SelectedCategory);
        Assert.Equal(2, vm.FilteredTags.Count);
        Assert.Contains(vm.FilteredTags, t => t.Bracket == "[Chorus]");
        Assert.Contains(vm.FilteredTags, t => t.Bracket == "[Verse]");
        Assert.DoesNotContain(vm.FilteredTags, t => t.Bracket == "[Whispered]");
    }

    [Fact]
    public void D2_ErrorConstructor_StillDefaultsToAllCategory()
    {
        // v1.13 (B-SUNO-014): error-state constructor explicitly preserved at
        // SelectedCategory="All" because Categories=new[] { "All" } only in that
        // branch — assigning "Structure" would point to a non-existent ComboBox
        // item. Regression-gate for that explicit non-change.
        var vm = new MainViewModel("simulated tags.json load failure");

        Assert.Equal("All", vm.SelectedCategory);
        Assert.Single(vm.Categories);
        Assert.Equal("All", vm.Categories[0]);
    }
}
