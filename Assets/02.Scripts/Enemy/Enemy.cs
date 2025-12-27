using System.Collections;
using UnityEngine;
using IdolMasterFanGame;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private int _damageToPlayer = 10;

    [Header("Attribute")]
    public IdolMode EnemyAttribute;

    [Header("Visual")]
    [SerializeField] private Color _hitColor = Color.white;

    private int _currentHealth;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 속성 색상 먼저 적용
        InitColorByAttribute();

        // 그 다음 그 색상을 '원본 색상'으로 저장 (피격 후 돌아오기 위해)
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        _currentHealth = _maxHealth;
        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;
    }

    private void InitColorByAttribute()
    {
        if (_spriteRenderer == null) return;

        switch (EnemyAttribute)
        {
            case IdolMode.Vocal:
                _spriteRenderer.color = new Color(1f, 0.6f, 0.6f); // Red Tint
                break;
            case IdolMode.Dance:
                _spriteRenderer.color = new Color(0.6f, 0.6f, 1f); // Blue Tint
                break;
            case IdolMode.Visual:
                _spriteRenderer.color = new Color(1f, 1f, 0.6f); // Yellow Tint
                break;
            default:
                _spriteRenderer.color = Color.white;
                break;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // GetComponent는 무거우므로 TryGetComponent 사용 권장 (유니티 최신 기능)
            if (collision.gameObject.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(_damageToPlayer);
            }
        }
    }

    public void TakeDamage(int baseDamage, IdolMode attackerMode)
    {
        float multiplier = 1.0f;

        // 상성 로직 (Vo > Da > Vi > Vo)
        if (attackerMode == IdolMode.Vocal && EnemyAttribute == IdolMode.Dance) multiplier = 1.5f;
        else if (attackerMode == IdolMode.Dance && EnemyAttribute == IdolMode.Visual) multiplier = 1.5f;
        else if (attackerMode == IdolMode.Visual && EnemyAttribute == IdolMode.Vocal) multiplier = 1.5f;

        // 역상성
        else if (attackerMode == IdolMode.Vocal && EnemyAttribute == IdolMode.Visual) multiplier = 0.5f;
        else if (attackerMode == IdolMode.Dance && EnemyAttribute == IdolMode.Vocal) multiplier = 0.5f;
        else if (attackerMode == IdolMode.Visual && EnemyAttribute == IdolMode.Dance) multiplier = 0.5f;

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        _currentHealth -= finalDamage;

        // 피격 효과
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FlashRoutine());
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _hitColor;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = _originalColor; // 원래 속성 색으로 복구
        }
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}