using UnityEngine;

public class QuestManager : SingleTonGeneric<QuestManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable() 
    {
        EventBus.Subscribe(TimeManager.OnDayStarted, OnDayStartQuest); //1.
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe(TimeManager.OnDayStarted, OnDayStartQuest); //1.
    }

    private void OnDayStartQuest()
    {
        Debug.Log("QuestManager: 하루 시작에 대한 처리 - 퀘스트 할당 부여");
    }


}
