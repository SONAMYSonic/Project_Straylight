using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 제어를 위해 필수

public class PlayerHealth : MonoBehaviour
{
    [Header("Mental Status")]
    // [컨벤션] Private 필드는 _(언더스코어) 접두사 사용
    [SerializeField] private int _maxMental = 100;
    private int _currentMental;

    [Header("Invincibility")]
    [SerializeField] private float _invincibilityDuration = 1.0f; // 피격 후 무적 시간
    private bool _isInvincible = false;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _flashInterval = 0.1f; // 깜빡임 속도

    [Header("UI Reference")]
    // [수정됨] Slider 대신 Image 컴포넌트를 사용 (Filled 타입 제어용)
    [SerializeField] private Image _mentalBarImage;

    private void Awake()
    {
        // 안전성 강화: null 체크
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentMental = _maxMental;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // Guard Clause: 무적 상태이거나 이미 죽었으면 로직 중단
        if (_isInvincible || _currentMental <= 0) return;

        _currentMental -= damage;
        UpdateUI();

        if (_currentMental <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private void UpdateUI()
    {
        if (_mentalBarImage != null)
        {
            // [수정됨] Slider.value 대신 Image.fillAmount 사용
            // 0.0 ~ 1.0 사이의 비율로 이미지를 채우거나 깎음
            _mentalBarImage.fillAmount = (float)_currentMental / _maxMental;
        }
    }

    private void Die()
    {
        Debug.Log("Game Over! (멘탈 붕괴)");
        // TODO: 게임 오버 팝업 띄우기 등의 로직 추가

        // 플레이어 오브젝트 비활성화
        gameObject.SetActive(false);
    }

    // 무적 코루틴 (깜빡임 효과)
    private IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;
        // Debug.Log("무적 상태 돌입!");

        float elapsed = 0f;
        while (elapsed < _invincibilityDuration)
        {
            // 렌더러가 존재할 때만 깜빡임 처리
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(_flashInterval);
            elapsed += _flashInterval;
        }

        // 루프 종료 후 확실하게 보이도록 복구
        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }

        _isInvincible = false;
        // Debug.Log("무적 해제");
    }
}