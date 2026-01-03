using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 성장 및 버프 시스템 관리
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public enum BuffType
    {
        None,
        Power,        // 0번 버튼: 공격력 +60%
        Acceleration, // 1번 버튼: 스킬/대쉬 쿨타임 -10%
        CriticalEye   // 2번 버튼: 치명타 확률 +30%, 치명타 150%
    }

    [Header("Buff Values")]
    [Tooltip("파워 버프 공격력 증가율")]
    [SerializeField] private float _powerDamageBonus = 0.6f;
    [Tooltip("가속 버프 쿨타임 감소율")]
    [SerializeField] private float _accelerationCooldownReduction = 0.1f;
    [Tooltip("심안 버프 치명타 확률")]
    [SerializeField] private float _criticalChance = 0.3f;
    [Tooltip("심안 버프 치명타 배율")]
    [SerializeField] private float _criticalMultiplier = 1.5f;

    // 현재 적용된 버프들
    private List<BuffType> _activeBuffs = new List<BuffType>();

    // 스탯 배율 (합연산)
    public float DamageMultiplier { get; private set; } = 1f;
    public float CooldownMultiplier { get; private set; } = 1f;
    public float CritChance { get; private set; } = 0f;
    public float CritMultiplier { get; private set; } = 1.5f;

    // 이벤트
    public event Action<BuffType> OnBuffAcquired;
    public event Action OnStatsChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ResetStats();
    }

    public void ResetStats()
    {
        _activeBuffs.Clear();
        DamageMultiplier = 1f;
        CooldownMultiplier = 1f;
        CritChance = 0f;
        CritMultiplier = _criticalMultiplier;
    }

    /// <summary>
    /// 버튼 인덱스에 해당하는 버프 타입 반환
    /// 0: Power, 1: Acceleration, 2: CriticalEye
    /// </summary>
    public static BuffType GetBuffTypeByIndex(int index)
    {
        return index switch
        {
            0 => BuffType.Power,
            1 => BuffType.Acceleration,
            2 => BuffType.CriticalEye,
            _ => BuffType.None
        };
    }

    /// <summary>
    /// 버프 획득
    /// </summary>
    public void AcquireBuff(BuffType buffType)
    {
        _activeBuffs.Add(buffType);

        switch (buffType)
        {
            case BuffType.Power:
                DamageMultiplier += _powerDamageBonus;
                break;

            case BuffType.Acceleration:
                CooldownMultiplier *= (1f - _accelerationCooldownReduction);
                break;

            case BuffType.CriticalEye:
                CritChance += _criticalChance;
                break;
        }

        Debug.Log($"[PlayerStats] 버프 획득: {buffType}, 공격력배율={DamageMultiplier:F2}, 쿨타임배율={CooldownMultiplier:F2}, 치명타확률={CritChance:P0}");

        OnBuffAcquired?.Invoke(buffType);
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 최종 데미지 계산 (치명타 포함)
    /// </summary>
    public int CalculateFinalDamage(int baseDamage)
    {
        float damage = baseDamage * DamageMultiplier;

        // 치명타 판정
        if (UnityEngine.Random.value < CritChance)
        {
            damage *= CritMultiplier;
            Debug.Log("[PlayerStats] 치명타 발동!");
        }

        return Mathf.RoundToInt(damage);
    }

    /// <summary>
    /// 쿨타임에 배율 적용
    /// </summary>
    public float ApplyCooldownReduction(float baseCooldown)
    {
        return baseCooldown * CooldownMultiplier;
    }

    public int GetBuffCount(BuffType type)
    {
        int count = 0;
        foreach (var buff in _activeBuffs)
        {
            if (buff == type) count++;
        }
        return count;
    }

    public List<BuffType> GetActiveBuffs()
    {
        return new List<BuffType>(_activeBuffs);
    }
}
