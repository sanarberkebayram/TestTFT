using UnityEngine;
using TestTFT.Scripts.Runtime.Combat.Ability;
using TestTFT.Scripts.Runtime.Combat.Effects;

namespace TestTFT.Scripts.Runtime.Combat.Demo
{
    public sealed class DemoFireballBootstrap : MonoBehaviour
    {
        [SerializeField] private DemoCaster caster;

        [Header("Fireball Defaults")]
        [SerializeField] private float manaCost = 50f;
        [SerializeField] private float baseDamage = 40f;
        [SerializeField, Range(0f,1f)] private float critChance = 0.2f;
        [SerializeField] private float critMultiplier = 1.5f;

        [Header("Burn Effect Defaults")]
        [SerializeField] private float burnDpsPerStack = 5f;
        [SerializeField] private float burnDuration = 4f;
        [SerializeField] private float burnTickInterval = 1f;
        [SerializeField] private int maxStacks = 1;
        [SerializeField] private StackingPolicy stacking = StackingPolicy.RefreshDuration;

        private void Awake()
        {
            if (!caster) caster = GetComponent<DemoCaster>();
        }

        private void Start()
        {
            if (caster == null) return;
            caster.SetSpell(CreateFireballDefinition());
        }

        private SpellDefinition CreateFireballDefinition()
        {
            var spell = ScriptableObject.CreateInstance<SpellDefinition>();
            spell.manaCost = Mathf.Max(0f, manaCost);
            spell.baseDamage = Mathf.Max(0f, baseDamage);
            spell.critChance = Mathf.Clamp01(critChance);
            spell.critMultiplier = Mathf.Max(1f, critMultiplier);

            var burn = ScriptableObject.CreateInstance<EffectDefinition>();
            burn.type = EffectType.Burn;
            burn.duration = Mathf.Max(0f, burnDuration);
            burn.maxStacks = Mathf.Max(1, maxStacks);
            burn.stacking = stacking;
            burn.magnitude = Mathf.Max(0f, burnDpsPerStack);
            burn.tickInterval = Mathf.Max(0.01f, burnTickInterval);

            spell.effects.Add(burn);
            return spell;
        }
    }
}
