using UnityEngine;

/// <summary>
/// 싱글톤으로 된 클래스들의 인스턴스 초기화 순서가 꼬이지 않게 만들어진 클래스. 
/// </summary>
public class SingletonInitializer : MonoBehaviour
{
    private void Awake()
    {
        PlayerManager.Init();
        TimeManager.Init();
        DialougeSystem.Init();
        DailyRoutineManager.Init();
       // DailySelectButtonManager.Init();
        UIManager.Init();
        QuestManager.Init();
        MiniGameManager.Init();
        PauseManager.Init();
        //StoryTriggerManager.Init();
        FadeInOutManager.Init();
        DotweenUIManager.Init();


    }
}
