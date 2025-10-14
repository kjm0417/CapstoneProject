using System;
using UnityEngine;

public class MiniGameBase : MonoBehaviour, IMiniGame
{
    [Header("기본 설정")]
    [SerializeField] private string _id;
    [SerializeField] private float _duration;

    [Header("결과에 따른 내용")]
    [SerializeField] private MiniGameReward[] _successReward;
    [SerializeField] private MiniGameReward[] _failReward;

    public string Id => _id;

    public float Duration => _duration;
    public float RemainingTime { get; protected set; }
    public bool IsRunning { get; protected set; }

    public event Action<IMiniGame> OnStarted;
    public event Action<IMiniGame, float> OnTick;
    public event Action<IMiniGame, MiniGameCompleteArgs> OnCompleted;
    public event Action<IMiniGame, string> OnAborted;

    private float elapsedTime; // 경과 시간 추적용 변수

    // 수명주기 메서드 구현   
    public virtual void Init()
    {
        RemainingTime = Duration;
        IsRunning = false;
        elapsedTime = 0f;
    }

    /// </summary>
    /// 게임 시작시 호출되는 함수
    public void StartGame()
    {
        if (IsRunning) return;
        IsRunning = true;
        elapsedTime = 0f;
        RemainingTime = Duration;

        // EventBus 알림(옵션)
        EventBus.Publish("MiniGame.Started");
        OnStarted?.Invoke(this);

        OnStartHook();

    }

    /// </summary>
    /// 성공했는지 실패 했는지 넘겨주는 함수
    public virtual void Complete(bool isSuccess)
    {
        if (!IsRunning) return;
        IsRunning = false;

        var args = new MiniGameCompleteArgs
        {
            success = isSuccess,
            usedTime = Duration - RemainingTime,
            remainingTime = RemainingTime
        };

        // 보상 지급
        var list = isSuccess ? _successReward : _failReward;
        if (list != null)
        {
            foreach (var r in list)
            {
                ApplyReward(r);
            }
        }

        EventBus.Publish(isSuccess ? "MiniGame.Success" : "MiniGame.Fail");
        OnCompleted?.Invoke(this, args);

        OnCompleteHook(isSuccess, args);
        Cleanup();
    }

    /// <summary>
    ///매 프레임마다 호출되는 함수
    public void UpdateGame(float deltaTime)
    {
        if (!IsRunning) return;

        elapsedTime += deltaTime;
        RemainingTime = Mathf.Max(0f, Duration - elapsedTime);

        OnUpdateHook(deltaTime);
        OnTick?.Invoke(this, deltaTime);

        // 시간 만료 처리(오버하면 실패/성공 여부는 훅에서 결정)
        if (RemainingTime <= 0f)
        {
            bool success = CheckClearOnTimeout();
            Complete(success);
            return;
        }

        // 실시간 클리어 체크(목표 달성 즉시 성공)
        if (CheckClearRealtime())
        {
            Complete(true);
        }



    }

    /// <summary>
    /// 중단  시 호출되는 함수
    public virtual void Abort(string reason = "USER_ABORT")
    {
        if (!IsRunning)
            return;
        IsRunning = false;

        OnAborted?.Invoke(this, reason);
        EventBus.Publish("MiniGame.Aborted");

        Cleanup();
    }



    /// <summary>
    /// 게임 종료 후 정리해야 할 내용
    public virtual void Cleanup()
    {
        OnCleanupHook();
    }

    public virtual MiniGameReward[] GetRewards() => _successReward;
    /// <summary>
    /// 미니게임 성공 시 지급 보상 조회
    protected virtual void ApplyReward(MiniGameReward reward)
    {
        // PlayerManager 연동 예시 (실제 메서드에 맞게 수정)
        switch (reward.type)
        {
            case RewardType.StressDown:
                Debug.Log($"[Reward] -stress : {reward.amount}");
                break;
            case RewardType.StressUp:
                Debug.Log($"[Reward] +stress : {reward.amount}");
                break;
            case RewardType.cuncentrationDown:
                Debug.Log($"[Reward] -cuncentration : {reward.amount}");
                break;
            case RewardType.ConcentrationUp:
                Debug.Log($"[Reward] +concentration : {reward.amount}");
                break;
           
        }
    }

    // ====== 확장 훅 ======

    /// <summary>시작 시 1회 호출</summary>
    protected virtual void OnStartHook() { }

    /// <summary>매 프레임 호출</summary>
    protected virtual void OnUpdateHook(float deltaTime) { }

    /// <summary>완료 직후 호출</summary>
    protected virtual void OnCompleteHook(bool success, MiniGameCompleteArgs args) { }

    /// <summary>정리 단계 호출</summary>
    protected virtual void OnCleanupHook() { }

    /// <summary>시간 만료 시 성공 여부(기본: 실패)</summary>
    protected virtual bool CheckClearOnTimeout() => false;

    /// <summary>실시간 클리어 여부(기본: 미사용)</summary>
    protected virtual bool CheckClearRealtime() => false;
}
