---
title: B-SUNO-007 / v1.6 Decision Table — sunoaiwiki Metatag List Reconciliation
type: source
status: planner-draft
authored: 2026-05-27
authored_by: planner (B-SUNO-007 / v1.6)
source_file: docs/reference/suno-metatag-list-source-2026-05-27.md
target_file: src/SunoMetatagApp/Resources/tags.json
target_categories: 5 existing extended (Vocal, Instrument, Production, SFX, Genre)
canonicalization_rules: docs/specs/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md §3.2-§3.7 (inherited unchanged)
v1.6_spec_rules: docs/specs/2026-05-27-suno-metatag-v1.6-sunoaiwiki-metatag-list.md §3.3-§3.6 (cross-category collision policy + split-row decisions)
tags: [sla, suno, metatag, taxonomy, curation, decision-table, planner-draft]
related: [[suno-metatag-list-source-2026-05-27]], [[sunometatag-tag-library]], [[B-SUNO-005-decision-table]], [[B-SUNO-006-decision-table]]
---

# B-SUNO-007 / v1.6 Decision Table — sunoaiwiki Metatag List Reconciliation

> **Planner draft** — pending Lead ratification at r1 review. Totals are planner-counted at draft time; the **T2 grep-recount discipline** verifies actual `tags.json` content count after T1 commit.

## Methodology

For every row in [`suno-metatag-list-source-2026-05-27.md`](./suno-metatag-list-source-2026-05-27.md):

- **Decision:** `ADD` / `MERGE` / `SKIP`
- **Target category:** for ADD rows (Vocal / Instrument / Production / SFX / Genre — no new categories in v1.6)
- **Bracket form:** `[Title Case]` per v1.5 §3.2 (inherited unchanged)
- **Rationale:** cite existing-collision check result, cross-category policy applied, source-respect-vs-category-respect choice

**Cross-category collision policy** (v1.6 §3.3):
- **SKIP-as-canonical-present** — source label canonicalizes to an existing bracket
- **MERGE-cross-category** — source label exists in a different category as a semantically-related variant (data for B-008 alias schema)
- **ADD-new-category** — source label categorizes differently from existing entry AND parallel coexistence is useful

**Split-row decisions** (v1.6 §3.4):
- "Christian & Gospel" → row 13a (Gospel half) + row 13b (Christian half)
- "Dance & Electronic" → row 16a (Dance half) + row 16b (Electronic half)
- Decision-table rows beyond 81 source items: **83 rows total** (81 source + 2 split-row extras).

---

## §1. Sound Effects Meta Tags (18 source items)

