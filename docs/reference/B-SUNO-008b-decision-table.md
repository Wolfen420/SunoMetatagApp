# B-SUNO-008b Decision Table — Full Prompt Corpus Curation (v1.9)

> Per-row ADD/SKIP decision artifact for the 136-prompt awesome-suno-prompts corpus, drafted at T1 for Lead ratification before T2 test finalization per Lead's interim-specialist-checkpoint execution guard (2026-05-27).

## Source

- **Repo:** `naqashmunir21/awesome-suno-prompts`
- **License:** CC0-1.0 (no attribution legally required; voluntary credit per project hygiene)
- **Default branch:** `main`
- **Commit SHA:** `e1d1247bd26f896127011d3bbc2ba8599d54960d` (unchanged from v1.8 capture)
- **Full immutable evidence:** [`awesome-suno-prompts-source-2026-05-27-v1.9.md`](awesome-suno-prompts-source-2026-05-27-v1.9.md)

## Format

`Decision = ADD` means the entry lands in `Resources/prompts.json` verbatim per spec §3 / §4 of `2026-05-27-suno-metatag-v1.9-prompt-library-curation.md`.

`Decision = SKIP` means the entry is intentionally excluded from `Resources/prompts.json` for the rationale stated.

`Energy = null` is a parsed value, valid in the schema and tolerated by `PromptService.LoadAll` + P5 [Theory]; only used for source descriptors that don't fit the `N/10` numeric form.

## Distribution summary

| Decision | Count | % |
|---|---:|---:|
| ADD | **136** | 100% |
| SKIP | **0** | 0% |
| **Total** | **136** | |

All 136 source entries are ADD. The pre-T1 audit identified no commercial-link bodies (commercial CTAs live in separate footer sections of each `.md` file, not in prompt entries), no structurally malformed bodies, and no in-source duplicates.

Per-genre ADD distribution (matches source-file count exactly):

| Genre | Source file | ADDs | Energy=null rows |
|---|---|---:|---:|
| Pop | `pop.md` | 21 | 2 (rows 20, 21) |
| Rock | `rock.md` | 18 | 0 |
| EDM | `edm.md` | 17 | 0 |
| Hip-Hop | `hip-hop.md` | 16 | 0 |
| Indie | `indie.md` | 18 | 0 |
| Jazz-Blues | `jazz-blues.md` | 18 | 0 |
| R&B-Soul | `rnb-soul.md` | 15 | 0 |
| Country | `country.md` | 13 | 0 |
| **Total** | | **136** | **2** |

## Per-row decisions

