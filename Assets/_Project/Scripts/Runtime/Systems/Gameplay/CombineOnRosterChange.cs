using System.Collections.Generic;
using System.Linq;
using TestTFT.Scripts.Runtime.Systems.Traits;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.Gameplay
{
    // Attach to bench root; merges 3-of-a-kind into +1 star
    public sealed class CombineOnRosterChange : MonoBehaviour
    {
        private void OnEnable()
        {
            RosterEvents.OnRosterChanged += TryCombine;
        }

        private void OnDisable()
        {
            RosterEvents.OnRosterChanged -= TryCombine;
        }

        private void TryCombine()
        {
            // Group by (name, star)
            var allUnits = GetComponentsInChildren<UnitData>(includeInactive: false);
            var groups = allUnits
                .Where(u => !string.IsNullOrEmpty(u.UnitName))
                .GroupBy(u => (u.UnitName, u.Star));

            foreach (var g in groups)
            {
                int count = g.Count();
                if (count >= 3)
                {
                    // Take any three
                    var three = g.Take(3).ToList();
                    // Upgrade one, remove two
                    var keeper = three[0];
                    keeper.Star += 1;

                    // Visual feedback: tint stronger per star
                    var img = keeper.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                    {
                        img.color *= 1.15f;
                    }

                    for (int i = 1; i < three.Count; i++)
                    {
                        Destroy(three[i].gameObject);
                    }

                    // Fire another pass in case cascades are possible
                    RosterEvents.Raise();
                    break;
                }
            }
        }
    }
}

