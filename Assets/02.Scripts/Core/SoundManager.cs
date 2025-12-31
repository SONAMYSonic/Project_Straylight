using UnityEngine;
using UnityEngine.Audio; // 오디오 믹서 사용

public enum SoundType { BGM, SFX, Voice }

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup _bgmGroup;
    [SerializeField] private AudioMixerGroup _sfxGroup;
    [SerializeField] private AudioMixerGroup _voiceGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;   // 배경음용 (Loop)
    [SerializeField] private AudioSource _voiceSource; // 목소리용 (보통 겹치지 않게 하나만)
    [SerializeField] private AudioSource _sfxSource;   // 효과음용 (OneShot)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 노래는 끊기면 안 됨
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.clip == clip) return; // 이미 같은 노래면 무시

        _bgmSource.outputAudioMixerGroup = _bgmGroup;
        _bgmSource.clip = clip;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;

        // SFX는 PlayOneShot을 써야 여러 소리가 겹쳐서 납니다 (탕! 탕! 탕!)
        _sfxSource.outputAudioMixerGroup = _sfxGroup;
        _sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;

        // 보이스는 보통 이전 대사를 끊고 새 대사가 나옵니다.
        _voiceSource.Stop();
        _voiceSource.outputAudioMixerGroup = _voiceGroup;
        _voiceSource.clip = clip;
        _voiceSource.Play();
    }

    // 볼륨 조절 (설정창용, -80 ~ 0db 변환 필요)
    public void SetVolume(SoundType type, float volume)
    {
        // 슬라이더 값(0~1)을 데시벨(-80~0)로 변환하는 로직 필요
        // mixer.SetFloat(type.ToString(), Mathf.Log10(volume) * 20); 
        // (이 부분은 나중에 설정창 만들 때 구현)
    }
}