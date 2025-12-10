# Memory & Meta Protocol

Folder layout (`/meta`):
- AGENTS.md, game-design.md, fast-tools.md, PROJECT_MEMORY.md, DECISIONS.md, SESSION_MEMORY.md

Bootstrap read order:
1) AGENTS.md → 2) PROJECT_MEMORY.md → 3) game-design.md → 4) fast-tools.md → 5) DECISIONS.md → 6) SESSION_MEMORY.md

If `/meta` missing:
1) Detect and analyze project.
2) Create `/meta`.
3) Create `PROJECT_MEMORY.md` and `SESSION_MEMORY.md`.
4) Move AGENTS.md into `/meta`.

Tracking:
- PROJECT_MEMORY: system map, architecture summary, EventBus usage, DI conventions, what exists and how it works, future implications.
- SESSION_MEMORY: session changes, open threads, next steps.
