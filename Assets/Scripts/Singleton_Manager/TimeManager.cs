using UnityEngine;

[System.Serializable]
public class GameDate
{
    //public int Year; //년도 , 잠시 보류
    public int Month; //몇 월
    public int Day; //몇 일
    public int RemaineDay; //남은 날짜, D - 31 
    public int WeekDay; //요일 
    public GameDate(int month ,int day,int remainDay, int dayOfWeek)
    {
        //this.Year = year; //-> 년도 표기는 잠시 보류
        this.Month = month;
        this.Day = day;
        this.RemaineDay = remainDay;
        this.WeekDay = dayOfWeek;
    }

}
public class TimeManager : SingleTonGeneric<TimeManager>
{
    public static string OnDayStarted = "OnDayStartEvent"; //하루 일상 시작 될 때
    public static string OnDayEnded = "OnDayEndedEvent"; //하루 일상 끝날 때

    public GameDate currentGameDate;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        InitDate(10, 1,31,0); //10월 1일부터 시작 , 0 ~ 6 = 월요일~일요일
    }

    private void OnEnable()
    {
        EventBus.Subscribe(OnDayEnded, UpdateDate);
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe(OnDayEnded, UpdateDate);
    }

    private void Update()
    {
        //임시 코드
        if (Input.GetKeyDown(KeyCode.T)) 
        {
            UpdateDate(); 
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EventBus.Publish(DailyRoutineManager.OnDailyWakeUp);
        }
    }

    public void InitDate(int month,int day, int remainDay, int dayOfWeek) //게임 시작 시, 날짜 로드 또는 초기화 하는 메서드
    {
        currentGameDate = new GameDate(month, day, remainDay, dayOfWeek);

        EventBus.Publish(OnDayStarted, currentGameDate); //GameDate 타입의 OnDayStarted 구독한 구독자에게 이벤트 뿌림 - 날짜 변경
    }

    public void UpdateDate() //날짜 변경 메서드. 하루 일상 끝나면 호출
    {
        currentGameDate.Day++;
        currentGameDate.WeekDay++;

        currentGameDate.RemaineDay--;

        if(currentGameDate.Day > 30) //30일 넘어가면
        {
            //11.1
            currentGameDate.Day = 1; //일 증가
            currentGameDate.Month++; //월 증가 
        }

        else if(currentGameDate.WeekDay > 6) //일요일 넘어가면
        {
            currentGameDate.WeekDay = 0; //월요일로 변경
        }


        EventBus.Publish(OnDayStarted,currentGameDate); //GameDate 타입의 OnDayStarted 구독한 구독자에게 이벤트 뿌림 - 날짜 변경
       
    }

}
