using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyManager : SingleTonGeneric<DailyManager> //일상 변경 및 선택 대사를 위한 매니저
{
    public static string OnDialougeSelectUIStart = "OnDialougeStartEvent";

    public static string OnDailySelecteEnded = "OnDailySelecteEndedEvent";

    public static string OnDailyWakeUp = "OnDailyWakeUpEvent";
    public static string OnDailyGotoSchool = "OnDailyGotoSchoolEvent";
    public static string OnDailyArriveSchool = "OnDailyArriveSchoolEvent";
    public static string OnDailyParm = "OnDailyParmEvent";



    [SerializeField]
    DialougeSystem system;

    private IDailyRoutine routineState;

    protected override void Awake()
    {
        base.Awake();
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
        EventBus.Subscribe<string>(OnDailySelecteEnded, CreateChoiceButtons);


        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //하루 시작 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyWakeUp, OnWakeUpSelectUI);   //침대에서 일어난 상태 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyGotoSchool, OnGotoSchoolSelectUI);   //등교 상태 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyArriveSchool, OnArriveSchoolSelectUI);  //학교 이벤트 구독

        EventBus.Subscribe<IDailyRoutine>(OnDailyParm, OnParmSelectUI);   //야자 이벤트 구독
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<string>(OnDailySelecteEnded, CreateChoiceButtons);

        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDailyStart);   //하루 시작 이벤트 구독

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyWakeUp, OnWakeUpSelectUI);   //침대에서 일어난 상태 이벤트 구독

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyGotoSchool, OnGotoSchoolSelectUI);   //등교 상태 이벤트 구독

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyArriveSchool, OnArriveSchoolSelectUI);  //학교 이벤트 구독

        EventBus.UnSubscribe<IDailyRoutine>(OnDailyParm, OnParmSelectUI);   //야자 이벤트 구독
    }

    private int currentDay;
    private void OnDailyStart(GameDate date)
    {
        currentDay = date.Day;

        Debug.Log("OnDailyStart");

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

    [SerializeField]
    DailyDialougeExcel DailyDialougeDB;

    [SerializeField] private DailyChangeSelect choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonParent; // 버튼들을 담을 부모

    private ObjectPool<DailyChangeSelect> choiceBtnItemPool;

    private List<DailyChangeSelect> choiceBtnItemList = new List<DailyChangeSelect>();

    public GameObject choiceParent; //선택 버튼의 최상위 부모. Scroll View

    private string currentPlayerId;

    private void OnWakeUpSelectUI(IDailyRoutine routineState)
    {
        choiceBtnItemPool = new ObjectPool<DailyChangeSelect>(choiceButtonPrefab, 3, choiceButtonParent);

        foreach(var dialogue in DailyDialougeDB.DailySelect)
        {
            if(dialogue.Pattern.Equals(routineState.DailyName))
            {
                Debug.Log("패턴 같음");
            }
        }
        Debug.Log("방 - 다음 상태로 가기 위한 대사");
        
        StartCoroutine(DialogueFlow()); 
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

    IEnumerator DialogueFlow()
    {
        // UpdateDialog가 true 반환될 때까지 반복
        while (!system.UpdateDialog())
        {
            yield return null;
        }
        Debug.Log("대화 종료됨");
    }
    //일상 - 방
    //침대에서 일어나는 모션 / 방 밖으로 돌아다니는 모션 -> 스토리 진행 -> 선택 대사 발동


    ///// <summary>
    ///// 선택 버튼 풀 반환 및 생성하는 메서드
    ///// <summary>
    private void CreateChoiceButtons(string choiceString)
    {
        // 기존 버튼 제거 (중복 방지)
        foreach (DailyChangeSelect choiceBtnItem in choiceBtnItemList)
        {
            choiceBtnItemPool.Return(choiceBtnItem);
        }

        choiceBtnItemList.Clear();

        // 쉼표 기준으로 나누기
        string[] choices = choiceString.Split(',');

        foreach (string choice in choices)
        {
            DailyChangeSelect choiceBtn = choiceBtnItemPool.Get();

            choiceBtn.InitButtonText(choice);

            choiceBtnItemList.Add(choiceBtn);

        }

    }
}
