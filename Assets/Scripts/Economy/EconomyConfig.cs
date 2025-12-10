using UnityEngine;

namespace Economy
{
    [CreateAssetMenu(menuName = "Game/EconomyConfig", fileName = "EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Base Income")]
        public int baseIncome = 0;
        public bool applyAtEndOfRound = true; // kept for clarity

        [Header("Interest")] 
        [Tooltip("Gold interest per full interestStep, typically +1 per 10g")] 
        public int interestPerStep = 1;
        [Tooltip("Amount of gold per step (e.g., 10 gold per +1 interest)")] 
        public int interestStep = 10;
        [Tooltip("Maximum interest granted per round")] 
        public int interestCap = 5;
        [Tooltip("If true, compute interest from bank before other payouts; otherwise include payouts.")]
        public bool interestOnPrePayoutGold = true;

        [Header("PvP")] 
        [Tooltip("Gold bonus for winning a PvP round")] 
        public int pvpWinBonus = 1;

        [System.Serializable]
        public struct StreakTier
        {
            public int threshold; // e.g., 2, 4, 6
            public int bonus;     // e.g., 1, 2, 3
        }

        [Header("Streak Bonuses (Win)")]
        public StreakTier[] winStreakTiers = new StreakTier[]
        {
            new StreakTier{ threshold = 2, bonus = 1},
            new StreakTier{ threshold = 4, bonus = 2},
            new StreakTier{ threshold = 6, bonus = 3},
        };

        [Header("Streak Bonuses (Loss)")]
        public StreakTier[] lossStreakTiers = new StreakTier[]
        {
            new StreakTier{ threshold = 2, bonus = 1},
            new StreakTier{ threshold = 4, bonus = 2},
            new StreakTier{ threshold = 6, bonus = 3},
        };
    }
}

