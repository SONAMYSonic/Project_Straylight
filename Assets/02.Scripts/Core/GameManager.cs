using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IdolMasterFanGame;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event Action OnGameOver;

    [Header("Game Settings")]
    [SerializeField] private int _killsPerStage = 10;
    [SerializeField] private int _targetKillCount = 30;

    [Header("UI Settings")]
    [SerializeField] private float[] _progressThresholds = new float[] { 0.25f, 0.50f, 0.75f, 1.0f };

    [Header("Game State")]
    public int KillCount { get; private set; } = 0;
    public int CurrentScore { get; private set; } = 0;
    
    public bool IsGameOver { get; private set; } = false;
    public bool IsBossBattleActive { get; private set; } = false;

    [Header("Boss Rush Settings")]
    [SerializeField] private GameObject _midBossPrefab;
    [SerializeField] private GameObject _finalBossPrefab;
    [SerializeField] private Transform _bossSpawnPoint;

    private List<IdolMode> _bossOrder;
    private int _currentStageIndex = 0;
    private int _currentWaveKills = 0;
    private bool _isFinalBossSpawned = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameSession.StartTime == 0f) GameSession.StartTime = Time.time;
        
        if (_progressThresholds == null || _progressThresholds.Length < 4)
            _progressThresholds = new float[] { 0.25f, 0.50f, 0.75f, 1.0f };

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

    public void OnEnemyKilled(int scoreValue)
    {
        if (IsGameOver) return;

        KillCount++;
        _currentWaveKills++;
        GameSession.TotalKills = KillCount;

        CurrentScore += scoreValue;
        GameSession.TotalScore = CurrentScore;

        CheckGameProgress();
        
        UIManager.Instance?.UpdateKillCount(KillCount, _targetKillCount);
    }

    private void CheckGameProgress()
    {
        if (!IsBossBattleActive)
        {
            UpdateProgressUI();

            if (_currentWaveKills >= _killsPerStage)
            {
                SpawnNextBoss();
            }
        }

        if (!IsBossBattleActive && !_isFinalBossSpawned && KillCount >= _targetKillCount)
        {
            StartBossRush();
        }
    }

    private void StartBossRush()
    {
        _isFinalBossSpawned = true;
        SpawnNextBoss();
    }
    
    private void UpdateProgressUI()
    {
        int safeIndex = Mathf.Clamp(_currentStageIndex, 0, _progressThresholds.Length - 1);
        float startPoint = (safeIndex == 0) ? 0f : _progressThresholds[safeIndex - 1];
        float targetPoint = _progressThresholds[safeIndex];
        float totalProgress = 0f;

        if (IsBossBattleActive) totalProgress = targetPoint;
        else
        {
            float waveRatio = Mathf.Clamp01((float)_currentWaveKills / _killsPerStage);
            totalProgress = Mathf.Lerp(startPoint, targetPoint, waveRatio);
        }

        int visualState = 0;
        if (IsBossBattleActive) visualState = (_currentStageIndex >= 3) ? 2 : 1;

        UIManager.Instance?.UpdateGameProgress(totalProgress, visualState);
    }

    public void SpawnNextBoss()
    {
        IsBossBattleActive = true;
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.StopWave();
        UpdateProgressUI();
        UIManager.Instance?.ShowBossWarning();
        Vector3 spawnPos = _bossSpawnPoint != null ? _bossSpawnPoint.position : Vector3.zero;
        if (_currentStageIndex >= 3) SpawnFinalBoss(spawnPos);
        else SpawnMidBoss(spawnPos);
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
                ConfigureWaveForBoss(currentMode, _currentStageIndex);
            }
        }
    }

    private void SpawnFinalBoss(Vector3 pos)
    {
        if (_finalBossPrefab != null) Instantiate(_finalBossPrefab, pos, Quaternion.identity);
    }

    public void OnBossDefeated()
    {
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
        if (_currentStageIndex >= 4) GameClear();
    }

    private void ConfigureWaveForBoss(IdolMode bossMode, int tier)
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) { /* 규칙 설정 */ }
    }

    public void GameClear()
    {
        Invoke(nameof(LoadEndingScene), 3.0f);
    }

    private void LoadEndingScene()
    {
        SceneManager.LoadScene("EndingScene");
    }

    public void OnPlayerDead()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        GameSession.DeathCount++;
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.StopWave();
        OnGameOver?.Invoke();
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}