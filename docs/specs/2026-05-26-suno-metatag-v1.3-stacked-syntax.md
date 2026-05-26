# SunoMetatagApp v1.3 — Stacked Metatag Syntax (Shift+click Merge)

**Status:** draft (B-SUNO-004 r1)
**Date:** 2026-05-26
**Supersedes:** none. Extends v1.1 inline-tag-insertion behavior; v1.2 visual layer is orthogonal and unchanged.
**Behavioral version:** v1.3 (visual remains v1.2 dark Suno theme).

## 1. Motivation

Suno AI accepts a **stacked-metatag syntax** within a single bracket using the `|` separator: `[Guitar | Drums | Bass]`. Today, v1.1 inline insertion always produces *one tag per bracket*. A user wanting `[Guitar | Drums]` must:

1. Click `[Guitar]` to insert.
2. Use the arrow keys to navigate inside the `]`.
3. Type ` | Drums` manually.

This is friction-heavy for a workflow Suno users hit often (mood-pair, instrument-pair, vocal+style stacks). v1.3 adds a one-click stacking primitive that produces canonical Suno stacked syntax with zero text-typing.

### Suno format reference

Per Suno AI prompt-engineering docs and community-curated reference (e.g. `sunometatagcreator.com`), the canonical stacked form is:

```
[Tag A | Tag B | Tag C]
```

- Single space on each side of `|`.
- All tags share one bracket.
- A line may contain multiple bracket groups (`[Verse] [Acoustic | Soft]`).
- Stacking is semantically distinct from concatenation (`[Tag A][Tag B]`) — Suno parses `|` as "all of these together," while concatenated brackets are independent attribute layers.

v1.3 emits the canonical form; concatenation remains available via plain click.

## 2. Behavioral contract

### 2.1 Plain click (unchanged)

Same as v1.1: inserts `[Tag B]` at the caret position in the focused lyric textbox, replacing any selected text. Identical to v1.1's `MainViewModel.InsertTag(TagViewModel)` semantics.

### 2.2 Shift+click (new)

When the user holds **Shift** while clicking a tag button in the picker:

1. App locates the **merge target**: the closest complete `[...]` bracket block on the **current line** in the focused lyric textbox, evaluated relative to the caret position (precise algorithm in §3).
2. If a merge target is found, app inserts ` | <inner-name>` immediately before the merge target's closing `]`. Result: `[Existing | New]` (or `[A | B | New]` if already stacked).
3. If no merge target is found, app **falls back to plain insert** at caret — identical to plain click. No error, no UX surprise.

### 2.3 Modifier-key choice: Shift

Shift is the chosen modifier. Rationale:

- **No conflict with lyric-textbox text-selection.** Picker buttons are `Focusable="False"` (preserved from v1.1), so clicking a picker button does not move keyboard focus or change the textbox caret. Holding Shift while clicking the picker has zero effect on the textbox's selection state. Shift state is read at click-time only.
- **Discoverable.** Convention for "extended" or "with-existing" operations across most desktop apps.
- **Not Ctrl** — Ctrl+C/X/V muscle memory is heavy in the lyric textbox; even though the modifier is only read at picker-click time, Ctrl risks user confusion ("did I just copy the tag?").
- **Not Alt** — Alt is less keyboard-ergonomic and less commonly understood as "with-existing."

### 2.3.1 Modifier sampling & accessibility notes (absorbed from specialist r1 LOW 4 + 7)

- **Modifier release timing.** `Keyboard.Modifiers` is sampled synchronously when the `Click` event fires (after mouse-up). If the user releases Shift between mouse-down and mouse-up, the action degrades to plain-click semantics (selection consumed if present). This is the WPF Click-event reality; not a bug. Mitigation is "release one Undo" or one more click. Alternative architectures (e.g., handling `MouseLeftButtonDown` to sample at press time) trade away click-vs-drag discrimination and keyboard Space/Enter button activation, which is worse — so `Click` with current-modifier is the right call. Documented for future maintainer.
- **Sticky Keys compatibility.** Windows Sticky Keys (accessibility feature) is compatible — Shift can be latched in a separate keypress and the modifier state persists across the subsequent click. One-handed users can perform Shift+click without simultaneously pressing both. Other modifier emulation paths (voice control, switch-access) depend on the host OS layer and are out of scope.

### 2.4 Selection handling

