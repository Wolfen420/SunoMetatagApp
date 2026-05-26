# Suno Metatag Cheat Sheet (user-provided 2026-05-26)

**Source:** Google Doc shared by user — `https://docs.google.com/document/d/1IyRMUndullNegR8scfAHE354ZkUoHemfg_zucIoY_ls/edit?tab=t.0` (auth-gated; user pasted contents into chat 2026-05-27 to unblock B-SUNO-005).

**Purpose:** Canonical reference for B-SUNO-005 reconciliation against [`Resources/tags.json`](../../src/SunoMetatagApp/Resources/tags.json). All ADD / MERGE / SKIP decisions reference this file by section + verbatim bracket form.

**Preservation rule:** Treat this file as **immutable source of truth** for the B-SUNO-005 cycle. Edits limited to typo fixes with a `> [!note]` callout; do not edit metatag entries without re-confirming with user.

---

## Section A — Example song using metatags (verbatim from doc opening)

```
[Intro]

[Spoken Word- create a section with spoken vocals]

[Fade In]

[Verse 1]

[Pre-Chorus] [Choir - add choir vocals]

[Chorus] [Harmonies]

[Post-Chorus]

[Verse 2]

[Bridge]

[Catchy Hook] [Harmonies]

[Verse 3]

[Break]

[Choir]: Adds choir vocals

[Chorus, repeated twice] [Harmonies]

[Chorus]

[Fade Out]

[Break]

[Choir - add choir vocals]

[Instrumental Break as Outro. Include complete copy of song without any vocals]:
```

**Observations for reconciliation:**

- Demonstrates `[A] [B]` line-sharing pattern (e.g. `[Pre-Chorus] [Choir - add choir vocals]`) — v1.1 inline insertion supports this; v1.3 Shift+click can produce `[A | B]` alternative form.
- Shows `[Tag- description]` and `[Tag - description]` patterns (whitespace varies); spec §2.4 treats these as bracket contents, not a separate syntax. **Decision:** treat as bracket-content variation, not new metatag.
- `[Tag, modifier]` pattern: e.g. `[Chorus, repeated twice]`. Same treatment.
- `[Tag]:` (with colon) used like a markdown speaker label: e.g. `[Choir]: Adds choir vocals`. This is doc-formatting only; the bracket token is just `[Choir]`.
- `[Long descriptive content]:` like `[Instrumental Break as Outro. Include complete copy of song without any vocals]:` — content inside brackets is freeform per Suno. We don't add it as a tag; the user can type freeform inside any bracket.

## Section B — Numbered categories (verbatim)

### 1. Song Structure Metatags (11 entries)

```
[Intro]: Marks the beginning of the song
[Verse]: Indicates a verse section
[Pre-Chorus]: Prepares for the chorus
[Chorus]: Highlights the main chorus
[Post-Chorus]: Adds a section after the chorus
[Bridge]: Introduces a contrasting section
[Outro]: Marks the end of the song
[Hook]: Emphasizes a catchy part
[Break]: Introduces a break in the song
[Fade Out]: Gradually decreases volume to end the song
[Fade In]: Gradually introduces a section
```

### 2. Instrumental Metatags (6 entries)

```
[Instrumental]: Adds an instrumental section
[Guitar Solo]: Features a guitar solo
[Piano Solo]: Features a piano solo
[Drum Solo]: Features a drum solo
[Bass Solo]: Features a bass solo
[Instrumental Break]: Insert an instrumental section
```

### 3. Vocal Metatags (8 entries)

```
[Male Vocal]: Specifies male vocals
[Female Vocal]: Specifies female vocals
[Duet]: Indicates a duet
[Choir]: Adds choir vocals
[Spoken Word]: Creates a section with spoken vocals
[Harmonies]: Adds vocal harmonies
[Vulnerable Vocals]: Generates raw, emotional vocal performances
[Whisper]: Generates softer, whispered vocals
```

### 4. Specific Elements Metatags (6 entries)

