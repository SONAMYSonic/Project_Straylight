using UnityEngine;

// 책임: 사용자의 입력을 감지하고 값을 제공하기만 함
public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveDir { get; private set; }
    public Vector2 MousePos { get; private set; }
    public bool IsDashTriggered { get; private set; }
    public bool IsFireHeld { get; private set; }
    public bool IsMeleeTriggered { get; private set; }
    public bool IsUltTriggered { get; private set; }

    public bool IsAttackHeld { get; private set; }
    public bool IsAttackTap { get; private set; }

    // 마우스 휠 입력
    public float ScrollY { get; private set; }

    // 숫자키 모드 변경 입력
    public bool IsVocalKeyPressed { get; private set; }
    public bool IsDanceKeyPressed { get; private set; }
    public bool IsVisualKeyPressed { get; private set; }

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 1. 이동 입력 (WASD)
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        MoveDir = new Vector2(x, y).normalized;

        // 2. 마우스 위치 (월드 좌표)
        if (mainCam != null)
            MousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // 3. 액션 키 입력
        IsDashTriggered = Input.GetKeyDown(KeyCode.LeftShift);
        IsAttackHeld = Input.GetMouseButton(0);
        IsAttackTap = Input.GetMouseButtonDown(0);
        IsUltTriggered = Input.GetMouseButtonDown(1); // 우클릭 스킬

        // 4. 마우스 휠 입력
        ScrollY = Input.mouseScrollDelta.y;

        // 5. 숫자키 모드 변경 (1: Vocal, 2: Dance, 3: Visual)
        IsVocalKeyPressed = Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
        IsDanceKeyPressed = Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
        IsVisualKeyPressed = Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
    }
}