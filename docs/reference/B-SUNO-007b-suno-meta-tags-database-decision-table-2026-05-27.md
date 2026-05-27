# B-SUNO-007b Decision Table - Suno Meta Tags Database (2026-05-27)

Source:
- `\\WolfenNAS\docker\hermes\Vaults\Ideaverse\Atlas\wiki\concepts\suno-meta-tags-database.md`

Reference baseline:
- `j:\SunoMetatagApp\src\SunoMetatagApp\Resources\tags.json`

Scope:
- Tags detected in the source that are **not exact bracket matches** in current `tags.json`.
- Decisions are `ADD`, `ALIAS`, or `SKIP`.

## Summary

- Candidate tags reviewed: **54**
- `ADD`: **39**
- `ALIAS`: **10**
- `SKIP`: **5**

Note: Initial draft of this Summary cited `ADD: 35 / ALIAS: 12 / SKIP: 7`, which did not match the row-level Decision Table below (39 / 10 / 5). Reconciled at v1.15 r1 absorption (Lead-directed) to the row-level authoritative breakdown.

## Decision Table

| Tag | Decision | Canonical Target (for ALIAS) | Rationale |
|---|---|---|---|
| `[Aggressive]` | ALIAS | `[Mood: Aggressive]` | Already represented with mood prefix; avoid duplicate semantic token. |
| `[Analog]` | ADD | - | Distinct production texture term; useful shorthand. |
| `[Bass]` | ADD | - | Generic instrument cue not currently present. |
| `[brackets]` | SKIP | - | False positive from prose, not a metatag. |
| `[Brass]` | ADD | - | Common instrumentation cue; absent today. |
| `[Building Energy]` | ALIAS | `[Energy: Building]` | Same intent as existing energy-prefixed tag. |
| `[Calm]` | SKIP | - | Not in source core tables; too vague/overlapping with peaceful/chill. |
| `[Clean]` | ADD | - | Useful production direction (clean mix/tone). |
| `[Cynical]` | ADD | - | Distinct lyrical/emotional posture in mood set. |
| `[Dark Atmosphere]` | ADD | - | Clear atmospheric instruction; not equivalent to `[Mood: Dark]`. |
| `[Dense]` | ADD | - | Useful arrangement-density instruction. |
| `[Digital]` | ADD | - | Distinct production texture cue vs analog. |
| `[Distorted]` | ADD | - | Broad distortion cue distinct from `[Effect: Distortion]`. |
| `[Dreamy]` | ALIAS | `[Atmosphere: Dreamy]` | Existing canonical form already present. |
| `[Dry]` | ADD | - | Valuable production direction opposite heavy reverb. |
| `[Emotional Climax]` | ADD | - | Strong dynamics/arrangement cue; not present. |
| `[Emotional]` | SKIP | - | Overly generic and ambiguous; low control value. |
| `[Ethereal]` | ADD | - | Common atmospheric cue; distinct from dreamy. |
| `[Euphoric]` | ALIAS | `[Mood: Euphoric]` | Existing canonical form already present. |
| `[Explosive]` | ALIAS | `[Energy: Explosive]` | Existing canonical form already present. |
| `[Falling Tension]` | ADD | - | Useful post-climax dynamic transition cue. |
| `[Gradual Swell]` | ADD | - | Distinct dynamics instruction; complements crescendo. |
| `[Gritty]` | ADD | - | Common vocal/texture cue absent in current set. |
| `[Harmonized Chorus]` | ADD | - | Frequently used vocal arrangement instruction. |
| `[Heavy Reverb]` | ADD | - | Concrete production cue absent in canonical effect set. |
| `[Hi-Fi]` | ADD | - | Distinct quality/production cue absent today. |
| `[High Energy]` | ALIAS | `[Mood: High Energy]` | Existing canonical form already present. |
| `[Hopeful]` | ADD | - | Distinct mood cue not currently in set. |
| `[Intense]` | ADD | - | Useful sustained-energy instruction; distinct from explosive. |
| `[Intimate]` | ADD | - | Common vocal/mood cue absent in current set. |
| `[Layered]` | ADD | - | Useful shorthand distinct from `[Layered Arrangement]`. |
| `[Low Energy]` | ADD | - | Complement to high-energy cues; currently missing. |
| `[Medium Energy]` | ADD | - | Useful mid-level energy cue for pacing control. |
| `[Melancholic]` | ALIAS | `[Mood: Melancholic]` | Existing canonical form already present. |
| `[Melismatic]` | ALIAS | `[Melisma]` | Same vocal technique already represented. |
| `[Minimalist]` | ADD | - | Useful arrangement constraint cue. |
| `[Mysterious]` | ADD | - | Distinct mood cue currently missing. |
| `[Nostalgic]` | ALIAS | `[Mood: Nostalgic]` | Existing canonical form already present. |
| `[Orchestral Swell]` | ADD | - | Distinct dynamics/orchestration cue. |
| `[Organ]` | ADD | - | Generic organ cue complements specific `[Hammond Organ]`. |
| `[Peaceful]` | ADD | - | Distinct mood cue not currently in set. |
| `[Playful]` | ADD | - | Distinct mood cue absent today. |
| `[Quiet Arrangement]` | ADD | - | Useful explicit arrangement-density cue. |
| `[Romantic]` | ALIAS | `[Mood: Romantic]` | Existing canonical form already present. |
| `[Saxophone]` | ADD | - | Common instrument cue absent (only sax solo exists). |
| `[Sensual]` | ADD | - | Distinct mood cue absent in current set. |
| `[Slow Down]` | ADD | - | Valuable transition cue not currently represented. |
| `[Sparse]` | SKIP | - | Redundant with proposed `[Minimalist]` / `[Quiet Arrangement]`. |
| `[Strings]` | ADD | - | Common ensemble instrumentation cue absent today. |
| `[Strong]` | SKIP | - | Too vague; overlaps with powerful/belted/intense. |
| `[Tense]` | ADD | - | Distinct mood/dynamics cue currently missing. |
| `[Unsettling]` | ADD | - | Distinct tonal/mood cue for dark/suspense use. |
| `[Vintage Warmth]` | ADD | - | Valuable production texture cue. |
| `[Yodel]` | ADD | - | Niche but valid vocal technique; explicit user value. |

## Recommended Execution Order

1. Implement `ADD` tags into `tags.json` with category mapping (`Mood`, `Vocal`, `Instrument`, `Effect`, `Production`).
2. Implement `ALIAS` normalization in search/filter and insertion resolution so non-prefixed user input maps to canonical bracket tags.
3. Keep `SKIP` tags excluded unless user requests explicit inclusion.

## Notes

- This table is additive to prior B-SUNO-007 curation artifacts and should be merged with existing decision history before final landing.
- Because the source includes community-discovered conventions, behavior should be treated as probabilistic and validated with user smoke tests.
