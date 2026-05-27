---
title: B-SUNO-006 / v1.5 Decision Table — Genre Taxonomy Import
type: source
status: planner-draft
authored: 2026-05-27
authored_by: planner (B-SUNO-006 / v1.5)
source_file: docs/reference/suno-genre-source-2026-05-27.md
target_file: src/SunoMetatagApp/Resources/tags.json
target_category: Genre (new top-level category, currently zero entries)
canonicalization_rules: docs/specs/2026-05-27-suno-metatag-v1.5-genre-taxonomy.md §3.2–§3.7
tags: [sla, suno, genre, taxonomy, curation, decision-table, planner-draft]
related: [[suno-genre-source-2026-05-27]], [[sunometatag-tag-library]]
---

# B-SUNO-006 / v1.5 Decision Table — Genre Taxonomy Import

> **Planner draft** — pending Lead ratification at r1 review. Totals are planner-counted at draft time; the **T2 grep-recount discipline** (v1.4 precedent `42c5a28`) verifies actual `tags.json` content count after T1 commit; any off-by-N triggers a hygiene commit.

## Methodology

For every row in [`suno-genre-source-2026-05-27.md`](./suno-genre-source-2026-05-27.md):

- **Decision:** `ADD` / `MERGE` / `SKIP`
- **Target category:** `Genre` (always, for ADD rows)
- **Bracket form:** `[Title Case]` per spec §3.2 (with "music"-suffix removal per §3.4 and abbreviation canonicalization per §3.5)
- **Rationale:** one-liner; cites source-internal duplicates, suffix removal, abbreviation policy, regional-super-label policy

No MERGEs are expected in v1.5 because no existing `Genre` entries exist in `tags.json` (current 199 entries span Structure/Vocal/Instrument/Mood/Effect/Production/SFX only). MERGE column reserved for future B-008 alias-schema reconciliation.

---

## A. Avant-garde & Experimental (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| A.0 | Avant-garde & Experimental | (heading, part 1) | ADD | `[Experimental]` | Section heading splits into two sibling parents per specialist LOW 1: Experimental as the broader umbrella. |
| A.0a | Avant-garde & Experimental | (heading, part 2) | ADD | `[Avant-garde]` | Source explicitly names "Avant-garde" as a distinct genre (formally innovative, often pre-electronic or art-music context). Splitting the heading into `[Experimental]` + `[Avant-garde]` preserves source semantics without forcing users to compose `[Experimental \| Avant-garde]` via v1.3 stacked syntax. Absorbed from B-SUNO-006 specialist plan-phase advisory LOW 1 (2026-05-27). |
| A.1 | Avant-garde & Experimental | Electroacoustic | ADD | `[Electroacoustic]` | Bare-form per §3.2. |
| A.2 | Avant-garde & Experimental | Industrial music | ADD | `[Industrial]` | "music" suffix dropped per §3.4. |
| A.3 | Avant-garde & Experimental | Noise music | ADD | `[Noise]` | "music" suffix dropped per §3.4. |
| A.4 | Avant-garde & Experimental | Progressive music | ADD | `[Progressive]` | "music" suffix dropped per §3.4. |
| A.5 | Avant-garde & Experimental | Psychedelic music | ADD | `[Psychedelic]` | "music" suffix dropped per §3.4. |

**Section A subtotal:** 7 ADD / 0 MERGE / 0 SKIP

---

## B. Blues (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| B.0 | Blues | (heading) | ADD | `[Blues]` | Standard parent label. |
| B.1 | Blues | Chicago blues | ADD | `[Chicago Blues]` | Title-Case per §3.2. |
| B.2 | Blues | Delta blues | ADD | `[Delta Blues]` | Title-Case per §3.2. |
| B.3 | Blues | Electric blues | ADD | `[Electric Blues]` | Title-Case per §3.2. |
| B.4 | Blues | Gospel blues | ADD | `[Gospel Blues]` | Title-Case per §3.2. |
| B.5 | Blues | Rhythm and blues | ADD | `[R&B]` | Abbreviation canonicalization per §3.5; well-established short form. |

**Section B subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## C. Country (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| C.0 | Country | (heading) | ADD | `[Country]` | Standard parent label. |
| C.1 | Country | Bluegrass | ADD | `[Bluegrass]` | Bare-form. |
| C.2 | Country | Country blues | ADD | `[Country Blues]` | Title-Case. |
| C.3 | Country | Country pop | ADD | `[Country Pop]` | Title-Case. |
| C.4 | Country | Country rock | ADD | `[Country Rock]` | Title-Case. |
| C.5 | Country | Nashville sound | ADD | `[Nashville Sound]` | Title-Case. |

