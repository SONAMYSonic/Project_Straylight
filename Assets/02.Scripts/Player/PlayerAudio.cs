using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;

public class PlayerAudio : MonoBehaviour
{
    [Header("Attack Voices")]
    [SerializeField] private List<AudioClip> _attackVoices;

    [Header("Attack SFX by Mode")]
    [Tooltip("Vocal 모드 공격 효과음")]
    [SerializeField] private AudioClip _vocalAttackSFX;
    [Tooltip("Dance 모드 공격 효과음")]
    [SerializeField] private AudioClip _danceAttackSFX;
    [Tooltip("Visual 모드 공격 효과음")]
    [SerializeField] private AudioClip _visualAttackSFX;

    [Header("Other Voices")]
    [SerializeField] private AudioClip _dashVoice;
    [SerializeField] private AudioClip _damageVoice;
    [SerializeField] private AudioClip _skillVoice;
    [SerializeField] private AudioClip _deathVoice;
    [SerializeField] private AudioClip _reviveVoice;

    [Header("Other SFX")]
    [SerializeField] private AudioClip _footstepSFX;
    [SerializeField] private AudioClip _reviveSFX;

    private PlayerHealth _health;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        if (_health != null)
        {
            _health.OnDamageTaken += PlayDamageSound;
            _health.OnDie += PlayDeathSound;
            _health.OnRevive += PlayReviveSound;
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDamageTaken -= PlayDamageSound;
            _health.OnDie -= PlayDeathSound;
            _health.OnRevive -= PlayReviveSound;
        }
    }

    /// <summary>
    /// 공격 보이스 재생 (재생 중이면 무시)
    /// </summary>
    public void PlayAttackVoice()
    {
        if (_attackVoices != null && _attackVoices.Count > 0)
        {
            int index = Random.Range(0, _attackVoices.Count);
            SoundManager.Instance?.PlayVoiceIfNotPlaying(_attackVoices[index]);
        }
    }

    /// <summary>
    /// 속성별 공격 효과음 재생
    /// </summary>
    public void PlayAttackSFX(IdolMode mode)
    {
        AudioClip clip = mode switch
        {
            IdolMode.Vocal => _vocalAttackSFX,
            IdolMode.Dance => _danceAttackSFX,
            IdolMode.Visual => _visualAttackSFX,
            _ => _vocalAttackSFX
        };

        SoundManager.Instance?.PlaySFX(clip);
    }

    public void PlayDashVoice()
    {
        SoundManager.Instance?.PlayVoice(_dashVoice);
    }

    private void PlayDamageSound()
    {
        SoundManager.Instance?.PlayVoice(_damageVoice);
    }

    public void PlaySkillVoice()
    {
        SoundManager.Instance?.PlayVoice(_skillVoice);
    }

    private void PlayDeathSound()
    {
        SoundManager.Instance?.PlayVoice(_deathVoice);
    }

    private void PlayReviveSound()
    {
        SoundManager.Instance?.PlayVoice(_reviveVoice);
        SoundManager.Instance?.PlaySFX(_reviveSFX);
    }
}