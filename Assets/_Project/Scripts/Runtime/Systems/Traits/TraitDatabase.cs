using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Traits
{
    public static class TraitDatabase
    {
        private static Dictionary<string, TraitDef> _byId;

        public static bool IsLoaded => _byId != null;

        public static void LoadFromResources(string path = "traits")
        {
            if (_byId != null) return;
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"TraitDatabase: Resources/{path}.json not found");
                _byId = new Dictionary<string, TraitDef>();
                return;
            }
            var data = JsonUtility.FromJson<TraitDatabaseData>(textAsset.text);
            _byId = data?.traits?.ToDictionary(t => t.id) ?? new Dictionary<string, TraitDef>();
        }

        public static IReadOnlyDictionary<string, TraitDef> All()
        {
            if (_byId == null) LoadFromResources();
            return _byId;
        }

        public static TraitDef Get(string id)
        {
            if (_byId == null) LoadFromResources();
            _byId.TryGetValue(id, out var def);
            return def;
        }
    }
}

