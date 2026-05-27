# SunoMetatagApp v1.11 — Spec (B-SUNO-011 Alphabetical Tag-Pill Ordering)

- **Authored:** 2026-05-27
- **Slice:** B-SUNO-011 / v1.11 — Sort metatag pills alphabetically within their active filter scope
- **Type:** Focused UX-polish slice — single LINQ `.OrderBy(...)` step appended to `TagService.Filter`; no schema, VM, or UI changes
- **Working baseline:** `master` tip `c1ac316` (v1.10 closeout = B-SUNO-012 PASS, B-SUNO-012 retired)
- **Specialist activation forecast:** FRONTEND/UX (user-facing rendering order change); ENGINE remains out of scope
- **Lead r1 absorptions** (carried in `target_item` from Lead's APPROVED PASS-WITH-NOTES verdict 2026-05-27):
  1. **USER REVIEW S2 explicit Verse-ordering verification** — confirm `[Verse]` / `[Verse 1]` / `[Verse 2]` lexical order is visible to the user (`[Verse 1]`, `[Verse 2]`, `[Verse]` because space `0x20` < `]` `0x5D` in ordinal comparison).
  2. **Wiki update lexical-vs-numeric caveat** — document in `[[sunometatag-tag-library]]` that sort is lexical (not natural/numeric) so `[Verse 1]` precedes `[Verse 10]` if such an entry existed; current corpus has only `[Verse 1]` and `[Verse 2]` so no actual user impact, but future entries of the same shape should know.
  3. **Case-mixed synthetic ordering test** — add a low-cost xUnit test verifying `StringComparer.OrdinalIgnoreCase` gives deterministic order across mixed-case Brackets (e.g., `[apple]` < `[BANANA]` < `[cherry]`).

## 1. Goal

Replace the current file-order pill rendering (`tags.json` insertion order) with stable alphabetical order by `Bracket` field (case-insensitive ordinal). Affects every active view of the picker (any category filter × any search query). Existing filter/search semantics unchanged — sort applies after filter.

**Resolves backlog item:** `B-SUNO-011` retires at v1.11 closeout.

## 2. Scope

### What this slice covers

- **`Services/TagService.cs` `Filter` method:** append `.OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase)` to the existing LINQ `.Where(...)` return chain. Single new step in the deferred-execution pipeline.
- **`tests/SunoMetatagApp.Tests/TagServiceFilterTests.cs`:** 3 new tests:
  - **O1** — alphabetical order across full library ("All" category, empty search).
  - **O2** — alphabetical order within a category filter, exercising prefix-form entries (`[Mood: Aggressive]` < `[Mood: Euphoric]`).
  - **O3** — case-mixed synthetic ordering test (Lead absorption #3): synthetic `TagDefinition` array with `[apple]` / `[BANANA]` / `[Cherry]` verifies `OrdinalIgnoreCase` produces deterministic, case-insensitive order.
- **No schema changes.** `TagDefinition` record unchanged.
- **No VM changes.** `MainViewModel.FilteredTags` / `InsertTag` / `InsertTagStacked` unchanged.
- **No XAML changes.** `WrapPanel` rendering unchanged; no new sort-toggle button.
- **No `tags.json` mutations.** 30,421 B / 331 entries unchanged.
- **No `prompts.json` mutations.** 75,743 B / 136 entries unchanged.

### What this slice does NOT cover (non-scope)

- **No prompt-library pill ordering.** `PromptBrowserPane`'s `ListView` already orders by `App.LoadedPrompts` natural file order (Genre → SubGenre → Title); out of v1.11 scope.
- **No category-dropdown re-ordering.** `DistinctCategories` already returns `StringComparer.Ordinal` ordered output (case-sensitive ordinal); v1.11 does not touch the category dropdown.
- **No alphabet-section dividers, grouping headers, or alphabet-jump scroll affordances** in the pill area.
- **No natural/numeric sort.** Pure lexical comparison (per the caveat in absorption #2).
- **No carry-over v1.8 dormant findings absorbed** — separate future cycles.

## 3. The mechanism (one new LINQ step)

### 3.1 Current behavior (v1.10 closeout tip `c1ac316`)

`TagService.Filter` returns filtered entries in **source-input order** (which for the production path is `tags.json` insertion order). The `MainViewModel.ComputeFiltered()` wraps each in `TagViewModel`, then binds to the `ItemsControl` → `WrapPanel`. Display order = file order.

### 3.2 The fix (one new LINQ step)

Append a single `.OrderBy(...)` step to the return value:

```csharp
return tags
    .Where(t => categoryMatches(t) && searchMatches(t))
    .OrderBy(t => t.Bracket, StringComparer.OrdinalIgnoreCase);
```

Sort applies after filtering; both filter and sort run as a single LINQ pipeline; deferred-execution semantics unchanged.

### 3.3 Sort behavior examples

`StringComparer.OrdinalIgnoreCase` comparing full `Bracket` text:

| Inputs | Sort order | Note |
|---|---|---|
| `[Verse]`, `[K-Pop]`, `[Pop]`, `[Whispered]` | `[K-Pop]`, `[Pop]`, `[Verse]`, `[Whispered]` | K < P < V < W |
| `[Mood: Euphoric]`, `[Mood: Aggressive]`, `[Mood: Nostalgic]` | `[Mood: Aggressive]`, `[Mood: Euphoric]`, `[Mood: Nostalgic]` | Alphabetical within `[Mood: *]` namespace |
| `[Vocal: Strong]`, `[Vocal: Tight]` | `[Vocal: Strong]`, `[Vocal: Tight]` | S < T after identical `[Vocal: ` prefix |
| `[Verse]`, `[Verse 1]`, `[Verse 2]` | **`[Verse 1]`, `[Verse 2]`, `[Verse]`** | **Lexical caveat (Lead absorption #1+#2):** space `0x20` < `]` `0x5D`, so `[Verse ` prefix < `[Verse]` |
| `[K-Pop]`, `[Kpop]` (hypothetical) | `[K-Pop]`, `[Kpop]` | Hyphen `-` `0x2D` < `p` `0x70` (lowercase after OrdinalIgnoreCase normalization) |
| `[R&B]`, `[R&B-Soul]` | `[R&B]`, `[R&B-Soul]` | Shorter prefix wins when chars equal |
| `[apple]`, `[BANANA]`, `[Cherry]` (synthetic) | `[apple]`, `[BANANA]`, `[Cherry]` | `OrdinalIgnoreCase`: case-folded compare a < b < c |

### 3.4 Lead absorption #2: lexical-vs-numeric sort caveat

The sort is **lexical**, not natural/numeric. For any entries that share a prefix and have numeric suffixes, the sort orders them char-by-char by ASCII code point of the digits, not by numeric value:

- `[Verse 1]` < `[Verse 2]` (lexical `1` < `2` matches numeric `1 < 2`).
- `[Verse 10]` < `[Verse 2]` would be the result if a `[Verse 10]` entry existed (lexical `1` < `2`, even though numerically 10 > 2).

**Current corpus has only `[Verse 1]` and `[Verse 2]`** — no `[Verse 10]` or higher — so no actual user-visible problem in v1.11. The wiki update at T7 documents this caveat so future curation cycles know.

### 3.5 Interaction with v1.7 search normalization

v1.7 search normalization is unchanged by v1.11. Sort happens **after** filter, so:

1. User types `kpop` in SearchBox → `NormalizeForSearch("kpop") = "kpop"` → filter returns entries whose normalized Label or Bracket contains "kpop" (e.g., `[K-Pop]`).
2. Sort applies to the post-filter result by **original Bracket text** (no normalization for sort — sort the rendered display order, not the normalized search form).

### 3.6 Interaction with v1.10 picker-pane focus preservation

v1.10's `IsAncestorOf(TagPickerPane, focused)` guard preserves `FocusedSection` when keyboard focus moves into the picker pane. v1.11 sort is a pure data-order change; no focus-state interaction. Pills' Click handler routing (`TagPickerButton_Click` → `InsertTagCommand` / `InsertTagStackedCommand`) unchanged.

## 4. Risks (carried from r1 plan packet §6, all LOW or INFO)

R1-R6 unchanged from r1 plan packet (validated by specialist with no HIGH/MEDIUM escalations). Lead's 3 LOW absorptions add explicit validation discipline rather than introducing new risks.

## 5. Validation gates

### 5.1 Test gates

`tests/SunoMetatagApp.Tests/TagServiceFilterTests.cs` additions:

| Test | Assertion | Result type |
|---|---|---|
| **O1** | `Filter` returns full library in alphabetical-by-Bracket order ("All" category, empty search). Spot-checks several known entries' relative order. | `[Fact]` |
| **O2** | `Filter` returns category-filtered entries in alphabetical order, including a prefix-form case (`[Mood: Aggressive]` before `[Mood: Euphoric]`). | `[Fact]` |
| **O3** | `Filter` returns synthetic mixed-case entries in `OrdinalIgnoreCase` order: `[apple]` < `[BANANA]` < `[Cherry]`. Lead absorption #3. | `[Fact]` |

Total test surface forecast for v1.11: **130/130 green** (127 v1.10 baseline + 3 new).

### 5.2 USER REVIEW S1-S6 (with absorption #1 Verse-ordering visibility)

| # | Scenario | Action | Expected | Critical? |
|---|---|---|---|---|
| S1 | Default-state v1.10-equivalence | Open exe; don't click anything. | Window opens; v1.10 layout preserved; initial focus on first lyric textbox; pills bright. | |
| S2 | **Alphabetical order visible + Verse triplet** (**CRITICAL** — user's reported feature + Lead absorption #1) | Look at the pill picker. Find the `[Verse]` / `[Verse 1]` / `[Verse 2]` cluster in the Structure category (or "All" filter). | (a) Pills render in alphabetical-by-Bracket order. (b) `[Verse 1]` and `[Verse 2]` appear **before** `[Verse]` (lexical caveat — space sorts before `]`). Both `[Verse 1]` and `[Verse 2]` appear in numeric order because `1` < `2`. | **YES** |
| S3 | Search + sort together | Type `solo` in SearchBox. | Results filter to entries containing "solo" in alphabetical order: `[Bass Solo]`, `[Drum Solo]`, `[Guitar Solo]`, `[Piano Solo]`, `[Saxophone Solo]`, `[Solo]`. | |
| S4 | Category filter + sort together | Change Category dropdown to **Genre**. | All Genre entries visible in alphabetical order: `[Afrobeat]`, `[Alternative Rock]`, `[Bebop]`, `[Bluegrass]`, `[Blues]`, ... | |
| S5 | Picker-pane focus preserve regression-gate (v1.10) | Focus a lyric. Click SearchBox. Type `verse`. Click `[Verse]` pill. | (a) Pills stay bright while SearchBox has focus (v1.10 contract). (b) Pill inserts into focused lyric. | **YES (regression)** |
| S6 | Stacked syntax + sort (v1.3 regression-gate) | Focus a lyric, insert `[Verse]`, then Shift+click `[Chorus]`. | Result: `[Verse | Chorus]`. | |

**Load-bearing cases:**
- **S2** — user's reported feature + Lead absorption #1 (CRITICAL).
- **S5** — regression-gate for v1.10 picker-pane focus preservation (CRITICAL).

### 5.3 Rollback path

Two-commit revert: `git revert <T2 commit> <T1 commit>` returns to v1.10 closeout tip `c1ac316`. Tests return to 127/127.

## 6. Wiki update forecast

Closeout-only wiki updates at T7 (per CLAUDE.md wiki-update gate):

- **`[[sunometatag-tag-library]]`** — new "Pill ordering (v1.11)" subsection documenting: (a) alphabetical-by-Bracket sort, (b) `StringComparer.OrdinalIgnoreCase` comparer, (c) interaction with v1.7 search normalization (sort raw Bracket, search normalized), (d) **lexical-vs-numeric sort caveat** (Lead absorption #2): `[Verse 1]` < `[Verse 2]` < `[Verse]`; if future curation adds `[Verse 10]` it sorts before `[Verse 2]`. Refresh `last_confirmed` + `review_due` frontmatter.
- **`[[sunometatag-app]]`** — title bump v1.10 → v1.11; new `## v1.10 → v1.11 (2026-05-27)` subsection.
- **`[[ai-plan-archive]]`** — Archive entry 25 prepended for v1.11 r1 plan packet at T8 closeout.

`wiki_sync_status: PASS` forecast.

## 7. Pre-submission self-check

1. **What exact question does this milestone prove?** That filtered tag entries can be rendered in alphabetical-by-Bracket order within their active filter scope via a single `.OrderBy(...)` step in `TagService.Filter`, without regressing v1.7 search normalization, v1.10 picker-pane focus preservation, v1.3 stacked-syntax, or any other prior cycle contract. Plus that the lexical-sort caveat (`[Verse 1]` before `[Verse]`) is visible and intentional.
2. **What exact code or data surface proves it?** (a) `TagService.cs` `Filter` method's new `.OrderBy` step; (b) O1+O2+O3 new tests; (c) USER REVIEW S2 visual confirmation including the Verse triplet; (d) USER REVIEW S5 v1.10 regression-gate.
3. **What is the strongest allowed conclusion?** Pills render alphabetically by Bracket within any active filter view. Prior cycle contracts preserved. Lexical-sort behavior documented.
4. **What remains unproven?** Whether users prefer natural/numeric sort over lexical for `[Verse 1]`/`[Verse 2]` style entries — user-confirmed brainstorm chose Bracket sort but didn't explicitly opine on numeric vs lexical. Documented caveat; if friction surfaces, separate future cycle.
5. **What would the reviewer reject?** A claim that the fix improves search behavior — it doesn't. A claim that prefix-form entries are now semantically grouped — they're alphabetical within their prefix-namespace cluster (which is a natural side-effect, not a semantic grouping per se).

**Claim labels:**
- One-line `.OrderBy(...)` mechanism = **Inference** (LINQ + WPF `ItemsControl` rendering well-understood).
- v1.10 contract preservation = **Inference** (sort runs after filter; picker-pane focus state unaffected).
- USER REVIEW S2 outcome (including Verse triplet) = will be **Measured** at T6.
- USER REVIEW S5 regression-gate outcome = will be **Measured** at T6.
- 130/130 test count forecast = **Hypothesis** (final exact assertions land at T2).
