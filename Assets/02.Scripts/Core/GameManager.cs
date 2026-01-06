using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IdolMasterFanGame;

public class GameManager : MonoBehaviour
{
    private const string TITLE_SCENE_NAME = "TitleScene";
    private const string ENDING_SCENE_NAME = "EndingScene";

    public static GameManager Instance { get; private set; }
    public event Action OnGameOver;
    public event Action<bool> OnPauseChanged;

    [Header("Game Settings")]
    [Tooltip("각 웨이브에서 보스 등장까지 필요한 킬 수 (웨이브별)")]
    [SerializeField] private int[] _killsPerWave = new int[] { 30, 50, 70, 100 };

    [Header("UI Settings")]
    [SerializeField] private float[] _progressThresholds = new float[] { 0.25f, 0.50f, 0.75f, 1.0f };

    [Header("Timing Settings")]
    [Tooltip("게임 클리어 후 엔딩 씬 전환까지 대기 시간 (초)")]
    [SerializeField] private float _endingSceneDelay = 3.0f;

    [Header("Game State")]
    public int KillCount { get; private set; } = 0;
    public int CurrentScore { get; private set; } = 0;
    public int ReviveCount { get; private set; } = 0;
    
    public bool IsGameOver { get; private set; } = false;
    public bool IsBossBattleActive { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    public bool CanRevive => true;

    [Header("Boss Rush Settings")]
    [SerializeField] private GameObject _midBossPrefab;
    [Tooltip("씬에 미리 배치해둔 최종보스 오브젝트 (비활성화 상태)")]
    [SerializeField] private GameObject _finalBossObject;
    [SerializeField] private Transform _bossSpawnPoint;

    [Header("Player Reference")]
    [SerializeField] private GameObject _playerObject;

    [Header("Audio")]
    [Tooltip("게임 씬에서 랜덤 재생될 BGM 목록")]
    [SerializeField] private AudioClip[] _gameBGMPlaylist;

    private List<IdolMode> _bossOrder;
    private int _currentStageIndex = 0;
    private int _waveKillCount = 0;
    private bool _isFinalBossSpawned = false;
    private bool _isWaitingForBuffSelection = false;
    private PlayerInput _playerInput;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeGameSession();
        InitializeThresholds();
        InitializeFinalBoss();
        FindPlayerIfNeeded();
        InitializeBossOrder();
        UpdateProgressUI();
        StartGameBGM();

        // 플레이어 입력 참조
        if (_playerObject != null)
            _playerInput = _playerObject.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        // ESC 키로 일시정지 토글 (게임오버/버프선택 중이 아닐 때만)
        if (_playerInput != null && _playerInput.IsPauseTriggered)
        {
            if (!IsGameOver && !_isWaitingForBuffSelection)
            {
                TogglePause();
            }
        }
    }

