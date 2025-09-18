using UnityEngine;

public class QuestManager : SingleTonGeneric<QuestManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable() 
    {
        EventBus.Subscribe(TimeManager.OnDayStarted, OnDayStartHandler); //1.
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe(TimeManager.OnDayStarted, OnDayStartHandler); //1.
    }

    private void OnDayStartHandler()
    {
        Debug.Log("QuestManager: 하루 시작에 대한 처리 - 미니게임 할당 부여");
    }


}
