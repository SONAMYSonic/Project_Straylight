using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;
using TMPro;

public class FinalBossTakayama : Enemy
{
    private enum BossState { Idle, NoticePattern, JewelPattern, RapidFirePattern, EmoRushPattern }

    [Header("Final Boss Stats")]
    [Tooltip("최종보스 체력")]
    [SerializeField] private int _finalBossHealth = 500;

    [Header("Final Boss Settings")]
    [SerializeField] private float _attributeChangeInterval = 5.0f; // 속성 바뀌는 시간
    [SerializeField] private GameObject _shieldEffect; // 방어막 비주얼 (자식 오브젝트)
    [SerializeField] private float _moveSpeed = 2f; // 이동 속도

    [Header("Movement Bounds")]
    [Tooltip("이동 제한 영역 (BoxCollider2D 또는 직접 설정)")]
    [SerializeField] private BoxCollider2D _movementBounds;
    [SerializeField] private Vector2 _boundsMin = new Vector2(-20f, -10f);
    [SerializeField] private Vector2 _boundsMax = new Vector2(20f, 10f);

    [Header("Visuals")]
    [SerializeField] private GameObject _emoTextGroup;
    [SerializeField] private GameObject _blockText; // 속성 다를 시 "BLOCK!" 텍스트

    [Header("Pattern - Notice")]
    [SerializeField] private GameObject _noticePrefab;

    [Header("Pattern - Jewel")]
    [SerializeField] private GameObject _jewelPrefab;
    [SerializeField] private int _jewelCount = 4;

    [Header("Pattern - Star (Rapid)")]
    [SerializeField] private GameObject _starPrefab; // StarProjectile이 붙은 프리팹
    [SerializeField] private int _waves = 5; // 탄막 몇 번 쏠지
    [SerializeField] private int _projectilesPerWave = 7; // 한 번에 몇 발 쏠지
    [SerializeField] private float _angleStep = 20f; // 탄막 사이 각도

    [Header("Pattern - EmoRush")]
    [SerializeField] private float _rushSpeed = 10f;
    [SerializeField] private float _rushDuration = 3f;

    [Header("Pattern Timing")]
    [SerializeField] private float _patternInterval = 2.0f; // 패턴 간격

    private Transform _playerTransform;
    private bool _isActing = false;
    private Coroutine _attributeRoutine;
    private Coroutine _bossRoutine;

    private void OnEnable()
    {
        // 최종보스 HP 설정
        SetHealth(_finalBossHealth);
        Debug.Log($"[FinalBoss] HP 설정: {_finalBossHealth}");

        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        // 이동 범위 설정
        InitializeMovementBounds();

        // 비주얼 초기화
        if (_emoTextGroup != null) _emoTextGroup.SetActive(false);
        if (_shieldEffect != null) _shieldEffect.SetActive(false);
        if (_blockText != null) _blockText.SetActive(false);

        // 코루틴 시작
        _bossRoutine = StartCoroutine(BossRoutine());
        _attributeRoutine = StartCoroutine(ChangeAttributeRoutine());
    }

    private void InitializeMovementBounds()
    {
        if (_movementBounds != null)
        {
            Bounds bounds = _movementBounds.bounds;
            _boundsMin = bounds.min;
            _boundsMax = bounds.max;
        }
    }

    private void OnDisable()
    {
        // 코루틴 정리
        if (_bossRoutine != null) StopCoroutine(_bossRoutine);
        if (_attributeRoutine != null) StopCoroutine(_attributeRoutine);
    }

    private void Update()
    {
        if (!_isActing && _playerTransform != null)
        {
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            transform.Translate(dir * _moveSpeed * Time.deltaTime);

            // 이동 범위 제한
            ClampPosition();

            if (_spriteRenderer != null)
                _spriteRenderer.flipX = dir.x < 0;
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, _boundsMin.x, _boundsMax.x);
        pos.y = Mathf.Clamp(pos.y, _boundsMin.y, _boundsMax.y);
        transform.position = pos;
    }

    // --- [핵심] 속성 변경 로직 ---
    private IEnumerator ChangeAttributeRoutine()
    {
        while (true)
        {
            // 랜덤 속성 선택
            IdolMode[] modes = { IdolMode.Vocal, IdolMode.Dance, IdolMode.Visual };
            EnemyAttribute = modes[Random.Range(0, modes.Length)];

            // 시각적 알림 (색상 변경)
            UpdateColorByMode(EnemyAttribute);

            yield return new WaitForSeconds(_attributeChangeInterval);
        }
    }

    private void UpdateColorByMode(IdolMode mode)
    {
        // Enemy.cs의 UpdateOriginalColor 사용
        Color color = Color.white;
        switch (mode)
        {
            case IdolMode.Vocal: color = new Color(1f, 0.4f, 0.7f); break; // 핑크
            case IdolMode.Dance: color = new Color(0.2f, 0.6f, 1f); break; // 블루
            case IdolMode.Visual: color = new Color(1f, 0.9f, 0.2f); break; // 옐로우
        }
        UpdateOriginalColor(color);
    }

