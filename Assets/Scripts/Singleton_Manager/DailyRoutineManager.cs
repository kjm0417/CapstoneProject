using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyRoutineManager : SingleTonGeneric<DailyRoutineManager> //일상 변경 및 선택 대사를 위한 매니저
{
    public static string OnReturnButtonSelected = "OnReturnButtonSelectEvent";
    public static string OnCreateButtonSelected = "OnCreateButtonSelectEvent";

    public static string OnDailyWakeUp = "OnDailyWakeUpEvent";
    public static string OnDailyGotoSchool = "OnDailyGotoSchoolEvent";
    public static string OnDailyArriveSchool = "OnDailyArriveSchoolEvent";
    public static string OnDailyParm = "OnDailyParmEvent";

    [SerializeField]
    DialougeSystem system;

    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    [SerializeField] private DailyChangeSelect choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonParent; // 버튼들을 담을 부모

    private ObjectPool<DailyChangeSelect> choiceBtnItemPool;

    private List<DailyChangeSelect> choiceBtnItemList = new List<DailyChangeSelect>();

    private IDailyRoutine routineState;

    protected override void Awake()
    {
        base.Awake();

        choiceBtnItemPool = new ObjectPool<DailyChangeSelect>(choiceButtonPrefab, 3, choiceButtonParent);

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            isChangeState = true;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<string>(OnCreateButtonSelected, CreateChoiceButtons); //선택 버튼 생성 이벤트 구독

        EventBus.Subscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독

        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //하루 시작 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyWakeUp, OnWakeUpSelectUI);   //침대에서 일어난 상태 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyGotoSchool, OnGotoSchoolSelectUI);   //등교 상태 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyArriveSchool, OnArriveSchoolSelectUI);  //학교 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyParm, OnParmSelectUI);   //야자 이벤트 구독
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<string>(OnCreateButtonSelected, CreateChoiceButtons);  //선택 버튼 생성 이벤트 구독 해제

        EventBus.UnSubscribe(OnReturnButtonSelected, ReturnChoiceButtons); //선택 버튼 클릭 이벤트 구독 해제

        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //하루 시작 이벤트 구독 해제

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyWakeUp, OnWakeUpSelectUI);   //침대에서 일어난 상태 이벤트 구독 해제

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyGotoSchool, OnGotoSchoolSelectUI);   //등교 상태 이벤트 구독 해제

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyArriveSchool, OnArriveSchoolSelectUI);  //학교 이벤트 구독 해제

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyParm, OnParmSelectUI);   //야자 이벤트 구독 해제
    }

    private int currentDay;
    private void OnDailyStart(GameDate date)
    {
        currentDay = date.Day;

        StartCoroutine(PlayerWakeUp());
    }

    bool isChangeState; //다음 상태로 갈지 말지에 대한 여부

    IEnumerator PlayerWakeUp() //방에서 일어나는 연출 및 플레이어 이동 가능 코루틴
    {
        Debug.Log("방에서 일어나는 연출");
        yield return new WaitForSeconds(2.0f);
        Debug.Log("플레이어 이동 가능");

        yield return new WaitUntil(() => isChangeState == true);

        routineState = new WakeUp("방");
        ChangeState(routineState);

        
    }
    private void ChangeState(IDailyRoutine routineState)
    {
        routineState?.Exit();
        this.routineState = routineState;
        Debug.Log($"ChangeDailyState : /{routineState.DailyName} ");
        routineState.Enter(routineState);

        isChangeState = false;
    }


    private void OnWakeUpSelectUI(IDailyRoutine routineState)
    {
        DialougeDataLoad(routineState);
        Debug.Log("방 - 다음 상태로 가기 위한 대사");       
    }

    private void OnGotoSchoolSelectUI(IDailyRoutine routineState)
    {
        Debug.Log("등교 -  - 다음 상태로 가기 위한 대사");
    }

    private void OnArriveSchoolSelectUI(IDailyRoutine routineState)
    {
        Debug.Log("학교 - 다음 상태로 가기 위한 대사");
    }

    private void OnParmSelectUI(IDailyRoutine routineState)
    {
        Debug.Log("야자 -  - 다음 상태로 가기 위한 대사");
    }

    ///// <summary>
    ///// 선택 대사의 데이터들을 DialougeSystem에 전달하는 메서드. 실질적인 대화 진행은 DialougeSystem 처리
    ///// <summary>
    private void DialougeDataLoad(IDailyRoutine routineState)
    {
        List<DialogData> dialogDatas = new List<DialogData>();

        for (int i = 0; i < DailyDialougeDB.DailySelect.Count; i++)
        {
            var row = DailyDialougeDB.DailySelect[i];

            if (row.Day == currentDay && row.Pattern == routineState.DailyName && row.DialougeType == "Transition")
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
            Debug.LogWarning($"{currentDay}의 패턴({routineState.DailyName}) 대사가 없음");
        }

        system.InitSelectDialouge(dialogDatas.ToArray(), true);
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
