using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Traits
{
    // Stateless compute helper + orchestrator for roster -> trait bonuses.
    public sealed class TraitSystem
    {
        public event Action<TraitsComputed> OnTraitsUpdated;

        // Recompute traits given the current roster of UnitTraits.
        public void Recompute(IEnumerable<UnitTraits> roster)
        {
            TraitDatabase.LoadFromResources();

            var counts = new Dictionary<string, int>();
            foreach (var unit in roster)
            {
                if (unit == null) continue;
                var set = unit.GetContributedTraits();
                foreach (var id in set)
                {
                    if (string.IsNullOrEmpty(id)) continue;
                    counts.TryGetValue(id, out var c);
                    counts[id] = c + 1;
                }
            }

            var result = new TraitsComputed();
            foreach (var pair in TraitDatabase.All())
            {
                var def = pair.Value;
                counts.TryGetValue(def.id, out var count);
                var (idx, effects) = ComputeBreakpoint(def, count);
                result.states.Add(new TraitState
                {
                    id = def.id,
                    name = def.name,
                    count = count,
                    achievedIndex = idx,
                    bonuses = effects
                });
            }

            OnTraitsUpdated?.Invoke(result);
        }

        private static (int index, TraitEffect[] effects) ComputeBreakpoint(TraitDef def, int count)
        {
            if (def?.breakpoints == null || def.breakpoints.Length == 0) return (-1, Array.Empty<TraitEffect>());
            int bestIdx = -1;
            TraitEffect[] best = Array.Empty<TraitEffect>();
            for (int i = 0; i < def.breakpoints.Length; i++)
            {
                var bp = def.breakpoints[i];
                if (count >= bp.threshold)
                {
                    if (bestIdx < 0 || bp.threshold >= def.breakpoints[bestIdx].threshold)
                    {
                        bestIdx = i;
                        best = bp.effects ?? Array.Empty<TraitEffect>();
                    }
                }
            }
            return (bestIdx, best);
        }
    }
}

