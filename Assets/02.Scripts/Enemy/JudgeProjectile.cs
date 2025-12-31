using System.Collections;
using UnityEngine;
using IdolMasterFanGame;

public class JudgeProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _speed = 10f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _lifeTime = 3f;

    [Header("Impact Feedback")]
    [Tooltip("땅이나 벽에 박힐 때 나는 소리")]
    [SerializeField] private AudioClip _impactSound;
    [Tooltip("땅에 박힐 때 카메라 흔들림 강도")]
    [SerializeField] private float _shakeIntensity = 1.0f;

    private Vector2 _dir;
    private bool _isLanded = false; // 땅에 박혔는지 여부

    public void Init(Vector2 direction)
    {
        _dir = direction.normalized;

        // 1. [회전] 이동 방향을 바라보게 함 (오른쪽이 앞이라고 가정)
        // 가로로 긴 직사각형이라면 이 코드로 인해 진행 방향으로 눕습니다.
        transform.right = _dir;

        // 2. 수명 관리 코루틴 시작 (Invoke 대신 코루틴 사용)
        StartCoroutine(LifeCycleRoutine());
    }

    // 외부에서 이미지 교체 (MidBossJudge에서 호출)
    public void SetVisual(Sprite sprite)
    {
        if (sprite != null)
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white; // 틴트 초기화
        }
    }

    private void Update()
    {
        if (_isLanded) return; // 박혔으면 이동 멈춤

        // 로컬 좌표계 기준 오른쪽(앞)으로 이동
        // Init에서 transform.right를 돌려놨으므로, 로컬 Right로 가면 앞으로 갑니다.
        transform.Translate(Vector3.right * _speed * Time.deltaTime);
    }

    private IEnumerator LifeCycleRoutine()
    {
        // 수명만큼 대기
        yield return new WaitForSeconds(_lifeTime);

        // 아직 살아있다면(플레이어에게 안 맞았다면) 땅에 박힘 처리
        if (!_isLanded)
        {
            Land();
        }
    }

    // "쿵" 하고 박히는 연출
    private void Land()
    {
        if (_isLanded) return;
        _isLanded = true;

        // 1. 쿵 소리 재생
        if (_impactSound != null)
        {
            // 오브젝트가 사라져도 소리는 나게 함
            AudioSource.PlayClipAtPoint(_impactSound, transform.position);
        }

        // 2. 카메라 흔들림
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeCamera(_shakeIntensity);
        }

        // 3. 잠시 후 사라짐 (땅에 박혀있는 모습 0.5초 유지)
        Destroy(gameObject, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isLanded) return; // 이미 박힌 상태면 무시

        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage(_damage);
            }
            // 플레이어 타격 시에는 즉시 파괴 (박히지 않음)
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            // 벽에 닿으면 땅에 박히는 연출 실행
            Land();
        }
    }
}