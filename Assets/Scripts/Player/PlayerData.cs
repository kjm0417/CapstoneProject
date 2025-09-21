using UnityEngine;


 [CreateAssetMenu(menuName = "Game/PlayerData", fileName = "PlayerData")]
public sealed class PlayerData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;

    [Header("Base Stats")]
    [Min(1)] public int maxHP = 100;
    [Range(0, 100)] public int stress = 0;
    [Range(0, 100)] public int concentration = 50;
    public float moveSpeed=3.5f;

    [Header("Animation")]
    public RuntimeAnimatorController animatorController; // AnimatorOverrideController도 할당 가능

    public string Id => id;

    /// <summary>
    /// SO를 오염시키지 않도록 런타임 상태를 ‘깊은 복사’로 생성
    public PlayerState CreateRuntimeState()
    {
        return new PlayerState(id, this);
    }
}