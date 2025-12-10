using TestTFT.Scripts.Runtime.Systems.Gameplay;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.UI.Common.Selection
{
    public sealed class SellOnDelete : MonoBehaviour
    {
        public int SellGold = 1;
        private EconomySystem _economy;
        private ShopSystem _shop;

        public void Init(EconomySystem eco, ShopSystem shop = null)
        {
            _economy = eco;
            _shop = shop;
        }

        private void Update()
        {
            if (SelectionSystem.Current == gameObject && Input.GetKeyDown(KeyCode.Delete))
            {
                // Determine sell value
                int value = SellGold;
                var data = GetComponent<TestTFT.Scripts.Runtime.Systems.Gameplay.UnitData>();
                if (data != null && data.Cost > 0)
                {
                    // For MVP: sell for base cost, ignore star multiplier
                    value = data.Cost;
                }
                if (_economy != null) _economy.AddGold(value);

                // Return unit to pool if available
                if (_shop != null && data != null && !string.IsNullOrEmpty(data.UnitName))
                {
                    _shop.ReturnToPool(data.UnitName, data.Cost);
                }

                Destroy(gameObject);
                // Notify roster systems that a unit was removed
                TestTFT.Scripts.Runtime.Systems.Traits.RosterEvents.Raise();
            }
        }
    }
}
