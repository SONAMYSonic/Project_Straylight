using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class PlayerCombat : MonoBehaviour
{
    // 각 모드별 스탯을 저장할 클래스 (인스펙터에서 보기 좋게)
    [System.Serializable]
    public class ModeStat
    {
        public string modeName;
        public int damage;
        public float attackCooldown;
        public float swingSpeed; // 칼 휘두르는 시간 (작을수록 빠름)
        public Vector3 swordScale; // 칼 크기
        public Color swordColor;   // 칼 색상
        public bool firesProjectile; // 투사체 발사 여부 (Vi 모드용)
    }

    [Header("Mode Settings")]
    [SerializeField] private List<ModeStat> _modeStats; // 인스펙터에서 3개(Vo,Da,Vi) 등록
    [SerializeField] private SpriteRenderer _swordRenderer; // 칼 색 바꿀 렌더러
    [SerializeField] private MeleeWeapon _meleeWeapon;      // 데미지 전달할 무기 스크립트
    [SerializeField] private GameObject _swordObject;       // 칼 부모 오브젝트

    [Header("Vi Mode Projectile")]
    [SerializeField] private string _projectileTag = "PlayerBullet"; // Vi 검기 태그
    [SerializeField] private Transform _firePoint;

    private PlayerInput _input;
    private ElementType _currentType = ElementType.Vo; // 현재 모드
    private ModeStat _currentStat;

    private float _lastAttackTime;
    private bool _isAttacking = false;

    public void Initialize(PlayerInput inputRef)
    {
        _input = inputRef;
        _swordObject.SetActive(false);

        // 초기 모드 설정 (Vo)
        ChangeMode(ElementType.Vo);
    }

    public void HandleAttack()
    {
        // 1. 모드 변경 입력 확인
        if (_input.IsSwapVo) ChangeMode(ElementType.Vo);
        else if (_input.IsSwapDa) ChangeMode(ElementType.Da);
        else if (_input.IsSwapVi) ChangeMode(ElementType.Vi);

        // 2. 공격 실행
        if (!_isAttacking && _input.IsAttackTap && Time.time >= _lastAttackTime + _currentStat.attackCooldown)
        {
            StartCoroutine(SwingSwordRoutine());
        }
    }

    private void ChangeMode(ElementType newType)
    {
        _currentType = newType;
        // 리스트에서 해당 모드 스탯 가져오기 (인덱스: 0=Vo, 1=Da, 2=Vi 순서라고 가정)
        _currentStat = _modeStats[(int)newType];

        // 비주얼 업데이트 (칼 색상 변경)
        if (_swordRenderer != null)
        {
            _swordRenderer.color = _currentStat.swordColor;
        }

        // 칼 크기 변경 (Vo는 크게, Da는 작게 등)
        _swordObject.transform.localScale = _currentStat.swordScale;

        Debug.Log($"Mode Changed to: {newType}");
    }

    private IEnumerator SwingSwordRoutine()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        // 무기에 현재 모드의 데미지와 속성 주입
        _meleeWeapon.SetStats(_currentStat.damage, _currentType);

        // Vi 모드면 검기 발사!
        if (_currentStat.firesProjectile)
        {
            ObjectPooler.Instance.SpawnFromPool(_projectileTag, _firePoint.position, transform.rotation);
        }

        _swordObject.SetActive(true);

        // 휘두르기 애니메이션 (속도는 모드별 swingSpeed에 따름)
        float duration = _currentStat.swingSpeed;
        float elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(0, 0, 60);
        Quaternion endRot = Quaternion.Euler(0, 0, -60);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // 부드러운 움직임 (SmoothStep)
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