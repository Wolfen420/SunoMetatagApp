# Suno-Style Visual Redesign — Implementation Plan (B-SUNO-002 / v1.2)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the Suno-style visual redesign to SunoMetatagApp v1.1 via two new `ResourceDictionary` files + minimal `MainWindow.xaml` edits. Visual-only — zero changes to Models, Services, ViewModels, code-behind logic, tests, or `tags.json`. Test suite stays at 31/31; all 6 v1.1 smoke cases pass on the restyled app.

**Reference spec:** [`docs/specs/2026-05-26-suno-visual-redesign.md`](../specs/2026-05-26-suno-visual-redesign.md) (r1).

**Architecture:** Token-driven WPF theming. `Themes/SunoTokens.xaml` holds `Color` + `SolidColorBrush` + `FontFamily` + spacing/radius/thickness/opacity tokens. `Themes/SunoStyles.xaml` holds implicit and named `Style`s consuming the tokens. `App.xaml` merges both via `Application.Resources.MergedDictionaries`. `MainWindow.xaml` strips inline chrome and references named styles.

**Tech Stack:** Unchanged — WPF, .NET 8, C# 12, CommunityToolkit.Mvvm, System.Text.Json, xUnit.

**Supersedes:** None. v1.1 spec + plan remain accurate for the behavioral layer; v1.2 is an orthogonal visual layer.

