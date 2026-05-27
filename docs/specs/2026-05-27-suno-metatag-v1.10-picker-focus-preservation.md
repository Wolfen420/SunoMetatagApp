# SunoMetatagApp v1.10 — Spec (B-SUNO-012 Tag-Picker Pane Focus Preservation)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-012 / v1.10 — Restore section focus when clicking inside the right tag-picker pane
- **Type:** Focused UX interaction fix — extends the v1.1 r2 defer-clear contract documented in `[[focus-flip-stale-insert]]`; no schema changes; no new VM commands; zero `tags.json` / `prompts.json` mutations
- **Working baseline:** `master` tip `1e623ce` (v1.9 closeout = B-SUNO-008b PASS, parent B-SUNO-008 retired)
- **Specialist activation forecast:** FRONTEND/UX (extends focus-handling code-behind); ENGINE remains out of scope (no geometry / determinism / persistence / export-contract surfaces)
- **Lead r1 absorptions** (carried in `target_item` from Lead's APPROVED PASS-WITH-NOTES verdict 2026-05-27):
  1. **USER REVIEW S4 ComboBox dropdown-open pause** — exercise the dropdown-open transient with a 1-2 second pause before closing, to validate the picker-pane guard survives Popup-boundary focus transitions.
  2. **§1 Q6 packet text S3/S5 numbering fix** — earlier plan-packet referred to "USER REVIEW S5" as the user's reported workflow; correct reference is **S3** (S5 in the v1.10 matrix is the preview-pane regression-gate).
  3. **Handler naming alignment** — the existing handler method name `LyricTextBox_LostFocus` mismatches the WPF event it subscribes to (`LostKeyboardFocus`). Rename method to `LyricTextBox_LostKeyboardFocus` and update the XAML attribute. Pure naming alignment; no behavioral change.
  4. **Wiki lifecycle field refresh** — at T7, `[[focus-flip-stale-insert]]` `last_confirmed` bumps to `2026-05-27` and `review_due` to `2026-11-27`.

## 1. Goal

Eliminate the user-reported friction where clicking SearchBox / Category ComboBox / empty space in the right tag-picker pane clears `MainViewModel.FocusedSection`, dimming the tag pills via the DataTrigger and silently breaking tag-pill insertion until the user clicks back into a lyric textbox.

User report verbatim (brainstorm 2026-05-27):

> *"The box that holds the chips on the right. It loses focus oddly. When I click in it it is not focused so I can't click on the chips to add them to the text box. I have to go back to the text box and click in it to get the chips to be clickable."*

**Resolves backlog item:** `B-SUNO-012` retires at v1.10 closeout.

## 2. Scope

### What this slice covers

- **`MainWindow.xaml` change:** add `x:Name="TagPickerPane"` to the right column root `<Grid Grid.Column="4">` at line 143. Update the `LyricTextBox` `LostKeyboardFocus` event wiring to call `LyricTextBox_LostKeyboardFocus` (renamed handler).
- **`MainWindow.xaml.cs` changes:**
  - Rename method `LyricTextBox_LostFocus` → `LyricTextBox_LostKeyboardFocus` to match the event it subscribes to (absorption #3).
  - Add new static helper `IsAncestorOf(DependencyObject ancestor, DependencyObject? descendant)` walking visual tree first, falling back to logical tree at boundaries (defensive coverage for ComboBox Popup transitions per absorption #1).
  - Extend the deferred continuation in `LyricTextBox_LostKeyboardFocus` with a third race-cancel check between the existing `sectionAtLossTime` check and the lyric-textbox check: skip the clear when `IsAncestorOf(TagPickerPane, Keyboard.FocusedElement)` returns true.
- **Test additions:** 1-2 unit tests in `MainViewModelTests.cs` documenting the VM-level invariant that `InsertTag` routes to `FocusedSection` regardless of which UI element holds keyboard focus.
- **No schema changes.** `Section`, `MainViewModel` state model unchanged.
- **No new VM commands.** `InsertTag` / `InsertTagStacked` / `FocusedSection` API unchanged.
- **No `tags.json` changes.** 30,421 B / 331 entries unchanged.
- **No `prompts.json` changes.** 75,743 B / 136 entries unchanged.
- **No new UI affordances.** No new buttons, no new sort/filter, no inline clear-search.

### What this slice does NOT cover (non-scope)

- **No section-card-chrome focus mechanism.** Initial reading of "section tag area" as the per-section card was clarified by user as NOT the scope — only the right tag-picker pane.
- **No tag-pill `Focusable` change.** Picker buttons remain `Focusable=False`.
- **No prompt-library / tag-library curation work.**
- **No new dim-affordance state.** Existing DataTrigger on `FocusedSection == null` continues to drive pill dim/bright behavior; v1.10 narrows when the trigger fires, not how it renders.
- **No caret manipulation.** WPF default behavior on `tb.Focus()` (from existing `OnCaretRestoreRequested`) handles caret restoration unchanged.
- **No carry-over v1.8 dormant findings absorbed** (SubGenre Visibility binding, narrow-window overflow, a11y annotations, etc.) — separate future cycles.

## 3. The mechanism (one new guard + one rename + one anchor)

### 3.1 The current friction (v1.9 closeout tip `1e623ce`)

1. User clicks lyric TextBox in Section A → `LyricTextBox_GotFocus` fires → `vm.FocusedSection = A`; `_currentFocusedTextBox` cached.
2. User clicks **SearchBox** in the right picker pane to filter tags → SearchBox takes keyboard focus → `LyricTextBox_LostKeyboardFocus` (renamed) fires on Section A's textbox → defer-clear scheduled via `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)`.
3. Deferred continuation runs:
   - Race-cancel check 1: `vm.FocusedSection != sectionAtLossTime` (A) → still A, no cancel.
   - Race-cancel check 2: `Keyboard.FocusedElement is TextBox focusedTb && focusedTb.DataContext is Section` → SearchBox's DataContext is the MainWindow VM, not a Section → check fails → does NOT skip clear.
   - **Clears `vm.FocusedSection = null`.**
4. DataTrigger on `FocusedSection == null` flips tag pills to dim.
5. User clicks a dim pill → `InsertTag` returns immediately (`if (tag is null || FocusedSection is null) return;`). Silent no-op.
6. User must click back into a lyric textbox to re-establish focus.

### 3.2 The fix (one new guard)

Extend the deferred continuation in `LyricTextBox_LostKeyboardFocus` with a third race-cancel check, between the existing checks 1 and 2:

```csharp
// v1.10 (B-SUNO-012): If keyboard focus moved to anywhere inside the tag-picker
// pane (SearchBox, Category ComboBox, scrollable pill area, etc.), treat it as
// "still working with the focused section" — don't clear FocusedSection.
// Tag-pill clicks (Focusable=False) route inserts to FocusedSection unchanged;
// SearchBox / ComboBox interactions don't disturb the lyric textbox's tracked
// caret position. Restores tag-pill clickability without forcing the user
// back into the lyric textbox.
if (Keyboard.FocusedElement is DependencyObject focused
    && IsAncestorOf(TagPickerPane, focused))
{
    return;
}
```

Plus a new helper method (mirrors the existing visual-tree-walker pattern, with logical-tree fallback for Popup boundaries per absorption #1):

```csharp
private static bool IsAncestorOf(DependencyObject ancestor, DependencyObject? descendant)
{
    var cur = descendant;
    while (cur != null)
    {
        if (ReferenceEquals(cur, ancestor)) return true;
        // Walk visual tree first (standard WPF parent chain).
        // Fall back to logical tree where the visual tree breaks at Popup/Adorner
        // boundaries (e.g., ComboBox dropdown ComboBoxItem inside a Popup has its
        // visual chain rooted in a PopupRoot whose VisualTreeHelper.GetParent is
        // null; the logical-tree chain bridges back to the ComboBox).
        var next = VisualTreeHelper.GetParent(cur) ?? LogicalTreeHelper.GetParent(cur);
        if (ReferenceEquals(next, cur)) break;  // safety: prevent infinite loop
        cur = next;
    }
    return false;
}
```

Plus the XAML anchor:

```xml
<!-- Right: tag picker -->
<Grid Grid.Column="4" Margin="4" x:Name="TagPickerPane">
```

### 3.3 Handler rename (absorption #3)

The existing handler method `LyricTextBox_LostFocus` subscribes to the WPF `LostKeyboardFocus` event (XAML line 130 of `MainWindow.xaml`). `LostFocus` and `LostKeyboardFocus` are different WPF events; method name mismatched the event for clarity. v1.10 renames:

- Method declaration: `MainWindow.xaml.cs` line 100 — `LyricTextBox_LostFocus` → `LyricTextBox_LostKeyboardFocus`.
- XAML attribute: `MainWindow.xaml` line 130 — `LostKeyboardFocus="LyricTextBox_LostFocus"` → `LostKeyboardFocus="LyricTextBox_LostKeyboardFocus"`.

No behavioral change; purely naming alignment with the event the handler subscribes to.

### 3.4 Why this is safe (preservation of v1.1 r2 contract)

The v1.1 r2 defer-clear contract has three load-bearing failure modes per `[[focus-flip-stale-insert]]`:

1. **Stale-insert** — `FocusedSection` non-null after focus left the lyric set, tag click inserts at stale caret. v1.10 narrows the "left the lyric set" definition to exclude the picker pane. Within the picker pane, the user is interacting with the picker (not editing the lyric), so the previously-focused lyric textbox remains the legitimate insertion target. **Stale-insert risk does not reopen.**
2. **Lost-during-quick-flip** — without defer, momentary focus loss to a `Focusable=False` button would clear `FocusedSection` mid-interaction. v1.10 preserves the defer-clear; the new guard only narrows the "skip clear" set.
3. **Stuck affordance** — without race-cancel check 1, `LostFocus` from Section A could overwrite a `GotFocus` set to Section B. v1.10 leaves race-cancel check 1 untouched.

The third guard ONLY skips the clear when focus moved into the picker pane. All other paths (preview pane click, window chrome, another window, non-picker UI) continue to clear `FocusedSection` per the v1.1 r2 contract.

### 3.5 Behavior matrix (v1.10 vs v1.9)

| Focus-source → focus-destination | v1.9 behavior | v1.10 behavior |
|---|---|---|
| Section A textbox → Section A textbox (no-op) | `FocusedSection = A` preserved | unchanged |
| Section A textbox → Section B textbox | `FocusedSection = B` (race-cancel check 1) | unchanged |
| Section A textbox → SearchBox | **`FocusedSection = null` (pills dim)** | **`FocusedSection = A` preserved (pills stay bright)** |
| Section A textbox → Category ComboBox | **`FocusedSection = null` (pills dim)** | **`FocusedSection = A` preserved (pills stay bright)** |
| Section A textbox → ComboBoxItem in dropdown popup | `FocusedSection = null` (popup interaction loses focus signal) | `FocusedSection = A` preserved (logical-tree fallback bridges Popup boundary) |
| Section A textbox → tag pill (Focusable=False) | `FocusedSection = A` preserved (no LostKeyboardFocus fires) | unchanged |
| Section A textbox → preview pane | `FocusedSection = null` (correct — outside picker) | unchanged |
| Section A textbox → window chrome / outside app | `FocusedSection = null` | unchanged |
| SearchBox → tag pill | pill insert no-ops (FocusedSection is null) | pill insert routes to last-focused Section A |

The fix is purely additive — every prior PASS path remains PASS; the specific FAIL paths the user reported transition to PASS.

## 4. Risks (carried from r1 plan packet § 6, all LOW or INFO)

R1-R8 unchanged from r1 plan packet (validated by specialist with no HIGH/MEDIUM escalations).

## 5. Validation gates

### 5.1 Test gates

`tests/SunoMetatagApp.Tests/MainViewModelTests.cs` additions (forecast — final exact assertions land at T2):

| Test | Assertion | Result type |
|---|---|---|
| **F1** | `InsertTag` mutates `FocusedSection.Lyrics` when called with `FocusedSection` set via property assignment (invariant: VM doesn't require View focus state for insertion). | `[Fact]` |
| **F2** | `InsertTag` with `FocusedSection = null` is a no-op (regression-gate against accidentally enabling insertion without a target). | `[Fact]` |

Total test surface forecast for v1.10: **~127-128 green** (126 v1.9 baseline + 1-2 new).

### 5.2 USER REVIEW S1-S8 (with absorption #1 dropdown-pause)

| # | Scenario | Action | Expected | Critical? |
|---|---|---|---|---|
| S1 | Default-state v1.9-equivalence | Open exe; don't click anything. | Window opens; v1.9 5-column layout preserved; initial focus on first lyric textbox (focused-border highlighted); pills bright. | |
| S2 | Section A focus baseline | Click in any section's lyric textbox; type a few characters. | Section card border highlights; tag pills bright. | |
| S3 | **Pill click works after SearchBox focus** (**CRITICAL** — the user's reported bug) | While Section A textbox is focused, click in the **SearchBox** (right pane); type `verse`. Then click a `[Verse]` pill. | (a) Pills stay **bright** while SearchBox has focus; (b) clicking the pill inserts `[Verse]` into Section A's lyric at the prior caret position; (c) lyric textbox regains focus after insertion. | **YES** |
| S4 | **Category ComboBox + dropdown-pause** (**CRITICAL** — absorption #1 from Lead's PASS-WITH-NOTES) | While Section A textbox is focused, click the **Category ComboBox** → wait for dropdown to fully open → **pause 1-2 seconds with dropdown visible** (lets the deferred continuation potentially fire while ComboBoxItem holds focus) → select a category → wait for dropdown to close → click a pill. | (a) Pills stay **bright** throughout the entire ComboBox interaction including the 1-2 second pause; (b) post-selection, the pill inserts into Section A at the prior caret position. | **YES** |
| S5 | Preview pane → pills dim (regression-gate for v1.1 r2 contract) | While Section A textbox is focused, click into the left **preview pane** (the read-only PreviewText TextBox). | (a) Pills **dim** (FocusedSection cleared, as in v1.9 — the picker-pane guard does NOT apply here); (b) clicking a pill while dim is a silent no-op (v1.9 behavior preserved). | **YES (regression)** |
| S6 | Inter-section focus | Click into Section A's textbox; type; click into Section B's textbox; type. | Pills stay bright; focused-border affordance moves from Section A to Section B; tag inserts route to Section B at its caret. | |
| S7 | Tag insert end-to-end (regression-gate, v1.7) | Search for `kpop` in the tag picker. `[K-Pop]` surfaces. Click it. | Pill inserts `[K-Pop]` into the focused lyric; v1.7 search normalization preserved. | |
| S8 | Stacked syntax (regression-gate, v1.3) | Focus a lyric, insert `[Verse]`, then Shift+click `[Chorus]`. | Result: `[Verse | Chorus]` per v1.3 stacked-syntax merge algorithm. | |

**Load-bearing cases:**
- **S3** — the user's reported workflow (CRITICAL).
- **S4** — absorbs specialist Finding 1 hypothesis (CRITICAL) — validates the logical-tree fallback handles ComboBox dropdown Popup boundary.
- **S5** — regression-gate for v1.1 r2 defer-clear contract preservation (CRITICAL).
- **S7+S8** — regression-gates for v1.7 + v1.3 prior cycle behavior.

### 5.3 Rollback path

Two-commit revert: `git revert <T2 commit> <T1 commit>` returns to v1.9 closeout tip `1e623ce`. Tests return to 126/126.

## 6. Wiki update forecast

Closeout-only wiki updates at T7 (per CLAUDE.md wiki-update gate):

- **`[[focus-flip-stale-insert]]`** — extend with picker-pane exemption + v1.10 third-guard documentation. **Refresh `last_confirmed: 2026-05-27`, `review_due: 2026-11-27`** (absorption #4). Optionally extend "What would break if the defer-clear were removed" with a fourth named failure mode: **picker-pane-trap** (focus moves to picker → `FocusedSection` clears → pills dim → silent no-op until user clicks back to a lyric).
- **`[[sunometatag-inline-editor]]`** — append a short paragraph under "Focus tracking + caret restoration" documenting v1.10's third-guard semantics.
- **`[[sunometatag-app]]`** — title bump v1.9 → v1.10; new `## v1.9 → v1.10 (2026-05-27)` subsection.
- **`[[ai-plan-archive]]`** — Archive entry 23 prepended for the v1.10 r1 plan packet at T8 closeout.

`wiki_sync_status: PASS` forecast.

## 7. Pre-submission self-check

1. **What exact question does this milestone prove?** That `MainViewModel.FocusedSection` can be preserved when keyboard focus moves into a known-safe UI subtree (the tag picker pane, including ComboBox dropdown Popup descendants), restoring tag-pill clickability without forcing the user back into a lyric textbox — without regressing the v1.1 r2 stale-insert defenses.
2. **What exact code or data surface proves it?** (a) `MainWindow.xaml.cs` `LyricTextBox_LostKeyboardFocus` deferred continuation with the new `IsAncestorOf(TagPickerPane, focused)` guard; (b) `IsAncestorOf` helper walking visual + logical trees; (c) `MainWindow.xaml` `Grid Grid.Column="4" x:Name="TagPickerPane"`; (d) USER REVIEW S3 + S4 critical-case end-to-end on the published artifact; (e) USER REVIEW S5 regression-gate for the v1.1 r2 contract.
3. **What is the strongest allowed conclusion?** The user's reported workflow (focus lyric → click picker pane / SearchBox / ComboBox → click pill) lands the pill insert into the previously-focused lyric without requiring the user to click back into the textbox. v1.1 r2 stale-insert defenses preserved for non-picker-pane focus transitions.
4. **What remains unproven?** Edge cases where keyboard focus moves into picker-pane descendants that should arguably still clear `FocusedSection` (e.g., hypothetical future modal dialogs hosted in the picker subtree). No such surfaces exist currently.
5. **What would the reviewer reject?** A claim that this fix improves discoverability of the dim-affordance — it doesn't (the affordance now triggers in fewer cases). A claim that the fix introduces new affordances — it doesn't.

**Claim labels:**
- One-line `IsAncestorOf` guard mechanism = **Inference** (WPF VisualTreeHelper / LogicalTreeHelper behavior; well-understood).
- v1.1 r2 contract preservation = **Inference** (the new guard is purely additive).
- USER REVIEW S3 + S4 critical-case outcome = will be **Measured** at T6.
- USER REVIEW S5 regression-gate outcome = will be **Measured** at T6.
- Test count forecast = **Hypothesis**.
