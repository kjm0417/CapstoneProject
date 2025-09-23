using System;
using UnityEngine;

#region Result/Reward Models
/// <summary>
/// 보상 타입(필요 시 자유 확장)
/// </summary>
public enum RewardType
{
    StressDown,
    StressUp,
    ConcentrationUp,
    cuncentrationDown,
}

/// <summary>
/// 미니게임 보상 구조체
/// - amount: 수치(정수), param: 추가 파라미터(아이템ID 등)
/// </summary>
[Serializable]
public struct MiniGameReward
{
    public RewardType type; //보상 타입
    public int amount; //값
}

/// <summary>
/// 완료시 콜백 인자(성공/소요/남은/코드)
/// </summary>
[Serializable]
public struct MiniGameCompleteArgs
{
    public bool success; //성공여부
    public float usedTime; //소요시간
    public float remainingTime; //남은시간 or 오버한 시간
}
#endregion

/// <summary>
/// 모든 미니게임이 따라야 하는 최소 공통 인터페이스
/// - 공통 속성: Id/Duration/RemainingTime/IsRunning
/// - 공통 이벤트: 시작/틱/완료/중단
/// - 공통 수명주기 메서드
/// </summary>
public interface IMiniGame
{
    // 고유 식별자(프리팹/컴포넌트 단위로 부여)
    string Id { get; }

    // 제한 시간(초)
    float Duration { get; }

    // 남은 시간(초)
    float RemainingTime { get; }

    // 진행 중 여부
    bool IsRunning { get; }

    // 이벤트 훅
    event Action<IMiniGame> OnStarted;
    event Action<IMiniGame, float> OnTick;
    event Action<IMiniGame, MiniGameCompleteArgs> OnCompleted;
    event Action<IMiniGame, string> OnAborted;

    /// <summary>외부 의존성 준비(필요 시 호출, 선택)</summary>
    void Init();

    /// <summary>게임 시작(타이머 가동)</summary>
    void StartGame();

    /// <summary>프레임/Fixed 갱신</summary>
    void UpdateGame(float deltaTime);

    /// <summary>성공/실패 확정</summary>
    void Complete(bool isSuccess);

    /// <summary>사용자/시스템 중단</summary>
    void Abort(string reason = "USER_ABORT");

    /// <summary>성공 시 지급 보상 조회</summary>
    MiniGameReward[] GetRewards();

    /// <summary>정리</summary>
    void Cleanup();
}
