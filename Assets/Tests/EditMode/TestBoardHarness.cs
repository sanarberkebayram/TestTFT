using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;
using TestTFT.Scripts.Runtime.Combat.Unit;
using TestTFT.Scripts.Runtime.Systems.Simulation;

public static class TestBoardHarness
{
    public sealed class Board
    {
        public SimulationSystem sim;
        public List<GameObject> units = new List<GameObject>();
    }

    public static Board SpawnBoard(params Vector3[] positions)
    {
        var b = new Board();
        var simGO = new GameObject("Sim");
        b.sim = simGO.AddComponent<SimulationSystem>();

        foreach (var pos in positions)
        {
            var u = new GameObject("Unit");
            u.transform.position = pos;
            u.AddComponent<HealthComponent>();
            u.AddComponent<ManaComponent>();
            u.AddComponent<TestTFT.Scripts.Runtime.Combat.Effects.EffectsController>();
            u.AddComponent<TestTFT.Scripts.Runtime.Combat.Ability.AbilityExecutor>();
            u.AddComponent<UnitBehaviour>();
            b.units.Add(u);
        }

        return b;
    }

    public static void RunTicks(Board b, int steps)
    {
        for (int i = 0; i < steps; i++) b.sim.TickOnce();
    }

    public static void Destroy(Board b)
    {
        foreach (var u in b.units) Object.DestroyImmediate(u);
        if (b.sim != null) Object.DestroyImmediate(b.sim.gameObject);
        SimulationSystem.ClearUnits();
    }
}

public class BoardHarnessSmokeTest
{
    [Test]
    public void TwoUnits_MoveAndFight()
    {
        var board = TestBoardHarness.SpawnBoard(new Vector3(-2, 0, 0), new Vector3(2, 0, 0));
        // Run for a few seconds (10Hz)
        TestBoardHarness.RunTicks(board, 100);

        var a = board.units[0].GetComponent<HealthComponent>().CurrentHealth;
        var b = board.units[1].GetComponent<HealthComponent>().CurrentHealth;

        // Expect some damage exchanged but not necessarily dead
        Assert.Less(a, 100f);
        Assert.Less(b, 100f);

        TestBoardHarness.Destroy(board);
    }
}

