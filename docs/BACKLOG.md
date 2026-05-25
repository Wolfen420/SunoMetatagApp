# SunoMetatagApp Backlog

Open work for v2+. v1 scope is locked by [`specs/2026-05-25-suno-metatag-section-editor-design.md`](specs/2026-05-25-suno-metatag-section-editor-design.md).

Roughly prioritized — top first.

## B-001 — Favorites / recently-used tags
**Status:** open · **Priority:** high
**Acceptance:** A "Pinned" group appears at the top of the tag picker; right-click a tag → Pin. Persisted to `%APPDATA%\SunoMetatagApp\favorites.json`.

## B-002 — Dark theme
**Status:** open · **Priority:** medium
**Acceptance:** Light/dark toggle in a small settings menu. Choice persisted. All controls re-themed.

## B-003 — Drag-and-drop reorder (chips and sections)
**Status:** open · **Priority:** medium
**Acceptance:** Drag a chip to a different position within its section's row, or to a different section's row. Drag a section's toolbar handle to reorder sections. v1 uses ◀/▶ on chips and ▲/▼ for sections.

## B-004 — Hotkeys
**Status:** open · **Priority:** medium
**Acceptance:** `Ctrl+N` adds a section. `Ctrl+A` arms all sections. `Ctrl+D` disarms all. `Ctrl+C` (when preview focused) copies all.

## B-005 — Persist prompt across launches
**Status:** open · **Priority:** medium
**Acceptance:** Sections + tags + lyrics serialized to `%APPDATA%\SunoMetatagApp\last-session.json` on close, restored on open. "Clear" button wipes everything.

## B-006 — *(retired — section reorder ships in v1 via ▲/▼ per spec §5.3)*

## B-007 — Reload `tags.json` without restart
**Status:** open · **Priority:** low
**Acceptance:** "Reload tags" action in a menu re-reads `tags.json`; updates the picker live; surfaces errors via the same banner.

## B-008 — Tag aliases / synonyms
**Status:** open · **Priority:** low
**Acceptance:** Optional `aliases: [...]` field on tags; search matches against label, bracket, and aliases.

## B-009 — Section type field
**Status:** open · **Priority:** low
**Acceptance:** Optional per-section "type" dropdown (Verse, Chorus, Bridge, …) separate from the tag chips. Auto-emits the matching tag in the preview.

## B-010 — Per-section "add tag" inline shortcut
**Status:** open · **Priority:** low
**Acceptance:** Type a tag name directly into a section's chip row (with autocomplete) instead of going to the right-pane picker.

## B-011 — Virtualize tag panel
**Status:** open · **Priority:** medium (trigger-based)
**Acceptance:** Tag panel uses a virtualizing host and stays smooth at 500+ tags. Trigger: tag count > 300 OR user-reported lag.

## B-012 — Chip-row hover affordances
**Status:** open · **Priority:** low
**Acceptance:** ◀/▶/✕ on chips appear only on hover; cleaner default look. v1 shows them always for discoverability.

## B-013 — Tag button visual treatment
**Status:** open · **Priority:** medium
**Acceptance:** `Style x:Key="TagButtonStyle"` replaces default WPF chrome with a flat, subtle hover style.

## B-014 — Screen-reader naming
**Status:** open · **Priority:** low
**Acceptance:** `AutomationProperties.Name` set across all interactive controls.

## B-015 — Persist splitter positions / column widths
**Status:** open · **Priority:** low
**Acceptance:** Saved to `%APPDATA%\SunoMetatagApp\layout.json`; restored on launch.

## B-016 — Permanent dim arm-hint (alternative to auto-clearing)
**Status:** open · **Priority:** low
**Acceptance:** When zero sections armed, a subtle persistent indicator near the tag picker; current v1 auto-clears on state change.

## B-017 — Auto-update `tags.json` from URL
**Status:** open · **Priority:** low
**Acceptance:** "Check for tag updates" pulls from a configured URL, shows a diff preview, confirms, then writes the merged file. Backup the prior file.

## B-018 — Retry `musci.io/blog/suno-tags` as a seed source
**Status:** open · **Priority:** low
**Acceptance:** When the URL becomes reachable (currently 500s), re-run seeding and merge into `tags.json`.

## B-019 — Tag chip drag-to-different-section
**Status:** open · **Priority:** low
**Acceptance:** Drag a chip from section A's row to section B's row → tag moves between sections.

## B-020 — Debounce preview recompute
**Status:** open · **Priority:** low
**Source:** r3 FRONTEND/UX advisory (MEDIUM)
**Acceptance:** `RecomputePreview` runs at most once per ~50ms via `DispatcherTimer` debounce. Trigger to ship: user reports keystroke lag in very long lyrics (~10KB+).

## B-021 — Inline delete-section confirm
**Status:** open · **Priority:** low
**Source:** r3 FRONTEND/UX advisory (LOW)
**Acceptance:** Replace the modal `MessageBox` with an inline `×` → "Delete?" two-click confirm on the section toolbar.

## B-022 — Preview pane cursor styling
**Status:** open · **Priority:** trivial
**Source:** r3 FRONTEND/UX advisory (LOW)
**Acceptance:** The read-only preview TextBox uses `Cursor="Arrow"` until the user starts selecting, then transitions to I-beam during selection only.
