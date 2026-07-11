using UnityEngine;
using DG.Tweening;

namespace Magic.Drawing
{
    [RequireComponent(typeof(RectTransform))]
    public class FloatingEffect : MonoBehaviour
    {
        [Header("Float Settings")]
        public float floatAmount = 20f;       // 떠오르는 높이 (상하 움직임 증가)
        public float floatDuration = 4.5f;    // 한 번 떠오르는 데 걸리는 시간 (시간 늘림)

        [Header("Rotation Settings")]
        public float rotationAmount = 0.5f;   // 좌우로 기우는 각도 (좌우 움직임 감소)
        public float rotationDuration = 6.0f; // 기우는 데 걸리는 시간 (높이와 다르게 주어 불규칙한 느낌 연출)

        [Header("Randomize Settings")]
        public bool randomizeTiming = true;   // 각자만의 타이밍(오프셋/속도)을 가질지 여부

        private RectTransform rectTransform;
        private Vector2 startPos;
        private Quaternion startRot;
        private float randomTimeOffsetFloat;
        private float randomTimeOffsetRot;
        private float randomDurationMult;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                startPos = rectTransform.anchoredPosition;
                startRot = rectTransform.localRotation;

                // 오브젝트마다 다른 타이밍을 갖도록 랜덤 오프셋 및 속도 배수 설정
                if (randomizeTiming)
                {
                    randomTimeOffsetFloat = Random.Range(0f, floatDuration * 2f);
                    randomTimeOffsetRot = Random.Range(0f, rotationDuration * 2f);
                    randomDurationMult = Random.Range(0.85f, 1.15f);
                }
                else
                {
                    randomDurationMult = 1f;
                }

                StartFloating(true);
            }
        }

        private void StartFloating(bool useRandomOffset = false)
        {
            rectTransform.DOKill();
            
            float fDuration = floatDuration * randomDurationMult;
            float rDuration = rotationDuration * randomDurationMult;

            // 위아래로 둥둥 떠다니는 애니메이션
            Tween floatTween = rectTransform.DOAnchorPosY(startPos.y + floatAmount, fDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            // 미세하게 좌우로 흔들리는 애니메이션
            Tween rotTween = rectTransform.DOLocalRotate(new Vector3(0, 0, rotationAmount), rDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            if (randomizeTiming && useRandomOffset)
            {
                floatTween.Goto(randomTimeOffsetFloat, true);
                rotTween.Goto(randomTimeOffsetRot, true);
            }
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
                StartFloating(false);
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
                StartFloating(true);
            }
        }
    }
}
