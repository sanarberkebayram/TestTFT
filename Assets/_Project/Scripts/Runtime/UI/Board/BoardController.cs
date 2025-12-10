using System.Collections.Generic;
using System.Linq;
using TestTFT.Scripts.Runtime.Systems.Gameplay;
using TestTFT.Scripts.Runtime.UI.Common.DragDrop;
using UnityEngine;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.UI.Board
{
    // Simple 2-row board with auto-arrange utility for bots.
    public sealed class BoardController : MonoBehaviour
    {
        [SerializeField] private int columns = 7;
        [SerializeField] private Transform frontRow; // closer to enemy
        [SerializeField] private Transform backRow;

        public void Init(int cols)
        {
            columns = Mathf.Max(3, cols);
            BuildIfNeeded();
        }

        private void BuildIfNeeded()
        {
            if (frontRow != null && backRow != null) return;
            var grid = gameObject.AddComponent<GridLayoutGroup>();
            grid.enabled = false; // use nested rows for layout

            frontRow = new GameObject("FrontRow").transform;
            backRow = new GameObject("BackRow").transform;
            frontRow.SetParent(transform, false);
            backRow.SetParent(transform, false);

            var fr = frontRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            fr.spacing = 6f;
            fr.childControlWidth = fr.childControlHeight = true;
            var br = backRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            br.spacing = 6f;
            br.childControlWidth = br.childControlHeight = true;

            for (int i = 0; i < columns; i++)
            {
                CreateSlot(frontRow, i);
                CreateSlot(backRow, i);
            }
        }

        private static void CreateSlot(Transform row, int i)
        {
            var slot = new GameObject($"Slot_{row.name}_{i}");
            slot.transform.SetParent(row, false);
            var img = slot.AddComponent<Image>();
            img.color = new Color(0.08f, 0.08f, 0.08f, 0.5f);
            slot.AddComponent<DropSlot>();
            var rt = slot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 90);
        }

        public void ClearBoard()
        {
            foreach (Transform row in new[] { frontRow, backRow })
            {
                if (row == null) continue;
                foreach (Transform child in row)
                {
                    var unit = child.GetComponentInChildren<Draggable>();
                    if (unit != null)
                    {
                        unit.transform.SetParent(null, false);
                    }
                }
            }
        }

        public void AutoArrange(IEnumerable<GameObject> units, int allowedSlots)
        {
            BuildIfNeeded();
            if (frontRow == null || backRow == null) return;
            var list = units
                .Select(u => new
                {
                    go = u,
                    data = u.GetComponent<UnitData>(),
                    name = u.GetComponent<UnitData>()?.UnitName ?? "",
                    star = u.GetComponent<UnitData>()?.Star ?? 1,
                    cost = u.GetComponent<UnitData>()?.Cost ?? 1,
                    role = UnitCatalog.GetRole(u.GetComponent<UnitData>()?.UnitName ?? "")
                })
                .OrderByDescending(x => x.star) // upgrades first
                .ThenByDescending(x => x.cost)
                .ToList();

            int capacity = Mathf.Clamp(allowedSlots, 0, columns * 2);
            var chosen = list.Take(capacity).ToList();

            // Tanks front, carries back, supports fill balance; spread across columns
            var tanks = chosen.Where(x => x.role == UnitCatalog.Role.Tank).ToList();
            var carries = chosen.Where(x => x.role == UnitCatalog.Role.Carry).ToList();
            var supports = chosen.Where(x => x.role == UnitCatalog.Role.Support).ToList();

            int fIdx = 0, bIdx = 0;
            void Place(Transform row, ref int idx, GameObject go)
            {
                int col = idx % columns;
                idx++;
                var slot = row.GetChild(col);
                go.transform.SetParent(slot, false);
                var rt = go.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = Vector2.zero;
            }

            foreach (var t in tanks) Place(frontRow, ref fIdx, t.go);
            foreach (var c in carries) Place(backRow, ref bIdx, c.go);
            foreach (var s in supports)
            {
                // fill remaining alternating rows
                if (fIdx <= bIdx) Place(frontRow, ref fIdx, s.go); else Place(backRow, ref bIdx, s.go);
            }
        }
    }
}

