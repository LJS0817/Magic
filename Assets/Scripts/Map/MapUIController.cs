using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapUIController : MonoBehaviour
{
    // 좌표 캐싱을 위한 내부 클래스
    class CachedFocusable
    {
        public MapFocusableObject Obj;
        public Rect Rect;
        public Vector2 Center;
    }

    [SerializeField] MapMonocle _monocle;
    [SerializeField] Transform _rootTransform;
    MapFocusableObject _currentFocused;
    
    List<CachedFocusable> _cachedObjects = new List<CachedFocusable>();
    RectTransform _monocleRect;

    [SerializeField] float _focusDistance = 1f; // 포커스를 잡을 최대 거리 (조절 가능)

    CanvasGroup _group;

    bool isOpened = false;

    void Start()
    {
        _group = GetComponent<CanvasGroup>();
        // 씬 내의 모든 MapFocusableObject를 찾습니다.
        foreach (var obj in _rootTransform.GetComponentsInChildren<MapFocusableObject>())
        {
            if (obj != null && obj.RectTrans != null)
            {
                // 변하지 않는 건축물의 Rect와 중심점(Center)을 게임 시작 시 미리 캐싱해둡니다.
                Rect worldRect = GetWorldRect(obj.RectTrans);
                _cachedObjects.Add(new CachedFocusable
                {
                    Obj = obj,
                    Rect = worldRect,
                    Center = worldRect.center
                });
            }
        }

        if (_monocle != null)
        {
            _monocleRect = _monocle.GetComponent<RectTransform>();
        }

        Close();
    }

    public void Open(bool justTrigged = false)
    {
        Cursor.visible = false;
        isOpened = true;
        if (justTrigged) return;
        _group.DOKill();
        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        _group.interactable = true;
    }

    public void Close(bool justTrigged = false)
    {
        Cursor.visible = true;
        _monocle.ClearInfo();
        isOpened = false;
        if (justTrigged) return;
        _group.DOKill();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;
    }

    void Update()
    {
        if (!isOpened) return;
        if (_monocle != null)
        {
            _monocle.UpdatePosition();
        }

        DetectFocusableObject();

        if (Input.GetMouseButtonDown(0) && _currentFocused != null)
        {
            var info = _currentFocused.GetInfo();
            if(info.Item2.Equals("exit"))
            {
                Close();
                return;
            }

            isOpened = false; // 씬 전환 중 중복 클릭 및 입력 방지
            _currentFocused.OnClick();
        }
    }

    void DetectFocusableObject()
    {
        if (_monocleRect == null) return;

        // 모노클의 중심점만 가져옵니다.
        Vector2 monocleCenter = _monocleRect.position;
        
        CachedFocusable closestCached = null;
        float minSqrDistance = float.MaxValue;
        float focusSqrDist = _focusDistance * _focusDistance;

        // 캐싱된 데이터를 순회하며 거리를 확인합니다.
        foreach (var cached in _cachedObjects)
        {
            if (cached.Obj == null || !cached.Obj.gameObject.activeInHierarchy) continue;

            float sqrDist = (monocleCenter - cached.Center).sqrMagnitude;
            
            // 지정된 거리(_focusDistance) 이내일 때만 판정
            if (sqrDist < focusSqrDist && sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                closestCached = cached;
            }
        }

        MapFocusableObject closestObj = closestCached?.Obj;

        // 현재 포커스 대상이 바뀌었을 경우 처리
        if (closestObj != _currentFocused)
        {
            if (_currentFocused != null)
            {
                _currentFocused.Unfocus();
                if (_monocle != null) _monocle.ClearInfo();
            }

            _currentFocused = closestObj;

            if (_currentFocused != null)
            {
                _currentFocused.Focus();
                var info = _currentFocused.GetInfo();
                if (_monocle != null) _monocle.SetInfo(info.Item1);
            }
        }
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 min = corners[0];
        Vector2 max = corners[2];
        Vector2 size = max - min;
        return new Rect(min, size);
    }
}
