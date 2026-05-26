# B-SUNO-005 Decision Table — Cheat-Sheet vs tags.json Reconciliation

**Status:** draft (Lead-ratifiable at plan-phase review)
**Cheat-sheet source:** [`suno-cheat-sheet-2026-05-26.md`](suno-cheat-sheet-2026-05-26.md)
**tags.json baseline:** 124 entries across 6 categories (Structure / Vocal / Instrument / Mood / Effect / Production)
**Decision model:** ADD / MERGE / SKIP (per spec §2)
**MERGE semantics:** no-tags.json mutation; documentation acknowledgement only (alias support deferred to B-008)
**Final counts (as-landed at T1 commit `301c672`):** ADD 75 / MERGE 16 / SKIP 21 → tags.json grows 124 → 199 (corrected from initial draft totals of 74/15/21/198 after T2 grep-recount of the as-landed file; D.1 subtotal had a +1 off-by-one)

## How to read this table

- **Cheat-sheet entry** column shows the verbatim bracket from the cheat sheet (section + line).
- **Existing tags.json match** column lists any existing entries that have semantic overlap.
- **Decision** column is one of ADD / MERGE / SKIP per the model in spec §2.
- **Target category** column is the category for ADD decisions (existing category or new `SFX`).
- **New bracket** column shows the canonical bracket for ADD entries (typically identical to cheat-sheet entry; occasionally normalized).
- **Rationale** column is the one-line justification.

---

## Section B.1 — Song Structure (11 entries)

| Cheat-sheet entry | Existing match in tags.json | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Intro]` | `[Intro]` (Structure, line 2) | SKIP | — | — | Already exists verbatim. |
| `[Verse]` | `[Verse]` (Structure, line 3) | SKIP | — | — | Already exists. |
| `[Pre-Chorus]` | `[Pre-Chorus]` (Structure, line 6) | SKIP | — | — | Already exists. |
| `[Chorus]` | `[Chorus]` (Structure, line 7) | SKIP | — | — | Already exists. |
| `[Post-Chorus]` | `[Post-Chorus]` (Structure, line 8) | SKIP | — | — | Already exists. |
| `[Bridge]` | `[Bridge]` (Structure, line 9) | SKIP | — | — | Already exists. |
| `[Outro]` | `[Outro]` (Structure, line 10) | SKIP | — | — | Already exists. |
| `[Hook]` | `[Hook]` (Structure, line 19) | SKIP | — | — | Already exists. |
| `[Break]` | `[Break]` (Structure, line 14) | SKIP | — | — | Already exists. |
| `[Fade Out]` | `[Fade Out]` (Structure, line 27) | SKIP | — | — | Already exists. |
| `[Fade In]` | `[Fade In]` (Structure, line 26) | SKIP | — | — | Already exists. |

**Subtotal B.1:** ADD 0 / MERGE 0 / SKIP 11.

## Section B.2 — Instrumental (6 entries)

| Cheat-sheet entry | Existing match | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Instrumental]` | `[Instrumental]` (Structure, line 12) | SKIP | — | — | Already exists. |
| `[Guitar Solo]` | `[Guitar Solo]` (Structure, line 22) | SKIP | — | — | Already exists. |
| `[Piano Solo]` | none ([Grand Piano], [Electric Piano] are Instrument category) | **ADD** | Structure | `[Piano Solo]` | Parallel to existing `[Guitar Solo]`, `[Saxophone Solo]`. |
| `[Drum Solo]` | none ([Drum Break] line 24, [Drum Fill] line 25 are related but distinct) | **ADD** | Structure | `[Drum Solo]` | Parallel to other solos; distinct from break/fill semantics. |
| `[Bass Solo]` | none | **ADD** | Structure | `[Bass Solo]` | Parallel to other solos. |
| `[Instrumental Break]` | `[Instrumental]` (line 12) + `[Break]` (line 14); could be v1.3 stacked `[Instrumental \| Break]` | **ADD** | Structure | `[Instrumental Break]` | Common Suno usage; one-click affordance preferred over v1.3 stacked syntax. |

**Subtotal B.2:** ADD 4 / MERGE 0 / SKIP 2.

## Section B.3 — Vocal (8 entries)

