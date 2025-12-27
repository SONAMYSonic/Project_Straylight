using UnityEngine;
using IdolMasterFanGame;

public class MeleeWeapon : MonoBehaviour
{
    private int _damage;
    private IdolMode _currentMode;

    // PlayerCombat에서 호출
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
                // 적에게 데미지와 "현재 내 속성"을 같이 전달
                enemy.TakeDamage(_damage, _currentMode);
            }
        }
    }
}