**Prerequisites:** v1.1 shipped and on `main` of `j:\SunoMetatagApp\` (last commit `d5f6fc8`). .NET 8 SDK present.

---

## Notes for the implementer

- **All commands assume CWD = `j:\SunoMetatagApp\`** unless stated otherwise.
- **Commit style:** Conventional commits (`feat:`, `refactor:`, `chore:`, `docs:`).
- **One commit per task** unless a task says otherwise.
- **Zero test edits.** Any test edit means scope has crept into logic — stop and re-scope.
- **`x:Key` discipline (load-bearing):** `SunoStyles.xaml` must define `SunoTagPill` with an explicit `x:Key="SunoTagPill"` *and* `TargetType="Button"`. The picker-scoped override in `MainWindow.xaml` references it via `BasedOn="{StaticResource SunoTagPill}"`. If the key is missing or misspelled, the picker loses `Focusable="False"` silently and the v1.1 focus model breaks (re-introducing the focus-flip-stale-insert risk). Smoke case V6 + v1.1 carry-over case 4 are the canaries.
- **Smoke-launch before USER REVIEW** (v1.1 closeout lesson): plan T6 runs `dotnet run --project src/SunoMetatagApp` to catch any parse-time XAML defect before going to user gate. Compile-clean + test-clean XAML can still crash at `Window.Show()`.

---

## Test count: 31 (unchanged from v1.1)

If `dotnet test` returns any number other than `Passed: 31 Failed: 0` after T5, **stop and investigate** — this slice should not change test count.

Smoke matrix: **17 cases total** = 6 v1.1 carry-over regression + 1 combined focus-required interaction model on dark theme + 10 visual token-application (V1–V11) + 1 optional error-banner (V9 conditional on user-initiated corrupt-tags test).

---

## Task 0 — Pre-flight (no commit)

**Goal:** Confirm v1.1 baseline is green before visual work begins.

- [ ] `git status` — clean working tree.
- [ ] `git log --oneline -n 10` — verify last commit is `d5f6fc8` (v1.1 closeout LOW notes) per `ai/EXECUTION_LOG.md`.
- [ ] `dotnet build` — green.
- [ ] `dotnet test` — 31/31 passing.

**Exit criteria:** v1.1 baseline confirmed.

---

## Task 1 — `Themes/SunoTokens.xaml` (`feat: add Suno design tokens`)

**Goal:** Create the canonical token `ResourceDictionary`.

- [ ] Create directory `src/SunoMetatagApp/Themes/`.
- [ ] Create `src/SunoMetatagApp/Themes/SunoTokens.xaml` with full token table per spec §2:
  - Section 2.1: 14 `Color` resources (`SunoSurfaceBackgroundColor`, `SunoSurfaceCardColor`, `SunoSurfaceCardElevatedColor`, `SunoBorderSubtleColor`, `SunoBorderStrongColor`, `SunoAccentPrimaryColor`, `SunoAccentPrimaryTextColor`, `SunoAccentPrimarySoftColor`, `SunoAccentSecondaryColor`, `SunoTextPrimaryColor`, `SunoTextSecondaryColor`, `SunoTextDisabledColor`, `SunoErrorBgColor`, `SunoErrorTextColor`).
  - Section 2.2: one `SolidColorBrush` per `Color`, named `Suno*Brush` (drop the `Color` suffix from the source color name).
  - Section 2.3: 6 typography tokens (`SunoFontPrimary`, `SunoFontMono`, `SunoFontSizeSmall`, `SunoFontSizeBody`, `SunoFontSizeLabel`, `SunoFontSizeHeader`). Font families are `FontFamily` resources; sizes are `system:Double` resources (requires `xmlns:system="clr-namespace:System;assembly=mscorlib"`).
  - Section 2.4: spacing, radius, thickness, opacity tokens. `Thickness` and `CornerRadius` use their respective WPF types. Opacity is `system:Double`.
- [ ] Ensure the file's root element is `<ResourceDictionary xmlns="..." xmlns:x="..." xmlns:system="clr-namespace:System;assembly=mscorlib">`.
- [ ] No build verification yet — file isn't referenced.
- [ ] Commit:
  ```
  git commit -m "feat: add SunoTokens.xaml design-token resource dictionary"
  ```

**Exit criteria:** File exists with the full token set per spec §2.

---

## Task 2 — `Themes/SunoStyles.xaml` (`feat: add Suno style resource dictionary`)

**Goal:** Create the canonical style `ResourceDictionary` consuming tokens.

- [ ] Create `src/SunoMetatagApp/Themes/SunoStyles.xaml` with implicit + named styles per spec §3:
  - **Header:** `<ResourceDictionary xmlns="..." xmlns:x="..."><ResourceDictionary.MergedDictionaries><ResourceDictionary Source="SunoTokens.xaml" /></ResourceDictionary.MergedDictionaries>...` so styles can `StaticResource` token keys.
  - **Implicit styles** (no `x:Key`): `Window`, `TextBlock`, `GridSplitter` per spec §3.2.
  - **Named styles** (`x:Key` + `TargetType`): `SunoButton`, `SunoIconButton`, `SunoPrimaryButton`, `SunoTagPill`, `SunoTextBox`, `SunoLyricTextBox` (extends `SunoTextBox` with MinLines/MaxLines), `SunoComboBox`, `SunoSectionCard` (Border style with `IsKeyboardFocusWithin` Trigger swapping `BorderBrush` to `SunoAccentPrimaryBrush` + `BorderThickness` to `SunoBorderThicknessFocused`), `SunoPreviewCard`, `SunoErrorBanner` per spec §4.
- [ ] Each named style per spec §4 includes the per-state Triggers (default / hover / pressed / focused / disabled) per spec §5.
- [ ] **Critical:** `SunoTagPill` must have both `x:Key="SunoTagPill"` AND `TargetType="Button"` (spec §11 migration risk). Verify by grep:
  - `grep -F 'x:Key="SunoTagPill"' src/SunoMetatagApp/Themes/SunoStyles.xaml` returns 1 line.
- [ ] No build verification yet — file isn't wired into App.xaml.
- [ ] Commit:
  ```
  git commit -m "feat: add SunoStyles.xaml with implicit and named styles consuming tokens"
  ```

**Exit criteria:** File exists with named styles per spec §3.1 keyed exactly as listed.

---

## Task 3 — Wire ResourceDictionaries into `App.xaml` (`feat: merge SunoStyles into application resources`)

**Goal:** App-level merge so all child windows / controls pick up the theme.

- [ ] Edit `src/SunoMetatagApp/App.xaml`:
  ```xml
  <Application.Resources>
      <ResourceDictionary>
          <ResourceDictionary.MergedDictionaries>
              <ResourceDictionary Source="Themes/SunoStyles.xaml" />
          </ResourceDictionary.MergedDictionaries>
      </ResourceDictionary>
  </Application.Resources>
  ```
  (`SunoStyles.xaml` already merges `SunoTokens.xaml` per Task 2, so we only need to reference SunoStyles here.)
- [ ] `dotnet build` — must succeed (XAML compile-time validation of token references).
- [ ] Verify Tokens/Styles xaml files are included as Build Action = `Page` in the csproj (SDK-style csproj globs `.xaml` under the project root as `Page` by default; if not, add explicit `<Page Include="Themes\SunoTokens.xaml" />` and `<Page Include="Themes\SunoStyles.xaml" />`).
- [ ] Commit:
  ```
  git commit -m "feat: wire SunoStyles ResourceDictionary into App.xaml"
  ```

**Exit criteria:** Build green; the new files are part of the assembly.

---

## Task 4 — Restyle `MainWindow.xaml` (`refactor: apply Suno styles to MainWindow XAML`)

**Goal:** Strip inline chrome from MainWindow.xaml and reference named styles per spec §6.1.

- [ ] Edit `src/SunoMetatagApp/MainWindow.xaml`:
  - **Window.Resources:** keep `NullToCollapsedConverter` and `StringIsNotEmptyConverter` declarations.
  - **Left column (preview pane):**
    - `Button Content="Copy all"` → add `Style="{StaticResource SunoPrimaryButton}"`, remove inline `Padding`.
    - Preview pane: wrap the existing `TextBox` in a `<Border Style="{StaticResource SunoPreviewCard}">`; strip the `TextBox`'s inline `Background`, `FontFamily`, `FontSize`, `Padding` (now provided by Border + implicit/named style).
  - **GridSplitter (×2):** remove inline `Background="#DDD"`; implicit `GridSplitter` style supplies `SunoBorderStrongBrush`.
  - **Middle column (section stack):**
    - `+ Add section` Button: add `Style="{StaticResource SunoPrimaryButton}"`, remove inline `Padding`.
    - Section `DataTemplate`:
      - Replace `<Border BorderBrush="#CCC" BorderThickness="1" CornerRadius="4" Margin="0,0,0,8" Padding="6" Background="White">` with `<Border Style="{StaticResource SunoSectionCard}">`.
      - **Delete** the v1.1 inline `<Border.Style>` block (the `IsKeyboardFocusWithin` Trigger is now part of `SunoSectionCard`).
      - Toolbar buttons (`▲ ▼ ×`): add `Style="{StaticResource SunoIconButton}"` to each; remove inline `Padding`, `Margin`, `FontSize`. (The `×` button stays `Click="DeleteSectionButton_Click"` with `CommandParameter="{Binding}"`; the `▲▼` stay `Command="..."` with `CommandParameter="{Binding}"`. `FontWeight="Bold"` on the `×` can stay inline if desired.)
      - Lyric `TextBox`: add `Style="{StaticResource SunoLyricTextBox}"`; remove inline `AcceptsReturn`, `TextWrapping`, `MinLines`, `MaxLines`, `VerticalScrollBarVisibility`, `FontFamily`, `FontSize`, `Padding`. **Keep verbatim**: `Text="{Binding Lyrics, UpdateSourceTrigger=PropertyChanged}"`, `GotKeyboardFocus="LyricTextBox_GotFocus"`, `LostKeyboardFocus="LyricTextBox_LostFocus"`, `SelectionChanged="LyricTextBox_SelectionChanged"`.
  - **Right column (tag picker):**
    - Error banner Border: replace inline `Background="#FFE5E5" BorderBrush="#C00" BorderThickness="1" Padding="6" Margin="0,0,0,6"` with `Style="{StaticResource SunoErrorBanner}"` + keep `Margin`. Keep the inline `Visibility="{Binding LoadError, Converter=...}"`.
    - Error banner inner `<TextBlock>`: drop `Foreground="#900"` (implicit `TextBlock` style provides `SunoErrorTextBrush`? No — that's for the Border. Add inline `Foreground="{StaticResource SunoErrorTextBrush}"` or let the implicit cascade if `SunoErrorBanner` style sets a `TextElement.Foreground` setter for descendants).
    - Error banner Copy button: add `Style="{StaticResource SunoButton}"`, remove inline `Padding`.
    - Search box `TextBox`: add `Style="{StaticResource SunoTextBox}"`, remove inline `Margin`, `Padding`.
    - Placeholder `<TextBlock>`: drop inline `Foreground="#999"`; add `Foreground="{StaticResource SunoTextSecondaryBrush}"`.
    - Category `<ComboBox>`: add `Style="{StaticResource SunoComboBox}"`, remove inline `Margin`, `Padding`.
    - Tag picker `ScrollViewer.Resources`: replace v1.1's `<Style TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">` with `<Style TargetType="Button" BasedOn="{StaticResource SunoTagPill}">`. **Preserve verbatim** the `Setter Property="Focusable" Value="False"` + the `DataTrigger Binding="...DataContext.FocusedSection" Value="{x:Null}"` block. **Update Triggers**: change `Opacity` setter from `0.55` to `{StaticResource SunoOpacityDim}`, **add** `Setter Property="Foreground" Value="{StaticResource SunoTextDisabledBrush}"`, keep tooltip swap setter.
    - Tag-picker `ItemsControl > DataTemplate > Button`: drop inline `Margin="2"` and `Padding="6,2"` (now provided by `SunoTagPill`). **Keep verbatim** `Content="{Binding Bracket}"`, `ToolTip="{Binding Description}"`, `ToolTipService.IsEnabled`, `Command`, `CommandParameter`.
    - "No tags match" `<TextBlock>`: drop inline `Foreground="#666"`; add `Foreground="{StaticResource SunoTextSecondaryBrush}"`.