| Cheat-sheet entry | Existing match | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Male Vocal]` | none | **ADD** | Vocal | `[Male Vocal]` | Suno-canonical voice-gender directive; commonly used. |
| `[Female Vocal]` | none | **ADD** | Vocal | `[Female Vocal]` | Suno-canonical voice-gender directive. |
| `[Duet]` | none ([Call and Response] line 56 is related but distinct) | **ADD** | Vocal | `[Duet]` | Distinct concept (two vocalists, often male/female). |
| `[Choir]` | `[Choir]` (Vocal, line 58) | SKIP | — | — | Already exists. |
| `[Spoken Word]` | `[Spoken Word]` (Vocal, line 33) | SKIP | — | — | Already exists. |
| `[Harmonies]` | `[Harmonies]` (Vocal, line 49) | SKIP | — | — | Already exists. |
| `[Vulnerable Vocals]` | none ([Breathy] line 42, [Airy] line 48 are distinct vocal techniques) | **ADD** | Vocal | `[Vulnerable Vocals]` | Distinct emotional-delivery directive; not covered by existing entries. |
| `[Whisper]` | `[Whispered]` (Vocal, line 29) — past-tense spelling | MERGE | — | — | Same concept, different spelling. v1.4 MERGE = no-op (keep `[Whispered]` canonical); alias support deferred to B-008. |

**Subtotal B.3:** ADD 4 / MERGE 1 / SKIP 3.

## Section B.4 — Specific Elements (6 composite entries)

| Cheat-sheet entry | Existing parts | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Catchy Hook]` | `[Hook]` exists; `[Catchy]` does not | **ADD** | Structure | `[Catchy Hook]` | One part new; ADD as composite for one-click access. |
| `[Emotional Bridge]` | `[Bridge]` exists; `[Emotional]` does not | **ADD** | Structure | `[Emotional Bridge]` | One part new; ADD as composite. |
| `[Powerful Outro]` | `[Outro]` (Structure, line 10) + `[Powerful]` (Vocal, line 34) both exist | **SKIP** | — | — | Both parts standalone exist; user expresses via v1.3 stacked `[Outro \| Powerful]`. Redundant entry. |
| `[Soft Intro]` | `[Intro]` (line 2) + `[Soft]` (Vocal, line 30) both exist | **SKIP** | — | — | Both parts exist; v1.3 stacked syntax `[Intro \| Soft]` covers this. Redundant. |
| `[Melodic Interlude]` | `[Interlude]` (Structure, line 13) exists; `[Melodic]` does not | **ADD** | Structure | `[Melodic Interlude]` | One part new; ADD as composite. |
| `[Percussion Break]` | `[Break]` (line 14) + `[Hand Percussion]` (Instrument, line 85) close match but not exact | **ADD** | Structure | `[Percussion Break]` | Distinct concept (percussion-only break, not just any break); ADD. |

**Subtotal B.4:** ADD 4 / MERGE 0 / SKIP 2.

## Section B.5 — Atmosphere and Mood (9 entries)

| Cheat-sheet entry | Existing match | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Eerie Whispers]` | none | **ADD** | Mood | `[Eerie Whispers]` | New canonical Suno form (bare bracket per spec §3.4). |
| `[Ghostly Echoes]` | none | **ADD** | Mood | `[Ghostly Echoes]` | New. |
| `[Ominous Drone]` | none | **ADD** | Mood | `[Ominous Drone]` | New. |
| `[Spectral Melody]` | none | **ADD** | Mood | `[Spectral Melody]` | New. |
| `[Melancholic Atmosphere]` | `[Mood: Melancholic]` (line 100) — semantically equivalent | MERGE | — | — | Same concept, different naming convention. Keep `[Mood: Melancholic]` canonical; document cheat-sheet form for B-008 alias support. |
| `[Euphoric Build]` | `[Mood: Euphoric]` (line 99) + cheat-sheet `[Building Intensity]` separate — composite distinct | **ADD** | Mood | `[Euphoric Build]` | Composite distinct from standalone `[Mood: Euphoric]` (adds "build" aspect). ADD. |
| `[Tense Underscore]` | none | **ADD** | Mood | `[Tense Underscore]` | New. |
| `[Serene Ambience]` | `[Atmosphere: Dreamy]` (line 108) is closest but distinct | **ADD** | Mood | `[Serene Ambience]` | "Serene" + "ambience" semantically distinct from "dreamy". ADD. |
| `[Nostalgic Tones]` | `[Mood: Nostalgic]` (line 102) — semantically equivalent | MERGE | — | — | Same concept; keep `[Mood: Nostalgic]` canonical. |

**Subtotal B.5:** ADD 7 / MERGE 2 / SKIP 0.

## Section B.6 — Dynamic and Progression (9 entries)

| Cheat-sheet entry | Existing match | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Building Intensity]` | `[Energy: Building]` (line 112) — partial overlap | MERGE | — | — | Same concept; keep `[Energy: Building]` canonical. |
| `[Climactic]` | none ([Energy: Explosive] line 111 is related but distinct) | **ADD** | Effect | `[Climactic]` | Distinct from "explosive" (a high point, not a burst). |
| `[Emotional Swell]` | `[Swell]` (Effect, line 123) — partial overlap | MERGE | — | — | Same concept (emotional descriptor of swell); keep `[Swell]` canonical. |
| `[Layered Arrangement]` | none | **ADD** | Effect | `[Layered Arrangement]` | New production directive. |
| `[Orchestral Build]` | none ([Orchestra] does not exist in tags.json) | **ADD** | Effect | `[Orchestral Build]` | New; valuable composite for orchestral genres. |
| `[Stripped Back]` | none | **ADD** | Effect | `[Stripped Back]` | New; minimal-arrangement directive. |
| `[Sudden Break]` | `[Break]` (Structure, line 14) is more general | **ADD** | Effect | `[Sudden Break]` | Distinct from generic break (emphasizes abruptness as effect). |
| `[Crescendo]` | `[Crescendo]` (Effect, line 124) | SKIP | — | — | Already exists. |
| `[Decrescendo]` | `[Decrescendo]` (Effect, line 125) | SKIP | — | — | Already exists. |

