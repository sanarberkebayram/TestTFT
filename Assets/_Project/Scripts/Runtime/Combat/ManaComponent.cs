using UnityEngine;

namespace TestTFT.Scripts.Runtime.Combat
{
    [DisallowMultipleComponent]
    public sealed class ManaComponent : MonoBehaviour
    {
        [Header("Mana Settings")]
        [SerializeField] private float maxMana = 100f;
        [SerializeField] private float startMana = 0f;
        [SerializeField] private float regenPerSecond = 0f;
        [SerializeField] private float manaOnHit = 0f;

        [SerializeField] private float currentMana;

        [Header("Simulation")] 
        [SerializeField] private bool useExternalTick = false;

        public float MaxMana => maxMana;
        public float StartMana => startMana;
        public float CurrentMana => currentMana;
        public float RegenPerSecond => regenPerSecond;
        public float ManaOnHit => manaOnHit;

        private void Awake()
        {
            currentMana = Mathf.Clamp(startMana, 0f, maxMana);
        }

        private void Update()
        {
            if (useExternalTick) return;
            SimTick(Time.deltaTime);
        }

        public void SimTick(float dt)
        {
            if (regenPerSecond > 0f && currentMana < maxMana)
            {
                GainMana(regenPerSecond * dt);
            }
        }

        public void GainMana(float amount)
        {
            if (amount <= 0f) return;
            currentMana = Mathf.Min(maxMana, currentMana + amount);
        }

        public bool CanAfford(float cost) => currentMana >= cost;

        public bool Spend(float cost)
        {
            if (!CanAfford(cost)) return false;
            currentMana -= cost;
            return true;
        }

        // Hook to grant mana on hit from external systems
        public void OnHitGainMana()
        {
            if (manaOnHit > 0f)
                GainMana(manaOnHit);
        }

        public void SetUseExternalTick(bool value)
        {
            useExternalTick = value;
        }
    }
}
