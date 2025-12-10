using System.Collections.Generic;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat.Effects;

namespace TestTFT.Scripts.Runtime.Combat.Ability
{
    [CreateAssetMenu(menuName = "_Project/Ability/Spell Definition", fileName = "SpellDefinition")]
    public sealed class SpellDefinition : ScriptableObject
    {
        [Header("Mana & Cost")]
        [Min(0f)] public float manaCost = 0f;

        [Header("Damage & Crit")]
        [Min(0f)] public float baseDamage = 0f;
        [Range(0f, 1f)] public float critChance = 0f;
        [Min(1f)] public float critMultiplier = 1.5f;

        [Header("Effects to Apply (on Hit)")]
        public List<EffectDefinition> effects = new List<EffectDefinition>();
    }
}