**Section C subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## D. Easy Listening (heading + 4 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| D.0 | Easy Listening | (heading) | ADD | `[Easy Listening]` | Standard parent label. |
| D.1 | Easy Listening | Adult contemporary music | ADD | `[Adult Contemporary]` | "music" suffix dropped per §3.4. |
| D.2 | Easy Listening | Elevator music (muzak) | ADD | `[Muzak]` | Parenthetical canonical form chosen; "Muzak" is the recognizable bracket label. |
| D.3 | Easy Listening | Lounge music | ADD | `[Lounge]` | "music" suffix dropped per §3.4. |
| D.4 | Easy Listening | Soft rock | ADD | `[Soft Rock]` | Title-Case; Easy Listening section is primary source (not Rock section). |

**Section D subtotal:** 5 ADD / 0 MERGE / 0 SKIP

---

## E. Electronic (heading + 9 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| E.0 | Electronic | (heading) | ADD | `[Electronic]` | Standard parent label. |
| E.1 | Electronic | Ambient | ADD | `[Ambient]` | Bare-form. |
| E.2 | Electronic | Breakbeat | ADD | `[Breakbeat]` | Bare-form. |
| E.3 | Electronic | Disco | ADD | `[Disco]` | Bare-form. |
| E.4 | Electronic | Drum and bass | ADD | `[Drum and Bass]` | Title-Case (mid-word "and" lowercase per common Suno bracket convention; `[DnB]` alias deferred to B-008). |
| E.5 | Electronic | Dub | ADD | `[Dub]` | Bare-form. |
| E.6 | Electronic | Electro | ADD | `[Electro]` | Bare-form. |
| E.7 | Electronic | House music | ADD | `[House]` | "music" suffix dropped per §3.4. |
| E.8 | Electronic | Techno | ADD | `[Techno]` | Bare-form. |
| E.9 | Electronic | Trance music | ADD | `[Trance]` | "music" suffix dropped per §3.4. |

**Section E subtotal:** 10 ADD / 0 MERGE / 0 SKIP

---

## F. Folk (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| F.0 | Folk | (heading) | ADD | `[Folk]` | Standard parent label. |
| F.1 | Folk | Americana | ADD | `[Americana]` | Bare-form. |
| F.2 | Folk | Celtic music | ADD | `[Celtic]` | "music" suffix dropped per §3.4. |
| F.3 | Folk | Folk rock | ADD | `[Folk Rock]` | Title-Case. |
| F.4 | Folk | Indie folk | ADD | `[Indie Folk]` | Title-Case. |
| F.5 | Folk | Singer-songwriter | ADD | `[Singer-Songwriter]` | Title-Case with hyphen preserved. |

**Section F subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## G. Hip Hop (heading + 4 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| G.0 | Hip Hop | (heading) | ADD | `[Hip Hop]` | Standard parent label (two words, space — matches section heading). |
| G.1 | Hip Hop | Alternative hip hop | ADD | `[Alternative Hip Hop]` | Title-Case. |
| G.2 | Hip Hop | Gangsta rap | ADD | `[Gangsta Rap]` | Title-Case (preserves "Gangsta" spelling per source). |
| G.3 | Hip Hop | Trap | ADD | `[Trap]` | Bare-form. |
| G.4 | Hip Hop | UK drill | ADD | `[UK Drill]` | Acronym preserved; "Drill" title-cased. |

**Section G subtotal:** 5 ADD / 0 MERGE / 0 SKIP

---

## H. Jazz (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| H.0 | Jazz | (heading) | ADD | `[Jazz]` | Standard parent label. |
| H.1 | Jazz | Bebop | ADD | `[Bebop]` | Bare-form. |
| H.2 | Jazz | Big band | ADD | `[Big Band]` | Title-Case. |
| H.3 | Jazz | Cool jazz | ADD | `[Cool Jazz]` | Title-Case. |
| H.4 | Jazz | Jazz fusion | ADD | `[Jazz Fusion]` | Title-Case. |
| H.5 | Jazz | Smooth jazz | ADD | `[Smooth Jazz]` | Title-Case. |