    // --- [핵심] 데미지 무효화 로직 (Override) ---
    public override void TakeDamage(int baseDamage, IdolMode attackerMode)
    {
        // 1. 속성이 다르면 데미지 0 (무적)
        if (attackerMode != EnemyAttribute)
        {
            StartCoroutine(ShowBlockEffect());
            return; // 부모의 TakeDamage를 부르지 않음 -> 체력 안 깎임
        }

        // 2. 속성이 같으면 정상 데미지
        base.TakeDamage(baseDamage, attackerMode);
    }

    private IEnumerator ShowBlockEffect()
    {
        // 방어막 표시
        if (_shieldEffect != null) _shieldEffect.SetActive(true);

        // "BLOCK!" 텍스트 표시
        if (_blockText != null)
        {
            _blockText.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

        if (_shieldEffect != null) _shieldEffect.SetActive(false);
        if (_blockText != null) _blockText.SetActive(false);
    }

    // --- 패턴 로직 ---

    private IEnumerator BossRoutine()
    {
        // 초기 대기
        yield return new WaitForSeconds(_patternInterval);

        while (true)
        {
            if (_playerTransform == null)
            {
                yield return null;
                continue;
            }

            int pattern = Random.Range(0, 4);
            _isActing = true;

            switch (pattern)
            {
                case 0: yield return StartCoroutine(Pattern_Notice()); break;
                case 1: yield return StartCoroutine(Pattern_Jewel()); break;
                case 2: yield return StartCoroutine(Pattern_RapidFire()); break;
                case 3: yield return StartCoroutine(Pattern_EmoRush()); break;
            }

            _isActing = false;

            yield return new WaitForSeconds(_patternInterval);
        }
    }

    private IEnumerator Pattern_Notice()
    {
        // (기존 코드와 동일)
        if (_noticePrefab != null && _playerTransform != null)
        {
            GameObject obj = Instantiate(_noticePrefab, transform.position, Quaternion.identity);
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            
            var projectile = obj.GetComponent<NoticeProjectile>();
            if (projectile != null) projectile.Init(dir);
        }
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Pattern_Jewel()
    {
        // (기존 코드와 동일)
        for (int i = 0; i < _jewelCount; i++)
        {
            if (_jewelPrefab != null && _playerTransform != null)
            {
                GameObject obj = Instantiate(_jewelPrefab, transform.position, Quaternion.identity);
                Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
                dir += Random.insideUnitCircle * 0.2f;
                
                var jewel = obj.GetComponent<ExplosiveJewel>();
                if (jewel != null) jewel.Init(dir);
            }
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(1.0f);
    }

    // [수정됨] 방사형 탄막 (샷건)
    private IEnumerator Pattern_RapidFire()
    {
        Debug.Log("패턴: 한정 가챠 탄막!");

        for (int w = 0; w < _waves; w++)
        {
            if (_starPrefab == null || _playerTransform == null) break;

            Vector2 targetDir = (_playerTransform.position - transform.position).normalized;
            float startAngle = -(_projectilesPerWave - 1) * _angleStep * 0.5f;

            for (int i = 0; i < _projectilesPerWave; i++)
            {
                float currentAngle = startAngle + (i * _angleStep);
                Vector2 fireDir = Quaternion.Euler(0, 0, currentAngle) * targetDir;

                GameObject obj = Instantiate(_starPrefab, transform.position, Quaternion.identity);
                var star = obj.GetComponent<StarProjectile>();
                if (star != null) star.Init(fireDir);
            }

            // 웨이브 간격
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Pattern_EmoRush()
    {
        if (_emoTextGroup != null) _emoTextGroup.SetActive(true);
        float chargeTime = 1.0f;
        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            if (_emoTextGroup != null) _emoTextGroup.transform.Rotate(0, 0, 360 * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_playerTransform != null)
        {
            Vector2 rushDir = (_playerTransform.position - transform.position).normalized;
            float rushTimer = 0f;
            while (rushTimer < _rushDuration)
            {
                if (_emoTextGroup != null) _emoTextGroup.transform.Rotate(0, 0, 720 * Time.deltaTime);
                transform.Translate(rushDir * _rushSpeed * Time.deltaTime);
                
                // 돌진 중에도 이동 범위 제한
                ClampPosition();
                
                rushTimer += Time.deltaTime;
                yield return null;
            }
        }

        if (_emoTextGroup != null) _emoTextGroup.SetActive(false);
        yield return new WaitForSeconds(1.0f);
    }

    protected override void Die()
    {
        if (_attributeRoutine != null) StopCoroutine(_attributeRoutine);
        if (_bossRoutine != null) StopCoroutine(_bossRoutine);
        
        base.Die(); // 점수 추가 및 비활성화

        // [추가] 게임 클리어 트리거
        GameManager.Instance?.GameClear();
    }
}