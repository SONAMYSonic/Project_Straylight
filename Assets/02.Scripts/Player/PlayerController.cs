using UnityEngine;
using IdolMasterFanGame.UI;

public class PlayerController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private PlayerModeUI _playerModeUI;

    [Header("Settings")]
    [Tooltip("피격 시 카메라 흔들림 강도")]
    [SerializeField] private float _damageShakeIntensity = 2.0f; // [매직 넘버 제거] 변수로 추출

    // 컴포넌트 캐싱
    private PlayerInput _input;
    private PlayerMovement _movement;
    private PlayerCombat _combat;
    private PlayerHealth _health;

    private void Awake()
    {
        // GetComponent는 Awake에서 한 번만 호출하여 캐싱 (성능 최적화)
        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
        _health = GetComponent<PlayerHealth>();

        // 방어 코드: 필수 컴포넌트가 없으면 추가
        if (_input == null) _input = gameObject.AddComponent<PlayerInput>();
        if (_movement == null) _movement = gameObject.AddComponent<PlayerMovement>();
        if (_combat == null) _combat = gameObject.AddComponent<PlayerCombat>();
        // Health는 보통 미리 붙어있다고 가정하지만, 없으면 경고
        if (_health == null) Debug.LogWarning("[PlayerController] PlayerHealth 컴포넌트가 없습니다!");
    }

    private void Start()
    {
        // 1. 하위 컴포넌트 초기화
        _movement.Initialize(_input);
        _combat.Initialize(_input);

        // 2. 이벤트 연결 (Observer Pattern) - 모드 UI
        if (_combat != null && _playerModeUI != null)
        {
            _combat.OnModeChanged += _playerModeUI.UpdateModeUI;
            // 초기 상태 동기화
            _playerModeUI.UpdateModeUI(_combat.CurrentMode);
        }

        // 3. [추가됨] 이벤트 연결 - 피격 시 카메라 흔들림
        if (_health != null)
        {
            // 람다식(Lambda)을 사용하여 깔끔하게 연결
            // "체력이 달면 -> 쉐이크 매니저야 흔들어라"
            _health.OnDamageTaken += HandleDamageShake;
        }
    }

    private void OnDestroy()
    {
        // [중요] 이벤트 구독 해제 (메모리 누수 방지)
        if (_combat != null && _playerModeUI != null)
        {
            _combat.OnModeChanged -= _playerModeUI.UpdateModeUI;
        }

        if (_health != null)
        {
            _health.OnDamageTaken -= HandleDamageShake;
        }
    }

    private void HandleDamageShake()
    {
        // 싱글톤 인스턴스 존재 여부 확인 후 호출
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeCamera(_damageShakeIntensity);
        }
    }

    private void Update()
    {
        // 입력에 따른 로직 수행
        _movement.HandleDash();
        _combat.HandleAttack();
    }

    private void FixedUpdate()
    {
        _movement.HandleMovement();
    }
}