### Pop (21 entries → 21 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 1 | Pop | Upbeat Dance Pop | Modern Pop Anthem (Female Vocals) | ADD | 9 | High-utility v1.8 seed carry; BPM/Key annotated; NotableFeature present |
| 2 | Pop | Upbeat Dance Pop | Festival Pop Banger | ADD | 10 | Festival anchor; BPM/Key annotated |
| 3 | Pop | Upbeat Dance Pop | K-Pop Inspired Dance Track | ADD | 10 | K-pop sub-style anchor; BPM/Key annotated |
| 4 | Pop | Upbeat Dance Pop | Retro 80s Dance Pop | ADD | 8 | 80s revival anchor; BPM/Key annotated |
| 5 | Pop | Upbeat Dance Pop | Latin Pop Fusion | ADD | 8 | Latin crossover sub-style; BPM/Key annotated |
| 6 | Pop | Emotional Ballads | Piano-Driven Power Ballad | ADD | 5 | v1.8 seed carry; BPM/Key annotated; NotableFeature present |
| 7 | Pop | Emotional Ballads | Orchestral Pop Ballad | ADD | 6 | Cinematic ballad sub-style; BPM/Key annotated |
| 8 | Pop | Emotional Ballads | Acoustic Intimate Ballad | ADD | 3 | Stripped-down ballad; BPM/Key annotated |
| 9 | Pop | Emotional Ballads | Gospel-Influenced Soul Ballad | ADD | 7 | Gospel-pop crossover; BPM/Key annotated |
| 10 | Pop | Indie Pop | Bedroom Pop Dreamy | ADD | 3 | Lo-fi indie sub-style; NotableFeature present |
| 11 | Pop | Indie Pop | Uplifting Indie Pop | ADD | 7 | Feel-good indie; BPM/Key annotated |
| 12 | Pop | Indie Pop | Melancholic Indie Pop | ADD | 4 | Introspective indie; BPM/Key annotated |
| 13 | Pop | Synth-Pop | Dark Synth-Pop | ADD | 6 | Moody synth-pop reference; BPM/Key annotated |
| 14 | Pop | Synth-Pop | Bright Synth-Pop | ADD | 8 | Uplifting synth-pop; BPM/Key annotated |
| 15 | Pop | Synth-Pop | Chillwave Synth-Pop | ADD | 4 | Hazy chillwave; BPM/Key annotated |
| 16 | Pop | Teen Pop | Bubblegum Pop | ADD | 8 | Teen pop anchor; BPM/Key annotated |
| 17 | Pop | Teen Pop | Pop-Punk Teen Anthem | ADD | 9 | Pop-punk crossover; BPM/Key annotated |
| 18 | Pop | Adult Contemporary | Radio-Friendly AC Pop | ADD | 6 | AC anchor; BPM/Key annotated |
| 19 | Pop | Adult Contemporary | Coffee Shop Pop | ADD | 4 | Background AC; BPM/Key annotated |
| 20 | Pop | Experimental Pop | Art Pop Avant-Garde | ADD | null | Source Energy = `"Variable"` (non-numeric); P5 [Theory] permits null on ballad/chill anchor; Body annotation `"BPM: Variable, Key: Atonal elements"` preserved verbatim |
| 21 | Pop | Experimental Pop | Hyperpop Chaotic Energy | ADD | null | Source Energy = `"11/10 (off the scale)"` (out-of-bounds for `int? 0-10`); P5 permits null; Body annotation `"BPM: 170, Key: Chromatic chaos"` preserved verbatim |

### Rock (18 entries → 18 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 22 | Rock | Stadium Rock Anthems | Epic Arena Anthem | ADD | 9 | v1.8 seed carry; BPM/Key annotated; NotableFeature present |
| 23 | Rock | Stadium Rock Anthems | Power Ballad Rock | ADD | 8 | Source Energy `"Medium builds to High (5→8/10)"` → final value 8; BPM/Key annotated |
| 24 | Rock | Stadium Rock Anthems | Victory Rock Anthem | ADD | 9 | Triumphant anchor; BPM/Key annotated |
| 25 | Rock | Grunge/Alternative | 90s Grunge Sound | ADD | 8 | 90s grunge anchor; NotableFeature present |
| 26 | Rock | Grunge/Alternative | Alternative Rock Edge | ADD | 7 | Modern alt-rock; BPM/Key annotated |
| 27 | Rock | Grunge/Alternative | Grunge Ballad | ADD | 6 | v1.8 seed carry; BPM/Key annotated |
| 28 | Rock | Classic Rock | 70s Classic Rock | ADD | 8 | Vintage anchor; NotableFeature present |
| 29 | Rock | Classic Rock | Southern Rock Groove | ADD | 8 | Southern rock sub-style; BPM/Key annotated |
| 30 | Rock | Classic Rock | 80s Arena Rock | ADD | 9 | 80s arena anchor; BPM/Key annotated incl. key change |
| 31 | Rock | Metal/Heavy Rock | Heavy Metal Power | ADD | 10 | Metal anchor; NotableFeature present |
| 32 | Rock | Metal/Heavy Rock | Doom Metal Darkness | ADD | 4 | Slow heavy doom; BPM/Key annotated |
| 33 | Rock | Metal/Heavy Rock | Thrash Metal Speed | ADD | 10 | Thrash speed anchor; BPM/Key annotated |
| 34 | Rock | Punk Rock | Classic Punk Attitude | ADD | 9 | Punk anchor; NotableFeature present |
| 35 | Rock | Punk Rock | Pop-Punk Energy | ADD | 9 | Pop-punk crossover; BPM/Key annotated |
| 36 | Rock | Punk Rock | Post-Punk Darkness | ADD | 5 | Post-punk atmosphere; BPM/Key annotated |
| 37 | Rock | Indie Rock | Indie Rock Jangle | ADD | 6 | Jangle anchor; BPM/Key annotated |
| 38 | Rock | Indie Rock | Garage Rock Revival | ADD | 8 | Garage rock sub-style; BPM/Key annotated |
| 39 | Rock | Indie Rock | Math Rock Complexity | ADD | 7 | Math rock complexity; NotableFeature present |

