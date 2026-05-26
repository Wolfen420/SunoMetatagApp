using SunoMetatagApp.Models;
using SunoMetatagApp.ViewModels;
using Xunit;

namespace SunoMetatagApp.Tests;

// v1.3 (B-SUNO-004): InsertTagStacked covers the Shift+click merge UX.
// Spec: docs/specs/2026-05-26-suno-metatag-v1.3-stacked-syntax.md S3 + S4 + S7.1.
public class MainViewModelInsertTagStackedTests
{
    private static readonly TagDefinition[] Sample =
    {
        new("Structure", "Verse",   "[Verse]"),
        new("Structure", "Chorus",  "[Chorus]"),
        new("Structure", "Bridge",  "[Bridge]"),
        new("Instrument","Drums",   "[Drums]"),
    };

    private static TagViewModel Chorus() => new(Sample[1]);
    private static TagViewModel Bridge() => new(Sample[2]);

    private static (MainViewModel vm, Models.Section section) NewVmWithFocusedSection()
    {
        var vm = new MainViewModel(Sample);
        var section = vm.Sections[0];
        vm.FocusedSection = section;
        return (vm, section);
    }

    // T1 — Empty section, caret at 0 → falls back to plain insert
    [Fact]
    public void T1_EmptySection_CaretZero_FallsBackToPlainInsert()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "";
        vm.FocusedCaretPosition = 0;

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Chorus]", s.Lyrics);
        Assert.Equal(8, vm.FocusedCaretPosition);
    }

    // T2 — [Verse] caret at 7 (after ]) → merge
    [Fact]
    public void T2_AfterClosingBracket_MergesIntoExisting()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse]";
        vm.FocusedCaretPosition = 7;

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse | Chorus]", s.Lyrics);
        Assert.Equal(16, vm.FocusedCaretPosition); // past new ']' = end of "[Verse | Chorus]"
    }

    // T3 — "Hello [Verse]" caret at 13 (after ]) → merge
    [Fact]
    public void T3_AfterBracketWithLeadingText_MergesIntoBracket()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "Hello [Verse]";
        vm.FocusedCaretPosition = 13;

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("Hello [Verse | Chorus]", s.Lyrics);
        Assert.Equal(22, vm.FocusedCaretPosition);
    }

    // T4 — [Verse] caret at 3 (inside, between V/e and e/r) → caret-inside-bracket merge
    [Fact]
    public void T4_CaretInsideBracket_MergesIntoContainingBracket()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse]";
        vm.FocusedCaretPosition = 3; // between 'e' and 'r'

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse | Chorus]", s.Lyrics);
        Assert.Equal(16, vm.FocusedCaretPosition);
    }

    // T5 — "Hello world" caret at 11, no bracket on line → fallback
    [Fact]
    public void T5_NoBracketOnLine_FallsBackToPlainInsert()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "Hello world";
        vm.FocusedCaretPosition = 11;

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("Hello world[Chorus]", s.Lyrics);
        Assert.Equal(19, vm.FocusedCaretPosition);
    }

    // T6 — "[Verse]\n" caret at 8 (line 2 start) → fallback (line scope)
    [Fact]
    public void T6_PrevLineHasBracket_CurrentLineEmpty_FallsBack()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse]\n";
        vm.FocusedCaretPosition = 8;

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse]\n[Chorus]", s.Lyrics);
        Assert.Equal(16, vm.FocusedCaretPosition);
    }

    // T7 — Two brackets, caret after rightmost → merge into rightmost
    [Fact]
    public void T7_MultipleBracketsOnLine_MergesIntoRightmost()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse] middle [Bridge]";
        vm.FocusedCaretPosition = 23; // after '[Bridge]' close

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse] middle [Bridge | Chorus]", s.Lyrics);
        Assert.Equal(32, vm.FocusedCaretPosition);
    }

    // T8 — Two brackets, caret between them (left of [Bridge]) → merge into [Verse]
    [Fact]
    public void T8_CaretBetweenBrackets_MergesIntoLeftBracket()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse] middle [Bridge]";
        vm.FocusedCaretPosition = 8; // right after '[Verse] '

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse | Chorus] middle [Bridge]", s.Lyrics);
        Assert.Equal(16, vm.FocusedCaretPosition);
    }

    // T9 — Already-stacked bracket → appends to it
    [Fact]
    public void T9_AlreadyStacked_AppendsAnotherTag()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse | Drums]";
        vm.FocusedCaretPosition = 15; // after ']'

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse | Drums | Chorus]", s.Lyrics);
        Assert.Equal(24, vm.FocusedCaretPosition);
    }

    // T10 — Unclosed bracket → fallback (no complete [...] block on line)
    [Fact]
    public void T10_UnclosedBracket_FallsBackToPlainInsert()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Ver";
        vm.FocusedCaretPosition = 4;

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Ver[Chorus]", s.Lyrics);
        Assert.Equal(12, vm.FocusedCaretPosition);
    }

    // T11 — Caret at 0, bracket starts at 0 (no chars left of caret) → fallback
    [Fact]
    public void T11_CaretBeforeBracketStart_FallsBack()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse]";
        vm.FocusedCaretPosition = 0;

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Chorus][Verse]", s.Lyrics);
        Assert.Equal(8, vm.FocusedCaretPosition);
    }

    // T12 — Null tag → no-op
    [Fact]
    public void T12_NullTag_IsNoOp()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse]";
        vm.FocusedCaretPosition = 7;

        vm.InsertTagStackedCommand.Execute(null);

        Assert.Equal("[Verse]", s.Lyrics);
        Assert.Equal(7, vm.FocusedCaretPosition);
    }

    // T13 — FocusedSection null → falls back to InsertTag which also no-ops
    [Fact]
    public void T13_NoFocusedSection_IsNoOp()
    {
        var vm = new MainViewModel(Sample);
        vm.Sections[0].Lyrics = "[Verse]";
        // FocusedSection stays null

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse]", vm.Sections[0].Lyrics);
    }

    // T14 — Caret inside bracket with active selection → selection ignored, merged
    [Fact]
    public void T14_SelectionInsideBracket_SelectionIgnored_StillMerges()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[Verse]";
        vm.FocusedCaretPosition = 4; // between 'r' and 's'
        vm.FocusedSelectionLength = 2; // 'se'

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("[Verse | Chorus]", s.Lyrics);
        Assert.Equal(16, vm.FocusedCaretPosition);
    }

    // T15 — Specialist LOW 3: mixed-mode fallback — Shift+click on bracket-free line
    // with active selection → fallback to plain insert, which REPLACES the selection.
    [Fact]
    public void T15_MixedModeFallback_SelectionReplacedOnFallback()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "Hello world";
        vm.FocusedCaretPosition = 6;  // start of 'world'
        vm.FocusedSelectionLength = 3; // 'wor'

        vm.InsertTagStackedCommand.Execute(Chorus());

        Assert.Equal("Hello [Chorus]ld", s.Lyrics);
        Assert.Equal(14, vm.FocusedCaretPosition); // 6 + len("[Chorus]")
        Assert.Equal(0, vm.FocusedSelectionLength);
    }

    // T16 — Specialist LOW 5: empty bracket [] + Shift+click → "[ | Tag]"
    // documents the malformed-but-canonical-for-the-new-tag behavior.
    [Fact]
    public void T16_EmptyBracket_MergesWithLeadingSpaceArtifact()
    {
        var (vm, s) = NewVmWithFocusedSection();
        s.Lyrics = "[]";
        vm.FocusedCaretPosition = 2;

        vm.InsertTagStackedCommand.Execute(new TagViewModel(Sample[0])); // [Verse]

        Assert.Equal("[ | Verse]", s.Lyrics);
        Assert.Equal(10, vm.FocusedCaretPosition); // past new ']' = end of "[ | Verse]"
    }
}
