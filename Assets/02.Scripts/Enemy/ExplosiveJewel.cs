using System.Collections;
using UnityEngine;

public class ExplosiveJewel : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _explodeDelay = 1.0f; // 날아가는 시간
    [SerializeField] private float _explosionRadius = 2.0f;
    [SerializeField] private int _damage = 20;
    [SerializeField] private float _knockbackForce = 10f;
    [SerializeField] private GameObject _explosionEffectPrefab; // 폭발 이펙트 (파티클)

    private Vector2 _dir;
    private bool _isExploded = false;

    public void Init(Vector2 dir)
    {
        _dir = dir.normalized;
        StartCoroutine(ExplodeRoutine());
    }

    private void Update()
    {
        if (!_isExploded)
        {
            transform.Translate(_dir * _speed * Time.deltaTime);
            transform.Rotate(0, 0, 360 * Time.deltaTime); // 쥬얼이니까 빙글빙글
        }
    }

    private IEnumerator ExplodeRoutine()
    {
        yield return new WaitForSeconds(_explodeDelay);
        Explode();
    }

    private void Explode()
    {
        if (_isExploded) return;
        _isExploded = true;

        // 이펙트 생성
        if (_explosionEffectPrefab != null)
            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        // 범위 데미지 & 넉백
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent(out PlayerHealth health))
                    health.TakeDamage(_damage);

                // 넉백 처리 (Rigidbody2D가 있다면)
                if (hit.TryGetComponent(out Rigidbody2D rb))
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDir * _knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        // 펑 터지고 삭제
        Destroy(gameObject);
    }
}