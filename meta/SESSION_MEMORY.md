# SESSION_MEMORY (Short-Term)

This session:
- Changes made:
  - Added `GameEntry` bootstrap and `UIHudShop` IMGUI-based HUD + Shop to enable minimal playable loop (gold, streak, interest, XP, reroll/lock, timer, bench drag-drop, hotkeys 1..5 buy, Delete to sell, tooltips).
  - Updated `PROJECT_MEMORY.md` with UI MVP details and future integration notes.
- Open threads:
  - Replace IMGUI with proper uGUI/UITK prefabs and visual polish.
  - Integrate real `EconomyManager` and ScriptableObject-driven content for units/items.
- Next steps:
  - Wire input mapping, add odds table, and basic animations; move logic into Systems with DI + EventBus.

Guidelines:
- Keep brief; reset next session.