**Subtotal B.6:** ADD 5 / MERGE 2 / SKIP 2.

## Section C — Perfect Studio Vocals (1 candidate)

| Cheat-sheet entry | Existing match | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Melismatic Runs]` (from raw "[ Melismatic Runs ...]" pattern) | `[Melisma]` (Vocal, line 52), `[Vocal Run]` (Vocal, line 51) | MERGE | — | — | Same concept covered by combination of existing `[Melisma]` + `[Vocal Run]`. User can stack via v1.3 `[Melisma \| Vocal Run]` or use existing standalone entries. Optional MERGE-as-no-op; could be ADD if Lead prefers explicit composite. |

**Subtotal C:** ADD 0 / MERGE 1 / SKIP 0.

## Section D — Sound Effects unstructured (~62 entries)

### D.1 — Pure SFX (target: new `SFX` category)

| Cheat-sheet entry | Existing match | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Beeping]` | none | **ADD** | SFX | `[Beeping]` | New SFX. |
| `[Sighs]` | none | **ADD** | SFX | `[Sighs]` | New SFX. |
| `[Footsteps]` | none | **ADD** | SFX | `[Footsteps]` | New SFX. |
| `[Beep]` | (overlaps `[Beeping]`) | MERGE | — | — | Same concept as `[Beeping]`; keep `[Beeping]` canonical (matches the multiple-sound pattern of `[Crickets Chirping]` etc.). |
| `[Gunshot]` | none | **ADD** | SFX | `[Gunshot]` | New SFX. |
| `[Wind]` | none | **ADD** | SFX | `[Wind]` | New SFX. |
| `[Rain]` | none | **ADD** | SFX | `[Rain]` | New SFX. |
| `[Door Shutting]` | none | **ADD** | SFX | `[Door Shutting]` | New SFX. |
| `[Clapping]` | none | **ADD** | SFX | `[Clapping]` | New SFX. |
| `[Thunder]` | none | **ADD** | SFX | `[Thunder]` | New SFX. |
| `[Birdsong]` | none | **ADD** | SFX | `[Birdsong]` | New SFX. |
| `[Waves]` | none | **ADD** | SFX | `[Waves]` | New SFX (different from `[Ocean Waves]` below — generic vs specific). |
| `[Siren]` | none | **ADD** | SFX | `[Siren]` | New SFX. |
| `[Clock Ticking]` | none | **ADD** | SFX | `[Clock Ticking]` | New SFX. |
| `[Dog Barking]` | none | **ADD** | SFX | `[Dog Barking]` | New SFX. |
| `[Car Engine]` | none | **ADD** | SFX | `[Car Engine]` | New SFX. |
| `[Crowd Cheering]` | none | **ADD** | SFX | `[Crowd Cheering]` | New SFX. |
| `[Heartbeat]` | none | **ADD** | SFX | `[Heartbeat]` | New SFX. |
| `[Bell Ringing]` | none | **ADD** | SFX | `[Bell Ringing]` | New SFX. |
| `[Glass Breaking]` | none | **ADD** | SFX | `[Glass Breaking]` | New SFX. |
| `[Train Whistle]` | none | **ADD** | SFX | `[Train Whistle]` | New SFX. |
| `[Laughing]` | none | **ADD** | SFX | `[Laughing]` | New SFX (distinct from cheat-sheet `[Audience Cheering]` and `[Crowd Cheering]`). |
| `[Whistling]` | none | **ADD** | SFX | `[Whistling]` | New SFX. |
| `[Horse Galloping]` | none | **ADD** | SFX | `[Horse Galloping]` | New SFX. |
| `[Fire Crackling]` | none | **ADD** | SFX | `[Fire Crackling]` | New SFX. |
| `[Helicopter]` | none | **ADD** | SFX | `[Helicopter]` | New SFX. |
| `[Typing]` | none | **ADD** | SFX | `[Typing]` | New SFX. |
| `[Crickets Chirping]` | none | **ADD** | SFX | `[Crickets Chirping]` | New SFX. |
| `[Camera Shutter]` | none | **ADD** | SFX | `[Camera Shutter]` | New SFX. |
| `[Applause]` | none | **ADD** | SFX | `[Applause]` | New SFX. |
| `[Snapping Fingers]` | none | **ADD** | SFX | `[Snapping Fingers]` | New SFX. |
| `[Telephone Ringing]` | none | **ADD** | SFX | `[Telephone Ringing]` | New SFX. |
| `[Audience Cheering]` | (overlaps `[Crowd Cheering]`) | MERGE | — | — | Same concept; keep `[Crowd Cheering]` canonical (more common phrasing). |
| `[Traffic Noise]` | none | **ADD** | SFX | `[Traffic Noise]` | New SFX. |
| `[Construction Sounds]` | none | **ADD** | SFX | `[Construction Sounds]` | New SFX. |
| `[Urban Street Noise]` | none | **ADD** | SFX | `[Urban Street Noise]` | New SFX. |
| `[Footsteps on Gravel]` | (overlaps `[Footsteps]`) | **ADD** | SFX | `[Footsteps on Gravel]` | Distinct surface variant; common in Suno SFX usage. ADD as separate. |
| `[Footsteps on Pavement]` | (overlaps `[Footsteps]`) | **ADD** | SFX | `[Footsteps on Pavement]` | Distinct surface variant. ADD as separate. |
| `[Railroad Sounds]` | none | **ADD** | SFX | `[Railroad Sounds]` | New SFX. |
| `[Train Tracks]` | (overlaps `[Railroad Sounds]`) | MERGE | — | — | Same concept; keep `[Railroad Sounds]` canonical (broader). |
| `[City Noise]` | (overlaps `[Urban Street Noise]`) | MERGE | — | — | Same concept; keep `[Urban Street Noise]` canonical (more specific). |
| `[Industrial Sounds]` | none | **ADD** | SFX | `[Industrial Sounds]` | New SFX. |
| `[River Sounds]` | none | **ADD** | SFX | `[River Sounds]` | New SFX. |
| `[Flowing Water]` | (overlaps `[River Sounds]`) | MERGE | — | — | Same concept; keep `[River Sounds]` canonical (or could keep both — Lead-ratifiable). Planner-default: MERGE (one canonical name). |
| `[Rainfall]` | (overlaps `[Rain]`) | MERGE | — | — | Same concept; keep `[Rain]` canonical (shorter). |
| `[Thunderstorms]` | (overlaps `[Thunder]`) | MERGE | — | — | Same concept; keep `[Thunder]` canonical (shorter). |
| `[Soft Breeze]` | (overlaps `[Wind]`) | **ADD** | SFX | `[Soft Breeze]` | Distinct intensity from `[Wind]`; ADD as separate. |
| `[Wind Howling]` | (overlaps `[Wind]`) | **ADD** | SFX | `[Wind Howling]` | Distinct intensity from `[Wind]`; ADD as separate. |
| `[Natural Ambience]` | none | **ADD** | SFX | `[Natural Ambience]` | New SFX (background nature sound bed). |
| `[Shouting]` | none | **ADD** | SFX | `[Shouting]` | New SFX (vocal-adjacent but SFX-class per cheat sheet section). |
| `[Daytime Atmosphere]` | none | **ADD** | SFX | `[Daytime Atmosphere]` | New SFX-atmosphere variant. |
| `[Nighttime Atmosphere]` | none | **ADD** | SFX | `[Nighttime Atmosphere]` | New SFX-atmosphere variant. |
| `[Ocean Waves]` | (overlaps `[Waves]`) | **ADD** | SFX | `[Ocean Waves]` | Distinct from generic `[Waves]` (specific ocean context). ADD as separate. |
| `[Church Bells]` | (overlaps `[Bell Ringing]`) | **ADD** | SFX | `[Church Bells]` | Distinct context (church bells = specific, bell ringing = generic). ADD as separate. |
| `[Creaking Doors]` | (overlaps `[Door Shutting]`) | **ADD** | SFX | `[Creaking Doors]` | Distinct sound (creaking vs shutting). ADD as separate. |
| `[Creaking Wood]` | none | **ADD** | SFX | `[Creaking Wood]` | New SFX (general wood-creaking ambient sound). |

