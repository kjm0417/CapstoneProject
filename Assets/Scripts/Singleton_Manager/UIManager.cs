using UnityEngine;

public class UIManager : SingleTonGeneric<UIManager> //UI 동적 생성 관리
{
  
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<QuestDataBaseExcel>(QuestManager.OnQuestStarted, UpdateQuestUI);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<QuestDataBaseExcel>(QuestManager.OnQuestStarted, UpdateQuestUI);
    }

    private void UpdateQuestUI(QuestDataBaseExcel questDatabase) //퀘스트가 시작 되면 호출
    {
        Debug.Log("UIManager/ 날짜 변경으로 인한 퀘스트 UI 생성 또는 재생성");
    }
}