    // --- 일시정지 시스템 ---

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        OnPauseChanged?.Invoke(true);
        UIManager.Instance?.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        OnPauseChanged?.Invoke(false);
        UIManager.Instance?.HidePauseMenu();
    }

    // --- 기존 코드 ---

    private void InitializeGameSession()
    {
        if (GameSession.StartTime == 0f) GameSession.StartTime = Time.time;
    }

    private void InitializeThresholds()
    {
        if (_progressThresholds == null || _progressThresholds.Length < 4)
            _progressThresholds = new float[] { 0.25f, 0.50f, 0.75f, 1.0f };

        if (_killsPerWave == null || _killsPerWave.Length < 4)
            _killsPerWave = new int[] { 30, 50, 70, 100 };
    }

    private void InitializeFinalBoss()
    {
        if (_finalBossObject != null) _finalBossObject.SetActive(false);
    }

    private void FindPlayerIfNeeded()
    {
        if (_playerObject == null)
        {
            _playerObject = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void InitializeBossOrder()
    {
        _bossOrder = new List<IdolMode> { IdolMode.Vocal, IdolMode.Dance, IdolMode.Visual };
        _currentStageIndex = 0;
        _waveKillCount = 0;

        ShuffleBossOrder();
    }

    private void ShuffleBossOrder()
    {
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

        IncrementKillCount();
        AddScore(scoreValue);
        CheckBossSpawn();
        
        UIManager.Instance?.UpdateKillCount(_waveKillCount, GetCurrentWaveKillTarget());
    }

    private void IncrementKillCount()
    {
        KillCount++;
        _waveKillCount++;
        GameSession.TotalKills = KillCount;
    }

    private void AddScore(int scoreValue)
    {
        CurrentScore += scoreValue;
        GameSession.TotalScore = CurrentScore;
    }

    private int GetCurrentWaveKillTarget()
    {
        if (_currentStageIndex >= _killsPerWave.Length)
            return _killsPerWave[_killsPerWave.Length - 1];
        return _killsPerWave[_currentStageIndex];
    }

    private void CheckBossSpawn()
    {
        if (IsBossBattleActive || _isFinalBossSpawned) return;

        int targetKills = GetCurrentWaveKillTarget();

        if (_waveKillCount >= targetKills)
        {
            SpawnNextBoss();
        }
        else
        {
            UpdateProgressUI();
        }
    }

    private void UpdateProgressUI()
    {
        int safeIndex = Mathf.Clamp(_currentStageIndex, 0, _progressThresholds.Length - 1);
        float startPoint = (safeIndex == 0) ? 0f : _progressThresholds[safeIndex - 1];
        float targetPoint = _progressThresholds[safeIndex];
        float totalProgress = CalculateProgress(startPoint, targetPoint);

        int visualState = DetermineVisualState();

        UIManager.Instance?.UpdateGameProgress(totalProgress, visualState);
    }

    private float CalculateProgress(float startPoint, float targetPoint)
    {
        if (IsBossBattleActive) return targetPoint;

        int targetKills = GetCurrentWaveKillTarget();
        float waveRatio = Mathf.Clamp01((float)_waveKillCount / targetKills);
        return Mathf.Lerp(startPoint, targetPoint, waveRatio);
    }

    private int DetermineVisualState()
    {
        if (!IsBossBattleActive) return 0;
        return (_currentStageIndex >= 3) ? 2 : 1;
    }

    public void SpawnNextBoss()
    {
        IsBossBattleActive = true;
        StopWaveSpawning();
        UpdateProgressUI();
        UIManager.Instance?.ShowBossWarning();

        Vector3 spawnPos = _bossSpawnPoint != null ? _bossSpawnPoint.position : Vector3.zero;

        if (_currentStageIndex >= 3)
        {
            _isFinalBossSpawned = true;
            ActivateFinalBoss(spawnPos);
        }
        else
        {
            SpawnMidBoss(spawnPos);
        }
    }

    private void StopWaveSpawning()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.StopWave();
    }

    private void SpawnMidBoss(Vector3 pos)
    {
        if (_midBossPrefab == null) return;

        GameObject bossObj = Instantiate(_midBossPrefab, pos, Quaternion.identity);
        if (bossObj.TryGetComponent(out MidBossJudge bossScript))
        {
            IdolMode currentMode = _bossOrder[_currentStageIndex];
            bossScript.InitBoss(_currentStageIndex, currentMode);
            ConfigureWaveForBoss(currentMode, _currentStageIndex);
        }
    }

    private void ActivateFinalBoss(Vector3 pos)
    {
        if (_finalBossObject == null) return;

        _finalBossObject.transform.position = pos;
        _finalBossObject.SetActive(true);
    }

    public void OnBossDefeated()
    {
        IsBossBattleActive = false;
        _currentStageIndex++;
        _waveKillCount = 0;

        if (_currentStageIndex >= 4)
        {
            GameClear();
            return;
        }

        ShowBuffSelectionUI();
    }

    private void ShowBuffSelectionUI()
    {
        _isWaitingForBuffSelection = true;
        Time.timeScale = 0f;  // 버프 선택 중 일시정지
        UIManager.Instance?.ShowBuffSelection();
    }

    public void ResumeAfterBuffSelection()
    {
        _isWaitingForBuffSelection = false;
        Time.timeScale = 1f;  // 버프 선택 후 재개

        AdvanceWave();
        ResumeWaveSpawning();
        UpdateProgressUI();
    }

    private void AdvanceWave()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) waveManager.AdvanceWave();
    }

    private void ResumeWaveSpawning()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.ResetSpawnRule();
            waveManager.ResumeWave();
        }
    }

    private void ConfigureWaveForBoss(IdolMode bossMode, int tier)
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null) { /* 규칙 설정 */ }
    }

    public void GameClear()
    {
        GameSession.FinalizePlayTime();
        Invoke(nameof(LoadEndingScene), _endingSceneDelay);
    }

    private void LoadEndingScene()
    {
        SceneManager.LoadScene(ENDING_SCENE_NAME);
    }

    public void OnPlayerDead()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        GameSession.DeathCount++;
        Time.timeScale = 0f;  // 게임오버 시 일시정지
        StopWaveSpawning();
        OnGameOver?.Invoke();
    }

    public void RevivePlayer()
    {
        if (_playerObject == null) return;

        ReviveCount++;
        IsGameOver = false;
        Time.timeScale = 1f;  // 부활 시 재개

        PlayerHealth playerHealth = _playerObject.GetComponent<PlayerHealth>();
        playerHealth?.Revive();

        ResumeWaveSpawning();
        UIManager.Instance?.HideGameOver();
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(TITLE_SCENE_NAME);
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        GameSession.ResetSession();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void StartGameBGM()
    {
        if (_gameBGMPlaylist != null && _gameBGMPlaylist.Length > 0)
        {
            SoundManager.Instance?.PlayRandomBGM(_gameBGMPlaylist);
        }
    }
}