# SunoMetatagApp Concept Reset — Local Suno Prompt Composer (No API / No Song Fetch)

**Date:** 2026-05-30
**Backlog:** B-SUNO-016
**Cycle:** SunoMetatagApp v1.22
**Lead r1 verdict:** `APPROVED (PASS-WITH-NOTES)` 2026-05-30 with one required absorption — precise egress wording (applied throughout).

---

## Executive summary

SunoMetatagApp is a **standalone local desktop tool** for composing and formatting Suno-ready prompts. It does NOT integrate with Suno APIs and does NOT fetch songs. Suno is a downstream paste target only — the user copies the assembled prompt text to the Windows clipboard via an explicit Copy action and pastes it into Suno separately.

This document records the **concept reset** ratifying this positioning per BACKLOG item `B-SUNO-016`.

## Removed surfaces

**None.** Zero API integration code or song-fetch workflows ever existed in the codebase. The concept reset is a positioning/documentation slice, not a code-removal slice.

Confirmed via direct audit at v1.22 T0:

| Audit query | Result |
|---|---|
| `grep` of `src/` for `HttpClient`, `WebClient`, `HttpRequest`, `api.suno`, `suno.api`, `song-fetch`, `songFetch`, `fetch.*song` | **0 matches** |
| `SunoMetatagApp.csproj` `PackageReference` list | **1 entry:** `CommunityToolkit.Mvvm` 8.4.2 (pure MVVM helpers; no networking) |
| `TargetFramework` | `net8.0-windows` (WPF desktop) |
| `using` directives across `src/SunoMetatagApp/*.cs` | None reference `System.Net.*` or any networking namespace |
| Existing test fixtures (216 cases) | Zero tests exercise API behavior (because no API exists to exercise) |
| Bundled runtime resources | 2 read-only JSON files (`tags.json`, `prompts.json`); no remote URLs |
| Runtime network egress observed during smoke testing | None |

All audit findings are **Measured** (one-shot direct evidence, not inference).

## Retained surfaces

The full v1.21 feature set is retained without modification:

### Structured lyric editing
- `Section` model with `Lyrics` + `SectionType` fields (v1.18+)
- Per-section toolbar (▲ ▼ × controls)
- Section card focus tracking and caret restoration

### Composable metatag / descriptor assembly
- `TagDefinition` library loaded from `tags.json` (374 entries across 8 categories)
- Pill picker pane with search + category filter
- Inline insertion (click) + Shift+click stacked syntax (`[tag | tag | tag]`)
- v1.19 SortOrder-based stacked-tag auto-reorder
- v1.21 alias resolution (search-only — short forms find canonical entries)
- v1.7 hyphen/space-insensitive search normalization
- v1.11 alphabetical pill-LIST ordering
- v1.13 default-Structure category on app load
- v1.16 inline search × clear control
- v1.17 SortOrder guidance banner (visible at category=All)

### Song-structure templates
- v1.18 four hardcoded built-in templates (Standard Pop, Simple Ballad, Rock / EDM, Rap / Hip-Hop)
- v1.20 user-defined template persistence to `%APPDATA%\SunoMetatagApp\templates.json`
- Save + Delete affordances; built-ins remain read-only

### Curated prompt library browser
- v1.8 mechanism + v1.9 full 136-prompt corpus
- Read-only catalog
- Browse + filter + copy-to-clipboard

### Output formatting + egress
- Live preview pane (auto-recomputed from sections)
- "Copy all" button → Windows clipboard via WPF `Clipboard.SetText`

### Visual + packaging
- Suno-themed dark UI (v1.2 visual theme + v1.10/v1.12 refinements)
- Single-file self-contained Windows publish target

## Replacement UX flows

**None needed.** No surfaces were removed, so no replacement flows are required. All retained surfaces continue to operate exactly as in v1.21.

## Migration / cleanup plan

This cycle's migration/cleanup scope:

| Surface | Action at v1.22 |
|---|---|
| `src/SunoMetatagApp/` | **No changes.** Codebase byte-unchanged. Already aligned with the local-only position. |
| `tests/SunoMetatagApp.Tests/` | **No changes.** 216/216 tests stay green. |
| `Resources/tags.json` + `Resources/prompts.json` | **No changes.** Bundled data unchanged. |
| `Themes/SunoTokens.xaml` + `Themes/SunoStyles.xaml` | **No changes.** |
| `SunoMetatagApp.csproj` | **No changes.** NuGet dependency list unchanged. |
| `README.md` | **Rewritten in this cycle.** Closes long-standing working-tree carry-over. New sections: "Local-Only Positioning", "What This Is Not". Updated feature list reflects v1.18-v1.21. Roadmap updated to mark shipped items (v1.8/v1.9 prompt library, v1.21 alias support) as Done. |
| `.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md` | **NEW** durable decision page. Captures local-only positioning + three-pillar scope + egress profile + enforcement (review discipline + review-trigger conditions). |
| Existing wiki pages mentioning "API" | **Audit conducted at T0.** All current `[Aa][Pp][Ii]` references are unambiguous code-level (e.g., `PromptService API`, `.NET API surface`). No disambiguation callouts needed. |
| `docs/` (other) | Spec + plan docs added; existing docs unchanged. |

No code migration. No test migration. No data migration. No NuGet changes.

## Egress profile (Measured, per Lead absorption #1)

The application's runtime egress is precisely the following four channels:

1. **No app-initiated API or network fetch.** The application never originates HTTP requests, never calls Suno APIs, never fetches songs or any external data at runtime. This is verified by the codebase audit above (zero networking imports, zero networking NuGet packages).
2. **Clipboard egress (primary).** User-explicit `Copy all` (and prompt-browser `Copy`) actions write text to the local Windows clipboard via `Clipboard.SetText`. The user then pastes into Suno (or anywhere else) separately. This is the primary mechanism by which output leaves the application.
3. **Local file I/O.** Read-only access to bundled `tags.json` + `prompts.json`. Read-write access to `%APPDATA%\SunoMetatagApp\templates.json` (user-defined templates introduced at v1.20). All file I/O is local-only; nothing crosses the network.
4. **User-initiated attribution hyperlink launches (potential, not currently exercised).** If a user clicks an attribution/source link rendered in the UI or in this repository's README, the standard Windows shell URL handler launches the user's default browser. This is **user-initiated**, not app-initiated. It is consistent with the no-API positioning because the application itself does not originate any network request; the user chooses to open an external URL.

This egress profile is durably documented in [`.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md`](../.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md) as a decision record.

## Commitment statement

SunoMetatagApp will not integrate with Suno APIs and will not fetch songs. Suno is a downstream paste target only.

Any future PR that:
- Adds a NuGet package with networking primitives (e.g., `System.Net.Http.*`, `RestSharp`, `Refit`, `Polly` with HTTP handlers), or
- Adds `using System.Net.*` directives, or
- Adds `HttpClient`, `WebClient`, `HttpRequest`, or `WebRequest` types, or
- Adds runtime URL-launch logic that is not user-initiated (e.g., automatic check-for-updates), or
- Adds telemetry, analytics, or any non-user-initiated outbound network traffic

triggers a `[[sunometatag-product-scope]]` revisit + explicit Lead Reviewer ratification before merge. The decision page is treated as authoritative product-scope policy.

## Acceptance check vs BACKLOG B-SUNO-016

| BACKLOG-required section | This document |
|---|---|
| Removed surfaces | §"Removed surfaces" — explicitly none, with one-shot Measured audit table |
| Retained surfaces | §"Retained surfaces" — full v1.21 feature enumeration |
| Replacement UX flows | §"Replacement UX flows" — explicitly none needed |
| Migration / cleanup plan | §"Migration / cleanup plan" — file-by-file action table; egress profile + commitment statement included |

All four BACKLOG-required sections present. Absorption #1 (precise egress wording) applied throughout. Ratification surfaces: `docs/concept-reset-2026-05-30.md` (this file), `[[sunometatag-product-scope]]` (decision page), `README.md` (user-facing).
