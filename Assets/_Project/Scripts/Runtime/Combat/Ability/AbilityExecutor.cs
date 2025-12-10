using System;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;
using TestTFT.Scripts.Runtime.Combat.Effects;

namespace TestTFT.Scripts.Runtime.Combat.Ability
{
    [DisallowMultipleComponent]
    public sealed class AbilityExecutor : MonoBehaviour
    {
        [SerializeField] private ManaComponent mana;
        [SerializeField] private HealthComponent health;
        [SerializeField] private EffectsController effects;

        private void Awake()
        {
            if (!mana) mana = GetComponent<ManaComponent>();
            if (!health) health = GetComponent<HealthComponent>();
            if (!effects) effects = GetComponent<EffectsController>();
        }

        public bool CanCast(SpellDefinition spell)
        {
            if (spell == null || mana == null) return false;
            if (effects != null && effects.IsStunned) return false;
            return mana.CanAfford(spell.manaCost);
        }

        public bool TryCast(SpellDefinition spell, GameObject target)
        {
            if (!CanCast(spell)) return false;
            if (!mana.Spend(spell.manaCost)) return false; // double-guard

            // Resolve crit
            bool isCrit = TestTFT.Scripts.Runtime.Systems.Core.DeterministicRng.NextFloat01(TestTFT.Scripts.Runtime.Systems.Core.DeterministicRng.Stream.Targeting) < spell.critChance;
            float outgoingMult = effects != null ? effects.GetOutgoingDamageMultiplier() : 1f;
            float damage = Mathf.Max(0f, spell.baseDamage) * outgoingMult;
            if (isCrit)
                damage *= Mathf.Max(1f, spell.critMultiplier);

            float dealt = 0f;
            if (target != null && target.TryGetComponent<HealthComponent>(out var targetHealth))
            {
                dealt = targetHealth.ApplyDamage(damage, new DamageContext(gameObject, target, isCrit));

                // Apply effects on hit
                if (dealt > 0f && target.TryGetComponent<EffectsController>(out var targetEffects))
                {
                    foreach (var eff in spell.effects)
                    {
                        targetEffects.ApplyEffect(eff, gameObject);
                    }
                }
            }

            // Invoke on-hit hooks (local listeners)
            var info = new OnHitInfo(gameObject, target, dealt, isCrit);
            BroadcastOnHit(info);

            return true;
        }

        private static readonly System.Collections.Generic.List<IOnHitHook> _hookCache = new System.Collections.Generic.List<IOnHitHook>(8);

        private void BroadcastOnHit(OnHitInfo info)
        {
            _hookCache.Clear();
            GetComponentsInChildren(true, _hookCache);
            for (int i = 0; i < _hookCache.Count; i++)
            {
                try { _hookCache[i].OnHit(info); }
                catch (Exception) { /* swallow to protect pipeline */ }
            }

            // Mana on hit convenience
            if (mana != null && info.DamageDealt > 0f)
            {
                mana.OnHitGainMana();
            }
        }
    }
}
