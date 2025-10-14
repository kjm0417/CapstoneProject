using System.Collections.Generic;
using System;
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
    public float moveSpeed = 3.5f;

    [Header("Animation")]
    public RuntimeAnimatorController animatorController; // AnimatorOverrideController도 가능

    [Header("Visuals (Resources)")]
    [Tooltip("Resources 폴더 기준 경로. 예: \"Characters/HeroA\"")]
    public string resourcesAddress;
    [Tooltip("로드한 스프라이트들 중 시작 시 적용할 기본 스프라이트 이름(옵션)")]
    public string defaultSpriteName;

    public string Id => id;

    /// SO를 오염시키지 않도록 런타임 상태를 깊은 복사로 생성합니다.
    /// </summary>
    public PlayerState CreateRuntimeState()
    {
        return new PlayerState(id, this);
    }

    [Serializable]
    public class RoutineRule
    {
        public DailyRoutine routine;
        public string requiredEffectId;
        public List<string> allowedEffectIds = new();
    }

    public List<RoutineRule> rules = new();
    public bool TryGetRule(DailyRoutine routine, out RoutineRule rule)
    {
        for (int i = 0; i < rules.Count; i++)
            if (rules[i].routine == routine) { rule = rules[i]; return true; }
        rule = null; return false;
    }

}
