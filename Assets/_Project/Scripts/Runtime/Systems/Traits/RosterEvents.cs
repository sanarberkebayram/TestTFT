using System;

namespace TestTFT.Scripts.Runtime.Systems.Traits
{
    public static class RosterEvents
    {
        public static event Action OnRosterChanged;

        public static void Raise()
        {
            OnRosterChanged?.Invoke();
        }
    }
}

