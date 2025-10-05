using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum RestPeriod
{
    Morning,       // 오전
    Afternoon,     // 오후
    Evening,       // 야자
    EveningExtend  // 야자 연장
}

public class RestTimeQuestionItem : MonoBehaviour
{
    [Header("현재 상태")]
    [SerializeField] private RestPeriod currentPeriod = RestPeriod.Morning;

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

    [SerializeField] private Button timeCheckBtn;

    [Header("최대 시간 설정")]
    [SerializeField] private int maxMorning = 250;
    [SerializeField] private int maxAfternoon = 190;
    [SerializeField] private int maxEvening = 180;
    [SerializeField] private int maxEveningExtend = 120;

    private int[] digits = new int[3]; // [0]=hundreds, [1]=tens, [2]=ones

    [SerializeField]
    int currentTime;
    private void Start()
    {
        hundredsUp.onClick.AddListener(() => ChangeDigit(0, +1));
        hundredsDown.onClick.AddListener(() => ChangeDigit(0, -1));

        tensUp.onClick.AddListener(() => ChangeDigit(1, +1));
        tensDown.onClick.AddListener(() => ChangeDigit(1, -1));

        onesUp.onClick.AddListener(() => ChangeDigit(2, +1));
        onesDown.onClick.AddListener(() => ChangeDigit(2, -1));

        timeCheckBtn.onClick.AddListener(() => OnCheckTime());
        maxTimeText.text = $"{GetMaxMinutes()} 분";
        UpdateTimeUI();
    }

    private void OnCheckTime()
    {
        EventBus.Publish(UIManager.OnRestTimeSelected, currentTime);
        RestPeriod next = GetNextPeriod(currentPeriod);
        SetPeriod(next);
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
        if (GetTotalMinutes() > GetMaxMinutes()) //최대 시간 초과 하면
        {
            digits[0] = before / 100;
            digits[1] = (before % 100) / 10;
            digits[2] = before % 10;
            Debug.Log("최대 시간 초과 → 변경 취소");
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
        switch (currentPeriod)
        {
            case RestPeriod.Morning: return maxMorning;
            case RestPeriod.Afternoon: return maxAfternoon;
            case RestPeriod.Evening: return maxEvening;
            case RestPeriod.EveningExtend: return maxEveningExtend;
            default: return 0;
        }
    }
    private RestPeriod GetNextPeriod(RestPeriod current)
    {
        switch (current)
        {
            case RestPeriod.Morning: return RestPeriod.Afternoon;
            case RestPeriod.Afternoon: return RestPeriod.Evening;
            case RestPeriod.Evening: return RestPeriod.EveningExtend;
            case RestPeriod.EveningExtend:
            default:
                return RestPeriod.Morning; // 마지막이면 다시 Morning으로 순환
        }
    }
    public void SetPeriod(RestPeriod period)
    {
        currentPeriod = period;
        digits[0] = digits[1] = digits[2] = 0;

        int max = GetMaxMinutes();
        maxTimeText.text = $"{max}";
        UpdateTimeUI();
    }
}
