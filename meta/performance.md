# Performance Rules

- No allocations in Update/FixedUpdate/LateUpdate.
- No LINQ in hot paths.
- Cache components; preallocate lists.
- Use object pooling for spawn/despawn.
- EventBus ops must be lightweight.
