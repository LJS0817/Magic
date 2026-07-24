using UnityEngine;
using UnityEditor;

namespace Magic.Editor
{
    public class ItemDatabaseGenerator
    {
        private struct PenDef
        {
            public string name; public float capacity; public float consumptionRate; public string grade; public string description; public bool consumesMana;
            public PenDef(string n, float c, float r, string g, string d, bool m = false) { name = n; capacity = c; consumptionRate = r; grade = g; description = d; consumesMana = m; }
        }

        private struct InkDef
        {
            public string name; public float maxAmount; public string quality; public string description; public Color color;
            public InkDef(string n, float m, string q, string d, Color c) { name = n; maxAmount = m; quality = q; description = d; color = c; }
        }

        private struct ScrollDef
        {
            public string name; public int maxDurability; public float accuracyScore; public string grade; public string description;
            public ScrollDef(string n, int md, float a, string g, string d) { name = n; maxDurability = md; accuracyScore = a; grade = g; description = d; }
        }

        private struct WandDef
        {
            public string name; public float manaCostMultiplier; public string grade; public string description;
            public WandDef(string n, float m, string g, string d) { name = n; manaCostMultiplier = m; grade = g; description = d; }
        }

        private struct PotionDef
        {
            public string name; public PotionType type; public PotionGrade grade; public float recoveryAmount; public string rarity; public string description;
            public PotionDef(string n, PotionType t, PotionGrade g, float r, string ra, string d) { name = n; type = t; grade = g; recoveryAmount = r; rarity = ra; description = d; }
        }

        private struct PouchDef
        {
            public string name; public int capacityBonus; public string grade; public string description;
            public PouchDef(string n, int cb, string g, string d) { name = n; capacityBonus = cb; grade = g; description = d; }
        }

        private static readonly PenDef[] PenDefs = new PenDef[]
        {
            // 일반 등급 5개
            new PenDef("낡은 깃털 펜", 30f, 5.0f, "일반", "깃촉이 닳아 마법 잉크가 고르게 나오지 않는 연습용 낡은 깃펜입니다."),
            new PenDef("연습용 나무 펜", 40f, 4.8f, "일반", "마도학교에서 훈련을 시작할 때 지급되는 가장 보편적이고 견고한 나무 펜입니다."),
            new PenDef("금이 간 유리 펜", 45f, 4.2f, "일반", "몸체에 미세한 금이 가 있어 잉크 효율이 떨어지나, 여전히 작동 가능한 유리 펜입니다."),
            new PenDef("마나 트레이너 펜", 0f, 8.0f, "일반", "잉크통 없이 플레이어의 마력을 직접 소모하여 동작하는 연습용 마나 펜입니다.", true),

            // 고급 등급 5개
            new PenDef("마법 깃털 펜", 60f, 3.5f, "고급", "푸른 그리폰의 깃털에 소량의 마력을 주입하여 필기감을 살린 중급 깃펜입니다."),
            new PenDef("세련된 강철 펜", 70f, 3.3f, "고급", "고급 연마 처리가 적용된 강철로, 잉크 막힘이 현저히 적고 안정적인 선 표현이 가능합니다."),
            new PenDef("응축된 유리 펜", 65f, 3.2f, "고급", "특수한 열처리로 유리의 밀도를 높여 마력이 고르게 스며들도록 만든 고급 펜입니다."),
            new PenDef("구리 룬 펜", 75f, 3.0f, "고급", "구리 표면에 활성화 룬이 새겨져 있어 마력 드로잉 시 소모를 억제합니다."),
            new PenDef("룬 마나 스타일러스", 0f, 6.0f, "고급", "잉크 대신 술사의 가벼운 마력을 감지하여 보라색 마력선을 그리는 보급형 마나 펜입니다.", true),

            // 희귀 등급 5개
            new PenDef("은빛 루비 펜", 90f, 2.5f, "희귀", "순은으로 감싼 바디에 루비 보석을 박아 마력을 집중시키는 상급 펜입니다."),
            new PenDef("마도학자의 만년필", 100f, 2.3f, "희귀", "정교하게 조립되어 잉크의 흐름을 균일하게 조율하고, 마법 완성도를 높여줍니다."),
            new PenDef("고대 나무 펜", 110f, 2.2f, "희귀", "천 년 묵은 엘프의 나무 가지를 깎아 제작하여 자연 마력과의 결합도가 훌륭합니다."),
            new PenDef("서리 깃털 펜", 115f, 2.0f, "희귀", "북방의 얼음 정령 깃털로 장식되어 차갑고 청명한 잉크 흐름을 자아냅니다."),
            new PenDef("비전 마력 필기구", 0f, 4.0f, "희귀", "잉크 대신 술사의 마나를 즉시 주입받아 비전 마력선을 직조하는 고급 마나 펜입니다.", true),

            // 영웅 등급 5개
            new PenDef("황금 용 비늘 펜", 150f, 1.5f, "영웅", "고대 드래곤의 심장 근처 비늘을 깎아 가공하여 강력한 화염 속성과 잉크 보존력을 지닙니다."),
            new PenDef("대마법사의 수정 펜", 170f, 1.3f, "영웅", "역사상 가장 위대했던 대마법사가 사용했다고 전해지는 투명하고 단단한 수정 펜입니다."),
            new PenDef("영혼 인도자 펜", 160f, 1.2f, "영웅", "망자의 혼령을 정화해 잉크의 흐름으로 환산하여, 영혼의 기운을 담아 그립니다."),
            new PenDef("성스러운 백은 펜", 180f, 1.1f, "영웅", "여신전의 축복을 받은 최고급 백금으로 만들어져 사악한 마력의 흔적을 정화합니다."),
            new PenDef("별무리 인도기", 0f, 2.5f, "영웅", "별무리의 흐름을 형상화하며 플레이어의 마력을 우아하게 변환해 드로잉하는 전설 등급의 마나 펜입니다.", true),

            // 전설 등급 5개
            new PenDef("창조주의 깃털", 250f, 0.8f, "전설", "태초의 신들이 세계를 지도로 그릴 때 썼던 영원의 신성을 품은 창조자의 깃펜입니다."),
            new PenDef("차원의 별빛 펜", 280f, 0.7f, "전설", "차원의 틈새에서 채취한 우주의 별빛 마나가 실시간으로 보충되는 초월적인 펜입니다."),
            new PenDef("아틀란티스의 신화 펜", 260f, 0.6f, "전설", "물에 잠긴 초고대 도시 아틀란티스의 바다 신들이 쓰던 영원무궁한 유물 펜입니다."),
            new PenDef("불멸의 태양 펜", 300f, 0.5f, "전설", "태양의 흑점에서 축출한 영겁의 열기가 불순한 잉크 소모를 완벽에 가깝게 차단해 줍니다."),
            new PenDef("영원의 크로노스 펜", 0f, 1.0f, "전설", "시공간의 흐름을 지연시키는 금속으로 이루어져 잉크 소모 없이 극소량의 마력만 소모하는 영원의 신화 펜입니다.", true),
        };

