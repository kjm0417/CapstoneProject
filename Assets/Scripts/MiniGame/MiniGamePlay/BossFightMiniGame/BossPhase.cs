using System;
using UnityEngine;

/// <summary>
/// 보스전 페이즈 데이터.
/// 각 페이즈는 지속 시간과 공격 패턴을 가집니다.
/// </summary>
[Serializable]
public class BossPhase
{
    [Header("Phase Info")]
    [Tooltip("페이즈 이름 (예: Phase 1, Phase 2)")]
    public string phaseName = "Phase 1";

    [Tooltip("이 페이즈의 지속 시간 (초)")]
    public float duration = 20f;

    [Header("Attack Patterns")]
    [Tooltip("이 페이즈에서 사용할 공격 패턴들")]
    public BossAttackPattern[] attackPatterns;

    [Header("Visual")]
    [Tooltip("페이즈 시작 시 표시할 텍스트 (선택)")]
    public string phaseStartText = "";

    [Tooltip("배경색 변화 (선택)")]
    public Color phaseColor = Color.white;
}
