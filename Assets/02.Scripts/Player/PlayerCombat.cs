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

    // 딕셔너리 초기화
    private Dictionary<IdolMode, GameObject> _weaponInstances = new Dictionary<IdolMode, GameObject>();
    private Dictionary<IdolMode, MeleeWeapon> _weaponScripts = new Dictionary<IdolMode, MeleeWeapon>();
    private Dictionary<IdolMode, ModeStat> _statMap = new Dictionary<IdolMode, ModeStat>();

    private GameObject _currentWeaponObj;
    private float _lastAttackTime;
    private bool _isAttacking = false;

    public void Initialize(PlayerInput inputRef)
    {
        _input = inputRef;

        SpawnAllWeapons();
        ChangeMode(IdolMode.Vocal, force: true);
    }

    private void SpawnAllWeapons()
    {
        foreach (var stat in _modeStats)
        {
            // 중복 방지 (Dictionary Key 중복 에러 방지)
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
        // 입력 우선순위 처리 (동시 입력 방지)
        if (_input.IsSwapVo) TryChangeMode(IdolMode.Vocal);
        else if (_input.IsSwapDa) TryChangeMode(IdolMode.Dance);
        else if (_input.IsSwapVi) TryChangeMode(IdolMode.Visual);

        if (!_isAttacking && _input.IsAttackTap && Time.time >= _lastAttackTime + _currentStat.AttackCooldown)
        {
            StartCoroutine(SwingSwordRoutine());
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
        if (!_weaponInstances.ContainsKey(newMode))
        {
            // Debug.LogWarning($"[PlayerCombat] {newMode} 프리팹이 없습니다.");
            return;
        }

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

        // 1. 스탯 갱신 (데미지 및 모드 전달)
        if (_weaponScripts.TryGetValue(_currentMode, out MeleeWeapon weaponScript))
        {
            weaponScript.SetStats(_currentStat.Damage, _currentMode);
        }

        // 2. 투사체 발사
        if (_currentStat.FiresProjectile)
        {
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            
            // 마우스 방향 계산
            Vector2 mouseWorldPos = _input.MousePos;
            Vector2 direction = (mouseWorldPos - (Vector2)spawnPos).normalized;
            
            GameObject bulletObj = ObjectPooler.Instance.SpawnFromPool(_projectileTag, spawnPos, Quaternion.identity);

            if (bulletObj != null && bulletObj.TryGetComponent(out Bullet bulletScript))
            {
                bulletScript.SetBulletStats(_currentStat.Damage, _currentMode);
                bulletScript.SetDirection(direction);
            }
        }

        // 3. 무기 활성화
        _currentWeaponObj.SetActive(true);

        float duration = _currentStat.SwingSpeed;
        float elapsed = 0f;

        // [수정 포인트] 시작/끝 각도 계산
        // 프리팹 내부에서 이미 "손잡이"를 (0,0)에 맞췄다고 가정합니다.
        Quaternion startRot = Quaternion.Euler(0, 0, _swingStartAngle);
        Quaternion endRot = Quaternion.Euler(0, 0, _swingEndAngle);

        // *추가 팁: 무기의 기본 회전값이 있다면 그것을 기준으로 더해줘야 함
        // 지금은 0도를 기준으로 +-60도 하므로 정직하게 위아래로 휘두릅니다.

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Easing: 휘두를 때는 빠르게, 끝에서 살짝 감속 (OutExpo 느낌)
            // t = t * t * (3f - 2f * t); // SmoothStep (부드러운 출발/도착)
            t = Mathf.Sin(t * Mathf.PI * SWING_EASING_MULTIPLIER);

            _currentWeaponObj.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _currentWeaponObj.SetActive(false);
        // 다음 공격을 위해 회전값 초기화 (선택 사항, 어차피 켤 때 다시 세팅하므로)
        _currentWeaponObj.transform.localRotation = Quaternion.identity;
        _isAttacking = false;
    }
}