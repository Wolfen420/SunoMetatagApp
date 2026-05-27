# awesome-suno-prompts — Source Capture (2026-05-27)

> Immutable evidence snapshot of the awesome-suno-prompts repository state used to populate the v1.8 seed corpus in `Resources/prompts.json`. Do not edit after capture.

## Repository metadata

- **Repo:** [`naqashmunir21/awesome-suno-prompts`](https://github.com/naqashmunir21/awesome-suno-prompts)
- **License:** Creative Commons Zero v1.0 Universal (CC0-1.0) — no attribution legally required; SunoMetatagApp credits the source voluntarily as project hygiene.
- **Default branch:** `main`
- **Description:** "🎵 A curated collection of 1000+ professional Suno AI style prompts for creating chart-quality songs. Organized by genre with production tips. Free & open source."
- **Capture date:** 2026-05-27 (UTC)
- **Capture commit SHA on `main`:** `e1d1247bd26f896127011d3bbc2ba8599d54960d`
- **Capture method:** `gh api -H "Accept: application/vnd.github.raw" repos/naqashmunir21/awesome-suno-prompts/contents/prompts/<file>.md`

## File listing (`prompts/` directory)

| File | Size (bytes) | Blob SHA |
|---|---:|---|
| `country.md` | 8,580 | `8a4e2860ec08a723ebb5de20d900b4ef252907fd` |
| `edm.md` | 10,240 | `ce43ea2c4df8c2f6afebde5d09c27debb38d0fed` |
| `hip-hop.md` | 9,731 | `75dc76a57cd498ef0fd83c520e238dfd735742fb` |
| `indie.md` | 9,931 | `e5f5bbab2a84ab13df41f264eebe427183028d28` |
| `jazz-blues.md` | 10,063 | `c556e9173d7a0cc6cb1c28e137762d29f24c211e` |
| `pop.md` | 9,558 | `c943feab8c2cd9c9bef0016693040cf2238664e5` |
| `rnb-soul.md` | 8,876 | `f248a5f77e29da5203ef0b7449046a07103da361` |
| `rock.md` | 10,105 | `408f8f1ae92e393f8512028aef0358e615d7291b` |

## Selection criteria applied (per spec §3.4)

- 2 entries per genre × 8 genres = 16 total.
- 1 high-energy anchor (Energy >= 7) + 1 ballad/chill anchor (Energy <= 6) per genre.
- Prefer entries with BPM + Key annotation and explicit Use Case.
- Avoid two entries from the same SubGenre within the same Genre.
- Avoid bodies that embed commercial promotion links (e.g. `songaifarm.com` cross-references).
- `Body` field stores the verbatim prompt text from the triple-backtick block only — surrounding markdown (headings, attribution links, "Use Case" / "Energy" labels, navigation footers) is excluded.

## Selected seed prompts (16)

Anchor URLs are GitHub auto-generated H3 slugs.

### 1. Pop — Upbeat Dance Pop — "Modern Pop Anthem (Female Vocals)"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/pop.md#modern-pop-anthem-female-vocals
- **Energy:** 9/10 | **Suno Version:** V5 | **Notable Feature:** Perfect for social media campaigns

```
Infectious pop anthem, female powerhouse vocals with layered harmonies,
pulsing 808 bass, synth-wave layers, euphoric build-ups, 
radio-ready polish, modern production, vocal chops, 
stadium-ready energy, BPM: 128, Key: C Major
```

### 2. Pop — Emotional Ballads — "Piano-Driven Power Ballad"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/pop.md#piano-driven-power-ballad
- **Energy:** 5/10 | **Suno Version:** V4.5 or V5 | **Notable Feature:** Perfect for emotional moments

```
Piano-driven emotional ballad, raw vulnerable vocals,
builds from intimate verse to soaring chorus,
subtle string arrangements, modern radio production,
reverb on vocals, stripped-down bridge, 
powerful final chorus, BPM: 72, Key: G Major
```

### 3. Rock — Stadium Rock Anthems — "Epic Arena Anthem"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/rock.md#epic-arena-anthem
- **Energy:** 9/10 | **Suno Version:** V5 | **Notable Feature:** Perfect for "fists-in-the-air" moments

```
Power rock anthem, soaring stadium vocals, massive guitar wall of sound,
thunderous drum fills, crowd-chant worthy chorus, anthem hooks,
crunchy distorted rhythm guitars, searing lead guitar solo,
BPM: 140, Key: E Major, radio-ready polish with arena reverb
```

### 4. Rock — Grunge/Alternative — "Grunge Ballad"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/rock.md#grunge-ballad
- **Energy:** 6/10 | **Suno Version:** Both

```
Melancholic grunge ballad, whispered verses building to screaming chorus,
clean-tone arpeggios verses, walls of distortion on chorus,
raw emotional delivery, tape saturation warmth,
BPM: 82, Key: E Minor
```

### 5. EDM — Festival Bangers — "Big Room House Anthem"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/edm.md#big-room-house-anthem
- **Energy:** 10/10 | **Suno Version:** V5 | **Notable Feature:** Bone-shaking bass drop designed for massive sound systems

```
Festival big room house, massive riser build-up, explosive drop,
supersaws layered thick, sidechain compression pumping,
euphoric lead melody, crowd-chant ready, stadium energy,
BPM: 128, Key: C Minor
```

### 6. EDM — Future Bass — "Chill Future Bass"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/edm.md#chill-future-bass
- **Energy:** 5/10 | **Suno Version:** V5

```
Chill future bass, lo-fi influenced, warm analog synths,
laid-back drums, atmospheric vocals, relaxed tempo,
study/work music vibe, emotional but not aggressive,
BPM: 140, Key: G Major
```

### 7. Hip-Hop — Trap Bangers — "Modern Trap Anthem"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/hip-hop.md#modern-trap-anthem
- **Energy:** 9/10 | **Suno Version:** V5 | **Notable Feature:** Signature rolling hi-hats define modern trap sound

```
Hard trap beat, rolling hi-hats (triplet flow), thunderous 808 bass slides,
dark atmospheric pads, ad-libs (skrrt, yeah), auto-tuned melodic hooks,
tight snare, sparse melodic elements, modern radio production,
BPM: 140, Key: C Minor
```

### 8. Hip-Hop — Boom Bap Classics — "90s Boom Bap"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/hip-hop.md#90s-boom-bap
- **Energy:** 5/10 | **Suno Version:** V4.5 or V5 | **Notable Feature:** Authentic vinyl sample aesthetic

```
Classic boom bap, dusty vinyl samples, hard-hitting kick and snare,
jazzy piano loops, vinyl crackle texture, authentic 90s drum breaks,
minimal bass line, golden era hip-hop vibe, BPM: 92, Key: D Minor
```

### 9. Indie — Indie Rock — "Jangly Indie Rock"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/indie.md#jangly-indie-rock
- **Energy:** 7/10 | **Suno Version:** V5 | **Notable Feature:** Signature jangly guitar tone

```
Jangly indie rock, clean Rickenbacker-style guitars,
melodic bass lines, crisp drums, warm vocals,
Smiths/R.E.M. influence, authentic indie charm,
BPM: 138, Key: D Major
```

### 10. Indie — Bedroom Pop — "Melancholic Bedroom Indie"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/indie.md#melancholic-bedroom-indie
- **Energy:** 3/10 | **Suno Version:** V5

```
Melancholic bedroom indie, reverb-soaked vocals,
minimal acoustic guitar, tape hiss, vulnerable lyrics mood,
late-night introspective vibe, raw emotional honesty,
BPM: 75, Key: E Minor
```

### 11. Jazz-Blues — Bebop/Hard Bop — "Hard Bop Groove"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/jazz-blues.md#hard-bop-groove
- **Energy:** 8/10 | **Suno Version:** V5

```
Hard bop, soulful bluesy feel, gospel influences,
piano comping with Hammond organ, driving swing rhythm,
Art Blakey/Horace Silver vibe, sophisticated groove,
BPM: 180, Key: F Minor
```

### 12. Jazz-Blues — Smooth Jazz — "Smooth Jazz Ballad"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/jazz-blues.md#smooth-jazz-ballad
- **Energy:** 3/10 | **Suno Version:** Both

```
Smooth jazz ballad, intimate sax or guitar lead,
lush chord progressions, soft piano comping,
romantic atmosphere, late-night radio vibe,
BPM: 70, Key: F Major
```

### 13. R&B-Soul — Classic Soul — "Motown Soul"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/rnb-soul.md#motown-soul
- **Energy:** 8/10 | **Suno Version:** V4.5 or V5 | **Notable Feature:** Authentic 60s Motown production

```
Classic Motown soul, live band feel, tambourine prominent,
string arrangements, upbeat four-on-floor, handclaps,
vintage warmth, joyful energy, 60s nostalgia,
BPM: 120, Key: G Major
```

### 14. R&B-Soul — Quiet Storm — "Smooth Quiet Storm"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/rnb-soul.md#smooth-quiet-storm
- **Energy:** 3/10 | **Suno Version:** Both | **Notable Feature:** Perfect for intimate romantic moments

```
Quiet storm, silky smooth vocals, lush string arrangements,
gentle Rhodes piano, soft brushed drums, romantic atmosphere,
late-night radio vibe, intimate production,
BPM: 70, Key: Bb Major
```

### 15. Country — Modern Pop-Country — "Radio Pop-Country Hit"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/country.md#radio-pop-country-hit
- **Energy:** 8/10 | **Suno Version:** V5 | **Notable Feature:** Pop-polished production with country instrumentation

```
Modern pop-country, twangy male vocals with slight rasp,
bright acoustic guitar, subtle pedal steel accents, stadium drums,
anthemic sing-along chorus, polished radio production,
BPM: 120, Key: G Major
```

### 16. Country — Traditional Country — "Country Waltz"

- **Source:** https://github.com/naqashmunir21/awesome-suno-prompts/blob/main/prompts/country.md#country-waltz
- **Energy:** 4/10 | **Suno Version:** V5 | **Notable Feature:** Classic 3/4 waltz time signature

```
Country waltz (3/4 time), pedal steel lead, acoustic guitar,
gentle fiddle, romantic flowing melody, vintage production,
dance hall atmosphere, BPM: 90 (3/4), Key: F Major
```

## Selection diversity audit

| Genre | High-energy SubGenre | Low/Ballad SubGenre | Distinct? |
|---|---|---|---|
| Pop | Upbeat Dance Pop | Emotional Ballads | yes |
| Rock | Stadium Rock Anthems | Grunge/Alternative | yes |
| EDM | Festival Bangers | Future Bass | yes |
| Hip-Hop | Trap Bangers | Boom Bap Classics | yes |
| Indie | Indie Rock | Bedroom Pop | yes |
| Jazz-Blues | Bebop/Hard Bop | Smooth Jazz | yes |
| R&B-Soul | Classic Soul | Quiet Storm | yes |
| Country | Modern Pop-Country | Traditional Country | yes |

## Excluded surfaces

- The "Custom Prompts" / `songaifarm.com` cross-promotion sections at the end of each genre file are commercial links and are explicitly excluded from `Body` content per spec §3.4.
- The "Production Tips" sections at the end of each genre file are reference material, not seed prompts, and are not imported.
