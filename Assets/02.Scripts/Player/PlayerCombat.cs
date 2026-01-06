using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IdolMasterFanGame;

public class PlayerCombat : MonoBehaviour, IModeChangeHandler
{
    [System.Serializable]
    public class ModeStat
    {
        public IdolMode ModeType;
        [Header("Weapon Prefab")]
        public GameObject WeaponPrefab;
        [Header("Combat Stats")]
        public int BaseDamage;
        public float AttackCooldown;
        public float SwingSpeed;
        public bool FiresProjectile;
    }

    public event Action<IdolMode> OnModeChanged;
    public IdolMode CurrentMode => _currentMode;

    [Header("Settings")]
    [SerializeField] private List<ModeStat> _modeStats;
    [SerializeField] private Transform _weaponHolder;

    [Header("Vi Mode Projectile")]
    [SerializeField] private string _projectileTag = "PlayerBullet";
    [SerializeField] private Transform _firePoint;

    [Header("Swing Animation")]
    [SerializeField] private float _swingStartAngle = 60f;
    [SerializeField] private float _swingEndAngle = -60f;

    private const float SWING_EASING_MULTIPLIER = 0.5f;

    private PlayerInput _input;
    private IdolMode _currentMode = IdolMode.Vocal;
    private ModeStat _currentStat;
    private PlayerAudio _playerAudio;
    private PlayerHealth _playerHealth;

    private Dictionary<IdolMode, GameObject> _weaponInstances = new Dictionary<IdolMode, GameObject>();
    private Dictionary<IdolMode, MeleeWeapon> _weaponScripts = new Dictionary<IdolMode, MeleeWeapon>();
    private Dictionary<IdolMode, ModeStat> _statMap = new Dictionary<IdolMode, ModeStat>();

    private GameObject _currentWeaponObj;
    private float _lastAttackTime;
    private bool _isAttacking = false;
    private Coroutine _swingCoroutine;

    private readonly List<IdolMode> _modeCycle = new List<IdolMode> { IdolMode.Vocal, IdolMode.Dance, IdolMode.Visual };

    public void Initialize(PlayerInput inputRef)
    {
        _input = inputRef;
        _playerAudio = GetComponent<PlayerAudio>();
        _playerHealth = GetComponent<PlayerHealth>();

        // 부활 이벤트 구독
        if (_playerHealth != null)
        {
            _playerHealth.OnRevive += ResetAttackState;
        }

        SpawnAllWeapons();
        ChangeMode(IdolMode.Vocal, force: true);
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnRevive -= ResetAttackState;
        }
    }

    private void OnDisable()
    {
        // 비활성화 시 공격 상태 리셋
        ResetAttackState();
    }

    private void ResetAttackState()
    {
        // 진행 중인 공격 코루틴 중지
        if (_swingCoroutine != null)
        {
            StopCoroutine(_swingCoroutine);
            _swingCoroutine = null;
        }

        // 공격 상태 리셋
        _isAttacking = false;

        // 무기 비활성화 및 회전 초기화
        if (_currentWeaponObj != null)
        {
            _currentWeaponObj.SetActive(false);
            _currentWeaponObj.transform.localRotation = Quaternion.identity;
        }
    }

    private void SpawnAllWeapons()
    {
        foreach (var stat in _modeStats)
        {
            if (_statMap.ContainsKey(stat.ModeType)) continue;
            _statMap.Add(stat.ModeType, stat);
            if (stat.WeaponPrefab == null) continue;

            GameObject obj = Instantiate(stat.WeaponPrefab, _weaponHolder);
            obj.SetActive(false);
            MeleeWeapon weaponScript = obj.GetComponent<MeleeWeapon>();

            _weaponInstances.Add(stat.ModeType, obj);
            if (weaponScript != null) _weaponScripts.Add(stat.ModeType, weaponScript);
        }
    }

    public void HandleAttack()
    {
        if (!_isAttacking)
        {
            RotateTowardsMouse();
        }

        if (_input.IsVocalKeyPressed) ChangeMode(IdolMode.Vocal);
        else if (_input.IsDanceKeyPressed) ChangeMode(IdolMode.Dance);
        else if (_input.IsVisualKeyPressed) ChangeMode(IdolMode.Visual);

        if (_input.ScrollY > 0) CycleMode(1);
        else if (_input.ScrollY < 0) CycleMode(-1);

        if (!_isAttacking && _input.IsAttackTap && Time.time >= _lastAttackTime + _currentStat.AttackCooldown)
        {
            _swingCoroutine = StartCoroutine(SwingSwordRoutine());
        }
    }

    private void CycleMode(int direction)
    {
        int currentIndex = _modeCycle.IndexOf(_currentMode);
        int nextIndex = (currentIndex + direction) % _modeCycle.Count;

        if (nextIndex < 0) nextIndex += _modeCycle.Count;

        ChangeMode(_modeCycle[nextIndex]);
    }

    private void RotateTowardsMouse()
    {
        if (_weaponHolder == null) return;
        Vector2 direction = (_input.MousePos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _weaponHolder.rotation = Quaternion.Euler(0, 0, angle);

        if (Mathf.Abs(angle) > 90) _weaponHolder.localScale = new Vector3(1, -1, 1);
        else _weaponHolder.localScale = new Vector3(1, 1, 1);
    }

    private void ChangeMode(IdolMode newMode, bool force = false)
    {
        if (!_weaponInstances.ContainsKey(newMode)) return;
        if (_currentWeaponObj != null) _currentWeaponObj.SetActive(false);

        _currentMode = newMode;
        _currentStat = _statMap[newMode];
        _currentWeaponObj = _weaponInstances[newMode];

        OnModeChanged?.Invoke(_currentMode);
    }

    private int CalculateFinalDamage()
    {
        if (PlayerStats.Instance != null)
        {
            return PlayerStats.Instance.CalculateFinalDamage(_currentStat.BaseDamage);
        }
        return _currentStat.BaseDamage;
    }

    private IEnumerator SwingSwordRoutine()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        _playerAudio?.PlayAttackVoice();
        _playerAudio?.PlayAttackSFX(_currentMode);

        int finalDamage = CalculateFinalDamage();

        if (_weaponScripts.TryGetValue(_currentMode, out MeleeWeapon weaponScript))
        {
            weaponScript.SetStats(finalDamage, _currentMode);
        }

        if (_currentStat.FiresProjectile)
        {
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            Vector2 direction = (_input.MousePos - (Vector2)spawnPos).normalized;
            GameObject bulletObj = ObjectPooler.Instance?.SpawnFromPool(_projectileTag, spawnPos, Quaternion.identity);

            if (bulletObj != null && bulletObj.TryGetComponent(out Bullet bulletScript))
            {
                bulletScript.SetBulletStats(finalDamage, _currentMode);
                bulletScript.SetDirection(direction);
            }
        }

        _currentWeaponObj.SetActive(true);
        float duration = _currentStat.SwingSpeed;
        float elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(0, 0, _swingStartAngle);
        Quaternion endRot = Quaternion.Euler(0, 0, _swingEndAngle);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * SWING_EASING_MULTIPLIER);
            _currentWeaponObj.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _currentWeaponObj.SetActive(false);
        _currentWeaponObj.transform.localRotation = Quaternion.identity;
        _isAttacking = false;
        _swingCoroutine = null;
    }
}