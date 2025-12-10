using System.Collections.Generic;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;

namespace TestTFT.Scripts.Runtime.Combat.Effects
{
    [DisallowMultipleComponent]
    public sealed class EffectsController : MonoBehaviour
    {
        private sealed class ActiveEffect
        {
            public EffectDefinition def;
            public int stacks;
            public float remaining;
            public float nextTickAt;
            public float shieldRemaining; // for Shield
        }

        private readonly List<ActiveEffect> _effects = new List<ActiveEffect>(8);

        private HealthComponent _health;
        private bool _stunned;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var e = _effects[i];
                e.remaining -= dt;

                if (e.def.type == EffectType.Burn)
                {
                    float interval = e.def.tickInterval > 0f ? e.def.tickInterval : 1f;
                    e.nextTickAt -= dt;
                    while (e.nextTickAt <= 0f && e.remaining > 0f)
                    {
                        var dmg = Mathf.Max(0f, e.def.magnitude) * e.stacks;
                        if (dmg > 0f && _health != null)
                        {
                            _health.ApplyDamage(dmg, new DamageContext(gameObject, gameObject, false));
                        }
                        e.nextTickAt += interval;
                    }
                }

                if (e.remaining <= 0f)
                {
                    if (e.def.type == EffectType.Stun)
                        _stunned = false;

                    _effects.RemoveAt(i);
                }
            }
        }

        public void ApplyEffect(EffectDefinition def, GameObject source)
        {
            if (def == null) return;

            var existingIndex = _effects.FindIndex(a => a.def == def);
            if (existingIndex >= 0)
            {
                var e = _effects[existingIndex];
                if (e.stacks < def.maxStacks)
                {
                    e.stacks++;
                    if (def.type == EffectType.Shield)
                        e.shieldRemaining += Mathf.Max(0f, def.magnitude);
                }

                switch (def.stacking)
                {
                    case StackingPolicy.RefreshDuration:
                        e.remaining = def.duration;
                        break;
                    case StackingPolicy.ExtendDuration:
                        e.remaining += def.duration;
                        break;
                }

                if (def.type == EffectType.Burn)
                {
                    var interval = def.tickInterval > 0f ? def.tickInterval : 1f;
                    if (e.nextTickAt > interval)
                        e.nextTickAt = interval; // keep soonest tick
                }
            }
            else
            {
                var e = new ActiveEffect
                {
                    def = def,
                    stacks = 1,
                    remaining = def.duration,
                    nextTickAt = def.type == EffectType.Burn ? (def.tickInterval > 0f ? def.tickInterval : 1f) : 0f,
                    shieldRemaining = def.type == EffectType.Shield ? Mathf.Max(0f, def.magnitude) : 0f
                };

                _effects.Add(e);

                if (def.type == EffectType.Stun)
                    _stunned = true;
            }
        }

        public float PreProcessIncomingDamage(float amount, DamageContext ctx)
        {
            if (amount <= 0f) return amount;

            // Shield absorbs damage first
            for (int i = 0; i < _effects.Count && amount > 0f; i++)
            {
                var e = _effects[i];
                if (e.def.type != EffectType.Shield || e.shieldRemaining <= 0f) continue;

                var absorb = Mathf.Min(e.shieldRemaining, amount);
                e.shieldRemaining -= absorb;
                amount -= absorb;

                if (e.shieldRemaining <= 0f)
                {
                    // Shield depleted; remove effect instance immediately
                    e.remaining = 0f;
                }
            }

            return amount;
        }

        public bool IsStunned => _stunned;

        public float GetOutgoingDamageMultiplier()
        {
            float mult = 1f;
            for (int i = 0; i < _effects.Count; i++)
            {
                var e = _effects[i];
                if (e.def.type == EffectType.Buff)
                {
                    mult *= Mathf.Max(0f, 1f + (e.def.magnitude * e.stacks));
                }
            }
            return mult;
        }
    }
}
