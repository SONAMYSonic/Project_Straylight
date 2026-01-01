using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;
using TMPro; // 데미지 텍스트용 (선택)

public class FinalBossTakayama : Enemy
{
    private enum BossState { Idle, NoticePattern, JewelPattern, RapidFirePattern, EmoRushPattern }

    [Header("Final Boss Settings")]
    [SerializeField] private float _attributeChangeInterval = 5.0f; // 속성 바뀌는 시간
    [SerializeField] private GameObject _shieldEffect; // 방어막 비주얼 (자식 오브젝트)

    [Header("Visuals")]
    [SerializeField] private GameObject _emoTextGroup;
    [SerializeField] private TextMeshPro _immuneText; // "BLOCK!" 같은 텍스트 띄울 곳 (선택)

    [Header("Pattern - Notice")]
    [SerializeField] private GameObject _noticePrefab;

    [Header("Pattern - Jewel")]
    [SerializeField] private GameObject _jewelPrefab;
    [SerializeField] private int _jewelCount = 4;

    [Header("Pattern - Star (Rapid)")]
    [SerializeField] private GameObject _starPrefab; // StarProjectile이 붙은 프리팹
    [SerializeField] private int _waves = 5; // 탄막 몇 번 쏠지
    [SerializeField] private int _projectilesPerWave = 7; // 한 번에 몇 발 쏠지
    [SerializeField] private float _angleStep = 15f; // 탄막 사이 각도

    [Header("Pattern - EmoRush")]
    [SerializeField] private float _rushSpeed = 10f;
    [SerializeField] private float _rushDuration = 2.5f;

    private Transform _playerTransform;
    private bool _isActing = false;
    private Coroutine _attributeRoutine;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        if (_emoTextGroup != null) _emoTextGroup.SetActive(false);
        if (_shieldEffect != null) _shieldEffect.SetActive(false);
        if (_immuneText != null) _immuneText.gameObject.SetActive(false);

        // 패턴 루틴 시작
        StartCoroutine(BossRoutine());

        // 속성 변경 루틴 시작
        _attributeRoutine = StartCoroutine(ChangeAttributeRoutine());
    }

    private void Update()
    {
        if (!_isActing && _playerTransform != null)
        {
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            transform.Translate(dir * 1.5f * Time.deltaTime);
            if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().flipX = dir.x < 0;
        }
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

            // [수정] 여기서 쉴드를 껐다 켰다 하지 않음 (시작하자마자 나오는 문제 해결)
            // 대신 플레이어에게 속성이 바뀌었다는 힌트(반짝임 등)를 주고 싶다면 여기서 처리
            // 예: StartCoroutine(FlashBossColor()); 

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
        if (_immuneText != null)
        {
            _immuneText.text = "BLOCK!";
            _immuneText.color = _originalColor; // 현재 보스 색상
            _immuneText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

        if (_shieldEffect != null) _shieldEffect.SetActive(false);
        if (_immuneText != null) _immuneText.gameObject.SetActive(false);
    }

    // --- 패턴 로직 ---

    private IEnumerator BossRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.0f);

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
        }
    }

    private IEnumerator Pattern_Notice()
    {
        // (기존 코드와 동일)
        if (_noticePrefab != null)
        {
            GameObject obj = Instantiate(_noticePrefab, transform.position, Quaternion.identity);
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            obj.GetComponent<NoticeProjectile>().Init(dir);
        }
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Pattern_Jewel()
    {
        // (기존 코드와 동일)
        for (int i = 0; i < _jewelCount; i++)
        {
            if (_jewelPrefab != null)
            {
                GameObject obj = Instantiate(_jewelPrefab, transform.position, Quaternion.identity);
                Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
                dir += Random.insideUnitCircle * 0.2f;
                obj.GetComponent<ExplosiveJewel>().Init(dir);
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
            if (_starPrefab == null) break;

            Vector2 targetDir = (_playerTransform.position - transform.position).normalized;

            // 시작 각도 계산 (부채꼴의 가장 왼쪽)
            float startAngle = -(_projectilesPerWave - 1) * _angleStep * 0.5f;

            for (int i = 0; i < _projectilesPerWave; i++)
            {
                float currentAngle = startAngle + (i * _angleStep);

                // 타겟 방향 기준으로 회전
                Vector2 fireDir = Quaternion.Euler(0, 0, currentAngle) * targetDir;

                GameObject obj = Instantiate(_starPrefab, transform.position, Quaternion.identity);
                obj.GetComponent<StarProjectile>().Init(fireDir);
            }

            // 웨이브 간격
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator Pattern_EmoRush()
    {
        // (기존 코드와 동일, 에모이 텍스트 켜고 돌진)
        if (_emoTextGroup != null) _emoTextGroup.SetActive(true);
        float chargeTime = 1.0f;
        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            if (_emoTextGroup != null) _emoTextGroup.transform.Rotate(0, 0, 360 * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector2 rushDir = (_playerTransform.position - transform.position).normalized;
        float rushTimer = 0f;
        while (rushTimer < _rushDuration)
        {
            if (_emoTextGroup != null) _emoTextGroup.transform.Rotate(0, 0, 720 * Time.deltaTime);
            transform.Translate(rushDir * _rushSpeed * Time.deltaTime);
            rushTimer += Time.deltaTime;
            yield return null;
        }

        if (_emoTextGroup != null) _emoTextGroup.SetActive(false);
        yield return new WaitForSeconds(1.0f);
    }

    protected override void Die()
    {
        if (_attributeRoutine != null) StopCoroutine(_attributeRoutine);
        base.Die();
        Debug.Log("타카야마 격파! 엔딩 크레딧으로...");
    }
}