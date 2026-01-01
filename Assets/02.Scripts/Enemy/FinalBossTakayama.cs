using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;

public class FinalBossTakayama : Enemy
{
    private enum BossState { Idle, NoticePattern, JewelPattern, RapidFirePattern, EmoRushPattern }

    [Header("Visuals")]
    [SerializeField] private GameObject _emoTextGroup; // 에모이 텍스트들이 담긴 자식 오브젝트 (빙글빙글 돌릴 것)

    [Header("Pattern - Notice (점검)")]
    [SerializeField] private GameObject _noticePrefab;
    [SerializeField] private float _noticeCooldown = 15f;

    [Header("Pattern - Jewel (폭발)")]
    [SerializeField] private GameObject _jewelPrefab;
    [SerializeField] private int _jewelCount = 3;

    [Header("Pattern - RapidFire (한정연타)")]
    [SerializeField] private GameObject _starPrefab; // UR, SSR 마크 등
    [SerializeField] private int _rapidFireCount = 10;

    [Header("Pattern - EmoRush (에모이병)")]
    [SerializeField] private float _rushSpeed = 8f;
    [SerializeField] private float _rushDuration = 2.0f;

    private Transform _playerTransform;
    private float _patternTimer = 0f;
    private bool _isActing = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        // 에모이 텍스트 그룹은 평소엔 꺼두거나 안 돌림
        if (_emoTextGroup != null) _emoTextGroup.SetActive(false);

        StartCoroutine(BossRoutine());
    }

    private void Update()
    {
        // 평소에 플레이어 천천히 추적
        if (!_isActing && _playerTransform != null)
        {
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            transform.Translate(dir * 1.5f * Time.deltaTime); // 기본 이동 속도
            if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().flipX = dir.x < 0;
        }
    }

    private IEnumerator BossRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.0f); // 패턴 사이 대기

            // 랜덤 패턴 선택
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

    // 패턴 1: 점검 공지 날리기
    private IEnumerator Pattern_Notice()
    {
        Debug.Log("패턴: 긴급 점검!");
        // 텔레그래프 (잠깐 멈칫)
        yield return new WaitForSeconds(0.5f);

        if (_noticePrefab != null)
        {
            GameObject obj = Instantiate(_noticePrefab, transform.position, Quaternion.identity);
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            obj.GetComponent<NoticeProjectile>().Init(dir);
        }
        yield return new WaitForSeconds(1.0f);
    }

    // 패턴 2: 쥬얼 폭탄 던지기
    private IEnumerator Pattern_Jewel()
    {
        Debug.Log("패턴: 쥬얼 폭발!");
        for (int i = 0; i < _jewelCount; i++)
        {
            if (_jewelPrefab != null)
            {
                GameObject obj = Instantiate(_jewelPrefab, transform.position, Quaternion.identity);
                // 플레이어 예상 위치 혹은 약간 랜덤하게
                Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
                dir += Random.insideUnitCircle * 0.2f; // 약간의 오차

                obj.GetComponent<ExplosiveJewel>().Init(dir);
            }
            yield return new WaitForSeconds(0.3f);
        }
        yield return new WaitForSeconds(1.0f);
    }

    // 패턴 3: 한정 연타 (샷건 탄막)
    private IEnumerator Pattern_RapidFire()
    {
        Debug.Log("패턴: 한정 가챠 연타!");
        Vector2 targetDir = (_playerTransform.position - transform.position).normalized;

        for (int i = 0; i < _rapidFireCount; i++)
        {
            if (_starPrefab != null)
            {
                // 부채꼴 발사 (샷건)
                int pellets = 3;
                for (int j = 0; j < pellets; j++)
                {
                    float angle = (j - 1) * 15f; // -15, 0, 15도
                    Vector2 fireDir = Quaternion.Euler(0, 0, angle) * targetDir;

                    GameObject obj = Instantiate(_starPrefab, transform.position, Quaternion.identity);
                    // JudgeProjectile을 재활용하거나 별도 스크립트 사용
                    obj.GetComponent<JudgeProjectile>().Init(fireDir);
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(1.0f);
    }

    // 패턴 4: 에모이병 (보호막 돌진)
    private IEnumerator Pattern_EmoRush()
    {
        Debug.Log("패턴: 에모이병 돌진!");

        // 1. 에모이 텍스트 켜기
        if (_emoTextGroup != null) _emoTextGroup.SetActive(true);

        // 2. 기 모으기
        float chargeTime = 1.0f;
        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            // 텍스트 회전
            if (_emoTextGroup != null) _emoTextGroup.transform.Rotate(0, 0, 360 * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. 플레이어에게 돌진
        Vector2 rushDir = (_playerTransform.position - transform.position).normalized;
        float rushTimer = 0f;

        while (rushTimer < _rushDuration)
        {
            // 돌진 중에도 회전
            if (_emoTextGroup != null) _emoTextGroup.transform.Rotate(0, 0, 720 * Time.deltaTime); // 더 빠르게 회전

            transform.Translate(rushDir * _rushSpeed * Time.deltaTime);
            rushTimer += Time.deltaTime;
            yield return null;
        }

        // 4. 종료
        if (_emoTextGroup != null) _emoTextGroup.SetActive(false);
        yield return new WaitForSeconds(1.0f);
    }

    // 보스 사망 시
    protected override void Die()
    {
        base.Die();
        Debug.Log("타카야마 격파! 엔딩 크레딧으로...");
        // TODO: 엔딩 씬 로드 또는 클리어 UI 호출
    }
}