- [ ] `dotnet build` — green.
- [ ] Commit:
  ```
  git commit -m "refactor: apply Suno styles to MainWindow.xaml; strip inline chrome"
  ```

**Exit criteria:** XAML compiles; v1.1 event handlers + bindings + Focusable=False discipline preserved verbatim.

---

## Task 5 — Build + test regression (`feat: confirm zero-regression`)

No commit.

- [ ] `dotnet build` — green.
- [ ] `dotnet test` — **31/31** passing. If any other number: **stop**, investigate, do not proceed.

**Exit criteria:** Test suite identical to v1.1 baseline.

---

## Task 6 — Smoke-launch dev exe (`feat: dev launch verification`)

**Goal:** Catch parse-time XAML defects before publish (lesson from v1.1 closeout).

No commit.

- [ ] `timeout 5 dotnet run --project src/SunoMetatagApp --no-build 2>&1 | head -40; echo "--exit code: $?"` (or equivalent on the host shell).
- [ ] If output is empty or shows only standard launch messages and exit code 0 (timeout-killed): **success**, the window launched and ran until timeout-killed.
- [ ] If output shows `XamlParseException` or any other exception with stack trace: **investigate**:
  - Common causes:
    - `BasedOn="{StaticResource SunoTagPill}"` not resolving — verify `x:Key="SunoTagPill"` exists in `SunoStyles.xaml`.
    - Token reference typo (`SunoSurfaceCardBrush` vs `SunoSurfaceCardBackgroundBrush`).
    - `Color` vs `Brush` confusion in a `Setter Value`.
    - `DataTrigger Value="{Binding}"` re-introduced anywhere (the v1.1 closeout defect class).
  - Fix and re-run.

