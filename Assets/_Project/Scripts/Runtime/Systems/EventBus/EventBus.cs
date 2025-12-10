using System;
using System.Collections.Generic;

namespace TestTFT.Scripts.Runtime.Systems.EventBus
{
    internal class EventBusBase : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        // Publish is allocation-free aside from delegate invocation
        public void Publish<TEvent>(TEvent evt)
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var del))
            {
                var action = del as Action<TEvent>;
                action?.Invoke(evt);
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null) return;

            var key = typeof(TEvent);
            if (_handlers.TryGetValue(key, out var existing))
            {
                _handlers[key] = (Action<TEvent>)existing + handler;
            }
            else
            {
                _handlers[key] = handler;
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null) return;

            var key = typeof(TEvent);
            if (_handlers.TryGetValue(key, out var existing))
            {
                var current = existing as Action<TEvent>;
                current -= handler;
                if (current == null)
                {
                    _handlers.Remove(key);
                }
                else
                {
                    _handlers[key] = current;
                }
            }
        }
    }

    public sealed class GlobalEventBus : EventBusBase, IGlobalEventBus { }
    public sealed class SceneEventBus  : EventBusBase, ISceneEventBus  { }
}
