using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Voices")]
    [SerializeField] private List<AudioClip> _attackVoices; // "에잇!", "핫!"
    [SerializeField] private AudioClip _dashVoice;          // "임임!"
    [SerializeField] private AudioClip _damageVoice;        // "아얏!"
    [SerializeField] private AudioClip _skillVoice;         // "이걸로 끝임다!"
    [SerializeField] private AudioClip _gameOverVoice;      // [추가] "으앙~", "프로듀서님..."

    [Header("SFX")]
    [SerializeField] private AudioClip _footstepSFX;
    [SerializeField] private AudioClip _swingSFX;

    // 컴포넌트 참조
    private PlayerHealth _health;
    private AudioSource _attackVoiceSource;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
        
        // 공격 보이스 전용 AudioSource 생성 (겹침 방지)
        _attackVoiceSource = gameObject.AddComponent<AudioSource>();
        _attackVoiceSource.playOnAwake = false;
    }

    private void Start()
    {
        // 이벤트 구독
        if (_health != null)
        {
            _health.OnDamageTaken += PlayDamageSound;
            _health.OnDie += PlayGameOverSound; // [추가] 사망 시 목소리 재생
        }
    }

    // Update 제거함: 대쉬 소리는 PlayerMovement가 직접 호출, 공격 소리는 PlayerCombat이 직접 호출

    // --- 재생 로직 ---

    public void PlayAttackVoice()
    {
        // 공격 보이스가 재생 중이 아닐 때만 재생 (겹침 방지)
        if (_attackVoiceSource != null && !_attackVoiceSource.isPlaying)
        {
            if (_attackVoices != null && _attackVoices.Count > 0)
            {
                int index = Random.Range(0, _attackVoices.Count);
                _attackVoiceSource.clip = _attackVoices[index];
                _attackVoiceSource.Play();
            }
        }
        
        // 공격 효과음은 항상 재생 (겹쳐도 됨)
        SoundManager.Instance.PlaySFX(_swingSFX);
    }

    public void PlayDashVoice()
    {
        SoundManager.Instance.PlayVoice(_dashVoice);
    }

    private void PlayDamageSound()
    {
        SoundManager.Instance.PlayVoice(_damageVoice);
    }

    public void PlaySkillSound()
    {
        SoundManager.Instance.PlayVoice(_skillVoice);
    }

    private void PlayGameOverSound()
    {
        SoundManager.Instance.PlayVoice(_gameOverVoice);
    }
}