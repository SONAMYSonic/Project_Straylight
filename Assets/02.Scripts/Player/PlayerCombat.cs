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
        public int Damage;
        public float AttackCooldown;
        public float SwingSpeed;
        public Vector3 SwordScale;
        public Color SwordColor;
        public bool FiresProjectile;
    }

    // 인터페이스 구현 (UI가 구독할 이벤트)
    public event Action<IdolMode> OnModeChanged;
    public IdolMode CurrentMode => _currentMode;

    [Header("Mode Settings")]
    [SerializeField] private List<ModeStat> _modeStats;
    [SerializeField] private SpriteRenderer _swordRenderer;
    [SerializeField] private MeleeWeapon _meleeWeapon;
    [SerializeField] private GameObject _swordObject;

    [Header("Vi Mode Projectile")]
    [SerializeField] private string _projectileTag = "PlayerBullet";
    [SerializeField] private Transform _firePoint;

    private PlayerInput _input;
    private IdolMode _currentMode = IdolMode.Vocal;
    private ModeStat _currentStat;
    private Dictionary<IdolMode, ModeStat> _statMap;

    private float _lastAttackTime;
    private bool _isAttacking = false;

    public void Initialize(PlayerInput inputRef)
    {
        _input = inputRef;
        _swordObject.SetActive(false);
        InitializeStatsMap();

        // 초기화 시 강제로 Vo 모드 적용
        ChangeMode(IdolMode.Vocal, force: true);
    }

    private void InitializeStatsMap()
    {
        _statMap = new Dictionary<IdolMode, ModeStat>();
        foreach (var stat in _modeStats)
        {
            if (!_statMap.ContainsKey(stat.ModeType))
                _statMap.Add(stat.ModeType, stat);
        }
    }

    public void HandleAttack()
    {
        // 입력 체크
        if (_input.IsSwapVo) TryChangeMode(IdolMode.Vocal);
        else if (_input.IsSwapDa) TryChangeMode(IdolMode.Dance);
        else if (_input.IsSwapVi) TryChangeMode(IdolMode.Visual);

        // 공격 체크
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
        if (!_statMap.ContainsKey(newMode)) return;

        _currentMode = newMode;
        _currentStat = _statMap[newMode];

        // 비주얼 갱신
        if (_swordRenderer != null) _swordRenderer.color = _currentStat.SwordColor;
        _swordObject.transform.localScale = _currentStat.SwordScale;

        // 이벤트 발신 -> UI가 듣고 이미지 바꿈
        OnModeChanged?.Invoke(_currentMode);
    }

    private IEnumerator SwingSwordRoutine()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        // 무기에 현재 상태 주입
        _meleeWeapon.SetStats(_currentStat.Damage, _currentMode);

        if (_currentStat.FiresProjectile)
        {
            GameObject bulletObj = ObjectPooler.Instance.SpawnFromPool(_projectileTag, _firePoint.position, transform.rotation);

            // bulletObj가 null이 아니고, Bullet 컴포넌트가 실제로 붙어있는지 한 번에 확인
            if (bulletObj != null && bulletObj.TryGetComponent(out Bullet bulletScript))
            {
                bulletScript.SetBulletStats(_currentStat.Damage, _currentMode);
            }
        }

        _swordObject.SetActive(true);

        float duration = _currentStat.SwingSpeed;
        float elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(0, 0, 60);
        Quaternion endRot = Quaternion.Euler(0, 0, -60);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            _swordObject.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _swordObject.SetActive(false);
        _swordObject.transform.localRotation = Quaternion.identity;
        _isAttacking = false;
    }
}