**Section H subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## I. Pop (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| I.0 | Pop | (heading) | ADD | `[Pop]` | Standard parent label. |
| I.1 | Pop | Dance-pop | ADD | `[Dance-Pop]` | Hyphen preserved; both segments Title-Case per §3.2. |
| I.2 | Pop | Electropop | ADD | `[Electropop]` | Compound-bare form (no hyphen in source; preserve). |
| I.3 | Pop | Indie pop | ADD | `[Indie Pop]` | Title-Case. |
| I.4 | Pop | K-pop | ADD | `[K-Pop]` | Hyphen preserved; both segments Title-Case. Canonical occurrence; Regional Music duplicate SKIPped in §N. |
| I.5 | Pop | Synth-pop | ADD | `[Synth-Pop]` | Hyphen preserved; both segments Title-Case. |

**Section I subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## J. R&B & Soul (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| J.0 | R&B & Soul | (heading) | ADD | `[Soul]` | Heading split: `[R&B]` already covered in B.5; remaining `[Soul]` ADDed as standalone parent label. |
| J.1 | R&B & Soul | Contemporary R&B | ADD | `[Contemporary R&B]` | Title-Case; preserves "R&B" abbreviation. |
| J.2 | R&B & Soul | Funk | ADD | `[Funk]` | Bare-form. |
| J.3 | R&B & Soul | Gospel music | ADD | `[Gospel]` | "music" suffix dropped per §3.4. |
| J.4 | R&B & Soul | Neo soul | ADD | `[Neo Soul]` | Title-Case. |
| J.5 | R&B & Soul | Quiet storm | ADD | `[Quiet Storm]` | Title-Case. |

**Section J subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## K. Rock (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| K.0 | Rock | (heading) | ADD | `[Rock]` | Standard parent label. |
| K.1 | Rock | Alternative rock | ADD | `[Alternative Rock]` | Title-Case. |
| K.2 | Rock | Classic rock | ADD | `[Classic Rock]` | Title-Case. |
| K.3 | Rock | Hard rock | ADD | `[Hard Rock]` | Title-Case. |
| K.4 | Rock | Indie rock | ADD | `[Indie Rock]` | Title-Case. |
| K.5 | Rock | Punk rock | ADD | `[Punk Rock]` | Canonical occurrence; Punk section duplicate SKIPped in §M. |

**Section K subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## L. Metal (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| L.0 | Metal | (heading) | ADD | `[Metal]` | Standard parent label. `[Heavy Metal]` is a sub-style sibling; both retained per §3.3. |
| L.1 | Metal | Black metal | ADD | `[Black Metal]` | Title-Case. |
| L.2 | Metal | Death metal | ADD | `[Death Metal]` | Title-Case. |
| L.3 | Metal | Heavy metal | ADD | `[Heavy Metal]` | Title-Case. |
| L.4 | Metal | Industrial metal | ADD | `[Industrial Metal]` | Title-Case. Compound-bare with both halves; not a "music" suffix case. |
| L.5 | Metal | Power metal | ADD | `[Power Metal]` | Title-Case. |

**Section L subtotal:** 6 ADD / 0 MERGE / 0 SKIP

---

## M. Punk (heading + 5 sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| M.0 | Punk | (heading) | ADD | `[Punk]` | Standard parent label. |
| M.1 | Punk | Anarcho punk | ADD | `[Anarcho Punk]` | Title-Case. |
| M.2 | Punk | Hardcore punk | ADD | `[Hardcore Punk]` | Title-Case. |
| M.3 | Punk | Pop punk | ADD | `[Pop Punk]` | Title-Case. |
| M.4 | Punk | Punk rock | SKIP | — | Source-internal duplicate (canonical ADD in K.5 Rock section). MERGE column reserved for B-008 alias schema. |
| M.5 | Punk | Skate punk | ADD | `[Skate Punk]` | Title-Case. |

**Section M subtotal:** 5 ADD / 0 MERGE / 1 SKIP

---

## N. Regional Music (heading + 4 super-labels + 8 parenthetical sub-styles)

