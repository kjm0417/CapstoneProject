using System;
using UnityEngine;

/// <summary>
/// 보스 공격 패턴 타입
/// </summary>
public enum AttackPatternType
{
    Projectile,         // 투사체 (날아오는 공격)
    GroundAOE,          // 바닥 장판 (특정 영역 위험)
    WavePattern,        // 파동 패턴 (좌우로 이동하는 공격)
    RandomSpawn,        // 랜덤 위치 스폰
    TrackingHand,       // 플레이어를 추적하는 손
    StraightLine,       // 일직선으로 지나가는 공격
    CircularPattern,    // 원형 패턴 (사방으로 퍼짐)
    SpiralPattern,      // 나선형 패턴
    CustomBehavior      // 커스텀 동작 (프리팹에 스크립트 직접 구현)
}

/// <summary>
/// 보스 공격 패턴 데이터.
/// 각 패턴은 타입, 스폰 간격, 프리팹을 가집니다.
/// </summary>
[Serializable]
public class BossAttackPattern
{
    [Header("Pattern Info")]
    [Tooltip("공격 패턴 타입")]
    public AttackPatternType patternType = AttackPatternType.Projectile;

    [Tooltip("공격 스폰 간격 (초)")]
    public float spawnInterval = 2f;

    [Tooltip("동시 스폰 개수")]
    public int spawnCount = 1;

    [Header("Prefab")]
    [Tooltip("공격 오브젝트 프리팹 (충돌 시 데미지)")]
    public GameObject attackPrefab;

    [Header("Movement")]
    [Tooltip("공격 이동 속도")]
    public float moveSpeed = 5f;

    [Tooltip("공격 지속 시간 (초, 이후 자동 삭제)")]
    public float lifetime = 5f;

    [Header("Tracking Settings (TrackingHand용)")]
    [Tooltip("추적 속도 (플레이어를 따라가는 속도)")]
    public float trackingSpeed = 3f;

    [Tooltip("추적 지속 시간 (초)")]
    public float trackingDuration = 3f;

    [Header("Line Settings (StraightLine용)")]
    [Tooltip("일직선 이동 방향 (0=오른쪽, 90=위, 180=왼쪽, 270=아래)")]
    public float lineAngle = 0f;

    [Tooltip("화면 가장자리에서 시작 (false면 보스 위치에서 시작)")]
    public bool startFromEdge = true;

    [Header("Spawn Position")]
    [Tooltip("보스 위치에서 생성 (Projectile, StraightLine 등에 적용)")]
    public bool spawnFromBoss = false;

    [Header("Circular/Spiral Settings")]
    [Tooltip("원형/나선 패턴 발사 개수")]
    public int circularCount = 8;

    [Tooltip("나선 회전 속도 (도/초)")]
    public float spiralRotationSpeed = 90f;

    [Header("Damage")]
    [Tooltip("충돌 시 시간 감소 (초)")]
    public float timePenalty = 5f;

    [Tooltip("완벽한 클리어 실패 여부")]
    public bool failsPerfectClear = true;
}