**Exit criteria:** App launches without exception.

---

## Task 7 — Publish single-file exe (`chore: publish v1.2`)

```
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

- [ ] Verify `publish/SunoMetatagApp.exe` exists (~147 MB ± a few MB).
- [ ] Verify `publish/tags.json` exists.
- [ ] **Smoke-launch the published exe** before USER REVIEW (carrying the v1.1 lesson): run `publish/SunoMetatagApp.exe`, confirm window appears with dark theme, close. If launch fails, diagnose via dev run (Task 6 commands).
- [ ] No commit (publish artifacts are gitignored).

**Exit criteria:** Published exe launches and shows dark theme.

---

## Task 8 — Manual smoke matrix on published exe (USER REVIEW NEEDED)

Run `publish/SunoMetatagApp.exe`. Verify the 17-case matrix per spec §8.

### Combined "focus-required interaction model on dark theme" case (specialist LOW item 9)

Click into a lyric textbox, then click into the search textbox. Verify **all three** of:
- (a) the focused-section accent border returns to subtle dark (no `SunoAccentPrimary`);
- (b) tag-picker pills dim to ~0.7 opacity AND foreground swaps to `SunoTextDisabled` (the muted gray);
- (c) hovering any picker pill shows tooltip "Click in a lyric textbox first, then click a tag."

All three signals must be present simultaneously for PASS.

### Carry-over v1.1 behavioral cases (zero-regression — all 6 must PASS)

| # | v1.1 Case | Expected on v1.2 |
|---|---|---|
| 1 | Initial focus + focused border + move-boundary disable | Border now `SunoAccentPrimary` instead of v1.1 SteelBlue; `▲` greyed at top, `▼` greyed at bottom |
| 2 | Inline insertion at caret | `Walking down the[Guitar] street` — unchanged |
| 3 | Selection replacement | `Walking down [Powerful] street` — unchanged |
| 4 | Focus-loss → dim affordance + no-op | Combined case above |
| 5 | Multi-section preview + Copy all | Both sections, blank-line separator, clipboard works |
| 6 | Rapid-fire 3-tag clicks | `A[Guitar][Drums][Powerful]B`, caret after third `]` |

### Visual token-application cases (V1–V11 per spec §8)

| # | Surface | Expected |
|---|---|---|
| V1 | Launch | App opens; no exception; dark surface visible |
| V2 | Color palette | No `#FFF`/`#FAFAFA`/`#F8F8F8`/`#FFE5E5` anywhere; all light text on dark |
| V3 | Typography | UI in Segoe UI Variable; lyric + preview in Cascadia Mono / Cascadia Code / Consolas |
| V4 | Section card | `RadiusL` (8px) rounded; subtle dark border; comfortable padding |
| V5 | Focused-section accent | Border → `SunoAccentPrimary` at 2px on focus; intensity feels balanced, not visually loud (specialist LOW item 7) |
| V6 | Tag-picker pills | Pill-shaped (`RadiusPill` 12px); subtle dark default; accent-tint on hover; brighter accent foreground on hover |
| V7 | Preview pane | `SunoSurfaceCard` background; monospace text; clean primary-style Copy-all button |
| V8 | Search + category | Dark surfaces; `SunoTextSecondary` placeholder; focused border `AccentPrimary` |
| V9 | Error banner | (Optional, user-initiated: rename `tags.json` to `tags.json.bak` next to the exe, relaunch.) `SunoErrorBg` background, `SunoErrorText` foreground; no longer bright pink/red. Restore `tags.json` afterward. |
| V10 | GridSplitter | Subtle dark divider (`BorderStrong`); drag still works |
| V11 | Move boundary buttons | `▲` greyed at index 0; `▼` greyed at last index; greyed visual is `TextDisabled` foreground (not opacity dim) |