| # | Source label | Decision | Target | Bracket form | Rationale |
|---|---|---|---|---|---|
| 1.1 | Barking | ADD | SFX | `[Barking]` | New animal-sound SFX; not in v1.4 SFX. |
| 1.2 | Beeping | SKIP | — | — | `[Beeping]` already present in SFX (v1.4 ADD). |
| 1.3 | Bell dings | ADD | SFX | `[Bell Dings]` | Distinct from existing `[Bell Ringing]` (multiple individual dings vs continuous ringing). |
| 1.4 | Birds chirping | MERGE | — | — | Existing `[Birdsong]` (v1.4 ADD) is semantic equivalent. MERGE for B-008 alias data: `[Birds Chirping]` → `[Birdsong]`. |
| 1.5 | Bleep | ADD | SFX | `[Bleep]` | Distinct from `[Beeping]` (censorship-bleep vs continuous beep). |
| 1.6 | Cheering | MERGE | — | — | Existing `[Crowd Cheering]` (v1.4 ADD) is closest semantic match. MERGE for B-008: `[Cheering]` → `[Crowd Cheering]`. |
| 1.7 | Cheers and applause | SKIP | — | — | Composite SFX; users can compose `[Crowd Cheering \| Applause]` via v1.3 stacked syntax. v1.4 precedent for composite SKIPs. |
| 1.8 | Chuckles | ADD | SFX | `[Chuckles]` | New laughter-class SFX; distinct from `[Giggling]` (also v1.6 ADD, see §2). |
| 1.9 | Clapping | SKIP | — | — | **Planner self-correction at T1** (2026-05-27): planner draft had ADD but `[Clapping]` already exists in v1.4 SFX (catched by C5/G5 bracket-collision tests). Decision corrected to SKIP-canonical-present. Mirrors v1.4 D.1 +1 off-by-one discipline + v1.5 T2.5 hygiene pattern. |
| 1.10 | Cough | ADD | SFX | `[Cough]` | New body-sound SFX. |
| 1.11 | Groaning | ADD | SFX | `[Groaning]` | New body/effort-sound SFX. |
| 1.12 | Phone ringing | ADD | SFX | `[Phone Ringing]` | New phone SFX; distinct from existing `[Bell Ringing]` (telephone vs bell). |
| 1.13 | Ringing | ADD | SFX | `[Ringing]` | Generic ringing-tone SFX; distinct from `[Bell Ringing]` (specific) and `[Phone Ringing]` (1.12). |
| 1.14 | Screams | ADD | SFX | `[Screams]` | Existing `[Shouting]` is different vocal action; screams is more anguished/emergency SFX. |
| 1.15 | Sighs | SKIP | — | — | `[Sighs]` already present in SFX (v1.4 ADD). |
| 1.16 | Squawking | ADD | SFX | `[Squawking]` | New bird-sound SFX; distinct from `[Birdsong]` (melodic vs harsh). |
| 1.17 | Whispers | ADD | SFX | `[Whispers]` | Semantically distinct from existing `[Whispered]` (Vocal style for the singer) — `[Whispers]` is a sound effect of others whispering. Different categories, different brackets. |
| 1.18 | Whistling | SKIP | — | — | `[Whistling]` already present in SFX (v1.4 ADD). |

**§1 subtotal:** 11 ADD / 2 MERGE / 5 SKIP (corrected from planner draft 12 ADD / 4 SKIP after T1 Clapping collision detection)

---

## §2. Vocal Expressions Meta Tags (9 source items)

| # | Source label | Decision | Target | Bracket form | Rationale |
|---|---|---|---|---|---|
| 2.1 | Announcer | ADD | Vocal | `[Announcer]` | New voice-type for narration/MC contexts. |
| 2.2 | Audience laughing | ADD | SFX | `[Audience Laughing]` | Crowd-class SFX; not in v1.4 SFX. Distinct from `[Crowd Cheering]`. |
| 2.3 | Female narrator | ADD | Vocal | `[Female Narrator]` | New voice-type; distinct from `[Female Vocal]` (singer) — narrator implies spoken-word narration role. |
| 2.4 | Giggling | ADD | SFX | `[Giggling]` | Light-laughter SFX; distinct from `[Chuckles]` (1.8) which is deeper/restrained. SFX (sound-of-person-giggling) over Vocal (singer-giggle technique). |
| 2.5 | Man | MERGE | — | — | Existing `[Male Vocal]` (v1.4 ADD) is canonical voice-type label. MERGE for B-008: `[Man]` → `[Male Vocal]`. |
| 2.6 | Reporter | ADD | Vocal | `[Reporter]` | New voice-type for news/interview contexts. |
| 2.7 | Woman | MERGE | — | — | Existing `[Female Vocal]` (v1.4 ADD) is canonical voice-type label. MERGE for B-008: `[Woman]` → `[Female Vocal]`. |
| 2.8 | Boy | ADD | Vocal | `[Boy]` | Voice-type per "import-as-source" discipline; distinct from `[Male Vocal]` (adult male). Per v1.5 §3.7 R3 risk note. |
| 2.9 | Girl | ADD | Vocal | `[Girl]` | Voice-type per "import-as-source" discipline; distinct from `[Female Vocal]` (adult female). Per v1.5 §3.7 R3 risk note. |

