using NUnit.Framework;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;
using TestTFT.Scripts.Runtime.Combat.Unit;
using TestTFT.Scripts.Runtime.Systems.Simulation;

public class TargetingTests
{
    private GameObject SpawnUnit(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.AddComponent<HealthComponent>();
        go.AddComponent<ManaComponent>();
        go.AddComponent<TestTFT.Scripts.Runtime.Combat.Effects.EffectsController>();
        go.AddComponent<TestTFT.Scripts.Runtime.Combat.Ability.AbilityExecutor>();
        go.AddComponent<UnitBehaviour>();
        return go;
    }

    [SetUp]
    public void SetUp()
    {
        SimulationSystem.ClearUnits();
    }

    [TearDown]
    public void TearDown()
    {
        SimulationSystem.ClearUnits();
    }

    [Test]
    public void AutoAcquires_Nearest_Living_Target()
    {
        var simGO = new GameObject("Sim");
        var sim = simGO.AddComponent<SimulationSystem>();

        var seeker = SpawnUnit("Seeker", Vector3.zero);
        var far = SpawnUnit("Far", new Vector3(5, 0, 0));
        var near = SpawnUnit("Near", new Vector3(1, 0, 0));

        // No explicit target; simulate 1 tick
        sim.TickOnce();
        Assert.AreSame(near, seeker.GetComponent<UnitBehaviour>().CurrentTarget);

        Object.DestroyImmediate(seeker);
        Object.DestroyImmediate(far);
        Object.DestroyImmediate(near);
        Object.DestroyImmediate(simGO);
    }

    [Test]
    public void Retargets_When_Target_Dies()
    {
        var simGO = new GameObject("Sim");
        var sim = simGO.AddComponent<SimulationSystem>();

        var seeker = SpawnUnit("Seeker", Vector3.zero);
        var near = SpawnUnit("Near", new Vector3(1, 0, 0));
        var far = SpawnUnit("Far", new Vector3(2, 0, 0));

        sim.TickOnce();
        var ub = seeker.GetComponent<UnitBehaviour>();
        Assert.AreSame(near, ub.CurrentTarget);

        // Kill current target
        near.GetComponent<HealthComponent>().ApplyDamage(1000f, new DamageContext(seeker, near, false));
        sim.TickOnce();
        Assert.AreSame(far, ub.CurrentTarget);

        Object.DestroyImmediate(seeker);
        Object.DestroyImmediate(near);
        Object.DestroyImmediate(far);
        Object.DestroyImmediate(simGO);
    }

    [Test]
    public void TieBreakers_AreStable_ByInstanceId()
    {
        var simGO = new GameObject("Sim");
        var sim = simGO.AddComponent<SimulationSystem>();

        var seeker = SpawnUnit("Seeker", Vector3.zero);
        // Create two equidistant targets; the one created first should have a smaller instance ID typically
        var a = SpawnUnit("A", new Vector3(1, 0, 0));
        var b = SpawnUnit("B", new Vector3(-1, 0, 0));

        sim.TickOnce();
        var ub = seeker.GetComponent<UnitBehaviour>();
        Assert.IsNotNull(ub.CurrentTarget);

        var chosen = ub.CurrentTarget;
        // Run more ticks to ensure stability
        for (int i = 0; i < 5; i++) sim.TickOnce();
        Assert.AreSame(chosen, ub.CurrentTarget);

        Object.DestroyImmediate(seeker);
        Object.DestroyImmediate(a);
        Object.DestroyImmediate(b);
        Object.DestroyImmediate(simGO);
    }
}

