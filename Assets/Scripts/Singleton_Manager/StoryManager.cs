using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using static UnityEngine.Rendering.DebugUI.Table;

public class StoryManager : SingleTonGeneric<StoryManager>
{
    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    [SerializeField]
    DialougeSystem dialougeSystem;

    private readonly Dictionary<int, Dictionary<string, List<DialogData>>> storyCache = new Dictionary<int, Dictionary<string, List<DialogData>>>();
    private bool isStoryCacheBuilt;
    private void OnEnable()
    {
        BuildStoryCache();

        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDayStarted); //하루 시작 이벤트      
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDayStarted);      
    }

    [SerializeField]
    private int currentDay;

    private void OnDayStarted(GameDate date)
    {
        currentDay = date.Day;   
    }
    private void StoryDialougeDataLoad(DailyRoutine routineState)
    {
        BuildStoryCache();

        string patternKey = routineState.ToString();

        if (!storyCache.TryGetValue(currentDay, out var dayCache) ||
            !dayCache.TryGetValue(patternKey, out var dialogDatas))
        {
            Debug.LogWarning($"{currentDay}의 패턴({patternKey}) 대사가 없음");
            dialougeSystem.InitSelectDialouge(Array.Empty<DialogData>(), true,DialougeType.Stroy);
            return;
        }

        dialougeSystem.InitSelectDialouge(dialogDatas.ToArray(), true, DialougeType.Stroy);
    }

    private void BuildStoryCache()
    {
        if (isStoryCacheBuilt || DailyDialougeDB == null)
        {
            return;
        }

        storyCache.Clear();

        for (int i = 0; i < DailyDialougeDB.Story.Count; i++)
        {
            var row = DailyDialougeDB.Story[i];

            if (!storyCache.TryGetValue(row.Day, out var dayCache))
            {
                dayCache = new Dictionary<string, List<DialogData>>();
                storyCache[row.Day] = dayCache;
            }

            string patternKey = row.Pattern;
           
            if (string.IsNullOrEmpty(patternKey))
            {

                continue;
            }
            if (!dayCache.TryGetValue(patternKey, out var dialogDatas))
            {
                dialogDatas = new List<DialogData>();
                dayCache[patternKey] = dialogDatas;
            }


            dialogDatas.Add(new DialogData
            {
                speakerIndex = row.PlayerId,
                name = row.PlayerName,
                dialogue = row.PlayerDialogue,
                //choices = row.Choices,
                speakerImagePos = row.PlayerImagePos
            });
        }


        isStoryCacheBuilt = true;
    }
    #region 매번 전체 탐색 - 주석 처리 
    //private void StoryDialougeDataLoad(DailyRoutine routineState)
    //{
    //    List<DialogData> dialogDatas = new List<DialogData>();

    //    for (int i = 0; i < DailyDialougeDB.Story.Count; i++)
    //    {
    //        var row = DailyDialougeDB.Story[i];

    //        if (row.Day == currentDay && row.Pattern == routineState.ToString())
    //        {
    //            dialogDatas.Add(new DialogData
    //            {
    //                speakerIndex = row.PlayerId,
    //                name = row.PlayerName,
    //                dialogue = row.PlayerDialogue,
    //                choices = row.Choices,
    //                speakerImagePos = row.PlayerImagePos
    //            });
    //        }
    //    }


    //    if (dialogDatas.Count == 0)
    //    {
    //        Debug.LogWarning($"{currentDay}의 패턴({routineState.ToString()}) 대사가 없음");
    //    }

    //    dialougeSystem.InitSelectDialouge(dialogDatas.ToArray(), true);
    //}
    #endregion
}
