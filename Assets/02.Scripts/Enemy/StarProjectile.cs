using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;

public class StarProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private int _damage = 15;
    [SerializeField] private float _lifeTime = 5f;

    [Header("Visuals")]
    [SerializeField] private List<Sprite> _sprites;

    private Vector2 _moveDir;

    public void Init(Vector2 direction)
    {
        _moveDir = direction.normalized;

        // 시각적 회전 (화살표처럼 진행 방향 보기)
        float angle = Mathf.Atan2(_moveDir.y, _moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 랜덤 스프라이트
        if (_sprites != null && _sprites.Count > 0)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = _sprites[Random.Range(0, _sprites.Count)];
        }

        Destroy(gameObject, _lifeTime);
    }

    private void Update()
    {
        // [수정] 가장 확실한 방법: 월드 좌표 기준으로 이동 (회전값 무시)
        transform.position += (Vector3)_moveDir * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(_damage);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}