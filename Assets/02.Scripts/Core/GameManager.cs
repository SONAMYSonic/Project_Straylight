using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IdolMasterFanGame;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // [추가] 게임오버 이벤트 (옵저버 패턴)
    public event Action OnGameOver;

    [Header("Game Settings")]
    [Tooltip("보스를 소환하기 위해 한 스테이지에서 잡아야 할 몬스터 수")]
    [SerializeField] private int _killsPerStage = 10;

    [Header("UI Settings (Progress Bar)")]
    [Tooltip("각 스테이지 보스가 등장하는 게이지 위치 (0.0 ~ 1.0). 4개 입력 필수!")]
    [SerializeField] private float[] _progressThresholds = new float[] { 0.25f, 0.50f, 0.75f, 1.0f };

    [Header("Game State")]
    public int KillCount { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;
    public bool IsBossBattleActive { get; private set; } = false;

    [Header("Boss Rush Settings")]
    [SerializeField] private GameObject _midBossPrefab;
    [SerializeField] private GameObject _finalBossPrefab;
    [SerializeField] private Transform _bossSpawnPoint;

    // 내부 로직 변수
    private List<IdolMode> _bossOrder;
    private int _currentStageIndex = 0; // 0, 1, 2 (중간보스), 3 (최종보스)
    private int _currentWaveKills = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 안전장치: 설정이 없으면 기본값 적용
        if (_progressThresholds == null || _progressThresholds.Length < 4)
        {
            Debug.LogWarning("[GameManager] 진행도 포인트 설정이 부족합니다. 기본값(0.25 단위)을 사용합니다.");
            _progressThresholds = new float[] { 0.25f, 0.50f, 0.75f, 1.0f };
        }

        InitializeBossOrder();
        UpdateProgressUI();
    }

    private void InitializeBossOrder()
    {
        _bossOrder = new List<IdolMode> { IdolMode.Vocal, IdolMode.Dance, IdolMode.Visual };
        _currentStageIndex = 0;
        _currentWaveKills = 0;

        for (int i = 0; i < _bossOrder.Count; i++)
        {
            IdolMode temp = _bossOrder[i];
            int randomIndex = UnityEngine.Random.Range(i, _bossOrder.Count);
            _bossOrder[i] = _bossOrder[randomIndex];
            _bossOrder[randomIndex] = temp;
        }
    }

    public void AddKillCount(int amount = 1)
    {
        if (IsGameOver) return;

        KillCount += amount;

        if (!IsBossBattleActive)
        {
            _currentWaveKills += amount;

            // 게이지 업데이트
            UpdateProgressUI();

            if (_currentWaveKills >= _killsPerStage)
            {
                SpawnNextBoss();
            }
        }

        UIManager.Instance?.UpdateKillCount(KillCount, 0);
    }

    // [핵심 로직 수정] 진행도 계산 로직 개선
    private void UpdateProgressUI()
    {
        // 게임 클리어 상태 등 인덱스 초과 방지
        int safeIndex = Mathf.Clamp(_currentStageIndex, 0, _progressThresholds.Length - 1);

        // 1. 현재 구간의 시작점과 목표점 찾기
        // (0단계일 때는 0부터 시작, 그 외에는 이전 단계의 끝지점부터 시작)
        float startPoint = (safeIndex == 0) ? 0f : _progressThresholds[safeIndex - 1];
        float targetPoint = _progressThresholds[safeIndex];

        float totalProgress = 0f;

        if (IsBossBattleActive)
        {
            // 보스전 중: 해당 단계 목표점까지 꽉 채움
            totalProgress = targetPoint;
        }
        else
        {
            // 웨이브 진행 중: 시작점 ~ 목표점 사이를 비율만큼 채움 (Lerp)
            float waveRatio = Mathf.Clamp01((float)_currentWaveKills / _killsPerStage);
            totalProgress = Mathf.Lerp(startPoint, targetPoint, waveRatio);
        }

        // 상태 결정 (0:일반, 1:중간보스, 2:최종보스)
        int visualState = 0;
        if (IsBossBattleActive)
        {
            visualState = (_currentStageIndex >= 3) ? 2 : 1;
        }

        UIManager.Instance?.UpdateGameProgress(totalProgress, visualState);
    }

    public void SpawnNextBoss()
    {
        IsBossBattleActive = true;

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.StopWave();

        UpdateProgressUI(); // 보스전 돌입 시 게이지 목표치까지 확정 채움
        UIManager.Instance?.ShowBossWarning();

        Vector3 spawnPos = _bossSpawnPoint != null ? _bossSpawnPoint.position : Vector3.zero;

        if (_currentStageIndex >= 3)
        {
            SpawnFinalBoss(spawnPos);
        }
        else
        {
            SpawnMidBoss(spawnPos);
        }
    }

    private void SpawnMidBoss(Vector3 pos)
    {
        if (_midBossPrefab != null)
        {
            GameObject bossObj = Instantiate(_midBossPrefab, pos, Quaternion.identity);
            if (bossObj.TryGetComponent(out MidBossJudge bossScript))
            {
                IdolMode currentMode = _bossOrder[_currentStageIndex];
                bossScript.InitBoss(_currentStageIndex, currentMode);
                Debug.Log($"[GameManager] 중간 보스 {currentMode} 등장! (Stage: {_currentStageIndex})");

                ConfigureWaveForBoss(currentMode, _currentStageIndex);
            }
        }
    }

    private void SpawnFinalBoss(Vector3 pos)
    {
        Debug.Log(">>> 최종 보스 타카야마 등장! <<<");
        if (_finalBossPrefab != null)
        {
            Instantiate(_finalBossPrefab, pos, Quaternion.identity);
        }
    }

    public void OnBossDefeated()
    {
        Debug.Log($"[GameManager] Stage {_currentStageIndex} 클리어!");

        IsBossBattleActive = false;
        _currentWaveKills = 0;
        _currentStageIndex++;

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.ResetSpawnRule();
            waveManager.ResumeWave();
        }

        UpdateProgressUI();
    }

    private void ConfigureWaveForBoss(IdolMode bossMode, int tier)
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager == null) return;

        // waveManager.SetSpawnRule(bossMode, tier == 0); 
    }

    public void OnPlayerDead()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.StopWave();
        
        // [수정] 이벤트 발행 (구독자들에게 알림)
        OnGameOver?.Invoke();
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}