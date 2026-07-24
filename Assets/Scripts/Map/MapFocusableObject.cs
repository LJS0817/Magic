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
        if(_onClickEvent == null) {
            if (!string.IsNullOrEmpty(_sceneName)) SceneManager.LoadScene(_sceneName);
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
