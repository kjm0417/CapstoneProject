using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class StoryManager : MonoBehaviour
{
    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    [SerializeField]
    DialougeSystem dialougeSystem;

    private void OnEnable()
    {
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDayStarted); //하루 시작 이벤트

        EventBus.Subscribe<DailyRoutine>(StoryTriggerManager.OnStoryRequested, StoryDialougeDataLoad); //1. 스토리 트리거 이벤트 구독 = 스토리 진행 대사 출력
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDayStarted);

        EventBus.UnSubscribe<DailyRoutine>(StoryTriggerManager.OnStoryRequested, StoryDialougeDataLoad); //2.  스토리 트리거 이벤트 구독 해제
    }

    [SerializeField]
    private int currentDay;

    private void OnDayStarted(GameDate date)
    {
        currentDay = date.Day;

        StoryDialougeDataLoad(DailyRoutine.Room); //하루 시작은 방으로 고정 
    }

    private void StoryDialougeDataLoad(DailyRoutine routineState)
    {
        List<DialogData> dialogDatas = new List<DialogData>();

        for (int i = 0; i < DailyDialougeDB.Story.Count; i++)
        {
            var row = DailyDialougeDB.Story[i];

            if (row.Day == currentDay && row.Pattern == routineState.ToString())
            {
                dialogDatas.Add(new DialogData
                {
                    speakerIndex = row.PlayerId,
                    name = row.PlayerName,
                    dialogue = row.PlayerDialogue,
                    choices = row.Choices,
                    speakerImagePos = row.PlayerImagePos
                });
            }
        }


        if (dialogDatas.Count == 0)
        {
            Debug.LogWarning($"{currentDay}의 패턴({routineState.ToString()}) 대사가 없음");
        }

        dialougeSystem.InitSelectDialouge(dialogDatas.ToArray(), true);
    }
}
