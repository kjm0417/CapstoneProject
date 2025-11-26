using System;
using UnityEngine;

#region Result/Reward Models
/// <summary>
/// 보상 타입 (필요 시 추가 확장)
/// </summary>
public enum RewardType
{
    StressDown,
    StressUp,
    ConcentrationUp,
    ConcentrationDown,
    Item, // 아이템 보상 추가
}

/// <summary>
/// 미니게임 보상 구조체
/// - amount: 수치(정수), param: 추가 파라미터(아이템ID 등)
/// </summary>
[Serializable]
public struct MiniGameReward
{
    public RewardType type; // 보상 타입
    public int amount; // 양
    public string itemId; // 아이템 ID (Item 타입일 때 사용)
}

/// <summary>
/// 완료 시 콜백 인자 (성공/소요/남은/자동)
/// </summary>
[Serializable]
public struct MiniGameCompleteArgs
{
    public bool success; // 성공여부
    public float usedTime; // 소요시간
    public float remainingTime; // 남은시간 or 초과한 시간
    public bool isPerfectClear; // 완벽한 클리어(◇) 달성 여부
}
#endregion

/// <summary>
/// 모든 미니게임이 구현해야 하는 최소 기능 인터페이스
/// - 기본 속성: Id/Duration/RemainingTime/IsRunning
/// - 기본 이벤트: 시작/틱/완료/중단
/// - 기본 라이프사이클 메서드
/// </summary>
public interface IMiniGame
{
    // 게임 식별자 (매니저/컴포넌트 검색에 사용)
    string Id { get; }

    // 게임 조우 타입 (미니게임 or 보스전)
    GameEncounterType EncounterType { get; }

    // 제한 시간 (초)
    float Duration { get; }

    // 남은 시간 (초)
    float RemainingTime { get; }

    // 실행 중 여부
    bool IsRunning { get; }

    // 이벤트 들
    event Action<IMiniGame> OnStarted;
    event Action<IMiniGame, float> OnTick;
    event Action<IMiniGame, MiniGameCompleteArgs> OnCompleted;
    event Action<IMiniGame, string> OnAborted;

    /// <summary>외부 매니저 준비 (필요 시 호출, 선택)</summary>
    void Init();

    /// <summary>게임 시작 (타이머 시작)</summary>
    void StartGame();

    /// <summary>업데이트/Fixed 루프</summary>
    void UpdateGame(float deltaTime);

    /// <summary>성공/실패 확정</summary>
    void Complete(bool isSuccess);

    /// <summary>유저/시스템 중단</summary>
    void Abort(string reason = "USER_ABORT");

    /// <summary>성공 시 받을 보상 조회</summary>
    MiniGameReward[] GetRewards();

    /// <summary>정리</summary>
    void Cleanup();
}
