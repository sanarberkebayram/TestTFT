# PROJECT_MEMORY (Global)

Summary:
- System map:
- Architecture overview:
- UI MVP uses a bootstrapper (`GameEntry`) and a single IMGUI MonoBehaviour (`UIHudShop`) to provide HUD + Shop interactions without prefab dependencies.
- EventBus usage:
- DI conventions:
- Existing systems + how they work:
- Economy (separate namespace under `Assets/Scripts/Economy`) provides config and compute utilities; current UI MVP implements a simplified, inline econ loop (base income, streak, interest) for rapid testing.
- UI: `UIHudShop` renders HUD (gold, streak, XP bar, reroll/lock, buy XP, timer) and Shop (5 cards, buy hotkeys 1..5). Bench supports drag-drop between 8 slots and selling via Delete. Basic hover tooltips show unit name and cost.
- Future implications:
- Replace IMGUI with uGUI/UITK and proper prefabs. Integrate real EconomyManager and content (UnitDef SOs) behind ShopSystem. Add DI (Zenject) and EventBus wiring.

Notes:
- Keep concise; update when architecture/systems change.
