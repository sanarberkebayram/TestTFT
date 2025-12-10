using UnityEngine;
using TestTFT.Scripts.Runtime.Combat.Ability;

namespace TestTFT.Scripts.Runtime.Combat.Demo
{
    public sealed class DemoCaster : MonoBehaviour
    {
        [SerializeField] private AbilityExecutor executor;
        [SerializeField] private SpellDefinition spell;
        [SerializeField] private GameObject target;
        [SerializeField] private KeyCode castKey = KeyCode.Space;

        private void Awake()
        {
            if (!executor) executor = GetComponent<AbilityExecutor>();
        }

        private void Update()
        {
            if (spell == null || target == null || executor == null) return;
            if (Input.GetKeyDown(castKey))
            {
                executor.TryCast(spell, target);
            }
        }

        public void SetSpell(SpellDefinition def)
        {
            spell = def;
        }
    }
}