**Subtotal D.1:** ADD 49 / MERGE 7 / SKIP 0 (corrected from initial draft of 48/6/0 after grep-recount of landed file).

### D.2 — Production effects (target: existing Effect category, mostly MERGE)

| Cheat-sheet entry | Existing match | Decision | Target category | New bracket | Rationale |
|---|---|---|---|---|---|
| `[Reverb]` | `[Effect: Reverb: Hall]` (line 115) | MERGE | — | — | Same concept; existing entry is more specific (Hall reverb). Keep prefix form canonical. |
| `[Echo/Delay]` | `[Effect: Delay: Ping-pong]` (line 116) | SKIP | — | — | Slash inside bracket is malformed Suno syntax; existing prefix form covers delay. SKIP entire entry. |
| `[Distortion]` | `[Effect: Distortion]` (line 117) | MERGE | — | — | Same concept; existing prefix-form entry canonical. |
| `[Flanger Effects]` | none ([Effect: Autopan] line 120 is a different sweep effect) | **ADD** | Effect | `[Effect: Flanger]` | New production effect; use prefix form to match neighbors. |
| `[Lo-fi Crackling]` | `[Effect: Lo-fi]` (line 114) | MERGE | — | — | Same concept; existing entry canonical. |
| `[Vinyl Record Sounds]` | (overlaps `[Effect: Lo-fi]`) | **ADD** | Effect | `[Effect: Vinyl Crackle]` | Distinct from generic lo-fi (specific vinyl-record sound); ADD with prefix form. |

