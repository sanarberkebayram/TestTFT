using NUnit.Framework;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;
using TestTFT.Scripts.Runtime.Combat.Unit;
using TestTFT.Scripts.Runtime.Systems.Simulation;

public class SimulationDeterminismTests
{
    private GameObject SpawnUnit(Vector3 pos)
    {
        var go = new GameObject("Unit");
        go.transform.position = pos;
        go.AddComponent<HealthComponent>();
        go.AddComponent<ManaComponent>();
        go.AddComponent<TestTFT.Scripts.Runtime.Combat.Effects.EffectsController>();
        go.AddComponent<TestTFT.Scripts.Runtime.Combat.Ability.AbilityExecutor>();
        go.AddComponent<UnitBehaviour>();
        return go;
    }

    [Test]
    public void Units_Tick_Deterministically()
    {
        // World 1
        SimulationSystem.ClearUnits();
        var simGO1 = new GameObject("Sim1");
        var sim1 = simGO1.AddComponent<SimulationSystem>();

        var u1a = SpawnUnit(new Vector3(-5, 0, 0));
        var u1b = SpawnUnit(new Vector3(5, 0, 0));

        u1a.GetComponent<UnitBehaviour>().SetTarget(u1b);
        u1b.GetComponent<UnitBehaviour>().SetTarget(u1a);

        for (int i = 0; i < 200; i++) // 20 seconds at 10 Hz
        {
            sim1.TickOnce();
        }

        var a_pos_1 = u1a.transform.position;
        var b_pos_1 = u1b.transform.position;
        var a_hp_1 = u1a.GetComponent<HealthComponent>().CurrentHealth;
        var b_hp_1 = u1b.GetComponent<HealthComponent>().CurrentHealth;

        // Teardown world 1
        Object.DestroyImmediate(u1a);
        Object.DestroyImmediate(u1b);
        Object.DestroyImmediate(simGO1);
        SimulationSystem.ClearUnits();

        // World 2 - identical setup
        var simGO2 = new GameObject("Sim2");
        var sim2 = simGO2.AddComponent<SimulationSystem>();

        var u2a = SpawnUnit(new Vector3(-5, 0, 0));
        var u2b = SpawnUnit(new Vector3(5, 0, 0));

        u2a.GetComponent<UnitBehaviour>().SetTarget(u2b);
        u2b.GetComponent<UnitBehaviour>().SetTarget(u2a);

        for (int i = 0; i < 200; i++)
        {
            sim2.TickOnce();
        }

        var a_pos_2 = u2a.transform.position;
        var b_pos_2 = u2b.transform.position;
        var a_hp_2 = u2a.GetComponent<HealthComponent>().CurrentHealth;
        var b_hp_2 = u2b.GetComponent<HealthComponent>().CurrentHealth;

        // Assert deterministic results
        Assert.That(a_pos_2, Is.EqualTo(a_pos_1));
        Assert.That(b_pos_2, Is.EqualTo(b_pos_1));
        Assert.That(a_hp_2, Is.EqualTo(a_hp_1));
        Assert.That(b_hp_2, Is.EqualTo(b_hp_1));

        // Teardown world 2
        Object.DestroyImmediate(u2a);
        Object.DestroyImmediate(u2b);
        Object.DestroyImmediate(simGO2);
        SimulationSystem.ClearUnits();
    }
}

