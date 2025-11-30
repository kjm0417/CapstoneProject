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

    private Dictionary<int, List<QuestData>> questByDay = new Dictionary<int, List<QuestData>>();
    protected override void Awake()
    {
        base.Awake();

        #region 저장 시스템 구현 시, 주석 풀기
        //if (SaveSystem.HasSaveData())
        //{
        //    // 저장된 데이터 불러오기
        //    LoadQuestDataFromSave();
        //}
        //else
        //{
        //    foreach (var questList in GetAllQuestLists())
        //    {
        //        foreach (var quest in questList)
        //        {
        //            quest.StartPage = 0;
        //            quest.EndPage = 0;
        //        }
        //    }
        //}
        #endregion


        //foreach (var questList in GetAllQuestLists())
        //{
        //    foreach (var quest in questList)
        //    {
        //        quest.StartPage = 0;
        //        quest.EndPage = 0;
        //    }
        //}

    }
    private void Start()
    {
        GroupQuestDataByDay();
    }
    private void GroupQuestDataByDay()
    {
        questByDay.Clear();

        foreach (QuestData quest in questDB.QuestList)
        {
            if (!questByDay.ContainsKey(quest.Day))
            {
                questByDay[quest.Day] = new List<QuestData>();
            }

            questByDay[quest.Day].Add(quest);
        }

        Debug.Log($"총 {questByDay.Count}개의 Day 데이터 그룹화 완료");
    }
    private void OnEnable()
    {
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독
        EventBus.Subscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, OnPlayerChanged);

    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDayStartQuest); //하루 시작 이벤트 구독해제
        EventBus.UnSubscribe<IPlayerStateReadOnly>(EventNames.ActiveCharacterChanged, OnPlayerChanged);
    }

    /// <summary>
    /// 날짜 업데이트 이벤트 발행 시 호출되는 메서드
    /// </summary>
    private void OnDayStartQuest(GameDate gameDate)
    {
        CurrentDay = gameDate.Day; 

        Debug.Log($"QuestManager / 하루 ( {CurrentDay} 일차 )에 대한 처리 - 퀘스트 할당 및 페이지 범위 변경");

        //InitPageRange();

        OnQuestChanged();
    }



    /// <summary>
    /// 캐릭터 변경 이벤트 발행 시 호출되는 메서드
    /// </summary>
    private void OnPlayerChanged(IPlayerStateReadOnly state)
    {
        OnQuestChanged();
    }

    /// <summary>
    /// 날짜에 따라 QuestDataBaseExcel의 QuestData를 갖고 있는 리스트를 설정
    /// </summary>
    private void OnQuestChanged()
    {
        if (questByDay.TryGetValue(CurrentDay, out var questList))
        {
            Debug.Log($"[{CurrentDay}일차] 퀘스트 {questList.Count}개 로드 완료");
            OnQuestStartUI(questList);
        }
        else
        {
            Debug.LogWarning($"[{CurrentDay}일차] 데이터가 없습니다.");
        }
    }



    /// <summary>
    /// 퀘스트 UI 생성을 하기 위해 퀘스트 시작하는 이벤트를 구독하고 있는 구독자에게 이벤트를 발행
    /// </summary>
    private void OnQuestStartUI<T>(List<T> questData)
    {
        EventBus.Publish(OnQuestStarted, questData); //UIManager에서 퀘스트 UI 생성하는 이벤트 발행
    }

    #region 페이지 범위 랜덤
    /// <summary>
    /// 페이지 범위 계산
    /// </summary>
    //private void AssignPagesForDay(List<QuestData> questList)
    //{
    //    if (questList == null) return;

    //    foreach (QuestData questData in questList)
    //    {
    //        //@@@@@@@@@@@@@@@@@@@@@@ foreach 순회 설명 - 1일차 기준 @@@@@@@@@@@@@@@

    //        int prevEnd = questData.EndPage; //어제 마지막 페이지는 없으므로 prevEnd = 0, 있다면 어제 마지막에 페이지 저장

    //        //2. 시작 페이지 = 어제 마지막 페이지 + 1 = 1
    //        int startPage = prevEnd == 0 ? 1 : prevEnd + 1; //마지막 페이지가 0이라면 1을 반환, 0이 아니라면 마지막페에지 + 1
    //        //ex) 1~

    //        //3. 날짜가 늘어날수록 페이지 범위 증가
    //        int minStep = 3 + (CurrentDay / 2); //3 + (1/2) = 3
    //        int maxStep = 8 + CurrentDay; // 8 + 1 = 9

    //        int pageRange = Random.Range(minStep, maxStep + 1);  //(3,9) 만약 5라면

    //        int endPage = startPage + pageRange - 1; //endPage = 1 + 5 - 1 = 5

    //        // QuestData 갱신
    //        questData.StartPage = startPage; //시작 페이지 = 1
    //        questData.EndPage = endPage; //마지막 페이지 5

    //        Debug.Log($"[{questData.Subject}] {CurrentDay}일차 범위: {questData.StartPage} ~ {questData.EndPage}p");
    //    }
    //}
    #endregion

    #region 초기 페이지 범위 설정
    /// <summary>
    /// 페이지 범위 설정하는 메서드를 인자로 QuestDataBaseExcel의 각 리스트의 리스트를 넘겨 호출
    /// </summary>
    //private void InitPageRange()
    //{
    //    foreach (var questList in GetAllQuestLists())
    //    {
    //        AssignPagesForDay(questList);
    //    }
    //}
    #endregion

    #region QuestData를 들고있는 리스트를 반환
    ///// <summary>
    ///// QuestDataBaseExcel의 각 QuestData를 물고 있는 List를 반환한다.
    ///// </summary>
    //private IEnumerable<List<QuestData>> GetAllQuestLists()
    //{
    //    yield return questDB.QuestList;
    //}
    #endregion
}