### EDM (17 entries → 17 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 40 | EDM | Festival Bangers | Big Room House Anthem | ADD | 10 | v1.8 seed carry; NotableFeature present |
| 41 | EDM | Festival Bangers | Mainstage EDM Drop | ADD | 9 | Mainstage anchor; BPM/Key annotated |
| 42 | EDM | Festival Bangers | Festival Trap Hybrid | ADD | 10 | Trap-EDM crossover; BPM/Key annotated |
| 43 | EDM | Progressive House | Melodic Progressive Journey | ADD | 8 | Prog-house anchor; NotableFeature present |
| 44 | EDM | Progressive House | Deep Progressive Groove | ADD | 7 | Deep prog sub-style; BPM/Key annotated |
| 45 | EDM | Progressive House | Progressive Trance | ADD | 9 | Prog-trance crossover; BPM/Key annotated |
| 46 | EDM | Dubstep/Bass Music | Heavy Dubstep Drop | ADD | 10 | Dubstep anchor; NotableFeature present |
| 47 | EDM | Dubstep/Bass Music | Melodic Dubstep | ADD | 8 | Melodic dubstep sub-style; BPM/Key annotated |
| 48 | EDM | Dubstep/Bass Music | Drum and Bass Energy | ADD | 10 | DnB anchor; BPM/Key annotated |
| 49 | EDM | Trance | Uplifting Trance Anthem | ADD | 9 | Trance anchor; NotableFeature present (key change) |
| 50 | EDM | Trance | Psy-Trance Journey | ADD | 9 | Psy-trance sub-style; BPM/Key annotated |
| 51 | EDM | Trance | Progressive Trance (Anjuna Style) | ADD | 8 | Anjuna sub-style; BPM/Key annotated |
| 52 | EDM | Techno | Peak-Time Techno | ADD | 9 | Techno anchor; NotableFeature present |
| 53 | EDM | Techno | Melodic Techno | ADD | 7 | Melodic techno sub-style; BPM/Key annotated |
| 54 | EDM | Techno | Industrial Techno | ADD | 10 | Industrial sub-style; BPM/Key annotated |
| 55 | EDM | Future Bass | Emotional Future Bass | ADD | 8 | Future bass anchor; NotableFeature present |
| 56 | EDM | Future Bass | Chill Future Bass | ADD | 5 | v1.8 seed carry; BPM/Key annotated |

