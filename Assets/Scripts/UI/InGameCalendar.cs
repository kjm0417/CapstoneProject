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
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDateChanged);  //OnDateChanged 이벤트 구독

        EventBus.Publish(TimeManager.OnDayCurrent); //OnDateChanged 발행
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDateChanged); //OnDateChanged 이벤트 해제
    }
    private void OnDateChanged(GameDate date)
    {
        CurrentDate.text = $"{date.Month} / {date.Day}";
        CurrentDay.text = $"D - {date.RemaineDay}";
    }

}
