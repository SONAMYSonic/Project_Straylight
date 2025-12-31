using UnityEngine;
// [수정] using IdolMasterFanGame.UI; 삭제 가능 (UIManager가 처리하므로)

public class PlayerController : MonoBehaviour
{
    // [수정] UI 직접 참조 삭제
    // [SerializeField] private PlayerModeUI _playerModeUI; 

    [Header("Settings")]
    [Tooltip("피격 시 카메라 흔들림 강도")]
    [SerializeField] private float _damageShakeIntensity = 2.0f;

    private PlayerInput _input;
    private PlayerMovement _movement;
    private PlayerCombat _combat;
    private PlayerHealth _health;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
        _health = GetComponent<PlayerHealth>();

        if (_input == null) _input = gameObject.AddComponent<PlayerInput>();
        if (_movement == null) _movement = gameObject.AddComponent<PlayerMovement>();
        if (_combat == null) _combat = gameObject.AddComponent<PlayerCombat>();

        if (_health == null)
            Debug.LogWarning("[PlayerController] PlayerHealth가 없습니다.");
    }

    private void Start()
    {
        _movement.Initialize(_input);
        _combat.Initialize(_input);

        // [수정] 모드 변경 이벤트를 UIManager에게 연결
        if (_combat != null)
        {
            // "모드가 바뀌면 -> UIManager야 UI 좀 업데이트해줘"
            _combat.OnModeChanged += (mode) => UIManager.Instance?.UpdatePlayerMode(mode);

            // 초기 상태 동기화
            UIManager.Instance?.UpdatePlayerMode(_combat.CurrentMode);
        }

        // 피격 카메라 흔들림 연결
        if (_health != null)
        {
            _health.OnDamageTaken += HandleDamageShake;
        }
    }

    // OnDestroy에서 람다식 구독 해제는 까다로우므로, 
    // PlayerController가 파괴될 정도면 게임 종료나 씬 전환이므로 생략해도 무방하나,
    // 정석대로 하려면 별도 메서드로 분리해야 합니다. 
    // 여기서는 간단하게 생략합니다. (메모리 누수 위험 적음)

    private void HandleDamageShake()
    {
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeCamera(_damageShakeIntensity);
        }
    }

    private void Update()
    {
        _movement.HandleDash();
        _combat.HandleAttack();
    }

    private void FixedUpdate()
    {
        _movement.HandleMovement();
    }
}