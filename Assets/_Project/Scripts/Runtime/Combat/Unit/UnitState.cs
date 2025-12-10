using System;
using System.Collections.Generic;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;
using TestTFT.Scripts.Runtime.Combat.Effects;

namespace TestTFT.Scripts.Runtime.Combat.Unit
{
    [Serializable]
    public struct UnitStats
    {
        [Min(0f)] public float moveSpeed;
        [Min(0f)] public float attackRange;
        [Min(0f)] public float attackDamage;
        [Min(0f)] public float attackCooldown;

        [Min(0f)] public float castRange;
        [Min(0f)] public float castWindup;
    }

    public enum UnitStatus
    {
        Idle,
        Moving,
        Attacking,
        Casting,
        Dead
    }
}

