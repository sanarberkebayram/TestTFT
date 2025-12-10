using TestTFT.Scripts.Runtime.Systems.EventBus;

namespace TestTFT.Scripts.Runtime.Systems.Core
{
    // Lightweight provider to access event buses when Zenject isn't present.
    public static class EventBusProvider
    {
        private static IGlobalEventBus _global;
        private static ISceneEventBus _scene;

        public static IGlobalEventBus Global => _global ??= new GlobalEventBus();
        public static ISceneEventBus Scene => _scene ??= new SceneEventBus();

        // Allow swapping in tests or via DI installers if desired
        public static void SetGlobal(IGlobalEventBus bus) { _global = bus; }
        public static void SetScene(ISceneEventBus bus) { _scene = bus; }
    }
}

