using System.Collections;
using System.Collections.Generic; // List 사용을 위해 필수
using UnityEngine;
using IdolMasterFanGame;

// Enemy를 상속받아 기본 기능(체력, 피격연출) 사용
public class MidBossJudge : Enemy
{
    // [추가됨] 속성별 스프라이트 데이터 구조체
    [System.Serializable]
    public struct BossVisualData
    {
        public IdolMode Mode;
        public Sprite BossBodySprite;    // 보스 본체 이미지
        public Sprite ProjectileSprite;  // 던지는 말풍선 이미지
    }

    [Header("Visual Settings")]
    [Tooltip("속성별 보스/투사체 이미지를 여기에 등록하세요")]
    [SerializeField] private List<BossVisualData> _visualDataList; // 인스펙터에서 설정

    [Header("Boss Specific")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _attackCooldown = 3f;

    [Header("Pattern - Dash")]
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _backStepDistance = 1.0f;

    [Header("Pattern - Shoot")]
    [SerializeField] private GameObject _projectilePrefab;
    private Transform _firePoint;

    // 내부 변수
    private int _difficultyTier = 0;
    private Transform _playerTransform;
    private Sprite _currentProjectileSprite; // 현재 모드에 맞는 투사체 이미지 캐싱

    // 초기화 (GameManager에서 소환할 때 호출)
    public void InitBoss(int tier, IdolMode mode)
    {
        _difficultyTier = tier;
        EnemyAttribute = mode;

        // 난이도 강화
        _dashSpeed += tier * 3f;
        _dashDuration += tier * 0.1f;

        // [핵심] 모드에 맞는 스프라이트 적용
        ApplyVisuals(mode);
    }

    private void ApplyVisuals(IdolMode mode)
    {
        // 리스트에서 해당 모드 데이터 찾기
        // (람다식: 리스트 안에 있는 x의 Mode가 매개변수 mode와 같은지 확인)
        BossVisualData data = _visualDataList.Find(x => x.Mode == mode);

        // 데이터가 없으면(구조체 기본값인 경우) 리턴
        if (data.Mode == IdolMode.None && mode != IdolMode.None)
        {
            // Debug.LogWarning($"[MidBossJudge] {mode}에 대한 Visual Data가 없습니다!");
            return;
        }

        // 1. 보스 본체 이미지 교체
        if (data.BossBodySprite != null)
        {
            _spriteRenderer.sprite = data.BossBodySprite;
            // 원본 색상 업데이트 (피격 플래시가 정상 작동하도록)
            UpdateOriginalColor(Color.white);
        }

        // 2. 투사체 이미지 캐싱 (나중에 발사할 때 씀)
        _currentProjectileSprite = data.ProjectileSprite;
    }

    private void Start()
    {
        _firePoint = transform;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        StartCoroutine(BossLogicRoutine());
    }

    // (TakeDamage 등 나머지 코드는 기존과 동일하므로 생략하지 않고 전체 흐름 유지)
    public override void TakeDamage(int baseDamage, IdolMode attackerMode)
    {
        float multiplier = (attackerMode == EnemyAttribute) ? 1.0f : 0.5f;
        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

        // 부모의 TakeDamage 호출 (체력 감소 및 피격 연출)
        base.TakeDamage(finalDamage, EnemyAttribute);
    }

    private IEnumerator BossLogicRoutine()
    {
        while (true)
        {
            if (_playerTransform == null) yield break;

            // 1. 추적
            float timer = 0f;
            while (timer < _attackCooldown)
            {
                MoveTowardsPlayer();
                timer += Time.deltaTime;
                yield return null;
            }

            // 2. 패턴 선택
            int pattern = Random.Range(0, 2);

            if (pattern == 0) yield return StartCoroutine(DashPattern());
            else yield return StartCoroutine(ShootPattern());

            // 3. 대기
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 dir = (_playerTransform.position - transform.position).normalized;
        transform.Translate(dir * _moveSpeed * Time.deltaTime);
        _spriteRenderer.flipX = dir.x < 0;
    }

    private IEnumerator DashPattern()
    {
        Vector3 startPos = transform.position;
        Vector3 targetDir = (_playerTransform.position - transform.position).normalized;
        Vector3 backPos = startPos - (targetDir * _backStepDistance);

        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            transform.position = Vector3.Lerp(startPos, backPos, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        Vector3 dashDir = (_playerTransform.position - transform.position).normalized;
        float dashTimer = 0f;

        while (dashTimer < _dashDuration)
        {
            transform.Translate(dashDir * _dashSpeed * Time.deltaTime);
            dashTimer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShootPattern()
    {
        int shotCount = 1 + (_difficultyTier * 2);

        for (int i = 0; i < shotCount; i++)
        {
            if (_projectilePrefab != null)
            {
                GameObject proj = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
                Vector2 dir = (_playerTransform.position - transform.position).normalized;

                if (i > 0)
                {
                    float angle = (i % 2 == 0 ? 1 : -1) * (i * 10f);
                    dir = Quaternion.Euler(0, 0, angle) * dir;
                }

                JudgeProjectile bulletScript = proj.GetComponent<JudgeProjectile>();
                if (bulletScript != null)
                {
                    bulletScript.Init(dir);
                    // [핵심] 여기서 투사체 이미지를 바꿔줍니다.
                    bulletScript.SetVisual(_currentProjectileSprite);
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    protected override void Die()
    {
        // 1. 점수 추가 (부모 기능)
        base.Die();

        // 2. [핵심] 게임 매니저에게 보스 처치 알림
        GameManager.Instance?.OnBossDefeated();
    }
}