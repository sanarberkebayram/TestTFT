using UnityEngine;
using TestTFT.Scripts.Runtime.Combat.Ability;

namespace TestTFT.Scripts.Runtime.Combat.Demo
{
    public sealed class OnHitLogger : MonoBehaviour, IOnHitHook
    {
        [SerializeField] private bool logToConsole = true;

        public void OnHit(OnHitInfo info)
        {
            if (!logToConsole) return;
            Debug.Log($"OnHit: caster={info.Caster?.name}, target={info.Target?.name}, dmg={info.DamageDealt:F1}, crit={info.IsCrit}");
        }
    }
}

