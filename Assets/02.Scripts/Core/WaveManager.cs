using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private BoxCollider2D _spawnArea;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _safeDistance = 5.0f;

    [Header("Enemy Pool Tags")]
    [Tooltip("ObjectPooler에 등록된 적 태그들 (예: EnemyVocal, EnemyDance, EnemyVisual)")]
    [SerializeField] private List<string> _enemyPoolTags;

    [System.Serializable]
    public class EnemyTagMapping
    {
        public string PoolTag;
        public IdolMode Mode;
    }

    [Header("Enemy Tag to Mode Mapping")]
    [SerializeField] private List<EnemyTagMapping> _enemyTagMappings;

    [Header("Spawn Timing")]
    [Tooltip("기본 스폰 주기 (초)")]
    [SerializeField] private float _baseSpawnInterval = 2.0f;
    [Tooltip("웨이브당 스폰 주기 감소 배율 (0.9 = 10% 빨라짐)")]
    [SerializeField] private float _spawnIntervalMultiplier = 0.9f;
    [Tooltip("최소 스폰 주기 (초)")]
    [SerializeField] private float _minSpawnInterval = 0.5f;

    [Header("Difficulty Scaling")]
    [Tooltip("웨이브당 적 HP 배율")]
    [SerializeField] private float _healthMultiplierPerWave = 1.2f;
    [Tooltip("웨이브당 적 이동속도 배율")]
    [SerializeField] private float _speedMultiplierPerWave = 1.05f;

    private int _currentWave = 0;
    private float _currentSpawnInterval;
    private bool _isWaveActive = true;

    // 필터링 규칙 (보스전용)
    private bool _useFilter = false;
    private IdolMode _targetMode = IdolMode.None;
    private bool _isInclusive = true;

    // 태그-모드 매핑 딕셔너리
    private Dictionary<string, IdolMode> _tagToModeMap;

    public int CurrentWave => _currentWave;

    private void Start()
    {
        ValidateReferences();
        FindPlayerIfNeeded();
        BuildTagModeMapping();

        _currentSpawnInterval = _baseSpawnInterval;
        StartCoroutine(SpawnRoutine());
    }

    private void ValidateReferences()
    {
        if (_spawnArea == null)
            Debug.LogError("[WaveManager] Spawn Area(BoxCollider2D)가 할당되지 않았습니다!");

        if (ObjectPooler.Instance == null)
            Debug.LogError("[WaveManager] ObjectPooler.Instance가 없습니다!");
    }

    private void FindPlayerIfNeeded()
    {
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }
    }

    private void BuildTagModeMapping()
    {
        _tagToModeMap = new Dictionary<string, IdolMode>();
        foreach (var mapping in _enemyTagMappings)
        {
            if (!_tagToModeMap.ContainsKey(mapping.PoolTag))
            {
                _tagToModeMap.Add(mapping.PoolTag, mapping.Mode);
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (_isWaveActive)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(_currentSpawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (_enemyPoolTags == null || _enemyPoolTags.Count == 0 || _spawnArea == null) return;
        if (ObjectPooler.Instance == null) return;

        string tagToSpawn = GetFilteredEnemyTag();
        if (string.IsNullOrEmpty(tagToSpawn)) return;

        Vector2 spawnPos = GetSafeRandomPosition();
        GameObject spawnedEnemy = ObjectPooler.Instance.SpawnFromPool(tagToSpawn, spawnPos, Quaternion.identity);

        if (spawnedEnemy != null)
        {
            ApplyWaveScaling(spawnedEnemy);
        }
    }

    private void ApplyWaveScaling(GameObject enemy)
    {
        if (_currentWave <= 0) return;

        // HP 스케일링
        if (enemy.TryGetComponent(out Enemy enemyScript))
        {
            float healthMultiplier = Mathf.Pow(_healthMultiplierPerWave, _currentWave);
            enemyScript.ApplyHealthMultiplier(healthMultiplier);
        }

        // 이동속도 스케일링
        if (enemy.TryGetComponent(out EnemyAI enemyAI))
        {
            float speedMultiplier = Mathf.Pow(_speedMultiplierPerWave, _currentWave);
            enemyAI.ApplySpeedMultiplier(speedMultiplier);
        }
    }

    private string GetFilteredEnemyTag()
    {
        if (!_useFilter)
        {
            return _enemyPoolTags[Random.Range(0, _enemyPoolTags.Count)];
        }

        List<string> candidates = new List<string>();

        foreach (var tag in _enemyPoolTags)
        {
            if (_tagToModeMap.TryGetValue(tag, out IdolMode mode))
            {
                bool isMatch = (mode == _targetMode);

                if (_isInclusive)
                {
                    if (isMatch) candidates.Add(tag);
                }
                else
                {
                    if (!isMatch) candidates.Add(tag);
                }
            }
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private Vector2 GetSafeRandomPosition()
    {
        Vector2 spawnPos = GetRandomPosition();

        if (_playerTransform == null) return spawnPos;

        int attempts = 0;
        int maxAttempts = 10;

        while (Vector2.Distance(spawnPos, _playerTransform.position) < _safeDistance && attempts < maxAttempts)
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

    public void StopWave()
    {
        _isWaveActive = false;
    }

    public void ResumeWave()
    {
        _isWaveActive = true;
    }

    public void AdvanceWave()
    {
        _currentWave++;
        UpdateSpawnInterval();
        Debug.Log($"[WaveManager] 웨이브 {_currentWave} 시작! 스폰 주기: {_currentSpawnInterval:F2}초");
    }

    private void UpdateSpawnInterval()
    {
        _currentSpawnInterval = _baseSpawnInterval * Mathf.Pow(_spawnIntervalMultiplier, _currentWave);
        _currentSpawnInterval = Mathf.Max(_currentSpawnInterval, _minSpawnInterval);
    }

    public void SetSpawnRule(IdolMode mode, bool onlyThisMode)
    {
        _targetMode = mode;
        _isInclusive = onlyThisMode;
        _useFilter = true;
        _isWaveActive = true;

        Debug.Log($"[WaveManager] 규칙 변경: {mode} 모드 {(onlyThisMode ? "전용" : "제외")} 소환");
    }

    public void ResetSpawnRule()
    {
        _useFilter = false;
    }
}