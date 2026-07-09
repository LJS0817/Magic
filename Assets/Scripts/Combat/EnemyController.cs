using UnityEngine;
using DG.Tweening;

namespace Magic.Combat
{
    public class EnemyController : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float health = 100f;
        public float attackDamage = 15f;
        public SpellElement currentAttackElement = SpellElement.None;

        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;
        private Vector3 originalPosition;

        private void Start()
        {
            health = maxHealth;
            originalPosition = transform.position;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            // 등장 애니메이션: 0에서 본래 크기로 통통 튀며 나타남
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.8f).SetEase(Ease.OutBounce);
        }

        public void TakeDamage(float damage, SpellElement element = SpellElement.None)
        {
            health -= damage;

            Color flashColor = Color.white;
            string elementColor = "orange";
            string elementName = "";
            switch (element)
            {
                case SpellElement.Fire: elementColor = "red"; elementName = " Fire"; flashColor = Color.red; break;
                case SpellElement.Ice: elementColor = "aqua"; elementName = " Ice"; flashColor = Color.cyan; break;
                case SpellElement.Lightning: elementColor = "yellow"; elementName = " Lightning"; flashColor = Color.yellow; break;
                case SpellElement.Earth: elementColor = "#8B4513"; elementName = " Earth"; flashColor = new Color(0.5f, 0.25f, 0f); break;
            }

            Debug.Log($"<color={elementColor}>[Enemy] Took {damage}{elementName} damage! Remaining Health: {health}</color>");
            
            // 피격 애니메이션: 색상 번쩍임 및 강한 흔들림
            if (spriteRenderer != null)
            {
                spriteRenderer.DOColor(flashColor, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() => spriteRenderer.color = Color.white);
            }
            transform.DOComplete(); // 진행 중인 쉐이크 취소
            transform.DOShakePosition(0.3f, strength: new Vector3(0.5f, 0f, 0f), vibrato: 20);

            if (health <= 0)
            {
                Die();
            }
        }

        public void PrepareAttack()
        {
            // 무작위 속성 선택 (None 제외 1~4)
            currentAttackElement = (SpellElement)Random.Range(1, 5);

            string elementColor = "white";
            switch (currentAttackElement)
            {
                case SpellElement.Fire: elementColor = "red"; break;
                case SpellElement.Ice: elementColor = "aqua"; break;
                case SpellElement.Lightning: elementColor = "yellow"; break;
                case SpellElement.Earth: elementColor = "#8B4513"; break;
            }

            Debug.Log($"<color=red>[Enemy] Preparing to attack with </color><color={elementColor}>{currentAttackElement}</color><color=red> element!</color>");
            
            // 공격 준비 애니메이션: 뒤로 살짝 물러나며 붉게 빛남 (Anticipation)
            transform.DOMoveX(originalPosition.x + 1f, 0.5f).SetEase(Ease.OutQuad);
            if (spriteRenderer != null) spriteRenderer.DOColor(new Color(1f, 0.5f, 0.5f), 0.5f);

            CombatManager.Instance.OpenParryWindow();
        }

        public void ExecuteAttack(float damageMultiplier = 1f)
        {
            Debug.Log($"<color=red>[Enemy] Attack Executed! (Multiplier: {damageMultiplier:F1})</color>");
            
            // 공격 실행 애니메이션: 앞으로 빠르게 돌진 후 원래 위치로 복귀
            transform.DOMoveX(originalPosition.x - 3f, 0.1f).SetEase(Ease.InExpo).OnComplete(() =>
            {
                transform.DOMoveX(originalPosition.x, 0.3f).SetEase(Ease.OutQuad);
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
            });

            if (CombatManager.Instance.player != null)
            {
                CombatManager.Instance.player.TakeDamage(attackDamage * damageMultiplier);
            }
        }

        public void OnParried()
        {
            Debug.Log("<color=cyan>[Enemy] Attack was Parried! Staggered!</color>");
            // 패링 성공 시 크게 뒤로 밀려나며 스턴 (흔들림)
            transform.DOComplete();
            transform.DOMoveX(originalPosition.x + 3f, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                transform.DOShakePosition(1f, new Vector3(0.2f, 0f, 0f), 10);
                transform.DOMoveX(originalPosition.x, 0.5f).SetDelay(1f);
            });
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
        }

        private void Die()
        {
            Debug.Log("<color=gray>[Enemy] Defeated!</color>");
            // 사망 애니메이션: 위로 살짝 뜨며 작아지고 투명해짐
            transform.DOComplete();
            transform.DOMoveY(originalPosition.y + 2f, 0.8f).SetEase(Ease.OutQuad);
            transform.DOScale(Vector3.zero, 0.8f).SetEase(Ease.InBack);
            if (spriteRenderer != null) spriteRenderer.DOFade(0f, 0.8f);
            
            // 1초 뒤 오브젝트 파괴 (또는 풀링 반환)
            Destroy(gameObject, 1f);
        }
    }
}
