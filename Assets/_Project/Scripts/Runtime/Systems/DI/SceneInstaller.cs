#if ZENJECT
using Zenject;
using TestTFT.Scripts.Runtime.Systems.EventBus;

namespace TestTFT.Scripts.Runtime.Systems.DI
{
    public sealed class SceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Scene-scoped services
            Container.Bind<ISceneEventBus>().To<SceneEventBus>().AsSingle();
        }
    }
}
#else
namespace TestTFT.Scripts.Runtime.Systems.DI
{
    // Zenject not present; stub to avoid compile errors
    internal sealed class SceneInstaller { }
}
#endif
