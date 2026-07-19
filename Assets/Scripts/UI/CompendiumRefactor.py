import re

with open('/Users/admin/develop/Magic/Assets/Scripts/UI/Compendium/RecipeCompendiumUIController.cs', 'r') as f:
    content = f.read()

# Replace class declaration
content = re.sub(r'public class RecipeCompendiumUIController : MonoBehaviour', 'public class RecipeCompendiumUIController : Magic.UI.PagedUIController<RecipeEntryUI>', content)

# Remove old containers
content = re.sub(r'\[Header\("Containers"\)\]\s*public RectTransform listContainer;\s*public GameObject entryPrefab;', '', content)

# Remove list variables
content = re.sub(r'private List<RecipeEntryUI> spawnedEntries = new List<RecipeEntryUI>\(\);\s*', '', content)

# Modify OnEnable
content = content.replace("private void OnEnable()", "protected override void OnEnable()")

# Modify RefreshList definition
refresh_list_old = """        public void RefreshList()
        {
            // Clear existing
            foreach (var entry in spawnedEntries)
            {
                if (entry != null) Destroy(entry.gameObject);
            }
            spawnedEntries.Clear();

            var database = DrawingDatabase.Instance;
            if (database == null || database.recipes == null) return;

            foreach (var recipe in database.recipes)
            {
                RecipeUnlockState state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
                
                // 완전히 숨김 조건 (A안 반영)
                if (state == RecipeUnlockState.Locked) continue;

                if (entryPrefab != null && listContainer != null)
                {
                    GameObject go = Instantiate(entryPrefab, listContainer);
                    RecipeEntryUI entryUI = go.GetComponent<RecipeEntryUI>();
                    if (entryUI != null)
                    {
                        entryUI.Setup(recipe, state, this);
                        spawnedEntries.Add(entryUI);
                    }
                }
            }
        }"""

# Need a list of unlocked/hinted recipes to know the capacity and to get by index
refresh_list_new = """        private List<SpellRecipeAsset> _visibleRecipes = new List<SpellRecipeAsset>();

        public override void RefreshList()
        {
            var database = DrawingDatabase.Instance;
            if (database == null || database.recipes == null) return;

            _visibleRecipes.Clear();
            foreach (var recipe in database.recipes)
            {
                RecipeUnlockState state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
                if (state != RecipeUnlockState.Locked)
                {
                    _visibleRecipes.Add(recipe);
                }
            }

            base.RefreshList();
        }

        protected override int GetTotalCapacity()
        {
            return _visibleRecipes.Count;
        }

        protected override void UpdateSlot(RecipeEntryUI slot, int dataIndex)
        {
            if (dataIndex < _visibleRecipes.Count)
            {
                var recipe = _visibleRecipes[dataIndex];
                RecipeUnlockState state = PlayerDataManager.Instance.GetRecipeState(recipe.SpellName);
                slot.Setup(recipe, state, this);
            }
        }"""
content = content.replace(refresh_list_old, refresh_list_new)

with open('/Users/admin/develop/Magic/Assets/Scripts/UI/Compendium/RecipeCompendiumUIController.cs', 'w') as f:
    f.write(content)
