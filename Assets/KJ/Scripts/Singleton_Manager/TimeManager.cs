using UnityEngine;

public class TimeManager : SingleTonGeneric<TimeManager>
{
    public static string OnDayStarted = "OnDayStartEvent"; //하루 일과 시작 될 때
    public static string OnSlotChanged = "OnSlotChangeEvent"; //
    public static string OnMiniGameEnded = "OnMiniGameEndEvent"; //하나의 미니게임이 끝났을 때
    public static string OnDialogueEnded = "OnDialogueEndEvent"; //

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        StartDay();
    }

    public void StartDay() //하루 일과 시작
    {
        Debug.Log("TimeManager: 하루 일과 시작 - UI 업데이트");
        EventBus.Publish(OnDayStarted); 
    }
}
