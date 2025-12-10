using System;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    public sealed class GameLoopSystem
    {
        public enum Phase
        {
            Shop,
            Combat
        }

        public Phase CurrentPhase { get; private set; } = Phase.Shop;
        public float PhaseTime { get; private set; }
        public float PhaseDuration { get; private set; } = 20f;
        public int RoundIndex { get; private set; } = 1;

        public event Action<Phase> OnPhaseChanged;
        public event Action<float, float> OnTimer;

        public void Tick(float deltaTime)
        {
            PhaseTime += deltaTime;
            if (PhaseTime > PhaseDuration)
            {
                Advance();
            }
            else
            {
                OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
            }
        }

        public void ForceShop(float duration = 20f)
        {
            CurrentPhase = Phase.Shop;
            PhaseDuration = duration;
            PhaseTime = 0f;
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
        }

        private void Advance()
        {
            PhaseTime = 0f;
            if (CurrentPhase == Phase.Shop)
            {
                CurrentPhase = Phase.Combat;
                PhaseDuration = 15f;
            }
            else
            {
                CurrentPhase = Phase.Shop;
                PhaseDuration = 20f;
                RoundIndex++;
            }
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
        }
    }
}