**§2 subtotal:** 7 ADD / 2 MERGE / 0 SKIP

---

## §3. Static and Other Effects (4 source items)

| # | Source label | Decision | Target | Bracket form | Rationale |
|---|---|---|---|---|---|
| 3.1 | Applause | SKIP | — | — | `[Applause]` already present in SFX (v1.4 ADD). |
| 3.2 | Clears throat | ADD | SFX | `[Clears Throat]` | New body-sound SFX. Title-Case both words. |
| 3.3 | Censored | ADD | Production | `[Censored]` | Production marker (post-production censorship indicator). Production category fits better than SFX. |
| 3.4 | Silence | ADD | Production | `[Silence]` | Production marker (timing/pause indicator). Production category fits better than SFX. |

**§3 subtotal:** 3 ADD / 0 MERGE / 1 SKIP

---

## §4. Structural Tags (4 source items)

| # | Source label | Decision | Target | Bracket form | Rationale |
|---|---|---|---|---|---|
| 4.1 | Chorus | SKIP | — | — | `[Chorus]` already present in Structure (v1 baseline). Exact-match. |
| 4.2 | Intro | SKIP | — | — | `[Intro]` already present in Structure (v1 baseline). Exact-match. |
| 4.3 | Outro | SKIP | — | — | `[Outro]` already present in Structure (v1 baseline). Exact-match. |
| 4.4 | Verse | SKIP | — | — | `[Verse]` already present in Structure (v1 baseline). Exact-match. |

**§4 subtotal:** 0 ADD / 0 MERGE / 4 SKIP — 100% overlap with existing Structure category.

---

## §5. Styles and Genres Meta Tags (46 source items + 2 split-row extras = 48 rows)

