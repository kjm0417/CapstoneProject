using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class RestTimeQuestionItem : MonoBehaviour
{
    public Button GetHundredsUp() => hundredsUp;
    public Button GetTensUp() => tensUp;
    public Button GetOnesUp() => onesUp;
    public Button GetHundredsDown() => hundredsDown;
    public Button GetTensDown() => tensDown;
    public Button GetOnesDown() => onesDown;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI hundredsText;
    [SerializeField] private TextMeshProUGUI tensText;
    [SerializeField] private TextMeshProUGUI onesText;
    [SerializeField] private TextMeshProUGUI maxTimeText;

    [Header("버튼")]
    [SerializeField] private Button hundredsUp;
    [SerializeField] private Button hundredsDown;
    [SerializeField] private Button tensUp;
    [SerializeField] private Button tensDown;
    [SerializeField] private Button onesUp;
    [SerializeField] private Button onesDown;

    [SerializeField]
    DailyRoutineTime dailyRoutineTime;

    private int[] digits = new int[3]; // [0]=hundreds, [1]=tens, [2]=ones

    [SerializeField]
    public int currentTime;

    [SerializeField]
    public int prevlastTime = 0;

    [SerializeField]
    int currentMaxTime = 0;

    private bool isFirstEnable = true; // 최초 1회만 GetMaxMinutes() 호출

    [SerializeField]
    private bool isTimeSelectBack;

    private void OnBackActive(bool active) 
    {
        if (active)  //ESC 누를시에 시간을 모두 0으로 초기화
        {
            digits[0] = digits[1] = digits[2] = 0;
            currentTime = 0;
            UpdateTimeUI();
        }

        isTimeSelectBack = active; 
    }

    public static string OnRestUIBacked = "OnRestUIBackEvent"; //현재 UI 활성화 된 상태에서 ESC 키 누름에 대한 이벤트

    public static string OnRestTimeCalculated = "OnRestTimeCalculateEvent";  //다음 시간까지 "최대시간"분, 최대시간 계산하는 이벤트


    private void OnEnable()
    {
        EventBus.Subscribe<bool>(OnRestUIBacked, OnBackActive);
        EventBus.Subscribe(OnRestTimeCalculated, SetNotFirstMaxTime);

        hundredsUp.onClick.AddListener(() => ChangeDigit(0, +1));
        hundredsDown.onClick.AddListener(() => ChangeDigit(0, -1));

        tensUp.onClick.AddListener(() => ChangeDigit(1, +1));
        tensDown.onClick.AddListener(() => ChangeDigit(1, -1));

        onesUp.onClick.AddListener(() => ChangeDigit(2, +1));
        onesDown.onClick.AddListener(() => ChangeDigit(2, -1));

        

        if (isFirstEnable)
        {
            SetFirstMaxTime();
        }
        else
        {
            Debug.Log("1");
            maxTimeText.text = $"{prevlastTime} 분";
        }


        UpdateTimeUI();
    }
    /// <summary>
    /// RestUI 활성화 되었을 때 MaxTime 계산 로직이며, 초기 MaxTime 계산임
    /// </summary>
    void SetFirstMaxTime() //초기 MaxTime
    {
        currentMaxTime = GetMaxMinutes();
        prevlastTime = currentMaxTime;
        maxTimeText.text = $"{currentMaxTime} 분";
        isFirstEnable = false;
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<bool>(OnRestUIBacked, OnBackActive);
        EventBus.UnSubscribe(OnRestTimeCalculated, SetNotFirstMaxTime);

        hundredsUp.onClick.RemoveAllListeners();
        hundredsDown.onClick.RemoveAllListeners();
        tensUp.onClick.RemoveAllListeners();
        tensDown.onClick.RemoveAllListeners();
        onesUp.onClick.RemoveAllListeners();
        onesDown.onClick.RemoveAllListeners();


        //if (!isTimeSelectBack) //back 눌렀을때 MaxTime 계산안되게. esc키 안 눌렀을때만 Set()실행
        //{
        //    currentTime = 0;
        //    digits[0] = digits[1] = digits[2] = 0;
        //}

        //SetNotFirstMaxTime();

    }
    /// <summary>
    /// RestUI의  MaxTime 계산 로직이며, 초기 MaxTime 계산 아님.
    /// </summary>
    void SetNotFirstMaxTime() 
    {
        prevlastTime -= currentTime; // 10 - 5 = 5

        if (prevlastTime <= 0)
        {
            EventBus.Publish(EffectManager.OnRestTimeEnded, true, currentTime);
            isFirstEnable = true;
        }

        currentTime = 0;
        digits[0] = digits[1] = digits[2] = 0;
    }

    private void ChangeDigit(int index, int delta)
    {
        int before = GetTotalMinutes();

        digits[index] += delta; //digits 배열의 인덱스에 맞게 delta값 증가

        if (delta > 0) // 자리 올림
        {
            for (int i = index; i >= 0; i--)
            {
                if (digits[i] > 9)
                {
                    digits[i] = 0;
                    if (i - 1 >= 0) digits[i - 1]++;
                }
            }
        }
        else if (delta < 0) // 자리 내림
        {
            for (int i = index; i >= 0; i--)
            {
                if (digits[i] < 0)
                {
                    digits[i] = 9;
                    if (i - 1 >= 0) digits[i - 1]--;
                }
            }
        }

        // 최소 보정
        if (GetTotalMinutes() < 0)
        {
            digits[0] = digits[1] = digits[2] = 0;
        }

        // 최대 보정 (넘으면 롤백)
        if (GetTotalMinutes() > prevlastTime) //최대 시간 초과 하면
        {
            digits[0] = before / 100;
            digits[1] = (before % 100) / 10;
            digits[2] = before % 10;
            Debug.Log("최대 시간 초과 -> 이전 값으로 세팅");
        }

        UpdateTimeUI();
    }

    private void UpdateTimeUI()
    {
        hundredsText.text = digits[0].ToString();
        tensText.text = digits[1].ToString();
        onesText.text = digits[2].ToString();   
    }

    public int GetTotalMinutes()
    {
        currentTime = digits[0] * 100 + digits[1] * 10 + digits[2];

        return currentTime;
    }

    private int GetMaxMinutes()
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
