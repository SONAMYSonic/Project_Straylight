using System.Collections;
using System.Collections.Generic;
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

    [Header("Boss Stats")]
    [Tooltip("기본 HP (Tier에 따라 배율 적용)")]
    [SerializeField] private int _bossBaseHealth = 300;
    [Tooltip("Tier당 HP 배율")]
    [SerializeField] private float _healthMultiplierPerTier = 1.5f;

    [Header("Damage Resistance")]
    [Tooltip("Tier별 받는 데미지 배율 (0: 100%, 1: 75%, 2: 50%)")]
    [SerializeField] private float[] _damageResistanceByTier = new float[] { 1.0f, 0.75f, 0.5f };

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _attackCooldown = 3f;

    [Header("Pattern - Dash")]
    [SerializeField] private float _baseDashSpeed = 15f;
    [SerializeField] private float _dashSpeedPerTier = 3f;
    [SerializeField] private float _baseDashDuration = 0.5f;
    [SerializeField] private float _dashDurationPerTier = 0.1f;
    [SerializeField] private float _backStepDistance = 1.0f;
    [SerializeField] private float _backStepDuration = 0.5f;
    [SerializeField] private float _dashChargeDelay = 0.2f;

    [Header("Pattern - Shoot")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private int _baseShotCount = 1;
    [SerializeField] private int _shotsPerTier = 2;
    [SerializeField] private float _shotInterval = 0.2f;
    [SerializeField] private float _spreadAnglePerShot = 10f;

    [Header("Minion Spawn")]
    [Tooltip("잡몹 스폰 주기 (초)")]
    [SerializeField] private float _minionSpawnInterval = 5f;

    // 내부 변수
    private int _difficultyTier = 0;
    private float _currentDashSpeed;
    private float _currentDashDuration;
    private float _currentDamageResistance = 1f;
    private Transform _playerTransform;
    private Sprite _currentProjectileSprite; // 현재 모드에 맞는 투사체 이미지 캐싱
    private Coroutine _bossRoutine;
    private Coroutine _minionRoutine;

    // 초기화 (GameManager에서 소환할 때 호출)
    public void InitBoss(int tier, IdolMode mode)
    {
        _difficultyTier = tier;
        EnemyAttribute = mode;

        // 난이도 강화
        ApplyTierScaling();
        // [핵심] 모드에 맞는 스프라이트 적용
        ApplyVisuals(mode);
        SetupMinionSpawnRule();
    }

    private void ApplyTierScaling()
    {
        // HP 스케일링 - 보스 전용 HP 사용
        float healthMultiplier = Mathf.Pow(_healthMultiplierPerTier, _difficultyTier);
        int scaledHealth = Mathf.RoundToInt(_bossBaseHealth * healthMultiplier);
        SetHealth(scaledHealth);  // 직접 HP 설정

        // 대시 스케일링
        _currentDashSpeed = _baseDashSpeed + (_difficultyTier * _dashSpeedPerTier);
        _currentDashDuration = _baseDashDuration + (_difficultyTier * _dashDurationPerTier);

        // 데미지 내성 설정
        int resistanceIndex = Mathf.Clamp(_difficultyTier, 0, _damageResistanceByTier.Length - 1);
        _currentDamageResistance = _damageResistanceByTier[resistanceIndex];

        Debug.Log($"[MidBossJudge] Tier {_difficultyTier}: HP={scaledHealth}, 데미지내성={_currentDamageResistance}");
    }

    private void SetupMinionSpawnRule()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager == null) return;

        switch (_difficultyTier)
        {
            case 0:
                // 1번째 보스: 같은 속성만 소환 (방해꾼)
                waveManager.SetSpawnRule(EnemyAttribute, onlyThisMode: true);
                break;
            case 1:
                // 2번째 보스: 모든 속성 소환
                waveManager.ResetSpawnRule();
                break;
            case 2:
                // 3번째 보스: 다른 속성만 소환
                waveManager.SetSpawnRule(EnemyAttribute, onlyThisMode: false);
                break;
        }

        waveManager.ResumeWave();
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
        FindPlayer();
        _bossRoutine = StartCoroutine(BossLogicRoutine());
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    // (TakeDamage 등 나머지 코드는 기존과 동일하므로 생략하지 않고 전체 흐름 유지)
    public override void TakeDamage(int baseDamage, IdolMode attackerMode)
    {
        // 속성 보너스 계산
        float attributeMultiplier = (attackerMode == EnemyAttribute) ? 1.0f : 0.5f;
        
        // 내성 적용
        int finalDamage = Mathf.RoundToInt(baseDamage * attributeMultiplier * _currentDamageResistance);

        // 부모의 TakeDamage 호출 (체력 감소 및 피격 연출)
        base.TakeDamage(finalDamage, EnemyAttribute);
    }

    private IEnumerator BossLogicRoutine()
    {
        while (true)
        {
            if (_playerTransform == null)
            {
                yield return null;
                continue;
            }

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
        if (_playerTransform == null) return;

        Vector2 dir = (_playerTransform.position - transform.position).normalized;
        transform.Translate(dir * _moveSpeed * Time.deltaTime);
        
        if (_spriteRenderer != null)
            _spriteRenderer.flipX = dir.x < 0;
    }

    private IEnumerator DashPattern()
    {
        if (_playerTransform == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 targetDir = (_playerTransform.position - transform.position).normalized;
        Vector3 backPos = startPos - (targetDir * _backStepDistance);

        // 뒤로 물러남
        float elapsed = 0f;
        while (elapsed < _backStepDuration)
        {
            transform.position = Vector3.Lerp(startPos, backPos, elapsed / _backStepDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(_dashChargeDelay);

        // 돌진
        if (_playerTransform == null) yield break;
        Vector3 dashDir = (_playerTransform.position - transform.position).normalized;
        float dashTimer = 0f;

        while (dashTimer < _currentDashDuration)
        {
            transform.Translate(dashDir * _currentDashSpeed * Time.deltaTime);
            dashTimer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShootPattern()
    {
        int shotCount = _baseShotCount + (_difficultyTier * _shotsPerTier);

        for (int i = 0; i < shotCount; i++)
        {
            if (_projectilePrefab == null || _playerTransform == null) break;

            GameObject proj = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            Vector2 dir = (_playerTransform.position - transform.position).normalized;

            // 탄 퍼짐
            if (i > 0)
            {
                float angle = (i % 2 == 0 ? 1 : -1) * (i * _spreadAnglePerShot);
                dir = Quaternion.Euler(0, 0, angle) * dir;
            }

            JudgeProjectile bulletScript = proj.GetComponent<JudgeProjectile>();
            if (bulletScript != null)
            {
                bulletScript.Init(dir);
                bulletScript.SetVisual(_currentProjectileSprite);
            }

            yield return new WaitForSeconds(_shotInterval);
        }
    }

    protected override void Die()
    {
        if (_bossRoutine != null) StopCoroutine(_bossRoutine);
        if (_minionRoutine != null) StopCoroutine(_minionRoutine);

        // 1. 점수 추가 (부모 기능)
        base.Die();

        // 2. [핵심] 게임 매니저에게 보스 처치 알림
        GameManager.Instance?.OnBossDefeated();
    }

    private void OnDisable()
    {
        if (_bossRoutine != null) StopCoroutine(_bossRoutine);
        if (_minionRoutine != null) StopCoroutine(_minionRoutine);
    }
}