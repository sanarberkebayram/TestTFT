using System.Collections.Generic;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat.Unit;

namespace TestTFT.Scripts.Runtime.Systems.Simulation
{
    [DefaultExecutionOrder(-1000)]
    public sealed class SimulationSystem : MonoBehaviour
    {
        public static readonly List<UnitBehaviour> Units = new List<UnitBehaviour>(64);

        [Header("Fixed Step Settings")]
        [SerializeField, Min(0.001f)] private float fixedDelta = 0.1f; // 10 Hz
        [SerializeField] private int randomSeed = 12345;

        private float _accumulator;

        public float FixedDelta => fixedDelta;

        private void OnEnable()
        {
            UnityEngine.Random.InitState(randomSeed);
            _accumulator = 0f;
        }

        private void Update()
        {
            _accumulator += Time.deltaTime;
            while (_accumulator + 1e-6f >= fixedDelta)
            {
                TickOnce();
                _accumulator -= fixedDelta;
            }
        }

        public void TickOnce()
        {
            // Deterministic iteration order: do not reorder the Units list
            for (int i = 0; i < Units.Count; i++)
            {
                var u = Units[i];
                if (u != null && u.isActiveAndEnabled)
                {
                    u.SimTick(fixedDelta);
                }
            }
        }

        // Finds the nearest living unit excluding the seeker itself.
        // Note: Team logic TBD — currently treats all other units as hostile.
        public static UnitBehaviour FindNearestLivingUnit(UnitBehaviour seeker)
        {
            if (seeker == null) return null;
            var seekerPos = seeker.transform.position;
            UnitBehaviour best = null;
            float bestDistSq = float.PositiveInfinity;

            for (int i = 0; i < Units.Count; i++)
            {
                var u = Units[i];
                if (u == null || u == seeker) continue;
                if (!u.isActiveAndEnabled) continue;
                var h = u.Health;
                if (h == null || h.IsDead) continue;

                var d = (u.transform.position - seekerPos);
                float distSq = d.x * d.x + d.y * d.y + d.z * d.z;
                if (distSq < bestDistSq - 1e-6f)
                {
                    bestDistSq = distSq;
                    best = u;
                }
                else if (Mathf.Abs(distSq - bestDistSq) <= 1e-6f)
                {
                    // Tie-breaker: stable by instanceID to keep determinism
                    if (u.GetInstanceID() < (best?.GetInstanceID() ?? int.MaxValue))
                    {
                        best = u;
                    }
                }
            }

            return best;
        }

        public static void Register(UnitBehaviour u)
        {
            if (u != null && !Units.Contains(u)) Units.Add(u);
        }

        public static void Unregister(UnitBehaviour u)
        {
            if (u != null) Units.Remove(u);
        }

        public static void ClearUnits()
        {
            Units.Clear();
        }
    }
}
