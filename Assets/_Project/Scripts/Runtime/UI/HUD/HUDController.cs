using System;
using TestTFT.Scripts.Runtime.Systems.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.UI.HUD
{
    public sealed class HUDController : MonoBehaviour
    {
        [Header("Refs")]
        public Text goldText;
        public Text streakText;
        public Text xpText;
        public Text interestText;
        public Image xpFill;
        public Text timerText;
        public Button rerollButton;
        public Button lockButton;
        public Button buyXpButton;

        private EconomySystem _economy;
        private ShopSystem _shop;
        private GameLoopSystem _loop;

        public void Init(EconomySystem economy, ShopSystem shop, GameLoopSystem loop)
        {
            _economy = economy;
            _shop = shop;
            _loop = loop;

            _economy.OnChanged += RefreshEconomy;
            _shop.OnChanged += RefreshShop;
            _loop.OnTimer += OnTimer;

            rerollButton.onClick.AddListener(() => OnReroll());
            lockButton.onClick.AddListener(() => OnLock());
            if (buyXpButton != null) buyXpButton.onClick.AddListener(() => OnBuyXp());

            if (xpFill)
            {
                xpFill.type = Image.Type.Filled;
                xpFill.fillMethod = Image.FillMethod.Horizontal;
                xpFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                xpFill.fillAmount = 0f;
            }

            RefreshEconomy();
            RefreshShop();
        }

        private void OnDestroy()
        {
            if (_economy != null) _economy.OnChanged -= RefreshEconomy;
            if (_shop != null) _shop.OnChanged -= RefreshShop;
            if (_loop != null) _loop.OnTimer -= OnTimer;
        }

        private void RefreshEconomy()
        {
            if (goldText) goldText.text = $"Gold: {_economy.Gold}";
            if (streakText) streakText.text = $"Streak: {_economy.Streak}";
            if (xpText) xpText.text = $"LV {_economy.Level}  XP {_economy.Xp}/{_economy.XpToLevel}";
            if (interestText) interestText.text = $"Interest: {Mathf.Min(5, _economy.Gold / 10)}";
            if (xpFill) xpFill.fillAmount = Mathf.Clamp01(_economy.Xp / (float)Math.Max(1, _economy.XpToLevel));
        }

        private void RefreshShop()
        {
            if (lockButton)
            {
                var t = lockButton.GetComponentInChildren<Text>();
                if (t) t.text = _shop.Locked ? "Unlock" : "Lock";
            }
        }

        private void OnTimer(float remaining, float total)
        {
            if (!timerText) return;
            timerText.text = $"{Mathf.CeilToInt(remaining)}s";
        }

        private void OnReroll()
        {
            // Reroll costs 2g typical
            if (_economy.TrySpend(2))
            {
                _shop.RerollForLevel(_economy.Level);
            }
        }

        private void OnLock()
        {
            _shop.ToggleLock();
        }

        private void OnBuyXp()
        {
            // MVP: Spend 4g for 4 XP
            if (_economy.TrySpend(4))
            {
                _economy.AddXp(4);
            }
        }
    }
}
