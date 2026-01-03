using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private const float INVINCIBLE_ALPHA = 0.5f;
    private const float NORMAL_ALPHA = 1.0f;
    private const float MIN_MENTAL = 0f;

    public event Action OnDamageTaken;
    public event Action OnDie;
    public event Action OnRevive;

    [Header("Mental Status")]
    [SerializeField] private int _maxMental = 100;
    private float _currentMental;

    [Header("HP Regeneration")]
    [Tooltip("HP 자동 회복 활성화")]
    [SerializeField] private bool _enableRegen = true;
    [Tooltip("초당 HP 회복량")]
    [SerializeField] private float _regenPerSecond = 2f;
    [Tooltip("피격 후 회복 시작까지 대기 시간 (초)")]
    [SerializeField] private float _regenDelay = 3f;

    [Header("Invincibility")]
    [SerializeField] private float _invincibilityDuration = 1.0f;
    [SerializeField] private float _reviveInvincibilityDuration = 3.0f;
    private bool _isInvincible = false;
    private bool _isDead = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flashInterval = 0.1f;

    private float _lastDamageTime;
    private Coroutine _regenCoroutine;

    public bool IsDead => _isDead;
    public float CurrentMental => _currentMental;
    public float MaxMental => _maxMental;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentMental = _maxMental;
        UIManager.Instance?.UpdateHealth(_currentMental, _maxMental);
    }

    private void Start()
    {
        if (_enableRegen)
        {
            _regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }

    private IEnumerator RegenRoutine()
    {
        while (true)
        {
            yield return null;

            if (_isDead) continue;
            if (_currentMental >= _maxMental) continue;

            // 피격 후 딜레이 체크
            if (Time.time - _lastDamageTime < _regenDelay) continue;

            // HP 회복
            _currentMental += _regenPerSecond * Time.deltaTime;
            _currentMental = Mathf.Min(_currentMental, _maxMental);

            UIManager.Instance?.UpdateHealth(_currentMental, _maxMental);
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isInvincible || _currentMental <= MIN_MENTAL) return;

        _currentMental -= damage;
        _lastDamageTime = Time.time;

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

    private void Die()
    {
        _isDead = true;
        OnDie?.Invoke();
        GameManager.Instance?.OnPlayerDead();
        gameObject.SetActive(false);
    }

    public void Revive()
    {
        if (!_isDead) return;

        _isDead = false;
        _currentMental = _maxMental;
        gameObject.SetActive(true);

        UIManager.Instance?.UpdateHealth(_currentMental, _maxMental);

        OnRevive?.Invoke();

        StartCoroutine(ReviveInvincibilityRoutine());
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

    private IEnumerator ReviveInvincibilityRoutine()
    {
        _isInvincible = true;
        float elapsed = 0f;
        while (elapsed < _reviveInvincibilityDuration)
        {
            if (_spriteRenderer != null)
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            yield return new WaitForSeconds(_flashInterval);
            elapsed += _flashInterval;
        }
        if (_spriteRenderer != null) _spriteRenderer.enabled = true;
        _isInvincible = false;
    }

    private void OnDisable()
    {
        if (_regenCoroutine != null)
        {
            StopCoroutine(_regenCoroutine);
        }
    }

    private void OnEnable()
    {
        if (_enableRegen && _regenCoroutine == null)
        {
            _regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }
}