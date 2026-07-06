using UnityEngine;

namespace Magic.Combat
{
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;

        [Header("Status (Read Only)")]
        public string armedSpellName = "";
        public bool isArmed = false;

        private TextMesh textMesh;

        void Awake()
        {
            // 플레이어 머리 위에 장전된 마법을 표시할 텍스트 생성
            GameObject textObj = new GameObject("ArmedSpellText");
            textObj.transform.SetParent(this.transform);
            textObj.transform.localPosition = new Vector3(0, 1.5f, 0); // 머리 위
            
            textMesh = textObj.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 20;
            textMesh.characterSize = 0.1f;
            textMesh.color = Color.cyan;
            textMesh.text = "";
        }

        void Update()
        {
            // 1. WASD 탑다운 이동
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 moveDir = new Vector3(h, v, 0f).normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            // 2. 장전 UI 업데이트
            if (isArmed)
            {
                textMesh.text = $"[장전됨]\n{armedSpellName}";
            }
            else
            {
                textMesh.text = "";
            }
        }

        public void ArmSpell(string spellName)
        {
            armedSpellName = spellName;
            isArmed = true;
            Debug.Log($"<color=orange>[Player] {spellName} 마법이 장전되었습니다!</color>");
        }

        public void CastArmedSpell(Vector3 targetWorldPos)
        {
            if (!isArmed) return;

            Vector3 fireDir = (targetWorldPos - transform.position).normalized;
            Debug.Log($"<color=red>[Player] {armedSpellName} 발사!! 방향: {fireDir}</color>");
            
            // TODO: 실제 투사체(Projectile) 생성 로직이 들어갈 곳

            isArmed = false;
            armedSpellName = "";
        }
    }
}
