using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _controlPanel;
    [SerializeField] private GameObject _optionPanel;

    [Header("Option Sliders")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _voiceSlider; // [추가] 보이스 슬라이더

    [Header("Audio")]
    [SerializeField] private AudioClip _titleBGM; // [추가] 타이틀 배경음

    [Header("UI SFX")]
    [SerializeField] private AudioClip _clickSFX;  // 일반 클릭 (딸깍)
    [SerializeField] private AudioClip _cancelSFX; // 뒤로가기/취소 (취소음)

    private void Start()
    {
        // 1. 패널 초기화
        if (_controlPanel != null) _controlPanel.SetActive(false);
        if (_optionPanel != null) _optionPanel.SetActive(false);

        // 2. 타이틀 BGM 재생
        if (SoundManager.Instance != null && _titleBGM != null)
        {
            SoundManager.Instance.PlayBGM(_titleBGM);
        }

        // 3. 슬라이더 초기값 설정 (기본값 1.0f)
        // 저장된 볼륨이 있다면 그걸 불러오는 로직이 SoundManager에 있어야 하지만,
        // 여기서는 간단하게 1.0(최대)으로 시작하거나 슬라이더의 현재 값을 따릅니다.
        if (_bgmSlider != null) _bgmSlider.value = 1f;
        if (_sfxSlider != null) _sfxSlider.value = 1f;
        if (_voiceSlider != null) _voiceSlider.value = 1f;
    }

    public void OnClickStart()
    {
        GameSession.ResetSession();
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickControls()
    {
        _controlPanel.SetActive(!_controlPanel.activeSelf);
    }

    public void OnClickOptions()
    {
        _optionPanel.SetActive(!_optionPanel.activeSelf);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- 슬라이더 이벤트 연결 함수들 ---

    public void OnBGMVolumeChanged(float value)
    {
        SoundManager.Instance?.SetVolume(SoundType.BGM, value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance?.SetVolume(SoundType.SFX, value);
    }

    // [추가] 보이스 볼륨 조절
    public void OnVoiceVolumeChanged(float value)
    {
        SoundManager.Instance?.SetVolume(SoundType.Voice, value);
    }

    public void PlayClickSound()
    {
        if (SoundManager.Instance != null && _clickSFX != null)
            SoundManager.Instance.PlaySFX(_clickSFX);
    }

    public void PlayCancelSound()
    {
        if (SoundManager.Instance != null && _cancelSFX != null)
            SoundManager.Instance.PlaySFX(_cancelSFX);
    }
}