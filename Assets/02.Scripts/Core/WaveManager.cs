using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;

public class WaveManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private BoxCollider2D _spawnArea; // 적이 생성될 범위
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _safeDistance = 5.0f; // 플레이어 안전 반경

    [Header("Wave Config")]
    [SerializeField] private List<GameObject> _enemyPrefabs; // [수정] 배열보다 리스트가 관리하기 편함
    [SerializeField] private float _spawnInterval = 2.0f;

    // 내부 상태 변수
    private bool _isWaveActive = true;

    // 필터링 규칙 (보스전용)
    private bool _useFilter = false;
    private IdolMode _targetMode = IdolMode.None;
    private bool _isInclusive = true; // true: 이것만 소환, false: 이것 빼고 소환

    private void Start()
    {
        if (_spawnArea == null)
            Debug.LogError("[WaveManager] Spawn Area(BoxCollider2D)가 할당되지 않았습니다!");

        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true) // 코루틴은 계속 돌되, active 상태만 체크
        {
            if (_isWaveActive)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Count == 0 || _spawnArea == null) return;

        // 1. 소환할 적 후보군 선정 (필터링 적용)
        GameObject prefabToSpawn = GetFilteredEnemy();

        if (prefabToSpawn == null)
        {
            // 필터링 결과 소환할 적이 없으면(예: 해당 속성 적이 아예 없으면) 그냥 아무거나 소환하거나 패스
            return;
        }

        // 2. 랜덤 위치 계산 (안전거리 확보)
        Vector2 spawnPos = GetSafeRandomPosition();

        // 3. 적 생성
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }

    // [핵심] 조건에 맞는 적을 골라내는 로직
    private GameObject GetFilteredEnemy()
    {
        // 필터가 없으면 전체 중 랜덤
        if (!_useFilter)
        {
            return _enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)];
        }

        // 필터가 있으면 조건에 맞는 후보만 추림
        List<GameObject> candidates = new List<GameObject>();

        foreach (var prefab in _enemyPrefabs)
        {
            if (prefab == null) continue;

            // 프리팹에서 속성 정보 가져오기
            if (prefab.TryGetComponent(out Enemy enemyScript))
            {
                bool isMatch = (enemyScript.EnemyAttribute == _targetMode);

                if (_isInclusive) // "이 속성만 소환해" (Tier 0 보스)
                {
                    if (isMatch) candidates.Add(prefab);
                }
                else // "이 속성 빼고 소환해" (Tier 1, 2 보스)
                {
                    if (!isMatch) candidates.Add(prefab);
                }
            }
        }

        // 후보가 하나도 없으면 꽝 (null 리턴)
        if (candidates.Count == 0) return null;

        // 후보 중 랜덤 선택
        return candidates[Random.Range(0, candidates.Count)];
    }

    private Vector2 GetSafeRandomPosition()
    {
        Vector2 spawnPos = GetRandomPosition();

        if (_playerTransform == null) return spawnPos;

        int attempts = 0;
        // 플레이어와 너무 가까우면 다시 뽑기 (최대 10회 시도)
        while (Vector2.Distance(spawnPos, _playerTransform.position) < _safeDistance && attempts < 10)
        {
            spawnPos = GetRandomPosition();
            attempts++;
        }
        return spawnPos;
    }

    private Vector2 GetRandomPosition()
    {
        Bounds bounds = _spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }

    // --- 외부 제어 메서드 ---

    public void StopWave()
    {
        _isWaveActive = false;
    }

    public void ResumeWave()
    {
        _isWaveActive = true;
    }

    // GameManager에서 호출할 함수: 소환 규칙 설정
    public void SetSpawnRule(IdolMode mode, bool onlyThisMode)
    {
        _targetMode = mode;
        _isInclusive = onlyThisMode;
        _useFilter = true;

        // 보스전 중에도 잡몹이 나와야 하므로 웨이브 재개
        _isWaveActive = true;

        Debug.Log($"[WaveManager] 규칙 변경: {mode} 모드 {(onlyThisMode ? "전용" : "제외")} 소환");
    }

    // 규칙 초기화 (필요 시 사용)
    public void ResetSpawnRule()
    {
        _useFilter = false;
    }
}