using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
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
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentMental = _maxMental;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (_isInvincible || _currentMental <= 0) return;

        _currentMental -= damage;
        UpdateUI();

        CameraShakeManager.Instance.ShakeCamera(2.0f);

        if (_currentMental <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // [추가됨] 외부(PlayerMovement)에서 무적 상태를 강제로 설정하는 함수
    public void SetInvincible(bool state)
    {
        _isInvincible = state;

        // 무적 상태 시각적 피드백 (반투명 처리 등)
        if (_spriteRenderer != null)
        {
            Color color = _spriteRenderer.color;
            color.a = state ? 0.5f : 1f; // 무적이면 반투명, 아니면 불투명
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