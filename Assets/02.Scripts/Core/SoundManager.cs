using UnityEngine;
using UnityEngine.Audio;

public enum SoundType { BGM, SFX, Voice }

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup _bgmGroup;
    [SerializeField] private AudioMixerGroup _sfxGroup;
    [SerializeField] private AudioMixerGroup _voiceGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _voiceSource;
    [SerializeField] private AudioSource _sfxSource;

    // Exposed Parameter 이름 (AudioMixer에서 설정한 이름과 동일해야 함)
    private const string MIXER_BGM = "BGM";
    private const string MIXER_SFX = "SFX";
    private const string MIXER_VOICE = "Voice";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.clip == clip) return;

        _bgmSource.outputAudioMixerGroup = _bgmGroup;
        _bgmSource.clip = clip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;

        _sfxSource.outputAudioMixerGroup = _sfxGroup;
        _sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;

        _voiceSource.Stop();
        _voiceSource.outputAudioMixerGroup = _voiceGroup;
        _voiceSource.clip = clip;
        _voiceSource.Play();
    }

    public void SetVolume(SoundType type, float volume)
    {
        // AudioMixer가 있으면 Mixer로 조절
        if (_audioMixer != null)
        {
            // 슬라이더(0.0001 ~ 1) -> 데시벨(-80 ~ 0) 변환
            float db = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;

            string paramName = type switch
            {
                SoundType.BGM => MIXER_BGM,
                SoundType.SFX => MIXER_SFX,
                SoundType.Voice => MIXER_VOICE,
                _ => ""
            };

            if (!string.IsNullOrEmpty(paramName))
            {
                bool success = _audioMixer.SetFloat(paramName, db);
                if (!success)
                {
                    Debug.LogWarning($"[SoundManager] AudioMixer에 '{paramName}' 파라미터가 없습니다. Exposed Parameter를 확인하세요.");
                    // 폴백: AudioSource 볼륨 직접 조절
                    SetVolumeDirectly(type, volume);
                }
            }
        }
        else
        {
            // AudioMixer가 없으면 AudioSource 볼륨 직접 조절
            SetVolumeDirectly(type, volume);
        }
    }

    private void SetVolumeDirectly(SoundType type, float volume)
    {
        switch (type)
        {
            case SoundType.BGM:
                if (_bgmSource != null) _bgmSource.volume = volume;
                break;
            case SoundType.SFX:
                if (_sfxSource != null) _sfxSource.volume = volume;
                break;
            case SoundType.Voice:
                if (_voiceSource != null) _voiceSource.volume = volume;
                break;
        }
    }
}