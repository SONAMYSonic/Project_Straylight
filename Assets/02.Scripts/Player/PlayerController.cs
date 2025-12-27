using UnityEngine;
using IdolMasterFanGame.UI; // UI 네임스페이스 사용

public class PlayerController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private PlayerModeUI _playerModeUI; // 인스펙터에서 할당

    private PlayerInput _input;
    private PlayerMovement _movement;
    private PlayerCombat _combat;

    void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();

        // 방어 코드: 없으면 추가
        if (_input == null) _input = gameObject.AddComponent<PlayerInput>();
        if (_movement == null) _movement = gameObject.AddComponent<PlayerMovement>();
        if (_combat == null) _combat = gameObject.AddComponent<PlayerCombat>();
    }

    void Start()
    {
        // 1. 컴포넌트 초기화
        _movement.Initialize(_input);
        _combat.Initialize(_input);

        // 2. 이벤트 연결 (Observer Pattern)
        // Combat에서 모드가 바뀌면 UI에게 업데이트하라고 알림
        if (_combat != null && _playerModeUI != null)
        {
            _combat.OnModeChanged += _playerModeUI.UpdateModeUI;

            // 게임 시작 시 초기 상태 한 번 동기화
            _playerModeUI.UpdateModeUI(_combat.CurrentMode);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (_combat != null && _playerModeUI != null)
        {
            _combat.OnModeChanged -= _playerModeUI.UpdateModeUI;
        }
    }

    void Update()
    {
        _movement.HandleDash();
        _combat.HandleAttack();
    }

    void FixedUpdate()
    {
        _movement.HandleMovement();
    }
}