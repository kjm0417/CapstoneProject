using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingleTonGeneric<UIManager> //UI 동적 생성 관리
{
    [SerializeField]
    GameObject SubjectItem;

    [SerializeField]
    Transform SubjectItemParent;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<List<QuestData>>(QuestManager.OnQuestStarted, UpdateQuestUI);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<List<QuestData>>(QuestManager.OnQuestStarted, UpdateQuestUI);
    }

    private void UpdateQuestUI(List<QuestData> questDataList) //퀘스트가 시작 되면 호출
    {
        foreach( Transform item in SubjectItemParent)
        {
            Destroy(item.gameObject);
        }

        foreach(QuestData questData in questDataList)
        {
            GameObject subjectItem = Instantiate(SubjectItem, SubjectItemParent);
            subjectItem.GetComponent<SubjectItem>().
                SubjectSet(questData.Period, questData.Subject,questData.StartPage,questData.EndPage);
       
        }
       
    }
}
