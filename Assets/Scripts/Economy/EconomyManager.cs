using System;

namespace Economy
{
    public class EconomyManager
    {
        private readonly EconomyConfig _config;

        public EconomyManager(EconomyConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public int ComputeInterest(int gold)
        {
            if (_config.interestStep <= 0) return 0;
            int steps = gold / _config.interestStep;
            int raw = steps * _config.interestPerStep;
            if (_config.interestCap >= 0)
            {
                raw = Math.Min(raw, _config.interestCap);
            }
            return Math.Max(0, raw);
        }

        public int ComputeStreakBonus(bool win, int streakCount)
        {
            var tiers = win ? _config.winStreakTiers : _config.lossStreakTiers;
            int best = 0;
            for (int i = 0; i < tiers.Length; i++)
            {
                if (streakCount >= tiers[i].threshold)
                {
                    best = Math.Max(best, tiers[i].bonus);
                }
            }
            return best;
        }

        // Applies end-of-round economy with this order:
        // 1) Base income
        // 2) PvP win bonus
        // 3) Update streak (after PvP outcome)
        // 4) Streak bonus (based on updated streak)
        // 5) Interest (on pre-payout or post-payout per config)
        public int ApplyEndOfRound(EconomyState state, RoundOutcome outcome)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            int payout = 0;

            // 1) Base income
            payout += _config.baseIncome;

            // 2) PvP win bonus
            if (outcome == RoundOutcome.Win)
                payout += _config.pvpWinBonus;

            // 3) Update streak after PvP
            // 4) Streak bonus using updated streak values
            // First update the streaks, then compute streak bonus accordingly
            state.ApplyOutcomeUpdateStreak(outcome);
            if (outcome == RoundOutcome.Win)
                payout += ComputeStreakBonus(true, state.WinStreak);
            else if (outcome == RoundOutcome.Loss)
                payout += ComputeStreakBonus(false, state.LossStreak);

            // 5) Interest
            if (_config.interestOnPrePayoutGold)
            {
                payout += ComputeInterest(state.Gold);
            }
            else
            {
                payout += ComputeInterest(state.Gold + payout);
            }

            state.AddGold(payout);
            return payout;
        }
    }
}

