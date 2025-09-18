using TMPro;
using UnityEngine;

public class InGameCalendar : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI CurrentDate;

    [SerializeField]
    TextMeshProUGUI CurrentDay;

    private void OnEnable()
    {
        EventBus.Subscribe(TimeManager.OnDayStarted, OnDayStartDateUpdate); //하루 시작 이벤트 구독
    }
    private void OnDisable()
    {
        EventBus.UnSubscribe(TimeManager.OnDayStarted, OnDayStartDateUpdate); //하루 시작 이벤트 구독해제
    }
    private void OnDayStartDateUpdate()
    {
        Debug.Log("날짜 변경");
    }
}