```
[Catchy Hook]: Creates a memorable hook
[Emotional Bridge]: Adds an emotionally intense bridge
[Powerful Outro]: Ends the song with a strong outro
[Soft Intro]: Starts the song softly
[Melodic Interlude]: Adds a melodic break
[Percussion Break]: Introduces a percussion-focused section
```

### 5. Atmosphere and Mood Metatags (9 entries)

```
[Eerie Whispers]: Adds faint, unsettling background vocals
[Ghostly Echoes]: Creates reverb-heavy, ethereal sounds
[Ominous Drone]: Introduces a low, continuous tone for tension
[Spectral Melody]: Generates a haunting, otherworldly melody
[Melancholic Atmosphere]: Creates a sad or reflective mood
[Euphoric Build]: Builds towards a joyful climax
[Tense Underscore]: Adds underlying tension to the music
[Serene Ambience]: Creates a peaceful, calm atmosphere
[Nostalgic Tones]: Evokes a sense of nostalgia
```

### 6. Dynamic and Progression Metatags (9 entries)

```
[Building Intensity]: Gradually increases musical intensity
[Climactic]: Reaches a musical high point
[Emotional Swell]: Creates a gradual build-up of emotional intensity
[Layered Arrangement]: Creates complex, multi-instrumental arrangements
[Orchestral Build]: Gradually introduces orchestral elements
[Stripped Back]: Reduces instrumentation to bare essentials
[Sudden Break]: Introduces an abrupt change
[Crescendo]: Gradually increases volume/intensity
[Decrescendo]: Gradually decreases volume/intensity
```

**Subtotal Section B: 49 unique metatags across 6 named categories.**

## Section C — Sample composed song (verbatim, contains an in-text "Perfect Studio Vocals" entry worth noting)

```
[Intro: Jazzy piano chords with a lo-fi crackle, distant sleigh bells, and a laid-back drum loop.]
[Spoken Word]
"Yo, it's the most wonderful time… and the most stressful.
From the gift wrap to the setbacks, let's talk about it.
Detroit, let's vibe."

[Chorus: Smooth, soulful hook with layered harmonies and a saxophone riff between lines.]
It's the holly and the hard times, joy and the grind,
Snow on the streets, but the heat's in the rhymes.
Eggnog and missteps, family and debt,
Lightin' up the tree, tryin' hard to forget.

Perfect Studio Vocals
[ Melismatic Runs Multiple notes sung on one syllable, showcasing vocal flexibility and emotion. R&B, Gospel, Soul, Pop Lead vocals, choir (alto/soprano) Melisma, embellishment, elongation (Melismatic]
```

