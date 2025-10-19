using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestUIManager : MonoBehaviour, IRestTimeProvider
{
    public static string OnRestUIBacked = "OnRestUIBackEvent"; //현재 UI 활성화 된 상태에서 ESC 키 누름에 대한 이벤트

    public static string OnRestTimeCalculated = "OnRestTimeCalculateEvent";  //다음 시간까지 "최대시간"분, 최대시간 계산하는 이벤트

    public static string OnStudyRestTimeculated = "OnStudyRestTimeculateEvent";
    public static string OnStudyRestTimeSaved = "OnStudyRestTimeSaveEvent";

    public static string OnRestTimeEnded = "OnRestTimeEndEvent"; //RestTime = 0일때 발동 이벤트

    //스터디 플래너를 선택했을 때 공부시간 기준으로 RestTime 감소

    [SerializeField]
    RestTimeQuestionUI restTimeUI;

    [SerializeField]
    DailyRoutineTime dailyRoutineTime;

    [Header("버튼")]
    [SerializeField] public Button[] TimeSelectUpButtons;
    [SerializeField] public Button[] TimeSelectDownButtons;

    [SerializeField]
    private int currentRestTime;

    [SerializeField]
    private int currentDecreaseRestTime;


    public Button GetHundredsUp() => TimeSelectUpButtons[2];
    public Button GetTensUp() => TimeSelectUpButtons[1];
    public Button GetOnesUp() => TimeSelectUpButtons[0];

    public Button GetHundredsDown() => TimeSelectDownButtons[2];
    public Button GetTensDown() => TimeSelectDownButtons[1];
    public Button GetOnesDown() => TimeSelectDownButtons[0];

    public int CurrentRestTime
    {
        get
        {
            return currentRestTime;
        }
        set
        {
            currentRestTime = value;
        }
    }
    public int CurrentDecreaseRestTime
    {
        get
        {
            return currentDecreaseRestTime;
        }
        set
        {
            currentDecreaseRestTime = value;
        }
    }

    DailySelectButtonManager dailyButtonManager;

    private void Start()
    {
        dailyButtonManager = FindObjectOfType<DailySelectButtonManager>();
    }
    private void OnEnable()
    {
        EventBus.Subscribe<int>(OnStudyRestTimeSaved, OnStudyTimeSave);
        EventBus.Subscribe(OnStudyRestTimeculated, OnRestUIInitSetting);
        EventBus.Subscribe(OnRestTimeEnded, OnDailyRestTimeEnd);


        EventBus.Subscribe(EventNames.MiniGameSuccesed, Test);
        EventBus.Subscribe(EventNames.MiniGameFailed, Test1);

        for (int i = 0; i < 3; i++)
        {
            int index = i; //캡쳐를 i가 아닌 index로 수행.

            TimeSelectUpButtons[2 - index].onClick.AddListener(() => restTimeUI.ChangeDigit(index, 1));
            TimeSelectDownButtons[2 - index].onClick.AddListener(() => restTimeUI.ChangeDigit(index, -1));
        }

        restTimeUI.InitRestUI(this); //인터페이스를 bottom에게 주입.
                                     //botto은 주입 받은 인터페이스 객체로 부모에 접근
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<int>(OnStudyRestTimeSaved, OnStudyTimeSave);
        EventBus.UnSubscribe(OnStudyRestTimeculated, OnRestUIInitSetting);
        EventBus.UnSubscribe(OnRestTimeEnded, OnDailyRestTimeEnd);

        EventBus.UnSubscribe(EventNames.MiniGameSuccesed, Test);
        EventBus.UnSubscribe(EventNames.MiniGameFailed, Test1);
        for (int i = 0; i < 3; i++)
        {
            TimeSelectUpButtons[i].onClick.RemoveAllListeners();
           TimeSelectDownButtons[i].onClick.RemoveAllListeners();
        }
    }
    private void Test() //미니게임 성공
    {
        OnRestUIInitSetting(); //공부시간 -> RestTime 계산
    }
    private void Test1() //미니게임 실패
    {
        OnRestUIInitSetting(); //공부시간 -> RestTime 계산

        EventBus.Publish(DailyRoutineManager.OnCurrentDailyDialouge);
    }
    private void OnDailyRestTimeEnd() //다음 패턴 이동 및 대사 로드
    {
       

        if (dailyButtonManager.Mode == DailySelectButtonManager.UIMode.Study )
        {
            EventBus.Publish(DailyRoutineManager.OnNextDailyDialouge);
            Debug.Log("Study 모드 restime end");
        }
        else if(dailyButtonManager.Mode == DailySelectButtonManager.UIMode.Rest)
        {
            EventBus.Publish(DailyRestSelectButton.OnRestTimeAfterEnded);
            Debug.Log("rest 모드 restime end");
        }
        else if (dailyButtonManager.Mode == DailySelectButtonManager.UIMode.FixedStudy)
        {
            EventBus.Publish(DailyRoutineManager.OnNextDailyDialouge);
            Debug.Log("FixedStudy 모드 restime end");
        }
    }

    private void OnStudyTimeSave(int value) //StudyTime 저장
    {
        CurrentDecreaseRestTime = value;
    }
    private void OnRestUIInitSetting() //쉰다 선택 -> RestTime 감소/ 자습한다 선택 -> StudyTime 존재 시, RestTime감소
    {
        restTimeUI.OnRestTimeInitCalcutate(); // <- 여기서 계산
    }
    public int GetMaxRestMinutes()
    {
        DailyRoutine dailyRoutine = DailyRoutineManager.Instance.DailyRoutine;

        Debug.Log(dailyRoutine);

        switch (dailyRoutine)
        {
            case DailyRoutine.GotoSchool: return dailyRoutineTime.maxGotoSchool;
            case DailyRoutine.ArriveSchool_Breakfast: return dailyRoutineTime.maxBreakfast;
            case DailyRoutine.ArriveSchool_Morning: return dailyRoutineTime.maxMorning;
            case DailyRoutine.ArriveSchool_Lanch: return dailyRoutineTime.maxLanch;
            case DailyRoutine.ArriveSchool_Afternoon: return dailyRoutineTime.maxAfternoon;
            case DailyRoutine.Parm: return dailyRoutineTime.maxParm;
            case DailyRoutine.AfterSchool: return dailyRoutineTime.maxAfterSchool;
            default: return 0;
        }
    }


}
