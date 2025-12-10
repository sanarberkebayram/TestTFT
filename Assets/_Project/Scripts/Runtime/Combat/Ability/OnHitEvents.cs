using System;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;

namespace TestTFT.Scripts.Runtime.Combat.Ability
{
    public readonly struct OnHitInfo
    {
        public readonly GameObject Caster;
        public readonly GameObject Target;
        public readonly float DamageDealt;
        public readonly bool IsCrit;
        public readonly bool IsDodged;
        public readonly DamageType DamageType;

        public OnHitInfo(GameObject caster, GameObject target, float damageDealt, bool isCrit, bool isDodged, DamageType damageType)
        {
            Caster = caster;
            Target = target;
            DamageDealt = damageDealt;
            IsCrit = isCrit;
            IsDodged = isDodged;
            DamageType = damageType;
        }
    }

    public interface IOnHitHook
    {
        void OnHit(OnHitInfo info);
    }
}