### Hip-Hop (16 entries → 16 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 57 | Hip-Hop | Trap Bangers | Modern Trap Anthem | ADD | 9 | v1.8 seed carry; NotableFeature present |
| 58 | Hip-Hop | Trap Bangers | Melodic Trap Vibes | ADD | 6 | Melodic trap sub-style; BPM/Key annotated |
| 59 | Hip-Hop | Trap Bangers | Aggressive Trap Energy | ADD | 10 | Aggressive trap; NotableFeature present |
| 60 | Hip-Hop | Boom Bap Classics | 90s Boom Bap | ADD | 5 | v1.8 seed carry; NotableFeature present |
| 61 | Hip-Hop | Boom Bap Classics | Jazz Rap Fusion | ADD | 4 | Jazz-rap crossover; BPM/Key annotated |
| 62 | Hip-Hop | Boom Bap Classics | East Coast Hardcore | ADD | 8 | East Coast sub-style; BPM/Key annotated |
| 63 | Hip-Hop | Melodic Rap | Emo Rap Atmosphere | ADD | 4 | Emo-rap sub-style; NotableFeature present |
| 64 | Hip-Hop | Melodic Rap | Sing-Rap Crossover | ADD | 6 | Pop-rap crossover; BPM/Key annotated |
| 65 | Hip-Hop | Melodic Rap | Cloudy SoundCloud Rap | ADD | 5 | SoundCloud sub-style; BPM/Key annotated |
| 66 | Hip-Hop | Drill Music | UK Drill Intensity | ADD | 9 | UK drill anchor; NotableFeature present |
| 67 | Hip-Hop | Drill Music | Chicago Drill Rawness | ADD | 9 | Chicago drill anchor; BPM/Key annotated |
| 68 | Hip-Hop | Conscious Rap | Lyrical Storytelling | ADD | 5 | Conscious-rap anchor; NotableFeature present |
| 69 | Hip-Hop | Conscious Rap | Spoken Word Hip-Hop | ADD | 4 | Spoken-word sub-style; BPM/Key annotated |
| 70 | Hip-Hop | Old School Hip-Hop | Golden Era 80s | ADD | 7 | Golden-era anchor; NotableFeature present |
| 71 | Hip-Hop | Old School Hip-Hop | Funky Hip-Hop Groove | ADD | 8 | Funky sub-style; BPM/Key annotated |
| 72 | Hip-Hop | Old School Hip-Hop | West Coast G-Funk | ADD | 6 | G-funk sub-style; NotableFeature present |

### Indie (18 entries → 18 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 73 | Indie | Bedroom Pop | Intimate Bedroom Pop | ADD | 4 | Bedroom anchor; NotableFeature present |
| 74 | Indie | Bedroom Pop | Upbeat Bedroom Pop | ADD | 7 | Upbeat bedroom sub-style; BPM/Key annotated |
| 75 | Indie | Bedroom Pop | Melancholic Bedroom Indie | ADD | 3 | v1.8 seed carry; BPM/Key annotated |
| 76 | Indie | Indie Rock | Jangly Indie Rock | ADD | 7 | v1.8 seed carry; NotableFeature present |
| 77 | Indie | Indie Rock | Garage Indie Revival | ADD | 8 | Garage indie; BPM/Key annotated |
| 78 | Indie | Indie Rock | Post-Punk Revival | ADD | 6 | Post-punk indie; BPM/Key annotated |
| 79 | Indie | Shoegaze | Classic Shoegaze Wall | ADD | 5 | Shoegaze anchor; NotableFeature present |
| 80 | Indie | Shoegaze | Modern Shoegaze | ADD | 6 | Modern shoegaze sub-style; BPM/Key annotated |
| 81 | Indie | Shoegaze | Blackgaze Darkness | ADD | 9 | Blackgaze hybrid; BPM/Key annotated |
| 82 | Indie | Dream Pop | Ethereal Dream Pop | ADD | 4 | Dream-pop anchor; NotableFeature present |
| 83 | Indie | Dream Pop | Synth Dream Pop | ADD | 5 | Synth-dream sub-style; BPM/Key annotated |
| 84 | Indie | Dream Pop | Upbeat Dream Pop | ADD | 7 | Upbeat dream sub-style; BPM/Key annotated |
| 85 | Indie | Folk/Acoustic Indie | Indie Folk Storytelling | ADD | 4 | Indie-folk anchor; NotableFeature present |
| 86 | Indie | Folk/Acoustic Indie | Folk-Pop Crossover | ADD | 8 | Folk-pop sub-style; BPM/Key annotated |
| 87 | Indie | Folk/Acoustic Indie | Melancholic Folk | ADD | 3 | Sad folk sub-style; BPM/Key annotated |
| 88 | Indie | Lo-Fi Indie | Lo-Fi Indie Pop | ADD | 5 | Lo-fi anchor; NotableFeature present |
| 89 | Indie | Lo-Fi Indie | Slacker Rock | ADD | 5 | Slacker sub-style; BPM/Key annotated |
| 90 | Indie | Lo-Fi Indie | Noise Pop | ADD | 8 | Noise-pop hybrid; BPM/Key annotated |

