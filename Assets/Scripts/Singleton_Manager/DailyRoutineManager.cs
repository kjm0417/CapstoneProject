using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DailyRoutine
{
    Room, //방
    GotoSchool, //등교
    ArriveSchool, //학교
    Parm, //야자
    AfterSchool, //학원
}
public class DailyRoutineManager : SingleTonGeneric<DailyRoutineManager> //일상 변경 및 선택 대사를 위한 매니저
{
    public static string OnReturnButtonSelected = "OnReturnButtonSelectEvent";
    public static string OnCreateButtonSelected = "OnCreateButtonSelectEvent";

    public static string OnDailyWakeUp = "OnDailyWakeUpEvent";
    public static string OnDailyGotoSchool = "OnDailyGotoSchoolEvent";
    public static string OnDailyArriveSchool = "OnDailyArriveSchoolEvent";
    public static string OnDailyParm = "OnDailyParmEvent";
    public static string OnDailyAfterSchool = "OnDailyAfterSchool";

    [SerializeField]
    DialougeSystem dialougeSystem;

    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    [SerializeField] private DailyChangeSelect choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonParent; // 버튼들을 담을 부모

    private ObjectPool<DailyChangeSelect> choiceBtnItemPool;

    private List<DailyChangeSelect> choiceBtnItemList = new List<DailyChangeSelect>();

    private DailyRoutine dailyRoutine;

    private DailyRoutine DailyRoutine
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
        EventBus.Subscribe<string>(OnCreateButtonSelected, CreateChoiceButtons); //선택 버튼 생성 이벤트 구독

        EventBus.Subscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독

        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //1. 하루 시작 이벤트 구독

        EventBus.Subscribe(OnDailyWakeUp, OnRoomSelectUI);   //2. 침대에서 일어난 상태 이벤트 구독

        EventBus.Subscribe(OnDailyGotoSchool, OnGotoSchoolSelectUI);   //3. 등교 상태 이벤트 구독

        EventBus.Subscribe(OnDailyArriveSchool, OnArriveSchoolSelectUI);  //4. 학교 이벤트 구독

        EventBus.Subscribe(OnDailyParm, OnParmSelectUI);   //5. 야자 이벤트 구독

        EventBus.Subscribe(OnDailyAfterSchool, OnAfterSchoolUI);   //5. 야자 이벤트 구독
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<string>(OnCreateButtonSelected, CreateChoiceButtons);  //선택 버튼 생성 이벤트 구독 해제

        EventBus.UnSubscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독 해제

        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //하루 시작 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyWakeUp, OnRoomSelectUI);   //침대에서 일어난 상태 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyGotoSchool, OnGotoSchoolSelectUI);   //등교 상태 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyArriveSchool, OnArriveSchoolSelectUI);  //학교 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyParm, OnParmSelectUI);   //야자 이벤트 구독 해제

        EventBus.UnSubscribe(OnDailyAfterSchool, OnAfterSchoolUI);   //학원 이벤트 구독 해제
    }

    private int currentDay;
    private void OnDailyStart(GameDate date)
    {
        currentDay = date.Day;

        StartCoroutine(PlayerWakeUp());
    }

    IEnumerator PlayerWakeUp() //방에서 일어나는 연출 및 플레이어 이동 가능 코루틴
    {
        DailyRoutine = DailyRoutine.Room;

        Debug.Log("방에서 일어나는 연출" + DailyRoutine);
        yield return new WaitForSeconds(2.0f);
        Debug.Log("플레이어 이동 가능");  
    }

    private void OnRoomSelectUI()
    {
        Debug.Log("방 패턴 대사 출력 후 등교 패턴으로 변경");
        DialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.GotoSchool; //등교 패턴으로 변경  
    }

    private void OnGotoSchoolSelectUI()
    {
        Debug.Log("등교 패턴 대사 출력 후 학교 패턴으로 변경");
        DialougeDataLoad(dailyRoutine);
        DailyRoutine = DailyRoutine.ArriveSchool; //학교 패턴으로 변경  
    }

    private void OnArriveSchoolSelectUI()
    {
        Debug.Log("학교 패턴 대사 출력 후 야자 또는 학원 패턴으로 변경");
        DialougeDataLoad(dailyRoutine);
       
        //여기서 캐릭터가 누구냐에 따라 야자로 할지, 학원으로 할지 결정
        DailyRoutine = DailyRoutine.Parm; //야자 패턴으로 변경  
        //DailyRoutine = DailyRoutine.AfterSchool; //학원 패턴으로 변경  
    }

    private void OnParmSelectUI()
    {
        Debug.Log("야자 - 방 패턴으로 가기 위한 대사");
        DialougeDataLoad(dailyRoutine);
    }
    private void OnAfterSchoolUI()
    {
        Debug.Log("학원 - 방 패턴으로 가기 위한 대사");
        DialougeDataLoad(dailyRoutine);
    }

    ///// <summary>
    ///// 선택 대사의 데이터들을 DialougeSystem에 전달하는 메서드. 실질적인 대화 진행은 DialougeSystem 처리
    ///// <summary>
    private void DialougeDataLoad(DailyRoutine routineState)
    {
        List<DialogData> dialogDatas = new List<DialogData>();

        for (int i = 0; i < DailyDialougeDB.Transition.Count; i++)
        {
            var row = DailyDialougeDB.Transition[i];

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

        dialougeSystem.InitSelectDialouge(dialogDatas.ToArray(), true );
    }

    ///// <summary>
    ///// 선택 버튼 풀 생성하는 메서드
    ///// <summary>
    private void CreateChoiceButtons(string choiceString)
    {
        ReturnChoiceButtons();

        string[] choices = choiceString.Split(',');

        foreach (string choice in choices)
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