| # | Source label | Decision | Target | Bracket form | Rationale |
|---|---|---|---|---|---|
| 5.1 | Acoustic | ADD | Genre | `[Acoustic]` | New Genre-as-stylistic-descriptor; distinct from existing `[Acoustic Guitar]` Instrument. |
| 5.2 | African | ADD | Genre | `[African]` | Source has bare "African" (no "music" suffix). Differs from v1.5 §3.7 SKIP of "African music" super-label. ADD per import-as-source discipline. Lead-overridable per v1.6 spec Q3. |
| 5.3 | Alternative metal | ADD | Genre | `[Alternative Metal]` | Title-Case. New Metal sub-style not in v1.5 §L. |
| 5.4 | Alternative pop | ADD | Genre | `[Alternative Pop]` | Title-Case. New Pop sub-style not in v1.5 §I. |
| 5.5 | Ambient | SKIP | — | — | `[Ambient]` already present in Genre (v1.5 §E.1). |
| 5.6 | Atlanta rap | ADD | Genre | `[Atlanta Rap]` | Title-Case. New Hip Hop sub-style; geographical variant. |
| 5.7 | Ballad | ADD | Genre | `[Ballad]` | New Genre/style. |
| 5.8 | Baroque | ADD | Genre | `[Baroque]` | New Genre (classical era). |
| 5.9 | Blues | SKIP | — | — | `[Blues]` already present in Genre (v1.5 §B.0). |
| 5.10 | Boom bap | ADD | Genre | `[Boom Bap]` | Title-Case. New Hip Hop sub-style. |
| 5.11 | Cello | SKIP | — | — | `[Cello]` already present in **Instrument** (v1 baseline). Source labels as Style/Genre — categorical disagreement, but canonical bracket exists. |
| 5.12 | Chill | MERGE | — | — | Existing `[Mood: Chill]` (v1 baseline) is closest semantic match in **Mood** category. MERGE for B-008 cross-category alias: `[Chill]` → `[Mood: Chill]`. |
| 5.13a | Christian & Gospel — `Gospel` half | MERGE | — | — | Split-row 13a per v1.6 §3.4. `[Gospel]` already present in Genre (v1.5 §J.3). Gospel-half of compound label maps to canonical existing entry; MERGE for B-008. |
| 5.13b | Christian & Gospel — `Christian` half | ADD | Genre | `[Christian]` | Split-row 13b per v1.6 §3.4. `[Christian]` standalone Genre ADD. Lead may override to `[Christian Rock]` per spec Q4. |
| 5.14 | Christmas | ADD | Genre | `[Christmas]` | New seasonal Genre. |
| 5.15 | Country & Americana | SKIP | — | — | Both halves covered: `[Country]` (v1.5 §C.0) + `[Americana]` (v1.5 §F.1). No new ADDs needed. |
| 5.16a | Dance & Electronic — `Dance` half | ADD | Genre | `[Dance]` | Split-row 16a per v1.6 §3.4. `[Dance]` standalone Genre — not present in v1.5 (only `[Dance-Pop]` exists). |
| 5.16b | Dance & Electronic — `Electronic` half | SKIP | — | — | Split-row 16b per v1.6 §3.4. `[Electronic]` already present in Genre (v1.5 §E.0). |
| 5.17 | Drums | ADD | Instrument | `[Drums]` | Source labels as Style/Genre, but Drums is clearly an instrument. Category-respect over source-respect (v1.6 spec Q5). Distinct from existing `[Drum Solo]`/`[Drum Break]`/`[Drum Fill]` Structure entries (specific song-structure markers). |
| 5.18 | EDM | ADD | Genre | `[EDM]` | Acronym preserves casing per v1.5 §3.2. |
| 5.19 | Girl group | ADD | Genre | `[Girl Group]` | Title-Case. New Genre/ensemble type. |
| 5.20 | Gospel | SKIP | — | — | `[Gospel]` already present in Genre (v1.5 §J.3). |
| 5.21 | Hardcore rap | ADD | Genre | `[Hardcore Rap]` | Title-Case. New Hip Hop sub-style. |
| 5.22 | Heavy metal | SKIP | — | — | `[Heavy Metal]` already present in Genre (v1.5 §L.3). Case-canonical. |
| 5.23 | Hip hop | SKIP | — | — | `[Hip Hop]` already present in Genre (v1.5 §G.0). Case-canonical. |
| 5.24 | Indie | ADD | Genre | `[Indie]` | New standalone Genre parent; distinct from sub-styles `[Indie Rock]`/`[Indie Pop]`/`[Indie Folk]` already present. |
| 5.25 | Indie rock | SKIP | — | — | `[Indie Rock]` already present in Genre (v1.5 §K.4). |
| 5.26 | J-pop | SKIP | — | — | `[J-Pop]` already present in Genre (v1.5 §N.4a). Case-canonical. |
| 5.27 | Jazz | SKIP | — | — | `[Jazz]` already present in Genre (v1.5 §H.0). |
| 5.28 | K-pop | SKIP | — | — | `[K-Pop]` already present in Genre (v1.5 §I.4). Case-canonical. |
| 5.29 | Lo-fi | ADD | Genre | `[Lo-Fi]` | New Genre `[Lo-Fi]` (hyphen Title-Case per v1.5 §3.2). Coexists with existing `[Effect: Lo-fi]` Effect — different categories, different brackets. v1.6 spec Q8. |
| 5.30 | Orchestra | ADD | Instrument | `[Orchestra]` | Instrument-collective; distinct from existing `[Orchestral Build]` Effect (technique, not instrument). Category-respect (v1.6 spec Q5). |
| 5.31 | Party | ADD | Genre | `[Party]` | New Genre/style ("party music"). |
| 5.32 | Piano | ADD | Instrument | `[Piano]` | Source labels as Style/Genre, but Piano is clearly an instrument. Distinct from existing `[Piano Solo]` Structure (song-structure marker). Category-respect (v1.6 spec Q5). |
| 5.33 | Pop | SKIP | — | — | `[Pop]` already present in Genre (v1.5 §I.0). |
| 5.34 | Pop-Rock | ADD | Genre | `[Pop-Rock]` | Hyphen Title-Case per v1.5 §3.2; preserve source hyphen. |
| 5.35 | Post-Hardcore | ADD | Genre | `[Post-Hardcore]` | Hyphen Title-Case; preserve source hyphen. |
| 5.36 | Punk Rock | SKIP | — | — | `[Punk Rock]` already present in Genre (v1.5 §K.5). Exact-match. |
| 5.37 | R&B | SKIP | — | — | `[R&B]` already present in Genre (v1.5 §B.5). |
| 5.38 | R&B & Soul | SKIP | — | — | Both halves covered: `[R&B]` (v1.5 §B.5) + `[Soul]` (v1.5 §J.0). v1.5 already applied heading-split. |
| 5.39 | Rap | ADD | Genre | `[Rap]` | Standalone Genre parent; distinct from sub-styles `[Gangsta Rap]`/`[Hardcore Rap]` (5.21)/`[Atlanta Rap]` (5.6) — generic vs specific. |
| 5.40 | Reggae | SKIP | — | — | `[Reggae]` already present in Genre (v1.5 §N.2a). |
| 5.41 | Rock | SKIP | — | — | `[Rock]` already present in Genre (v1.5 §K.0). |
| 5.42 | Romantic | MERGE | — | — | Existing `[Mood: Romantic]` (v1 baseline) is closest semantic match in **Mood** category. MERGE for B-008 cross-category alias: `[Romantic]` → `[Mood: Romantic]`. |
| 5.43 | Soul | SKIP | — | — | `[Soul]` already present in Genre (v1.5 §J.0 heading split). |
| 5.44 | Synth | ADD | Instrument | `[Synth]` | Synthesizer-instrument; distinct from existing `[Synth-Pop]` Genre (v1.5 §I.5). Category-respect (v1.6 spec Q5). |
| 5.45 | Synth pop | MERGE | — | — | Existing `[Synth-Pop]` (v1.5 §I.5, hyphenated) is canonical. Source has space variant. MERGE for B-008: `[Synth Pop]` → `[Synth-Pop]`. |
| 5.46 | Techno | SKIP | — | — | `[Techno]` already present in Genre (v1.5 §E.8). |

