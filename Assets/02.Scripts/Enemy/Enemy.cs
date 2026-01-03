using System.Collections;
using UnityEngine;
using IdolMasterFanGame;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _baseMaxHealth = 30;
    [SerializeField] private int _damageToPlayer = 10;
    [SerializeField] private int _scoreValue = 1;

    [Header("Attribute")]
    public IdolMode EnemyAttribute;

    [Header("Visual")]
    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private int _flashCount = 2;

    // 내부 변수
    protected int _maxHealth;
    protected int _currentHealth;
    protected SpriteRenderer _spriteRenderer;
    protected Color _originalColor;
    private Sequence _flashSequence;

    // 상수 정의 (상성 배율)
    private const float DAMAGE_MULTIPLIER_SAME = 1.5f;    // 같은 속성 = 강함
    private const float DAMAGE_MULTIPLIER_NORMAL = 1.0f;  // 다른 속성 = 보통

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;

        _maxHealth = _baseMaxHealth;
    }

    private void OnEnable()
    {
        _currentHealth = _maxHealth;
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }

    private void OnDisable()
    {
        _flashSequence?.Kill();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(_damageToPlayer);
            }
        }
    }

    /// <summary>
    /// HP를 직접 설정 (보스용)
    /// </summary>
    public void SetHealth(int health)
    {
        _maxHealth = health;
        _currentHealth = health;
    }

    /// <summary>
    /// 웨이브 스케일링용 HP 배율 적용
    /// </summary>
    public void ApplyHealthMultiplier(float multiplier)
    {
        _maxHealth = Mathf.RoundToInt(_baseMaxHealth * multiplier);
        _currentHealth = _maxHealth;
    }

    public virtual void TakeDamage(int baseDamage, IdolMode attackerMode)
    {
        float multiplier = DAMAGE_MULTIPLIER_NORMAL;

        // 같은 속성이면 추가 데미지
        if (attackerMode == EnemyAttribute)
        {
            multiplier = DAMAGE_MULTIPLIER_SAME;
        }

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        _currentHealth -= finalDamage;

        if (gameObject.activeInHierarchy)
        {
            PlayHitFlash();
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    protected void PlayHitFlash()
    {
        if (_spriteRenderer == null) return;

        _flashSequence?.Kill();
        _spriteRenderer.color = _originalColor;

        _flashSequence = DOTween.Sequence();
        float singleFlashDuration = _flashDuration / (_flashCount * 2);

        for (int i = 0; i < _flashCount; i++)
        {
            _flashSequence.Append(_spriteRenderer.DOColor(_hitColor, singleFlashDuration).SetEase(Ease.OutQuad));
            _flashSequence.Append(_spriteRenderer.DOColor(_originalColor, singleFlashDuration).SetEase(Ease.InQuad));
        }
    }

    protected void UpdateOriginalColor(Color newColor)
    {
        _originalColor = newColor;
        if (_spriteRenderer != null)
            _spriteRenderer.color = newColor;
    }

    protected virtual void Die()
    {
        GameManager.Instance?.OnEnemyKilled(_scoreValue);
        gameObject.SetActive(false);
    }
}