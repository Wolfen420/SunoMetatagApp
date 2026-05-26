# Suno-Style Visual Redesign — Design (B-SUNO-002 / v1.2)

**Date:** 2026-05-26
**Status:** r1 — drafted in response to FRONTEND/UX `ADVISORY_NEEDS_REVISION` on the B-SUNO-002 plan packet (HIGH: missing artifacts; MEDIUM: dim opacity tuning, font fallback cleanup, namespace ratification; LOW items 5–10). MEDIUM + load-bearing LOW items absorbed into this spec.
**Owner:** Planner (Claude)
**Target repo:** `j:\SunoMetatagApp\`
**Builds on:** v1.1 (inline-tag-insertion model — shipped, `APPROVED (PASS-WITH-NOTES)` 2026-05-26)

**Layer:** **Visual only.** No changes to Models, Services, ViewModels, code-behind logic, tests, or `tags.json`. v1.1 functional behavior preserved 1:1.

---

## 0. Why this design

v1.1 ships functionally complete but visually generic — default WPF chrome reads as a desktop utility, not part of the Suno ecosystem. B-SUNO-002 establishes a coherent **dark-theme design language** via a token-driven `ResourceDictionary` system. Future visual evolution (light theme, custom dialogs, animations) consumes the same tokens.

**Inspiration sources** (publicly visible, no proprietary access claimed):
- `suno.com` — landing/app pages: dark surfaces, gradient purple/pink accents, generous spacing, sans-serif typography.
- `sunometatagcreator.com` — community tooling using similar visual vocabulary.

**Non-goals:** pixel-exact match to any reference screenshot; bundled brand assets; animations; custom Window chrome (frameless / acrylic); light theme (defers to B-002).

---

## 1. Overview

Two new `ResourceDictionary` files (`Themes/SunoTokens.xaml` + `Themes/SunoStyles.xaml`) wired into `App.xaml`'s `MergedDictionaries`. `MainWindow.xaml` strips inline chrome and consumes the token-driven styles. Zero code-behind / VM / model changes.

```
App.xaml
└── Application.Resources
    └── MergedDictionaries
        ├── Themes/SunoTokens.xaml   ← Colors, Brushes, FontFamilies, Sizes, Spacings, Radii, Thicknesses, Opacities
        └── Themes/SunoStyles.xaml   ← Implicit + named Styles consuming the tokens
```

---

## 2. Design tokens (canonical)

All token names are `x:Key`s in `SunoTokens.xaml`. Spec values are authoritative; implementation must match exactly unless §15 marks a value as specialist-tunable.

### 2.1 Color tokens (`Color` resources)

| Key | Value | Use |
|---|---|---|
| `SunoSurfaceBackgroundColor` | `#FF0F0F12` | App / window background |
| `SunoSurfaceCardColor` | `#FF1A1A22` | Section card, picker pane, preview pane |
| `SunoSurfaceCardElevatedColor` | `#FF22222C` | Hover / pressed surfaces, button defaults |
| `SunoBorderSubtleColor` | `#FF2A2A35` | Default control + card borders |
| `SunoBorderStrongColor` | `#FF3A3A48` | Hover / elevated borders, splitters |
| `SunoAccentPrimaryColor` | `#FF8B5CF6` | Focused borders, primary actions, accent backgrounds |
| `SunoAccentPrimaryTextColor` | `#FFA78BFA` | Accent **on text** (brighter variant, ~6:1 contrast over Card; specialist LOW item 5) |
| `SunoAccentPrimarySoftColor` | `#3A8B5CF6` | Soft tint backgrounds (alpha-prefixed = 0x3A ≈ 22%) |
| `SunoAccentSecondaryColor` | `#FFEC4899` | Optional secondary accent (currently unused; reserved for future gradient work) |
| `SunoTextPrimaryColor` | `#FFE5E5EB` | Body text |
| `SunoTextSecondaryColor` | `#FF9A9AA8` | Labels, placeholders, secondary metadata |
| `SunoTextDisabledColor` | `#FF5A5A66` | Disabled state foreground |
| `SunoErrorBgColor` | `#FF3A1A1A` | Error banner background |
| `SunoErrorTextColor` | `#FFFCA5A5` | Error banner foreground |

### 2.2 Brush tokens (`SolidColorBrush` resources)