        private static readonly InkDef[] InkDefs = new InkDef[]
        {
            new InkDef("싸구려 흑색 잉크", 50f, "하급", "물에 많이 희석되어 색이 흐린 저품질 잉크입니다.", Color.gray),
            new InkDef("표준 마법 잉크", 100f, "일반", "마도학교에서 표준으로 사용하는 무난한 성능의 잉크입니다.", Color.black),
            new InkDef("농축된 마력 잉크", 150f, "고급", "마력이 짙게 배어있어 선명하고 뚜렷한 마법진을 그릴 수 있습니다.", new Color(0.2f, 0.2f, 0.8f)),
            new InkDef("요정의 눈물 잉크", 200f, "희귀", "요정 숲의 샘물과 섞어 만든 잉크로, 소모율이 줄어드는 효과가 있습니다.", new Color(0.2f, 0.8f, 0.4f)),
            new InkDef("심연의 검은 잉크", 300f, "전설", "절대적인 어둠의 농도를 지녀, 파괴적인 마법 위력을 발휘할 수 있습니다.", new Color(0.1f, 0f, 0.1f))
        };

        private static readonly ScrollDef[] ScrollDefs = new ScrollDef[]
        {
            new ScrollDef("해진 연습용 양피지", 2, 0.8f, "하급", "끝이 해지고 낡은 양피지입니다. 마법을 두세 번 쓰면 찢어질 것 같습니다."),
            new ScrollDef("표준 마법 스크롤", 5, 1.0f, "일반", "규격에 맞춰 제작된 흔한 마법 스크롤입니다."),
            new ScrollDef("단단한 가죽 스크롤", 10, 1.0f, "고급", "특수 처리된 가죽으로 만들어져 내구도가 매우 뛰어납니다."),
            new ScrollDef("은실 직조 스크롤", 8, 1.2f, "희귀", "은실이 섞여 짜여 있어 마법진이 더 정교하게 그려집니다. (마법 완성도 판정 보너스)"),
            new ScrollDef("상급 비전 스크롤", 15, 1.3f, "영웅", "불순물을 덜어낸 고급 양피지를 사용하여 복잡하고 예민한 고위 마법식도 무리 없이 기록할 수 있습니다."),
            new ScrollDef("영원의 두루마리", 50, 1.5f, "전설", "절대 찢어지지 않을 것 같은 신비한 소재의 두루마리입니다.")
        };

