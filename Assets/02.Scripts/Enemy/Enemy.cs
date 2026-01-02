using System.Collections;
using UnityEngine;
using IdolMasterFanGame;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private int _damageToPlayer = 10;
    [SerializeField] private int _scoreValue = 1;

    [Header("Attribute")]
    public IdolMode EnemyAttribute;

    [Header("Visual")]
    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private int _flashCount = 2;

    // 내부 변수
    protected int _currentHealth;
    protected SpriteRenderer _spriteRenderer;
    protected Color _originalColor;
    private Sequence _flashSequence;

    // 상수 정의 (상성 배율)
    private const float DAMAGE_MULTIPLIER_STRONG = 1.5f;
    private const float DAMAGE_MULTIPLIER_WEAK = 0.5f;
    private const float DAMAGE_MULTIPLIER_NORMAL = 1.0f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
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

    public virtual void TakeDamage(int baseDamage, IdolMode attackerMode)
    {
        float multiplier = DAMAGE_MULTIPLIER_NORMAL;

        // 상성 로직 (Vo > Da > Vi > Vo)
        if (attackerMode == IdolMode.Vocal && EnemyAttribute == IdolMode.Dance) multiplier = DAMAGE_MULTIPLIER_STRONG;
        else if (attackerMode == IdolMode.Dance && EnemyAttribute == IdolMode.Visual) multiplier = DAMAGE_MULTIPLIER_STRONG;
        else if (attackerMode == IdolMode.Visual && EnemyAttribute == IdolMode.Vocal) multiplier = DAMAGE_MULTIPLIER_STRONG;

        // 역상성
        else if (attackerMode == IdolMode.Vocal && EnemyAttribute == IdolMode.Visual) multiplier = DAMAGE_MULTIPLIER_WEAK;
        else if (attackerMode == IdolMode.Dance && EnemyAttribute == IdolMode.Vocal) multiplier = DAMAGE_MULTIPLIER_WEAK;
        else if (attackerMode == IdolMode.Visual && EnemyAttribute == IdolMode.Dance) multiplier = DAMAGE_MULTIPLIER_WEAK;

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

        // 기존 시퀀스 중단
        _flashSequence?.Kill();
        _spriteRenderer.color = _originalColor;

        // DOTween Sequence로 깜빡임 효과
        _flashSequence = DOTween.Sequence();
        float singleFlashDuration = _flashDuration / (_flashCount * 2);

        for (int i = 0; i < _flashCount; i++)
        {
            _flashSequence.Append(_spriteRenderer.DOColor(_hitColor, singleFlashDuration).SetEase(Ease.OutQuad));
            _flashSequence.Append(_spriteRenderer.DOColor(_originalColor, singleFlashDuration).SetEase(Ease.InQuad));
        }
    }

    /// <summary>
    /// 스프라이트 색상 변경 후 _originalColor도 함께 업데이트
    /// </summary>
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