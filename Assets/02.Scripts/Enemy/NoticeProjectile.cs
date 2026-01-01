using UnityEngine;

public class NoticeProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 7f;
    [SerializeField] private float _blindDuration = 3.0f; // 화면 가리는 시간

    private Vector2 _dir;

    public void Init(Vector2 dir)
    {
        _dir = dir.normalized;
        Destroy(gameObject, 5f); // 5초 뒤 삭제
    }

    private void Update()
    {
        transform.Translate(_dir * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 데미지는 없지만 화면을 가림
            UIManager.Instance?.ShowMaintenanceNotice(_blindDuration);
            Destroy(gameObject);
        }
    }
}