**Observations:**
- `[Intro: <descriptive prose>]` — bracket content can be very long and descriptive; just a freeform usage; no new metatag implied.
- `[ Melismatic Runs ...]` — appears to be either a malformed entry (leading space, parenthetical description, etc.) OR a candidate canonical metatag `[Melismatic Runs]`. **Reconciliation candidate:** add `[Melismatic Runs]` as a `Vocal` tag (alias/related to v1's `[Whispered]` but different — about technique). Flag for ADD/MERGE/SKIP decision.

## Section D — Unstructured "Sound Effects" list (verbatim; ~63 entries)

```
Sound Effects
[Beeping]
[Sighs]
[Footsteps]
[Beep]
[Gunshot]
[Wind]
[Rain]
[Door Shutting]
[Clapping]
[Thunder]
[Birdsong]
[Waves]
[Siren]
[Clock Ticking]
[Dog Barking]
[Car Engine]
[Crowd Cheering]
[Heartbeat]
[Bell Ringing]
[Glass Breaking]
[Train Whistle]
[Laughing]
[Whistling]
[Horse Galloping]
[Fire Crackling]
[Helicopter]
[Typing]
[Crickets Chirping]
[Nighttime Atmosphere]
[Camera Shutter]
[Applause]
[Snapping Fingers]
[Telephone Ringing]
[Audience Cheering]
[Traffic Noise]
[Construction Sounds]
[Urban Street Noise]
[Footsteps on Gravel]
[Footsteps on Pavement]
[Railroad Sounds]
[Train Tracks]
[City Noise]
[Industrial Sounds]
[River Sounds]
[Flowing Water]
[Rainfall]
[Thunderstorms]
[Soft Breeze]
[Wind Howling]
[Natural Ambience]
[Shouting]
[Reverb]
[Echo/Delay]
[Distortion]
[Flanger Effects]
[Lo-fi Crackling]
[Vinyl Record Sounds]
[Daytime Atmosphere]
[Ocean Waves]
[Church Bells]
[Creaking Doors]
[Creaking Wood]
```

**Subtotal Section D: ~62 entries** — but the list is mixed:
- **Pure SFX (the bulk):** `[Beeping]`, `[Sighs]`, `[Footsteps]`, `[Gunshot]`, etc.
- **Atmosphere variants:** `[Nighttime Atmosphere]`, `[Daytime Atmosphere]`, `[Natural Ambience]`, `[Soft Breeze]`, `[Wind Howling]` — semantic overlap with Section 5 Atmosphere/Mood; reconciliation needs to decide whether to treat as Effect or Mood.
- **Production effects:** `[Reverb]`, `[Echo/Delay]`, `[Distortion]`, `[Flanger Effects]`, `[Lo-fi Crackling]`, `[Vinyl Record Sounds]` — these are technically *Effect/Production* category, not Sound Effects. Reconciliation needs to remap these.
- **Duplicates with prior sections / tags.json:** `[Whistling]`, `[Clapping]`, `[Applause]` (likely already covered).
- **Slash-separated names:** `[Echo/Delay]` — slash inside bracket is unusual; canonical alternatives are `[Echo]` + `[Delay]` separately, OR `[Echo/Delay]` as-is. Reconciliation needs to decide.

## Section E — Total cheat-sheet counts

| Section | Entries | Notes |
|---|---|---|
| A — Example song | 0 unique new | All tags illustrated are duplicated in Section B. |
| B — Numbered categories | 49 | Clean, canonical entries. |
| C — Perfect Studio Vocals | 1 candidate | `[Melismatic Runs]` — needs ADD/MERGE/SKIP decision. |
| D — Sound Effects unstructured | ~62 | Mixed category; needs remap during reconciliation. |
| **Total unique metatag candidates** | **~112** | Cross-reconcile vs ~115 existing in `tags.json`. |

## Section F — Notable patterns for reconciliation methodology

1. **Multi-bracket line-sharing** (e.g. `[Pre-Chorus] [Choir - add choir vocals]`) — already supported in v1.1 inline insertion + v1.3 Shift+click. No new behavior needed.
2. **Bracket-content descriptions** (e.g. `[Spoken Word- create a section with spoken vocals]`) — content inside brackets is freeform per Suno. Not a tag-system change.
3. **Bracket-content modifiers** (e.g. `[Chorus, repeated twice]`) — same; freeform.
4. **`[Tag]:` markdown speaker labels** — doc-formatting only; bracket token is just `[Tag]`.
5. **Slash-separated names** (e.g. `[Echo/Delay]`) — reconciliation decision: keep as-is or split.
6. **Near-duplicates** (e.g. `Whisper` cheat-sheet vs `Whispered` existing tags.json; `Hook` cheat-sheet vs `Hook` existing vs `Catchy Hook` cheat-sheet) — primary MERGE candidates.

## Section G — Reconciliation decisions are deferred

This file is **source material**, not the decision table. The B-SUNO-005 spec + plan packet (see `j:\SunoMetatagApp\docs\specs\2026-05-26-suno-metatag-v1.4-cheatsheet-reconciliation.md` and matching plan) defines the methodology; the actual ADD / MERGE / SKIP decisions are produced as the execution output and ratified by Lead before any `tags.json` mutation lands.
