using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Magic.Inventory;
using Magic.Data;

namespace Magic.Drawing
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class PenController : MonoBehaviour
    {
        private PlayerDataManager playerDataManager;
        private RectTransform penVisual;
        private Image penImage;

        private bool isAtInkBottle = false;
        private RectTransform targetInkBottle;

        public event Action<PlayerDataManager> OnResourceConsumed;

        public Item_Pen CurrentPen
        {
            get
            {
                return InventoryManager.Instance != null ? InventoryManager.Instance.EquippedPen : null;
            }
        }

        private void Awake()
        {
            penVisual = GetComponent<RectTransform>();
            penImage = GetComponent<Image>();
        }

        private void Start()
        {
            playerDataManager = PlayerDataManager.Instance;
            // 매터리얼 인스턴스를 생성하여 원본 에셋(Asset)이 변형되면서 발생하는 에디터 렉 방지
            if (penImage != null && penImage.material != null)
            {
                penImage.material = new Material(penImage.material);

                // 시작 시 디졸브 초기화 방어
                if (penImage.material.HasProperty("_DissolveAmount"))
                {
                    penImage.material.SetFloat("_DissolveAmount", 0f);
                }
            }
        }

        public bool CanDraw()
        {
            Item_Pen pen = CurrentPen;
            if (pen == null) return false;
            if (pen.PenData != null && pen.PenData.consumesMana) return true;
            return pen.currentInkCapacity > 0f;
        }

        public void ConsumeResource()
        {
            Item_Pen pen = CurrentPen;
            if (pen == null) return;
            // if (pen.PenData != null && pen.PenData.consumesMana) return; 
            float rate = pen.PenData.inkConsumptionRate;
            
            if (pen.PenData.consumesMana)
            {
                if (playerDataManager != null)
                {
                    playerDataManager.currentMana -= rate * Time.deltaTime;
                    if (playerDataManager.currentMana < 0) 
                        playerDataManager.currentMana = 0;
                }
            } else {
                pen.currentInkCapacity -= rate * Time.deltaTime;
                if (pen.currentInkCapacity < 0) pen.currentInkCapacity = 0;
            }

            OnResourceConsumed?.Invoke(playerDataManager);
        }

        private void Update()
        {
            // 잉크통에 있을 때는 잉크통의 움직임(떠다니는 효과 등)을 그대로 따라가게 함
            if (isAtInkBottle && targetInkBottle != null)
            {
                penVisual.position = targetInkBottle.position;
                penVisual.rotation = targetInkBottle.rotation;
            }
        }

        private void OnDisable()
        {
            // 게임 종료나 컴포넌트 비활성화 시 머티리얼 값이 중간에 고정되는 것을 방지
            if (penImage != null && penImage.material != null && penImage.material.HasProperty("_DissolveAmount"))
            {
                penImage.material.SetFloat("_DissolveAmount", 0f);
            }
        }

        /// <summary>
        /// Screen Space - Camera 환경 등을 고려하여 마우스의 올바른 월드 좌표를 가져옵니다.
        /// </summary>
        public Vector3 GetMouseWorldPosition(Camera mainCamera)
        {
            Canvas canvas = penVisual.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return Input.mousePosition;
            }

            Camera uiCamera = canvas.worldCamera != null ? canvas.worldCamera : mainCamera;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                penVisual.parent as RectTransform, 
                Input.mousePosition, 
                uiCamera, 
                out Vector3 worldPoint);
                
            return worldPoint;
        }

        /// <summary>
        /// 펜을 마우스 위치로 지속적으로 업데이트합니다.
        /// </summary>
        public void TrackMouse(Camera mainCamera)
        {
            if (CurrentPen == null) return;
            if (InventoryManager.Instance != null && InventoryManager.Instance.EquippedScroll == null) return;
            
            penVisual.position = GetMouseWorldPosition(mainCamera);
        }

        public void PlayIdleAnimation(bool smooth = false)
        {
            if (CurrentPen == null) return;
            penVisual.DOKill();
            
            if (smooth)
            {
                // 현재 각도에서 부드럽게 세워진 후 흔들거림 루프 시작
                DOTween.Sequence()
                    .Append(penVisual.DOLocalRotate(new Vector3(0, 0, -2.5f), 0.25f).SetEase(Ease.InOutSine))
                    .Append(penVisual.DOLocalRotate(new Vector3(0, 0, 2.5f), 1.0f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine))
                    .SetTarget(penVisual);
            }
            else
            {
                // 즉시 세워진 후 흔들거림 루프 시작
                penVisual.localRotation = Quaternion.Euler(0, 0, -2.5f);
                penVisual.DOLocalRotate(new Vector3(0, 0, 2.5f), 1.0f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }

        public void PlayDrawAnimation()
        {
            if (CurrentPen == null) return;
            penVisual.DOKill();
            // 약간 눕혀서 그리는 느낌의 각도로 변경 (-35도, 속도 완화)
            penVisual.DOLocalRotate(new Vector3(0, 0, -35f), 0.25f).SetEase(Ease.OutQuad);
        }

        public void GoToInkBottle(RectTransform inkBottle, Action onArrived, Action onComplete, bool instant = false)
        {
            if (CurrentPen == null) return;
            penVisual.DOKill();
            penVisual.localRotation = Quaternion.Euler(0, 0, 0);
            
            if (inkBottle == null)
            {
                Debug.LogWarning("[PenController] 잉크병(inkBottle)이 할당되지 않았습니다.");
                onArrived?.Invoke();
                isAtInkBottle = false;
                onComplete?.Invoke();
                return;
            }

            targetInkBottle = inkBottle;

            if (instant)
            {
                penVisual.position = targetInkBottle.position;
                penVisual.rotation = targetInkBottle.rotation;
                
                if (penImage != null && penImage.material != null && penImage.material.HasProperty("_DissolveAmount"))
                {
                    penImage.material.DOKill();
                    penImage.material.SetFloat("_DissolveAmount", 0f);
                }
                
                HandleInkBottleArrival(onArrived, onComplete);
            }
            else
            {
                // 디졸브로 순간 이동
                if (penImage != null && penImage.material != null && penImage.material.HasProperty("_DissolveAmount"))
                {
                    Material mat = penImage.material;
                    mat.DOKill();
                    mat.DOFloat(1f, "_DissolveAmount", 0.5f).OnComplete(() => {
                        penVisual.position = targetInkBottle.position;
                        penVisual.rotation = targetInkBottle.rotation;
                        HandleInkBottleArrival(onArrived, null);
                        
                        mat.DOFloat(0f, "_DissolveAmount", 0.5f).OnComplete(() => {
                            onComplete?.Invoke();
                        });
                    });
                }
                else
                {
                    penVisual.position = targetInkBottle.position;
                    penVisual.rotation = targetInkBottle.rotation;
                    HandleInkBottleArrival(onArrived, onComplete);
                }
            }
        }

        private void HandleInkBottleArrival(Action onArrived, Action onComplete)
        {
            // 잉크 장착 여부와 상관없이 무조건 잉크통 위치를 지속적으로 따라다니도록 설정
            // (잉크통 쪽 FloatingEffect가 상시 돌아가면서 펜도 자연스럽게 같이 떠다님)
            isAtInkBottle = true;
            
            onArrived?.Invoke();
            onComplete?.Invoke();
        }

        public void ReturnToMouse(Camera mainCamera, Action onComplete)
        {
            if (CurrentPen == null)
            {
                onComplete?.Invoke();
                return;
            }
            
            if (InventoryManager.Instance != null && InventoryManager.Instance.EquippedScroll == null)
            {
                onComplete?.Invoke();
                return;
            }

            isAtInkBottle = false;

            if (penImage != null && penImage.material != null && penImage.material.HasProperty("_DissolveAmount"))
            {
                Material mat = penImage.material;
                mat.DOKill();
                
                mat.DOFloat(1f, "_DissolveAmount", 0.3f).OnComplete(() => {
                    
                    TrackMouse(mainCamera);
                        
                    // 디졸브(나타나기) 전, 펜이 보이지 않을 때 미리 각도와 애니메이션 세팅
                    PlayIdleAnimation();
                    
                    mat.DOFloat(0f, "_DissolveAmount", 0.3f)
                       .OnUpdate(() => {
                           TrackMouse(mainCamera);
                       })
                       .OnComplete(() => {
                           onComplete?.Invoke();
                       });
                });
            }
            else
            {
                PlayIdleAnimation();
                onComplete?.Invoke();
            }
        }
    }
}
