using System.Collections;
using UnityEngine;

public class ExplosiveJewel : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _explodeDelay = 1.5f; // 사거리 조절
    [SerializeField] private float _explosionRadius = 2.5f;
    [SerializeField] private int _damage = 20;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private GameObject _explosionEffectPrefab;

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
            // [수정] Space.World를 사용하여 회전과 무관하게 지정된 방향으로 이동
            transform.Translate(_dir * _speed * Time.deltaTime, Space.World);

            // [수정] 이동과는 별개로 이미지는 빙글빙글 돔
            transform.Rotate(0, 0, 360 * Time.deltaTime);
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

        if (_explosionEffectPrefab != null)
            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent(out PlayerHealth health))
                    health.TakeDamage(_damage);

                if (hit.TryGetComponent(out Rigidbody2D rb))
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDir * _knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
        Destroy(gameObject);
    }

    // [추가] 벽에 닿으면 즉시 폭발
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isExploded && collision.CompareTag("Wall"))
        {
            Explode();
        }
    }
}