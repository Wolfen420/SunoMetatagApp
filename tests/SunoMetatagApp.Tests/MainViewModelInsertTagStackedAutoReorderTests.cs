using System.Linq;
using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.19 (B-027): InsertTagStacked auto-reorder by canonical SortOrder.
// Covers 2-token reorder, 3-token reorder, stable sort within same SortOrder,
// unknown-token default-99 to end, Genre=99 to end, fresh-bracket no-reorder,
// already-sorted-bracket new-token canonical slot, mid-group insertion.
//
// Sample uses the explicit 5-parameter TagDefinition record constructor form
// (Category, Label, Bracket, Description, SortOrder) so each test tag has the
// canonical SortOrder seeded — per Lead absorption #3 precision wording. The
// existing MainViewModelInsertTagStackedTests.cs T1-T16 use the 3-parameter
// form (relying on SortOrder=99 positional default) and remain stable no-op
// under v1.19 because all tags there resolve to the same SortOrder.
public class MainViewModelInsertTagStackedAutoReorderTests
{
    private static readonly TagDefinition[] Sample =
    {
        new("Structure",  "Verse",     "[Verse]",      null, 1),
        new("Structure",  "Chorus",    "[Chorus]",     null, 1),
        new("Vocal",      "Whispered", "[Whispered]",  null, 2),
        new("Instrument", "Drums",     "[Drums]",      null, 3),
        new("Instrument", "Guitar",    "[Guitar]",     null, 3),
        new("Mood",       "Cynical",   "[Cynical]",    null, 4),
        new("Effect",     "Distorted", "[Distorted]",  null, 5),
        new("Effect",     "Reverb",    "[Reverb]",     null, 5),
        new("SFX",        "Thunder",   "[Thunder]",    null, 6),
        new("Production", "Hi-Fi",     "[Hi-Fi]",      null, 7),
        new("Genre",      "Rock",      "[Rock]",       null, 99),
    };

    private static TagViewModel TagFor(string label) =>
        new(Sample.Single(t => t.Label == label));

    private static (MainViewModel vm, Section section) NewVmWithFocus()
    {
        var vm = new MainViewModel(Sample);
        var section = vm.Sections[0];
        vm.FocusedSection = section;
        return (vm, section);
    }

    // U1 — 2-token reorder: Effect(5)+Instrument(3) → Instrument before Effect.
    [Fact]
    public void U1_TwoTagReorder_InstrumentBeforeEffect()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "[Distorted]";
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.InsertTagStackedCommand.Execute(TagFor("Drums"));
        Assert.Equal("[Drums | Distorted]", s.Lyrics);
    }

    // U2 — 3-token reorder across two categories.
    [Fact]
    public void U2_ThreeTagReorder_OneInstrumentTwoEffects()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "[Distorted | Reverb]";
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.InsertTagStackedCommand.Execute(TagFor("Drums"));
        Assert.Equal("[Drums | Distorted | Reverb]", s.Lyrics);
    }

    // U3 — Stable sort within same SortOrder preserves user-typed order.
    // Verse + Chorus + Chorus are all Structure=1; the second Chorus added
    // by Shift+click should land after the existing Chorus (stable).
    [Fact]
    public void U3_StableSortWithinSameSortOrder_PreservesUserOrder()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "[Verse | Chorus]";
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.InsertTagStackedCommand.Execute(TagFor("Chorus"));
        Assert.Equal("[Verse | Chorus | Chorus]", s.Lyrics);
    }

    // U4 — Unknown token (not present in _allTags) defaults to 99 and sorts
    // to the end alongside Genre.
    [Fact]
    public void U4_UnknownToken_SortsToEnd()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "[CustomThing | Distorted]";
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.InsertTagStackedCommand.Execute(TagFor("Drums"));
        // Drums=3, Distorted=5, CustomThing=99 → Drums | Distorted | CustomThing
        Assert.Equal("[Drums | Distorted | CustomThing]", s.Lyrics);
    }

    // U5 — Genre falls to 99 default and sorts to end (Lead absorption #1
    // ratification: Genre stays at end for this slice).
    [Fact]
    public void U5_GenreToken_SortsToEnd()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "[Rock | Distorted]";
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.InsertTagStackedCommand.Execute(TagFor("Drums"));
        // Drums=3, Distorted=5, Rock=99 → Drums | Distorted | Rock
        Assert.Equal("[Drums | Distorted | Rock]", s.Lyrics);
    }

    // U6 — Fresh bracket (no existing bracket on the line) falls back to
    // InsertTag; the single-token result has no reorder logic to apply.
    [Fact]
    public void U6_FreshBracket_NoReorderApplies()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "";
        vm.FocusedCaretPosition = 0;
        vm.InsertTagStackedCommand.Execute(TagFor("Distorted"));
        Assert.Equal("[Distorted]", s.Lyrics);
    }

    // U7 — Already-canonically-sorted bracket: the new token lands in its
    // canonical slot (here at the end since Distorted=5 is the largest known).
    [Fact]
    public void U7_AlreadySortedBracket_NewTokenLandsInCanonicalSlot()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "[Verse | Drums]";
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.InsertTagStackedCommand.Execute(TagFor("Distorted"));
        Assert.Equal("[Verse | Drums | Distorted]", s.Lyrics);
    }

    // U8 — Mid-group insertion: Drums(3) slots between Structure(1) and Effect(5).
    [Fact]
    public void U8_MidGroupInsertion_NewTokenSlotsBetweenCategories()
    {
        var (vm, s) = NewVmWithFocus();
        s.Lyrics = "[Verse | Distorted]";
        vm.FocusedCaretPosition = s.Lyrics.Length;
        vm.InsertTagStackedCommand.Execute(TagFor("Drums"));
        Assert.Equal("[Verse | Drums | Distorted]", s.Lyrics);
    }
}
