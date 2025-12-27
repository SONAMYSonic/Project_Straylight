using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    private Animator _animator;
    private PlayerInput _input;
    private Rigidbody2D _rb;

    // 마지막으로 바라본 방향 저장 (마우스를 떼거나 멈췄을 때 유지를 위해)
    private Vector2 _lastLookDir = Vector2.down;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _input = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_animator == null) return;

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // 1. 이동 중인지 확인 (걷기 vs 대기)
        // rb.velocity.magnitude 대신 input.MoveDir를 쓰는 게 더 반응이 빠릅니다.
        bool isMoving = _input.MoveDir.magnitude > 0.1f;
        _animator.SetBool("IsMoving", isMoving);

        // 2. 바라보는 방향 계산 (플레이어 -> 마우스)
        Vector2 mousePos = _input.MousePos;
        Vector2 playerPos = transform.position;
        Vector2 lookDir = (mousePos - playerPos).normalized;

        // 3. 애니메이터 파라미터 전달
        // (블렌드 트리가 이 값을 받아 상하좌우를 결정합니다)
        _animator.SetFloat("LookX", lookDir.x);
        _animator.SetFloat("LookY", lookDir.y);
    }
}