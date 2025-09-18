using UnityEngine;

public class QuestManager : SingleTonGeneric<QuestManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable() 
    {
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독
    }

    private void OnDayStartQuest(GameDate date)
    {
        Debug.Log("QuestManager: 하루 시작에 대한 처리 - 퀘스트 할당 부여");
    }


}
