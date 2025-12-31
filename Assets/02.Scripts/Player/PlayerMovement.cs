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
    [SerializeField] private string ghostTag = "PlayerGhost";
    [SerializeField] private float ghostSpawnInterval = 0.05f;

    private Rigidbody2D rb;
    private PlayerInput input;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private PlayerAudio _playerAudio; // [추가] 오디오 참조

    private bool isDashing = false;
    private float lastDashTime = -10f;

    public bool IsDashing => isDashing;

    public void Initialize(PlayerInput inputRef)
    {
        rb = GetComponent<Rigidbody2D>();
        input = inputRef;
        playerHealth = GetComponent<PlayerHealth>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // [추가] 오디오 컴포넌트 가져오기
        _playerAudio = GetComponent<PlayerAudio>();
    }

    public void HandleMovement()
    {
        if (isDashing) return;

        rb.linearVelocity = input.MoveDir * moveSpeed;

        if (input.MousePos != Vector2.zero)
        {
            rb.rotation = 0f;
        }
    }

    public void HandleDash()
    {
        // 쿨타임과 상태 체크 후 통과되면 코루틴 실행
        if (input.IsDashTriggered && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // [추가] 실제 대쉬가 시작될 때 소리 재생 (쿨타임 걸리면 여기 못 들어옴)
        _playerAudio?.PlayDashVoice();

        Vector2 dashDir;
        if (input.MoveDir != Vector2.zero) dashDir = input.MoveDir.normalized;
        else dashDir = (input.MousePos - rb.position).normalized;

        if (playerHealth != null) playerHealth.SetInvincible(true);
        rb.linearVelocity = dashDir * dashSpeed;

        StartCoroutine(SpawnGhostRoutine());

        Debug.Log("Dash Start!");

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        if (playerHealth != null) playerHealth.SetInvincible(false);
        Debug.Log("Dash End");
    }

    private IEnumerator SpawnGhostRoutine()
    {
        while (isDashing)
        {
            GameObject ghostObj = ObjectPooler.Instance.SpawnFromPool(ghostTag, transform.position, transform.rotation);

            if (ghostObj != null && spriteRenderer != null)
            {
                PlayerGhost ghostScript = ghostObj.GetComponent<PlayerGhost>();
                if (ghostScript != null)
                {
                    ghostScript.SetGhost(spriteRenderer.sprite, spriteRenderer.flipX, transform.localScale);
                }
            }
            yield return new WaitForSeconds(ghostSpawnInterval);
        }
    }
}