| # | Section | Source label | Decision | Bracket form | Rationale |
|---|---|---|---|---|---|
| N.0 | Regional Music | (heading) | SKIP | — | Too coarse per spec §3.7. |
| N.1 | Regional Music | Brazilian music (super-label) | SKIP | — | Super-label too coarse per §3.7. |
| N.1a | Regional Music → Brazilian | Samba | ADD | `[Samba]` | Bare-form. |
| N.1b | Regional Music → Brazilian | Bossa nova | ADD | `[Bossa Nova]` | Title-Case. |
| N.2 | Regional Music | Caribbean music (super-label) | SKIP | — | Super-label too coarse per §3.7. |
| N.2a | Regional Music → Caribbean | Reggae | ADD | `[Reggae]` | Bare-form. |
| N.2b | Regional Music → Caribbean | Dancehall | ADD | `[Dancehall]` | Bare-form. |
| N.3 | Regional Music | African music (super-label) | SKIP | — | Super-label too coarse per §3.7. |
| N.3a | Regional Music → African | Afrobeat | ADD | `[Afrobeat]` | Bare-form. |
| N.3b | Regional Music → African | Highlife | ADD | `[Highlife]` | Bare-form. |
| N.4 | Regional Music | Asian music (super-label) | SKIP | — | Super-label too coarse per §3.7. |
| N.4a | Regional Music → Asian | J-pop | ADD | `[J-Pop]` | Hyphen preserved; Title-Case. |
| N.4b | Regional Music → Asian | K-pop | SKIP | — | Source-internal duplicate (canonical ADD in I.4 Pop section). MERGE column reserved for B-008 alias schema. |

**Section N subtotal:** 7 ADD / 0 MERGE / 6 SKIP

---

## Grand totals (planner draft — to be verified at T2 grep-recount)

| Section | ADD | MERGE | SKIP | Section total |
|---|---:|---:|---:|---:|
| A. Avant-garde & Experimental | 7 | 0 | 0 | 7 |
| B. Blues | 6 | 0 | 0 | 6 |
| C. Country | 6 | 0 | 0 | 6 |
| D. Easy Listening | 5 | 0 | 0 | 5 |
| E. Electronic | 10 | 0 | 0 | 10 |
| F. Folk | 6 | 0 | 0 | 6 |
| G. Hip Hop | 5 | 0 | 0 | 5 |
| H. Jazz | 6 | 0 | 0 | 6 |
| I. Pop | 6 | 0 | 0 | 6 |
| J. R&B & Soul | 6 | 0 | 0 | 6 |
| K. Rock | 6 | 0 | 0 | 6 |
| L. Metal | 6 | 0 | 0 | 6 |
| M. Punk | 5 | 0 | 1 | 6 |
| N. Regional Music | 7 | 0 | 6 | 13 |
| **TOTAL** | **87** | **0** | **7** | **94** |

**Projected `tags.json` content after T1 commit:**
- Existing v1.4 entries: **199**
- New Genre ADDs from this table: **87** (86 planner-draft + 1 absorbed at T0 per specialist LOW 1: `[Avant-garde]` sibling parent)
- New total: **286** entries / **8** top-level categories (Structure, Vocal, Instrument, Mood, Effect, Production, SFX, **Genre**)

**Test threshold targets (per spec §7.1):**
- G1 (total count): `>= 270` (planner draft target; 286 actual after LOW 1 absorption)
- G2 (Genre count): `>= 70` (planner draft target; 87 actual after LOW 1 absorption)

These thresholds intentionally leave headroom in case the grep-recount finds a draft off-by-N (precedent: v1.4 D.1 +1 off-by-one caught at T2). The tests remain green for any actual count `>= 70` Genre / `>= 270` total.

---

## Lead-ratification request

This table is a **planner draft**. Lead is requested to ratify (or override) the following at r1 review:

1. **Decision counts:** 87 ADD / 0 MERGE / 7 SKIP / 94 total decision rows (93 source rows + 1 absorbed-LOW row for `[Avant-garde]` sibling parent in §A).
2. **Section-heading-as-Genre policy** (§3.3): 14 heading ADDs (one heading SKIP for `Regional Music`; §A heading splits into 2 sibling parents `[Experimental]` + `[Avant-garde]`).
3. **"music" suffix removal** (§3.4): applied to 8 entries (Industrial, Noise, Progressive, Psychedelic, House, Trance, Celtic, Gospel, plus Adult Contemporary).
4. **Abbreviation canonicalization** (§3.5): `[R&B]` for "Rhythm and blues"; `[Muzak]` for "Elevator music (muzak)"; long-form `[Drum and Bass]` only (no `[DnB]` alias).
5. **Internal duplicates** (§3.6): SKIP `Punk rock` (Punk section, canonical in Rock) and `K-pop` (Asian Regional, canonical in Pop).
6. **Regional super-labels** (§3.7): SKIP all 4 super-labels and the section heading; expand 7 sub-styles (Samba, Bossa Nova, Reggae, Dancehall, Afrobeat, Highlife, J-Pop).
7. **R&B & Soul heading split** (J.0): heading ADDed as `[Soul]` standalone since `[R&B]` already covered in B.5.

If Lead overrides any of these, the table will be regenerated and totals recomputed before T1 commit.
