using UnityEngine;

namespace TestTFT.Scripts.Runtime.Combat
{
    [DisallowMultipleComponent]
    public sealed class HealthComponent : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;

        private Effects.EffectsController _effects;

        private void Awake()
        {
            _effects = GetComponent<Effects.EffectsController>();
            if (currentHealth > maxHealth) currentHealth = maxHealth;
        }

        public float ApplyDamage(float amount, DamageContext ctx)
        {
            if (amount <= 0f || currentHealth <= 0f) return 0f;

            // Let effects (e.g., shield) pre-process damage
            if (_effects != null)
            {
                amount = _effects.PreProcessIncomingDamage(amount, ctx);
            }

            if (amount <= 0f) return 0f;

            var before = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            var dealt = before - currentHealth;
            return dealt;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || currentHealth <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public bool IsDead => currentHealth <= 0f;
    }

    public readonly struct DamageContext
    {
        public readonly GameObject Source;
        public readonly GameObject Target;
        public readonly bool IsCrit;

        public DamageContext(GameObject source, GameObject target, bool isCrit)
        {
            Source = source;
            Target = target;
            IsCrit = isCrit;
        }
    }
}

