using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _hitCooldown = 1.0f; // 연속 히트 방지

    private float _lastHitTime = -10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time < _lastHitTime + _hitCooldown) return;

        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(_damage);
                _lastHitTime = Time.time;
            }
        }
    }
}