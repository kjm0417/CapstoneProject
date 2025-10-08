using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class RestTimeQuestionItem : MonoBehaviour
{
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

    [Header("최대 시간 설정")]
    [SerializeField] private int maxGotoSchool = 10; //등교시간
    [SerializeField] private int maxBreakfast = 10; //아침 시간(조례)
    [SerializeField] private int maxMorning = 240; //오전 시간
    [SerializeField] private int maxLanch = 20; //점심 시간
    [SerializeField] private int maxAfternoon = 170; //오후 시간
    [SerializeField] private int maxParm = 50; //야자 시간 
    [SerializeField] private int maxAfterSchool; //야자 연장 시간 or 학원 시간 


    private int[] digits = new int[3]; // [0]=hundreds, [1]=tens, [2]=ones

    [SerializeField]
    int currentTime;

    private bool isFirstEnable = true; // 최초 1회만 GetMaxMinutes() 호출

    private void OnEnable()
    {
        hundredsUp.onClick.AddListener(() => ChangeDigit(0, +1));
        hundredsDown.onClick.AddListener(() => ChangeDigit(0, -1));

        tensUp.onClick.AddListener(() => ChangeDigit(1, +1));
        tensDown.onClick.AddListener(() => ChangeDigit(1, -1));

        onesUp.onClick.AddListener(() => ChangeDigit(2, +1));
        onesDown.onClick.AddListener(() => ChangeDigit(2, -1));

        if (isFirstEnable)
        {
            currentMaxTime = GetMaxMinutes();
            maxTimeText.text = $"{currentMaxTime} 분";
            isFirstEnable = false;
        }
        else
        {
            maxTimeText.text = $"{prevlastTime} 분";
        }


        UpdateTimeUI();
    }
    [SerializeField]
    public int prevlastTime;

    [SerializeField]
    int currentMaxTime = 0;

    private void OnDisable()
    {
        hundredsUp.onClick.RemoveAllListeners();
        hundredsDown.onClick.RemoveAllListeners();
        tensUp.onClick.RemoveAllListeners();
        tensDown.onClick.RemoveAllListeners();
        onesUp.onClick.RemoveAllListeners();
        onesDown.onClick.RemoveAllListeners();

        Set();
    }
    void Set()
    {
        currentMaxTime -= currentTime; // 10 - 5 = 5

        if(currentMaxTime <= 0)
        {
            EventBus.Publish(EffectManager.OnRestTimeEnded, true);
            isFirstEnable = true;
        }
        prevlastTime = currentMaxTime;

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
        if (GetTotalMinutes() > currentMaxTime) //최대 시간 초과 하면
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

        switch (dailyRoutine)
        {
            case DailyRoutine.GotoSchool: return maxGotoSchool;
            case DailyRoutine.ArriveSchool_Breakfast: return maxBreakfast;
            case DailyRoutine.ArriveSchool_Morning: return maxMorning;
            case DailyRoutine.ArriveSchool_Lanch: return maxLanch;
            case DailyRoutine.ArriveSchool_Afternoon: return maxAfternoon;
            case DailyRoutine.Parm: return maxParm;
            case DailyRoutine.AfterSchool: return maxAfterSchool;
            default: return 0;
        }
    }
}
