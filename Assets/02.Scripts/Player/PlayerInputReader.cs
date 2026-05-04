using UnityEngine;
using UnityEngine.InputSystem;

// 책임: 사용자의 입력을 감지하고 값을 제공하기만 함
// Input System 기반. PC(키보드/마우스) + 모바일(터치 OnScreen UI) 둘 다 지원.
// 이름은 PlayerInputReader (UnityEngine.InputSystem.PlayerInput과의 충돌 회피)
public class PlayerInputReader : MonoBehaviour
{
    [Header("Input Actions Asset")]
    [Tooltip("Assets/InputSystem_Actions.inputactions 를 드래그")]
    [SerializeField] private InputActionAsset _inputActions;

    public Vector2 MoveDir { get; private set; }
    public Vector2 MousePos { get; private set; }
    public bool IsDashTriggered { get; private set; }
    public bool IsUltTriggered { get; private set; }
    public bool IsAttackTap { get; private set; }

    public float ScrollY { get; private set; }

    public bool IsVocalKeyPressed { get; private set; }
    public bool IsDanceKeyPressed { get; private set; }
    public bool IsVisualKeyPressed { get; private set; }

    public bool IsPauseTriggered { get; private set; }

    private InputActionMap _playerMap;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _attackAction;
    private InputAction _dashAction;
    private InputAction _ultAction;
    private InputAction _modeVocalAction;
    private InputAction _modeDanceAction;
    private InputAction _modeVisualAction;
    private InputAction _modeCycleAction;
    private InputAction _pauseAction;

    private Camera _mainCam;

    private void Awake()
    {
        if (_inputActions == null)
        {
            Debug.LogError("[PlayerInputReader] InputActionAsset이 할당되지 않았습니다. Inspector에서 InputSystem_Actions를 드래그하세요.");
            enabled = false;
            return;
        }

        _playerMap = _inputActions.FindActionMap("Player", throwIfNotFound: true);
        _moveAction = _playerMap.FindAction("Move", throwIfNotFound: true);
        _lookAction = _playerMap.FindAction("Look", throwIfNotFound: true);
        _attackAction = _playerMap.FindAction("Attack", throwIfNotFound: true);
        _dashAction = _playerMap.FindAction("Dash", throwIfNotFound: true);
        _ultAction = _playerMap.FindAction("Ult", throwIfNotFound: true);
        _modeVocalAction = _playerMap.FindAction("ModeVocal", throwIfNotFound: true);
        _modeDanceAction = _playerMap.FindAction("ModeDance", throwIfNotFound: true);
        _modeVisualAction = _playerMap.FindAction("ModeVisual", throwIfNotFound: true);
        _modeCycleAction = _playerMap.FindAction("ModeCycle", throwIfNotFound: true);
        _pauseAction = _playerMap.FindAction("Pause", throwIfNotFound: true);
    }

    private void Start()
    {
        _mainCam = Camera.main;
    }

    private void OnEnable()
    {
        _playerMap?.Enable();
    }

    private void OnDisable()
    {
        _playerMap?.Disable();
    }

    private void Update()
    {
        // 1. 이동 (Vector2)
        MoveDir = _moveAction.ReadValue<Vector2>().normalized;

        // 2. 포인터/마우스 위치 (스크린 → 월드 좌표)
        if (_mainCam != null)
        {
            Vector2 screenPos = _lookAction.ReadValue<Vector2>();
            MousePos = _mainCam.ScreenToWorldPoint(screenPos);
        }

        // 3. 액션 (one-shot triggers)
        IsAttackTap = _attackAction.WasPressedThisFrame();
        IsDashTriggered = _dashAction.WasPressedThisFrame();
        IsUltTriggered = _ultAction.WasPressedThisFrame();

        // 4. 모드 변경 키
        IsVocalKeyPressed = _modeVocalAction.WasPressedThisFrame();
        IsDanceKeyPressed = _modeDanceAction.WasPressedThisFrame();
        IsVisualKeyPressed = _modeVisualAction.WasPressedThisFrame();

        // 5. 마우스 휠 (모드 사이클)
        ScrollY = _modeCycleAction.ReadValue<float>();

        // 6. 일시정지
        IsPauseTriggered = _pauseAction.WasPressedThisFrame();
    }
}
