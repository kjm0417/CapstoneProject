using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingleTonGeneric<UIManager> //UI 동적 생성 관리
{
    [SerializeField]
    SubjectItem SubjectItem;

    [SerializeField]
    Transform SubjectItemParent;

    private ObjectPool<SubjectItem> itemPool;

    private List<SubjectItem> activeItems = new List<SubjectItem>(); // 현재 사용 중인 아이템 캐싱

    public static string OnRestQuestions = "OnRestQuestionEvent"; //얼마나 쉴지 보여주는 UI 이벤트

    public static string OnRestTimeSelected = "OnRestTimeSelectEvent"; //얼마나 쉴지 선택 완료 이벤트

    [SerializeField]
    GameObject RestQuestionUI;
    protected override void Awake()
    {
        base.Awake();

        //런타임 중 Instantiate 최소화
        itemPool = new ObjectPool<SubjectItem>(SubjectItem, 7, SubjectItemParent);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<List<QuestData>>(QuestManager.OnQuestStarted, UpdateQuestUI);

        EventBus.Subscribe<int>(OnRestTimeSelected, ConfirmRestTime);

        EventBus.Subscribe<bool>(OnRestQuestions, OnRestQuestUI);
    }

    private void OnDisable()
    {     
        EventBus.UnSubscribe<List<QuestData>>(QuestManager.OnQuestStarted, UpdateQuestUI);

        EventBus.UnSubscribe<int>(OnRestTimeSelected, ConfirmRestTime);

        EventBus.UnSubscribe<bool>(OnRestQuestions, OnRestQuestUI);
    }


    public void ConfirmRestTime(int minutes)
    {
        Debug.Log($"전달 받은 휴식 타임 {minutes};");

        OnRestQuestUI(false);
    }
    private void UpdateQuestUI(List<QuestData> questDataList) //퀘스트가 시작 되면 호출
    {
        foreach(SubjectItem subjectItem in activeItems)
        {
            itemPool.Return(subjectItem);
        }

        activeItems.Clear();

        foreach(QuestData questData in questDataList)
        {
            SubjectItem subjectItem = itemPool.Get();
            subjectItem.SubjectSet(questData.Period, questData.Subject, questData.StartPage, questData.EndPage);
            activeItems.Add(subjectItem);
        }
       
    }

    private void OnRestQuestUI(bool isActive)
    {
        DailyRoutine dailyRoutine = DailyRoutineManager.Instance.DailyRoutine;
        string nextPattern = dailyRoutine.ToString();

        if (isActive == false)
        {
            EventBus.Publish(EffectManager.OnSelectEffectStarted, "쉰다", nextPattern);
        }
        RestQuestionUI.gameObject.SetActive(isActive);
    }
}
