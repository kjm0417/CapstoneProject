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
    AfterSchool, //학원
}
public class DailyRoutineManager : SingleTonGeneric<DailyRoutineManager> //일상 변경 및 선택 대사를 위한 매니저
{
    public static string OnReturnButtonSelected = "OnReturnButtonSelectEvent";
    public static string OnCreateButtonSelected = "OnCreateButtonSelectEvent";

    public static string OnDailyWakeUp = "OnDailyWakeUpEvent";
    public static string OnDailyGotoSchool = "OnDailyGotoSchoolEvent";

    public static string OnDailySchool_BreakFast = "OnDailySchool_BreakFastEvent";
    public static string OnDailySchool_Morning = "OnDailySchoolMorningEvent";
    public static string OnDailySchool_Lanch = "OnDailySchoolLanchEvent";
    public static string OnDailySchool_Afternoon = "OnDailySchoolnoonEvent";




    public static string OnDailyEndedParm = "OnDailyParmEvent";
    public static string OnDailyEndedAfterSchool = "OnDailyAfterSchool";

    [SerializeField]
    DialougeSystem dialougeSystem;

    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    [SerializeField] private DailyChangeSelect choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonParent; // 버튼들을 담을 부모

    private ObjectPool<DailyChangeSelect> choiceBtnItemPool;

    private List<DailyChangeSelect> choiceBtnItemList = new List<DailyChangeSelect>();

    private readonly Dictionary<int, Dictionary<string, List<DialogData>>> DailyCache = new Dictionary<int, Dictionary<string, List<DialogData>>>();
    private bool isStoryCacheBuilt;


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

        choiceBtnItemPool = new ObjectPool<DailyChangeSelect>(choiceButtonPrefab, 3, choiceButtonParent);

    }
    private void OnEnable()
    {
        BuildDailyPattenrCache();

        EventBus.Subscribe<List<ChoiceData>>(OnCreateButtonSelected, CreateChoiceButtons); //선택 버튼 생성 이벤트 구독

        EventBus.Subscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독

        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //1. 하루 시작 이벤트 구독

        EventBus.Subscribe(OnDailyWakeUp, OnChangeGotoSchoolUI);   //침대에서 일어난 상태 이벤트 구독 해제

        EventBus.Subscribe(OnDailyGotoSchool, OnChangeSchoolBreakFastUI);   //2. 등교 상태

        EventBus.Subscribe(OnDailySchool_BreakFast, OnChangeSchoolMorningUI);   //3. 학교 - 아침 시간


        EventBus.Subscribe(OnDailySchool_Morning, OnSchoolLanchUI);  //4. 학교 - 오전 시간

        EventBus.Subscribe(OnDailySchool_Lanch, OnSchoolAfternoonUI);  //4. 학교 - 점심 시간

        EventBus.Subscribe(OnDailySchool_Afternoon, OnAfterSchoolSelectUI);  //4. 학교 - 오후 시간ㅇ




        //하루 끝
        EventBus.Subscribe(OnDailyEndedParm, OnParmSelectUI);   //5. 야자 이벤트 구독

        EventBus.Subscribe(OnDailyEndedAfterSchool, OnAfterSchoolSelectUI);   //5. 야자 이벤트 구독
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<List<ChoiceData>>(OnCreateButtonSelected, CreateChoiceButtons);  //선택 버튼 생성 이벤트 구독 해제

        EventBus.UnSubscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독 해제

        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //하루 시작 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyWakeUp, OnChangeGotoSchoolUI);   //침대에서 일어난 상태 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyGotoSchool, OnChangeSchoolBreakFastUI);   //등교 상태 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailySchool_BreakFast, OnChangeSchoolMorningUI);   //등교 상태 이벤트 구독 해제


        EventBus.UnSubscribe(OnDailySchool_Morning, OnSchoolLanchUI);  //4. 학교 이벤트 구독

        EventBus.UnSubscribe(OnDailySchool_Lanch, OnSchoolAfternoonUI);  //4. 학교 이벤트 구독

        EventBus.UnSubscribe(OnDailySchool_Afternoon, OnAfterSchoolSelectUI);  //4. 학교 이벤트 구독


        //하루 끝
        EventBus.UnSubscribe(OnDailyEndedParm, OnParmSelectUI);   //야자 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyEndedAfterSchool, OnAfterSchoolSelectUI);   //학원 이벤트 구독 해제
    }

    private int currentDay;
    private void OnDailyStart(GameDate date)
    {
        currentDay = date.Day;
        DailyRoutine = DailyRoutine.Room;
    }
 
    private void OnChangeGotoSchoolUI() //등교 변경
    {
        DailyPatternDialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.GotoSchool; //등교 패턴으로 변경  
    }

    private void OnChangeSchoolBreakFastUI()
    {
        DailyPatternDialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.ArriveSchool_Breakfast; //학교 아침 시간
    }
    private void OnChangeSchoolMorningUI() //학교 오전 시간 변경
    {
        DailyPatternDialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.ArriveSchool_Morning; 
    }

    private void OnSchoolLanchUI() //학교 점심 시간 변경
    {
        DailyPatternDialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.ArriveSchool_Lanch; 
    }

    private void OnSchoolAfternoonUI() //학교 오후 시간 변경
    {
        DailyPatternDialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.ArriveSchool_Afternoon; 
    }


    private void OnParmSelectUI() //학교 야자시간 변경
    {
        DailyPatternDialougeDataLoad(dailyRoutine);     
        DailyRoutine = DailyRoutine.Parm; //야자 패턴으로 변경   
    }
    private void OnAfterSchoolSelectUI() //학원 변경
    {
        DailyPatternDialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.AfterSchool; 
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
    ///// <summary>
    ///// 선택 버튼 풀 생성하는 메서드
    ///// <summary>
    private void CreateChoiceButtons(List<ChoiceData> choices)
    {
        ReturnChoiceButtons();

        foreach (var choice in choices)
        {
            DailyChangeSelect choiceBtn = choiceBtnItemPool.Get();

            choiceBtn.InitButtonText(choice);

            choiceBtnItemList.Add(choiceBtn);

        }

    }

    ///// <summary>
    ///// 선택 버튼 풀 반환하는 메서드
    ///// <summary>
    private void ReturnChoiceButtons()
    {
        foreach (DailyChangeSelect choiceBtnItem in choiceBtnItemList)
        {
            choiceBtnItemPool.Return(choiceBtnItem);
        }

        choiceBtnItemList.Clear();

    }

}
