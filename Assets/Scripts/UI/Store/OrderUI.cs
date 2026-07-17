using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Magic.Store
{
    [RequireComponent(typeof(RectTransform))]
    public class OrderUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public CustomerOrder OrderData { get; private set; }
        private Action<OrderUI> _onClickCallback;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private RectTransform _parentRectTransform;

        public void Initialize(CustomerOrder order, Action<OrderUI> onClickCallback, Canvas canvas)
        {
            OrderData = order;
            _onClickCallback = onClickCallback;
            _canvas = canvas;
            _rectTransform = GetComponent<RectTransform>();
            _parentRectTransform = _rectTransform.parent as RectTransform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Bring to front while dragging
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null || _parentRectTransform == null) return;

            // Move the object based on pointer delta and canvas scale
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
            ClampPosition();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ClampPosition();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Don't register click if we were dragging
            if (eventData.dragging) return;

            _onClickCallback?.Invoke(this);
        }

        private void ClampPosition()
        {
            if (_parentRectTransform == null) return;

            Vector3[] parentCorners = new Vector3[4];
            _parentRectTransform.GetWorldCorners(parentCorners);

            Vector3[] rectCorners = new Vector3[4];
            _rectTransform.GetWorldCorners(rectCorners);

            // Using local bounds of parent
            Vector2 minPosition = _parentRectTransform.rect.min - _rectTransform.rect.min;
            Vector2 maxPosition = _parentRectTransform.rect.max - _rectTransform.rect.max;

            Vector2 clampedPosition = _rectTransform.anchoredPosition;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minPosition.x, maxPosition.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minPosition.y, maxPosition.y);

            _rectTransform.anchoredPosition = clampedPosition;
        }

        public void CompleteOrder()
        {
            OrderData.state = OrderState.Completed;
            Destroy(gameObject);
        }

        public void FailOrder()
        {
            OrderData.state = OrderState.Failed;
            Destroy(gameObject);
        }
    }
}
