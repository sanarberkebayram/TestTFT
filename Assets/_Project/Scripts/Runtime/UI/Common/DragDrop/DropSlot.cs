using UnityEngine;
using UnityEngine.EventSystems;

namespace TestTFT.Scripts.Runtime.UI.Common.DragDrop
{
    public sealed class DropSlot : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            var drag = eventData.pointerDrag;
            if (drag == null) return;
            var rect = drag.GetComponent<RectTransform>();
            drag.transform.SetParent(transform);
            rect.anchoredPosition = Vector2.zero;
            // Notify roster systems that a drop occurred (roster potentially changed)
            TestTFT.Scripts.Runtime.Systems.Traits.RosterEvents.Raise();
        }
    }
}
