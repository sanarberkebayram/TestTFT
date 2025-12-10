using System;
using System.Collections.Generic;
using System.Linq;
using TestTFT.Scripts.Runtime.Systems.Gameplay;
using UnityEngine;

namespace TestTFT.Scripts.Runtime.AI.Bot
{
    // Minimal heuristic bot to satisfy MVP:
    // - During Shop: roll/buy to complete at least two 2★ units; prefer board upgrades
    // - During Carousel: pick highest-cost offer
    // Acceptance: Bot fields legal units (bench slots) and progresses rounds
    public sealed class BotController : MonoBehaviour
    {
        private EconomySystem _economy;
        private ShopSystem _shop;
        private GameLoopSystem _loop;
        private TestTFT.Scripts.Runtime.UI.Bench.BenchController _bench;
        private CarouselSystem _carousel;
        private TestTFT.Scripts.Runtime.UI.Shop.ShopController _shopUI;

        private float _decisionCooldown = 0.25f; // act ~4 times/sec during shop
        private float _decTimer;

        private int _rerollsThisShop;
        private const int MaxRerollsPerShop = 6;

        public void Init(EconomySystem economy, ShopSystem shop, GameLoopSystem loop,
            TestTFT.Scripts.Runtime.UI.Bench.BenchController bench,
            CarouselSystem carousel,
            TestTFT.Scripts.Runtime.UI.Shop.ShopController shopUI)
        {
            _economy = economy;
            _shop = shop;
            _loop = loop;
            _bench = bench;
            _carousel = carousel;
            _shopUI = shopUI;

            _loop.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (_loop != null) _loop.OnPhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(GameLoopSystem.Phase p)
        {
            if (p == GameLoopSystem.Phase.Shop)
            {
                _rerollsThisShop = 0;
            }
        }

        private void Update()
        {
            if (_loop == null) return;
            _decTimer -= Time.deltaTime;
            if (_decTimer > 0f) return;
            _decTimer = _decisionCooldown;

            switch (_loop.CurrentPhase)
            {
                case GameLoopSystem.Phase.Shop:
                    ThinkShop();
                    break;
                case GameLoopSystem.Phase.Carousel:
                    ThinkCarousel();
                    break;
            }
        }

        private void ThinkShop()
        {
            if (_bench == null || _shop == null || _economy == null || _shopUI == null) return;

            // Goal: get to at least 2 units at 2★
            int twoStars = CountTwoStarsOnBench();

            // If bench is full or no gold, do nothing
            if (!_bench.HasFreeSlot() || _economy.Gold <= 0) return;

            // 1) If shop has a copy that completes a 2★ (we have 2 copies of the same 1★), buy that first
            int idx = FindCompletingCopyIndex();
            if (idx >= 0)
            {
                _shopUI.TryBuySlot(idx);
                return;
            }

            // 2) If we are below target twoStars, try to buy copies towards upgrading existing units
            idx = FindProgressCopyIndex();
            if (idx >= 0)
            {
                _shopUI.TryBuySlot(idx);
                return;
            }

            // 3) Otherwise, buy the highest cost affordable unit (board power)
            idx = FindHighestCostAffordableIndex();
            if (idx >= 0)
            {
                _shopUI.TryBuySlot(idx);
                return;
            }

            // 4) If nothing to buy and we still need upgrades, reroll within budget
            if (twoStars < 2 && _economy.Gold >= 2 && _rerollsThisShop < MaxRerollsPerShop)
            {
                _economy.TrySpend(2);
                _shop.RerollForLevel(_economy.Level);
                _rerollsThisShop++;
            }
        }

        private void ThinkCarousel()
        {
            if (_carousel == null) return;
            // Pick the highest cost offer immediately
            int bestIdx = -1;
            int bestCost = -1;
            for (int i = 0; i < _carousel.Current.Length; i++)
            {
                var offer = _carousel.Get(i);
                if (offer.Cost > bestCost)
                {
                    bestCost = offer.Cost;
                    bestIdx = i;
                }
            }
            if (bestIdx >= 0)
            {
                _carousel.TryPick(bestIdx);
            }
        }

        private int CountTwoStarsOnBench()
        {
            int count = 0;
            var units = _bench.GetComponentsInChildren<UnitData>(includeInactive: false);
            foreach (var u in units)
            {
                if (u != null && u.Star >= 2) count++;
            }
            return count;
        }

        private Dictionary<string, int> CountCopiesByName()
        {
            var map = new Dictionary<string, int>();
            var units = _bench.GetComponentsInChildren<UnitData>(includeInactive: false);
            foreach (var u in units)
            {
                if (u == null || string.IsNullOrEmpty(u.UnitName) || u.Star != 1) continue;
                map.TryGetValue(u.UnitName, out var c);
                map[u.UnitName] = c + 1;
            }
            return map;
        }

        private int FindCompletingCopyIndex()
        {
            var copies = CountCopiesByName();
            // look for names with exactly 2 copies at 1★ and see if shop has it
            for (int i = 0; i < 5; i++)
            {
                var offer = _shop.Get(i);
                if (string.IsNullOrEmpty(offer.Name)) continue;
                if (copies.TryGetValue(offer.Name, out var c) && c == 2 && offer.Cost <= _economy.Gold)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindProgressCopyIndex()
        {
            var copies = CountCopiesByName();
            int bestIdx = -1;
            int bestCost = -1;
            for (int i = 0; i < 5; i++)
            {
                var offer = _shop.Get(i);
                if (string.IsNullOrEmpty(offer.Name)) continue;
                if (_economy.Gold < offer.Cost) continue;
                if (copies.TryGetValue(offer.Name, out var c) && c == 1)
                {
                    // Prefer higher cost when multiple progress options exist
                    if (offer.Cost > bestCost)
                    {
                        bestCost = offer.Cost;
                        bestIdx = i;
                    }
                }
            }
            return bestIdx;
        }

        private int FindHighestCostAffordableIndex()
        {
            int bestIdx = -1;
            int bestCost = -1;
            for (int i = 0; i < 5; i++)
            {
                var offer = _shop.Get(i);
                if (string.IsNullOrEmpty(offer.Name)) continue;
                if (offer.Cost <= _economy.Gold && offer.Cost > bestCost)
                {
                    bestCost = offer.Cost;
                    bestIdx = i;
                }
            }
            return bestIdx;
        }
    }
}

