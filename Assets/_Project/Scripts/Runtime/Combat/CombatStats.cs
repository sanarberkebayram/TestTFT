using UnityEngine;

namespace TestTFT.Scripts.Runtime.Combat
{
    [DisallowMultipleComponent]
    public sealed class CombatStats : MonoBehaviour
    {
        [Header("Defenses")]
        public float armor = 0f;
        public float magicResist = 0f;

        [Header("Avoidance")]
        [Range(0f, 1f)] public float dodgeChance = 0f;

        public float GetMitigationMultiplier(DamageType type)
        {
            switch (type)
            {
                case DamageType.Physical:
                    return MitigationFromStat(armor);
                case DamageType.Magic:
                    return MitigationFromStat(magicResist);
                case DamageType.True:
                    return 1f;
                default:
                    return 1f;
            }
        }

        private static float MitigationFromStat(float stat)
        {
            // 100 / (100 + stat) works for positive and negative stats
            return 100f / (100f + stat);
        }

        public bool RollDodge()
        {
            if (dodgeChance <= 0f) return false;
            if (dodgeChance >= 1f) return true;
            return Random.value < dodgeChance;
        }
    }
}

