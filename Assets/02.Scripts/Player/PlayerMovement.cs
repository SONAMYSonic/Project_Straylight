using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ghost Trail")]
    [SerializeField] private string ghostTag = "PlayerGhost"; // 풀링 태그
    [SerializeField] private float ghostSpawnInterval = 0.05f; // 잔상 생성 간격

    private Rigidbody2D rb;
    private PlayerInput input;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer; // [추가] 현재 스프라이트 가져오기용

    private bool isDashing = false;
    private float lastDashTime = -10f;

    public bool IsDashing => isDashing;

    public void Initialize(PlayerInput inputRef)
    {
        rb = GetComponent<Rigidbody2D>();
        input = inputRef;
        playerHealth = GetComponent<PlayerHealth>();

        // [추가] 플레이어의 SpriteRenderer 찾기 (없으면 자식에서라도 찾음)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void HandleMovement()
    {
        if (isDashing) return;

        rb.linearVelocity = input.MoveDir * moveSpeed;

        if (input.MousePos != Vector2.zero)
        {
            // 회전값 0으로 고정하여 스프라이트가 기울어지지 않도록 함
            rb.rotation = 0f;
        }
    }

    public void HandleDash()
    {
        if (input.IsDashTriggered && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // 방향 계산
        Vector2 dashDir;
        if (input.MoveDir != Vector2.zero) dashDir = input.MoveDir.normalized;
        else dashDir = (input.MousePos - rb.position).normalized;

        if (playerHealth != null) playerHealth.SetInvincible(true);
        rb.linearVelocity = dashDir * dashSpeed;

        // [추가] 잔상 생성 코루틴 시작 (병렬 실행)
        StartCoroutine(SpawnGhostRoutine());

        Debug.Log("Dash Start!");

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        if (playerHealth != null) playerHealth.SetInvincible(false);
        Debug.Log("Dash End");
    }

    // [추가] 대쉬하는 동안 잔상을 찍어내는 코루틴
    private IEnumerator SpawnGhostRoutine()
    {
        while (isDashing) // 대쉬가 끝날 때까지 반복
        {
            // 1. 오브젝트 풀에서 잔상 가져오기
            GameObject ghostObj = ObjectPooler.Instance.SpawnFromPool(ghostTag, transform.position, transform.rotation);

            // 2. 현재 플레이어의 스프라이트 정보를 잔상에 전달
            if (ghostObj != null && spriteRenderer != null)
            {
                PlayerGhost ghostScript = ghostObj.GetComponent<PlayerGhost>();
                if (ghostScript != null)
                {
                    ghostScript.SetGhost(spriteRenderer.sprite, spriteRenderer.flipX, transform.localScale);
                }
            }

            // 3. 간격만큼 대기
            yield return new WaitForSeconds(ghostSpawnInterval);
        }
    }
}