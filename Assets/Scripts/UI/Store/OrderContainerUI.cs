using UnityEngine;
using DG.Tweening;

namespace Magic.Store
{
    [RequireComponent(typeof(RectTransform))]
    public class OrderContainerUI : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Vector2 openPosition = Vector2.zero;
        [SerializeField] private float animationDuration = 0.4f;
        [Tooltip("Ease.OutBack provides a single bounce (overshoot) effect.")]
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private float bounceOvershoot = 1.5f; // Control the strength of the 1-time bounce
        [SerializeField] private Ease closeEase = Ease.InBack;

        private RectTransform _rectTransform;
        private Vector2 _closedPosition;
        private Tween _currentTween;

        bool isOpen = false;
        public bool IsOpened => isOpen;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _closedPosition = _rectTransform.anchoredPosition;
            openPosition = _closedPosition + openPosition; // Adjust open position relative to closed position
        }

        public void ToggleUI()
        {
            if (isOpen) Close();
            else Open();
            isOpen = !isOpen;
        }

        public void Open()
        {
            _currentTween?.Kill();
            _currentTween = _rectTransform.DOAnchorPos(openPosition, animationDuration).SetEase(openEase, bounceOvershoot);
        }

        public void Close()
        {
            _currentTween?.Kill();

            // Animate to closed position
            _currentTween = _rectTransform.DOAnchorPos(_closedPosition, animationDuration).SetEase(closeEase);
        }
    }
}
