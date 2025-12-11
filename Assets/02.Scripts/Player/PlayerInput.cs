using UnityEngine;

// 책임: 사용자의 입력을 감지하고 값을 제공하기만 함
public class PlayerInput : MonoBehaviour
{
    public Vector2 MoveDir { get; private set; }
    public Vector2 MousePos { get; private set; }
    public bool IsDashTriggered { get; private set; }
    public bool IsFireHeld { get; private set; } // 연사 가능
    public bool IsMeleeTriggered { get; private set; }
    public bool IsUltTriggered { get; private set; }

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
        IsDashTriggered = Input.GetKeyDown(KeyCode.RightShift);
        IsFireHeld = Input.GetMouseButton(0); // 0: 좌클릭
        IsMeleeTriggered = Input.GetMouseButtonDown(1); // 1: 우클릭
        IsUltTriggered = Input.GetKeyDown(KeyCode.Space);
    }
}