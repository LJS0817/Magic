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
            public SpellElement resElement; public float resDuration; public float resPercentage;
            public PotionDef(string n, PotionType t, PotionGrade g, float r, string ra, string d, SpellElement re = SpellElement.None, float rd = 30f, float rp = 0.25f)
            {
                name = n; type = t; grade = g; recoveryAmount = r; rarity = ra; description = d;
                resElement = re; resDuration = rd; resPercentage = rp;
            }
        }

        private struct PouchDef
        {
            public string name; public int capacityBonus; public string grade; public string description;
            public PouchDef(string n, int cb, string g, string d) { name = n; capacityBonus = cb; grade = g; description = d; }
        }

        private struct CloakDef
        {
            public string name; public int def; public int atk; public float mana; public SpellElement element; public float res; public string grade; public string description;
            public CloakDef(string n, int d, int a, float m, SpellElement e, float r, string g, string desc) { name = n; def = d; atk = a; mana = m; element = e; res = r; grade = g; description = desc; }
        }

        private struct DrawingToolDef
        {
            public string name; public DrawingToolShape shape; public float accuracy; public float inkMult; public string grade; public string description;
            public DrawingToolDef(string n, DrawingToolShape s, float a, float i, string g, string desc) { name = n; shape = s; accuracy = a; inkMult = i; grade = g; description = desc; }
        }

        private struct RobeDef
        {
            public string name; public int def; public int atk; public float mana; public SpellElement element; public float res; public string grade; public string description;
            public RobeDef(string n, int d, int a, float m, SpellElement e, float r, string g, string desc) { name = n; def = d; atk = a; mana = m; element = e; res = r; grade = g; description = desc; }
        }

        private struct MaterialDef
        {
            public string name; public long price; public int maxStack; public string grade; public string description;
            public MaterialDef(string n, long p, int ms, string g, string desc) { name = n; price = p; maxStack = ms; grade = g; description = desc; }
        }

        private struct QuestDef
        {
            public string templateName; public string titleFormat; public string descFormat;
            public QuestDef(string n, string t, string d) { templateName = n; titleFormat = t; descFormat = d; }
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
            new PotionDef("상급 마나 물약", PotionType.Mana, PotionGrade.Greater, 100f, "희귀", "순수한 마력의 정수가 담겨 있어 마나를 대량으로 회복합니다."),
            new PotionDef("하급 화염 저항 물약", PotionType.ElementalResistance, PotionGrade.Lesser, 0f, "일반", "화염 속성 공격에 대한 저항력을 15% 높여주는 물약입니다.", SpellElement.Fire, 20f, 0.15f),
            new PotionDef("중급 화염 저항 물약", PotionType.ElementalResistance, PotionGrade.Medium, 0f, "고급", "화염 속성 공격에 대한 저항력을 30% 높여주는 물약입니다.", SpellElement.Fire, 40f, 0.30f),
            new PotionDef("상급 화염 저항 물약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "희귀", "화염 속성 공격에 대한 저항력을 50% 높여주는 고농축 물약입니다.", SpellElement.Fire, 60f, 0.50f),
            new PotionDef("하급 냉기 저항 물약", PotionType.ElementalResistance, PotionGrade.Lesser, 0f, "일반", "냉기 속성 공격에 대한 저항력을 15% 높여주는 따뜻한 물약입니다.", SpellElement.Ice, 20f, 0.15f),
            new PotionDef("중급 냉기 저항 물약", PotionType.ElementalResistance, PotionGrade.Medium, 0f, "고급", "냉기 속성 공격에 대한 저항력을 30% 높여주는 불꽃빛 물약입니다.", SpellElement.Ice, 40f, 0.30f),
            new PotionDef("상급 냉기 저항 물약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "희귀", "혹한의 눈보라 속에서도 60초간 50%의 냉기 내성을 보장하는 고농축 물약입니다.", SpellElement.Ice, 60f, 0.50f),
            new PotionDef("하급 번개 저항 물약", PotionType.ElementalResistance, PotionGrade.Lesser, 0f, "일반", "전격 속성 공격에 대한 저항력을 15% 높여주는 절연 물약입니다.", SpellElement.Lightning, 20f, 0.15f),
            new PotionDef("중급 번개 저항 물약", PotionType.ElementalResistance, PotionGrade.Medium, 0f, "고급", "전격 속성 공격에 대한 저항력을 30% 높여주는 노란 절연 물약입니다.", SpellElement.Lightning, 40f, 0.30f),
            new PotionDef("상급 번개 저항 물약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "희귀", "거센 번개 공격 속에서도 60초간 50%의 전격 내성을 보장하는 고농축 물약입니다.", SpellElement.Lightning, 60f, 0.50f),
            new PotionDef("하급 대지 저항 물약", PotionType.ElementalResistance, PotionGrade.Lesser, 0f, "일반", "대지 속성 공격에 대한 저항력을 15% 높여주는 가벼운 물약입니다.", SpellElement.Earth, 20f, 0.15f),
            new PotionDef("중급 대지 저항 물약", PotionType.ElementalResistance, PotionGrade.Medium, 0f, "고급", "대지 공격 및 진동에 대한 저항력을 30% 높여주는 초록빛 물약입니다.", SpellElement.Earth, 40f, 0.30f),
            new PotionDef("상급 대지 저항 물약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "희귀", "거대한 암석과 지진 속에서도 60초간 50%의 대지 내성을 보장하는 고농축 물약입니다.", SpellElement.Earth, 60f, 0.50f),
            new PotionDef("영웅의 화염 저항 영약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "영웅", "고대 드래곤의 피가 섞여 90초 동안 화염 내성을 70%까지 부여하는 영웅의 영약입니다.", SpellElement.Fire, 90f, 0.70f),
            new PotionDef("영웅의 냉기 저항 영약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "영웅", "만년빙의 정수가 담겨 90초 동안 냉기 내성을 70%까지 부여하는 영웅의 영약입니다.", SpellElement.Ice, 90f, 0.70f),
            new PotionDef("전설의 번개 저항 영약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "전설", "천둥 신의 번개를 가둬 120초 동안 전격 내성을 85%까지 부여하는 전설적인 영약입니다.", SpellElement.Lightning, 120f, 0.85f),
            new PotionDef("전설의 대지 저항 영약", PotionType.ElementalResistance, PotionGrade.Greater, 0f, "전설", "대지의 심장부 기운을 담아 120초 동안 대지 내성을 85%까지 부여하는 전설적인 영약입니다.", SpellElement.Earth, 120f, 0.85f)
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

        private static readonly CloakDef[] CloakDefs = new CloakDef[]
        {
            new CloakDef("여행자의 낡은 망토", 2, 0, 5f, SpellElement.None, 0f, "하급", "비바람을 막아주는 낡고 얇은 여행자용 망토입니다."),
            new CloakDef("견습 마법사의 망토", 3, 1, 10f, SpellElement.None, 0f, "일반", "마도학교 견습생들에게 지급되는 표준 단색 망토입니다."),
            new CloakDef("마력의 룬 망토", 5, 2, 20f, SpellElement.None, 0.05f, "고급", "천에 미세한 마나 룬이 수놓아져 있어 마력 흐름을 돕는 망토입니다."),
            new CloakDef("타오르는 불꽃 망토", 6, 3, 15f, SpellElement.Fire, 0.10f, "고급", "화염 정령의 실로 짜여 있어 화염 힘을 수용하고 능력을 더해줍니다."),
            new CloakDef("서리벌판의 그림자 망토", 8, 4, 30f, SpellElement.Ice, 0.15f, "희귀", "차가운 서리의 기운을 뿜어내며 착용자의 마력을 보호합니다."),
            new CloakDef("폭풍을 부르는 자의 망토", 10, 5, 35f, SpellElement.Lightning, 0.15f, "희귀", "정전기가 일어나는 푸른 망토로, 번개의 힘을 수용합니다."),
            new CloakDef("대지거인의 수호 망토", 15, 6, 45f, SpellElement.Earth, 0.20f, "영웅", "대지의 단단함을 품은 두꺼운 마법 가죽 망토입니다."),
            new CloakDef("홍염의 불사조 망토", 14, 10, 50f, SpellElement.Fire, 0.25f, "영웅", "불사조의 깃털로 장식되어 막대한 화염 속성 내성을 제공합니다."),
            new CloakDef("별빛 차원의 망토", 20, 12, 70f, SpellElement.None, 0.30f, "전설", "밤하늘의 별빛을 엮어 만들어 차원의 공격을 흘려보내는 신비한 망토입니다."),
            new CloakDef("대마도사의 영광", 25, 15, 100f, SpellElement.None, 0.35f, "전설", "역사상 최고의 대마도사가 둘렀던 전설적인 망토로 완벽에 가까운 마력 보조를 자랑합니다.")
        };

        private static readonly DrawingToolDef[] DrawingToolDefs = new DrawingToolDef[]
        {
            new DrawingToolDef("조잡한 나무 원형 도장", DrawingToolShape.Circle, 0.85f, 1.2f, "하급", "나무를 깎아 만든 투박한 원형 도장입니다. 잉크가 많이 낭비됩니다."),
            new DrawingToolDef("표준 마도 삼각 자", DrawingToolShape.Triangle, 0.88f, 1.1f, "일반", "기하학 마법을 그릴 때 사용하는 기본적인 삼각 도끼 모양 도구입니다."),
            new DrawingToolDef("단단한 황동 사각 인장", DrawingToolShape.Square, 0.90f, 1.0f, "일반", "사각형 마법진을 빠르고 정확하게 찍어내는 황동 인장입니다."),
            new DrawingToolDef("정밀한 유리 마름모 도구", DrawingToolShape.Rhombus, 0.92f, 0.95f, "고급", "정밀하게 가공된 유리가 마름모 형태의 술식을 잉크 낭비 없이 그려냅니다."),
            new DrawingToolDef("마도공학 원형 컴퍼스", DrawingToolShape.Circle, 0.93f, 0.90f, "고급", "완벽한 원을 그리도록 도와주는 마도공학 보조 도구입니다."),
            new DrawingToolDef("은빛 룬 삼각 도장", DrawingToolShape.Triangle, 0.95f, 0.85f, "희귀", "순은으로 제작된 삼각형 도장으로 룬의 힘이 마법 완성도를 보정해줍니다."),
            new DrawingToolDef("황금 비율의 사각 틀", DrawingToolShape.Square, 0.96f, 0.80f, "희귀", "황금비로 제작된 사각 인장으로, 안정적인 마술 구조를 직조합니다."),
            new DrawingToolDef("차원 결정의 마름모 인장", DrawingToolShape.Rhombus, 0.97f, 0.75f, "영웅", "차원 결정이 박혀 있어 마름모 형태의 마법을 극도의 정밀함으로 구현합니다."),
            new DrawingToolDef("천상의 원형 성상", DrawingToolShape.Circle, 0.98f, 0.70f, "영웅", "천상의 기하학이 담긴 성물로, 원형 마법진의 정확도를 비약적으로 끌어올립니다."),
            new DrawingToolDef("창조의 기하학 마도구", DrawingToolShape.Circle, 0.99f, 0.50f, "전설", "태초의 기하학적 진리가 담긴 전설의 마도구로 잉크 소모를 최소화하며 완벽에 가까운 도형을 직조합니다.")
        };

        private static readonly RobeDef[] RobeDefs = new RobeDef[]
        {
            new RobeDef("견습생의 낡은 로브", 3, 0, 10f, SpellElement.None, 0f, "하급", "마도학교 입학 시 지급되는 얇고 수수한 견습생용 로브입니다."),
            new RobeDef("정규 학도의 로브", 5, 1, 20f, SpellElement.None, 0f, "일반", "마도학 연구와 실습을 위해 편안하게 제작된 표준 로브입니다."),
            new RobeDef("마력 보존의 두꺼운 로브", 8, 2, 35f, SpellElement.None, 0.05f, "고급", "특수한 약재에 절여 마력 보존율을 높인 고급 펠트 로브입니다."),
            new RobeDef("홍염의 수술 로브", 9, 4, 30f, SpellElement.Fire, 0.10f, "고급", "화염 술사들을 위해 불에 타지 않는 방화포로 제작된 붉은 로브입니다."),
            new RobeDef("빙하의 결정 로브", 12, 5, 50f, SpellElement.Ice, 0.15f, "희귀", "만년빙의 냉기를 품은 실로 짜여져 정신을 맑게 하고 마나를 채워줍니다."),
            new RobeDef("천둥벌판의 도사 로브", 14, 7, 55f, SpellElement.Lightning, 0.15f, "희귀", "번개 문양이 새겨진 푸른 빛 로브로 전격 마법과 훌륭한 공명을 이룹니다."),
            new RobeDef("대지의 수호자 의복", 20, 8, 70f, SpellElement.Earth, 0.20f, "영웅", "대지 정령의 가호가 깃들어 뛰어난 물리 및 마법 방어력을 자랑합니다."),
            new RobeDef("고위 학정의 예복", 18, 12, 85f, SpellElement.None, 0.22f, "영웅", "마도협회 고위 간부들만이 입을 수 있는 위엄 넘치고 화려한 예복입니다."),
            new RobeDef("심연의 별자리 로브", 28, 18, 120f, SpellElement.None, 0.30f, "전설", "옷감 위에 심연의 우주와 별자리가 살아 움직이는 듯한 전설적인 예복입니다."),
            new RobeDef("영원불멸의 대마도사 예복", 35, 25, 150f, SpellElement.None, 0.35f, "전설", "세계를 구원한 전설적인 대마도사가 입었다고 전해지는 궁극의 로브입니다.")
        };

        private static readonly MaterialDef[] MaterialDefs = new MaterialDef[]
        {
            new MaterialDef("슬라임의 점액질", 10, 99, "하급", "어디서나 흔히 볼 수 있는 슬라임에서 채취한 끈적한 점액입니다."),
            new MaterialDef("마력 깃든 풀잎", 20, 99, "일반", "미세한 마나를 품고 자라난 푸른 약초 잎사귀입니다."),
            new MaterialDef("마도석 조각", 30, 99, "일반", "마력이 응축된 광산에서 캐낸 작은 마도석 파편입니다."),
            new MaterialDef("그리폰의 깃털", 50, 99, "고급", "바람을 가르는 그리폰의 깃털로, 가볍고 튼튼한 마법 도구 재료로 쓰입니다."),
            new MaterialDef("화염 원소의 가루", 60, 99, "고급", "화염 정령의 잔해에서 수집한 따뜻한 열기를 품은 가루입니다."),
            new MaterialDef("만년빙의 결정", 100, 50, "희귀", "녹지 않는 얼음동굴 깊은 곳에서 채취한 투명하고 차가운 결정입니다."),
            new MaterialDef("비룡의 비늘", 150, 50, "희귀", "하늘을 지배하는 비룡의 단단한 비늘로 뛰어난 내성을 지녔습니다."),
            new MaterialDef("세계수 수액", 300, 20, "영웅", "고대 세계수에서 천 년에 한 방울 떨어진다는 황금빛 생명의 수액입니다."),
            new MaterialDef("심연의 암흑 물질", 500, 20, "영웅", "차원의 균열 너머에서 흘러들어온 순수한 어둠의 농축물입니다."),
            new MaterialDef("드래곤의 심장 결정", 1000, 10, "전설", "고대 드래곤의 중심에서 거대한 마력을 생성해내던 궁극의 연금술 재료입니다.")
        };

        private static readonly QuestDef[] QuestDefs = new QuestDef[]
        {
            new QuestDef("기본 길드 의뢰", "길드 의뢰: {0}", "모험자 길드에서 {0} {1}개를 수집하고 있습니다."),
            new QuestDef("긴급 납품 의뢰", "긴급! {0} 조달", "{0} {1}개가 당장 필요합니다! 길드의 긴급 요청입니다."),
            new QuestDef("마법 연구 재료", "마법 연구 재료: {0}", "마도학자의 연구를 위해 {0} {1}개가 필요합니다."),
            new QuestDef("마을 주민의 부탁", "주민의 부탁: {0}", "마을 주민이 {0} {1}개를 구하고 있습니다. 도와주시겠습니까?"),
            new QuestDef("전리품 매입", "토벌 후처리: {0}", "마물 토벌 후 획득한 {0} {1}개를 길드에서 매입합니다."),
            new QuestDef("연금술 공방 의뢰", "연금술 재료 조달: {0}", "연금술 공방에서 특별한 비약을 만들기 위해 {0} {1}개를 찾고 있습니다."),
            new QuestDef("비밀스런 의뢰", "비밀스런 의뢰: {0}", "발주자를 알 수 없는 의뢰입니다. {0} {1}개를 조용히 납품하십시오."),
            new QuestDef("정기 물자 보충", "길드 정기 물자 보충: {0}", "길드 창고 비축을 위해 {0} {1}개가 필요합니다. 늦지 않게 부탁드립니다."),
            new QuestDef("상단 매입 의뢰", "상단의 수집 의뢰: {0}", "이웃 도시 상단에서 {0} {1}개를 대량 매입 중입니다. 좋은 기회입니다."),
            new QuestDef("수배 품목 납품", "수배 품목: {0}", "현재 시장 수요가 폭발적인 {0} {1}개를 길드에서 특별히 매입 중입니다.")
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
            CreateFolderIfNotExists(rootPath, "Cloaks");
            CreateFolderIfNotExists(rootPath, "DrawingTools");
            CreateFolderIfNotExists(rootPath, "Robes");
            CreateFolderIfNotExists(rootPath, "Materials");
            CreateFolderIfNotExists(rootPath, "Quests");

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
            db.cloaks.Clear();
            db.drawingTools.Clear();
            db.robes.Clear();
            db.materials.Clear();
            if(db.questTemplates != null) db.questTemplates.Clear();

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
                asset.resistanceElement = def.resElement;
                asset.resistanceDuration = def.resDuration;
                asset.resistancePercentage = def.resPercentage;
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

            // Generate Cloaks
            foreach (var def in CloakDefs)
            {
                string assetPath = $"{rootPath}/Cloaks/Cloak_{def.name.Replace(" ", "_")}.asset";
                ItemCloakSO asset = GetOrCreateAsset<ItemCloakSO>(assetPath);

                asset.itemName = def.name;
                asset.bonusDefense = def.def;
                asset.bonusAttack = def.atk;
                asset.bonusMaxMana = def.mana;
                asset.cloakElement = def.element;
                asset.elementResistanceBonus = def.res;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;

                EditorUtility.SetDirty(asset);
                db.cloaks.Add(asset);
            }

            // Generate DrawingTools
            foreach (var def in DrawingToolDefs)
            {
                string assetPath = $"{rootPath}/DrawingTools/DrawingTool_{def.name.Replace(" ", "_")}.asset";
                ItemDrawingToolSO asset = GetOrCreateAsset<ItemDrawingToolSO>(assetPath);

                asset.itemName = def.name;
                asset.targetShape = def.shape;
                asset.accuracyBonus = def.accuracy;
                asset.inkConsumptionMultiplier = def.inkMult;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;

                EditorUtility.SetDirty(asset);
                db.drawingTools.Add(asset);
            }

            // Generate Robes
            foreach (var def in RobeDefs)
            {
                string assetPath = $"{rootPath}/Robes/Robe_{def.name.Replace(" ", "_")}.asset";
                ItemRobeSO asset = GetOrCreateAsset<ItemRobeSO>(assetPath);

                asset.itemName = def.name;
                asset.bonusDefense = def.def;
                asset.bonusAttack = def.atk;
                asset.bonusMaxMana = def.mana;
                asset.robeElement = def.element;
                asset.elementResistanceBonus = def.res;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;

                EditorUtility.SetDirty(asset);
                db.robes.Add(asset);
            }

            // Generate Materials
            foreach (var def in MaterialDefs)
            {
                string assetPath = $"{rootPath}/Materials/Material_{def.name.Replace(" ", "_")}.asset";
                ItemMaterialSO asset = GetOrCreateAsset<ItemMaterialSO>(assetPath);

                asset.itemName = def.name;
                asset.basePriceInCopper = def.price;
                asset.maxStack = def.maxStack;
                asset.rarity = GetRarityFromString(def.grade);
                asset.itemDescription = def.description;

                EditorUtility.SetDirty(asset);
                db.materials.Add(asset);
            }

            // Generate Quests
            foreach (var def in QuestDefs)
            {
                string assetPath = $"{rootPath}/Quests/QuestTemplate_{def.templateName.Replace(" ", "_")}.asset";
                ItemQuestSO asset = GetOrCreateAsset<ItemQuestSO>(assetPath);

                asset.itemName = def.templateName;
                asset.questTitleFormat = def.titleFormat;
                asset.questDescFormat = def.descFormat;
                asset.rarity = ItemRarity.Common;

                EditorUtility.SetDirty(asset);
                if (db.questTemplates != null)
                {
                    db.questTemplates.Add(asset);
                }
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

            Debug.Log($"<color=lime>🎉 ItemDatabase 생성이 완료되었습니다!</color>\n- 펜: {db.pens.Count}종\n- 잉크: {db.inks.Count}종\n- 스크롤: {db.scrolls.Count}종\n- 지팡이: {db.wands.Count}종\n- 물약: {db.potions.Count}종\n- 주머니: {db.pouches.Count}종\n- 망토: {db.cloaks.Count}종\n- 도구(도장): {db.drawingTools.Count}종\n- 로브: {db.robes.Count}종\n- 재료: {db.materials.Count}종\n- 퀘스트 템플릿: {(db.questTemplates != null ? db.questTemplates.Count : 0)}종\nDB 위치: {dbPath}");
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