        private static readonly WandDef[] WandDefs = new WandDef[]
        {
            new WandDef("갈라진 재물나무 지팡이", 1.2f, "하급", "손잡이가 닳고 나무가 갈라져 마력 전도율이 다소 떨어지는 견습용 지팡이입니다."),
            new WandDef("단풍나무 학도 지팡이", 1.0f, "일반", "가볍고 견고한 단풍나무로 제작되어 마도학교 학도들이 주로 사용하는 표준 지팡이입니다."),
            new WandDef("은도금 호두나무 지팡이", 0.9f, "고급", "단단한 호두나무 표면에 은선을 입혀 마나의 방출을 한결 매끄럽게 다듬은 고급 지팡이입니다."),
            new WandDef("창공의 비룡 뼈 지팡이", 0.8f, "희귀", "바람을 가르는 비룡의 뼈를 가공해 만들어, 속성 마법의 위력을 크게 증폭시켜 줍니다."),
            new WandDef("심연의 흑단목 지팡이", 0.6f, "영웅", "빛조차 스며들지 않는 칠흑 같은 흑단목으로 벼려내어, 막대한 마력을 폭발적으로 쏟아낼 수 있습니다."),
            new WandDef("초월자의 이그드라실 가지", 0.4f, "전설", "신화 속 세계수에서 떨어져 나온 신성한 가지로, 마력을 주입하는 즉시 기적을 실체화하는 전설의 지팡이입니다.")
        };

        private static readonly PotionDef[] PotionDefs = new PotionDef[]
        {
            new PotionDef("하급 체력 물약", PotionType.Health, PotionGrade.Lesser, 25f, "일반", "작은 상처를 치료할 수 있는 붉은색 물약입니다."),
            new PotionDef("중급 체력 물약", PotionType.Health, PotionGrade.Medium, 50f, "고급", "보통의 상처를 순식간에 아물게 하는 물약입니다."),
            new PotionDef("상급 체력 물약", PotionType.Health, PotionGrade.Greater, 100f, "희귀", "치명적인 부상도 빠르게 회복시켜 주는 고농축 물약입니다."),
            new PotionDef("하급 마나 물약", PotionType.Mana, PotionGrade.Lesser, 25f, "일반", "소모된 마나를 조금 보충해 주는 푸른색 물약입니다."),
            new PotionDef("중급 마나 물약", PotionType.Mana, PotionGrade.Medium, 50f, "고급", "마나를 꽤 많이 회복시켜 주어 전투 중 유용합니다."),
            new PotionDef("상급 마나 물약", PotionType.Mana, PotionGrade.Greater, 100f, "희귀", "순수한 마력의 정수가 담겨 있어 마나를 대량으로 회복합니다.")
        };

        private static readonly PouchDef[] PouchDefs = new PouchDef[]
        {
            new PouchDef("조잡한 마법 주머니", 1, "하급", "공간 마법이 아주 조금 적용되어 있는 작은 주머니입니다."),
            new PouchDef("가죽 마법 주머니", 1, "일반", "일반적인 여행자들이 자주 쓰는 마법 주머니입니다."),
            new PouchDef("견고한 공간 주머니", 3, "고급", "내부가 제법 넓은 고급 마법 주머니입니다."),
            new PouchDef("은빛 실 주머니", 3, "희귀", "은빛 실로 짜여 있어 많은 물건을 무리 없이 담을 수 있습니다."),
            new PouchDef("아공간 배낭", 5, "영웅", "별도의 아공간과 연결되어 무수한 스크롤을 꺼낼 수 있는 뛰어난 가방입니다."),
            new PouchDef("차원 왜곡의 주머니", 8, "전설", "내부에 작은 우주가 담겨 있는 전설적인 차원 주머니입니다.")
        };

        [MenuItem("Magic/Tools/Generate Item Database")]
        public static void GenerateDatabase()
        {
            string rootPath = "Assets/Resources/Items";
            CreateFolderIfNotExists("Assets/Resources", "Items");
            CreateFolderIfNotExists(rootPath, "Pens");
            CreateFolderIfNotExists(rootPath, "Inks");
            CreateFolderIfNotExists(rootPath, "Scrolls");
            CreateFolderIfNotExists(rootPath, "Wands");
            CreateFolderIfNotExists(rootPath, "Potions");
            CreateFolderIfNotExists(rootPath, "Pouches");

            string dbPath = rootPath + "/ItemDatabase.asset";
            ItemDatabase db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(dbPath);
            bool isNewDb = false;
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ItemDatabase>();
                isNewDb = true;
            }

