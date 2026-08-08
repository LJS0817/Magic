using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class DrawingManager : MonoBehaviour
{
    public static bool IsDrawingBlocked = false;

    [Header("Magic Combo Stats")]
    public List<DrawnShape> drawnShapes = new List<DrawnShape>();

    [Header("Resources (UI Only)")]
    public CustomSlider manaSlider;

    [Header("References")]
    public RectTransform drawingArea; // 스크롤 UI 영역
    FloatingEffect scrollFloatingEffect;

    [Header("Settings")]
    public Camera mainCamera;

    public DrawingData currentDrawingData { get; protected set; }
    protected DrawingLine currentLine;
    protected bool isDrawing = false;
    protected bool isRefilling = false;
    protected bool isOutsideArea = false;
    protected bool isPenReady = false;

    protected DrawingDatabase drawingDatabase;

    [Header("Pen Visuals")]
    public PenController penController;
    [Header("Ink Controller")]
    public InkController inkController;

    protected virtual void Awake()
    {
        ShapeRecognizer.EnsureInitialized();
        IsDrawingBlocked = false;
    }

    protected virtual void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        currentDrawingData = new DrawingData();
        drawingDatabase = DrawingDatabase.Instance;
        // 처음에 펜을 잉크통에 위치시키고 바깥에 있는 것으로 처리
        if (penController != null && inkController != null && inkController.inkBottleVisual != null)
        {
            penController.GoToInkBottle(inkController.inkBottleVisual, null, () => {
                isPenReady = true;
            }, instant: true);
        }
        else
        {
            isPenReady = true;
        }
        isOutsideArea = true;

        // 시작 시 잉크가 비어있을 수 있으므로 충전 한 번 시도
        if (inkController != null && penController != null)
        {
            inkController.TryRefillPen(penController.CurrentPen);
        }

        scrollFloatingEffect = drawingArea.parent.GetComponent<FloatingEffect>();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInkEquipped += HandleInkEquipped;
            InventoryManager.Instance.OnPenEquipped += HandlePenEquipped;
            InventoryManager.Instance.OnScrollEquipped += HandleScrollEquipped;
        }

        if (manaSlider != null && PlayerDataManager.Instance != null)
        {
            manaSlider.SetValue(PlayerDataManager.Instance.currentMana, PlayerDataManager.Instance.GetMaxMana());
            PlayerDataManager.Instance.OnManaChanged += HandleManaChanged;
        }



        penController.OnResourceConsumed += HandleResourceConsumed;
    }

    private void HandleManaChanged(float currentMana, float maxMana)
    {
        if (manaSlider != null) manaSlider.SetValue(currentMana, maxMana);
    }



    private void HandleResourceConsumed(PlayerDataManager pMan)
    {
        if (pMan != null) pMan.NotifyManaChanged();
    }

    private void HandleInkEquipped(Item_Ink ink)
    {
        if (penController != null && penController.CurrentPen != null)
        {
            penController.GoToInkBottle(inkController.inkBottleVisual, null, () => {
                if (ink != null) 
                {
                    inkController.TryRefillPen(penController.CurrentPen);
                }
            }, true);
        }
    }

    private void HandlePenEquipped(Item_Pen pen)
    {
        if (penController != null && pen != null)
        {
            penController.GoToInkBottle(inkController.inkBottleVisual, null, () => {
                if (inkController.CurrentInk != null)
                {
                    inkController.TryRefillPen(pen);
                }
            }, instant: true);
        }
    }

    private void HandleScrollEquipped(Item_Scroll scroll)
    {
        // 스크롤이 장착 해제되었을 때
        if (scroll == null)
        {
            if (isDrawing) EndStroke();
            
            // 마우스가 도화지 밖으로 나간 것과 동일하게 처리 (펜을 잉크통으로 복귀)
            if (!isOutsideArea)
            {
                isOutsideArea = true;
                if (scrollFloatingEffect != null) scrollFloatingEffect.ResumeFloating();
                RefillPen();
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInkEquipped -= HandleInkEquipped;
            InventoryManager.Instance.OnPenEquipped -= HandlePenEquipped;
            InventoryManager.Instance.OnScrollEquipped -= HandleScrollEquipped;
        }

        if (penController != null)
        {
            penController.OnResourceConsumed -= HandleResourceConsumed;
        }

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnManaChanged -= HandleManaChanged;
        }
    }

    protected virtual void Update()
    {
        HandleDrawingInput();
    }

    protected virtual bool RequiresScrollToDraw()
    {
        return true;
    }

    protected virtual void HandleDrawingInput()
    {
        if (IsDrawingBlocked)
        {
            if (isDrawing) EndStroke();
            return;
        }

        if (RequiresScrollToDraw() && InventoryManager.Instance != null && InventoryManager.Instance.EquippedScroll == null)
        {
            if (isDrawing) EndStroke();
            return;
        }

        // 그리기 영역(UI) 제한 검사
        if (drawingArea != null)
        {
            Camera uiCamera = null;
            Canvas canvas = drawingArea.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = canvas.worldCamera != null ? canvas.worldCamera : mainCamera;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(drawingArea, Input.mousePosition, uiCamera))
            {
                if (isDrawing)
                    EndStroke(); // 영역을 벗어나면 획 강제 종료
                
                if (!isOutsideArea)
                {
                    isOutsideArea = true;
                    if (scrollFloatingEffect != null) scrollFloatingEffect.ResumeFloating();
                    RefillPen();
                }
                    
                return;
            }
            else
            {
                if (isOutsideArea)
                {
                    isOutsideArea = false;
                    if (scrollFloatingEffect != null) scrollFloatingEffect.PauseFloating();
                    ReturnPenToMouse();
                }
            }
        }

        if (penController != null && !isRefilling)
        {
            penController.TrackMouse(mainCamera);
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartStroke();
        }
        else if (Input.GetMouseButton(0))
        {
            if (!isDrawing && isPenReady)
            {
                StartStroke();
            }
            else
            {
                UpdateStroke();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndStroke();
        }
        MatchComboUseKey();
    }

    protected virtual void MatchComboUseKey()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MatchCombo();
        }
        // 개발자 도구: 현재 그려진 선을 'Star' 템플릿으로 저장 (엔터 키)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (currentDrawingData != null && currentDrawingData.strokes.Count > 0)
            {
                ShapeRecognizer.SaveTemplateAsset(currentDrawingData, "Star");
                Debug.Log("<color=magenta>[개발자 도구] 방금 그린 모양을 'Star' 커스텀 템플릿으로 저장했습니다!</color>");
                ClearDrawing();
            }
            else
            {
                Debug.LogWarning("저장할 도형이 없습니다. 먼저 별을 그려주세요.");
            }
        }
    }

    protected virtual void StartStroke()
    {
        if (!isPenReady || isRefilling || isOutsideArea) 
        {
            return; 
        }

        ItemInstance currentTool = penController != null ? penController.CurrentTool : null;
        
        // 스크롤이 필요한 경우에만 빈 스크롤 장착 여부 확인 (지팡이 등은 예외)
        if (RequiresScrollToDraw() && !(currentTool is Item_Wand))
        {
            Item_Scroll currentScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
            if (currentScroll == null || !currentScroll.isEmpty)
            {
                return;
            }
        }

        if (penController == null || !penController.CanDraw())
        {
            string warnMsg = currentTool != null && currentTool is Item_Pen pen && pen.PenData != null && pen.PenData.consumesMana 
                ? "[그리기 실패] 마력이 부족하여 마나 펜을 사용할 수 없습니다." 
                : "[그리기 실패] 그릴 도구를 선택하지 않았거나 잉크가 없습니다. (스크롤 밖으로 나가서 충전하세요)";
            Debug.LogWarning(warnMsg);
            return;
        }

        isDrawing = true;
        if (penController != null) penController.PlayDrawAnimation();
        
        GameObject lineObj = Instantiate(drawingDatabase.linePrefab, drawingArea);
        currentLine = lineObj.GetComponent<DrawingLine>();

        // 지팡이는 시안색, 펜은 장착된 잉크의 색상으로 선 색상 변경
        Color lineColor = Color.black;
        if (currentTool is Item_Wand)
        {
            lineColor = Color.cyan;
        }
        else if (inkController != null && penController != null)
        {
            lineColor = inkController.GetLineColor(currentTool as Item_Pen);
        }
        currentLine.SetColor(lineColor);

        UpdateStroke();
    }

    protected virtual void UpdateStroke()
    {
        if (!isDrawing || currentLine == null) return;

        Vector3 mousePos = Input.mousePosition;
        if (mainCamera == null) Debug.LogError("[UpdateStroke] mainCamera가 null입니다!");
        
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        // LineRenderer가 로컬 좌표계를 사용하므로 월드 좌표를 로컬 좌표로 변환합니다.
        Vector3 localPos = currentLine.transform.InverseTransformPoint(worldPos);
        Vector2 newPoint = new Vector2(localPos.x, localPos.y);

        if (currentLine.currentStroke == null) Debug.LogError("[UpdateStroke] currentStroke가 null입니다!");
        int prevCount = currentLine.currentStroke.points.Count;

        if (ShouldConsumeInk() && penController != null)
        {
            if (!penController.CanDraw())
            {
                EndStroke();
                ItemInstance currentTool = penController.CurrentTool;
                string warnMsg = currentTool != null && currentTool is Item_Pen pen && pen.PenData != null && pen.PenData.consumesMana
                    ? "[그리기] 플레이어 마력이 바닥나 그릴 수 없습니다!"
                    : "[그리기] 그릴 도구의 자원이 바닥났습니다! 스크롤 밖으로 나가 충전하세요.";
                Debug.LogWarning(warnMsg);
                return;
            }
            penController.ConsumeResource();
        }

        currentLine.AddPoint(newPoint);
    }


    protected virtual bool ShouldConsumeInk()
    {
        return true;
    }

    protected virtual bool CanDrawWithPen(Item_Pen pen)
    {
        if (pen == null) return false;
        if (pen.PenData != null && pen.PenData.consumesMana)
        {
            return PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentMana > 0f;
        }
        return pen.currentInkCapacity > 0f;
    }

    protected virtual void ConsumePenResource(Item_Pen pen)
    {
        if (pen == null) return;
        float rate = pen.PenData != null ? pen.PenData.inkConsumptionRate : 0f;
        if (pen.PenData != null && pen.PenData.consumesMana && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.currentMana -= rate * Time.deltaTime;
            if (PlayerDataManager.Instance.currentMana < 0) PlayerDataManager.Instance.currentMana = 0;
            PlayerDataManager.Instance.NotifyManaChanged();
        }
        else
        {
            pen.currentInkCapacity -= rate * Time.deltaTime;
            if (pen.currentInkCapacity < 0) pen.currentInkCapacity = 0;
        }
    }

    protected virtual async void EndStroke()
    {
        if (!isDrawing) return;
        isDrawing = false;
        if (penController != null) penController.PlayIdleAnimation(true); // 마우스 클릭을 뗄 때는 부드럽게 복귀
        
        if (currentLine != null)
        {
            if (currentLine.currentStroke.points.Count < 2)
            {
                Debug.LogWarning("[그리기 종료] 포인트가 2개 미만이라 선이 삭제됩니다.");
                Destroy(currentLine.gameObject);
            }
            else
            {
                currentDrawingData.AddStroke(currentLine.currentStroke);

                DrawingData singleStrokeData = new DrawingData();
                singleStrokeData.AddStroke(currentLine.currentStroke);
                
                Rect bounds = GetBounds(currentLine.currentStroke);
                Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
                int pointCount = currentLine.currentStroke.points.Count;

                // 백그라운드 스레드에서 인식 수행 전, 메인 스레드에서 필요한 에셋 로드 보장
                ShapeRecognizer.EnsureInitialized();

                // 유저 경험을 위해 무거운 도형 판별 작업을 백그라운드 스레드로 넘깁니다 (렉 방지)
                RecognizerResult result = await System.Threading.Tasks.Task.Run(() => ShapeRecognizer.Recognize(singleStrokeData));

                string shapeName = result.Name;
                float score = result.Score;
                
                drawnShapes.Add(new DrawnShape(shapeName, bounds, center, score));

                PrintCurrentStats();
            }
            currentLine = null;
        }
    }

    protected virtual void MatchCombo()
    {
        if (drawnShapes.Count == 0) return;

        if (drawingDatabase == null || drawingDatabase.recipes == null || drawingDatabase.recipes.Length == 0)
        {
            Debug.LogWarning("[Combo Matcher] 전역 드로잉 데이터베이스가 없거나 등록된 레시피 에셋이 없습니다.");
            return;
        }

        // 힌트가 있거나 이미 해금된 레시피만 매칭 대상으로 필터링 (Locked 상태는 아무리 잘 그려도 실패 처리)
        List<SpellRecipeAsset> matchableRecipes = new List<SpellRecipeAsset>();
        if (PlayerDataManager.Instance != null)
        {
            foreach (var recipe in drawingDatabase.recipes)
            {
                var state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
                if (state != RecipeUnlockState.Locked)
                {
                    matchableRecipes.Add(recipe);
                }
            }
        }
        else
        {
            matchableRecipes.AddRange(drawingDatabase.recipes);
        }

        var matchResult = ComboMatcher.Match(drawnShapes, matchableRecipes.ToArray());
        OnSpellMatched(matchResult.Item1, matchResult.Item2);
    }

    protected virtual void OnSpellMatched(string matchedSpell, float averageScore)
    {
        if (matchedSpell.Contains("실패") || matchedSpell.Contains("아무것도"))
        {
            Debug.Log($"<color=red>퍼석... {matchedSpell} (Score: {averageScore:F2})</color>");
        }
        else
        {
            Item_Scroll currentScroll = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedScroll : null;
            SpellRecipeAsset matchedRecipe = null;
            float spellManaCost = 30f;
            if (drawingDatabase != null && drawingDatabase.recipes != null)
            {
                foreach (var r in drawingDatabase.recipes)
                {
                    if (r.SpellName == matchedSpell)
                    {
                        matchedRecipe = r;
                        spellManaCost = r.manaCost;
                        break;
                    }
                }
            }

            if (currentScroll != null && currentScroll.isEmpty)
            {
                float drawCost = spellManaCost * 0.5f;

                if (UpgradeManager.Instance != null)
                {
                    float freeChance = UpgradeManager.Instance.GetTotalUpgradeValue(UpgradeType.FreeInkChance);
                    if (UnityEngine.Random.Range(0f, 100f) < freeChance)
                    {
                        Debug.Log($"<color=cyan>[Free Ink] 잉크 소모 없음 효과가 발동했습니다!</color>");
                        drawCost = 0f;
                    }
                }

                if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentMana >= drawCost)
                {
                    PlayerDataManager.Instance.currentMana -= drawCost;
                    PlayerDataManager.Instance.NotifyManaChanged();
                    
                    currentScroll.isEmpty = false;
                    currentScroll.spellName = matchedSpell;
                    currentScroll.accuracyScore = averageScore;
                    currentScroll.scrollElement = matchedRecipe != null ? matchedRecipe.Element : SpellElement.None;
                    currentScroll.spellIcon = matchedRecipe != null ? matchedRecipe.icon : null;
                    currentScroll.userDrawingData = currentDrawingData.Clone();

                    if (PlayerDataManager.Instance != null)
                    {
                        PlayerDataManager.Instance.UnlockRecipe(matchedSpell);
                    }

                    string rank = averageScore >= 0.85f ? "[대성공]" : "[성공]";
                    Debug.Log($"<color=cyan>✨ [마법 정착] {rank} {matchedSpell} (마나 소모: {drawCost:F1}) ✨</color>");
                    
                    if (InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.NotifyInventoryChanged();
                    }
                    
                    // 왁스 씰 연출 코루틴(비동기) 실행 - 1초 대기 후 장착 해제 및 초기화
                    StartCoroutine(WaxSealSequence(currentScroll));
                    return; // ClearDrawing()은 코루틴 안에서 수행
                }
                else
                {
                    Debug.LogWarning($"<color=red>[그리기 실패] 마나가 부족하여 스크롤에 마법을 새길 수 없습니다. (필요 마나: {drawCost:F1})</color>");
                }
            }
            else
            {
                float castCost = spellManaCost;
                if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentMana >= castCost)
                {
                    bool magicUsed = false;
                    
                    if (DungeonManager.Instance != null && DungeonManager.Instance.IsEventActive)
                    {
                        var popupUI = FindObjectOfType<EventPopupUI>();
                        if (popupUI != null && popupUI.TrySolveEvent(matchedSpell, matchedRecipe != null ? matchedRecipe.Element : SpellElement.None))
                        {
                            magicUsed = true;
                        }
                    }
                    else
                    {
                        if (DungeonManager.Instance != null)
                        {
                            magicUsed = DungeonManager.Instance.CastFieldSpell(matchedSpell, matchedRecipe != null ? matchedRecipe.Element : SpellElement.None);
                            if (magicUsed)
                            {
                                var windowController = FindObjectOfType<WindowController>();
                                if (windowController != null) windowController.ToggleInventoryAndDrawing();
                            }
                        }
                    }
                    
                    if (magicUsed)
                    {
                        PlayerDataManager.Instance.currentMana -= castCost;
                        PlayerDataManager.Instance.NotifyManaChanged();
                        string rank = averageScore >= 0.85f ? "[대성공]" : "[성공]";
                        Debug.Log($"<color=cyan>✨ [마법 발동] {rank} {matchedSpell} (마나 소모: {castCost:F1}) ✨</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=orange>[시전 취소] 대상이나 필드 효과가 없어 마법이 취소되었습니다. (마나 보존됨)</color>");
                    }
                }
                else
                {
                    Debug.LogWarning($"<color=red>[발동 실패] 마나가 부족하여 마법을 시전할 수 없습니다. (필요 마나: {castCost:F1})</color>");
                }
            }
        }
        ClearDrawing();
    }

    protected Rect GetBounds(Stroke stroke)
    {
        if (stroke.points.Count == 0) return new Rect();
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in stroke.points)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    public virtual void ClearDrawing()
    {
        currentDrawingData.Clear();
        drawnShapes.Clear();
        
        if (drawingArea != null)
        {
            foreach (Transform child in drawingArea)
            {
                if (child.GetComponent<DrawingLine>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<DrawingLine>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    protected virtual void PrintCurrentStats()
    {
        Debug.Log($"<color=yellow>[누적 도형] 현재 그려진 도형 수: {drawnShapes.Count}</color>");
    }

    protected virtual void RefillPen()
    {
        isPenReady = false; 
        
        Item_Pen pen = penController != null ? penController.CurrentPen : null;


        isRefilling = true;

        if (penController != null && inkController != null)
        {
            penController.GoToInkBottle(inkController.inkBottleVisual, 
                () => { inkController.TryRefillPen(penController.CurrentPen); }, 
                () => { isPenReady = true; }
            );
        }
        else
        {
            if (inkController != null && penController != null) inkController.TryRefillPen(penController.CurrentPen);
            isPenReady = true;
            isRefilling = false;
        }
    }

    protected virtual void ReturnPenToMouse()
    {
        isRefilling = true;
        isPenReady = false;

        if (penController != null)
        {
            penController.ReturnToMouse(mainCamera, () => {
                isRefilling = false;
                isPenReady = true;
            });
        }
        else
        {
            isRefilling = false;
            isPenReady = true;
        }
    }

    protected virtual void OnGUI()
    {
        if (drawnShapes == null || drawnShapes.Count == 0) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.alignment = TextAnchor.MiddleLeft;
        style.richText = true; // 리치 텍스트 활성화

        float startX = 20f;
        float startY = Screen.height * 0.4f;

        // 가독성을 위해 배경 박스 하나 깔아주기
        GUI.Box(new Rect(startX - 10, startY - 10, 300, 40 + (drawnShapes.Count * 30)), "");

        GUI.Label(new Rect(startX, startY, 200, 30), "<color=white>--- Drawn Shapes ---</color>", style);
        
        int displayIndex = 0;
        foreach (var shape in drawnShapes)
        {
            string displayName = shape.Name;
            string scoreText = $" : {shape.Accuracy:F2}";
            string colorTag = "<color=red>";

            // 이름이 Unknown이거나 정확도가 0.5 미만이면 실패로 간주
            bool isFailed = shape.Name == "Unknown" || shape.Name == "None" || shape.Accuracy < 0.5f;

            if (isFailed)
            {
                displayName = "판정 실패";
                scoreText = ""; // 실패 시 점수는 숨기거나 표시 안함 (어떤 도형으로 판정 실패했는지 숨김)
                colorTag = "<color=gray>";
            }
            else if (shape.Accuracy >= 0.85f)
            {
                colorTag = "<color=yellow>"; // 대성공
            }
            else if (shape.Accuracy >= 0.5f)
            {
                colorTag = "<color=green>"; // 성공
            }

            GUI.Label(new Rect(startX, startY + 30 + (displayIndex * 30), 300, 30), 
                $"{colorTag}[{displayIndex + 1}] {displayName}{scoreText}</color>", style);
            
            displayIndex++;
        }
    }

    protected virtual System.Collections.IEnumerator WaxSealSequence(Item_Scroll currentScroll)
    {
        IsDrawingBlocked = true;
        Debug.Log("<color=orange>쾅! [왁스 씰 연출 중...] (1초 대기)</color>");
        
        // TODO: UI에 왁스 씰 도장이 찍히는 연출(이펙트) 추가 가능 공간
        
        yield return new WaitForSeconds(1.0f);
        
        IsDrawingBlocked = false;
        
        if (InventoryManager.Instance != null && InventoryManager.Instance.EquippedScroll == currentScroll)
        {
            Debug.Log($"<color=white>[수거 완료] {currentScroll.spellName} 스크롤이 인벤토리로 수거되었습니다.</color>");
            InventoryManager.Instance.EquippedScroll = null;
        }
        
        ClearDrawing();
    }
}

