using NUnit.Framework;
using TestTFT.Scripts.Runtime.Systems.Core;

public class DeterministicRngTests
{
    [Test]
    public void SameSeed_YieldsSameSequence_PerStream()
    {
        DeterministicRng.SetSeed(42UL);
        var a1 = DeterministicRng.NextInt(DeterministicRng.Stream.Loot, 0, 1000);
        var a2 = DeterministicRng.NextInt(DeterministicRng.Stream.Loot, 0, 1000);
        var af1 = DeterministicRng.NextFloat01(DeterministicRng.Stream.Loot);

        DeterministicRng.SetSeed(42UL);
        var b1 = DeterministicRng.NextInt(DeterministicRng.Stream.Loot, 0, 1000);
        var b2 = DeterministicRng.NextInt(DeterministicRng.Stream.Loot, 0, 1000);
        var bf1 = DeterministicRng.NextFloat01(DeterministicRng.Stream.Loot);

        Assert.AreEqual(a1, b1);
        Assert.AreEqual(a2, b2);
        Assert.AreEqual(af1, bf1);
    }

    [Test]
    public void DifferentStreams_AreIndependent()
    {
        DeterministicRng.SetSeed(123UL);
        // Draw a few from Shop
        var s1 = DeterministicRng.NextInt(DeterministicRng.Stream.Shop, 0, 10000);
        var s2 = DeterministicRng.NextInt(DeterministicRng.Stream.Shop, 0, 10000);
        // Draw from Targeting
        DeterministicRng.SetSeed(123UL);
        var t1 = DeterministicRng.NextInt(DeterministicRng.Stream.Targeting, 0, 10000);
        var t2 = DeterministicRng.NextInt(DeterministicRng.Stream.Targeting, 0, 10000);

        // With same seed, sequences within stream are equal, but across streams differ
        Assert.AreNotEqual(s1, t1);
        Assert.AreNotEqual(s2, t2);
    }

    [Test]
    public void NextInt_RespectsBounds()
    {
        DeterministicRng.SetSeed(777UL);
        for (int i = 0; i < 1000; i++)
        {
            var v = DeterministicRng.NextInt(DeterministicRng.Stream.Loot, 5, 9);
            Assert.GreaterThanOrEqual(v, 5);
            Assert.Less(v, 9);
        }
    }
}

