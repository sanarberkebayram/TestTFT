using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Traits
{
    // Attach to a root (e.g., Bench) and track all child UnitTraits under it.
    public sealed class RosterTracker : MonoBehaviour
    {
        [Tooltip("Optional additional roots to scan (e.g., Board)")]
        public List<Transform> extraRoots = new List<Transform>();

        private TraitSystem _system;

        public void Init(TraitSystem system)
        {
            _system = system;
            RosterEvents.OnRosterChanged += Recompute;
            Recompute();
        }

        private void OnDestroy()
        {
            RosterEvents.OnRosterChanged -= Recompute;
        }

        private void Recompute()
        {
            if (_system == null) return;
            var units = new List<UnitTraits>();
            Collect(transform, units);
            foreach (var root in extraRoots)
            {
                if (root != null) Collect(root, units);
            }
            _system.Recompute(units);
        }

        private static void Collect(Transform root, List<UnitTraits> buffer)
        {
            if (root == null) return;
            var comps = root.GetComponentsInChildren<UnitTraits>(includeInactive: false);
            buffer.AddRange(comps);
        }
    }
}

