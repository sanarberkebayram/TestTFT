using TestTFT.Scripts.Runtime.Systems.Gameplay;
using TestTFT.Scripts.Runtime.UI.Common.Tooltip;
using TestTFT.Scripts.Runtime.UI.HUD;
using TestTFT.Scripts.Runtime.UI.Shop;
using TestTFT.Scripts.Runtime.UI.Carousel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.Systems.Bootstrap
{
    public sealed class UIBootstrap : MonoBehaviour
    {
        private EconomySystem _economy;
        private ShopSystem _shop;
        private GameLoopSystem _loop;
        private CarouselSystem _carousel;
        private GameObject _benchRoot;

        private Canvas _canvas;

        private bool _firstShop = true;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            EnsureEventSystem();
            _canvas = EnsureCanvas();
            _economy = new EconomySystem();
            _shop = new ShopSystem();
            _shop.RerollForLevel(_economy.Level);
            _loop = new GameLoopSystem();
            _carousel = new CarouselSystem();
        }

        private void Start()
        {
            BuildHUD();
            BuildBench();
            BuildShop();
            BuildCarousel();
            BuildTraitsUI();
            BuildTooltip();
            _loop.OnPhaseChanged += OnPhaseChanged;
            _loop.ForceShop(20f);
        }

        private void Update()
        {
            _loop.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_loop != null) _loop.OnPhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(GameLoopSystem.Phase phase)
        {
            if (phase == GameLoopSystem.Phase.Shop)
            {
                if (_firstShop)
                {
                    _firstShop = false;
                }
                else
                {
                    bool win = TestTFT.Scripts.Runtime.Systems.Core.DeterministicRng.NextFloat01(TestTFT.Scripts.Runtime.Systems.Core.DeterministicRng.Stream.Loot) > 0.5f;
                    _economy.AddStreak(win);
                }
                _economy.AddGold(5); // base income
                _economy.GainInterest();
                _shop.RerollForLevel(_economy.Level);
            }
            else if (phase == GameLoopSystem.Phase.Carousel)
            {
                _carousel.StartRound();
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private static Canvas EnsureCanvas()
        {
            var existing = FindObjectOfType<Canvas>();
            if (existing != null) return existing;

            var go = new GameObject("Canvas", typeof(Canvas));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        private void BuildHUD()
        {
            var hud = new GameObject("HUD");
            hud.transform.SetParent(_canvas.transform, false);
            var grid = hud.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 28);
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 1;
            grid.spacing = new Vector2(8, 0);

            var gold = CreateText("Gold: 0", hud.transform);
            var streak = CreateText("Streak: 0", hud.transform);
            var interest = CreateText("Interest: 0", hud.transform);
            var xp = CreateText("LV 1  XP 0/6", hud.transform);

            var xpBarGo = new GameObject("XPBar");
            xpBarGo.transform.SetParent(hud.transform, false);
            var xpBg = xpBarGo.AddComponent<Image>();
            xpBg.color = new Color(0, 0, 0, 0.35f);
            var xpFillGo = new GameObject("Fill");
            xpFillGo.transform.SetParent(xpBarGo.transform, false);
            var xpFill = xpFillGo.AddComponent<Image>();
            xpFill.color = new Color(0.2f, 0.6f, 1f, 0.85f);
            var rt = xpBarGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 12);
            var rtFill = xpFillGo.GetComponent<RectTransform>();
            rtFill.anchorMin = new Vector2(0, 0);
            rtFill.anchorMax = new Vector2(0, 1);
            rtFill.pivot = new Vector2(0, 0.5f);
            rtFill.sizeDelta = new Vector2(160, 0);

            var timer = CreateText("20s", hud.transform);
            var buyXp = CreateButton("Buy XP (4g)", hud.transform);

            var reroll = CreateButton("Reroll (2g)", hud.transform);
            var lck = CreateButton("Lock", hud.transform);

            var ctrl = hud.AddComponent<HUDController>();
            ctrl.goldText = gold;
            ctrl.streakText = streak;
            ctrl.xpText = xp;
            ctrl.xpFill = xpFill;
            ctrl.timerText = timer;
            ctrl.rerollButton = reroll;
            ctrl.lockButton = lck;
            ctrl.interestText = interest;
            ctrl.buyXpButton = buyXp;
            ctrl.Init(_economy, _shop, _loop);
        }

        private void BuildShop()
        {
            var panel = new GameObject("Shop");
            panel.transform.SetParent(_canvas.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 20);

            var grid = panel.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(180, 64);
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 1;
            grid.spacing = new Vector2(8, 0);

            var ctrl = panel.AddComponent<ShopController>();
            ctrl.slots = new ShopController.Slot[5];
            for (int i = 0; i < 5; i++)
            {
                var slot = new GameObject($"Slot{i+1}");
                slot.transform.SetParent(panel.transform, false);
                var bg = slot.AddComponent<Image>();
                bg.color = new Color(0, 0, 0, 0.25f);

                var name = CreateText("Name", slot.transform);
                var cost = CreateText("1g", slot.transform);
                var btn = CreateButton($"Buy [{i+1}]", slot.transform);

                var layout = slot.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 2f;
                layout.childControlHeight = true;

                ctrl.slots[i] = new ShopController.Slot
                {
                    nameText = name,
                    costText = cost,
                    buyButton = btn
                };
            }
            // Bind bench controller for spawning purchased units
            var benchCtrl = _benchRoot.GetComponent<TestTFT.Scripts.Runtime.UI.Bench.BenchController>();
            ctrl.Init(_economy, _shop, benchCtrl);
        }

        private void BuildBench()
        {
            var bench = new GameObject("Bench");
            _benchRoot = bench;
            bench.transform.SetParent(_canvas.transform, false);
            var rt = bench.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 100);

            var grid = bench.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(90, 90);
            grid.spacing = new Vector2(6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 1;

            for (int i = 0; i < 8; i++)
            {
                var slot = new GameObject($"BenchSlot{i+1}");
                slot.transform.SetParent(bench.transform, false);
                var img = slot.AddComponent<Image>();
                img.color = new Color(0.1f, 0.1f, 0.1f, 0.4f);
                slot.AddComponent<TestTFT.Scripts.Runtime.UI.Common.DragDrop.DropSlot>();

                var unit = new GameObject("Unit");
                unit.transform.SetParent(slot.transform, false);
                var uimg = unit.AddComponent<Image>();
                uimg.color = new Color(0.5f, 0.7f, 1f, 0.9f);
                unit.AddComponent<TestTFT.Scripts.Runtime.UI.Common.DragDrop.Draggable>();
                var tip = unit.AddComponent<TestTFT.Scripts.Runtime.UI.Common.Tooltip.TooltipTarget>();
                tip.Message = "Drag me! Delete to sell";
                unit.AddComponent<TestTFT.Scripts.Runtime.UI.Common.Selection.SelectOnClick>();
                var sell = unit.AddComponent<TestTFT.Scripts.Runtime.UI.Common.Selection.SellOnDelete>();
                sell.Init(_economy, _shop);

                // Sample trait assignment for demo purposes
                var ut = unit.AddComponent<TestTFT.Scripts.Runtime.Systems.Traits.UnitTraits>();
                if (i % 3 == 0) ut.baseTraits.Add("warrior");
                if (i % 3 == 1) ut.baseTraits.Add("ranger");
                if (i % 3 == 2) ut.baseTraits.Add("mystic");
                if (i == 0) ut.emblems.Add("warrior"); // example emblem on first
            }

            // Attach controllers
            var benchCtrl = bench.AddComponent<TestTFT.Scripts.Runtime.UI.Bench.BenchController>();
            benchCtrl.Init(_economy, _shop);
            bench.AddComponent<TestTFT.Scripts.Runtime.Systems.Gameplay.CombineOnRosterChange>();
        }

        private void BuildTraitsUI()
        {
            var panel = new GameObject("TraitsPanel");
            panel.transform.SetParent(_canvas.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-20, -20);

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.35f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(panel.transform, false);
            var t = textGo.AddComponent<Text>();
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.alignment = TextAnchor.UpperLeft;
            var trt = t.GetComponent<RectTransform>();
            trt.sizeDelta = new Vector2(260, 160);

            var traitsUI = panel.AddComponent<TestTFT.Scripts.Runtime.UI.Traits.TraitsUIController>();
            traitsUI.output = t;

            // Attach a roster tracker to the bench and bind
            var tracker = _benchRoot.AddComponent<TestTFT.Scripts.Runtime.Systems.Traits.RosterTracker>();
            traitsUI.BindToRoster(tracker);
        }

        private void BuildTooltip()
        {
            var root = new GameObject("Tooltip");
            root.transform.SetParent(_canvas.transform, false);
            var panel = root.AddComponent<Image>();
            panel.color = new Color(0, 0, 0, 0.8f);
            var rt = root.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(220, 60);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(root.transform, false);
            var t = textGo.AddComponent<Text>();
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.alignment = TextAnchor.UpperLeft;
            t.raycastTarget = false;
            var trt = t.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(1, 1);
            trt.offsetMin = new Vector2(8, 8);
            trt.offsetMax = new Vector2(-8, -8);

            var tip = root.AddComponent<TooltipController>();
            tip.GetType(); // keep reference
        }

        private static Text CreateText(string content, Transform parent)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.color = Color.white;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.alignment = TextAnchor.MiddleLeft;
            return t;
        }

        private static Button CreateButton(string label, Transform parent)
        {
            var go = new GameObject("Button", typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var btn = go.AddComponent<Button>();

            var txt = CreateText(label, go.transform);
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(8, 0);
            rt.offsetMax = new Vector2(-8, 0);
            txt.alignment = TextAnchor.MiddleCenter;

            var size = go.GetComponent<RectTransform>();
            size.sizeDelta = new Vector2(160, 28);
            return btn;
        }
        private void BuildCarousel()
        {
            var panel = new GameObject("Carousel");
            panel.transform.SetParent(_canvas.transform, false);
            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.4f);

            var grid = panel.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 80);
            grid.spacing = new Vector2(12, 12);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4; // 4x2 grid

            var ctrl = panel.AddComponent<CarouselController>();
            ctrl.slots = new CarouselController.Slot[8];
            for (int i = 0; i < 8; i++)
            {
                var slot = new GameObject($"CarouselSlot{i + 1}");
                slot.transform.SetParent(panel.transform, false);
                var bgSlot = slot.AddComponent<Image>();
                bgSlot.color = new Color(0, 0, 0, 0.25f);

                var name = CreateText("Name", slot.transform);
                var cost = CreateText("1g", slot.transform);
                var btn = CreateButton($"Pick [{i + 1}]", slot.transform);

                var layout = slot.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 2f;
                layout.childControlHeight = true;

                ctrl.slots[i] = new CarouselController.Slot
                {
                    nameText = name,
                    costText = cost,
                    selectButton = btn
                };
            }
            ctrl.Init(_economy, _carousel, _loop);
            panel.SetActive(false);
        }
    }
}
