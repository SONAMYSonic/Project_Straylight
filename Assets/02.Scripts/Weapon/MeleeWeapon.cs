using UnityEngine;
using IdolMasterFanGame; // [필수] IdolMode 인식을 위해 추가

public class MeleeWeapon : MonoBehaviour
{
    private int _damage;
    private IdolMode _currentMode;

    // 공격 시 PlayerCombat에서 호출하여 스탯 설정
    public void SetStats(int damage, IdolMode mode)
    {
        _damage = damage;
        _currentMode = mode;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                // [수정됨] 데미지와 함께 공격 속성(_currentMode)을 전달해야 상성 계산이 됩니다.
                enemy.TakeDamage(_damage, _currentMode);
            }
        }
    }
}