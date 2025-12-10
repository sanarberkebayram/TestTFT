using System;
using System.Collections.Generic;
using TestTFT.Scripts.Runtime.Systems.Core;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    public sealed class ShopSystem
    {
        public struct Offer
        {
            public string Name;
            public int Cost;
        }

        public Offer[] Current { get; private set; } = new Offer[5];
        public bool Locked { get; private set; }

        // RNG handled via DeterministicRng with the Shop stream

        public event Action OnChanged;

        // --- Pools and odds (MVP) ---
        // Tiers map to costs 1..3
        private readonly string[] _tier1 = { "Swordsman", "Archer", "Guardian", "Mage", "Assassin" };
        private readonly string[] _tier2 = { "Bruiser", "Healer", "Knight", "Gunner", "Invoker" };
        private readonly string[] _tier3 = { "Sentinel", "Shaman", "Berserker", "Oracle", "Vanguard" };

        // Copies per unit by tier (small numbers for demo)
        private readonly Dictionary<int, int> _copiesPerTier = new()
        {
            {1, 10}, {2, 7}, {3, 5}
        };

        // pool[cost][name] = remaining copies
        private readonly Dictionary<int, Dictionary<string, int>> _pool = new();

        public ShopSystem()
        {
            InitPools();
        }

        private void InitPools()
        {
            _pool.Clear();
            _pool[1] = new Dictionary<string, int>();
            _pool[2] = new Dictionary<string, int>();
            _pool[3] = new Dictionary<string, int>();
            foreach (var n in _tier1) _pool[1][n] = _copiesPerTier[1];
            foreach (var n in _tier2) _pool[2][n] = _copiesPerTier[2];
            foreach (var n in _tier3) _pool[3][n] = _copiesPerTier[3];
        }

        public void ToggleLock()
        {
            Locked = !Locked;
            OnChanged?.Invoke();
        }

        public void RerollForLevel(int level)
        {
            if (Locked) return;
            for (int i = 0; i < 5; i++)
            {
                int cost = RollCostByLevel(level);
                var name = DrawFromPool(cost);
                Current[i] = new Offer { Name = name, Cost = cost };
            }
            OnChanged?.Invoke();
        }

        public Offer Get(int index) => Current[index];

        // Odds table for costs [1,2,3] by player level (1..9)
        private static readonly float[][] Odds = new float[][]
        {
            // dummy 0 index (unused)
            new float[]{1,0,0},
            // L1..L9
            new float[]{1.00f, 0.00f, 0.00f},
            new float[]{0.85f, 0.15f, 0.00f},
            new float[]{0.70f, 0.25f, 0.05f},
            new float[]{0.55f, 0.35f, 0.10f},
            new float[]{0.45f, 0.40f, 0.15f},
            new float[]{0.30f, 0.45f, 0.25f},
            new float[]{0.20f, 0.45f, 0.35f},
            new float[]{0.15f, 0.40f, 0.45f},
            new float[]{0.10f, 0.35f, 0.55f},
        };

        private int RollCostByLevel(int level)
        {
            int lvl = Math.Clamp(level, 1, 9);
            var odds = Odds[lvl];
            float r = DeterministicRng.NextFloat01(DeterministicRng.Stream.Shop);
            if (r < odds[0]) return 1;
            if (r < odds[0] + odds[1]) return 2;
            return 3;
        }

        private string DrawFromPool(int cost)
        {
            if (!_pool.ContainsKey(cost)) return "—";
            var dict = _pool[cost];
            // Build a temp list of available names
            var available = new List<string>();
            foreach (var kvp in dict)
            {
                if (kvp.Value > 0) available.Add(kvp.Key);
            }
            if (available.Count == 0) return "Sold Out";
            int idx = DeterministicRng.NextInt(DeterministicRng.Stream.Shop, 0, available.Count);
            string name = available[idx];
            dict[name] = dict[name] - 1;
            return name;
        }

        public void ReturnToPool(string name, int cost)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (!_pool.ContainsKey(cost)) return;
            var dict = _pool[cost];
            if (!dict.ContainsKey(name)) dict[name] = 0;
            dict[name] = dict[name] + 1;
        }
    }
}
