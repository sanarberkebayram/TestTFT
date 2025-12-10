#if ZENJECT
using Zenject;
using TestTFT.Scripts.Runtime.Systems.EventBus;

namespace TestTFT.Scripts.Runtime.Systems.DI
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Global singletons
            Container.Bind<IGlobalEventBus>().To<GlobalEventBus>().AsSingle();
        }
    }
}
#else
namespace TestTFT.Scripts.Runtime.Systems.DI
{
    // Zenject not present; stub to avoid compile errors
    internal sealed class ProjectInstaller { }
}
#endif