If the lyric textbox has an active selection at Shift+click time, the selection is **ignored** for merge-target evaluation. The caret position (`SelectionStart`) is used as the reference. The selection is NOT consumed; the merged tag is inserted into the existing bracket, leaving any selected text untouched.

For plain click, selection consumption remains identical to v1.1 (selection is replaced by the inserted bracket).

**Mixed-mode fallback semantics (absorbed from specialist r1 LOW 3):** when Shift+click falls back to plain insert (no merge target found on the current line) AND a selection is active, the selection IS replaced per plain-insert semantics. The fallback path is `InsertTag(tag)` invoked verbatim — same selection-consumption behavior as v1.1 plain click. Unit test T15 covers this scenario explicitly.

## 3. Merge algorithm (formal)

Given:
- `lyrics` = current `Section.Lyrics` string
- `caret` = `FocusedCaretPosition`, clamped to `[0, lyrics.Length]`
- `tagBracket` = the clicked tag's `Bracket` property (e.g. `"[Chorus]"`)
- `innerName` = `tagBracket` with surrounding `[ ]` trimmed (e.g. `"Chorus"`)

### 3.1 Determine current-line bounds

```
lineStart = (caret == 0) ? 0 : lyrics.LastIndexOf('\n', caret - 1) + 1
lineEnd   = lyrics.IndexOf('\n', caret)
            (or lyrics.Length if no '\n' exists at or after caret)
```

The "current line" is `lyrics[lineStart..lineEnd]`. `\r\n` line endings are handled because the algorithm only cares about `\n`; the `\r` (if present) stays at the end of the prior segment and does not bleed into the current line's bracket-search range.

### 3.2 Caret-inside-bracket case (priority 1)

Find the most recent `[` on the current line at position `≤ caret`:

```
lastOpen = lyrics.LastIndexOf('[', max(lineStart, caret - 1))
```

If `lastOpen >= lineStart` AND no `]` exists in `lyrics[lastOpen .. caret-1]`:
- The caret is **inside** a bracket whose opening is `lastOpen` and whose closing `]` is at some position `nextClose >= caret`.
- If `nextClose` is within `[lineStart, lineEnd]`, the merge target is this bracket. The append position is `nextClose`.

### 3.3 Walk-left-for-complete-bracket case (priority 2)

If priority 1 doesn't apply (caret is not inside any bracket on this line), walk left from `caret - 1` looking for the most recent `]` on the current line:

```
closeIdx = lyrics.LastIndexOf(']', max(lineStart, caret - 1))
```

If `closeIdx >= lineStart`:
- Find the most recent `[` before `closeIdx`: `openIdx = lyrics.LastIndexOf('[', closeIdx)`.
- If `openIdx >= lineStart`, the merge target is the bracket `[openIdx..closeIdx]`. The append position is `closeIdx`.
- If `openIdx < lineStart`, the `]` is unmatched on this line → no merge target via this `]`. Continue walking left for an earlier `]` (rare; only occurs on malformed lines).

### 3.4 No merge target found

If neither priority 1 nor priority 2 produces a merge target, fall back to plain insert: invoke the existing `InsertTag(TagViewModel)` path.

### 3.5 Append semantics

When a merge target is found at `appendPosition` (the position of the closing `]`):

```
insertText = " | " + innerName
newLyrics  = lyrics.Insert(appendPosition, insertText)
```

