using UnityEngine;

namespace Magic.Drawing
{
    public class RecipeDeveloper : DrawingManager
    {
        [Header("Template Generator")]
        [Tooltip("체크하면 획을 하나 그릴 때마다 해당 획을 템플릿으로 자동 저장합니다.")]
        public bool autoSaveTemplateOnRelease = false;
        [Tooltip("템플릿 저장에 사용될 이름입니다.")]
        public string shapeNameToSave = "MyNewShape";

        [Header("Recipe Generator (Auto Create)")]
        [Tooltip("켜두면 스페이스바를 누를 때 마법을 쏘는 대신, 방금 그린 마법진을 분석해 새로운 레시피 에셋으로 자동 저장합니다.")]
        public bool recordRecipeMode = false;
        [Tooltip("자동 생성될 레시피의 이름입니다.")]
        public string newRecipeName = "MyAwesomeSpell";

        protected override void MatchCombo()
        {
            // 개발 모드가 켜져있다면, 마법 발동 대신 레시피를 저장합니다.
            if (recordRecipeMode)
            {
                SaveRecipe();
            }
            else
            {
                base.MatchCombo();
            }
        }

        protected override void EndStroke()
        {
            base.EndStroke();

            // 획을 그리고 마우스를 뗐을 때 자동 저장
            if (autoSaveTemplateOnRelease)
            {
                SaveTemplate();
            }
        }

        protected override bool ShouldConsumeInk()
        {
            return !autoSaveTemplateOnRelease;
        }

        private void SaveTemplate()
        {
            if (currentDrawingData == null || currentDrawingData.strokes.Count == 0) return;

            // 방금 그린 마지막 획만 저장
            var strokes = currentDrawingData.strokes;
            var lastStroke = strokes[strokes.Count - 1];

            DrawingData singleStrokeData = new DrawingData();
            singleStrokeData.AddStroke(lastStroke);

            ShapeRecognizer.SaveTemplateAsset(singleStrokeData, shapeNameToSave);
            
            // 템플릿 양산 시 화면이 지저분해지지 않도록 지워주기
            ClearDrawing();
        }

        private void SaveRecipe()
        {
            if (drawnShapes.Count == 0) return;

            Debug.Log($"<color=orange>[Recipe Generator] '{newRecipeName}' 레시피 자동 생성을 시작합니다...</color>");
            RecipeGenerator.GenerateAndSaveRecipe(drawnShapes, newRecipeName);
            
            // 사용한 빈 스크롤을 완성된 스크롤로 상태 변환 (내구도 차감 없음)
            if (selectedScrollIndex != -1 && selectedScrollIndex < inventory.Count)
            {
                Magic.Inventory.Item_Scroll currentScroll = inventory[selectedScrollIndex] as Magic.Inventory.Item_Scroll;
                if (currentScroll != null && currentScroll.isEmpty)
                {
                    currentScroll.isEmpty = false;
                    if (currentScroll.ScrollData != null) currentScroll.ScrollData.spellName = newRecipeName;
                    Debug.Log($"[그리기] 빈 스크롤이 '{newRecipeName}' 마법 스크롤로 영구히 변환되었습니다!");
                }
            }

            ClearDrawing();
        }
    }
}
