# Suno Metatag Editor

A single-window WPF utility for assembling Suno AI prompts as structured sections. Each section is a row of tag chips above a lyric textbox; the left pane shows a live preview you can copy to Suno.

Scratch space — no save/load.

## Requirements

- Windows 10 / 11
- .NET 8 SDK to build (`dotnet --version` → `8.x`)
- No SDK needed to *run* the published exe (self-contained)

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

## Publish (single-file self-contained exe)

```powershell
dotnet publish src/SunoMetatagApp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

Output: `publish\SunoMetatagApp.exe` + `publish\tags.json`. Copy the folder anywhere; double-click the exe.

## Editing tags

`tags.json` ships next to the exe. Edit it to add, remove, or rename tags. The app reads it once at startup; restart to pick up changes.

## Design

See [`docs/specs/2026-05-25-suno-metatag-section-editor-design.md`](docs/specs/2026-05-25-suno-metatag-section-editor-design.md).
