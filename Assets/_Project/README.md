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
