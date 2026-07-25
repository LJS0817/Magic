using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MapFocusableObject : MonoBehaviour
{
    [SerializeField] string _name;
    [SerializeField] string _sceneName;
    [SerializeField] UnityEvent _onClickEvent;
    
    Image _img;
    public RectTransform RectTrans { get; private set; }

    private void Awake()
    {
        _img = GetComponent<Image>();
        RectTrans = GetComponent<RectTransform>();
    }

    public (string, string) GetInfo()
    {
        return (_name, _sceneName);
    }

    public void OnClick()
    {
        Cursor.visible = true;
        if(_onClickEvent.GetPersistentEventCount() == 0) {
            if (!string.IsNullOrEmpty(_sceneName))
            {
                // 클릭 입력 프레임과 씬 로드 프레임을 분리하여 다음 씬으로 클릭 이벤트가 전파되는 것을 방지
                DOVirtual.DelayedCall(0.05f, () => SceneManager.LoadScene(_sceneName));
            }
        } else {
            _onClickEvent?.Invoke();
        }
    }

    public void Focus()
    {
        if (_img != null)
        {
            _img.DOKill();
            _img.transform.DOScale(1.1f, 0.2f);
        }
    }

    public void Unfocus()
    {
        if (_img != null)
        {
            _img.DOKill();
            _img.transform.DOScale(1.0f, 0.2f);
        }
    }
}
