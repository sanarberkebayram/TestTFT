# Architecture

- Engine: Unity 2023+, URP.
- Language: C#.
- DI: Zenject (no Signals).
- Messaging: Custom `IGlobalEventBus` / `ISceneEventBus` (strongly typed).
- Data: ScriptableObject-centric, data-only.
- Performance: High; zero allocs in hot paths; no LINQ in updates.

Philosophy:
- SOLID, low coupling, feature-based.
- Extensible systems; DI-driven; testable.
- Maintainable, production-grade structure.
