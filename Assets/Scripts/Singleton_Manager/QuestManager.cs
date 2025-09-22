using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class QuestManager : SingleTonGeneric<QuestManager>
{
    //퀘스트 변경 조건
    //1. 플레이어가 변경됐을 때
    //2. 하루가 바뀌었을 때, 

    [SerializeField]
    private QuestDataBaseExcel questDB;

    public static string OnQuestStarted = "OnQuestStartEvent";

    [SerializeField]
    int CurrentDay;  //현재 일자 - 용도 : 현재 일자가 증가됨에 따라 과목(퀘스트)의 범위가 늘어남
                     //= 플레이 시간이 길 수록 난이도 증가

    protected override void Awake()
    {
        base.Awake();

    }

    private void OnEnable() 
    {
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독
        //플레이어 교체 이벤트 구독

    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독해제
        //플레이어 교체 이벤트 구독 해제
    }


    private void OnDayStartQuest(GameDate gameDate)
    {
        CurrentDay = -gameDate.RemaineDay + 32; //-31 + 32 = 1일차, -30 + 32 = 2일차 ... -1 + 32 = 31일차

        Debug.Log($"QuestManager / 하루 ( {CurrentDay} 일차 )에 대한 처리 - 퀘스트 할당 및 페이지 범위 변경");

        //여기서 현재 플레이어가 누군지 알아내고 아래 코드 호출
        AssignPagesForDay(questDB.Juhee);
        OnQuestChanged("Juhee");

        
    }
    private void AssignPagesForDay(List<QuestData> questList) 
    {
        if (questList == null) return;

        foreach (QuestData questData in questList)
        {
            //@@@@@@@@@@@@@@@@@@@@@@ foreach 순회 설명 - 1일차 기준 @@@@@@@@@@@@@@@

            int prevEnd = questData.EndPage; //어제 마지막 페이지는 없으므로 prevEnd = 0, 있다면 어제 마지막에 페이지 저장

            //2. 시작 페이지 = 어제 마지막 페이지 + 1 = 1
            int startPage = prevEnd == 0 ? 1 : prevEnd + 1; //마지막 페이지가 0이라면 1을 반환, 0이 아니라면 마지막페에지 + 1
            //ex) 1~

            //3. 날짜가 늘어날수록 페이지 범위 증가
            int minStep = 3 + (CurrentDay / 2); //3 + (1/2) = 3
            int maxStep = 8 + CurrentDay; // 8 + 1 = 9

            int pageRange = Random.Range(minStep, maxStep + 1);  //(3,9) 만약 5라면

            int endPage = startPage + pageRange - 1; //endPage = 1 + 5 - 1 = 5

            // QuestData 갱신
            questData.StartPage = startPage; //시작 페이지 = 1
            questData.EndPage = endPage; //마지막 페이지 5

            Debug.Log($"[{questData.Subject}] {CurrentDay}일차 범위: {questData.StartPage} ~ {questData.EndPage}p");
        }
    }

    private void OnQuestChanged(string playerName)
    {
        switch(playerName)
        {
            case "Juhee":
                OnQuestStartUI(questDB.Juhee);
                break;
            case "Chaun":
                OnQuestStartUI(questDB.Chaun);
                break;
            case "Minzoo":
                OnQuestStartUI(questDB.Minzoo);
                break;
        }
        Debug.Log($"QuestManager / 현재 캐릭터 이름 :{playerName} / 퀘스트 이벤트 발행 ");

    }

    private void OnQuestStartUI<T>(List<T> questData)
    {
        EventBus.Publish(OnQuestStarted, questData); //UIManager에서 퀘스트 UI 생성하는 이벤트 발행
    }

}
