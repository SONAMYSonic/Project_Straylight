using UnityEngine;
using static GameEnums;

public class MeleeWeapon : MonoBehaviour
{
    private int _damage;
    private ElementType _currentElement;

    // PlayerCombat에서 공격할 때마다 이 함수를 호출해 스탯을 갱신해줍니다.
    public void SetStats(int damage, ElementType element)
    {
        _damage = damage;
        _currentElement = element;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 최종 데미지 계산
                int finalDamage = CalculateElementalDamage(_damage, _currentElement, enemy.elementType);
                enemy.TakeDamage(finalDamage);
            }
        }
    }

    private int CalculateElementalDamage(int baseDamage, ElementType attacker, ElementType defender)
    {
        float multiplier = 1.0f;

        // 상성 로직 (Vo > Da > Vi > Vo)
        if (attacker == ElementType.Vo && defender == ElementType.Da) multiplier = 1.5f;
        else if (attacker == ElementType.Da && defender == ElementType.Vi) multiplier = 1.5f;
        else if (attacker == ElementType.Vi && defender == ElementType.Vo) multiplier = 1.5f;

        // 역상성 (반대)
        else if (attacker == ElementType.Vo && defender == ElementType.Vi) multiplier = 0.5f;
        else if (attacker == ElementType.Da && defender == ElementType.Vo) multiplier = 0.5f;
        else if (attacker == ElementType.Vi && defender == ElementType.Da) multiplier = 0.5f;

        // 정수로 반환
        return Mathf.RoundToInt(baseDamage * multiplier);
    }
}