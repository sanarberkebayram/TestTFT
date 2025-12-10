using UnityEngine;
using UnityEngine.EventSystems;

namespace TestTFT.Scripts.Runtime.UI.Common.DragDrop
{
    public sealed class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Canvas _canvas;
        private RectTransform _rect;
        private CanvasGroup _group;
        private Transform _originalParent;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _rect = GetComponent<RectTransform>();
            _group = gameObject.GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalParent = transform.parent;
            transform.SetParent(_canvas.transform);
            _group.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position, _canvas.worldCamera, out var pos);
            _rect.anchoredPosition = pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _group.blocksRaycasts = true;
            if (transform.parent == _canvas.transform)
            {
                transform.SetParent(_originalParent);
                _rect.anchoredPosition = Vector2.zero;
            }
        }
    }
}

