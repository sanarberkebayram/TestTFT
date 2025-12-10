using System;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    public sealed class PlayerHealthSystem
    {
        public int MaxHp { get; }
        public int Hp { get; private set; }

        public event Action OnChanged;

        public PlayerHealthSystem(int maxHp = 100)
        {
            MaxHp = Math.Max(1, maxHp);
            Hp = MaxHp;
        }

        public void ApplyDamage(int dmg)
        {
            if (dmg <= 0) return;
            Hp = Math.Max(0, Hp - dmg);
            OnChanged?.Invoke();
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;
            Hp = Math.Min(MaxHp, Hp + amount);
            OnChanged?.Invoke();
        }

        public bool IsDead => Hp <= 0;
    }
}

