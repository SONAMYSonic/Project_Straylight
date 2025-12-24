using System.Collections;
using UnityEngine;
using static GameEnums;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _maxHealth = 3;
    [SerializeField] private int _damageToPlayer = 10; // 플레이어에게 줄 데미지

    [Header("Element")]
    // 이 변수를 인스펙터에서 바꿔서 적의 속성을 결정합니다.
    public ElementType elementType;

    [Header("Visual")]
    [SerializeField] private Color _hitColor = Color.white; // 피격 시 반짝일 색상

    // 내부 변수 (Private fields: _ 접두사 사용)
    private int _currentHealth;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        // 1. 컴포넌트 가져오기
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 2. 속성(Vo/Da/Vi)에 따라 색상 초기화
        InitColorByElement();

        // 3. 초기화된 색상을 원본 색상으로 저장 (나중에 피격 효과 후 돌아올 때 사용)
        _originalColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        // 풀링 재사용 시 초기화
        _currentHealth = _maxHealth;

        // 색상도 원래 속성 색으로 복구
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    // 속성에 따라 스프라이트 틴트(색조) 변경
    private void InitColorByElement()
    {
        if (_spriteRenderer == null) return;

        switch (elementType)
        {
            case ElementType.Vo: // 보컬 (파워)
                _spriteRenderer.color = new Color(1f, 0.6f, 0.6f); // 붉은색
                break;
            case ElementType.Da: // 댄스 (스피드)
                _spriteRenderer.color = new Color(0.6f, 0.6f, 1f); // 파란색
                break;
            case ElementType.Vi: // 비주얼 (유틸)
                _spriteRenderer.color = new Color(1f, 1f, 0.6f); // 노란색
                break;
            default:
                _spriteRenderer.color = Color.white;
                break;
        }
    }

    // 플레이어와의 충돌 감지 (몸통 박치기)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // PlayerHealth 컴포넌트를 찾아서 데미지를 줌
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damageToPlayer);
            }
        }
    }

    // 무기(Weapon)에서 호출하는 피격 함수
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        // 로그 출력 (디버깅용)
        // Debug.Log($"[{elementType}] Enemy Hit! HP: {_currentHealth}/{_maxHealth}");

        // 피격 효과 (깜빡임)
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
        // 맞았을 때 하얀색(또는 지정색)으로 깜빡임
        _spriteRenderer.color = _hitColor;

        yield return new WaitForSeconds(0.1f);

        // 원래 속성 색상으로 복구
        _spriteRenderer.color = _originalColor;
    }

    private void Die()
    {
        // TODO: 나중에 아이템 드랍이나 점수 추가 로직이 들어갈 곳
        // Debug.Log($"[{elementType}] 적 처치됨!");

        // 오브젝트 풀링을 위해 비활성화
        gameObject.SetActive(false);
    }
}