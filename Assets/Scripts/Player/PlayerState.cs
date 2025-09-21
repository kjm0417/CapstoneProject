using System;
using static UnityEngine.InputSystem.Controls.AxisControl;
using static UnityEngine.Rendering.DebugUI;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;


public interface IPlayerStateReadOnly
{
    string Id { get; }
    int CurrentHP { get; }
    int MaxHP { get; }
    int Stress { get; }
    int Concentration { get; }
}

/// <summary>
/// 런타임에서 변하는 플레이어 상태(단일 소유: PlayerManager)
/// </summary>
[Serializable]
public sealed class PlayerState : IPlayerStateReadOnly
{
    [SerializeField] private string id;     // PlayerData.Id 참조
    [SerializeField] private int maxHP;     // 고정 상한 (SO에서 복사)
    [SerializeField] private int currentHP; // 현재 체력
    [SerializeField] private int stress;    // 0~100
    [SerializeField] private int concentration; // 0~100

    public string Id => id;
    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public int Stress => stress;
    public int Concentration => concentration;

    public event Action<IPlayerStateReadOnly> OnChanged;

    public PlayerState(string id, PlayerData baseData)
    {
        this.id = id ?? string.Empty;
        if (baseData != null)
        {
            maxHP        = Mathf.Max(1, baseData.maxHP);
            currentHP    = maxHP; // 시작 규칙: 풀피 (필요시 변경)
            stress       = Mathf.Clamp(baseData.stress, 0, 100);
            concentration= Mathf.Clamp(baseData.concentration, 0, 100);
        }
        else
        {
            maxHP = 1; currentHP = 1;
            stress = 0; concentration = 0;
        }

    }

    // ===== 변경 메서드(검증/이벤트 포함) =====
    public void SetHP(int value)
    {
        var clamped = Mathf.Clamp(value, 0, MaxHP);
        if (clamped == currentHP) return;
        currentHP = clamped;
        OnChanged?.Invoke(this);
    }
    public void Damage(int amount)
    {
        if (amount <= 0) return;

        SetHP(currentHP - amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        SetHP(currentHP + amount);
    }

    public void SetStress(int value)
    {
        var clamped = Mathf.Clamp(value, 0, 100);
        if (clamped == stress) return;
        stress = clamped;
        OnChanged?.Invoke(this);
    }
    public void SetConcentration(int value)
    {
        var clamped = Mathf.Clamp(value, 0, 100);
        if (clamped == concentration) return;
        concentration = clamped;
        OnChanged?.Invoke(this);
    }
}