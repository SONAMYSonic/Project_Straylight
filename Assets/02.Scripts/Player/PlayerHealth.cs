using System; // Action 이벤트를 위해 필수
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    // [매직 넘버 제거] 상수 정의
    private const float INVINCIBLE_ALPHA = 0.5f; // 무적일 때 투명도
    private const float NORMAL_ALPHA = 1.0f;     // 평상시 불투명도

    // [옵저버 패턴] 외부로 보낼 이벤트: "데미지를 입었음"
    // Action은 C#의 기본 델리게이트로, 구독자들에게 신호를 보냅니다.
    public event Action OnDamageTaken;
    public event Action OnDie; // 죽었을 때 이벤트도 추가해두면 나중에 편리합니다.

    [Header("Mental Status")]
    [SerializeField] private int _maxMental = 100;
    private int _currentMental;

    [Header("Invincibility")]
    [SerializeField] private float _invincibilityDuration = 1.0f;
    private bool _isInvincible = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flashInterval = 0.1f;

    [Header("UI Reference")]
    [SerializeField] private Image _mentalBarImage;

    private void Awake()
    {
        // 안전성 강화: 캐싱
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentMental = _maxMental;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // Guard Clause
        if (_isInvincible || _currentMental <= 0) return;

        _currentMental -= damage;
        UpdateUI();

        // [핵심] 직접 카메라를 흔드는 대신, "나 맞았어!"라고 외치기만 함
        // ?.Invoke()는 구독자가 있을 때만 실행하라는 안전한 호출 방식입니다.
        OnDamageTaken?.Invoke();

        if (_currentMental <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // 외부(PlayerMovement 등)에서 무적 상태를 제어할 때 사용
    public void SetInvincible(bool state)
    {
        _isInvincible = state;

        if (_spriteRenderer != null)
        {
            Color color = _spriteRenderer.color;
            // 매직 넘버(0.5f)를 상수로 대체
            color.a = state ? INVINCIBLE_ALPHA : NORMAL_ALPHA;
            _spriteRenderer.color = color;
        }
    }

    private void UpdateUI()
    {
        if (_mentalBarImage != null)
        {
            _mentalBarImage.fillAmount = (float)_currentMental / _maxMental;
        }
    }

    private void Die()
    {
        Debug.Log("Game Over! (멘탈 붕괴)");

        OnDie?.Invoke(); // 사망 이벤트 발생

        // 플레이어 비활성화
        gameObject.SetActive(false);
    }

    private IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;

        float elapsed = 0f;
        while (elapsed < _invincibilityDuration)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.enabled = !_spriteRenderer.enabled;

            yield return new WaitForSeconds(_flashInterval);
            elapsed += _flashInterval;
        }

        // 루프 종료 후 확실하게 보이도록 복구
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
        _isInvincible = false;
    }
}