using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using IdolMasterFanGame;

public class PlayerSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private int _baseDamage = 150;
    [SerializeField] private float _baseCooldown = 30.0f;

    [Header("Visual & Audio")]
    [SerializeField] private VideoClip _skillVideoClip;
    [SerializeField] private AudioClip _skillReadySound1;
    [SerializeField] private AudioClip _skillReadySound2;

    [Header("Camera Shake")]
    [SerializeField] private float _shakeIntensity = 5.0f;

    private PlayerInput _input;
    private PlayerAudio _playerAudio;
    private float _lastSkillTime = -999f;
    private bool _isUsingSkill = false;
    private bool _isCooldownReady = true;

    private float CurrentCooldown => PlayerStats.Instance != null 
        ? PlayerStats.Instance.ApplyCooldownReduction(_baseCooldown) 
        : _baseCooldown;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _playerAudio = GetComponent<PlayerAudio>();
    }

    private void Start()
    {
        _lastSkillTime = -999f;
    }

    private void Update()
    {
        float timePassed = Time.time - _lastSkillTime;
        float ratio = Mathf.Clamp01(timePassed / CurrentCooldown);

        UIManager.Instance?.UpdateSkillCooldown(ratio);

        if (ratio >= 1.0f && !_isCooldownReady)
        {
            _isCooldownReady = true;
            PlayCooldownReadySound();
        }

        if (_input.IsUltTriggered && !_isUsingSkill)
        {
            if (_isCooldownReady)
            {
                UseSkill();
            }
        }
    }

    private void PlayCooldownReadySound()
    {
        if (_skillReadySound1 != null)
        {
            SoundManager.Instance?.PlaySFX(_skillReadySound1);
        }
        if (_skillReadySound2 != null)
        {
            SoundManager.Instance?.PlaySFX(_skillReadySound2);
        }
    }

    private void UseSkill()
    {
        _isUsingSkill = true;
        _isCooldownReady = false;
        _lastSkillTime = Time.time;

        _playerAudio?.PlaySkillVoice();

        UIManager.Instance?.PlaySkillCutscene(_skillVideoClip, OnSkillCutsceneFinished);
    }

    private void OnSkillCutsceneFinished()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        int killCount = 0;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                int finalDamage = CalculateDamage();
                enemy.TakeDamage(finalDamage, IdolMode.None);
                killCount++;
            }
        }

        CameraShakeManager.Instance?.ShakeCamera(_shakeIntensity);

        Debug.Log($"[PlayerSkill] 필살기 발동! {killCount}마리 타격, 데미지: {CalculateDamage()}");
        _isUsingSkill = false;
    }

    private int CalculateDamage()
    {
        if (PlayerStats.Instance != null)
        {
            return PlayerStats.Instance.CalculateFinalDamage(_baseDamage);
        }
        return _baseDamage;
    }
}