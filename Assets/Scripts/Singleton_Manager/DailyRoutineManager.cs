using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DailyRoutine
{
    Room, //방
    GotoSchool, //등교
    ArriveSchool_Breakfast, //학교 - 아침 시간
    ArriveSchool_Morning, //학교 - 오전 시간
    ArriveSchool_Lanch, //학교 - 점심
    ArriveSchool_Afternoon, //학교 - 오후 시간
    Parm, //야자
    AfterSchool, //야자 연장 or 학원
}
public class DailyRoutineManager : SingleTonGeneric<DailyRoutineManager> //일상 변경 및 선택 대사를 위한 매니저
{
    public static string OnDailyChanged = "OnDailyChange";

    public static string OnDailyWakeUp = "OnDailyWakeUpEvent";

    [SerializeField]
    DialougeSystem dialougeSystem;

    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    private readonly Dictionary<int, Dictionary<string, List<DialogData>>> DailyCache = new Dictionary<int, Dictionary<string, List<DialogData>>>();
    private bool isStoryCacheBuilt;

    [SerializeField]
    private int currentDay;

    [SerializeField]
    private DailyRoutine dailyRoutine;

    public DailyRoutine DailyRoutine
    {
        get
        {
            return dailyRoutine;
        }
        set
        {
            dailyRoutine = value;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("awake");
    }


    private void OnEnable()
    {
        BuildDailyPattenrCache();

        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //1. 하루 시작 이벤트 구독

        EventBus.Subscribe<string>(OnDailyChanged, OnDailyRoutineChange);   //침대에서 일어난 상태 이벤트 구독 해제

        EventBus.Subscribe(OnDailyWakeUp, OnChangeGotoSchoolUI);   //침대에서 일어난 상태 이벤트 구독 해제

    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //하루 시작 이벤트 구독 해제

        EventBus.UnSubscribe<string>(OnDailyChanged, OnDailyRoutineChange);   //침대에서 일어난 상태 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyWakeUp, OnChangeGotoSchoolUI);   //침대에서 일어난 상태 이벤트 구독 해제
    }

    private void OnDailyRoutineChange(string NextPattern) 
    {
        switch (NextPattern)
        {
            case "GotoSchool":
                DailyRoutine = DailyRoutine.GotoSchool; //등교 패턴으로 변경  
                break;
            case "ArriveSchool_Breakfast":
                DailyRoutine = DailyRoutine.ArriveSchool_Breakfast; //학교 아침 시간
                break;
            case "ArriveSchool_Morning":
                DailyRoutine = DailyRoutine.ArriveSchool_Morning; //학교 오전시간
                break;
            case "ArriveSchool_Lanch":
                DailyRoutine = DailyRoutine.ArriveSchool_Lanch; //학교 점심시간
                break;
            case "ArriveSchool_Afternoon":
                DailyRoutine = DailyRoutine.ArriveSchool_Afternoon; //학교 오후시간
                break;
            case "Parm":
                DailyRoutine = DailyRoutine.Parm; //야자 패턴으로 변경
                break;
            case "AfterSchool":
                DailyRoutine = DailyRoutine.AfterSchool; //학원 패턴으로 변경
                break;
        }
        OnChangeRoutine(DailyRoutine);
    }

    private void OnDailyStart(GameDate date)
    {
        currentDay = date.Day;
        DailyRoutine = DailyRoutine.Room;

        EventBus.Publish(UIManager.OnInGameUI, false);
    }
    
    private void OnChangeRoutine(DailyRoutine dailyRoutine)
    {

        DailyPatternDialougeDataLoad(dailyRoutine);
    }
    private void OnChangeGotoSchoolUI() //방 -> 등교 변경
    {
        DailyPatternDialougeDataLoad(dailyRoutine);
    }

    ///// <summary>
    ///// 선택 대사의 데이터들을 DialougeSystem에 전달하는 메서드. 실질적인 대화 진행은 DialougeSystem 처리
    ///// <summary>
    private void DailyPatternDialougeDataLoad(DailyRoutine routineState)
    {
        BuildDailyPattenrCache();

        string patternKey = routineState.ToString();

        if (!DailyCache.TryGetValue(currentDay, out var dayCache) ||
             !dayCache.TryGetValue(patternKey, out var dialogDatas))
        {
            Debug.LogWarning($"{currentDay}의 패턴({patternKey}) 대사가 없음");
            dialougeSystem.InitSelectDialouge(Array.Empty<DialogData>(), true);
            return;
        }

        dialougeSystem.InitSelectDialouge(dialogDatas.ToArray(), true);
    }

    private void BuildDailyPattenrCache()
    {
        if (isStoryCacheBuilt || DailyDialougeDB == null)
        {
            return;
        }

        DailyCache.Clear();

        for (int i = 0; i < DailyDialougeDB.Transition.Count; i++)
        {
            var row = DailyDialougeDB.Transition[i];

            if (!DailyCache.TryGetValue(row.Day, out var dayCache))
            {
                dayCache = new Dictionary<string, List<DialogData>>();
                DailyCache[row.Day] = dayCache;
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

            List<ChoiceData> choices = new List<ChoiceData>();
            foreach (var choiceRow in DailyDialougeDB.TransitionEffect)
            {
                
                if (choiceRow.MasterId == row.MasterId) // FK 매칭
                {
                    choices.Add(new ChoiceData
                    {
                        ChoiceId = choiceRow.ChoiceId,

                        Text = choiceRow.ChoiceText,
                        EffectId = choiceRow.EffectId,
                        NextPattern = choiceRow.NextPattern,
                    });

                }
            }

            dialogDatas.Add(new DialogData
            {
                speakerIndex = row.PlayerId,
                name = row.PlayerName,
                dialogue = row.PlayerDialogue,
                choices = choices,
                speakerImagePos = row.PlayerImagePos
            });
        }
        isStoryCacheBuilt = true;
    }

    #region 매번 전체 탐색 - 주석 처리
    //private void DialougeDataLoad(DailyRoutine routineState)
    //{
    //    List<DialogData> dialogDatas = new List<DialogData>();

    //    for (int i = 0; i < DailyDialougeDB.Transition.Count; i++)
    //    {
    //        var row = DailyDialougeDB.Transition[i];

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

    //    dialougeSystem.InitSelectDialouge(dialogDatas.ToArray(), true );
    //}
    #endregion

}
