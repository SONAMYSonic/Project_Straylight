using UnityEngine;
using IdolMasterFanGame; // 공용 Enum 사용을 위해 필수

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 2f;

    // 외부에서 주입받을 데이터
    private int _damage;
    private IdolMode _bulletMode;
    private Vector2 _direction;

    private void OnEnable()
    {
        // 총알 수명 카운트
        Invoke(nameof(DisableBullet), _lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void Update()
    {
        // 이동 로직
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }

    // [핵심] PlayerCombat에서 총알을 만들자마자 이 함수를 호출해 줘야 함
    public void SetBulletStats(int damage, IdolMode mode)
    {
        _damage = damage;
        _bulletMode = mode;
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction.normalized;
    }

    private void DisableBullet()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                // [수정됨] 데미지와 함께 "총알의 속성"도 같이 전달
                enemy.TakeDamage(_damage, _bulletMode);
            }

            DisableBullet();
        }
        else if (collision.CompareTag("Wall"))
        {
            DisableBullet();
        }
    }
}