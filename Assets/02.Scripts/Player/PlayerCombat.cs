using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string bulletTag = "PlayerBullet"; // 풀링 태그 이름과 일치해야 함!
    [SerializeField] private float fireRate = 0.1f; // 연사 속도 (초)
    [SerializeField] private Transform firePoint;   // 총알 나가는 위치 (총구)

    private PlayerInput input;
    private float lastFireTime;

    public void Initialize(PlayerInput inputRef)
    {
        input = inputRef;
    }

    public void HandleAttack()
    {
        // 공격 키를 누르고 있고 + 쿨타임이 지났다면
        if (input.IsFireHeld && Time.time >= lastFireTime + fireRate)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    private void Shoot()
    {
        // 총알이 나갈 위치가 지정 안 되어 있으면 플레이어 몸통에서 나감
        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;

        // 플레이어의 회전값(마우스 보는 방향)을 그대로 가져감
        Quaternion spawnRot = transform.rotation;

        // 오브젝트 풀에서 총알 하나 꺼내옴
        ObjectPooler.Instance.SpawnFromPool(bulletTag, spawnPos, spawnRot);
    }
}