Whitespace contract: always ` | ` (space-pipe-space). The app does not adapt to existing whitespace patterns in the bracket (e.g., if user had typed `[Verse|Drums]` without spaces, the merge still produces `[Verse|Drums | New]` — canonical form for the new tag, user's malformed form preserved for existing).

### 3.6 Caret landing position

After merge:

```
FocusedCaretPosition = appendPosition + insertText.Length + 1
                     // +1 to land AFTER the closing ']'
FocusedSelectionLength = 0
```

This places the caret immediately after the merged-into bracket's `]`, so the user can Shift+click another tag immediately to keep stacking (`[A | B] → [A | B | C] → [A | B | C | D]`).

## 4. Enumerated edge cases & expected outputs

Notation: `|<-caret` marks the caret position.

| # | Before | Action | After | Notes |
|---|---|---|---|---|
| 1 | `[Verse]|<-caret` | Shift+click `[Chorus]` | `[Verse \| Chorus]|<-caret` | Priority 2 (walk-left): closest `]` is the `[Verse]` close. |
| 2 | `Hello [Verse] |<-caret` | Shift+click `[Chorus]` | `Hello [Verse \| Chorus] |<-caret`* | Priority 2. *Caret lands after the new `]`, then the space remains as user content. |
| 3 | `[Ver\|<-caretse]` (caret inside) | Shift+click `[Chorus]` | `[Verse \| Chorus]|<-caret` | Priority 1: caret inside bracket. |
| 4 | `\|<-caret` (empty line, prev line `[Verse]`) | Shift+click `[Chorus]` | `[Chorus]|<-caret` (plain insert) | No bracket on current line → fallback. |
| 5 | `[Verse] lyric [Bridge]|<-caret` | Shift+click `[Chorus]` | `[Verse] lyric [Bridge \| Chorus]|<-caret` | Priority 2: closest `]` to left is `[Bridge]`'s. |
| 6 | `[Verse] lyric |<-caret [Bridge]` | Shift+click `[Chorus]` | `[Verse \| Chorus] lyric |<-caret [Bridge]` | Priority 2: closest `]` left of caret is `[Verse]`'s. |
| 7 | `\|<-caret some text` (line has no bracket) | Shift+click `[Chorus]` | `[Chorus]\|<-caret some text` (plain insert) | No bracket on current line → fallback. |
| 8 | (empty section, caret at 0) | Shift+click `[Chorus]` | `[Chorus]\|<-caret` (plain insert) | Fallback. |
| 9 | `[Verse \| Drums]|<-caret` (already stacked) | Shift+click `[Chorus]` | `[Verse \| Drums \| Chorus]|<-caret` | Priority 2: appends to already-stacked bracket. |
| 10 | `[Ver\|<-caretse` (unclosed bracket) | Shift+click `[Chorus]` | `[Ver[Chorus]\|<-caretse` (plain insert) | No complete `[...]` block on line; caret is not "inside" because no `]` follows on this line → fallback. App does NOT auto-close brackets. |
| 11 | Selection active: `[Ver{se]` selected}, Shift+click `[Chorus]` | `[Verse \| Chorus]` (selection ignored) | Priority 1 (caret inside if SelectionStart is inside) or Priority 2; selection is NOT consumed by Shift+click. |
| 12 | `[A]\n[B]|<-caret` (caret on line 2) | Shift+click `[C]` | `[A]\n[B \| C]|<-caret` | Line scope ensures merge target is `[B]`, not `[A]`. |
| 13 | `[A]|<-caret\n[B]` (caret on line 1, right after `]`) | Shift+click `[C]` | `[A \| C]|<-caret\n[B]` | Priority 2 within line 1. |
| 14 | `Suno style \|<-caret| (caret after a literal `|` typed by user) | Shift+click `[C]` | `[C]\|<-caret` somewhere — no bracket on line → fallback. | Literal `|` outside a bracket is not a merge anchor. |

## 5. Non-goals

- **No auto-merge on plain click.** Plain click semantics are identical to v1.1; users opted into merge via the explicit Shift modifier.
- **No bracket auto-closing.** If the user has malformed brackets (unclosed `[`, dangling `]`), v1.3 does not repair them. Fallback to plain insert.
- **No whitespace normalization.** Existing bracket contents are preserved verbatim. Only the new tag is appended with canonical ` | ` spacing. **Specialist r1 LOW 5 absorbed:** empty bracket `[]` + Shift+click produces `[ | NewTag]` (leading space inside bracket). This is malformed-but-canonical-for-the-new-tag; v1.3 does not normalize whitespace inside pre-existing bracket contents, even when those contents are empty. Test T16 documents the behavior; user is responsible for cleaning up if they intentionally type `[]`.
- **No multi-tag selection.** v1.3 does not add a "select multiple tags then click" UX. One Shift+click adds one tag.
- **No visual preview when Shift is held.** v1.3 does not light up the merge target or change picker button appearance based on modifier state. (Could be added as a polish slice in v1.4; out of scope here.)
- **No CHANGES to the picker style, ComboBox, color theme, focus model, defer-clear, section reorder, error banner, or any other v1.1/v1.2 surface.**

## 6. Architecture impact

### 6.1 New VM RelayCommand method

`MainViewModel.InsertTagStacked(TagViewModel?)` — exposed via `[RelayCommand]` attribute so a `InsertTagStackedCommand` property is auto-generated by `CommunityToolkit.Mvvm` (matches the v1.1 toolkit pattern at `MainViewModel.cs:95` for `InsertTagCommand`). The method itself is `private` per the toolkit convention but is **invocable from unit tests via the auto-generated `InsertTagStackedCommand.Execute(tag)` path**, and from code-behind via the same Command property. Per specialist r1 LOW 2:

```csharp
[RelayCommand]
private void InsertTagStacked(TagViewModel? tag)
{
    if (tag is null || FocusedSection is null) { InsertTag(tag); return; }
    var section = FocusedSection;
    var lyrics = section.Lyrics ?? string.Empty;
    var caret = Math.Clamp(FocusedCaretPosition, 0, lyrics.Length);

    // 3.1 line bounds
    int lineStart = caret == 0 ? 0 : lyrics.LastIndexOf('\n', caret - 1) + 1;
    int lineEnd = lyrics.IndexOf('\n', caret);
    if (lineEnd < 0) lineEnd = lyrics.Length;

    int? appendAt = null;

    // 3.2 caret-inside-bracket
    if (caret > lineStart)
    {
        int lastOpen = lyrics.LastIndexOf('[', caret - 1);
        if (lastOpen >= lineStart)
        {
            int closeAfter = lyrics.IndexOf(']', lastOpen + 1);
            // If no ']' between lastOpen and caret, and a ']' exists after caret on this line:
            int gapLen = caret - (lastOpen + 1);
            bool noCloseBeforeCaret = gapLen <= 0
                || lyrics.IndexOf(']', lastOpen + 1, gapLen) < 0;
            if (noCloseBeforeCaret && closeAfter >= caret && closeAfter <= lineEnd)
            {
                appendAt = closeAfter;
            }
        }
    }

    // 3.3 walk-left-for-complete-bracket
    if (appendAt is null && caret > lineStart)
    {
        int closeIdx = lyrics.LastIndexOf(']', caret - 1);
        while (closeIdx >= lineStart)
        {
            int openIdx = lyrics.LastIndexOf('[', closeIdx - 1);
            if (openIdx >= lineStart) { appendAt = closeIdx; break; }
            if (closeIdx == lineStart) break;
            closeIdx = lyrics.LastIndexOf(']', closeIdx - 1);
        }
    }

    // 3.4 fallback
    if (appendAt is null) { InsertTag(tag); return; }

    // 3.5 append
    var innerName = (tag.Bracket ?? string.Empty).Trim('[', ']');
    var insertText = " | " + innerName;
    section.Lyrics = lyrics.Insert(appendAt.Value, insertText);

    // 3.6 caret landing
    FocusedCaretPosition = appendAt.Value + insertText.Length + 1; // +1 past ']'
    FocusedSelectionLength = 0;
    CaretRestoreRequested?.Invoke(this, FocusedCaretPosition);
}
```

**Specialist r1 LOW 1 absorbed:** the dead-code `int openBetween` line from the r1 spec draft has been removed; the boolean expression now computes the same predicate inline (`lyrics.IndexOf(']', lastOpen + 1, gapLen) < 0`) and short-circuits when `gapLen <= 0` to avoid the `IndexOf(char, int, int)` argument-validation throw when `caret == lastOpen + 1`.

The auto-generated `InsertTagStackedCommand` supports future hotkey binding (e.g., [[Backlog#B-004]] hotkey work) without redesign.

### 6.2 Code-behind click router

Replace the `Command`/`CommandParameter` binding on the picker button with a `Click` handler in code-behind. The handler reads `Keyboard.Modifiers` and routes:

```csharp
private void TagPickerButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is not Button btn) return;
    if (btn.DataContext is not TagViewModel tag) return;
    if (DataContext is not MainViewModel vm) return;

    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        vm.InsertTagStackedCommand.Execute(tag);
    else
        vm.InsertTagCommand.Execute(tag);
}
```

This is a small step away from MVVM-pure command binding, but it is the conventional WPF idiom for modifier-aware buttons. Both VM commands remain fully unit-testable via their auto-generated `*Command.Execute(tag)` entry points; the code-behind only does modifier detection + dispatch.

### 6.3 XAML change

`MainWindow.xaml` (current line 229–233, inside the picker `ItemsControl.ItemTemplate`):

```xml
<!-- v1.2 (current) -->
<Button Content="{Binding Bracket}"
        ToolTip="{Binding Description}"
        ToolTipService.IsEnabled="{Binding Description, Converter={StaticResource StringIsNotEmptyConverter}}"
        Command="{Binding DataContext.InsertTagCommand, RelativeSource={RelativeSource AncestorType=Window}}"
        CommandParameter="{Binding}" />
