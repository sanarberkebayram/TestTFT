using System;
using TestTFT.Scripts.Runtime.Systems.Core;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    public sealed class EconomySystem
    {
        public int Gold { get; private set; } = 10;
        public int Streak { get; private set; } = 0;
        public int Level { get; private set; } = 2;
        public int Xp { get; private set; } = 0;
        public int XpToLevel { get; private set; } = 6;

        public event Action OnChanged;

        public void AddGold(int amount)
        {
            if (amount == 0) return;
            Gold = Math.Max(0, Gold + amount);
            OnChanged?.Invoke();
        }

        public void GainInterest()
        {
            var interest = Math.Min(5, Gold / 10);
            if (interest > 0) AddGold(interest);
        }

        public bool TrySpend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            OnChanged?.Invoke();
            return true;
        }

        public void AddStreak(bool win)
        {
            Streak = win ? Math.Min(99, Streak + 1) : 0;
            OnChanged?.Invoke();
        }

        public void AddXp(int amount)
        {
            Xp += amount;
            while (Xp >= XpToLevel)
            {
                Xp -= XpToLevel;
                Level++;
                XpToLevel += 2; // simple curve
            }
            OnChanged?.Invoke();
        }
    }
}

