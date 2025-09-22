using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class QuestManager : SingleTonGeneric<QuestManager>
{
    //퀘스트 변경 조건
    //1. 플레이어가 변경됐을 때
    //2. 하루가 바뀌었을 때, 

    [SerializeField]
    private int branch;
    [SerializeField]
    private QuestDataBaseExcel questDB;

    public static string OnQuestStarted = "OnQuestStartEvent";

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable() 
    {
        EventBus.Subscribe(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독
    }

    private void OnDayStartQuest()
    {
        Debug.Log("QuestManager / 하루 시작에 대한 처리 - 퀘스트 할당 부여");

        EventBus.Publish(OnQuestStarted, questDB); //UIManager에서 퀘스트 UI 생성하는 메서드 호출
    }

}
