using System.Linq;
using System.Text;
using TestTFT.Scripts.Runtime.Systems.Traits;
using UnityEngine;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.UI.Traits
{
    // Minimal UI hook that renders current trait counts and bonuses into a Text.
    public sealed class TraitsUIController : MonoBehaviour
    {
        public Text output;

        private readonly TraitSystem _system = new TraitSystem();

        public TraitSystem System => _system;

        public void BindToRoster(RosterTracker tracker)
        {
            _system.OnTraitsUpdated += OnTraitsUpdated;
            tracker.Init(_system);
        }

        private void OnDestroy()
        {
            _system.OnTraitsUpdated -= OnTraitsUpdated;
        }

        private void OnTraitsUpdated(TraitsComputed data)
        {
            if (output == null) return;
            var sb = new StringBuilder();
            // Show only traits with count > 0 for clarity
            var visible = data.states.Where(s => s.count > 0).OrderByDescending(s => s.count);
            foreach (var s in visible)
            {
                sb.Append($"{s.name}: {s.count}");
                if (s.achievedIndex >= 0 && s.bonuses != null && s.bonuses.Length > 0)
                {
                    sb.Append("  –  ");
                    for (int i = 0; i < s.bonuses.Length; i++)
                    {
                        var ef = s.bonuses[i];
                        sb.Append(i == 0 ? "" : ", ");
                        sb.Append($"{ef.stat}+{ef.value}");
                    }
                }
                sb.AppendLine();
            }
            output.text = sb.Length > 0 ? sb.ToString() : "No active traits";
        }
    }
}

