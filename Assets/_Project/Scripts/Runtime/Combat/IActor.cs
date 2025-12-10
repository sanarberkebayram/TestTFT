using UnityEngine;

namespace TestTFT.Scripts.Runtime.Combat
{
    public interface IActor
    {
        GameObject gameObject { get; }
        Transform transform { get; }
        HealthComponent Health { get; }
        ManaComponent Mana { get; }
        Effects.EffectsController Effects { get; }
    }
}

