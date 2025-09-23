using System;
using UnityEngine;

public class MiniGameBase : MonoBehaviour, IMiniGame
{
    [Header("기본 설정")]
    [SerializeField] private string _id;
    [SerializeField] private float _duration;

    [Header("결과에 따른 내용")]
    [SerializeField] private MiniGameReward[] _sucessReward;
    [SerializeField] private MiniGameReward[] _failRewaord;

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
    public void Init()
    {
        RemainingTime = Duration;
        IsRunning = false;
        elapsedTime = 0f;
    }

    /// </summary>
    /// 게임 시작시 호출되는 함수
    public void StartGame()
    {
        if(IsRunning)
        {
            return; // 이미 실행 중이면 무시
        }
        IsRunning = true;
        elapsedTime = 0f;

        EventBus.Publish("MiniGameStarted");
        OnStarted?.Invoke(this);

    }

    /// </summary>
    /// 성공했는지 실패 했는지 넘겨주는 함수
    public void Complete(bool isSuccess)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///매 프레임마다 호출되는 함수
    public void UpdateGame(float deltaTime)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 중단  시 호출되는 함수
    public void Abort(string reason = "USER_ABORT")
    {
        
    }

    /// <summary>
    /// 게임 종료 후 정리해야 할 내용
    public void Cleanup()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 미니게임 성공 시 지급 보상 조회
    public MiniGameReward[] GetRewards()
    {
        throw new NotImplementedException();
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
