using UnityEngine;
using DG.Tweening;

namespace Magic.Drawing
{
    [RequireComponent(typeof(RectTransform))]
    public class FloatingEffect : MonoBehaviour
    {
        [Header("Float Settings")]
        public float floatAmount = 10f;       // 떠오르는 높이
        public float floatDuration = 2.5f;    // 한 번 떠오르는 데 걸리는 시간

        [Header("Rotation Settings")]
        public float rotationAmount = 1.5f;   // 좌우로 기우는 각도
        public float rotationDuration = 3.5f; // 기우는 데 걸리는 시간 (높이와 다르게 주어 불규칙한 느낌 연출)

        private RectTransform rectTransform;
        private Vector2 startPos;
        private Quaternion startRot;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                startPos = rectTransform.anchoredPosition;
                startRot = rectTransform.localRotation;
                StartFloating();
            }
        }

        private void StartFloating()
        {
            rectTransform.DOKill();
            
            // 위아래로 둥둥 떠다니는 애니메이션
            rectTransform.DOAnchorPosY(startPos.y + floatAmount, floatDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            // 미세하게 좌우로 흔들리는 애니메이션
            rectTransform.DOLocalRotate(new Vector3(0, 0, rotationAmount), rotationDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        public void PauseFloating()
        {
            if (rectTransform != null)
            {
                rectTransform.DOKill();
                // 그리기 편하도록 부드럽게 원래 위치로 복귀
                rectTransform.DOAnchorPos(startPos, 0.3f).SetEase(Ease.OutQuad);
                rectTransform.DOLocalRotateQuaternion(startRot, 0.3f).SetEase(Ease.OutQuad);
            }
        }

        public void ResumeFloating()
        {
            if (rectTransform != null && gameObject.activeInHierarchy)
            {
                StartFloating();
            }
        }

        private void OnDisable()
        {
            if (rectTransform != null)
            {
                rectTransform.DOKill();
                rectTransform.anchoredPosition = startPos;
                rectTransform.localRotation = startRot;
            }
        }
        
        private void OnEnable()
        {
            // Start()가 실행되어 초기 좌표가 저장된 이후라면 다시 켤 때 애니메이션 재시작
            if (rectTransform != null && startPos != Vector2.zero) 
            {
                StartFloating();
            }
        }
    }
}
