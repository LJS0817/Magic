using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HaggleChatUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dealText;
    [SerializeField] private CustomSlider slider; // 빨간줄 오브젝트

    private CanvasGroup _canvasGroup;
    private Tween _lineTween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void Show(string deal, bool isRejected)
    {
        _canvasGroup.alpha = 1f;
        dealText.text = deal.Split(':')[1];
        
        if (slider != null)
        {
            _lineTween?.Kill();
            if (isRejected)
            {
                slider.SetValue(0f, 1f);
                _lineTween = DOTween.To(() => 0f, x => slider.SetValue(x, 1f), 1f, 0.5f).SetEase(Ease.OutQuart);
            }
        }
    }

    public void Close()
    {
        _canvasGroup.alpha = 0f;
        
        _lineTween?.Kill();
        slider.SetValue(0f, 1f);
    }
}

