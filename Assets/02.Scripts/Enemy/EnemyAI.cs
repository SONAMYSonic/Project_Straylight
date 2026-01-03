using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _baseMoveSpeed = 2.5f;
    [SerializeField] private float _stopDistance = 0.5f;

    private float _currentMoveSpeed;
    private Transform _target;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentMoveSpeed = _baseMoveSpeed;
    }

    private void OnEnable()
    {
        FindPlayer();
        _currentMoveSpeed = _baseMoveSpeed;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _target = playerObj.transform;
        }
    }

    /// <summary>
    /// 웨이브 스케일링용 이동속도 배율 적용
    /// </summary>
    public void ApplySpeedMultiplier(float multiplier)
    {
        _currentMoveSpeed = _baseMoveSpeed * multiplier;
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        Vector2 direction = (_target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, _target.position);

        if (distance > _stopDistance)
        {
            Vector2 newPos = _rb.position + direction * _currentMoveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(newPos);
        }

        UpdateFacing(direction);
    }

    private void UpdateFacing(Vector2 direction)
    {
        if (_spriteRenderer != null && direction.x != 0)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }
    }
}