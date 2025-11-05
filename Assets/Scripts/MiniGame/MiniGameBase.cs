using System;
using UnityEngine;

public class MiniGameBase : MonoBehaviour, IMiniGame
{
    [Header("�⺻ ����")]
    [SerializeField] private string _id;
    [SerializeField] private float _duration;

    [Header("����� ���� ����")]
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

    private float elapsedTime; // ��� �ð� ������ ����

    // �����ֱ� �޼��� ����   
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
            remainingTime = RemainingTime
        };

        // 보상 적용
        var list = isSuccess ? _successReward : _failReward;
        if (list != null)
        {
            foreach (var r in list)
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
    ///�� �����Ӹ��� ȣ��Ǵ� �Լ�
    public void UpdateGame(float deltaTime)
    {
        if (!IsRunning) return;

        elapsedTime += deltaTime;
        RemainingTime = Mathf.Max(0f, Duration - elapsedTime);

        OnUpdateHook(deltaTime);
        OnTick?.Invoke(this, deltaTime);

        // �ð� ���� ó��(�����ϸ� ����/���� ���δ� �ſ��� ����)
        if (RemainingTime <= 0f)
        {
            bool success = CheckClearOnTimeout();
            Complete(success);
            return;
        }

        // �ǽð� Ŭ���� üũ(��ǥ �޼� ��� ����)
        if (CheckClearRealtime())
        {
            Complete(true);
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
    /// ���� ���� �� �����ؾ� �� ����
    public virtual void Cleanup()
    {
        OnCleanupHook();
    }

    public virtual MiniGameReward[] GetRewards() => _successReward;
    /// <summary>
    /// �̴ϰ��� ���� �� ���� ���� ��ȸ
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
    // ====== Ȯ�� �� ======

    /// <summary>���� �� 1ȸ ȣ��</summary>
    protected virtual void OnStartHook() { }

    /// <summary>�� ������ ȣ��</summary>
    protected virtual void OnUpdateHook(float deltaTime) { }

    /// <summary>�Ϸ� ���� ȣ��</summary>
    protected virtual void OnCompleteHook(bool success, MiniGameCompleteArgs args) { }

    /// <summary>���� �ܰ� ȣ��</summary>
    protected virtual void OnCleanupHook() { }

    /// <summary>�ð� ���� �� ���� ����(�⺻: ����)</summary>
    protected virtual bool CheckClearOnTimeout() => false;

    /// <summary>�ǽð� Ŭ���� ����(�⺻: �̻��)</summary>
    protected virtual bool CheckClearRealtime() => false;
}

