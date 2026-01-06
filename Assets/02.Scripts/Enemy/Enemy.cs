using System.Collections;
using UnityEngine;
using IdolMasterFanGame;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _baseMaxHealth = 20;
    [SerializeField] private int _damageToPlayer = 10;
    [SerializeField] private int _scoreValue = 1;

    [Header("Attribute")]
    public IdolMode EnemyAttribute;

    [Header("Visual")]
    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private int _flashCount = 2;

    [Header("Hit Effect")]
    [Tooltip("피격 시 생성할 파티클 이펙트 프리팹")]
    [SerializeField] private GameObject _hitEffectPrefab;
    [Tooltip("사망 시 생성할 파티클 이펙트 프리팹")]
    [SerializeField] private GameObject _deathEffectPrefab;

    [Header("Hit Sound")]
    [Tooltip("피격 효과음")]
    [SerializeField] private AudioClip _hitSFX;
    [Tooltip("사망 효과음")]
    [SerializeField] private AudioClip _deathSFX;

    [Header("Knockback")]
    [Tooltip("넉백 강도 (0이면 넉백 없음)")]
    [SerializeField] protected float _knockbackForce = 3f;
    [Tooltip("넉백 면역 (보스용)")]
    [SerializeField] protected bool _knockbackImmune = false;

    // 내부 변수
    protected int _maxHealth;
    protected int _currentHealth;
    protected SpriteRenderer _spriteRenderer;
    protected Color _originalColor;
    protected Rigidbody2D _rigidbody;
    private Sequence _flashSequence;

    // 상수 정의 (상성 배율)
    private const float DAMAGE_MULTIPLIER_SAME = 1.5f;
    private const float DAMAGE_MULTIPLIER_NORMAL = 1.0f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();

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

        if (attackerMode == EnemyAttribute)
        {
            multiplier = DAMAGE_MULTIPLIER_SAME;
        }

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        _currentHealth -= finalDamage;

        if (gameObject.activeInHierarchy)
        {
            PlayHitFlash();
            SpawnHitEffect();
            PlayHitSound();
            ApplyKnockback();
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback()
    {
        // 넉백 면역이거나 넉백 강도가 0이면 무시
        if (_knockbackImmune || _rigidbody == null || _knockbackForce <= 0f) return;

        // 플레이어 위치에서 멀어지는 방향으로 넉백
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            _rigidbody.AddForce(knockbackDir * _knockbackForce, ForceMode2D.Impulse);
        }
    }

    private void SpawnHitEffect()
    {
        if (_hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
            // 파티클 시스템은 재생 후 자동 삭제되도록 설정
            Destroy(effect, 2f);
        }
    }

    private void SpawnDeathEffect()
    {
        if (_deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    private void PlayHitSound()
    {
        if (_hitSFX != null)
        {
            SoundManager.Instance?.PlaySFX(_hitSFX);
        }
    }

    private void PlayDeathSound()
    {
        if (_deathSFX != null)
        {
            SoundManager.Instance?.PlaySFX(_deathSFX);
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
        SpawnDeathEffect();
        PlayDeathSound();
        GameManager.Instance?.OnEnemyKilled(_scoreValue);
        gameObject.SetActive(false);
    }
}