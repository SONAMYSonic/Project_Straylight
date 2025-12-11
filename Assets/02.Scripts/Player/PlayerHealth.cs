using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI 제어를 위해 필수

public class PlayerHealth : MonoBehaviour
{
    [Header("Mental Status")]
    [SerializeField] private int maxMental = 100;
    private int currentMental;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1.0f; // 피격 후 무적 시간
    private bool isInvincible = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashInterval = 0.1f; // 깜빡임 속도

    [Header("UI Reference")]
    public Slider mentalSlider; // HP바(슬라이더) 연결

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        currentMental = maxMental;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // 무적 상태이거나 이미 죽었으면 무시
        if (isInvincible || currentMental <= 0) return;

        currentMental -= damage;
        UpdateUI();

        if (currentMental <= 0)
        {
            Die();
        }
        else
        {
            // 아직 살았으면 무적 시간 발동
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private void UpdateUI()
    {
        if (mentalSlider != null)
        {
            // 슬라이더 값 갱신 (0 ~ 1 사이 비율로)
            mentalSlider.value = (float)currentMental / maxMental;
        }
    }

    private void Die()
    {
        Debug.Log("Game Over! (멘탈 붕괴)");
        // TODO: 게임 오버 UI 띄우기

        // 임시: 플레이어 사라지게 하기
        gameObject.SetActive(false);

        // 임시: 게임 멈춤
        // Time.timeScale = 0; 
    }

    // 무적 코루틴 (깜빡임 효과)
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        Debug.Log("무적 상태 돌입!");

        // 무적 시간 동안 깜빡거림
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // 껐다 켰다
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        spriteRenderer.enabled = true; // 확실하게 켜진 상태로 복구
        isInvincible = false;
        Debug.Log("무적 해제");
    }
}