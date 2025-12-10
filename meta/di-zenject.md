# Dependency Injection (Zenject)

Usage:
- Prefer constructor injection for pure C# services.
- Use `[Inject]` for MonoBehaviours only when needed.

Installers:
- `ProjectInstaller`: binds global services (singletons).
- `SceneInstaller`: binds scene-scoped services.
