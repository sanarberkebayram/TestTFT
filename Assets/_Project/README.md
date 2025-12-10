# _Project Structure

This folder follows the `/meta/AGENTS.md` guidelines:

- Scripts structure under `Assets/_Project/Scripts/`:
  - Runtime/ (Player, Enemies, Systems, UI, Tools)
  - Data/ (Items, Abilities, Enemies, Configs)
  - Editor/ (Inspectors, Windows, Tools)

Included infrastructure:
- Strongly-typed EventBus interfaces and concrete implementations.
- Zenject `ProjectInstaller` and `SceneInstaller` (guarded by `#if ZENJECT`).

Namespaces mirror the folder path, using `TestTFT` as the root.

## UI MVP (HUD + Shop)

- Entry: `Systems/Bootstrap/GameEntry.cs` ensures `UIBootstrap` is spawned at runtime.
- Systems: `EconomySystem`, `ShopSystem`, `GameLoopSystem` drive the loop.
- HUD: Gold, streak, XP bar, reroll/lock, timer.
- Shop: 5 cards with buy buttons and keys [1..5].
- Drag-drop: Draggable units on 8 bench slots; Delete key sells selected unit.
- Tooltips: Basic hover tooltips on units.

How to run:
- Open `Assets/Scenes/SampleScene.unity` and press Play.
- Use keys 1..5 to buy; click Reroll/Lock; press Delete to sell a selected bench unit; drag units between bench slots.