### Jazz-Blues (18 entries → 18 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 91 | Jazz-Blues | Smooth Jazz | Contemporary Smooth Jazz | ADD | 4 | Smooth-jazz anchor; NotableFeature present |
| 92 | Jazz-Blues | Smooth Jazz | Smooth Jazz Ballad | ADD | 3 | v1.8 seed carry; BPM/Key annotated |
| 93 | Jazz-Blues | Smooth Jazz | Upbeat Smooth Jazz | ADD | 7 | Upbeat sub-style; BPM/Key annotated |
| 94 | Jazz-Blues | Bebop/Hard Bop | Fast Bebop | ADD | 9 | Bebop anchor; NotableFeature present |
| 95 | Jazz-Blues | Bebop/Hard Bop | Hard Bop Groove | ADD | 8 | v1.8 seed carry; BPM/Key annotated |
| 96 | Jazz-Blues | Bebop/Hard Bop | Modal Jazz | ADD | 5 | Modal sub-style; BPM/Key annotated |
| 97 | Jazz-Blues | Chicago Blues | Electric Chicago Blues | ADD | 7 | Chicago blues anchor; NotableFeature present |
| 98 | Jazz-Blues | Chicago Blues | Slow Blues Ballad | ADD | 4 | Slow blues sub-style; BPM/Key annotated |
| 99 | Jazz-Blues | Chicago Blues | Jump Blues | ADD | 8 | Source Energy `"Very High (8/10)"` → 8; jump-blues sub-style; BPM/Key annotated |
| 100 | Jazz-Blues | Jazz Fusion | Jazz-Rock Fusion | ADD | 9 | Fusion anchor; NotableFeature present |
| 101 | Jazz-Blues | Jazz Fusion | Funk-Jazz Fusion | ADD | 8 | Funk-jazz sub-style; BPM/Key annotated |
| 102 | Jazz-Blues | Jazz Fusion | Latin Jazz Fusion | ADD | 9 | Latin-jazz sub-style; BPM/Key annotated |
| 103 | Jazz-Blues | Swing/Big Band | Classic Big Band Swing | ADD | 8 | Big-band anchor; NotableFeature present |
| 104 | Jazz-Blues | Swing/Big Band | Modern Big Band | ADD | 9 | Modern big-band sub-style; BPM/Key annotated |
| 105 | Jazz-Blues | Swing/Big Band | Gypsy Jazz | ADD | 9 | Gypsy-jazz sub-style; BPM/Key annotated |
| 106 | Jazz-Blues | Contemporary Jazz | Neo-Soul Jazz | ADD | 6 | Neo-soul jazz anchor; NotableFeature present |
| 107 | Jazz-Blues | Contemporary Jazz | Ambient Jazz | ADD | 2 | Ambient sub-style; BPM/Key annotated |
| 108 | Jazz-Blues | Contemporary Jazz | Jazz-House Fusion | ADD | 8 | Jazz-house sub-style; BPM/Key annotated |

