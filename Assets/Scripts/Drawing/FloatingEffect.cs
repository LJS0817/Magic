using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class FloatingEffect : MonoBehaviour
{
    [Header("Float Settings")]
    public bool playInStart = false;
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

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdateBasePosition();
    }

    private void Start()
    {
        // 오브젝트마다 다른 타이밍을 갖도록 랜덤 오프셋 및 속도 배수 설정
        CalculateRandomValues();
        if (playInStart) StartFloating(true);
    }

    void UpdateBasePosition()
    {
        startPos = rectTransform.anchoredPosition;
        startRot = rectTransform.localRotation;
    }

    private void StartFloating(bool useRandomOffset = false)
    {
        if (IsPlaying) rectTransform.DOKill();
        IsPlaying = true;

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
        IsPlaying = false;
        if (rectTransform != null)
        {
            rectTransform.DOKill();
            // 그리기 편하도록 부드럽게 원래 위치로 복귀
            rectTransform.DOAnchorPos(startPos, 0.3f).SetEase(Ease.OutQuad);
            rectTransform.DOLocalRotateQuaternion(startRot, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    public void StopFloating()
    {
        IsPlaying = false;
        if (rectTransform != null)
        {
            rectTransform.DOKill();
            rectTransform.anchoredPosition = startPos;
            rectTransform.localRotation = startRot;
        }
    }

    public void ResumeFloating()
    {
        if (rectTransform != null && !IsPlaying)
        {
            StartFloating(false);
        }
    }

    void CalculateRandomValues()
    {
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

    }

    public void RestartFloatingEffect()
    {
        if (rectTransform == null || IsPlaying) return;

        rectTransform.DOKill();

        CalculateRandomValues();
        StartFloating(true);
    }
}

