using UnityEngine;
using IdolMasterFanGame;

/// <summary>
/// 디버그용 컨트롤러 - 테스트 목적으로만 사용
/// 빌드 시 비활성화 또는 제거 필요
/// </summary>
public class DebugController : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("디버그 기능 활성화")]
    [SerializeField] private bool _enableDebug = true;
    [Tooltip("전체 공격에 필요한 키 입력 횟수")]
    [SerializeField] private int _requiredPressCount = 5;
    [Tooltip("키 입력 리셋 시간 (초)")]
    [SerializeField] private float _resetTime = 1.0f;
    [Tooltip("전체 공격 데미지")]
    [SerializeField] private int _debugDamage = 999999;
    [Tooltip("디버그 키")]
    [SerializeField] private KeyCode _debugKey = KeyCode.F12;

    private int _currentPressCount = 0;
    private float _lastPressTime = 0f;

    private void Update()
    {
        if (!_enableDebug) return;

        if (Input.GetKeyDown(_debugKey))
        {
            HandleDebugKeyPress();
        }
    }

    private void HandleDebugKeyPress()
    {
        // 일정 시간 지나면 카운트 리셋
        if (Time.unscaledTime - _lastPressTime > _resetTime)
        {
            _currentPressCount = 0;
        }

        _currentPressCount++;
        _lastPressTime = Time.unscaledTime;

        Debug.Log($"[Debug] 키 입력: {_currentPressCount}/{_requiredPressCount}");

        if (_currentPressCount >= _requiredPressCount)
        {
            ExecuteDebugAttack();
            _currentPressCount = 0;
        }
    }

    private void ExecuteDebugAttack()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        int killCount = 0;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                enemy.TakeDamage(_debugDamage, IdolMode.None);
                killCount++;
            }
        }

        Debug.Log($"[Debug] 전체 공격 발동! {killCount}마리에게 {_debugDamage} 데미지");
    }
}
