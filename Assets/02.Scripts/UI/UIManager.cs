using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdolMasterFanGame.UI;

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

    [Header("Game Over Panel")]
    [SerializeField] private GameObject _gameOverPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // GameManager의 게임오버 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }

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
        if (_scoreText != null) _scoreText.text = $"Kill: {currentKill}";
    }

    public void UpdateGameProgress(float progress, int state)
    {
        if (_progressUI != null)
            _progressUI.UpdateProgress(progress, state);
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
    }

    public void ShowBossWarning()
    {
        if (_bossWarningPanel != null)
        {
            _bossWarningPanel.SetActive(true);
            Invoke(nameof(HideBossWarning), 3.0f);
        }
    }

    private void HideBossWarning()
    {
        if (_bossWarningPanel != null) _bossWarningPanel.SetActive(false);
    }
}