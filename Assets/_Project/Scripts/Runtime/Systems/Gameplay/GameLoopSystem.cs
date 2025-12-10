using System;
using UnityEngine;
using TestTFT.Scripts.Runtime.Systems.Core;

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
        public int Stage { get; private set; } = 1;
        public int StageRound { get; private set; } = 1;
        public bool IsPvE { get; private set; } = true;

        public event Action<Phase> OnPhaseChanged;
        public event Action<float, float> OnTimer;
        public event Action<int, int, bool> OnRoundStarted; // stage, stageRound, isPvE
        public event Action<bool, bool, int> OnRoundResolved; // isPvE, win, damageToPlayer

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
            // Ensure round state is configured and broadcast on manual force
            ConfigureRoundForCurrentIndex();
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
                // Combat timer; keep aligned with CombatTime overtime threshold
                PhaseDuration = 15f;
                OnPhaseChanged?.Invoke(CurrentPhase);
                OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
                return;
            }
            else if (CurrentPhase == Phase.Combat)
            {
                // Resolve the just-finished combat round first
                var (isPve, win, damage) = ResolveRoundOutcome();
                OnRoundResolved?.Invoke(isPve, win, damage);

                // Progress round counters for the next round
                IncrementRound();

                if (IsCarouselRound(RoundIndex))
                {
                    CurrentPhase = Phase.Carousel;
                    PhaseDuration = 12f; // short carousel window
                }
                else
                {
                    CurrentPhase = Phase.Shop;
                    // Configure round (PvE/PvP) and durations for the next round's shop
                    ConfigureRoundForCurrentIndex();
                }
            }
            else // Carousel -> Shop
            {
                // After carousel finishes, enter the next round's shop
                CurrentPhase = Phase.Shop;
                ConfigureRoundForCurrentIndex();
            }
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
        }

        private void IncrementRound()
        {
            RoundIndex++;
            // Simple staging: first 3 rounds = Stage 1 (PvE), then each 5 rounds per stage PvP
            if (RoundIndex <= 3)
            {
                Stage = 1;
                StageRound = RoundIndex; // 1-3
            }
            else
            {
                // After PvE opener, define 5 rounds per stage
                var offset = RoundIndex - 3; // starts at 1 on first PvP round
                Stage = 1 + ((offset - 1) / 5) + 1; // Stage 2 starts at offset 1
                StageRound = ((offset - 1) % 5) + 1; // 1..5
            }
        }

        private void ConfigureRoundForCurrentIndex()
        {
            // Determine if PvE and set durations
            if (RoundIndex <= 3)
            {
                // PvE opener rounds with shorter shop
                IsPvE = true;
                PhaseDuration = 15f; // shop time
            }
            else
            {
                IsPvE = false;
                PhaseDuration = 20f; // shop time
            }

            OnRoundStarted?.Invoke(Stage, StageRound, IsPvE);
        }

        private (bool isPve, bool win, int damageToPlayer) ResolveRoundOutcome()
        {
            bool isPve = IsPvE;
            // Placeholder resolution: random outcome for PvP, always win PvE for MVP
            bool win = isPve ? true : DeterministicRng.NextFloat01(DeterministicRng.Stream.Loot) > 0.5f;
            int damage = 0;
            if (!isPve && !win)
            {
                // Damage = StageBase + SurvivingStarDamage (MVP: random survivors proxy)
                int stageBase = Mathf.Max(1, Stage);
                int survivingStarDamage = ComputeSurvivingStarDamage();
                damage = stageBase + survivingStarDamage;
            }
            return (isPve, win, damage);
        }

        // MVP approximation: return 0-3 star damage using deterministic RNG
        private int ComputeSurvivingStarDamage()
        {
            // Later: compute from actual surviving enemy units and their star levels
            return DeterministicRng.NextInt(DeterministicRng.Stream.Loot, 0, 4); // 0..3
        }

        // Allows UI to finish carousel early (e.g., after pick)
        public void EndCarouselNow()
        {
            if (CurrentPhase != Phase.Carousel) return;
            PhaseTime = 0f;
            CurrentPhase = Phase.Shop;
            PhaseDuration = 20f;
            // Entering shop for the next configured round (round index already incremented in Combat branch)
            ConfigureRoundForCurrentIndex();
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnTimer?.Invoke(PhaseDuration - PhaseTime, PhaseDuration);
        }
    }
}
