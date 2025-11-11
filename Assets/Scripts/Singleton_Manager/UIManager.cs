using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingleTonGeneric<UIManager> //UI 동적 생성 관리 및 UI 시각적 제어만 담당
{
    [SerializeField]
    SubjectItem SubjectItem;

    [SerializeField]
    Transform SubjectItemParent;

    private ObjectPool<SubjectItem> itemPool;

    private List<SubjectItem> activeItems = new List<SubjectItem>(); // 현재 사용 중인 아이템 캐싱

    public static string OnRestQuestions = "OnRestQuestionEvent"; //얼마나 쉴지 보여주는 UI 이벤트

    public static string OnInGameUI = "OnInGameUIEvent"; //전체적인 인 게임 UI 이벤트


    [SerializeField]
    GameObject RestQuestionUI; //얼마나 쉴지 보여주는 UI

    [SerializeField]
    GameObject InGameUI;
    protected override void Awake()
    {
        base.Awake();

        itemPool = new ObjectPool<SubjectItem>(SubjectItem, 11, SubjectItemParent);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<List<QuestData>>(QuestManager.OnQuestStarted, UpdateQuestUI);

        EventBus.Subscribe<bool>(OnInGameUI, OnIngameUI);

        EventBus.Subscribe<bool>(OnRestQuestions, OnRestQuestUI);
    }

    private void OnDisable()
    {     
        EventBus.UnSubscribe<List<QuestData>>(QuestManager.OnQuestStarted, UpdateQuestUI);

        EventBus.UnSubscribe<bool>(OnInGameUI, OnIngameUI);

        EventBus.UnSubscribe<bool>(OnRestQuestions, OnRestQuestUI);

    }


 
    private void UpdateQuestUI(List<QuestData> questDataList) //퀘스트가 시작 되면 호출
    {

        foreach (SubjectItem subjectItem in activeItems)
        {
            itemPool.Return(subjectItem);
        }

        activeItems.Clear();

        foreach (QuestData questData in questDataList)
        {
            SubjectItem subjectItem = itemPool.Get();
            subjectItem.SubjectSet(questData.Subject,questData.StudyTime, questData.MasterID);
            activeItems.Add(subjectItem);
        }

    }

    private void OnIngameUI(bool isActive)
    {
        InGameUI.gameObject.SetActive(isActive);
    }
    private void OnRestQuestUI(bool isActive)
    {
        RestQuestionUI.gameObject.SetActive(isActive);
    }
}