**Result format:** PASS/FAIL per case + free-form note on overall impression + any specific surface that feels wrong.

---

## Task 9 — Update wiki pages (`docs: refresh wiki pages for v1.2 visual`)

In `j:\SunoSongSetup\.SunoSongSetup-wiki\wiki\`:

- [ ] **NEW:** `architecture/sunometatag-visual-theme.md`:
  - `type: architecture`, `status: active`, `claim_state: active`.
  - `related: [[sunometatag-app]], [[sunometatag-inline-editor]], [[suno-visual-language]]`.
  - Body: documents `SunoTokens.xaml` + `SunoStyles.xaml` separation, full token table (mirrors spec §2), style key vocabulary (mirrors spec §3), how to add a new themed surface, how to swap themes in a future B-002 light-theme work.
- [ ] **NEW:** `decisions/suno-visual-language.md`:
  - `type: decision`, `status: active`, `claim_state: active`.
  - `related: [[sunometatag-visual-theme]], [[sunometatag-app]]`.
  - Body: *why* dark + accent-purple, inspiration sources (publicly visible Suno surfaces), explicit non-goals (no pixel parity, no animations, no custom Window chrome, no light toggle), decision to ship dark-only deferring B-002.
- [ ] **UPDATE:** `features/sunometatag-app.md`:
  - Add "v1.1 → v1.2 (visual redesign)" subsection describing the model change.
  - Update `related:` to include `[[sunometatag-visual-theme]]` and `[[suno-visual-language]]`.
  - Bump `last_confirmed: 2026-05-26`.
- [ ] **UPDATE:** `architecture/sunometatag-inline-editor.md`:
  - Add a "Visual layer" pointer at the bottom: `See [[sunometatag-visual-theme]] for the v1.2+ theme system.`
- [ ] **UPDATE:** `reference/ai-plan-archive.md` — prepend Archive entry 6 with the B-SUNO-002 r1 plan packet (the one that returned NEEDS_REVISION) and Archive entry 7 with the B-SUNO-002 r2 plan packet (the one that ships).

No commit in `j:\SunoMetatagApp\` (wiki lives in non-git SunoSongSetup).

**Exit criteria:** All five wiki updates landed; cross-references resolve.

---

## Task 10 — Result packet (`ai/PLAN.md` rewrite)

After USER REVIEW (Task 8) returns PASS:

- [ ] Archive current B-SUNO-002 r2 plan packet from `ai/PLAN.md` into `.SunoSongSetup-wiki/wiki/reference/ai-plan-archive.md` as Archive entry 7 (prepend; per the archive-before-edit discipline).
- [ ] Rewrite `j:\SunoSongSetup\ai\PLAN.md` as the B-SUNO-002 RESULT packet using `RESULT_REVIEW_TEMPLATE.md`. Include:
  - Files changed (the 5 commits: T1 tokens, T2 styles, T3 App.xaml, T4 MainWindow restyle, plus any T6 fix commit if a parse defect surfaced).
  - 31/31 unit tests confirmation.
  - 17-case smoke matrix results (user-confirmed).
  - Wiki updates landed: 2 new + 2 updated + 1 archive append.
  - Backlog impact: B-SUNO-002 retires; B-026 + B-027 added per specialist LOW item 6.
  - Reviewer closeout question on namespace ratification (B-SUNO-002 vs B-025).
- [ ] Surface USER ACTION NEEDED for routing to Specialist + Lead closeout.

---

## Rollback plan

Per-commit `git revert <commit-hash>` reverses any single T1–T4 step. Functional behavior unaffected because no logic-bearing surfaces are touched. Full rollback to v1.1: `git revert` the visual commits in reverse order (T4 → T3 → T2 → T1) or `git reset --hard d5f6fc8` (destructive — requires explicit user authorization per CLAUDE.md). Repo nuke: `Remove-Item -Recurse -Force j:\SunoMetatagApp` (does not affect SunoSongSetup).

---

## Out of scope for this plan

Same as spec §7:
- B-002 light theme toggle
- B-026 custom dark MessageBox (seeded at closeout)
- B-027 dark ScrollBar template (seeded at closeout)
- Inter font bundling
- B-024 syntax highlighting
- Animations / transitions
- Custom Window chrome (frameless, acrylic)
- Branding assets
- B-014 accessibility audit
- Right-click context menus

---

## Summary

11 tasks (0–10). Five code commits expected (T1 tokens, T2 styles, T3 App.xaml, T4 MainWindow, any T6 fix). Plus T0 baseline + T5 test regression + T6 dev smoke + T7 publish + T8 USER REVIEW + T9 wiki + T10 result. Visual-only; 31/31 tests carry over with zero edits; 6/6 v1.1 smoke cases pass on the restyled app.
