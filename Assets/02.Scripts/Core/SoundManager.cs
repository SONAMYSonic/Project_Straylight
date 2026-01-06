using System.Collections;
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

    [Header("BGM Playlist Settings")]
    [Tooltip("같은 곡이 연속으로 재생되지 않도록 방지")]
    [SerializeField] private bool _preventRepeat = true;

    // Exposed Parameter 이름
    private const string MIXER_BGM = "BGM";
    private const string MIXER_SFX = "SFX";
    private const string MIXER_VOICE = "Voice";

    private AudioClip[] _currentPlaylist;
    private int _lastPlayedIndex = -1;
    private Coroutine _playlistRoutine;

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

    /// <summary>
    /// 단일 BGM 재생 (루프)
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

        StopPlaylist();

        _bgmSource.outputAudioMixerGroup = _bgmGroup;
        _bgmSource.clip = clip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    /// <summary>
    /// 플레이리스트 설정 및 랜덤 BGM 재생 시작
    /// </summary>
    public void PlayRandomBGM(AudioClip[] playlist)
    {
        if (playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("[SoundManager] BGM 플레이리스트가 비어있습니다.");
            return;
        }

        _currentPlaylist = playlist;
        _lastPlayedIndex = -1;

        StopPlaylist();
        _playlistRoutine = StartCoroutine(PlaylistRoutine());
    }

    /// <summary>
    /// 플레이리스트 재생 중지
    /// </summary>
    public void StopPlaylist()
    {
        if (_playlistRoutine != null)
        {
            StopCoroutine(_playlistRoutine);
            _playlistRoutine = null;
        }
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        StopPlaylist();
        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    private IEnumerator PlaylistRoutine()
    {
        while (true)
        {
            AudioClip nextClip = GetRandomClip();
            if (nextClip == null) yield break;

            _bgmSource.outputAudioMixerGroup = _bgmGroup;
            _bgmSource.clip = nextClip;
            _bgmSource.loop = false;
            _bgmSource.Play();

            Debug.Log($"[SoundManager] BGM 재생: {nextClip.name}");

            // 곡이 끝날 때까지 대기
            yield return new WaitForSecondsRealtime(nextClip.length);

            // 약간의 간격
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private AudioClip GetRandomClip()
    {
        if (_currentPlaylist == null || _currentPlaylist.Length == 0) return null;

        if (_currentPlaylist.Length == 1) return _currentPlaylist[0];

        int randomIndex;

        if (_preventRepeat)
        {
            do
            {
                randomIndex = Random.Range(0, _currentPlaylist.Length);
            }
            while (randomIndex == _lastPlayedIndex);
        }
        else
        {
            randomIndex = Random.Range(0, _currentPlaylist.Length);
        }

        _lastPlayedIndex = randomIndex;
        return _currentPlaylist[randomIndex];
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

    /// <summary>
    /// 보이스가 재생 중이 아닐 때만 재생 (겹침 방지)
    /// </summary>
    public void PlayVoiceIfNotPlaying(AudioClip clip)
    {
        if (clip == null) return;
        if (_voiceSource.isPlaying) return;

        _voiceSource.outputAudioMixerGroup = _voiceGroup;
        _voiceSource.clip = clip;
        _voiceSource.Play();
    }

    /// <summary>
    /// 보이스가 재생 중인지 확인
    /// </summary>
    public bool IsVoicePlaying => _voiceSource != null && _voiceSource.isPlaying;

    public void SetVolume(SoundType type, float volume)
    {
        if (_audioMixer != null)
        {
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
                    Debug.LogWarning($"[SoundManager] AudioMixer에 '{paramName}' 파라미터가 없습니다.");
                    SetVolumeDirectly(type, volume);
                }
            }
        }
        else
        {
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