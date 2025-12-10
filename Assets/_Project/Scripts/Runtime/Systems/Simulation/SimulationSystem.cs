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
