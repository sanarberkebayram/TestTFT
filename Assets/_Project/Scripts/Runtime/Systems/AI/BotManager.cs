using System.Collections.Generic;
using System.Linq;
using TestTFT.Scripts.Runtime.Systems.Gameplay;
using TestTFT.Scripts.Runtime.Systems.Traits;
using TestTFT.Scripts.Runtime.UI.Bench;
using TestTFT.Scripts.Runtime.UI.Board;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.Systems.AI
{
    // MVP bot that buys/rolls and arranges units using simple heuristics.
    public sealed class BotManager : MonoBehaviour
    {
        private EconomySystem _eco;
        private ShopSystem _shop;
        private GameLoopSystem _loop;
        private BenchController _bench;
        private BoardController _board;
        private RosterTracker _roster;
        private TraitsComputed _lastTraits;

        public void Init(EconomySystem eco, ShopSystem shop, GameLoopSystem loop,
            BenchController bench, BoardController board, RosterTracker roster)
        {
            _eco = eco; _shop = shop; _loop = loop; _bench = bench; _board = board; _roster = roster;
            if (_roster != null)
            {
                var system = new TraitSystem();
                system.OnTraitsUpdated += tc => _lastTraits = tc;
                _roster.Init(system);
            }
            _loop.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (_loop != null) _loop.OnPhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(GameLoopSystem.Phase p)
        {
            if (p != GameLoopSystem.Phase.Shop) return;
            TryShopHeuristics();
            AutoArrangeBoard();
        }

        private void TryShopHeuristics()
        {
            // Stop if no bench
            if (_bench == null || _shop == null || _eco == null) return;

            // Ensure shop offers available
            _shop.RerollForLevel(_eco.Level);

            int SafeRerollCost = 2;
            int guard = 0; // safety to avoid infinite
            while (guard++ < 50)
            {
                // Check target trait condition: >= 2 traits at threshold 2 (count >= 2)
                int activeTraitsAt2 = CountActiveTraitsAtThreshold(2);
                if (activeTraitsAt2 >= 2) break;

                // Compute preferences
                var offers = Enumerable.Range(0, 5).Select(i => (_shop.Get(i), i)).ToList();
                bool bought = TryBuyBestOffer(offers);
                if (bought) continue;

                // If nothing to buy, try reroll if affordable and we still need traits
                if (_eco.Gold >= SafeRerollCost)
                {
                    if (_eco.TrySpend(SafeRerollCost))
                    {
                        _shop.RerollForLevel(_eco.Level);
                        continue;
                    }
                }
                break;
            }
        }

        private int CountActiveTraitsAtThreshold(int threshold)
        {
            if (_lastTraits == null || _lastTraits.states == null) return 0;
            int count = 0;
            foreach (var st in _lastTraits.states)
            {
                if (st == null) continue;
                if (st.count >= threshold) count++;
            }
            return count;
        }

        private bool TryBuyBestOffer(List<(ShopSystem.Offer offer, int idx)> offers)
        {
            if (!_bench.HasFreeSlot()) return false;

            // Snapshot current bench units for duplicate logic
            var allUnits = GetAllUnits();
            var nameCounts = allUnits
                .GroupBy(u => u.GetComponent<UnitData>()?.UnitName ?? "")
                .ToDictionary(g => g.Key, g => g.Count());

            // Score each offer
            (ShopSystem.Offer offer, int idx, float score)? best = null;
            foreach (var o in offers)
            {
                var name = o.offer.Name;
                int cost = o.offer.Cost;
                if (_eco.Gold < cost) continue;

                float score = 0f;
                // Prefer completing 2★ by buying a third copy
                if (nameCounts.TryGetValue(name, out var c))
                {
                    if (c % 3 == 2) score += 100f; // instant 2★
                    else if (c % 3 == 1) score += 20f; // progress toward 2★
                }

                // Prefer offers that contribute to traits below threshold 2
                var traits = UnitCatalog.GetTraits(name);
                foreach (var t in traits)
                {
                    var current = _lastTraits?.states?.FirstOrDefault(s => s.id == t)?.count ?? 0;
                    if (current < 2) score += 15f; // pushes toward level-2 breakpoint
                    else score += 2f; // still useful
                }

                // Small bias for higher cost units (power)
                score += Mathf.Clamp01(cost) * 2f;

                if (best == null || score > best.Value.score)
                {
                    best = (o.offer, o.idx, score);
                }
            }

            if (best == null) return false;
            var pick = best.Value;
            if (_eco.TrySpend(pick.offer.Cost))
            {
                _bench.AddUnit(pick.offer.Name, pick.offer.Cost);
                // Clear purchased slot visual
                // UI ShopController will update on next reroll; here we just blank locally
                return true;
            }
            return false;
        }

        private List<GameObject> GetAllUnits()
        {
            var list = new List<GameObject>();
            if (_bench != null)
            {
                foreach (Transform child in _bench.transform)
                {
                    var slot = child;
                    var unit = slot.GetComponentsInChildren<TestTFT.Scripts.Runtime.UI.Common.DragDrop.Draggable>(false).FirstOrDefault();
                    if (unit != null) list.Add(unit.gameObject);
                }
            }
            return list;
        }

        private void AutoArrangeBoard()
        {
            if (_board == null || _eco == null) return;
            int allowed = GetBoardSlotsForLevel(_eco.Level);
            var units = GetAllUnits();
            _board.AutoArrange(units, allowed);
        }

        private static int GetBoardSlotsForLevel(int level)
        {
            // From meta defaults (L1..L9): 2,3,4,5,6,7,8,8,9
            int[] curve = { 0, 2, 3, 4, 5, 6, 7, 8, 8, 9 };
            int lvl = Mathf.Clamp(level, 1, 9);
            return curve[lvl];
        }
    }
}

