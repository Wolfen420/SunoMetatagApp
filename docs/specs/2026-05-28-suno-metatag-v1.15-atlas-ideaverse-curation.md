# SunoMetatagApp v1.15 — Atlas Ideaverse Metatag Database Curation — Spec

**Date:** 2026-05-28
**Slice:** B-SUNO-007b / v1.15 (Medium priority follow-on to v1.6's B-SUNO-007 cycle)
**Scope:** Land 39 ADD decisions from the user-prepared decision table into `tags.json`. Defer 10 ALIAS decisions. Document 5 SKIPs.

## 1. Problem

User created `docs/reference/B-SUNO-007b-suno-meta-tags-database-decision-table-2026-05-27.md` capturing curation decisions for 54 candidate metatags discovered in `\\WolfenNAS\docker\hermes\Vaults\Ideaverse\Atlas\wiki\concepts\suno-meta-tags-database.md` that don't have exact bracket matches in current `tags.json`. Row-level decisions: **39 ADD / 10 ALIAS / 5 SKIP** (the stated Summary 35/12/7 in the original file was inconsistent with the row-level breakdown; reconciled at r1 absorption #1).

## 2. Mechanism

### 2.1 ADD: 39 entries → `tags.json` with planner-proposed category mapping

| Category | Count | Entries |
|---|---|---|
| Mood | 13 | `[Cynical]`, `[Ethereal]`, `[Hopeful]`, `[Intense]`, `[Intimate]`, `[Low Energy]`, `[Medium Energy]`, `[Mysterious]`, `[Peaceful]`, `[Playful]`, `[Sensual]`, `[Tense]`, `[Unsettling]` |
| Production | 10 | `[Analog]`, `[Clean]`, `[Dense]`, `[Digital]`, `[Dry]`, `[Hi-Fi]`, `[Layered]`, `[Minimalist]`, `[Quiet Arrangement]`, `[Vintage Warmth]` |
| Effect | 8 | `[Dark Atmosphere]`, `[Distorted]`, `[Emotional Climax]`, `[Falling Tension]`, `[Gradual Swell]`, `[Heavy Reverb]`, `[Orchestral Swell]`, `[Slow Down]` |
| Instrument | 5 | `[Bass]`, `[Brass]`, `[Organ]`, `[Saxophone]`, `[Strings]` |
| Vocal | 3 | `[Gritty]`, `[Harmonized Chorus]`, `[Yodel]` |
| **Total** | **39** | |

New entries appended within their respective category clusters in `tags.json` (after the last existing entry of each category). All new entries use the 4-field flat schema without `description` (matching the v1.6+ Vocal/Boy/Girl precedent); descriptions can be added in a follow-on cycle if user wants.

### 2.2 ALIAS: 10 entries → wiki documentation (logic deferred)

Each ALIAS row maps a non-prefixed bracket (user input target) to an existing canonical prefixed bracket. Listed for wiki at T7:

| Non-prefixed input | Canonical target |
|---|---|
| `[Aggressive]` | `[Mood: Aggressive]` |
| `[Building Energy]` | `[Energy: Building]` |
| `[Dreamy]` | `[Atmosphere: Dreamy]` |
| `[Euphoric]` | `[Mood: Euphoric]` |
| `[Explosive]` | `[Energy: Explosive]` |
| `[High Energy]` | `[Mood: High Energy]` |
| `[Melancholic]` | `[Mood: Melancholic]` |
| `[Melismatic]` | `[Melisma]` |
| `[Nostalgic]` | `[Mood: Nostalgic]` |
| `[Romantic]` | `[Mood: Romantic]` |

ALIAS resolution would require new logic in `TagService.Filter` / `MainViewModel.InsertTag` so non-prefixed user input maps to canonical prefixed entries. Deferred as separate follow-on cycle (B-SUNO-007c v1.16 candidate).

### 2.3 SKIP: 5 entries → wiki documentation (no code change)

| Skipped entry | Rejection rationale |
|---|---|
| `[brackets]` | False positive from prose — not a metatag. |
| `[Calm]` | Too vague/overlapping with peaceful/chill (already covered by other entries). |
| `[Emotional]` | Overly generic; low control value. |
| `[Sparse]` | Redundant with `[Minimalist]` / `[Quiet Arrangement]` (which DO land at v1.15). |
| `[Strong]` | Too vague; overlaps with powerful/belted/intense. |

## 3. Counts after v1.15

| Metric | v1.14 baseline | v1.15 target | Delta |
|---|---|---|---|
| Total `tags.json` entries | 335 | **374** | +39 |
| Mood category | 21 | **34** | +13 |
| Production category | 6 | **16** | +10 |
| Effect category | 19 | **27** | +8 |
| Instrument category | 36 | **41** | +5 |
| Vocal category | 45 | **48** | +3 |
| SFX / Genre / Structure | 63 / 107 / 38 | unchanged | 0 |

## 4. Borderline category mapping (6 entries — Lead-ratified at r1)

Planner chose category for 6 entries with plausible alternates; Lead r1 did NOT object to any. Default mappings stand:

| Entry | Chose | Alternative | Rationale |
|---|---|---|---|
| `[Ethereal]` | Mood | Effect | Existing `[Mood: Dreamy]` precedent. |
| `[Intimate]` | Mood | Vocal | Broader than vocal delivery. |
| `[Emotional Climax]` | Effect | Structure | Rationale text says "dynamics/arrangement cue". |
| `[Slow Down]` | Effect | Structure | Dynamic-direction class. |
| `[Gritty]` | Vocal | Production | Decision-table rationale says "vocal/texture cue". |
| `[Layered]` | Production | Vocal | Broader arrangement-density consistent with `[Layered Arrangement]`. |

## 5. Test additions

### 5.1 H8 (new) — Atlas Ideaverse presence

`H8_AtlasIdeaverseMetatagDatabase_PresentInExpectedCategory` `[Theory]` with 10 `[InlineData]` cases asserting `(bracket, expectedCategory)` tuples via `LoadProductionTagsJson()`:

- 5 category representatives: `[Cynical]` Mood, `[Hi-Fi]` Production, `[Heavy Reverb]` Effect, `[Saxophone]` Instrument, `[Yodel]` Vocal.
- 5 borderline-decision verifiers: `[Ethereal]` Mood, `[Intimate]` Mood, `[Emotional Climax]` Effect, `[Gritty]` Vocal, `[Layered]` Production.

### 5.2 H2 extension (absorption #2 — Lead-directed)

Existing `H2_ExtendedCategoryCountsMet` `[Theory]` currently has 5 `[InlineData]` rows (Vocal 45, Instrument 36, Production 6, SFX 63, Genre 107). Extended at v1.15 to include Mood and Effect for regression-protection parity:

```csharp
[InlineData("Mood", 34)]    // v1.15 post-Atlas-Ideaverse baseline
[InlineData("Effect", 27)]  // v1.15 post-Atlas-Ideaverse baseline
```

Production lower bound bumped 6 → 16 to reflect post-v1.15 actual (also for stronger regression protection).

### 5.3 H5 stale comment refresh

`H5_LoadAll_NoBracketCollisionsAcrossAllCategories` test comment updated from `335 entries post-v1.14` → `374 entries post-v1.15`.

## 6. Test count forecast

| Layer | Delta | Cumulative |
|---|---|---|
| v1.14 baseline | — | 136 |
| H8 [Theory] × 10 InlineData | +10 | 146 |
| H2 extension (Mood, Effect) | +2 | 148 |
| **v1.15 total forecast** | **+12** | **148** |

## 7. Parity boundaries (what does NOT change)

- `TagDefinition` 4-field schema unchanged.
- `TagService.LoadAll` / `DistinctCategories` / `Filter` unchanged.
- `MainViewModel` unchanged.
- `MainWindow.xaml` + `MainWindow.xaml.cs` unchanged.
- `Themes/SunoTokens.xaml` + `Themes/SunoStyles.xaml` unchanged.
- `prompts.json` byte-identical to v1.14 (zero references to the 39 new brackets).
- v1.7 search normalization, v1.10 picker-pane focus preservation, v1.11 alphabetical ordering, v1.12 chip-pill colors, v1.13 default-category-Structure, v1.14 Verse-cluster — all preserved.

## 8. Validation

USER REVIEW S1-S6 (see r1 plan packet §7.2):
- S1: new Mood entries visible (primary)
- S2: cross-category sampling (Production, Effect, Instrument, Vocal)
- S3: insert parity (`[Heavy Reverb]`)
- S4: v1.7 search composition (`medium` filter)
- S5: v1.10 picker-pane focus + v1.12 chip-pill color regression-gate
- S6: v1.13 default-category + v1.14 Verse-cluster regression-gate (no Structure pollution)

## 9. Rollback

Two-commit revert: `git revert <T2-sha> <T1-sha>`. tags.json returns to 335 entries. Tests return to 136. Decision-table file remains tracked (since T1 committed it from untracked → tracked) but reverts to its pre-fix Summary breakdown if revert is full.

## 10. Related

- `[[sunometatag-tag-library]]` — architecture page; v1.15 wiki updates land at T7.
- `[[sunometatag-app]]` — feature page; v1.15 subsection lands at T7.
- B-SUNO-006 + B-SUNO-007 backlog drift surfaced in r1 plan preamble — Lead-discretion to retire at v1.15 RESULT closeout reviewer-memory updates.
- B-SUNO-007c (v1.16 candidate) — ALIAS resolution follow-on (not yet a formal backlog row).
