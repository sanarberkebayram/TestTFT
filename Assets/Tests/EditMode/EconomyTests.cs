using NUnit.Framework;
using UnityEngine;
using Economy;

public class EconomyTests
{
    private EconomyConfig MakeConfig(
        int baseIncome = 5,
        int interestPerStep = 1,
        int interestStep = 10,
        int interestCap = 5,
        int pvpWinBonus = 1,
        bool interestOnPrePayoutGold = true)
    {
        var cfg = ScriptableObject.CreateInstance<EconomyConfig>();
        cfg.baseIncome = baseIncome;
        cfg.interestPerStep = interestPerStep;
        cfg.interestStep = interestStep;
        cfg.interestCap = interestCap;
        cfg.pvpWinBonus = pvpWinBonus;
        cfg.interestOnPrePayoutGold = interestOnPrePayoutGold;
        // Defaults for streak tiers are already set in the ScriptableObject class
        return cfg;
    }

    [Test]
    public void Interest_CapsAtMax()
    {
        var cfg = MakeConfig();
        var manager = new EconomyManager(cfg);
        Assert.AreEqual(5, manager.ComputeInterest(50));
        Assert.AreEqual(5, manager.ComputeInterest(99));
        Assert.AreEqual(0, manager.ComputeInterest(9));
    }

    [Test]
    public void EndOfRound_Win_AppliesBase_PvP_StreakAndInterest()
    {
        var cfg = MakeConfig(baseIncome: 5, pvpWinBonus: 1, interestOnPrePayoutGold: true);
        var manager = new EconomyManager(cfg);
        var state = new EconomyState(startingGold: 20);

        // First win: streak becomes 1; streak bonus 0 at threshold <2
        int payout1 = manager.ApplyEndOfRound(state, RoundOutcome.Win);
        // base(5) + pvp(1) + streak(0) + interest(20 -> +2) = 8
        Assert.AreEqual(8, payout1);
        Assert.AreEqual(28, state.Gold);
        Assert.AreEqual(1, state.WinStreak);
        Assert.AreEqual(0, state.LossStreak);

        // Second win: streak becomes 2; streak bonus +1
        int payout2 = manager.ApplyEndOfRound(state, RoundOutcome.Win);
        // base(5) + pvp(1) + streak(1) + interest(28 -> +2) = 9
        Assert.AreEqual(9, payout2);
        Assert.AreEqual(37, state.Gold);
        Assert.AreEqual(2, state.WinStreak);
    }

    [Test]
    public void EndOfRound_Loss_AppliesBase_StreakLossAndInterest()
    {
        var cfg = MakeConfig(baseIncome: 5, pvpWinBonus: 1, interestOnPrePayoutGold: true);
        var manager = new EconomyManager(cfg);
        var state = new EconomyState(startingGold: 19);

        // First loss: loss streak 1; below threshold => 0 bonus
        int payout1 = manager.ApplyEndOfRound(state, RoundOutcome.Loss);
        // base(5) + pvp(0) + streakLoss(0) + interest(19 -> +1) = 6
        Assert.AreEqual(6, payout1);
        Assert.AreEqual(25, state.Gold);
        Assert.AreEqual(0, state.WinStreak);
        Assert.AreEqual(1, state.LossStreak);

        // Second loss: loss streak 2; bonus +1
        int payout2 = manager.ApplyEndOfRound(state, RoundOutcome.Loss);
        // base(5) + pvp(0) + streakLoss(1) + interest(25 -> +2) = 8
        Assert.AreEqual(8, payout2);
        Assert.AreEqual(33, state.Gold);
        Assert.AreEqual(2, state.LossStreak);
    }
}

