using System;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Combat.Ability
{
    public readonly struct OnHitInfo
    {
        public readonly GameObject Caster;
        public readonly GameObject Target;
        public readonly float DamageDealt;
        public readonly bool IsCrit;

        public OnHitInfo(GameObject caster, GameObject target, float damageDealt, bool isCrit)
        {
            Caster = caster;
            Target = target;
            DamageDealt = damageDealt;
            IsCrit = isCrit;
        }
    }

    public interface IOnHitHook
    {
        void OnHit(OnHitInfo info);
    }
}

