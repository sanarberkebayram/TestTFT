using System.Collections.Generic;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;
using TestTFT.Scripts.Runtime.Combat.Ability;
using TestTFT.Scripts.Runtime.Combat.Effects;

namespace TestTFT.Scripts.Runtime.Combat.Unit
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(ManaComponent))]
    [RequireComponent(typeof(EffectsController))]
    public sealed class UnitBehaviour : MonoBehaviour, IActor
    {
        [Header("Stats")]
        public UnitStats stats = new UnitStats
        {
            moveSpeed = 2f,
            attackRange = 1.5f,
            attackDamage = 10f,
            attackCooldown = 1.0f,
            castRange = 6f,
            castWindup = 0.5f
        };

        [Header("Combat")]
        [SerializeField] private SpellDefinition spell;
        [SerializeField] private AbilityExecutor executor;

        [Header("Runtime State")]
        [SerializeField] private GameObject target;
        [SerializeField] private UnitStatus status;

        private float _attackCd;
        private float _castTimer;

        private readonly List<string> _inventory = new List<string>();

        public HealthComponent Health { get; private set; }
        public ManaComponent Mana { get; private set; }
        public EffectsController Effects { get; private set; }

        GameObject IActor.gameObject => gameObject;
        Transform IActor.transform => transform;

        public UnitStatus Status => status;
        public GameObject CurrentTarget => target;
        public IReadOnlyList<string> Inventory => _inventory;

        private void Awake()
        {
            Health = GetComponent<HealthComponent>();
            Mana = GetComponent<ManaComponent>();
            Effects = GetComponent<EffectsController>();
            if (!executor) executor = GetComponent<AbilityExecutor>();

            // Ensure Mana/Effects run under external tick for determinism
            Mana.SetUseExternalTick(true);
            Effects.SetUseExternalTick(true);
        }

        private void OnEnable()
        {
            Systems.Simulation.SimulationSystem.Register(this);
        }

        private void OnDisable()
        {
            Systems.Simulation.SimulationSystem.Unregister(this);
        }

        public void SetTarget(GameObject t)
        {
            target = t;
        }

        public void SimTick(float dt)
        {
            if (Health.IsDead)
            {
                status = UnitStatus.Dead;
                return;
            }

            // Tick sub-systems deterministically
            Mana.SimTick(dt);
            Effects.SimTick(dt);

            if (Effects.IsStunned)
            {
                status = UnitStatus.Idle;
                // Cooldowns still progress while stunned
                if (_attackCd > 0f) _attackCd -= dt;
                if (_castTimer > 0f) _castTimer = Mathf.Max(0f, _castTimer - dt);
                return;
            }

            // Refresh cooldowns
            if (_attackCd > 0f) _attackCd -= dt;

            // Ensure we have a valid target; try to auto-acquire nearest living unit if missing or dead
            bool needTarget = false;
            if (target == null || !target.activeInHierarchy)
            {
                needTarget = true;
            }
            else
            {
                var th = target.GetComponent<HealthComponent>();
                if (th == null || th.IsDead) needTarget = true;
            }

            if (needTarget)
            {
                var nearest = Systems.Simulation.SimulationSystem.FindNearestLivingUnit(this);
                if (nearest != null)
                {
                    target = nearest.gameObject;
                }
                else
                {
                    status = UnitStatus.Idle;
                    return;
                }
            }

            float dist = Vector3.Distance(transform.position, target.transform.position);

            // Prefer casting if affordable and in range
            bool canCast = spell != null && executor != null && executor.CanCast(spell) && dist <= stats.castRange;
            if (canCast)
            {
                status = UnitStatus.Casting;
                _castTimer += dt;
                if (_castTimer >= Mathf.Max(0f, stats.castWindup))
                {
                    _castTimer = 0f;
                    executor.TryCast(spell, target);
                }
                return;
            }

            // Otherwise, attack if in range
            if (dist <= stats.attackRange && _attackCd <= 0f)
            {
                status = UnitStatus.Attacking;
                DoAttack(target);
                _attackCd = Mathf.Max(0.01f, stats.attackCooldown);
                return;
            }

            // Move toward target
            status = UnitStatus.Moving;
            if (stats.moveSpeed > 0f)
            {
                var dir = (target.transform.position - transform.position);
                var step = stats.moveSpeed * dt;
                var len = dir.magnitude;
                if (len > 1e-5f)
                {
                    var move = dir / len * Mathf.Min(step, len - Mathf.Max(0f, dist - stats.attackRange));
                    transform.position += move;
                }
            }
        }

        private void DoAttack(GameObject tgt)
        {
            if (tgt == null) return;
            if (tgt.TryGetComponent<HealthComponent>(out var h))
            {
                var dmg = Mathf.Max(0f, stats.attackDamage);
                var ctx = new DamageContext(gameObject, tgt, false);
                h.ApplyDamage(dmg, ctx);
                // Grant mana on hit
                Mana.OnHitGainMana();
            }
        }

        public void AddItem(string id)
        {
            if (!string.IsNullOrEmpty(id)) _inventory.Add(id);
        }

        public bool RemoveItem(string id)
        {
            return _inventory.Remove(id);
        }
    }
}
