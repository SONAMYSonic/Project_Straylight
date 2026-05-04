using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    private Animator _animator;
    private PlayerInputReader _input;
    private PlayerCombat _combat;
    private Rigidbody2D _rb;

    // 마지막으로 바라본 방향 저장 (타겟이 없고 가만히 있을 때 유지)
    private Vector2 _lastLookDir = Vector2.down;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _input = GetComponent<PlayerInputReader>();
        _combat = GetComponent<PlayerCombat>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_animator == null) return;

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // 1. 이동 중인지 확인
        bool isMoving = _input.MoveDir.magnitude > 0.1f;
        _animator.SetBool("IsMoving", isMoving);

        // 2. 바라보는 방향 결정 (우선순위: 자동 타겟 > 이동 방향 > 직전 방향)
        Vector2 lookDir = _lastLookDir;
        Transform target = _combat != null ? _combat.CurrentTarget : null;

        if (target != null)
        {
            lookDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        }
        else if (isMoving)
        {
            lookDir = _input.MoveDir.normalized;
        }

        if (lookDir.sqrMagnitude > 0.01f) _lastLookDir = lookDir;

        // 3. 애니메이터 파라미터 전달
        _animator.SetFloat("LookX", lookDir.x);
        _animator.SetFloat("LookY", lookDir.y);
    }
}