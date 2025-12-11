using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 0.5f; // 플레이어와 너무 딱 붙지 않게

    private Transform target;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // 적이 생성(활성화)될 때마다 플레이어를 다시 찾습니다.
        // 태그로 찾는 방식은 간단하지만, 나중에는 GameManager에서 넘겨주는 게 더 좋습니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        // 1. 방향 계산 (플레이어 위치 - 내 위치)
        Vector2 direction = (target.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target.position);

        // 2. 이동 (너무 가까우면 멈춤)
        if (distance > stopDistance)
        {
            // rb.MovePosition을 써야 물리 충돌을 유지하며 움직입니다.
            Vector2 newPos = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }

        // 3. 시선 처리 (왼쪽/오른쪽 바라보기)
        if (direction.x != 0)
        {
            // 플레이어가 오른쪽에 있으면(x > 0) false(원본), 왼쪽이면 true(반전)
            spriteRenderer.flipX = direction.x < 0;
        }
    }
}