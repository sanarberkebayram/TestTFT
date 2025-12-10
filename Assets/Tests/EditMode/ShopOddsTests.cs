using NUnit.Framework;
using TestTFT.Scripts.Runtime.Systems.Core;
using TestTFT.Scripts.Runtime.Systems.Gameplay;

public class ShopOddsTests
{
    [Test]
    public void Locked_Shop_DoesNotChange_OnReroll()
    {
        DeterministicRng.SetSeed(999UL);
        var shop = new ShopSystem();
        shop.RerollForLevel(3);
        var before = (ShopSystem.Offer[])shop.Current.Clone();

        shop.ToggleLock();
        shop.RerollForLevel(3);

        for (int i = 0; i < before.Length; i++)
        {
            Assert.AreEqual(before[i].Cost, shop.Current[i].Cost);
            Assert.AreEqual(before[i].Name, shop.Current[i].Name);
        }
    }

    [Test]
    public void CostDistribution_MatchesOdds_WithinTolerance_Level5()
    {
        DeterministicRng.SetSeed(12345UL);
        var shop = new ShopSystem();

        int nSamples = 2000; // 2000 * 5 = 10k offers
        int c1 = 0, c2 = 0, c3 = 0;
        for (int i = 0; i < nSamples; i++)
        {
            shop.RerollForLevel(5);
            for (int k = 0; k < 5; k++)
            {
                var cost = shop.Current[k].Cost;
                if (cost == 1) c1++; else if (cost == 2) c2++; else if (cost == 3) c3++;
            }
        }

        float total = c1 + c2 + c3;
        float p1 = c1 / total;
        float p2 = c2 / total;
        float p3 = c3 / total;

        // From odds table at level 5: {0.45, 0.40, 0.15}
        Assert.That(p1, Is.InRange(0.40f, 0.50f));
        Assert.That(p2, Is.InRange(0.35f, 0.45f));
        Assert.That(p3, Is.InRange(0.12f, 0.18f));
    }
}

