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

            // Global timing
            CombatTime.EnsureStarted();

            bool isDodged = false;
            bool isCrit = false;
            float dealt = 0f;

            if (target != null && target.TryGetComponent<HealthComponent>(out var targetHealth))
            {
                // Dodge resolves before crit
                CombatStats targetStats = null;
                target.TryGetComponent<CombatStats>(out targetStats);
                if (targetStats != null && targetStats.RollDodge())
                {
                    isDodged = true;
                    dealt = 0f;
                }
                else
                {
                    // Crit resolves after dodge
                    isCrit = UnityEngine.Random.value < spell.critChance;

                    float outgoingMult = effects != null ? effects.GetOutgoingDamageMultiplier() : 1f;
                    // Apply global overtime multiplier
                    outgoingMult *= CombatTime.GetOvertimeDamageMultiplier();

                    float damage = Mathf.Max(0f, spell.baseDamage) * outgoingMult;
                    if (isCrit)
                        damage *= Mathf.Max(1f, spell.critMultiplier);

                    // Apply mitigation based on damage type
                    float mitigation = 1f;
                    if (targetStats != null)
                    {
                        mitigation = targetStats.GetMitigationMultiplier(spell.damageType);
                    }

                    float finalDamage = damage * mitigation;
                    dealt = targetHealth.ApplyDamage(finalDamage, new DamageContext(gameObject, target, isCrit));

                    // Apply effects on hit only if damage dealt
                    if (dealt > 0f && target.TryGetComponent<EffectsController>(out var targetEffects))
                    {
                        foreach (var eff in spell.effects)
                        {
                            targetEffects.ApplyEffect(eff, gameObject);
                        }
                    }
                }
            }

            // Invoke on-hit hooks (local listeners)
            var info = new OnHitInfo(gameObject, target, dealt, isCrit, isDodged, spell != null ? spell.damageType : DamageType.Physical);
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
