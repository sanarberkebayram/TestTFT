# Unity Lifecycle (Strict)

Awake:
- Cache Unity components only; do not use injected services.

OnEnable:
- Subscribe to EventBus; bind inputs.

Start:
- Initialize using injected services; scene setup.

Update:
- Input checks and lightweight state logic only.
- Forbidden: allocations, LINQ, instantiation/deletion, heavy loops.

FixedUpdate:
- Physics only.

LateUpdate:
- Camera follow; post-frame adjustments.

OnDisable/OnDestroy:
- Unsubscribe/cleanup listeners and coroutines.