```

```xml
<!-- v1.3 -->
<Button Content="{Binding Bracket}"
        ToolTip="{Binding Description}"
        ToolTipService.IsEnabled="{Binding Description, Converter={StaticResource StringIsNotEmptyConverter}}"
        Click="TagPickerButton_Click" />
```

The picker-scoped `Focusable="False"` Setter (line 212) remains in effect because the implicit-style `BasedOn="{StaticResource SunoTagPill}"` is unchanged. The dim-when-no-focus DataTrigger (line 215–219) is unchanged. The picker visual layer is fully preserved.

### 6.3.1 Accessibility (AutomationProperties.HelpText) — absorbed from specialist r1 LOW 6

Add `AutomationProperties.HelpText` to the picker-scoped Style so screen readers / assistive tech announce the Shift+click modifier action even when no tooltip is visible:

```xml
<Style TargetType="Button" BasedOn="{StaticResource SunoTagPill}">
    <Setter Property="Focusable" Value="False" />
    <Setter Property="Opacity" Value="1.0" />
    <Setter Property="AutomationProperties.HelpText"
            Value="Click to insert. Shift+click to stack into the nearest bracket on the current line." />
    ...
</Style>
```

WPF announces `AutomationProperties.HelpText` via UI Automation regardless of `ToolTipService.IsEnabled` precedence, so this is independent of the tooltip-discoverability investigation in §6.4 / LOW 8.

### 6.4 Tooltip enhancement (optional, in-scope)

Update the picker-scoped Style trigger tooltip to mention Shift+click discoverability when focus is present:

```xml
<!-- Default tooltip (focus present): "Click to insert; Shift+click to stack into nearest bracket." -->
<Setter Property="ToolTip" Value="Click to insert; Shift+click to stack into nearest bracket." />
```

This Setter goes on the base Style (not inside the DataTrigger). The DataTrigger's tooltip override (when `FocusedSection == null`) stays as the v1.2 dim-mode hint.

**Alternative:** keep the per-tag `Description` tooltip from the button's local `ToolTip` binding (lines 230–231) which takes precedence over the Style Setter. In that case, add the Shift+click hint to `Description` strings in `tags.json` — but that's content-side, out-of-scope for v1.3. **Decision:** add the Style-level Setter as a fallback; per-button local `ToolTip` still wins when a tag has a description.

**Specialist r1 LOW 8 verification gate (PIN AT T7 SMOKE):** the per-button XAML at line 231 sets `ToolTipService.IsEnabled="{Binding Description, Converter={StaticResource StringIsNotEmptyConverter}}"`. WPF's `ToolTipService.IsEnabled` is an attached property at the element level — when `false`, no tooltip dispatches from this element regardless of where the `ToolTip` value originates (local attribute, Style Setter, or DataTrigger Setter). Hypothesis: for tags without a `Description` (currently 114 of ~115 tags), `IsEnabled=false` may suppress the Style-level Shift+click hint too. **Verify at T7 dev smoke-launch: hover a description-less tag button — does the "Click to insert; Shift+click to stack..." tooltip appear?** If NO, the discoverability mechanism breaks for the majority case and one of the following must land before USER REVIEW:

- **Option A:** remove `ToolTipService.IsEnabled` from the per-button XAML so the Style-level Setter always reaches the user (empty `ToolTip` binding on description-less tags would show an empty box — need to verify behavior).
- **Option B:** restructure the per-button binding to fall back to the Style-level hint when `Description` is empty.
- **Option C:** rely solely on `AutomationProperties.HelpText` (LOW 6, §6.3.1) for screen-reader discoverability and accept that visual discoverability requires hovering a tag with a `Description`.

If LOW 8 reveals the hint shows correctly (because `ToolTipService.IsEnabled` only suppresses tooltips when the inheritance chain has explicitly enabled one but its value is empty), no action needed — Style-level Setter delivers as designed.

## 7. Test plan

### 7.1 New unit tests (`MainViewModelInsertTagStackedTests.cs`)

| # | Scenario | Expected |
|---|---|---|
| T1 | Empty section, caret at 0 | Falls back to plain insert: `[Chorus]`, caret at 8 |
| T2 | Lyrics `[Verse]`, caret at 7 (after `]`) | `[Verse \| Chorus]`, caret at 17 |
| T3 | Lyrics `Hello [Verse]`, caret at 13 (after `]`) | `Hello [Verse \| Chorus]`, caret at 23 |
| T4 | Lyrics `[Verse]`, caret at 3 (inside, between `e` and `r`) | `[Verse \| Chorus]`, caret at 17 |
| T5 | Lyrics `Hello world`, caret at 11 (no bracket on line) | `Hello world[Chorus]`, caret at 19 (plain insert) |
| T6 | Lyrics `[Verse]\n`, caret at 8 (line 2, empty) | `[Verse]\n[Chorus]`, caret at 16 (plain insert; prev line bracket ignored) |
| T7 | Lyrics `[Verse] middle [Bridge]`, caret at 23 (after `[Bridge]`) | `[Verse] middle [Bridge \| Chorus]`, caret at 33 |
| T8 | Lyrics `[Verse] middle [Bridge]`, caret at 8 (between `]` and ` `) | `[Verse \| Chorus] middle [Bridge]`, caret at 17 |
| T9 | Lyrics `[Verse \| Drums]`, caret at 15 (after `]`) | `[Verse \| Drums \| Chorus]`, caret at 25 |
| T10 | Lyrics `[Ver`, caret at 4 (unclosed) | `[Ver[Chorus]`, caret at 12 (plain insert fallback) |
| T11 | Lyrics `[Verse]`, caret at 0 (before `[`) | `[Chorus][Verse]`, caret at 8 (plain insert; no bracket left of caret on line) |
| T12 | Tag is null | No-op (no exception) |
| T13 | FocusedSection is null | Falls back to plain InsertTag (which also no-ops): no-op |
| T14 | Selection active inside bracket: `[Ver` + selected `se` + `]`, SelectionStart=4, SelectionLength=2 | `[Verse \| Chorus]`, caret at 17 (selection ignored, merged) |
| T15 | Mixed-mode fallback (absorbed from spec LOW 3): selection active on a bracket-free line, e.g. `Hello world` with `wor` selected (SelectionStart=6, SelectionLength=3), Shift+click `[Chorus]` | `Hello [Chorus]ld`, caret at 14 (selection IS replaced via plain-insert fallback semantics) |
| T16 | Empty bracket (absorbed from spec LOW 5): `[]` (length 2), caret at 2 (after `]`), Shift+click `[Verse]` | `[ \| Verse]`, caret at 11 (Priority 2 merge into empty bracket; leading-space artifact documented as non-goal in §5) |

### 7.2 Carry-over regression tests

All **31 existing unit tests** must continue to pass without modification. The existing `InsertTag` tests verify plain-click semantics are unchanged.

### 7.3 Manual smoke matrix (USER REVIEW)

| # | Scenario | Pass criteria |
|---|---|---|
| S1 | Launch app; type `Hello`; click `[Verse]`; observe | `Hello[Verse]` with caret after `]` (v1.1 plain-click unchanged) |
| S2 | In `[Verse]|`, Shift+click `[Chorus]` | `[Verse \| Chorus]` with caret after `]` |
| S3 | Continue from S2: Shift+click `[Bridge]` (still after `]`) | `[Verse \| Chorus \| Bridge]` — stack-of-3 works |
| S4 | Empty section, Shift+click `[Verse]` | `[Verse]` (fallback to plain insert) |
| S5 | `Hello world|`, Shift+click `[Chorus]` | `Hello world[Chorus]` (fallback; caret on line w/o bracket) |
| S6 | `[Verse]\n|` (caret on line 2), Shift+click `[Chorus]` | `[Verse]\n[Chorus]` (fallback; line scope) |
| S7 | Plain click still works after S2-S6 | Plain click inserts `[Tag]` at caret unchanged |
| S8 | Focus model unchanged: click between sections, then Shift+click tag | Insert into focused section's bracket if any |
| S9 | Picker dim affordance: no focus → all picker buttons dimmed → Shift+click is also dimmed (no merge) | Click any tag (Shift or not) does nothing per v1.1 FocusedSection==null guard |
| S10 | Copy preview: stacked output renders verbatim | Preview text shows `[Verse \| Chorus]` exactly |
| S11 | Visual layer unchanged from v1.2: fuchsia pills, violet focused border, dark theme intact | No regression from v1.2 |
| S12 | **47 unit tests** still green (31 v1 carry-over + 16 new for T1–T16) | `dotnet test` output |
| S13 | **LOW 8 verification (absorbed from specialist r1):** hover a description-less tag button (e.g., `[Verse]`) on the published exe → does the Shift+click hint tooltip appear? | EITHER tooltip appears (Style Setter reaches user) OR fix-cycle lands per §6.4 options A/B/C |

## 8. Wiki impact

- **UPDATE** [[sunometatag-inline-editor]] — add a new section "Stacked-syntax insertion (Shift+click)" near the existing "Focus tracking + caret restoration" section. Refresh `last_confirmed: 2026-05-26`.
- **UPDATE** [[sunometatag-app]] — bump title to v1.3, add "v1.2 → v1.3 (2026-05-26)" subsection above the existing v1.1→v1.2 subsection.
- **NEW (small):** none. No new architecture / decision / risk pages needed; this is a behavioral refinement layered on the existing inline-editor architecture.
- **Archive:** v1.3 RESULT will be archived as Archive entry 9 at closeout per the retention rule.

## 9. Migration risk

- **Picker-button binding change** (Command → Click handler) is the primary migration risk. If the existing `InsertTagCommand` is referenced elsewhere (tests, alternate XAML surfaces), those references should continue to work — the command binding stays in the VM, only the XAML button uses Click instead.
- **`InsertTagStackedCommand`** is exposed for callers that prefer command-binding (e.g., future hotkey support — see [[Backlog#B-004]]).
- **Backwards-compat for v1.1/v1.2 smoke cases:** all v1.1 plain-click cases must produce identical output. The Shift+click path is purely additive.

## 10. Non-functional contracts

- **Performance:** merge algorithm is `O(L)` in line length L (line bounds + 2 `LastIndexOf` per priority). Lyric sections are short (~hundreds of chars typically); zero performance concern.
- **Determinism:** algorithm is fully deterministic given `(lyrics, caret, tag)`; no clock/random dependency.
- **No I/O:** pure string manipulation + property setter; no clipboard, file, or network access.

## 11. Open scope decisions

Decisions surfaced for Specialist + Lead Reviewer ratification at plan-phase review:

1. **Backlog ID namespace.** Lead's directive uses `B-SUNO-004`. v1's `BACKLOG.md` uses `B-001..B-024`. The `B-SUNO-NNN` namespace is not yet formally documented in `BACKLOG.md`'s header. Planner-default: keep the `B-SUNO-NNN` namespace as a documented prefix for "Suno-specific feature work" and ratify at closeout. Alternative: remap to `B-028` (or next available).
2. **Wiki extension vs new page.** Planner-default: extend [[sunometatag-inline-editor]] with a stacked-syntax section (refinement of inline insertion, not orthogonal). Alternative: create [[sunometatag-stacked-syntax]] as a new architecture page if the Lead prefers stronger separation.
3. **Optional `InsertTagStackedCommand`.** Add the RelayCommand wrapper or expose only the public method? Planner-default: expose both (cheap), `InsertTagStackedCommand` is needed if future hotkey work (B-004) wants to bind a key to it.
4. **Style-level Shift+click tooltip Setter.** Add or skip? Planner-default: add at base Style level; per-button local `ToolTip` (the `Description` binding) still wins for tagged tags.

## 12. Source paths

- `j:\SunoMetatagApp\src\SunoMetatagApp\ViewModels\MainViewModel.cs` (+ `InsertTagStacked` method, ~50 LOC)
- `j:\SunoMetatagApp\src\SunoMetatagApp\MainWindow.xaml` (picker button: replace Command binding with Click handler, ~4 line delta)
- `j:\SunoMetatagApp\src\SunoMetatagApp\MainWindow.xaml.cs` (+ `TagPickerButton_Click`, ~10 LOC)
- `j:\SunoMetatagApp\tests\SunoMetatagApp.Tests\MainViewModelInsertTagStackedTests.cs` (new, ~14 test cases)
