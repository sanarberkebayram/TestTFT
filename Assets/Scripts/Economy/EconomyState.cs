namespace Economy
{
    public class EconomyState
    {
        public int Gold { get; private set; }
        public int WinStreak { get; private set; }
        public int LossStreak { get; private set; }

        public EconomyState(int startingGold = 0)
        {
            Gold = startingGold;
            WinStreak = 0;
            LossStreak = 0;
        }

        public void SetGold(int amount)
        {
            Gold = amount < 0 ? 0 : amount;
        }

        public void AddGold(int delta)
        {
            int next = Gold + delta;
            Gold = next < 0 ? 0 : next;
        }

        public void ApplyOutcomeUpdateStreak(RoundOutcome outcome)
        {
            switch (outcome)
            {
                case RoundOutcome.Win:
                    WinStreak += 1;
                    LossStreak = 0;
                    break;
                case RoundOutcome.Loss:
                    LossStreak += 1;
                    WinStreak = 0;
                    break;
                default:
                    // Draw or None: reset nothing by default
                    break;
            }
        }
    }
}

