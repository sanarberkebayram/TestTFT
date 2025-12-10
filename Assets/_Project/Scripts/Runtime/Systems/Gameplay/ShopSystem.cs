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
            int idx = DeterministicRng.NextInt(DeterministicRng.Stream.Shop, 0, pool.Length);
            return pool[idx];
        }

        private int RandomCost()
        {
            int[] costs = { 1, 2, 3, 4, 5 };
            int idx = DeterministicRng.NextInt(DeterministicRng.Stream.Shop, 0, costs.Length);
            return costs[idx];
        }
    }
}
