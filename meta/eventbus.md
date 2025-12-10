# EventBus

Interfaces:
```
public interface IEventBus {
    void Publish<TEvent>(TEvent evt);
    void Subscribe<TEvent>(Action<TEvent> handler);
    void Unsubscribe<TEvent>(Action<TEvent> handler);
}
public interface IGlobalEventBus : IEventBus {}
public interface ISceneEventBus : IEventBus {}
```

Event types:
- Strongly typed (class/record per event), e.g. `record PlayerDamagedEvent(int Amount)`.

Usage:
- Scene bus: player, enemies, combat, level/scene lifecycle.
- Global bus: meta, profile/progression, global UI, analytics, cross-scene.