### R&B-Soul (15 entries → 15 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 109 | R&B-Soul | Modern Trap-Soul | Trap-Soul Vibes | ADD | 4 | Trap-soul anchor; NotableFeature present |
| 110 | R&B-Soul | Modern Trap-Soul | Alternative R&B Darkness | ADD | 3 | Moody alt-R&B sub-style; BPM/Key annotated |
| 111 | R&B-Soul | Modern Trap-Soul | Upbeat Modern R&B | ADD | 7 | Upbeat modern R&B; BPM/Key annotated |
| 112 | R&B-Soul | Classic Soul | Motown Soul | ADD | 8 | v1.8 seed carry; NotableFeature present |
| 113 | R&B-Soul | Classic Soul | Southern Soul Ballad | ADD | 5 | Southern-soul sub-style; BPM/Key annotated |
| 114 | R&B-Soul | Classic Soul | 70s Soul Groove | ADD | 7 | 70s soul groove; BPM/Key annotated |
| 115 | R&B-Soul | Neo-Soul | 90s Neo-Soul | ADD | 5 | Neo-soul anchor; NotableFeature present |
| 116 | R&B-Soul | Neo-Soul | Contemporary Neo-Soul | ADD | 6 | Modern neo-soul sub-style; BPM/Key annotated |
| 117 | R&B-Soul | Neo-Soul | Neo-Soul Ballad | ADD | 4 | Neo-soul ballad sub-style; BPM/Key annotated |
| 118 | R&B-Soul | Quiet Storm | Smooth Quiet Storm | ADD | 3 | v1.8 seed carry; NotableFeature present |
| 119 | R&B-Soul | Quiet Storm | Bedroom R&B | ADD | 2 | Bedroom R&B sub-style; BPM/Key annotated |
| 120 | R&B-Soul | Quiet Storm | Sensual Slow Jam | ADD | 3 | Sensual sub-style; BPM/Key annotated |
| 121 | R&B-Soul | Alternative R&B | Experimental Alternative R&B | ADD | 4 | Experimental alt-R&B anchor; NotableFeature present |
| 122 | R&B-Soul | Alternative R&B | Electronic R&B Fusion | ADD | 7 | Electronic R&B sub-style; BPM/Key annotated |
| 123 | R&B-Soul | Alternative R&B | PBR&B (Alternative R&B) | ADD | 4 | PBR&B sub-style; BPM/Key annotated |

### Country (13 entries → 13 ADD)

| # | Genre | SubGenre | Title | Decision | Energy | Rationale |
|---|---|---|---|---|---:|---|
| 124 | Country | Modern Pop-Country | Radio Pop-Country Hit | ADD | 8 | v1.8 seed carry; NotableFeature present |
| 125 | Country | Modern Pop-Country | Pop-Country Ballad | ADD | 5 | Pop-country ballad sub-style; BPM/Key annotated |
| 126 | Country | Modern Pop-Country | Bro-Country Party | ADD | 9 | Bro-country anchor; BPM/Key annotated |
| 127 | Country | Traditional Country | Classic Nashville Sound | ADD | 6 | Nashville anchor; NotableFeature present |
| 128 | Country | Traditional Country | Honky-Tonk Spirit | ADD | 7 | Honky-tonk sub-style; BPM/Key annotated |
| 129 | Country | Traditional Country | Country Waltz | ADD | 4 | v1.8 seed carry; NotableFeature present (3/4 time) |
| 130 | Country | Outlaw Country | Outlaw Country Anthem | ADD | 8 | Outlaw anchor; NotableFeature present |
| 131 | Country | Outlaw Country | Country Blues Fusion | ADD | 5 | Country-blues hybrid; BPM/Key annotated |
| 132 | Country | Bluegrass | Traditional Bluegrass | ADD | 9 | Bluegrass anchor; NotableFeature present |
| 133 | Country | Bluegrass | Progressive Bluegrass | ADD | 8 | Modern bluegrass sub-style; BPM/Key annotated |
| 134 | Country | Bluegrass | Bluegrass Gospel | ADD | 8 | Bluegrass-gospel sub-style; BPM/Key annotated |
| 135 | Country | Country Rock | Country-Rock Crossover | ADD | 9 | Country-rock crossover; BPM/Key annotated |
| 136 | Country | Country Rock | Red Dirt Country | ADD | 8 | Texas country-rock sub-style; NotableFeature present |

## v1.8-seed carry confirmation

