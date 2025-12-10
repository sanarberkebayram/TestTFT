using System;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    public sealed class GameLoopSystem
    {
        public enum Phase
        {
            Shop,
            Combat,
            Carousel
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

        private bool IsCarouselRound(int round)
        {
            // Simple MVP rule: every 3rd round has a carousel
            return round > 0 && (round % 3 == 0);
        }

        private void Advance()
        {
            PhaseTime = 0f;
            if (CurrentPhase == Phase.Shop)
            {
                CurrentPhase = Phase.Combat;
                PhaseDuration = 15f;
            }
            else if (CurrentPhase == Phase.Combat)
            {
                if (IsCarouselRound(RoundIndex + 1))
                {
                    CurrentPhase = Phase.Carousel;
                    PhaseDuration = 12f; // short carousel window
                }
                else
                {
                    CurrentPhase = Phase.Shop;
                    PhaseDuration = 20f;
                    RoundIndex++;
                }
            }
            else // Carousel -> Shop
            {
                CurrentPhase = Phase.Shop;
                PhaseDuration = 20f;
                RoundIndex++;
            }
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
        }

        // Allows UI to finish carousel early (e.g., after pick)
        public void EndCarouselNow()
        {
            if (CurrentPhase != Phase.Carousel) return;
            PhaseTime = 0f;
            CurrentPhase = Phase.Shop;
            PhaseDuration = 20f;
            RoundIndex++;
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
        }
    }
}