            db.pens.Clear();
            db.inks.Clear();
            db.scrolls.Clear();
            db.wands.Clear();
            db.potions.Clear();
            db.pouches.Clear();

            // Generate Pens
            foreach (var def in PenDefs)
            {
                string assetPath = $"{rootPath}/Pens/Pen_{def.name.Replace(" ", "_")}.asset";
                ItemPenSO asset = GetOrCreateAsset<ItemPenSO>(assetPath);
                
                asset.itemName = def.name;
                asset.maxInkCapacity = def.capacity;
                asset.inkConsumptionRate = def.consumptionRate;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;
                asset.consumesMana = def.consumesMana;
                
                EditorUtility.SetDirty(asset);
                db.pens.Add(asset);
            }

            // Generate Inks
            foreach (var def in InkDefs)
            {
                string assetPath = $"{rootPath}/Inks/Ink_{def.name.Replace(" ", "_")}.asset";
                ItemInkSO asset = GetOrCreateAsset<ItemInkSO>(assetPath);
                
                asset.itemName = def.name;
                asset.maxAmount = def.maxAmount;
                asset.rarity = GetRarityFromString(def.quality);
                asset.itemDescription = def.description;
                asset.inkColor = def.color;
                
                EditorUtility.SetDirty(asset);
                db.inks.Add(asset);
            }

            // Generate Scrolls
            foreach (var def in ScrollDefs)
            {
                string assetPath = $"{rootPath}/Scrolls/Scroll_{def.name.Replace(" ", "_")}.asset";
                ItemScrollSO asset = GetOrCreateAsset<ItemScrollSO>(assetPath);
                
                asset.itemName = def.name;
                asset.maxDurability = def.maxDurability;
                asset.accuracyScore = def.accuracyScore;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;
                
                EditorUtility.SetDirty(asset);
                db.scrolls.Add(asset);
            }

            // Generate Wands
            foreach (var def in WandDefs)
            {
                string assetPath = $"{rootPath}/Wands/Wand_{def.name.Replace(" ", "_")}.asset";
                ItemWandSO asset = GetOrCreateAsset<ItemWandSO>(assetPath);
                
                asset.itemName = def.name;
                asset.defaultManaCostMultiplier = def.manaCostMultiplier;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;
                
                EditorUtility.SetDirty(asset);
                db.wands.Add(asset);
            }

            // Generate Potions
            foreach (var def in PotionDefs)
            {
                string assetPath = $"{rootPath}/Potions/Potion_{def.name.Replace(" ", "_")}.asset";
                ItemPotionSO asset = GetOrCreateAsset<ItemPotionSO>(assetPath);
                
                asset.itemName = def.name;
                asset.potionType = def.type;
                asset.potionGrade = def.grade;
                asset.recoveryAmount = def.recoveryAmount;
                asset.rarity = GetRarityFromString(def.rarity);
                asset.itemDescription = def.description;
                
                EditorUtility.SetDirty(asset);
                db.potions.Add(asset);
            }

            // Generate Pouches
            foreach (var def in PouchDefs)
            {
                string assetPath = $"{rootPath}/Pouches/Pouch_{def.name.Replace(" ", "_")}.asset";
                ItemPouchSO asset = GetOrCreateAsset<ItemPouchSO>(assetPath);
                
                asset.itemName = def.name;
                asset.loadoutCapacityBonus = def.capacityBonus;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;
                
                EditorUtility.SetDirty(asset);
                db.pouches.Add(asset);
            }

            if (isNewDb)
            {
                AssetDatabase.CreateAsset(db, dbPath);
            }
            else
            {
                EditorUtility.SetDirty(db);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=lime>🎉 ItemDatabase 생성이 완료되었습니다!</color>\n- 펜: {db.pens.Count}종\n- 잉크: {db.inks.Count}종\n- 스크롤: {db.scrolls.Count}종\n- 지팡이: {db.wands.Count}종\n- 물약: {db.potions.Count}종\n- 주머니: {db.pouches.Count}종\nDB 위치: {dbPath}");
        }

        private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static void CreateFolderIfNotExists(string parentPath, string folderName)
        {
            string fullPath = parentPath + "/" + folderName;
            if (!AssetDatabase.IsValidFolder(fullPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static ItemRarity GetRarityFromString(string grade)
        {
            if (grade == "전설") return ItemRarity.Legendary;
            if (grade == "영웅") return ItemRarity.Epic;
            if (grade == "희귀") return ItemRarity.Rare;
            if (grade == "고급") return ItemRarity.Uncommon;
            if (grade == "일반") return ItemRarity.Common;
            if (grade == "하급") return ItemRarity.Common;
            return ItemRarity.Common;
        }
    }
}
