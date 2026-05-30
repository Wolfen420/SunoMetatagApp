# Implementation Plan — SunoMetatagApp v1.21 — ALIAS Resolution Follow-on (B-SUNO-007c)

**Date:** 2026-05-30
**Spec:** `docs/specs/2026-05-30-suno-metatag-v1.21-alias-resolution.md`
**Cycle predecessor:** v1.20 closeout `APPROVED (PASS)` 2026-05-30
**Lead r1 verdict:** `APPROVED (PASS)` clean variant — no absorptions required

---

## Task sequence

### T1 — Primary implementation commit

Files extended:
- `src/SunoMetatagApp/Models/TagDefinition.cs` — 6-param record adds `IReadOnlyList<string>? Aliases = null` positional default.
- `src/SunoMetatagApp/Services/TagService.cs` — `TagDto.Aliases` JSON field; `LoadAll` passes Aliases to 6-arg constructor; `Filter.searchMatches` extends to 3-tier (Label, Bracket, Aliases).
- `src/SunoMetatagApp/Resources/tags.json` — 10 canonical entries gain `aliases: ["[<short form>]"]`.

Docs:
- `docs/specs/2026-05-30-suno-metatag-v1.21-alias-resolution.md`
- `docs/plans/2026-05-30-suno-metatag-v1.21-alias-resolution.md`

### T2 — Test fixtures commit

- `tests/SunoMetatagApp.Tests/TagServiceAliasFilterTests.cs` — A1-A8 covering alias short-form match, normalization, null/empty defaults, all 10 v1.15 mappings findable ([Theory]), alphabetical sort preservation, no duplicates, LoadAll populates Aliases correctly.

### T3 — Build + tests

`dotnet test tests/SunoMetatagApp.Tests` → expect ~207-209 green.

### T4 — Dev smoke

`timeout 8 dotnet run --project src/SunoMetatagApp --no-build` → expect `EXIT=124`.

### T5 — Publish + smoke

`dotnet publish ... -p:PublishSingleFile=true -o publish` → exe expected small positive delta; `publish/tags.json` non-zero positive delta from 10 alias entries (~200-400 bytes); `publish/prompts.json` byte-identical to v1.20.
`timeout 8 ./publish/SunoMetatagApp.exe` → expect `EXIT=124`.

### T6 — USER REVIEW S1-S6

Per spec acceptance + plan §6.4:
- S1: Primary alias search via `[Aggressive]`
- S2: All 10 v1.15 aliases findable
- S3: Bracketless alias (existing Label-substring match)
- S4: Pill insertion unchanged (canonical Bracket inserted)
- S5: Multi-cycle regression-gates
- S6: No duplicates from alias match

### T7 — Wiki updates

- `[[sunometatag-app]]` v1.20 → v1.21 title bump + new subsection
- `[[sunometatag-tag-library]]` new "v1.15 ALIAS deferral lifted at v1.21 (B-SUNO-007c)" section
- `[[ai-plan-archive]]` archive entry 45 (v1.21 r1 plan packet)

### T8 — RESULT packet

Replace `ai/PLAN.md` with v1.21 RESULT packet; append `ai/EXECUTION_LOG.md` v1.21 entry; surface `USER ACTION NEEDED` for Lead closeout routing.