One `SolidColorBrush` per color above, named `SunoSurfaceBackgroundBrush`, `SunoSurfaceCardBrush`, etc. WPF prefers `Brush` over `Color` in `Setter Value` so all `Style.Setters` resolve via brush keys.

### 2.3 Typography tokens

| Key | Value | Use |
|---|---|---|
| `SunoFontPrimary` | `Segoe UI Variable, Segoe UI, sans-serif` | All UI text. **Inter dropped** (not Windows-bundled; would fall through to sans-serif anyway; specialist MEDIUM item 3). Bundling Inter is a future B-NEW item. |
| `SunoFontMono` | `Cascadia Mono, Cascadia Code, Consolas, monospace` | Preview pane, lyric textbox |
| `SunoFontSizeSmall` | `11` | Move buttons, picker pill labels |
| `SunoFontSizeBody` | `13` | Lyric textbox, preview pane, controls |
| `SunoFontSizeLabel` | `12` | Search placeholder, secondary labels |
| `SunoFontSizeHeader` | `14` | (reserved; currently no headers in v1.2) |

### 2.4 Spacing + radius + thickness + opacity tokens

| Key | Value | Use |
|---|---|---|
| `SunoSpacingXS` | `4` | Tight inner gaps |
| `SunoSpacingS` | `8` | Default control gap, toolbar internal spacing |
| `SunoSpacingM` | `12` | Card inner padding |
| `SunoSpacingL` | `16` | Card outer margin, pane padding |
| `SunoSpacingXL` | `24` | Section between major surfaces |
| `SunoRadiusS` | `4` | Inputs (textboxes, combos) |
| `SunoRadiusM` | `6` | Buttons |
| `SunoRadiusL` | `8` | Cards (section, picker, preview) |
| `SunoRadiusPill` | `12` | Tag-picker pills |
| `SunoBorderThicknessThin` | `1` | Default control + card borders |
| `SunoBorderThicknessFocused` | `2` | Focused-section border, focused-input border (specialist LOW item 7: pinned as token so smoke can reference) |
| `SunoOpacityDim` | `0.7` | Tag-picker dim-when-no-focus (specialist MEDIUM item 2: **tuned from v1.1's 0.55 to 0.7** for dark-theme readability). Picker buttons also get `TextDisabled` foreground swap in the no-focus state to reinforce the signal — opacity alone isn't enough on dark. |

### 2.5 Token vocabulary rules

- All keys begin with `Suno*` prefix to avoid collision with WPF default resources or future third-party themes.
- Brush tokens drop the `Color` suffix from the underlying color (`SunoSurfaceCardColor` → `SunoSurfaceCardBrush`).
- All numeric tokens use bare units (no `px` suffix — WPF treats values as device-independent pixels).

---

## 3. Style key vocabulary (canonical)

`SunoStyles.xaml` exposes the following `x:Key`s. **Explicit key naming is required** (specialist LOW item 8) so picker-scoped styles can `BasedOn` resolve.

### 3.1 Named styles (`x:Key` + `TargetType`)

| Key | TargetType | Purpose |
|---|---|---|
| `SunoButton` | `Button` | App-level primary Button style. Picker-scoped Button styles must `BasedOn="{StaticResource SunoButton}"` to inherit Suno chrome while overriding `Focusable` + `Opacity`. |
| `SunoIconButton` | `Button` | Compact button for `▲ ▼ ×` toolbar actions. Smaller padding, no background by default. |
| `SunoPrimaryButton` | `Button` | "Copy all" + "+ Add section" — accent-treated. `BasedOn="{StaticResource SunoButton}"`. |
| `SunoTagPill` | `Button` | Tag-picker pill style. `BasedOn="{StaticResource SunoButton}"`. Sets `Focusable="False"` in the *picker-scoped* override (see §4.3); the named style here is the chrome only. |
| `SunoTextBox` | `TextBox` | Lyric textbox + search box (with property variants applied inline). |
| `SunoComboBox` | `ComboBox` | Category dropdown. |
| `SunoSectionCard` | `Border` | Section card chrome — `SurfaceCard` background, `BorderSubtle` border, `RadiusL` corners, `SpacingM` padding. Includes focused-state Trigger on `IsKeyboardFocusWithin` (load-bearing v1.1 mechanism preserved). |
| `SunoPickerCard` | `Border` | Picker pane chrome (reserved; currently the Grid is unstyled). |
| `SunoPreviewCard` | `Border` | Preview pane chrome — `SurfaceCard` background, `RadiusL` corners. |
| `SunoErrorBanner` | `Border` | Error banner — `ErrorBg` background, `ErrorText` foreground, `RadiusS` corners. |

### 3.2 Implicit styles (no `x:Key`)

| TargetType | Purpose |
|---|---|
| `Window` | App-level Window background = `SurfaceBackground`, foreground = `TextPrimary`. |
| `TextBlock` | App-level default — `TextPrimary` foreground, `FontPrimary` font. |
| `GridSplitter` | Subtle dark divider — `BorderStrong` background. |

**No implicit `Button` style.** Buttons must explicitly reference `SunoButton`, `SunoIconButton`, `SunoPrimaryButton`, or `SunoTagPill` via `Style="{StaticResource ...}"`. Rationale: prevents implicit cascading from breaking the picker-scoped Focusable/Opacity override (the v1.1 carry-over discipline — specialist mechanism check confirmed this approach).

---

## 4. Per-surface style rules

### 4.1 Section card (`SunoSectionCard`)

```
Default:
  Background  = SunoSurfaceCardBrush
  BorderBrush = SunoBorderSubtleBrush
  BorderThickness = SunoBorderThicknessThin
  CornerRadius = SunoRadiusL
  Padding = SunoSpacingM
  Margin (outer)  = 0, 0, 0, SunoSpacingS (bottom-only, between cards)

Focused (IsKeyboardFocusWithin = True):
  BorderBrush = SunoAccentPrimaryBrush
  BorderThickness = SunoBorderThicknessFocused
```

The `IsKeyboardFocusWithin` Trigger is the load-bearing carry-over from v1.1 (resolves the original DataTrigger-Value-Binding crash and is documented in `[[focus-flip-stale-insert]]`). **Do not change the mechanism**; only swap the visual tokens.

### 4.2 Section toolbar (`SunoIconButton` × 3)

`▲`, `▼`, `×` buttons in the section's top-right toolbar.

```
Default:
  Background = Transparent
  Foreground = SunoTextSecondaryBrush
  BorderBrush = Transparent
  Padding = SunoSpacingXS, 2
  FontSize = SunoFontSizeSmall
  MinWidth = 28

Hover:
  Background = SunoSurfaceCardElevatedBrush
  Foreground = SunoTextPrimaryBrush

Pressed:
  Background = SunoBorderSubtleBrush

Focused (keyboard):
  BorderBrush = SunoAccentPrimaryBrush
  BorderThickness = SunoBorderThicknessThin

Disabled (CanExecute = False at boundary):
  Foreground = SunoTextDisabledBrush
  Background = Transparent
  (Opacity unchanged; rely on color contrast instead)
```

The `×` button is `Click="DeleteSectionButton_Click"` (no Command); the `▲▼` are bound to `MoveSectionUp/DownCommand` with `CanExecute` predicates from v1.1 — those still drive `IsEnabled` automatically.

### 4.3 Lyric textbox (`SunoTextBox` variant for lyric use)

```
Default:
  Background = SunoSurfaceBackgroundBrush
  BorderBrush = SunoBorderSubtleBrush
  BorderThickness = SunoBorderThicknessThin
  CornerRadius (via TextBox.Template override) = SunoRadiusS
  Padding = SunoSpacingS
  Foreground = SunoTextPrimaryBrush
  FontFamily = SunoFontMono
  FontSize = SunoFontSizeBody
  CaretBrush = SunoAccentPrimaryBrush
  SelectionBrush = SunoAccentPrimarySoftBrush
  AcceptsReturn = True
  TextWrapping = Wrap
  MinLines = 6, MaxLines = 14
  VerticalScrollBarVisibility = Auto

Focused (per-control; section-level focused state is on the Border):
  BorderBrush = SunoAccentPrimaryBrush
  BorderThickness = SunoBorderThicknessFocused
```

The TextBox's own focused border + the parent SunoSectionCard's focused border are both `AccentPrimary`; they read as one continuous accent when the user clicks in. **Event hooks preserved verbatim from v1.1:** `GotKeyboardFocus="LyricTextBox_GotFocus"`, `LostKeyboardFocus="LyricTextBox_LostFocus"`, `SelectionChanged="LyricTextBox_SelectionChanged"`.

### 4.4 Tag picker pill (`SunoTagPill`, scoped via `BasedOn`)

App-level `SunoTagPill` defines the chrome:

```
Default:
  Background = SunoSurfaceCardElevatedBrush
  Foreground = SunoTextPrimaryBrush
  BorderBrush = SunoBorderSubtleBrush
  BorderThickness = SunoBorderThicknessThin
  Padding = SunoSpacingS, SunoSpacingXS
  CornerRadius = SunoRadiusPill
  FontFamily = SunoFontPrimary
  FontSize = SunoFontSizeSmall
  Margin = 2

Hover:
  Background = SunoAccentPrimarySoftBrush
  Foreground = SunoAccentPrimaryTextBrush   ← brighter variant; ~6:1 contrast over the soft tint
  BorderBrush = SunoAccentPrimaryBrush

Pressed:
  Background = SunoAccentPrimaryBrush
  Foreground = SunoTextPrimaryBrush          ← over the saturated accent fill, primary white-ish reads better than the accent text variant
```

The **picker-scoped override** lives in `MainWindow.xaml`'s `ScrollViewer.Resources`:

```xml
<ScrollViewer x:Name="TagPickerScroll">
    <ScrollViewer.Resources>
        <Style TargetType="Button" BasedOn="{StaticResource SunoTagPill}">
            <Setter Property="Focusable" Value="False" />
            <Setter Property="Opacity" Value="1.0" />
            <Style.Triggers>
                <DataTrigger Binding="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=DataContext.FocusedSection}" Value="{x:Null}">
                    <Setter Property="Opacity" Value="{StaticResource SunoOpacityDim}" />
                    <Setter Property="Foreground" Value="{StaticResource SunoTextDisabledBrush}" />
                    <Setter Property="ToolTip" Value="Click in a lyric textbox first, then click a tag." />
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </ScrollViewer.Resources>
    ...
</ScrollViewer>
```

The picker-scoped style **must** `BasedOn="{StaticResource SunoTagPill}"` (named, not implicit) so the chrome cascades while `Focusable="False"` and the dim affordance stay in this style's control. This is the load-bearing carry-over from v1.1's focus model (specialist LOW item 8).

**Dim affordance (specialist MEDIUM item 2):** opacity tuned from v1.1's `0.55` to `SunoOpacityDim = 0.7` for dark-theme readability, **plus** a foreground swap to `SunoTextDisabledBrush` to reinforce the disabled signal via color rather than opacity alone. Specialist's recommendation (c) — combine opacity + color swap — adopted here.

### 4.5 Preview pane (`SunoPreviewCard` + read-only `TextBox`)

```
Border (SunoPreviewCard):
  Background = SunoSurfaceCardBrush
  CornerRadius = SunoRadiusL
  Padding = SunoSpacingM

Inner TextBox:
  IsReadOnly = True
  Background = Transparent (inherits SunoPreviewCard background)
  Foreground = SunoTextPrimaryBrush
  FontFamily = SunoFontMono
  FontSize = SunoFontSizeBody
  Padding = 0
  BorderThickness = 0
  AcceptsReturn = True
  TextWrapping = Wrap
  VerticalScrollBarVisibility = Auto
  SelectionBrush = SunoAccentPrimarySoftBrush
```

### 4.6 Search box + category dropdown (`SunoTextBox` + `SunoComboBox`)

```
Default:
  Background = SunoSurfaceBackgroundBrush
  Foreground = SunoTextPrimaryBrush
  BorderBrush = SunoBorderSubtleBrush
  BorderThickness = SunoBorderThicknessThin
  Padding = SunoSpacingS, SunoSpacingXS
  FontFamily = SunoFontPrimary
  FontSize = SunoFontSizeBody
  CornerRadius = SunoRadiusS

Focused:
  BorderBrush = SunoAccentPrimaryBrush
  BorderThickness = SunoBorderThicknessFocused

Placeholder (search box only):
  Foreground = SunoTextSecondaryBrush
```

### 4.7 Error banner (`SunoErrorBanner`)

```
Background = SunoErrorBgBrush
BorderBrush = SunoErrorTextBrush
BorderThickness = SunoBorderThicknessThin
CornerRadius = SunoRadiusS
Padding = SunoSpacingS
Foreground (TextBlock child) = SunoErrorTextBrush
```

The "Copy" button inside the banner uses `SunoButton` style.

### 4.8 Primary action buttons (`SunoPrimaryButton`)

`Copy all` (left column) and `+ Add section` (middle column):

```
Default:
  Background = SunoAccentPrimaryBrush
  Foreground = SunoTextPrimaryBrush
  BorderBrush = SunoAccentPrimaryBrush
  BorderThickness = SunoBorderThicknessThin
  Padding = SunoSpacingS, SunoSpacingXS
  CornerRadius = SunoRadiusM
  FontFamily = SunoFontPrimary
  FontSize = SunoFontSizeBody

Hover:
  Background = SunoAccentPrimaryTextBrush   ← lighter accent variant for hover lift

Pressed:
  Background = SunoBorderStrongBrush
  Foreground = SunoAccentPrimaryTextBrush
```

### 4.9 GridSplitter

```
Background = SunoBorderStrongBrush
Width = 6
Cursor = SizeWE
HorizontalAlignment = Stretch, VerticalAlignment = Stretch
```

Carry-over `Width=6` from v1.1; only color swaps from `#DDD` to `SunoBorderStrongBrush`.

---

## 5. Per-state visual contract (canonical)

Every interactive control respects this state hierarchy:

| State | Indicator (priority order) |
|---|---|
| `Default` | Token-defined base values |
| `Hover` (`IsMouseOver=True`) | Background brighter (SurfaceCardElevated → BorderStrong) and/or accent tint |
| `Focused` (`IsKeyboardFocused=True`) | `AccentPrimary` border at `BorderThicknessFocused` |
| `Pressed` (`IsPressed=True`) | Background slightly darker than default; accent foreground intensified |
| `Disabled` (`IsEnabled=False`) | Foreground → `TextDisabled`; no opacity change |
| `Dim` (picker-scoped, `FocusedSection=Null`) | Opacity → `SunoOpacityDim` (0.7) **AND** Foreground → `TextDisabled` **AND** tooltip swap |

States compose via WPF's Style Triggers stacking; later-defined Triggers override earlier ones if their conditions are true.

---

## 6. Migration from v1.1

### 6.1 `MainWindow.xaml` edits (concrete)

| v1.1 markup | v1.2 markup |
|---|---|
| `<Border BorderBrush="#CCC" BorderThickness="1" CornerRadius="4" Margin="0,0,0,8" Padding="6" Background="White">` (section card root) | `<Border Style="{StaticResource SunoSectionCard}">` (CornerRadius / Margin / Padding moved into the style) |
| `<TextBox Background="#FAFAFA" ... FontFamily="Consolas" FontSize="12" Padding="6">` (preview pane) | `<Border Style="{StaticResource SunoPreviewCard}"><TextBox .../></Border>` (chrome moves to Border) |
| `<Border Background="#FFE5E5" BorderBrush="#C00" BorderThickness="1" Padding="6" Margin="0,0,0,6">` (error banner) | `<Border Style="{StaticResource SunoErrorBanner}">` |
| `<Button Content="Copy all" Padding="8,4" Margin="0,0,0,6">` | `<Button Style="{StaticResource SunoPrimaryButton}" Content="Copy all" Margin="0,0,0,6">` (Padding moves to style) |
| `<Button Content="+ Add section" Margin="0,8,0,0" Padding="8,4">` | `<Button Style="{StaticResource SunoPrimaryButton}" Content="+ Add section" Margin="0,8,0,0">` |
| `<Button Content="▲" Padding="6,0" Margin="0,0,3,0" FontSize="11">` | `<Button Style="{StaticResource SunoIconButton}" Content="▲" Margin="0,0,3,0">` (Padding + FontSize move to style) |
| `<Button Content="×" Padding="8,0" FontWeight="Bold">` | `<Button Style="{StaticResource SunoIconButton}" Content="×" FontWeight="Bold">` (FontWeight stays inline; not part of token system) |
| `<Border BorderBrush="#EEE" BorderThickness="1" Background="#F8F8F8" Padding="3" MinHeight="36">` | **Deleted** — this was the v1 chip-row container, already removed in v1.1; v1.2 has no chip row. |
| `<GridSplitter ... Background="#DDD">` (×2) | `<GridSplitter ...>` (implicit `GridSplitter` style supplies `BorderStrongBrush`) |
| Tag-picker `<Style TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">` in `ScrollViewer.Resources` (v1.1 default-Button-based) | `<Style TargetType="Button" BasedOn="{StaticResource SunoTagPill}">` — **named** BasedOn (specialist LOW item 8). Triggers (Focusable=False, Opacity dim via `SunoOpacityDim`, Foreground swap, tooltip swap) preserved with token values. |
| `<TextBox Text="{Binding Lyrics, ...}" ... FontFamily="Consolas" FontSize="13" Padding="6">` (lyric textbox) | `<TextBox Style="{StaticResource SunoLyricTextBox}" Text="{Binding Lyrics, ...}">` where `SunoLyricTextBox` extends `SunoTextBox` with `MinLines=6 MaxLines=14`. |
| Search box `<TextBox x:Name="SearchBox" ... Padding="4">` | `<TextBox x:Name="SearchBox" Style="{StaticResource SunoTextBox}" Text="{Binding SearchText, ...}">` |
| `<ComboBox ... Padding="4">` (category) | `<ComboBox Style="{StaticResource SunoComboBox}" ...>` |

### 6.2 What does NOT change

- Every `x:Name` value (e.g., `SectionsHost`, `SearchBox`, `TagPickerScroll`).
- Every event handler attribute (e.g., `GotKeyboardFocus="LyricTextBox_GotFocus"`).
- Every `Binding` expression.
- Every `Command="..."` / `CommandParameter="..."` value.
- Every `DataTemplate` structure (only chrome inside changes).
- `Window` `Title`, `Width`, `Height`, `MinWidth`, `MinHeight`, `WindowStartupLocation` attributes.

If any of those change, the v1.1 behavioral carry-over is broken and the slice has scope-crept.

---

## 7. Out of scope (explicit)

- **B-002 light theme toggle** — defers.
- **Custom dark-themed delete-confirm dialog** — defers as **B-026** (specialist LOW item 6). Native WPF `MessageBox` still appears; it uses Windows OS chrome (light or dark per OS theme). Acceptable v1.2 visual gap.
- **Custom `ScrollBar` template for dark theme** — defers as **B-027** (specialist LOW item 6). Default WPF `ScrollBar` chrome stays. Acceptable v1.2 visual gap.
- **Inter font bundling** — Inter dropped from `FontPrimary` chain entirely (specialist MEDIUM item 3). Future B-NEW if Suno-brand consistency requires it.
- **B-024 inline `[Tag]` syntax highlighting** — unchanged from v1.1 closeout; still requires `RichTextBox`.
- **Animations / transitions** beyond default WPF state changes — out.
- **Custom `Window` chrome** (frameless / acrylic / title bar overlay) — out.
- **Branding assets** (app icon, logo, splash) — out.
- **Accessibility audit (B-014)** — out; best-effort contrast tuning only.
- **Right-click context menus** anywhere — out.

---

## 8. Testing strategy

**Unit tests:** 31/31 carry-over from v1.1 unchanged. **Zero** test edits expected. If any test changes, scope has crept into logic.

**Smoke matrix (USER REVIEW gated on published exe):**

### Combined "focus-required interaction model on dark theme" (single combined case — specialist LOW item 9)

Click into a lyric textbox, then click into the search textbox. Verify **all three** of:
(a) the focused-section accent border returns to subtle dark (no accent),
(b) tag-picker pills dim to `SunoOpacityDim` (0.7) **and** swap foreground to `SunoTextDisabledBrush`,
(c) hovering any picker pill shows tooltip "Click in a lyric textbox first, then click a tag."

This combined check replaces the v1.1 fragmented cases 5 + 9 to validate the discoverability story under dark theme as a coherent signal.

### Carry-over v1.1 behavioral cases (zero-regression)

All 6 v1.1 smoke cases (initial focus + move-boundary disable; inline insertion at caret; selection replacement; focus-loss → dim + no-op; multi-section preview + Copy all; rapid-fire 3-tag clicks) must PASS unchanged. The dark theme must not break any v1.1 behavior.

### Visual token-application cases

| # | Surface | Acceptance |
|---|---|---|
| V1 | Launch | App opens; no `XamlParseException`; dark surface visible |
| V2 | Color palette | No white/`#FAFAFA`/`#F8F8F8`/`#FFE5E5` backgrounds anywhere; all light text on dark surface |
| V3 | Typography | UI in Segoe UI Variable; lyric textbox + preview in Cascadia Mono / Cascadia Code / Consolas |
| V4 | Section card | Rounded `RadiusL` corners; subtle border; `SpacingM` padding visible |
| V5 | Focused-section accent | Clicking into a lyric textbox swaps the section border to `SunoAccentPrimary` at `SunoBorderThicknessFocused` (2px); intensity feels balanced — not visually overpowering (specialist LOW item 7 explicit assessment) |
| V6 | Tag-picker pills | Pill-shaped (`RadiusPill` 12px corners); subtle dark default; accent-tint on hover; brighter accent foreground (`SunoAccentPrimaryText`) on hover |
| V7 | Preview pane | Dark `SurfaceCard` background; monospace text; clean Copy-all button (primary style) |
| V8 | Search + category | Dark surfaces; `SunoTextSecondary` placeholder; focused border `AccentPrimary` |
| V9 | Error banner | (Optional — corrupt `tags.json` test) `SunoErrorBg` background, `SunoErrorText` foreground; no longer bright pink/red |
| V10 | GridSplitter | Subtle dark divider (`BorderStrong`); drag still works |
| V11 | Move boundary buttons | `▲` greyed at index 0; `▼` greyed at last index; greyed visual is `TextDisabled` foreground (not opacity dim) |
| V12 | (Combined focus-required interaction case — see above) | All three signals present after focus moves to search box |

---

## 9. Open design questions (for r1 review)

- Should `SunoIconButton` show a subtle background on default state (e.g., `SunoSurfaceCardElevatedBrush`) for slight depth, or stay transparent-by-default as proposed (cleaner)?
- Should the "Copy all" + "+ Add section" buttons use **filled accent** (current proposal) or **outlined accent** (transparent background + `AccentPrimary` border + `AccentPrimaryText` foreground)? Filled reads as primary; outlined feels lighter.
- Focused-border thickness pinned at 2 (`SunoBorderThicknessFocused`); specialist LOW item 7 suggested 1.5 as alternative. WPF supports fractional `Thickness` but default DPI rendering at 2 reads crisper. Defaulting to 2; specialist may push for 1.5 during r1 review.
- Tag-pill hover background `SunoAccentPrimarySoftBrush` (~22% alpha violet). Visible against the dark card? Specialist may want it more saturated (e.g., 35–40% alpha) for clearer hover signal.

---

## 10. Closes / Defers (backlog impact)

**Closes:** B-SUNO-002 (or remapped to B-025 per Lead namespace decision).

**Adds at closeout** (specialist LOW item 6):
- **B-026** — Custom dark-themed delete-confirm dialog (replaces native `MessageBox`). Priority: low. Triggered if dark/light contrast jarring becomes a user complaint.
- **B-027** — Dark-themed `ScrollBar` template. Priority: low. Same trigger.

**No supersession** of any existing wiki page. v1.1 architecture (`sunometatag-inline-editor`) describes the behavioral layer; v1.2 visual is orthogonal.

---

## 11. Migration risk

The single load-bearing carry-over is the tag-picker-scoped `Style TargetType="Button" BasedOn="..."` that provides `Focusable="False"` and the dim affordance. v1.1 used `BasedOn="{StaticResource {x:Type Button}}"` (default Button); v1.2 uses `BasedOn="{StaticResource SunoTagPill}"` (named — specialist LOW item 8).

If `SunoTagPill` doesn't exist as a named `x:Key` in `SunoStyles.xaml`, the `BasedOn` reference fails silently at XAML load and the picker buttons become focusable — which breaks v1.1's focus model and reintroduces the stale-insert risk documented in [[focus-flip-stale-insert]].

**Mitigation:** `SunoStyles.xaml` defines `SunoTagPill` with both `x:Key="SunoTagPill"` **and** the implicit-style attributes (so it can also be referenced via `TargetType` lookup if someone forgets the `Style="..."` reference). Verified by smoke case V6 (pill chrome present) + the v1.1 carry-over smoke case 4 (focus-loss → dim affordance).

---

## 12. Spec change log

- 2026-05-26 r1 — Initial spec, drafted in response to FRONTEND/UX `ADVISORY_NEEDS_REVISION` on the B-SUNO-002 plan packet. Absorbs HIGH (artifact-existence), MEDIUM (dim opacity 0.55→0.7 + token-swap, drop Inter, namespace deferred to Lead), and load-bearing LOW (5: AccentPrimaryText variant; 6: B-026 + B-027 seeds; 7: focused thickness token; 8: explicit x:Key naming; 9: combined focus-required case; 10: §7 evidence table restoration in packet).
