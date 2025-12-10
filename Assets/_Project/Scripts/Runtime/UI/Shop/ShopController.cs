using System;
using TestTFT.Scripts.Runtime.Systems.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.UI.Shop
{
    public sealed class ShopController : MonoBehaviour
    {
        [Serializable]
        public sealed class Slot
        {
            public Text nameText;
            public Text costText;
            public Button buyButton;
        }

        public Slot[] slots = new Slot[5];

        private EconomySystem _economy;
        private ShopSystem _shop;
        private TestTFT.Scripts.Runtime.UI.Bench.BenchController _bench;

        public void Init(EconomySystem economy, ShopSystem shop, TestTFT.Scripts.Runtime.UI.Bench.BenchController bench)
        {
            _economy = economy;
            _shop = shop;
            _bench = bench;
            _shop.OnChanged += Refresh;

            for (int i = 0; i < slots.Length; i++)
            {
                int idx = i;
                if (slots[i]?.buyButton != null)
                    slots[i].buyButton.onClick.AddListener(() => TryBuy(idx));
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (_shop != null) _shop.OnChanged -= Refresh;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) TryBuy(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryBuy(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryBuy(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) TryBuy(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) TryBuy(4);
        }

        private void TryBuy(int index)
        {
            if (index < 0 || index >= 5) return;
            var offer = _shop.Get(index);
            if (_bench != null && !_bench.HasFreeSlot()) return;
            if (_economy.TrySpend(offer.Cost))
            {
                // Spawn onto bench and grant small XP
                _bench?.AddUnit(offer.Name, offer.Cost);
                _economy.AddXp(1);
                SetEmpty(index);
            }
        }

        private void SetEmpty(int index)
        {
            if (index < 0 || index >= slots.Length) return;
            var s = slots[index];
            if (s?.nameText) s.nameText.text = "—";
            if (s?.costText) s.costText.text = "";
        }

        private void Refresh()
        {
            for (int i = 0; i < 5 && i < slots.Length; i++)
            {
                var offer = _shop.Get(i);
                var s = slots[i];
                if (s?.nameText) s.nameText.text = offer.Name;
                if (s?.costText) s.costText.text = $"{offer.Cost}g";
            }
        }
    }
}
