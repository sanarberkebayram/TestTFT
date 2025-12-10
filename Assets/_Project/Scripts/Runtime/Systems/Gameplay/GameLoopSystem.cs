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
            else
            {
                // Resolve round on combat end, then move to next round's shop
                var (isPve, win, damage) = ResolveRoundOutcome();
                OnRoundResolved?.Invoke(isPve, win, damage);

                // Progress round counters
                IncrementRound();

                // Prepare next round (shop)
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
            bool win = isPve ? true : UnityEngine.Random.value > 0.5f;
            int damage = 0;
            if (!isPve && !win)
            {
                // Simple damage model: base damage equals current stage
                damage = Mathf.Max(1, Stage);
            }
            return (isPve, win, damage);
        }
    }
}
