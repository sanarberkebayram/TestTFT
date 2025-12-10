using NUnit.Framework;
using UnityEngine;
using TestTFT.Scripts.Runtime.Combat;
using TestTFT.Scripts.Runtime.Combat.Ability;

public class CombatDamageTests
{
    private sealed class OnHitCapture : MonoBehaviour, IOnHitHook
    {
        public OnHitInfo? Last;
        public void OnHit(OnHitInfo info) { Last = info; }
    }

    private (GameObject caster, AbilityExecutor exec, GameObject target, HealthComponent targetHealth, CombatStats targetStats, OnHitCapture hook) MakeRig()
    {
        // Reset overtime unless a test overrides
        CombatTime.ResetForTests(0f);
        CombatTime.ForceOvertime(false);

        var caster = new GameObject("Caster");
        caster.AddComponent<ManaComponent>();
        var exec = caster.AddComponent<AbilityExecutor>();
        var hook = caster.AddComponent<OnHitCapture>();

        var target = new GameObject("Target");
        var health = target.AddComponent<HealthComponent>();
        var stats = target.AddComponent<CombatStats>();

        return (caster, exec, target, health, stats, hook);
    }

    private SpellDefinition MakeSpell(DamageType type, float baseDamage, float critChance = 0f, float critMult = 2f)
    {
        var def = ScriptableObject.CreateInstance<SpellDefinition>();
        def.damageType = type;
        def.baseDamage = baseDamage;
        def.critChance = critChance;
        def.critMultiplier = critMult;
        def.manaCost = 0f;
        return def;
    }

    [Test]
    public void PhysicalDamage_WithArmor100_Halved()
    {
        var rig = MakeRig();
        rig.targetStats.armor = 100f;
        var spell = MakeSpell(DamageType.Physical, 100f);

        var ok = rig.exec.TryCast(spell, rig.target);
        Assert.IsTrue(ok);
        Assert.AreEqual(50f, rig.targetHealth.CurrentHealth, 0.01f, "Health should drop by 50 from 100 to 50");
    }

    [Test]
    public void MagicDamage_WithMR50_ReducedCorrectly()
    {
        var rig = MakeRig();
        rig.targetStats.magicResist = 50f;
        var spell = MakeSpell(DamageType.Magic, 100f);

        rig.exec.TryCast(spell, rig.target);
        // Multiplier = 100 / (100+50) = 0.6666667 => 66.6667 damage
        Assert.AreEqual(33.3333f, rig.targetHealth.CurrentHealth, 0.01f);
    }

    [Test]
    public void TrueDamage_IgnoresMitigation()
    {
        var rig = MakeRig();
        rig.targetStats.armor = 999f;
        rig.targetStats.magicResist = 999f;
        var spell = MakeSpell(DamageType.True, 100f);

        rig.exec.TryCast(spell, rig.target);
        Assert.AreEqual(0f, rig.targetHealth.CurrentHealth, 0.01f);
    }

    [Test]
    public void NegativeArmor_IncreasesDamage()
    {
        var rig = MakeRig();
        rig.targetStats.armor = -50f; // Multiplier = 100/50 = 2x
        // increase max/current health to observe >100 damage
        // HealthComponent default max/current are 100, so 200 would drop to 0; assert dealt equals 100? We'll set baseDamage 50 to see 100.
        var spell = MakeSpell(DamageType.Physical, 50f);

        rig.exec.TryCast(spell, rig.target);
        Assert.AreEqual(0f, rig.targetHealth.CurrentHealth, 0.01f);
    }

    [Test]
    public void Crit_AppliesBeforeMitigation()
    {
        var rig = MakeRig();
        rig.targetStats.armor = 100f; // 0.5 multiplier
        var spell = MakeSpell(DamageType.Physical, 100f, critChance: 1f, critMult: 2f);

        rig.exec.TryCast(spell, rig.target);
        // 100 base * 2 crit * 0.5 armor = 100 final damage
        Assert.AreEqual(0f, rig.targetHealth.CurrentHealth, 0.01f);
        Assert.IsTrue(rig.hook.Last.HasValue && rig.hook.Last.Value.IsCrit);
    }

    [Test]
    public void Dodge_ResolvesBeforeCrit_AndDealsNoDamage()
    {
        var rig = MakeRig();
        rig.targetStats.dodgeChance = 1f; // guaranteed dodge
        var spell = MakeSpell(DamageType.Physical, 100f, critChance: 1f); // would crit, but dodge cancels

        rig.exec.TryCast(spell, rig.target);
        Assert.AreEqual(100f, rig.targetHealth.CurrentHealth, 0.01f);
        Assert.IsTrue(rig.hook.Last.HasValue);
        var info = rig.hook.Last.Value;
        Assert.IsTrue(info.IsDodged);
        Assert.IsFalse(info.IsCrit);
        Assert.AreEqual(0f, info.DamageDealt, 0.001f);
    }

    [Test]
    public void Overtime_After15s_IncreasesDamageBy50Percent()
    {
        var rig = MakeRig();
        CombatTime.ForceOvertime(true);
        var spell = MakeSpell(DamageType.Physical, 100f, critChance: 0f);

        rig.exec.TryCast(spell, rig.target);
        Assert.AreEqual(100f - 150f, rig.targetHealth.CurrentHealth, 0.01f);
    }

    [Test]
    public void ZeroDamage_NoEffect()
    {
        var rig = MakeRig();
        var spell = MakeSpell(DamageType.Magic, 0f);
        rig.exec.TryCast(spell, rig.target);
        Assert.AreEqual(100f, rig.targetHealth.CurrentHealth, 0.01f);
    }
}
