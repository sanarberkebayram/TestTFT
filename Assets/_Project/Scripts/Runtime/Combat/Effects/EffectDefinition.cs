using UnityEngine;

namespace TestTFT.Scripts.Runtime.Combat.Effects
{
    [CreateAssetMenu(menuName = "_Project/Effects/Effect Definition", fileName = "EffectDefinition")]
    public sealed class EffectDefinition : ScriptableObject
    {
        public EffectType type = EffectType.Buff;
        [Min(0f)] public float duration = 5f;
        [Min(0)] public int maxStacks = 1;
        public StackingPolicy stacking = StackingPolicy.RefreshDuration;

        [Header("Values by Type")]
        [Tooltip("Buff: outgoing damage multiplier per stack. Shield: shield amount per stack. Burn: damage per tick per stack.")]
        public float magnitude = 0f;

        [Tooltip("Applies to Burn: seconds between ticks. If 0, defaults to 1s.")]
        [Min(0f)] public float tickInterval = 1f;
    }
}

