using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Magic.Inventory;

namespace Magic.Editor
{
    public class PenDatabaseGenerator
    {
        [MenuItem("Magic/Generate Pen Database")]
        public static void GeneratePenDatabase()
        {
            string folderPath = "Assets/Resources";
            string assetPath = "Assets/Resources/PenDatabase.asset";

            // Resources 폴더가 없으면 생성
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            PenDatabase database = AssetDatabase.LoadAssetAtPath<PenDatabase>(assetPath);
            bool isNew = false;

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<PenDatabase>();
                isNew = true;
            }

            database.pens.Clear();

            // 1. 일반 등급 5개 (용량: 30 ~ 50, 소모율: 4.0 ~ 5.0)
            AddPen(database, "낡은 깃털 펜", 30f, 5.0f, "일반", "깃촉이 닳아 마법 잉크가 고르게 나오지 않는 연습용 낡은 깃펜입니다.");
            AddPen(database, "연습용 나무 펜", 40f, 4.8f, "일반", "마도학교에서 훈련을 시작할 때 지급되는 가장 보편적이고 견고한 나무 펜입니다.");
            AddPen(database, "훈련용 연필", 35f, 4.5f, "일반", "스케치 수준의 드로잉을 연습하기에 용이하게 제작된 초보용 탄소 연필입니다.");
            AddPen(database, "금이 간 유리 펜", 45f, 4.2f, "일반", "몸체에 미세한 금이 가 있어 잉크 효율이 떨어지나, 여전히 작동 가능한 유리 펜입니다.");
            AddPen(database, "일반 청동 펜", 50f, 4.0f, "일반", "구리와 아연을 합금하여 무게감이 있고 내구성이 괜찮은 일반 청동 펜입니다.");

            // 2. 고급 등급 5개 (용량: 60 ~ 80, 소모율: 2.8 ~ 3.5)
            AddPen(database, "마법 깃털 펜", 60f, 3.5f, "고급", "푸른 그리폰의 깃털에 소량의 마력을 주입하여 필기감을 살린 중급 깃펜입니다.");
            AddPen(database, "세련된 강철 펜", 70f, 3.3f, "고급", "고급 연마 처리가 적용된 강철로, 잉크 막힘이 현저히 적고 안정적인 선 표현이 가능합니다.");
            AddPen(database, "응축된 유리 펜", 65f, 3.2f, "고급", "특수한 열처리로 유리의 밀도를 높여 마력이 고르게 스며들도록 만든 고급 펜입니다.");
            AddPen(database, "구리 룬 펜", 75f, 3.0f, "고급", "구리 표면에 활성화 룬이 새겨져 있어 마력 드로잉 시 소모를 억제합니다.");
            AddPen(database, "학자의 잉크 펜", 80f, 2.8f, "고급", "대마도 연구회 학자들이 밤새 연구할 때 쓰던 잉크 저장 공간이 향상된 펜입니다.");

            // 3. 희귀 등급 5개 (용량: 90 ~ 120, 소모율: 1.8 ~ 2.5)
            AddPen(database, "은빛 루비 펜", 90f, 2.5f, "희귀", "순은으로 감싼 바디에 루비 보석을 박아 마력을 집중시키는 상급 펜입니다.");
            AddPen(database, "마도학자의 만년필", 100f, 2.3f, "희귀", "정교하게 조립되어 잉크의 흐름을 균일하게 조율하고, 마법 완성도를 높여줍니다.");
            AddPen(database, "고대 나무 펜", 110f, 2.2f, "희귀", "천 년 묵은 엘프의 나무 가지를 깎아 제작하여 자연 마력과의 결합도가 훌륭합니다.");
            AddPen(database, "서리 깃털 펜", 115f, 2.0f, "희귀", "북방의 얼음 정령 깃털로 장식되어 차갑고 청명한 잉크 흐름을 자아냅니다.");
            AddPen(database, "비전 마력 필기구", 120f, 1.8f, "희귀", "기계공학적 원리로 드로잉 마법의 속도를 급격히 높여주는 특수 장치입니다.");

            // 4. 전설 등급 5개 (용량: 150 ~ 200, 소모율: 1.0 ~ 1.5)
            AddPen(database, "황금 용 비늘 펜", 150f, 1.5f, "전설", "고대 드래곤의 심장 근처 비늘을 깎아 가공하여 강력한 화염 속성과 잉크 보존력을 지닙니다.");
            AddPen(database, "대마법사의 수정 펜", 170f, 1.3f, "전설", "역사상 가장 위대했던 대마법사가 사용했다고 전해지는 투명하고 단단한 수정 펜입니다.");
            AddPen(database, "영혼 인도자 펜", 160f, 1.2f, "전설", "망자의 혼령을 정화해 잉크의 흐름으로 환산하여, 영혼의 기운을 담아 그립니다.");
            AddPen(database, "성스러운 백은 펜", 180f, 1.1f, "전설", "여신전의 축복을 받은 최고급 백금으로 만들어져 사악한 마력의 흔적을 정화합니다.");
            AddPen(database, "심연의 흑요석 펜", 200f, 1.0f, "전설", "심해 깊은 곳의 흑요석을 원형 그대로 깎아 잉크의 소모를 극적으로 차단합니다.");

            // 5. 신화 등급 5개 (용량: 250 ~ 350, 소모율: 0.4 ~ 0.8)
            AddPen(database, "창조주의 깃털", 250f, 0.8f, "신화", "태초의 신들이 세계를 지도로 그릴 때 썼던 영원의 신성을 품은 창조자의 깃펜입니다.");
            AddPen(database, "차원의 별빛 펜", 280f, 0.7f, "신화", "차원의 틈새에서 채취한 우주의 별빛 마나가 실시간으로 보충되는 초월적인 펜입니다.");
            AddPen(database, "아틀란티스의 신화 펜", 260f, 0.6f, "신화", "물에 잠긴 초고대 도시 아틀란티스의 바다 신들이 쓰던 영원무궁한 유물 펜입니다.");
            AddPen(database, "불멸의 태양 펜", 300f, 0.5f, "신화", "태양의 흑점에서 축출한 영겁의 열기가 불순한 잉크 소모를 완벽에 가깝게 차단해 줍니다.");
            AddPen(database, "영원의 크로노스 펜", 350f, 0.4f, "신화", "시공간의 흐름을 정지시키는 특수한 티타늄으로 만들어져 기적적인 수준으로 잉크를 아낄 수 있습니다.");

            if (isNew)
            {
                AssetDatabase.CreateAsset(database, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=lime>🎉 총 {database.pens.Count}개의 펜 데이터 프리셋이 {assetPath}에 생성되었습니다!</color>");
        }

        private static void AddPen(PenDatabase db, string name, float capacity, float consumptionRate, string grade, string description)
        {
            // 인스턴스를 하나 만들어서 리스트에 추가 (currentInkCapacity는 일단 기본적으로 0 또는 max로 채워둠)
            var newPen = new Item_Pen(capacity, consumptionRate, grade, name, description);
            newPen.currentInkCapacity = capacity; // 데이터베이스 프리셋용으로는 꽉 채워둠
            db.pens.Add(newPen);
        }
    }
}
