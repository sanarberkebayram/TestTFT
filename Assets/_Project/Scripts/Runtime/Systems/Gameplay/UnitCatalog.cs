using System.Collections.Generic;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    // Minimal unit metadata for heuristics: traits and roles by name.
    public static class UnitCatalog
    {
        public enum Role { Tank, Carry, Support }

        private static readonly Dictionary<string, string[]> TraitsByUnit = new()
        {
            // Tier 1
            {"Swordsman", new[]{"warrior"}},
            {"Archer", new[]{"ranger"}},
            {"Guardian", new[]{"warrior"}},
            {"Mage", new[]{"mystic"}},
            {"Assassin", new[]{"ranger"}},
            // Tier 2
            {"Bruiser", new[]{"warrior"}},
            {"Healer", new[]{"mystic"}},
            {"Knight", new[]{"warrior"}},
            {"Gunner", new[]{"ranger"}},
            {"Invoker", new[]{"mystic"}},
            // Tier 3
            {"Sentinel", new[]{"warrior"}},
            {"Shaman", new[]{"mystic"}},
            {"Berserker", new[]{"warrior"}},
            {"Oracle", new[]{"mystic"}},
            {"Vanguard", new[]{"warrior"}},
        };

        private static readonly Dictionary<string, Role> RoleByUnit = new()
        {
            {"Swordsman", Role.Tank},
            {"Guardian", Role.Tank},
            {"Knight", Role.Tank},
            {"Bruiser", Role.Tank},
            {"Vanguard", Role.Tank},
            {"Berserker", Role.Tank},

            {"Archer", Role.Carry},
            {"Gunner", Role.Carry},
            {"Mage", Role.Carry},
            {"Invoker", Role.Carry},
            {"Assassin", Role.Carry},

            {"Healer", Role.Support},
            {"Shaman", Role.Support},
            {"Oracle", Role.Support},
            {"Sentinel", Role.Support},
        };

        public static IReadOnlyList<string> GetTraits(string unitName)
        {
            return TraitsByUnit.TryGetValue(unitName, out var arr) ? arr : System.Array.Empty<string>();
        }

        public static Role GetRole(string unitName)
        {
            return RoleByUnit.TryGetValue(unitName, out var role) ? role : Role.Support;
        }
    }
}

