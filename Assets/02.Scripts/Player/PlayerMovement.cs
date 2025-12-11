using System.Collections;
using UnityEngine;

// 책임: 리지드바디를 이용한 실제 이동 및 회피 로직 수행
[RequireComponent(typeof(Rigidbody2D))] // 의존성 명시
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody2D rb;
    private PlayerInput input; // 의존성

    private bool isDashing = false;
    private float lastDashTime = -10f;

    // 외부에서 현재 상태 확인용 프로퍼티
    public bool IsDashing => isDashing;

    public void Initialize(PlayerInput inputRef)
    {
        rb = GetComponent<Rigidbody2D>();
        input = inputRef;
    }

    public void HandleMovement()
    {
        if (isDashing) return; // 대시 중에는 일반 이동 불가

        // 이동 처리
        rb.linearVelocity = input.MoveDir * moveSpeed;

        // 회전 처리 (마우스 바라보기)
        Vector2 lookDir = input.MousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    public void HandleDash()
    {
        // 쿨타임 및 입력 체크
        if (input.IsDashTriggered && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // 이동 중이면 이동 방향으로, 멈춰있으면 마우스 방향으로 대시
        Vector2 dashDir = input.MoveDir != Vector2.zero ? input.MoveDir : (input.MousePos - rb.position).normalized;

        rb.linearVelocity = dashDir * dashSpeed;

        // TODO: 무적 처리 로직 추가 (예: GetComponent<PlayerHealth>().SetInvincible(true))
        Debug.Log("Dash Start!");

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        Debug.Log("Dash End");
    }
}