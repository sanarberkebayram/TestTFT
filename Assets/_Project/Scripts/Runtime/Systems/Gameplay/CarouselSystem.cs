using System;
using TestTFT.Scripts.Runtime.Systems.Core;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    public sealed class CarouselSystem
    {
        public struct Offer
        {
            public string Name;
            public int Cost;
        }

        public Offer[] Current { get; private set; } = new Offer[8];
        public bool Active { get; private set; }

        public event Action OnChanged;
        public event Action<int, Offer> OnPicked;

        public void StartRound()
        {
            Active = true;
            for (int i = 0; i < Current.Length; i++)
            {
                Current[i] = new Offer
                {
                    Name = RandomName(),
                    Cost = RandomCost()
                };
            }
            OnChanged?.Invoke();
        }

        public void EndRound()
        {
            Active = false;
            OnChanged?.Invoke();
        }

        public bool TryPick(int index)
        {
            if (!Active) return false;
            if (index < 0 || index >= Current.Length) return false;
            var offer = Current[index];
            Active = false;
            OnPicked?.Invoke(index, offer);
            OnChanged?.Invoke();
            return true;
        }

        public Offer Get(int index) => Current[index];

        private string RandomName()
        {
            string[] pool = { "Berserker", "Sniper", "Paladin", "Sorcerer", "Rogue", "Vanguard", "Oracle", "Monk", "Brawler", "Enchanter" };
            int idx = DeterministicRng.NextInt(DeterministicRng.Stream.Carousel, 0, pool.Length);
            return pool[idx];
        }

        private int RandomCost()
        {
            int[] costs = { 1, 2, 3, 4, 5 };
            int idx = DeterministicRng.NextInt(DeterministicRng.Stream.Carousel, 0, costs.Length);
            return costs[idx];
        }
    }
}

