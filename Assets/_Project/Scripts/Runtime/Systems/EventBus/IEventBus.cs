using System;

namespace TestTFT.Scripts.Runtime.Systems.EventBus
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evt);
        void Subscribe<TEvent>(Action<TEvent> handler);
        void Unsubscribe<TEvent>(Action<TEvent> handler);
    }

    public interface IGlobalEventBus : IEventBus {}
    public interface ISceneEventBus : IEventBus {}
}
