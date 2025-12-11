using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifeTime = 2f; // 2초 뒤 자동 삭제
    public int damage = 1;

    // 풀링 시스템에서는 Start가 아니라 OnEnable이 "생성 시점"입니다.
    private void OnEnable()
    {
        // 총알이 활성화될 때마다 수명 카운트 시작
        Invoke(nameof(DisableBullet), lifeTime);
    }

    private void OnDisable()
    {
        // 비활성화될 때 예약된 Invoke 취소 (안 하면 재사용될 때 꼬임)
        CancelInvoke();
    }

    void Update()
    {
        // 스스로 앞(위쪽)으로 날아감
        // 2D에서 transform.up은 초록색 화살표(Y축) 방향입니다.
        // 총알 프리팹 회전값에 따라 날아가는 방향이 결정됩니다.
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void DisableBullet()
    {
        gameObject.SetActive(false); // Destroy 대신 비활성화 -> 풀로 돌아감
    }

    // 충돌 처리 (나중에 적 만들면 사용)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 태그가 Enemy인 물체와 부딪혔을 때
        if (collision.CompareTag("Enemy"))
        {
            // 적 스크립트를 가져와서 데미지를 줌
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            DisableBullet(); // 총알은 사라짐
        }
        else if (collision.CompareTag("Wall"))
        {
            DisableBullet();
        }
    }
}