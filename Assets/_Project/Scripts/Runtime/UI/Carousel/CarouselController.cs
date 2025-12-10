using System;
using TestTFT.Scripts.Runtime.Systems.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.UI.Carousel
{
    public sealed class CarouselController : MonoBehaviour
    {
        [Serializable]
        public sealed class Slot
        {
            public Text nameText;
            public Text costText;
            public Button selectButton;
        }

        public Slot[] slots = new Slot[8];

        private EconomySystem _economy;
        private CarouselSystem _carousel;
        private GameLoopSystem _loop;

        public void Init(EconomySystem economy, CarouselSystem carousel, GameLoopSystem loop)
        {
            _economy = economy;
            _carousel = carousel;
            _loop = loop;

            _carousel.OnChanged += Refresh;
            _carousel.OnPicked += OnPicked;
            _loop.OnPhaseChanged += OnPhaseChanged;

            for (int i = 0; i < slots.Length; i++)
            {
                int idx = i;
                if (slots[i]?.selectButton != null)
                    slots[i].selectButton.onClick.AddListener(() => TryPick(idx));
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_carousel != null)
            {
                _carousel.OnChanged -= Refresh;
                _carousel.OnPicked -= OnPicked;
            }
            if (_loop != null) _loop.OnPhaseChanged -= OnPhaseChanged;
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) TryPick(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryPick(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryPick(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) TryPick(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) TryPick(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) TryPick(5);
            if (Input.GetKeyDown(KeyCode.Alpha7)) TryPick(6);
            if (Input.GetKeyDown(KeyCode.Alpha8)) TryPick(7);
        }

        private void OnPhaseChanged(GameLoopSystem.Phase p)
        {
            bool active = p == GameLoopSystem.Phase.Carousel;
            gameObject.SetActive(active);
            if (active) Refresh();
        }

        private void OnPicked(int index, CarouselSystem.Offer offer)
        {
            // Reward: simple MVP bonus
            _economy.AddGold(3);
            // End carousel early if picked
            _loop.EndCarouselNow();
        }

        private void TryPick(int index)
        {
            _carousel.TryPick(index);
        }

        private void Refresh()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (s == null) continue;
                var offer = _carousel.Get(i);
                if (s.nameText) s.nameText.text = offer.Name;
                if (s.costText) s.costText.text = $"{offer.Cost}g";
                if (s.selectButton) s.selectButton.interactable = _carousel.Active;
            }
        }
    }
}

