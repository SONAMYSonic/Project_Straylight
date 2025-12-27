using System.Collections;
using UnityEngine;
using IdolMasterFanGame; // 네임스페이스 추가

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private int _damageToPlayer = 10;

    [Header("Attribute")]
    public IdolMode EnemyAttribute; // 이름 변경 (elementType -> EnemyAttribute)

    [Header("Visual")]
    [SerializeField] private Color _hitColor = Color.white;

    private int _currentHealth;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        InitColorByAttribute();

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
                _spriteRenderer.color = new Color(1f, 0.6f, 0.6f); // Red
                break;
            case IdolMode.Dance:
                _spriteRenderer.color = new Color(0.6f, 0.6f, 1f); // Blue
                break;
            case IdolMode.Visual:
                _spriteRenderer.color = new Color(1f, 1f, 0.6f); // Yellow
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
            var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damageToPlayer);
            }
        }
    }

    // [핵심 변경] 공격자의 속성(attackerMode)을 받아 상성 계산
    public void TakeDamage(int baseDamage, IdolMode attackerMode)
    {
        float multiplier = 1.0f;

        // 상성 로직: 가위바위보 (Vo > Da > Vi > Vo)
        if (attackerMode == IdolMode.Vocal && EnemyAttribute == IdolMode.Dance) multiplier = 1.5f;
        else if (attackerMode == IdolMode.Dance && EnemyAttribute == IdolMode.Visual) multiplier = 1.5f;
        else if (attackerMode == IdolMode.Visual && EnemyAttribute == IdolMode.Vocal) multiplier = 1.5f;

        // 역상성 (반감)
        else if (attackerMode == IdolMode.Vocal && EnemyAttribute == IdolMode.Visual) multiplier = 0.5f;
        else if (attackerMode == IdolMode.Dance && EnemyAttribute == IdolMode.Vocal) multiplier = 0.5f;
        else if (attackerMode == IdolMode.Visual && EnemyAttribute == IdolMode.Dance) multiplier = 0.5f;

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        _currentHealth -= finalDamage;

        // Debug.Log($"Hit! Mode: {attackerMode} vs {EnemyAttribute} / Dmg: {finalDamage} (x{multiplier})");

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
        _spriteRenderer.color = _hitColor;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = _originalColor;
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}