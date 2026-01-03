using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _baseDashCooldown = 1f;

    [Header("Ghost Trail")]
    [SerializeField] private string _ghostTag = "PlayerGhost";
    [SerializeField] private float _ghostSpawnInterval = 0.05f;

    private Rigidbody2D _rb;
    private PlayerInput _input;
    private PlayerHealth _playerHealth;
    private SpriteRenderer _spriteRenderer;
    private PlayerAudio _playerAudio;

    private bool _isDashing = false;
    private float _lastDashTime = -10f;

    public bool IsDashing => _isDashing;

    private float CurrentDashCooldown => PlayerStats.Instance != null
        ? PlayerStats.Instance.ApplyCooldownReduction(_baseDashCooldown)
        : _baseDashCooldown;

    public void Initialize(PlayerInput inputRef)
    {
        _rb = GetComponent<Rigidbody2D>();
        _input = inputRef;
        _playerHealth = GetComponent<PlayerHealth>();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _playerAudio = GetComponent<PlayerAudio>();
    }

    public void HandleMovement()
    {
        if (_isDashing) return;

        _rb.linearVelocity = _input.MoveDir * _moveSpeed;

        if (_input.MousePos != Vector2.zero)
        {
            _rb.rotation = 0f;
        }
    }

    public void HandleDash()
    {
        if (_input.IsDashTriggered && !_isDashing && Time.time >= _lastDashTime + CurrentDashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        _lastDashTime = Time.time;

        _playerAudio?.PlayDashVoice();

        Vector2 dashDir;
        if (_input.MoveDir != Vector2.zero) dashDir = _input.MoveDir.normalized;
        else dashDir = (_input.MousePos - _rb.position).normalized;

        if (_playerHealth != null) _playerHealth.SetInvincible(true);
        _rb.linearVelocity = dashDir * _dashSpeed;

        StartCoroutine(SpawnGhostRoutine());

        yield return new WaitForSeconds(_dashDuration);

        _rb.linearVelocity = Vector2.zero;
        _isDashing = false;

        if (_playerHealth != null) _playerHealth.SetInvincible(false);
    }

    private IEnumerator SpawnGhostRoutine()
    {
        while (_isDashing)
        {
            GameObject ghostObj = ObjectPooler.Instance?.SpawnFromPool(_ghostTag, transform.position, transform.rotation);

            if (ghostObj != null && _spriteRenderer != null)
            {
                PlayerGhost ghostScript = ghostObj.GetComponent<PlayerGhost>();
                if (ghostScript != null)
                {
                    ghostScript.SetGhost(_spriteRenderer.sprite, _spriteRenderer.flipX, transform.localScale);
                }
            }
            yield return new WaitForSeconds(_ghostSpawnInterval);
        }
    }
}