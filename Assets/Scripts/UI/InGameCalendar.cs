using TMPro;
using UnityEngine;

public class InGameCalendar : MonoBehaviour //실제 UI 업데이트 및 출력
{
    [SerializeField]
    TextMeshProUGUI CurrentDate;

    [SerializeField]
    TextMeshProUGUI CurrentDay;

    private void OnEnable()
    {
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDateChanged); //하루 시작 이벤트 구독
       
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDateChanged); //하루 시작 이벤트 구독해제
    }
    private void OnDateChanged(GameDate date)
    {
        string currentWeekDay = UpdateWeekDay(date.WeekDay);
        Debug.Log(currentWeekDay.ToString());

        CurrentDate.text = $"{date.Month} / {date.Day}";
        CurrentDay.text = $"D - {date.RemaineDay}";
    }

    public string UpdateWeekDay(int weekDay) //요일 계산
    {
        string currentWeekDay = " ";

        switch (weekDay)
        {
            case 0:
                currentWeekDay = "월요일";
                break;
            case 1:
                currentWeekDay = "화요일";
                break;
            case 2:
                currentWeekDay = "수요일";
                break;
            case 3:
                currentWeekDay = "목요일";
                break;
            case 4:
                currentWeekDay = "금요일";
                break;
            case 5:
                currentWeekDay = "토요일";
                break;
            case 6:
                currentWeekDay = "일요일";
                break;
        }

        return currentWeekDay;
    }
}
