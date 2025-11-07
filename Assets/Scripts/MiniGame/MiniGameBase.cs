using System;
using UnityEngine;

public class MiniGameBase : MonoBehaviour, IMiniGame
{
    [Header("miniGame ID 및 플레이타임")]
    [SerializeField] private string _id;
    [SerializeField] private float _duration;

    [Header("성공 실패 보상 관련")]
    [SerializeField] private MiniGameReward[] _successReward;
    [SerializeField] private MiniGameReward[] _failReward;
    [SerializeField] private MiniGameReward[] _perfectClearReward; // 완벽한 클리어(◇) 보상


    public string Id => _id;

    public float Duration => _duration;
    public float RemainingTime { get; protected set; }
    public bool IsRunning { get; protected set; }
    public bool IsPerfectClear { get; protected set; } // 완벽한 클리어 달성 여부

    public event Action<IMiniGame> OnStarted;
    public event Action<IMiniGame, float> OnTick;
    public event Action<IMiniGame, MiniGameCompleteArgs> OnCompleted;
    public event Action<IMiniGame, string> OnAborted;

    private float elapsedTime; // 경과 시간 추적을 위한 변수

    // 초기화 메서드 정의   
    public virtual void Init()
    {
        RemainingTime = Duration;
        IsRunning = false;
        elapsedTime = 0f;
        IsPerfectClear = true; // 기본값은 완벽한 클리어 상태로 시작
    }

    /// </summary>
    /// 게임 시작시 호출되는 함수
    public void StartGame()
    {
        if (IsRunning) return;
        IsRunning = true;
        elapsedTime = 0f;
        RemainingTime = Duration;
        IsPerfectClear = true; // 게임 시작 시 완벽한 클리어 상태로 초기화

        // PauseManager를 통해 다른 오브젝트들 일시정지
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.StartMiniGame();
        }

        // EventBus 알림(옵션)
        EventBus.Publish("MiniGame.Started");
        OnStarted?.Invoke(this);

