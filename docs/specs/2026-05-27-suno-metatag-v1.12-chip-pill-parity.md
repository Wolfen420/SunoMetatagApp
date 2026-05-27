# SunoMetatagApp v1.12 — Chip-Pill Background/BorderBrush Color-State Parity with Add Section Button — Spec

**Date:** 2026-05-27
**Slice:** B-SUNO-010 / v1.12 (Low priority)
**Scope:** Token-driven WPF style edit. Zero behavioral change.

## 1. Problem

Chip pills in the tag-picker (`SunoTagPill` style) historically use the fuchsia accent token family (`SunoAccentPill*` — #D946EF base, #E879F9 hover, #C026D3 pressed) introduced at v1.2 per user direction (2026-05-26). The Add Section button (`SunoPrimaryButton` style) uses the brand-primary purple token family (`SunoAccentPrimary*` — #8B5CF6 base, #A78BFA hover, #3A3A48 pressed via `SunoBorderStrong`). Visually the two primary accent surfaces in the app live in different hue families.

User request (2026-05-27, B-SUNO-010 acceptance language): *"Chip pills use the same style token set and visual treatment as the Add Section button (base, hover, pressed, focus states), while preserving any chip-specific functional states."*

## 2. Brainstorm scope question (resolved 2026-05-27)

Three parity options were presented:

- **Full parity** — chips become rectangular (radius 6), body font size (13), and purple state colors.
- **Color-state parity only (Recommended)** — chips keep pill shape (radius 12) and small font (11) but swap state colors to purple primary chain. **User picked this.**
- **Brand-only parity** — swap base background only; keep hover/pressed in fuchsia family.

## 3. Scope (chosen path: Background/BorderBrush color-state parity)

### 3.1 In scope

Six token references on the `SunoTagPill` style swap from the `SunoAccentPill*` chain to the `SunoAccentPrimary*` / `SunoBorderStrong*` chain, matching `SunoPrimaryButton` line-for-line:

| Trigger | Property | Before (v1.11) | After (v1.12) |
|---|---|---|---|
| (base setter) | Background | `SunoAccentPillBrush` (#D946EF) | `SunoAccentPrimaryBrush` (#8B5CF6) |
| (base setter) | BorderBrush | `SunoAccentPillBrush` | `SunoAccentPrimaryBrush` |
| `IsMouseOver=True` | Background | `SunoAccentPillHoverBrush` (#E879F9) | `SunoAccentPrimaryTextBrush` (#A78BFA) |
| `IsMouseOver=True` | BorderBrush | `SunoAccentPillHoverBrush` | `SunoAccentPrimaryTextBrush` |
| `IsPressed=True` | Background | `SunoAccentPillPressedBrush` (#C026D3) | `SunoBorderStrongBrush` (#3A3A48) |
| `IsPressed=True` | BorderBrush | `SunoAccentPillPressedBrush` | `SunoBorderStrongBrush` |

A new `IsKeyboardFocused=True` trigger is added matching `SunoPrimaryButton`:

```xml
<Trigger Property="IsKeyboardFocused" Value="True">
    <Setter Property="BorderBrush" Value="{StaticResource SunoAccentPrimaryTextBrush}" />
    <Setter Property="BorderThickness" Value="{StaticResource SunoBorderThicknessFocused}" />
</Trigger>
```

Three orphan Color definitions plus their three SolidColorBrush definitions are deleted from `SunoTokens.xaml`:
- `SunoAccentPillColor` + `SunoAccentPillBrush`
- `SunoAccentPillHoverColor` + `SunoAccentPillHoverBrush`
- `SunoAccentPillPressedColor` + `SunoAccentPillPressedBrush`

Pre-T1 `grep` against `j:\SunoMetatagApp` confirmed zero external consumers of these six tokens — they are referenced only by `SunoTagPill` (which is being switched to the new chain in the same commit).

### 3.2 Out of scope (parity boundaries — intentionally divergent from `SunoPrimaryButton`)

These chip-specific functional states are **deliberately retained** vs the Add Section button's values:

| Property | `SunoTagPill` (v1.12) | `SunoPrimaryButton` | Why divergent |
|---|---|---|---|
| Foreground | `SunoTextOnPillBrush` (#FFFFFF pure white) | `SunoTextPrimaryBrush` (#E5E5EB near-white) | v1.10 user-feedback polish — chips need maximum contrast on accent backgrounds. White-on-purple ≈ 6.5:1 contrast (well above WCAG AA). |
| CornerRadius (in template) | `SunoRadiusPill` (12) | `SunoRadiusM` (6) | User explicitly chose "color-state parity only" — pill shape preserved for chip identity. |
| FontSize | `SunoFontSizeSmall` (11) | `SunoFontSizeBody` (13) | User explicitly chose color-state-only — small font preserves dense WrapPanel layout. |
| Margin | `2` | `0` (control default; XAML sets per-instance Margin) | Inter-chip spacing for WrapPanel; Add Section button is full-width and doesn't need inter-pill spacing. |
| Padding | `8,4` | `SunoPaddingControl` = `8,4` | Numerically identical — no divergence; both use 8 horizontal + 4 vertical. |

Other chip-specific functional states preserved via picker-scoped Style override in `MainWindow.xaml` ScrollViewer.Resources:
- `Focusable="False"` — load-bearing per [[focus-flip-stale-insert]] risk page.
- `DataTrigger` on `FocusedSection==null` — dim affordance (opacity + foreground swap to `SunoTextDisabledBrush`).
- Style-level ToolTip + AutomationProperties.HelpText for accessibility.
- Per-button local ToolTip="{Binding Description}" for per-tag descriptions.

### 3.3 Behavioral non-changes

- Click handlers untouched (`TagPickerButton_Click` in code-behind).
- `LyricTextBox_LostKeyboardFocus` defer-clear chain untouched (three-guard v1.10 contract intact).
- `TagService.Filter` untouched (v1.11 alphabetical ordering intact).
- `tags.json` + `prompts.json` byte-identical to v1.11 closeout tip `430c4d4`.
- No new tests; existing 130-test suite gates that no behavior regressed.

## 4. Implementation

Single file edits:

1. `src/SunoMetatagApp/Themes/SunoStyles.xaml` — `SunoTagPill` style block: replace 6 token references in Setters + Triggers; add new `IsKeyboardFocused` Trigger; rewrite explanatory comment.
2. `src/SunoMetatagApp/Themes/SunoTokens.xaml` — delete the three `SunoAccentPill*Color` lines (with their obsolete fuchsia comment) and the three matching `SolidColorBrush` definitions.

Plus two new doc files (this spec + companion plan doc).

## 5. Validation

USER REVIEW S1-S6 against `publish/SunoMetatagApp.exe`:

| # | Scenario |
|---|---|
| S1 | Base color parity — chip pills and Add Section button share the same purple (#8B5CF6) base. |
| S2 | Foreground legibility — white (#FFFFFF) on purple reads clearly. |
| S3 | **Hover + Pressed parity (explicit sub-checks per Lead absorption #2):** (a) hover both surfaces → lighter purple (#A78BFA); (b) press-and-hold both → very dark (#3A3A48), NOT deep fuchsia. |
| S4 | v1.10 picker-pane focus preservation regression-gate. |
| S5 | Picker dim affordance — gray-on-purple at 0.7 opacity still reads as dim. |
| S6 | v1.3 stacked syntax regression-gate. |

Test count forecast: **130/130 unchanged** (no new tests; visual-styling-only change).

Smoke: `dotnet run` EXIT=124 + `publish/SunoMetatagApp.exe` EXIT=124.

## 6. Rollback

Single-commit revert: `git revert <T1-sha>` returns chip pills to fuchsia + restores orphan tokens.

## 7. Related

- `[[sunometatag-visual-theme]]` — architecture page documenting token vocabulary + style key vocabulary; v1.12 wiki updates land at T7.
- `[[focus-flip-stale-insert]]` — risk page contract: picker-scoped `BasedOn="{StaticResource SunoTagPill}"` reference must resolve (the named key `SunoTagPill` remains unchanged in v1.12).
- `[[sunometatag-app]]` — feature page; title bump v1.11 → v1.12 at T7.
