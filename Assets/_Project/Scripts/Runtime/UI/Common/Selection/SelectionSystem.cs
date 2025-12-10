using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TestTFT.Scripts.Runtime.UI.Common.Selection
{
    public static class SelectionSystem
    {
        public static GameObject Current;
    }

    public sealed class SelectOnClick : MonoBehaviour, IPointerClickHandler
    {
        private Image _image;
        private Color _orig;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null) _orig = _image.color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SelectionSystem.Current = gameObject;
            if (_image != null) _image.color = _orig * 1.2f;
        }

        private void OnDisable()
        {
            if (_image != null) _image.color = _orig;
            if (SelectionSystem.Current == gameObject) SelectionSystem.Current = null;
        }
    }
}

