using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.UI
{
    // Minimal, IMGUI-based HUD + Shop UI for MVP loop
    public class UIHudShop : MonoBehaviour
    {
        [Serializable]
        private class UnitDef
        {
            public string Id;
            public string Name;
            public int Cost;
        }

        [Serializable]
        private class UnitStack
        {
            public UnitDef Def;
            public int Count; // stacks for star-upgrade simulation (3 combines)
        }

        private class PlayerState
        {
            public int Gold = 10;
            public int Level = 1;
            public int Xp = 0;
            public int XpToLevel = 6; // simple curve placeholder
            public int WinStreak = 0;
            public int LossStreak = 0;
        }

        // State
        private PlayerState _player = new PlayerState();
        private System.Random _rng = new System.Random(1337);
        private List<UnitDef> _pool = new List<UnitDef>();
        private UnitDef[] _shop = new UnitDef[5];
        private bool _locked = false;

        private UnitStack[] _bench = new UnitStack[8];
        private int? _selectedBenchIndex = null;
        private int? _dragIndex = null;
        private Vector2 _dragOffset;
        private float _shopTimer = 20f;

        private GUIStyle _labelCenter;
        private GUIStyle _header;
        private GUIStyle _button;
        private GUIStyle _tooltip;

        private void Awake()
        {
            InitStyles();
            InitContent();
            RerollShop(force:true);
        }

        private void InitStyles()
        {
            _labelCenter = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            _header = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            _button = new GUIStyle(GUI.skin.button) { fontSize = 14 };
            _tooltip = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, wordWrap = true };
        }

        private void InitContent()
        {
            // Dummy pool of units (cost 1..5)
            for (int i = 0; i < 10; i++)
            {
                _pool.Add(new UnitDef
                {
                    Id = $"u{i}",
                    Name = i % 2 == 0 ? $"Slinger {i}" : $"Guardian {i}",
                    Cost = 1 + (i % 5)
                });
            }
        }

        private void Update()
        {
            // Timer countdown
            _shopTimer -= Time.deltaTime;
            if (_shopTimer <= 0f)
            {
                // Simulate round end: simple win/loss alternation to show streak & income
                bool win = (Mathf.FloorToInt(Time.timeSinceLevelLoad / 20f) % 2) == 0;
                EndOfRound(win);
                _shopTimer = 20f;
                if (!_locked)
                    RerollShop(force:true);
            }

            // Hotkeys: buy slots 1..5
            for (int i = 0; i < 5; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryBuy(i);
                }
            }

            // Delete sells selected bench slot
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_selectedBenchIndex.HasValue)
                {
                    SellBench(_selectedBenchIndex.Value);
                }
            }
        }

        private void EndOfRound(bool win)
        {
            // Base income 5g (placeholder), interest +1 per 10 up to +5
            int payout = 5;
            if (win)
            {
                _player.WinStreak += 1;
                _player.LossStreak = 0;
                // simple streak bonus: +1 per 2 wins
                payout += (_player.WinStreak >= 2) ? 1 + (_player.WinStreak - 2) / 2 : 0;
            }
            else
            {
                _player.LossStreak += 1;
                _player.WinStreak = 0;
                payout += (_player.LossStreak >= 2) ? 1 + (_player.LossStreak - 2) / 2 : 0;
            }
            payout += Mathf.Clamp(_player.Gold / 10, 0, 5);
            _player.Gold += payout;
        }

        private void RerollShop(bool force = false)
        {
            if (!force && _locked) return;
            for (int i = 0; i < 5; i++)
            {
                _shop[i] = _pool[_rng.Next(_pool.Count)];
            }
        }

        private void TryBuy(int index)
        {
            if (index < 0 || index >= _shop.Length) return;
            var def = _shop[index];
            if (def == null) return;
            if (_player.Gold < def.Cost) return;

            int freeSlot = FindFreeBenchSlot();
            if (freeSlot < 0) return; // no space

            _player.Gold -= def.Cost;
            PlaceOnBench(freeSlot, def);

            if (!_locked)
            {
                // Replace bought slot with a new roll (keeps shop at 5)
                _shop[index] = _pool[_rng.Next(_pool.Count)];
            }
        }

        private int FindFreeBenchSlot()
        {
            for (int i = 0; i < _bench.Length; i++)
            {
                if (_bench[i] == null || _bench[i].Def == null)
                    return i;
            }
            return -1;
        }

        private void PlaceOnBench(int slot, UnitDef def)
        {
            if (_bench[slot] == null)
                _bench[slot] = new UnitStack();

            if (_bench[slot].Def == null)
            {
                _bench[slot].Def = def;
                _bench[slot].Count = 1;
            }
            else if (_bench[slot].Def.Id == def.Id)
            {
                _bench[slot].Count += 1;
                TryCombine(slot);
            }
            else
            {
                // overwrite (simple behavior to keep MVP flow deterministic)
                _bench[slot].Def = def;
                _bench[slot].Count = 1;
            }
        }

        private void TryCombine(int slot)
        {
            var stack = _bench[slot];
            if (stack == null || stack.Def == null) return;
            while (stack.Count >= 3)
            {
                stack.Count -= 2; // consume 2 to rank up 1 (3 of a kind -> +1 star)
                // For MVP we don’t track stars visually; count represents copies
            }
        }

        private void SellBench(int slot)
        {
            var stack = _bench[slot];
            if (stack == null || stack.Def == null) return;
            // Refund half cost per copy (rounded down)
            int refund = (stack.Def.Cost * stack.Count) / 2;
            _player.Gold += refund;
            _bench[slot] = null;
            if (_selectedBenchIndex == slot) _selectedBenchIndex = null;
        }

        private void BuyXp()
        {
            const int xpCost = 4;
            if (_player.Gold < xpCost) return;
            _player.Gold -= xpCost;
            _player.Xp += 4;
            if (_player.Xp >= _player.XpToLevel)
            {
                _player.Level += 1;
                _player.Xp -= _player.XpToLevel;
                _player.XpToLevel += 2; // simple curve
            }
        }

        private void OnGUI()
        {
            const float pad = 8f;
            float w = Screen.width;
            float h = Screen.height;

            // HUD bar
            GUILayout.BeginArea(new Rect(pad, pad, w - pad * 2, 120));
            GUILayout.Label("HUD", _header);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Gold: {_player.Gold}", GUILayout.Width(150));
            GUILayout.Label($"Streak W/L: {_player.WinStreak}/{_player.LossStreak}", GUILayout.Width(200));
            GUILayout.Label($"Lvl {_player.Level}  XP {_player.Xp}/{_player.XpToLevel}", GUILayout.Width(220));
            if (GUILayout.Button("Buy XP (4)", _button, GUILayout.Width(120))) BuyXp();
            if (GUILayout.Button("Reroll (2)", _button, GUILayout.Width(120)))
            {
                if (_player.Gold >= 2)
                {
                    _player.Gold -= 2;
                    RerollShop();
                }
            }
            if (GUILayout.Button(_locked ? "Unlock" : "Lock", _button, GUILayout.Width(120))) _locked = !_locked;
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Shop: {_shopTimer:0}s", GUILayout.Width(120));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Shop row (5 cards)
            float cardW = 160f;
            float cardH = 80f;
            float shopY = 140f;
            Rect tooltipRect = default;
            string tooltipText = null;

            for (int i = 0; i < 5; i++)
            {
                float x = pad + i * (cardW + pad);
                var rect = new Rect(x, shopY, cardW, cardH);
                GUI.Box(rect, "");
                var def = _shop[i];
                if (def != null)
                {
                    GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 20), def.Name, _labelCenter);
                    GUI.Label(new Rect(rect.x, rect.y + 28, rect.width, 20), $"Cost: {def.Cost}", _labelCenter);
                    if (GUI.Button(new Rect(rect.x + 10, rect.y + 50, rect.width - 20, 24), $"Buy [{i + 1}]"))
                    {
                        TryBuy(i);
                    }

                    if (rect.Contains(Event.current.mousePosition))
                    {
                        tooltipText = $"{def.Name}\nCost {def.Cost}";
                        tooltipRect = new Rect(rect.x, rect.y + rect.height + 4, rect.width, 40);
                    }
                }
            }

            if (!string.IsNullOrEmpty(tooltipText))
            {
                GUI.Box(tooltipRect, tooltipText, _tooltip);
            }

            // Bench (8 slots) with drag/drop and selection
            float benchY = h - 120f;
            float slotW = (w - pad * 2 - 7 * pad) / 8f; // 8 slots with spacing
            float slotH = 90f;
            int hot = GUIUtility.hotControl;

            for (int i = 0; i < 8; i++)
            {
                float x = pad + i * (slotW + pad);
                var rect = new Rect(x, benchY, slotW, slotH);

                var bgColor = GUI.color;
                if (_selectedBenchIndex == i) GUI.color = Color.yellow;
                GUI.Box(rect, "");
                GUI.color = bgColor;

                var stack = _bench[i];
                if (stack != null && stack.Def != null)
                {
                    GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 20), stack.Def.Name, _labelCenter);
                    GUI.Label(new Rect(rect.x, rect.y + 28, rect.width, 20), $"x{stack.Count} (c{stack.Def.Cost})", _labelCenter);
                }

                // Mouse interactions
                var e = Event.current;
                if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                {
                    _selectedBenchIndex = i;
                    if (stack != null && stack.Def != null)
                    {
                        _dragIndex = i;
                        _dragOffset = e.mousePosition - new Vector2(rect.x, rect.y);
                        e.Use();
                    }
                }
                else if (e.type == EventType.MouseUp && _dragIndex.HasValue)
                {
                    // Drop on nearest slot
                    int target = NearestBenchIndex(e.mousePosition, pad, benchY, slotW, pad);
                    if (target >= 0 && target < 8 && target != _dragIndex.Value)
                    {
                        SwapBench(_dragIndex.Value, target);
                    }
                    _dragIndex = null;
                    e.Use();
                }
            }

            // Draw dragged element preview
            if (_dragIndex.HasValue)
            {
                var stack = _bench[_dragIndex.Value];
                if (stack != null && stack.Def != null)
                {
                    var pos = Event.current.mousePosition - _dragOffset;
                    var rect = new Rect(pos.x, pos.y, slotW, slotH);
                    GUI.color = new Color(1f, 1f, 1f, 0.8f);
                    GUI.Box(rect, "");
                    GUI.color = Color.white;
                    GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 20), stack.Def.Name, _labelCenter);
                    GUI.Label(new Rect(rect.x, rect.y + 28, rect.width, 20), $"x{stack.Count} (c{stack.Def.Cost})", _labelCenter);
                }
            }
        }

        private int NearestBenchIndex(Vector2 mouse, float pad, float benchY, float slotW, float spacing)
        {
            float bestDist = float.MaxValue;
            int best = -1;
            for (int i = 0; i < 8; i++)
            {
                float x = pad + i * (slotW + spacing);
                var center = new Vector2(x + slotW * 0.5f, benchY + 45f);
                float d = Vector2.SqrMagnitude(mouse - center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }
            return best;
        }

        private void SwapBench(int a, int b)
        {
            var tmp = _bench[a];
            _bench[a] = _bench[b];
            _bench[b] = tmp;
        }
    }
}

