using UnityEngine;
using UnityEngine.UI;

public class EventDrawingManager : DrawingManager
{
    [Header("Event Drawing Specifics")]
    public Button doneButton;
    public Button showContentButton; // Cancel / Close button

    // Optional event for when the player hits 'Done' and a spell is matched
    public event System.Action<SpellRecipeAsset, float> OnEventSpellMatched;
    public event System.Action OnCancelDrawing;

    protected override void Start()
    {
        base.Start();

        if (doneButton != null)
        {
            doneButton.onClick.AddListener(() =>
            {
                MatchCombo();
            });
        }

        if (showContentButton != null)
        {
            showContentButton.onClick.AddListener(() =>
            {
                OnCancelDrawing?.Invoke();
            });
        }
    }

    protected override bool RequiresScrollToDraw()
    {
        // 이벤트용 그리기에서는 스크롤이 필요 없음
        return false;
    }

    protected override bool ShouldConsumeInk()
    {
        // 이벤트 그리기에서는 잉크를 소모하지 않음 (기획에 따라 변경 가능)
        return false;
    }

    protected override void MatchComboUseKey()
    {
        // 이벤트용 그리기에서는 스페이스바로 판정을 시작하지 않음 (완료 버튼 사용)
    }

    protected override void OnSpellMatched(string matchedSpell, float averageScore)
    {
        // 원래 DrawingManager의 매칭 로직을 가로챔
        if (matchedSpell.Contains("실패") || matchedSpell.Contains("아무것도"))
        {
            Debug.Log($"<color=red>이벤트 마법 그리기 실패: {matchedSpell}</color>");
            OnEventSpellMatched?.Invoke(null, averageScore);
        }
        else
        {
            SpellRecipeAsset matchedRecipe = null;
            if (drawingDatabase != null && drawingDatabase.recipes != null)
            {
                foreach (var r in drawingDatabase.recipes)
                {
                    if (r.SpellName == matchedSpell)
                    {
                        matchedRecipe = r;
                        break;
                    }
                }
            }
            
            OnEventSpellMatched?.Invoke(matchedRecipe, averageScore);
        }

        ClearDrawing();
    }

    // 마법 조합은 doneButton을 통해서만 호출하도록 기존 스페이스바 입력 무시 가능 (원한다면)
    protected override void HandleDrawingInput()
    {
        // 부모의 Update/HandleDrawingInput이 스페이스바를 누를 때 MatchCombo를 호출하므로, 
        // 키보드 입력을 막고 버튼으로만 제어하고 싶다면 여기에 커스텀 로직을 넣거나 base를 호출하되,
        // 현재는 base를 유지합니다.
        base.HandleDrawingInput();
    }
}
