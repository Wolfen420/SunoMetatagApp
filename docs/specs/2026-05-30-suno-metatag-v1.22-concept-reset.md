# SunoMetatagApp v1.22 — Concept Reset (B-SUNO-016)

**Date:** 2026-05-30
**Backlog:** B-SUNO-016 — Concept Reset (Local Suno Prompt Composer, No API / No Song Fetch)
**Cycle predecessor:** v1.21 (B-SUNO-007c alias resolution) closeout `APPROVED (PASS)` 2026-05-30
**Lead r1 verdict:** `APPROVED (PASS-WITH-NOTES)` 2026-05-30 — one required absorption (precise egress wording)

---

## Scope

Pure docs/positioning cycle. Produces the Lead-mandated concept-reset packet, a new durable wiki decision page, rewrites the long-standing carry-over `README.md`, and ratifies the local-only Suno prompt composer positioning. **No source code changes; codebase is already pure-local** (audited at T0: zero networking imports in `src/`, single `CommunityToolkit.Mvvm` NuGet ref, `net8.0-windows` TargetFramework).

## Deliverables

| Artifact | Path | Purpose |
|---|---|---|
| Concept-reset packet | `docs/concept-reset-2026-05-30.md` | BACKLOG-acceptance four-section packet (removed/retained/replacement/migration) + Measured audit findings + commitment statement |
| Wiki decision page | `.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md` (NEW) | Durable scope decision record; three-pillar scope; egress profile; revisit-trigger conditions |
| README rewrite | `README.md` | User-facing positioning with "Local-only positioning" + "What this is NOT" sections; closes long-standing carry-over |
| Spec doc | `docs/specs/2026-05-30-suno-metatag-v1.22-concept-reset.md` | This file |
| Plan doc | `docs/plans/2026-05-30-suno-metatag-v1.22-concept-reset.md` | T0-T8 task sequence |
| Wiki feature-log update | `.SunoSongSetup-wiki/wiki/features/sunometatag-app.md` | v1.21 → v1.22 title bump + new subsection |
| Wiki plan-archive entry | `.SunoSongSetup-wiki/wiki/reference/ai-plan-archive.md` | Archive entry 47 for v1.22 r1 plan packet |

## Absorption #1 — precise egress wording (Lead)

Lead's r1 required absorption: refine egress wording from "clipboard-only egress" to a precise four-channel description consistent with v1.21 implementation. Applied throughout all v1.22 artifacts:

1. **No app-initiated API or network fetch** (preferred phrasing).
2. **Clipboard egress (primary)** — `Copy all` + prompt-library `Copy` via `Clipboard.SetText`.
3. **Local file I/O** — read-only `tags.json` + `prompts.json`; read-write `%APPDATA%\SunoMetatagApp\templates.json` (v1.20+).
4. **User-initiated attribution hyperlink launch (potential, not currently exercised)** — Windows shell URL handler if a user clicks a community-source link.

The application-initiated vs user-initiated distinction is load-bearing and is repeated consistently across the concept-reset packet, the wiki decision page, and the README.

## Audit (T0, Measured)

| Query | Result |
|---|---|
| `grep` of `src/` for `HttpClient`/`WebClient`/`HttpRequest`/`api.suno`/`suno.api`/`song-fetch`/`songFetch`/`fetch.*song` | **0 matches** |
| `SunoMetatagApp.csproj` PackageReference list | `CommunityToolkit.Mvvm` 8.4.2 only |
| `TargetFramework` | `net8.0-windows` (WPF desktop) |
| `using System.Net.*` directives in `src/SunoMetatagApp/*.cs` | **None** |
| Existing test fixtures | 216/216 green; no test exercises API behavior (because no API exists) |
| Existing wiki `[Aa][Pp][Ii]` audit (4 pages: `sunometatag-prompt-library`, `sunometatag-app`, `ai-engine-review-archive`, `ai-plan-archive`) | All references unambiguous code-level (`PromptService API`, `service API`, `.NET API`); no disambiguation callouts needed |

All findings recorded as Measured in the concept-reset packet.

## Validation

- Tests: **216/216** unchanged (no source changes).
- Build: clean.
- Dev/publish smoke: `EXIT=124` expected.
- Publish artifacts: **byte-identical to v1.21** expected (no source changes).
- USER REVIEW S1-S4 per plan §6.3.

## Explicit non-changes

- **No `src/SunoMetatagApp/` changes.** Codebase byte-unchanged.
- **No `tests/SunoMetatagApp.Tests/` changes.** 216/216 unchanged.
- **No `Resources/tags.json` / `Resources/prompts.json` changes.**
- **No `Themes/*` / `MainWindow.xaml` / `MainWindow.xaml.cs` / `App.xaml.cs` changes.**
- **No `SunoMetatagApp.csproj` changes.** NuGet dependency list unchanged.
- **No code-level guards** (no architectural regression tests; no policy comment blocks). Enforcement is review discipline + the wiki decision page.
- **No removal of existing wiki pages.** Only additions (new decision page) and feature-log update at T7.
- **No edit to historical `ai/` archives.** Only archive entry 47 prepend at T8.

## Acceptance

- BACKLOG B-SUNO-016 four required sections present in concept-reset packet.
- Absorption #1 (precise egress wording) applied throughout.
- Long-standing `README.md` carry-over closed (committed).
- All 216 v1.21 tests still green.
- Multi-cycle regression-gates intact (no source changes).
