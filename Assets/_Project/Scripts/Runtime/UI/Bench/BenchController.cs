using System.Linq;
using TestTFT.Scripts.Runtime.Systems.Gameplay;
using TestTFT.Scripts.Runtime.Systems.Traits;
using TestTFT.Scripts.Runtime.UI.Common.DragDrop;
using TestTFT.Scripts.Runtime.UI.Common.Selection;
using TestTFT.Scripts.Runtime.UI.Common.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.UI.Bench
{
    // Controls spawning units into first free bench slot.
    public sealed class BenchController : MonoBehaviour
    {
        private EconomySystem _economy;
        private ShopSystem _shop;

        public void Init(EconomySystem economy, ShopSystem shop)
        {
            _economy = economy;
            _shop = shop;
        }

        public bool HasFreeSlot()
        {
            return GetFirstFreeSlot() != null;
        }

        public GameObject AddUnit(string unitName, int cost)
        {
            var slot = GetFirstFreeSlot();
            if (slot == null) return null;

            var unit = new GameObject("Unit");
            unit.transform.SetParent(slot.transform, false);

            var img = unit.AddComponent<Image>();
            img.color = new Color(0.5f, 0.7f, 1f, 0.9f);
            unit.AddComponent<Draggable>();
            var tip = unit.AddComponent<TooltipTarget>();
            tip.Message = $"{unitName} (Cost {cost})\nDrag to move. Delete to sell.";
            unit.AddComponent<SelectOnClick>();

            var sell = unit.AddComponent<SellOnDelete>();
            sell.Init(_economy, _shop);

            // Attach identity
            var data = unit.AddComponent<UnitData>();
            data.UnitName = unitName;
            data.Cost = cost;

            // Optional: traits could be assigned by name in future
            var traits = unit.AddComponent<UnitTraits>();
            // Assign base traits from catalog so TraitSystem can compute synergies
            var traitList = TestTFT.Scripts.Runtime.Systems.Gameplay.UnitCatalog.GetTraits(unitName);
            foreach (var t in traitList)
            {
                if (!string.IsNullOrEmpty(t)) traits.baseTraits.Add(t);
            }

            // Notify systems that roster changed
            RosterEvents.Raise();
            return unit;
        }

        private GameObject GetFirstFreeSlot()
        {
            // A slot is considered free if it has no child with Draggable (i.e., no Unit)
            foreach (Transform child in transform)
            {
                if (child.GetComponent<DropSlot>() == null) continue; // ensure it's a bench slot
                var hasUnit = child.GetComponentsInChildren<Draggable>(includeInactive: false).Any();
                if (!hasUnit) return child.gameObject;
            }
            return null;
        }
    }
}