**§5 subtotal:** 24 ADD / 4 MERGE / 20 SKIP across 48 decision rows (46 source items + 2 split-row extras).

---

## Grand totals (planner draft — to be verified at T2 grep-recount)

| Section | ADD | MERGE | SKIP | Decision rows |
|---|---:|---:|---:|---:|
| §1. Sound Effects | 11 | 2 | 5 | 18 |
| §2. Vocal Expressions | 7 | 2 | 0 | 9 |
| §3. Static and Other Effects | 3 | 0 | 1 | 4 |
| §4. Structural Tags | 0 | 0 | 4 | 4 |
| §5. Styles and Genres | 24 | 4 | 20 | 48 (incl. 2 split-row extras) |
| **TOTAL** | **45** | **8** | **30** | **83** (from 81 source items + 2 split rows) |

**ADD distribution by target category:**
- Vocal: +5 (Announcer, Female Narrator, Reporter, Boy, Girl)
- Instrument: +4 (Drums, Orchestra, Piano, Synth)
- Production: +2 (Censored, Silence)
- SFX: +14 (Barking, Bell Dings, Bleep, Chuckles, Cough, Groaning, Phone Ringing, Ringing, Screams, Squawking, Whispers, Audience Laughing, Giggling, Clears Throat) — Clapping SKIPped per T1 self-correction
- Genre: +20 (Acoustic, African, Alternative Metal, Alternative Pop, Atlanta Rap, Ballad, Baroque, Boom Bap, Christian, Christmas, Dance, EDM, Girl Group, Hardcore Rap, Indie, Lo-Fi, Party, Pop-Rock, Post-Hardcore, Rap)
- **No new categories** (unlike v1.4 SFX or v1.5 Genre).

