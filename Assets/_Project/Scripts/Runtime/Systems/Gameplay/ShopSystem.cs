using System;
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

        private readonly Random _rng = new Random(12345);

        public event Action OnChanged;

        public void ToggleLock()
        {
            Locked = !Locked;
            OnChanged?.Invoke();
        }

        public void Reroll()
        {
            if (Locked) return;
            for (int i = 0; i < 5; i++)
            {
                Current[i] = new Offer
                {
                    Name = RandomName(),
                    Cost = RandomCost()
                };
            }
            OnChanged?.Invoke();
        }

        public Offer Get(int index) => Current[index];

        private string RandomName()
        {
            string[] pool = { "Swordsman", "Archer", "Guardian", "Mage", "Assassin", "Bruiser", "Healer", "Knight", "Gunner", "Invoker" };
            return pool[_rng.Next(pool.Length)];
        }

        private int RandomCost()
        {
            int[] costs = { 1, 2, 3, 4, 5 };
            return costs[_rng.Next(costs.Length)];
        }
    }
}