The 16 v1.8 seed entries are part of the 136 ADDs (rows 1, 6, 22, 27, 40, 56, 57, 60, 75, 76, 92, 95, 112, 118, 124, 129). Their bodies are identical to the v1.8 seed corpus (sourced from the same commit SHA `e1d1247`); re-importing as part of the 136 introduces no data drift.

## P5 [Theory] satisfaction audit (per-genre high/low energy split)

P5 requires per-genre `>=1` entry with `Energy >= 7` AND `>=1` entry with `Energy <= 6 OR null`. Audit:

| Genre | High (≥7) count | Low (≤6 or null) count | P5 satisfied |
|---|---:|---:|---|
| Pop | 10 | 11 (incl. 2 null) | ✓ |
| Rock | 14 | 4 | ✓ |
| EDM | 16 | 1 | ✓ |
| Hip-Hop | 7 | 9 | ✓ |
| Indie | 7 | 11 | ✓ |
| Jazz-Blues | 12 | 6 | ✓ |
| R&B-Soul | 4 | 11 | ✓ |
| Country | 9 | 4 | ✓ |

All 8 P5 [Theory] inline rows pass.

## P3 minimum count assertion (v1.9 shape)

Source-file distribution becomes the per-genre minimum in `tests/SunoMetatagApp.Tests/PromptServiceTests.cs` P3:

| Genre | Min |
|---|---:|
| Pop | 21 |
| Rock | 18 |
| EDM | 17 |
| Hip-Hop | 16 |
| Indie | 18 |
| Jazz-Blues | 18 |
| R&B-Soul | 15 |
| Country | 13 |

Since all 136 are ADD, the actual counts exactly match the minimums. P3 reads "each genre has `>=` source-distributed minimum" which trivially passes.

## Planner notes for Lead

- **No SKIPs.** Pre-T1 audit identified no commercial-link bodies, no malformed bodies, no in-source duplicates. If Lead at interim-specialist-checkpoint requires a defensive SKIP for the 2 Energy=null entries (rows 20-21 "Art Pop Avant-Garde" and "Hyperpop Chaotic Energy"), the table can be amended in r2 — but planner-default ADD because (1) P5 [Theory] explicitly permits null, (2) the Body text is rich and useful, (3) the SubGenre "Experimental Pop" gets two entries either way.
- **Energy parsing rules** are documented in spec §4.4 and applied consistently here: `N/10` → `N`; `Variable` → `null`; `11/10 (off the scale)` → `null`; `"Medium builds to High (5→8/10)"` → final value `8`.
- **NotableFeature present** annotations in the Rationale column flag entries that carry the upstream `**Notable Feature:**` label, useful as visual UI hints in the detail panel.
- **"BPM/Key annotated"** annotations confirm the source's `BPM: N, Key: <key>` line is present in the body — useful evidence for the no-schema-additions discipline (BPM/Key stays inline in Body rather than being parsed into separate fields).

## Source enumeration verification

Prompt H3-entry count per file (H3 headings *above* the per-file `## Production Tips` section; H3s under Production Tips like `### Vocal Production` / `### Structure Patterns` are reference material, not prompts):

| File | Prompt H3s | Production-Tips H3s | Total H3s in file |
|---|---:|---:|---:|
| `pop.md` | **21** | 3 | 24 |
| `rock.md` | **18** | 4 | 22 |
| `edm.md` | **17** | 4 | 21 |
| `hip-hop.md` | **16** | 5 | 21 |
| `indie.md` | **18** | 5 | 23 |
| `jazz-blues.md` | **18** | 5 | 23 |
| `rnb-soul.md` | **15** | 5 | 20 |
| `country.md` | **13** | 5 | 18 |
| **TOTAL prompts** | **136** | 36 | 172 |

Verification command (pop.md production-tips header is `## 💡 Pop Production Tips` at line 328; other files use `## Production Tips` or `## 💡 Production Tips`):

```bash
awk 'NR<328 && /^### /' pop.md | wc -l   # → 21
```