**Projected `tags.json` content after T1 commit:**
- Existing v1.5 entries: **286**
- New ADDs from this table: **45** (planner draft was 46; T1 self-correction removed Clapping ADD after collision detection)
- New total: **331** entries / **8** categories unchanged

**Test threshold targets (per spec §7.1):**
- H1 (total count): `>= 320` (planner draft target; 331 actual)
- H2 (per-category extension): Vocal `>= 45` (actual 45), Instrument `>= 36` (actual 36), Production `>= 6` (actual 6), SFX `>= 63` (actual 63), Genre `>= 107` (actual 107)

**T1 planner self-correction (2026-05-27):** Decision-table draft listed row 1.9 (Clapping) as ADD. T1 commit application caught a bracket-collision with existing v1.4 `[Clapping]` SFX entry (v1.5 C5/G5 bracket-uniqueness tests failed at first dotnet test run). Decision corrected to SKIP-canonical-present; tags.json entry removed; decision-table grand-totals + per-category counts + projected counts refreshed; specialist plan-phase advisory's pre-clear arithmetic verification (46/8/29) updated to (45/8/30) — same v1.4 D.1 +1 off-by-one discipline + v1.5 T2.5 hygiene pattern, caught pre-USER-REVIEW. No spec/test/algorithm impact; H1 `>= 320` threshold still satisfied by 331.

Loose-threshold pattern leaves headroom for off-by-N drift (v1.4 precedent allowed 199 against `>= 198`; v1.5 allowed 286 against `>= 270`).

**Cumulative MERGE rows for future B-008 alias schema** (post-v1.6 total: **24**):
- v1.4: 16 (cheat-sheet alias data from B-SUNO-005)
- v1.5: 0 (Genre category was empty pre-cycle)
- v1.6: 8 (Birds chirping→Birdsong, Cheering→Crowd Cheering, Man→Male Vocal, Woman→Female Vocal, Chill→Mood:Chill, Gospel-half→Gospel-Genre, Romantic→Mood:Romantic, Synth pop→Synth-Pop)

---

## Lead-ratification request

This table is a **planner draft**. Lead is requested to ratify (or override) the following at r1 review:

1. **Decision counts:** 45 ADD / 8 MERGE / 30 SKIP / 83 decision rows from 81 source items (corrected from planner draft 46/29 after T1 Clapping collision detection — see decision-table T1 self-correction note).
2. **Cross-category collision policy** (v1.6 §3.3): SKIP-canonical / MERGE-cross-cat / ADD-new-cat distinctions.
3. **Split-row decisions** (v1.6 §3.4): "Christian & Gospel" → 13a + 13b; "Dance & Electronic" → 16a + 16b.
4. **`[African]` ADD** despite v1.5 §3.7 super-label SKIP precedent (different exact-source-label: bare "African" without "music" suffix).
5. **`[Christian]` standalone ADD** from §5.13 split: Lead may prefer SKIP or `[Christian Rock]` alternative.
6. **Voice-type ADDs** (`[Announcer]`, `[Female Narrator]`, `[Reporter]`, `[Boy]`, `[Girl]`) per import-as-source discipline.
7. **Instrument-category ADDs** (`[Drums]`, `[Piano]`, `[Synth]`, `[Orchestra]`) despite source's Style/Genre labeling — category-respect convention applied.
8. **Production-category ADDs** (`[Censored]`, `[Silence]`) — alternative placements possible (SFX or new category).
9. **`[Lo-Fi]` Genre coexists with `[Effect: Lo-fi]` Effect** — cross-category ADD example for future curation slices.
10. **Whispers SFX standalone** despite existing `[Whispered]` Vocal — semantically distinct.

If Lead overrides any of these, the table will be regenerated and totals recomputed before T1 commit.
