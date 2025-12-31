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
        public int Damage;
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
    private PlayerAudio _playerAudio; // [추가] 오디오 참조

    private Dictionary<IdolMode, GameObject> _weaponInstances = new Dictionary<IdolMode, GameObject>();
    private Dictionary<IdolMode, MeleeWeapon> _weaponScripts = new Dictionary<IdolMode, MeleeWeapon>();
    private Dictionary<IdolMode, ModeStat> _statMap = new Dictionary<IdolMode, ModeStat>();

    private GameObject _currentWeaponObj;
    private float _lastAttackTime;
    private bool _isAttacking = false;

    public void Initialize(PlayerInput inputRef)
    {
        _input = inputRef;

        // [추가] 오디오 컴포넌트 가져오기
        _playerAudio = GetComponent<PlayerAudio>();

        SpawnAllWeapons();
        ChangeMode(IdolMode.Vocal, force: true);
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
            if (weaponScript != null)
                _weaponScripts.Add(stat.ModeType, weaponScript);
        }
    }

    public void HandleAttack()
    {
        if (!_isAttacking)
        {
            RotateTowardsMouse();
        }

        if (_input.IsSwapVo) TryChangeMode(IdolMode.Vocal);
        else if (_input.IsSwapDa) TryChangeMode(IdolMode.Dance);
        else if (_input.IsSwapVi) TryChangeMode(IdolMode.Visual);

        if (!_isAttacking && _input.IsAttackTap && Time.time >= _lastAttackTime + _currentStat.AttackCooldown)
        {
            StartCoroutine(SwingSwordRoutine());
        }
    }

    private void RotateTowardsMouse()
    {
        if (_weaponHolder == null) return;

        Vector2 direction = (_input.MousePos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        _weaponHolder.rotation = Quaternion.Euler(0, 0, angle);

        // 무기만 상하 반전 (플레이어 방향은 PlayerAnimation이 처리)
        if (Mathf.Abs(angle) > 90)
        {
            _weaponHolder.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            _weaponHolder.localScale = new Vector3(1, 1, 1);
        }
    }

    private void TryChangeMode(IdolMode newMode)
    {
        if (_currentMode != newMode)
        {
            ChangeMode(newMode);
        }
    }

    private void ChangeMode(IdolMode newMode, bool force = false)
    {
        if (!_weaponInstances.ContainsKey(newMode)) return;

        if (_currentWeaponObj != null)
            _currentWeaponObj.SetActive(false);

        _currentMode = newMode;
        _currentStat = _statMap[newMode];
        _currentWeaponObj = _weaponInstances[newMode];

        OnModeChanged?.Invoke(_currentMode);
    }

    private IEnumerator SwingSwordRoutine()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        // [추가] 공격 소리 재생
        _playerAudio?.PlayAttackVoice();

        if (_weaponScripts.TryGetValue(_currentMode, out MeleeWeapon weaponScript))
        {
            weaponScript.SetStats(_currentStat.Damage, _currentMode);
        }

        if (_currentStat.FiresProjectile)
        {
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            Vector2 direction = (_input.MousePos - (Vector2)spawnPos).normalized;

            GameObject bulletObj = ObjectPooler.Instance.SpawnFromPool(_projectileTag, spawnPos, Quaternion.identity);

            if (bulletObj != null && bulletObj.TryGetComponent(out Bullet bulletScript))
            {
                bulletScript.SetBulletStats(_currentStat.Damage, _currentMode);
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
    }
}