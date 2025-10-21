using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class RestTimeQuestionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI maxTimeText;

    IRestTimeProvider restTimeProvider;

    private int[] digits = new int[3]; // [0]=hundreds, [1]=tens, [2]=ones

    [Header("텍스트")]
    [SerializeField] public TextMeshProUGUI[] TimeTexts;

    [SerializeField]
    public int prevlastTime = 0;

    [SerializeField]
    int currentMaxTime = 0;

    [SerializeField]
    private bool isTimeSelectBack;



    private void OnEnable()
    {
        EventBus.Subscribe<bool>(RestUIManager.OnRestUIBacked, OnBackActive);
        EventBus.Subscribe(RestUIManager.OnRestTimeCalculated, SetNotFirstMaxTime);
        // EventBus.Subscribe(RestUIManager.OnRestTimeInitSetting, OnRestTimeInitSetting);

        OnRestTimeInitCalcutate();
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<bool>(RestUIManager.OnRestUIBacked, OnBackActive);
        EventBus.UnSubscribe(RestUIManager.OnRestTimeCalculated, SetNotFirstMaxTime);     
    }

    [SerializeField]
    private bool isFirstEnable = true; // 최초 1회만 GetMaxMinutes() 호출

    public void InitRestUI(IRestTimeProvider provider)
    {
        restTimeProvider = provider;
    }
    public void OnRestTimeInitCalcutate()
    {
        if (restTimeProvider.CurrentDecreaseRestTime > 0)
        {
            SetNotFirstMaxTime();
            maxTimeText.text = $"{prevlastTime} 분";

            isFirstEnable = false;
            restTimeProvider.CurrentDecreaseRestTime = 0;

            if(prevlastTime > 0 )
            {
                EventBus.Publish(DailyRoutineManager.OnCurrentDailyDialouge);
            }

            return;
        }

        if (isFirstEnable == false)
        {
            maxTimeText.text = $"{prevlastTime} 분";
        }
        else
        {
            SetFirstMaxTime();

        }

        UpdateTimeUI();
    }

    public void OnBackActive(bool active)
    {
        if (active)  //ESC 누를시에 시간을 모두 0으로 초기화
        {
            digits[0] = digits[1] = digits[2] = 0;

            restTimeProvider.CurrentRestTime = 0;
            UpdateTimeUI();
        }

        isTimeSelectBack = active;
    }

    /// <summary>
    /// RestUI 활성화 되었을 때 MaxTime 계산 로직이며, 초기 MaxTime 계산임
    /// </summary>
    public void SetFirstMaxTime() //초기 MaxTime
    {
        currentMaxTime = restTimeProvider.GetMaxRestMinutes();
        prevlastTime = currentMaxTime;
        maxTimeText.text = $"{currentMaxTime} 분";
        isFirstEnable = false;
    }


    /// <summary>
    /// RestUI의  MaxTime 계산 로직이며, 초기 MaxTime 계산 아님.
    /// </summary>
    public void SetNotFirstMaxTime()
    {
        prevlastTime -= restTimeProvider.CurrentRestTime > 0 ? restTimeProvider.CurrentRestTime : restTimeProvider.CurrentDecreaseRestTime; // 10 - 5 = 5

        if (prevlastTime <= 0)
        {
            EventBus.Publish(RestUIManager.OnRestTimeEnded);

            isFirstEnable = true;
        }

        restTimeProvider.CurrentRestTime = 0;

        digits[0] = digits[1] = digits[2] = 0;
    }

    public void ChangeDigit(int index, int delta)
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
        TimeTexts[2].text = digits[0].ToString();
        TimeTexts[1].text = digits[1].ToString();
        TimeTexts[0].text = digits[2].ToString();
    }

    public int GetTotalMinutes()
    {
        restTimeProvider.CurrentRestTime = digits[0] * 100 + digits[1] * 10 + digits[2];

        return restTimeProvider.CurrentRestTime;
    }


}