        OnStartHook();

    }

    /// </summary>
    /// 성공했거나 실패 중단을 담당하는 함수
    public virtual void Complete(bool isSuccess)
    {
        if (!IsRunning) return;
        IsRunning = false;

        // PauseManager를 통해 다른 오브젝트들 재개
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.EndMiniGame();
        }

        var args = new MiniGameCompleteArgs
        {
            success = isSuccess,
            usedTime = Duration - RemainingTime,
            remainingTime = RemainingTime,
            isPerfectClear = IsPerfectClear && isSuccess // 성공하면서 완벽한 클리어 조건 달성
        };

        // 보상 적용
        MiniGameReward[] rewardList = null;
        
        if (isSuccess)
        {
            // 완벽한 클리어 시 추가 보상
            if (IsPerfectClear && _perfectClearReward != null && _perfectClearReward.Length > 0)
            {
                Debug.Log("[MiniGameBase] 완벽한 클리어(◇) 달성! 추가 보상 지급");
                foreach (var r in _perfectClearReward)
                {
                    ApplyReward(r);
                }
            }
            rewardList = _successReward;
        }
        else
        {
            rewardList = _failReward;
        }

        // 기본 보상 적용
        if (rewardList != null)
        {
            foreach (var r in rewardList)
            {
                ApplyReward(r);
            }
        }

        //ex) 포인트 할당이나 미니게임 결과에 따라 받는 보상 reward에서 다른 매니저로 전달해야 할 수 있음
        EventBus.Publish(isSuccess ? EventNames.MiniGameSuccesed : EventNames.MiniGameFailed);
        OnCompleted?.Invoke(this, args);

        OnCompleteHook(isSuccess, args);
        Cleanup();

        //EventBus.Publish(RestUIManager.OnStudyRestTimeculated);

        //EventBus.Publish(DailyRoutineManager.OnCurrentDailyDialouge);
    }

    /// <summary>
    /// 매 프레임마다 호출되는 함수
    /// </summary>
    public void UpdateGame(float deltaTime)
    {
        if (!IsRunning) return;

        elapsedTime += deltaTime;
        RemainingTime = Mathf.Max(0f, Duration - elapsedTime);

        // 스트레스 지속적으로 증가 (PlayerManager에 적용)
        ApplyStressOverTime(deltaTime);

        OnUpdateHook(deltaTime);
        OnTick?.Invoke(this, deltaTime);

        // 실시간 실패 조건 체크 (각 미니게임마다 고유한 실패 조건)
        if (CheckFailCondition())
        {
            Complete(false);
            return;
        }

        // 실시간 클리어 체크 (목표 달성 등의 경우)
        if (CheckClearRealtime())
        {
            Complete(true);
            return;
        }

        // 시간 종료 처리 (시간이 끝나면 자동으로 처리되는 미니게임용)
        // Duration이 0이면 시간 제한 없는 미니게임
        if (Duration > 0f && RemainingTime <= 0f)
        {
            OnTimeOut();
        }
    }

    /// <summary>
    /// 중단 시 호출되는 함수
    public virtual void Abort(string reason = "USER_ABORT")
    {
        if (!IsRunning)
            return;
        IsRunning = false;

        // PauseManager를 통해 다른 오브젝트들 재개
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.EndMiniGame();
        }

        OnAborted?.Invoke(this, reason);
        EventBus.Publish("MiniGame.Aborted");

        Cleanup();
    }



    /// <summary>
    /// 게임 종료 후 정리해야 할 작업
    /// </summary>
    public virtual void Cleanup()
    {
        OnCleanupHook();
    }

    public virtual MiniGameReward[] GetRewards() => _successReward;

    /// <summary>
    /// 미니게임 완료 후 보상 적용 처리
    /// </summary>
    protected virtual void ApplyReward(MiniGameReward reward)
    {
        var playerManager = PlayerManager.Instance;
        if (playerManager == null)
        {
            Debug.LogWarning("[MiniGameBase] PlayerManager instance is not available.");
            return;
        }

        playerManager.ApplyReward(reward);
    }

    /// <summary>
    /// 게임 진행 중 스트레스 지속 증가
    /// </summary>
    protected virtual void ApplyStressOverTime(float deltaTime)
    {

    }

    /// <summary>
    /// 완벽한 클리어 조건 실패 처리 (자식 클래스에서 호출)
    /// 예: 장애물에 부딪혔을 때
    /// </summary>
    protected void FailPerfectClear()
    {
        if (IsPerfectClear)
        {
            IsPerfectClear = false;
            Debug.Log("[MiniGameBase] 완벽한 클리어(◇) 조건 실패");
        }
    }
    // ====== 확장 훅 ======

    /// <summary>게임 시작 시 1회 호출</summary>
    protected virtual void OnStartHook() { }

    /// <summary>매 프레임 호출</summary>
    protected virtual void OnUpdateHook(float deltaTime) { }

    /// <summary>완료 시점 호출</summary>
    protected virtual void OnCompleteHook(bool success, MiniGameCompleteArgs args) { }

    /// <summary>정리 단계 호출</summary>
    protected virtual void OnCleanupHook() { }

    /// <summary>
    /// 실시간 클리어 조건 체크 (기본: 미달성)
    /// 자식 클래스에서 오버라이드하여 클리어 조건 구현
    /// 예: 목표 점수 달성, 모든 아이템 수집 등
    /// </summary>
    protected virtual bool CheckClearRealtime() => false;

    /// <summary>
    /// 실시간 실패 조건 체크 (기본: 없음)
    /// 자식 클래스에서 오버라이드하여 실패 조건 구현
    /// 예: 체력이 0이 됨, 3번 실수함 등
    /// </summary>
    protected virtual bool CheckFailCondition() => false;

    /// <summary>
    /// 시간 종료 시 호출 (기본: 아무 동작 안함)
    /// 자식 클래스에서 오버라이드하여 타임아웃 처리
    /// 시간이 끝나면 자동으로 성공/실패 처리가 필요한 경우 구현
    /// </summary>
    protected virtual void OnTimeOut() { }
}

