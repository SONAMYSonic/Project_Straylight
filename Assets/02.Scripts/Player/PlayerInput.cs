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

    public bool IsAttackHeld { get; private set; } // 총 (연사)
    public bool IsAttackTap { get; private set; }  // 칼 (단발)

    public bool IsSwapVo { get; private set; } // 1번 키
    public bool IsSwapDa { get; private set; } // 2번 키
    public bool IsSwapVi { get; private set; } // 3번 키

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
        IsAttackHeld = Input.GetMouseButton(0);     // 꾹 누르고 있음
        IsAttackTap = Input.GetMouseButtonDown(0);  // 딸깍 클릭함
        IsUltTriggered = Input.GetKeyDown(KeyCode.Space);

        IsSwapVo = Input.GetKeyDown(KeyCode.Alpha1);
        IsSwapDa = Input.GetKeyDown(KeyCode.Alpha2);
        IsSwapVi = Input.GetKeyDown(KeyCode.Alpha3);
    }
}