**Subtotal D.2:** ADD 2 / MERGE 3 / SKIP 1.

---

## Totals

| Section | ADD | MERGE | SKIP | Total entries |
|---|---|---|---|---|
| B.1 Song Structure | 0 | 0 | 11 | 11 |
| B.2 Instrumental | 4 | 0 | 2 | 6 |
| B.3 Vocal | 4 | 1 | 3 | 8 |
| B.4 Specific Elements | 4 | 0 | 2 | 6 |
| B.5 Atmosphere/Mood | 7 | 2 | 0 | 9 |
| B.6 Dynamic/Progression | 5 | 2 | 2 | 9 |
| C Perfect Studio Vocals | 0 | 1 | 0 | 1 |
| D.1 Pure SFX | 49 | 7 | 0 | 56 |
| D.2 Production effects | 2 | 3 | 1 | 6 |
| **TOTAL** | **75** | **16** | **21** | **112** |

**Effect on tags.json (as-landed at T1 commit `301c672`):** 124 baseline → **+75 ADD entries** → **199 entries** post-v1.4 (the initial draft totals row of 74/15/21/198 was off by 1 in D.1 ADD/MERGE columns; this was caught at T2 grep-recount and corrected here for source-of-truth alignment).

**New SFX category:** 48 entries.

## Lead-ratification questions

1. **Refined total of 198 vs spec §5 estimate of ~209:** does Lead want the table re-aggressive on ADDs (e.g., ADD `[Train Tracks]` vs MERGE-to-`[Railroad Sounds]`)? Planner-default kept the table conservative on near-duplicates.
2. **Composite specific-elements:** `[Powerful Outro]` and `[Soft Intro]` are planner-SKIPed (express via v1.3 stacked syntax). Lead-ratify or override to ADD?
3. **Footsteps variants (gravel/pavement) ADD vs MERGE:** planner kept both as separate entries (different surfaces = different sounds). Lead-ratify or collapse to single `[Footsteps]`?
4. **Sound-classification ambiguity:** `[Shouting]` is classified SFX (per cheat sheet section D). Could go to Vocal. Lead-ratify or override?
5. **Naming convention for new Effect-category entries:** planner used prefix form (`[Effect: Flanger]`, `[Effect: Vinyl Crackle]`) to match neighbors. Alternative: bare form (`[Flanger]`, `[Vinyl Crackle]`). Lead-ratify.
6. **`[Echo/Delay]` SKIP:** planner SKIPed due to slash character. Lead may want to ADD as `[Echo]` + `[Delay]` separately. Override?

The decision table is **mechanically applicable at T1 execution** once Lead ratifies the rows above.
