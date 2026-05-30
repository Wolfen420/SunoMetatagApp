# SunoMetatagApp

SunoMetatagApp is a fast, focused desktop editor for composing Suno-ready prompt text — lyrics, metatags, and song structure — in one place. Pick tags from a curated library, insert them inline with lyrics, stack them with `|` syntax, save your favorite section structures as reusable templates, and copy the finished prompt to your clipboard for pasting into Suno.

## Local-only positioning

**SunoMetatagApp is a standalone local desktop tool.** It does not integrate with Suno APIs and does not fetch songs. Suno is a downstream paste target only — you copy the assembled prompt to the Windows clipboard via the **Copy all** button and paste it into Suno yourself.

The application's runtime egress profile is precisely:

1. **No app-initiated API or network fetch.** Zero HTTP requests originate from this application.
2. **Clipboard egress (primary).** `Copy all` and prompt-library `Copy` actions write text to the local Windows clipboard.
3. **Local file I/O.** Read-only access to bundled `tags.json` + `prompts.json`. Read-write access to `%APPDATA%\SunoMetatagApp\templates.json` for user-defined song-structure templates.
4. **User-initiated attribution hyperlink launches (potential).** If you click a community-source link rendered in the UI or in this README, the standard Windows shell URL handler launches your default browser. This is user-initiated, not app-initiated.

This positioning is recorded as a durable scope decision in [`.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md`](../.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md) and ratified by the v1.22 concept reset (`docs/concept-reset-2026-05-30.md`).

## What it does

### Structured lyric editing
- Per-section lyric textboxes with optional type labels (Intro / Verse 1 / Chorus / etc.)
- Section toolbar: move up (▲), move down (▼), delete (×)
- Inline metatag insertion directly into lyric text at the caret

### Composable metatag assembly
- 374-entry curated tag library across 8 categories (Structure, Vocal, Instrument, Mood, Effect, SFX, Production, Genre)
- Pill picker with search + category + alias filtering
- **Shift+click stacked syntax**, e.g. `[Acoustic Guitar | Distorted Guitar]`
- **Automatic canonical-order stacking** — Shift+stacked tags auto-reorder by role (Structure → Vocal → Instrument → Mood → Effect → SFX → Production)
- **Alias search** — short forms like `[Aggressive]` find canonical entries like `[Mood: Aggressive]` (v1.21)
- Hyphen/space-insensitive search (e.g. `kpop` finds `[K-Pop]`)
- Alphabetical pill ordering within active scope

### Song structure templates
- 4 built-in templates: Standard Pop, Simple Ballad, Rock / EDM, Rap / Hip-Hop
- **Save your own templates** — capture the current section structure as a named template (v1.20)
- Saved templates persist locally to `%APPDATA%\SunoMetatagApp\templates.json` across app restarts
- Templates ComboBox groups built-ins and user templates separately

### Curated pre-made prompt library
- 136-entry browsable catalog (read-only) imported from public community references
- Filter by genre, browse details, copy to clipboard

### Output and packaging
- Live prompt preview as you type
- One-click **Copy all** to clipboard
- Dark, Suno-inspired UI
- Single-file Windows publish target for simple local use

## What this is NOT

- **No Suno API integration.** The app does not authenticate against Suno, call Suno endpoints, or consume any Suno SDK.
- **No song fetching.** The app does not download songs, audio, generation status, or output produced by Suno.
- **No cloud sync.** User templates persist locally only; nothing is uploaded anywhere.
- **No telemetry.** No usage statistics, no error reports, no analytics over the network.
- **No automatic update checks.** The app does not contact any update server.

If a future version were to add any of the above, the change would require explicit revisit of [`.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md`](../.SunoSongSetup-wiki/wiki/decisions/sunometatag-product-scope.md) and Lead Reviewer ratification before merge.

## Data sources and attribution

All metatags and prompt examples in this project are sourced from public internet references and community-shared materials — cheat sheets, public repositories, and community prompt collections.

If you contribute new tags or prompts, include source context when possible so the library remains transparent, curated, and useful.

## Roadmap

The local-only positioning constrains the roadmap. Items below are scope-compatible additions; anything implying network or API integration is out of scope.

### Shipped (representative)
- Curated pre-made prompt library — v1.8 / v1.9
- Genre / style taxonomy import — v1.5
- sunoaiwiki metatag reconciliation — v1.6
- Atlas Ideaverse metatag curation — v1.15
- Hyphen/space-insensitive search normalization — v1.7
- Shift+click stacked syntax + canonical-order auto-reorder — v1.3 / v1.19
- Song-structure templates (built-in + user-defined persistence) — v1.18 / v1.20
- Alias / synonym search support — v1.21
- Local-only product scope concept reset — v1.22

### Possible future
- Extend stacked auto-reorder to consult aliases for typed-in-lyrics short forms
- Preview / copy-time normalization for typed short forms to canonical
- Additional alias mappings (data-only edits to `tags.json`)
- User-template rename / in-place edit / export
- More metatag curation from community references

## Community contributions welcome

Community contributions are encouraged, especially:

- New metatag suggestions
- Better naming/merging of near-duplicate tags
- Genre/style taxonomy improvements
- High-quality prompt examples that are easy to reuse

Open an issue or pull request with proposed tags/prompts and rationale. PRs that add NuGet networking packages or any non-user-initiated outbound traffic must reference the scope decision page above and explain the rationale.

## Requirements

- Windows 10 / 11
- .NET 8 SDK to build (`dotnet --version` should return `8.x`)
- No SDK required to run the published self-contained exe

## Build

```powershell
dotnet build
```

## Run

```powershell
dotnet run --project src/SunoMetatagApp
```

## Test

```powershell
dotnet test
```

## Publish (single-file self-contained EXE)

```powershell
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

Output: `publish\SunoMetatagApp.exe` plus runtime data files (`tags.json`, `prompts.json`). Copy the publish folder anywhere and run the exe — no installation required.
