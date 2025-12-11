using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.red;
    private Color originalColor;

    [Header("Attack")]
    [SerializeField] private int damageToPlayer = 10; // 플레이어에게 줄 데미지

    // 충돌 감지 (몸통 박치기)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // PlayerHealth 컴포넌트를 찾아서 데미지를 줌
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
            }
        }
    }

    private void Awake()
    {
        // 스프라이트 렌더러 자동 찾기 (없으면 수동 할당)
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        // 풀링으로 재사용될 때를 대비해 체력 리셋
        currentHealth = maxHealth;
        spriteRenderer.color = originalColor;
    }

    // 외부(총알)에서 호출할 함수
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 피격 효과 (깜빡임)
        StartCoroutine(FlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine()
    {
        // 빨간색으로 변했다가
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        // 원래 색으로 복구
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        // 나중에 점수 추가, 아이템 드랍 로직이 여기에 들어갑니다.
        Debug.Log("적 처치!");

        // 풀링 시스템을 쓴다면 비활성화, 아니면 Destroy
        // 일단은 비활성화(SetActive false)로 처리합니다.
        gameObject.SetActive(false);
    }
}