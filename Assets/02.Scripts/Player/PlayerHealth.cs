using System;
using System.Collections;
using UnityEngine;
// [수정] using UnityEngine.UI; 삭제 (UI 직접 참조 안 함)

public class PlayerHealth : MonoBehaviour
{
    private const float INVINCIBLE_ALPHA = 0.5f;
    private const float NORMAL_ALPHA = 1.0f;
    private const float MIN_MENTAL = 0f;

    public event Action OnDamageTaken;
    public event Action OnDie;

    [Header("Mental Status")]
    [SerializeField] private int _maxMental = 100;
    private float _currentMental;

    [Header("Invincibility")]
    [SerializeField] private float _invincibilityDuration = 1.0f;
    private bool _isInvincible = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flashInterval = 0.1f;

    // [수정] UI 변수 삭제 -> UIManager가 담당함
    // [SerializeField] private Image _mentalBarImage;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentMental = _maxMental;
        // 시작 시 UI 초기화 요청
        UIManager.Instance?.UpdateHealth(_currentMental, _maxMental);
    }

    public void TakeDamage(int damage)
    {
        if (_isInvincible || _currentMental <= MIN_MENTAL) return;

        _currentMental -= damage;

        // [수정] UIManager에게 체력 갱신 요청
        UIManager.Instance?.UpdateHealth(_currentMental, _maxMental);

        OnDamageTaken?.Invoke();

        if (_currentMental <= MIN_MENTAL)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    public void SetInvincible(bool state)
    {
        _isInvincible = state;
        UpdateVisualColor(state);
    }

    private void UpdateVisualColor(bool isInvincible)
    {
        if (_spriteRenderer != null)
        {
            Color color = _spriteRenderer.color;
            color.a = isInvincible ? INVINCIBLE_ALPHA : NORMAL_ALPHA;
            _spriteRenderer.color = color;
        }
    }

    // [수정] UpdateUI 메서드 삭제 (필요 없음)

    private void Die()
    {
        OnDie?.Invoke();
        GameManager.Instance?.OnPlayerDead();
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
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
        _isInvincible = false;
    }
}