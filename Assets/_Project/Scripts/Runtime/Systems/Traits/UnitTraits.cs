using System.Collections.Generic;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Traits
{
    // Attach to a Unit to define its native traits and emblem-granted traits.
    public sealed class UnitTraits : MonoBehaviour
    {
        [Tooltip("Native traits on this unit (from its definition)")]
        public List<string> baseTraits = new List<string>();

        [Tooltip("Emblem-granted traits (each counts only if not already native)")]
        public List<string> emblems = new List<string>();

        // Returns a set of unique traits contributed by this unit (emblems do not double-stack native)
        public HashSet<string> GetContributedTraits()
        {
            var set = new HashSet<string>(baseTraits);
            foreach (var e in emblems)
            {
                if (!string.IsNullOrEmpty(e)) set.Add(e);
            }
            return set;
        }
    }
}

