using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdolMasterFanGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Sub UI Components")]
    [SerializeField] private PlayerModeUI _modeUI;
    [SerializeField] private GameProgressUI _progressUI;

    [Header("HUD References")]
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _bossWarningPanel;
    [Tooltip("보스 경고 표시 시간 (초)")]
    [SerializeField] private float _bossWarningDuration = 3.0f;

    [Header("Skill UI")]
    [SerializeField] private Image _skillCooldownImage;
    [SerializeField] private GameObject _playerSkillHud;
    [Tooltip("스킬 준비 완료 시 펀치 스케일 크기")]
    [SerializeField] private float _skillReadyPunchScale = 1.3f;
    [Tooltip("스킬 준비 완료 시 펀치 애니메이션 시간")]
    [SerializeField] private float _skillReadyPunchDuration = 0.3f;

    [Header("Pause Menu")]
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseReturnToTitleButton;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _reviveButton;
    [SerializeField] private Button _returnToTitleButton;

    [Header("Buff Selection Panel")]
    [SerializeField] private GameObject _buffSelectionPanel;
    [SerializeField] private List<Button> _buffButtons;
    [Tooltip("버프 버튼 등장 시간 (초)")]
    [SerializeField] private float _buffButtonPopDuration = 0.3f;
    [Tooltip("버프 버튼 등장 딜레이 (순차 등장)")]
    [SerializeField] private float _buffButtonPopDelay = 0.1f;
    [Tooltip("호버링 높이")]
    [SerializeField] private float _buffButtonHoverHeight = 10f;
    [Tooltip("호버링 주기 (초)")]
    [SerializeField] private float _buffButtonHoverDuration = 1.0f;

    [Header("Skill Cutscene")]
    [SerializeField] private GameObject _cutscenePanel; 
    [SerializeField] private RawImage _cutsceneDisplay;
    [SerializeField] private VideoPlayer _videoPlayer;
    [Tooltip("스킵에 필요한 클릭 횟수")]
    [SerializeField] private int _skipClickCount = 2;
    [Tooltip("클릭 카운트 리셋 시간 (초)")]
    [SerializeField] private float _skipClickResetTime = 0.5f;

    [Header("Boss Gimmick UI")]
    [SerializeField] private GameObject _maintenancePanel;

    private bool _wasSkillReady = false;
    private bool _isPlayingCutscene = false;
    private int _currentSkipClicks = 0;
    private float _lastClickTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePanels();
    }

    private void InitializePanels()
    {
        if (_cutscenePanel != null) _cutscenePanel.SetActive(false);
        if (_maintenancePanel != null) _maintenancePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_buffSelectionPanel != null) _buffSelectionPanel.SetActive(false);
        if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(false);
    }

    private void Start()
    {
        SubscribeToEvents();
        BindButtons();
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnGameOver += ShowGameOver;
        if (_videoPlayer != null) _videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void BindButtons()
    {
        if (_reviveButton != null) _reviveButton.onClick.AddListener(OnReviveButtonClicked);
        if (_returnToTitleButton != null) _returnToTitleButton.onClick.AddListener(OnReturnToTitleButtonClicked);

        // 일시정지 메뉴 버튼
        if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResumeButtonClicked);
        if (_pauseReturnToTitleButton != null) _pauseReturnToTitleButton.onClick.AddListener(OnReturnToTitleButtonClicked);

        // 버프 버튼 바인딩
        for (int i = 0; i < _buffButtons.Count; i++)
        {
            int index = i;
            if (_buffButtons[i] != null)
            {
                _buffButtons[i].onClick.AddListener(() => OnBuffSelected(index));
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        UnbindButtons();
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnGameOver -= ShowGameOver;
    }

    private void UnbindButtons()
    {
        if (_reviveButton != null) _reviveButton.onClick.RemoveListener(OnReviveButtonClicked);
        if (_returnToTitleButton != null) _returnToTitleButton.onClick.RemoveListener(OnReturnToTitleButtonClicked);
        if (_resumeButton != null) _resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
        if (_pauseReturnToTitleButton != null) _pauseReturnToTitleButton.onClick.RemoveListener(OnReturnToTitleButtonClicked);

        foreach (var btn in _buffButtons)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }
    }

    private void Update()
    {
        if (_isPlayingCutscene && Input.GetMouseButtonDown(0))
        {
            HandleCutsceneSkip();
        }
    }

    private void HandleCutsceneSkip()
    {
        if (Time.unscaledTime - _lastClickTime > _skipClickResetTime)
        {
            _currentSkipClicks = 0;
        }

        _currentSkipClicks++;
        _lastClickTime = Time.unscaledTime;

        if (_currentSkipClicks >= _skipClickCount)
        {
            SkipCutscene();
        }
    }

    private void SkipCutscene()
    {
        if (_videoPlayer != null && _videoPlayer.isPlaying)
        {
            _videoPlayer.Stop();
        }
        _isPlayingCutscene = false;
        _currentSkipClicks = 0;
    }

    // --- Pause Menu ---

    public void ShowPauseMenu()
    {
        if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(false);
    }

    private void OnResumeButtonClicked()
    {
        GameManager.Instance?.ResumeGame();
    }

    // --- 기존 코드 ---

    public void UpdatePlayerMode(IdolMasterFanGame.IdolMode mode)
    {
        if (_modeUI != null) _modeUI.UpdateModeUI(mode);
    }

    public void UpdateHealth(float current, float max)
    {
        if (_healthBarImage != null) _healthBarImage.fillAmount = current / max;
    }

    public void UpdateKillCount(int currentKill, int targetKill)
    {
        if (_scoreText != null) _scoreText.text = $"Kill: {currentKill} / {targetKill}";
    }

    public void UpdateGameProgress(float progress, int state)
    {
        if (_progressUI != null) _progressUI.UpdateProgress(progress, state);
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        UpdateReviveButtonVisibility();
    }

    private void UpdateReviveButtonVisibility()
    {
        if (_reviveButton != null)
        {
            bool canRevive = GameManager.Instance != null && GameManager.Instance.CanRevive;
            _reviveButton.gameObject.SetActive(canRevive);
        }
    }

    public void HideGameOver()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
    }

    private void OnReviveButtonClicked()
    {
        GameManager.Instance?.RevivePlayer();
    }

    private void OnReturnToTitleButtonClicked()
    {
        GameManager.Instance?.ReturnToTitle();
    }

    public void ShowBossWarning()
    {
        if (_bossWarningPanel != null)
        {
            _bossWarningPanel.SetActive(true);
            Invoke(nameof(HideBossWarning), _bossWarningDuration);
        }
    }

    private void HideBossWarning()
    {
        if (_bossWarningPanel != null) _bossWarningPanel.SetActive(false);
    }

    public void ShowMaintenanceNotice(float duration)
    {
        if (_maintenancePanel != null) StartCoroutine(MaintenanceRoutine(duration));
    }

    private IEnumerator MaintenanceRoutine(float duration)
    {
        _maintenancePanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        _maintenancePanel.SetActive(false);
    }

    // --- Buff Selection UI ---

    public void ShowBuffSelection()
    {
        if (_buffSelectionPanel == null) return;

        _buffSelectionPanel.SetActive(true);

        // 버프 버튼 애니메이션 시작
        StartCoroutine(AnimateBuffButtons());
    }

    private IEnumerator AnimateBuffButtons()
    {
        // 모든 버튼 초기화 (스케일 0)
        foreach (var btn in _buffButtons)
        {
            if (btn != null)
            {
                btn.transform.localScale = Vector3.zero;
                btn.transform.DOKill();
            }
        }

        // 순차적으로 팝업 등장
        for (int i = 0; i < _buffButtons.Count; i++)
        {
            if (_buffButtons[i] == null) continue;

            Button btn = _buffButtons[i];
            RectTransform rect = btn.GetComponent<RectTransform>();
            Vector2 originalPos = rect.anchoredPosition;

            // 스케일 0 → 1 (팝업)
            btn.transform.DOScale(Vector3.one, _buffButtonPopDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            // 호버링 애니메이션 (위아래 반복)
            rect.DOAnchorPosY(originalPos.y + _buffButtonHoverHeight, _buffButtonHoverDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetDelay(_buffButtonPopDuration);

            // 다음 버튼 딜레이
            yield return new WaitForSecondsRealtime(_buffButtonPopDelay);
        }
    }

    private void OnBuffSelected(int index)
    {
        // 버튼 애니메이션 정지
        StopBuffButtonAnimations();

        // 0: Power, 1: Acceleration, 2: CriticalEye
        PlayerStats.BuffType selectedBuff = PlayerStats.GetBuffTypeByIndex(index);
        PlayerStats.Instance?.AcquireBuff(selectedBuff);

        _buffSelectionPanel.SetActive(false);

        GameManager.Instance?.ResumeAfterBuffSelection();
    }

    private void StopBuffButtonAnimations()
    {
        foreach (var btn in _buffButtons)
        {
            if (btn != null)
            {
                btn.transform.DOKill();
                RectTransform rect = btn.GetComponent<RectTransform>();
                rect.DOKill();
            }
        }
    }

    public void HideBuffSelection()
    {
        StopBuffButtonAnimations();
        if (_buffSelectionPanel != null) _buffSelectionPanel.SetActive(false);
    }

    // --- Skill Cutscene ---

    public void PlaySkillCutscene(VideoClip clip, System.Action onComplete)
    {
        // WebGL에서는 VideoPlayer 문제가 있으므로 스킵 옵션 제공
#if UNITY_WEBGL
        // WebGL에서는 비디오 스킵하고 바로 콜백 실행
        onComplete?.Invoke();
        return;
#else
        if (_cutscenePanel == null || _videoPlayer == null || clip == null)
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(PlayVideoRoutine(clip, onComplete));
#endif
    }

    private IEnumerator PlayVideoRoutine(VideoClip clip, System.Action onComplete)
    {
        _isPlayingCutscene = true;
        _currentSkipClicks = 0;

        Time.timeScale = 0f;
        _cutscenePanel.SetActive(true);
        _videoPlayer.clip = clip;
        _videoPlayer.Prepare();

        // 준비 타임아웃 (3초)
        float prepareTimeout = 3f;
        float prepareTimer = 0f;

        while (!_videoPlayer.isPrepared && prepareTimer < prepareTimeout)
        {
            prepareTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 준비 실패 시 스킵
        if (!_videoPlayer.isPrepared)
        {
            Debug.LogWarning("[UIManager] 비디오 준비 실패, 스킵합니다.");
            FinishCutscene(onComplete);
            yield break;
        }

        _videoPlayer.Play();

        // 재생 타임아웃 (비디오 길이 + 2초)
        float playTimeout = (float)_videoPlayer.clip.length + 2f;
        float playTimer = 0f;

        while (_videoPlayer.isPlaying && _isPlayingCutscene && playTimer < playTimeout)
        {
            playTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        FinishCutscene(onComplete);
    }

    private void FinishCutscene(System.Action onComplete)
    {
        if (_videoPlayer != null && _videoPlayer.isPlaying)
        {
            _videoPlayer.Stop();
        }

        _cutscenePanel.SetActive(false);
        Time.timeScale = 1f;
        _isPlayingCutscene = false;

        onComplete?.Invoke();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        _isPlayingCutscene = false;
    }

    // --- Skill Cooldown UI ---

    public void UpdateSkillCooldown(float ratio)
    {
        if (_skillCooldownImage != null)
        {
            _skillCooldownImage.fillAmount = ratio;
        }

        bool isSkillReady = ratio >= 1f;
        if (isSkillReady && !_wasSkillReady)
        {
            PlaySkillReadyEffect();
        }
        _wasSkillReady = isSkillReady;
    }

    private void PlaySkillReadyEffect()
    {
        if (_playerSkillHud == null) return;

        _playerSkillHud.transform.DOKill();

        _playerSkillHud.transform.localScale = Vector3.one;
        _playerSkillHud.transform.DOPunchScale(
            Vector3.one * (_skillReadyPunchScale - 1f),
            _skillReadyPunchDuration,
            vibrato: 1,
            elasticity: 0.5f
        ).SetUpdate(true);
    }
}