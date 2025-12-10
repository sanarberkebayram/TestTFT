using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestTFT.Scripts.Runtime.UI.Common.Tooltip
{
    public sealed class TooltipController : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Text _text;

        private static TooltipController _instance;

        private void Awake()
        {
            _instance = this;
            Hide();
        }

        public static void Show(string message, Vector2 screenPos)
        {
            if (_instance == null) return;
            _instance._text.text = message;
            _instance._panel.gameObject.SetActive(true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _instance._canvas.transform as RectTransform,
                screenPos, _instance._canvas.worldCamera, out var local);
            _instance._panel.anchoredPosition = local + new Vector2(8, -8);
        }

        public static void Hide()
        {
            if (_instance == null) return;
            _instance._panel.gameObject.SetActive(false);
        }
    }

    public sealed class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea]
        public string Message;

        public void OnPointerEnter(PointerEventData eventData)
        {
            TooltipController.Show(Message, eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipController.Hide();
        }
    }
}

