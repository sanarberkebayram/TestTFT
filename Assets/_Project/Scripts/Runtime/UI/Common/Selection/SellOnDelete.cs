using TestTFT.Scripts.Runtime.Systems.Gameplay;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.UI.Common.Selection
{
    public sealed class SellOnDelete : MonoBehaviour
    {
        public int SellGold = 1;
        private EconomySystem _economy;

        public void Init(EconomySystem eco)
        {
            _economy = eco;
        }

        private void Update()
        {
            if (SelectionSystem.Current == gameObject && Input.GetKeyDown(KeyCode.Delete))
            {
                if (_economy != null) _economy.AddGold(SellGold);
                Destroy(gameObject);
                // Notify roster systems that a unit was removed
                TestTFT.Scripts.Runtime.Systems.Traits.RosterEvents.Raise();
            }
        }
    }
}
