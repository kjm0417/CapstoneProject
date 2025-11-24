using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameCalendar : UIAnimatedPanel //실제 UI 업데이트 및 출력
{
    public enum SlideDirection { Left, Right }

    [SerializeField] private SlideUIElement[] elements;

    [SerializeField]
    TextMeshProUGUI CurrentDate;

    [SerializeField]
    TextMeshProUGUI CurrentDay;

    [SerializeField]
    Button MoveBtn;

    [SerializeField]
    Button HideBtn;

    [SerializeField]
    float MoveUISpeed;

    protected override void Awake()
    {
        base.Awake();

        //MoveBtn.onClick.RemoveAllListeners();
        //MoveBtn.onClick.AddListener(() =>
        //{
        //    PlayShowAnimation();
        //});
        //HideBtn.onClick.RemoveAllListeners();
        //HideBtn.onClick.AddListener(() =>
        //{
        //    PlayHideAnimation();
        //});
    }

    [SerializeField]
    bool isSet = true;
    protected override void OnEnable()
    {
        EventBus.Subscribe<GameDate>(TimeManager.OnDayStarted, OnDateChanged);  //OnDateChanged 이벤트 구독

        //EventBus.Publish(TimeManager.OnDayCurrent); //OnDateChanged 발행

        if (isSet)
        {
            PlayInitAnimation();
            isSet = false;
            return;
        }

        PlayShowAnimation(); 

    }


    protected override void OnDisable()
    {
        base.OnDisable();

        PlayHideAnimation();

        EventBus.UnSubscribe<GameDate>(TimeManager.OnDayStarted, OnDateChanged); //OnDateChanged 이벤트 해제
    }
    private void OnDateChanged(GameDate date)
    {
        CurrentDate.text = $"{date.Month} / {date.Day}";
        CurrentDay.text = $"D - {date.RemaineDay}";
    }

    protected override void PlayInitAnimation()
    {
        foreach (var e in elements)
        {
            e.InitPostion = e.target.anchoredPosition; //초기값 설정

            Vector2 offset = (e.Direction == SlideDirection.Left) ?
                new Vector2(400, 0) : new Vector2(-260, 0);
            e.showPosition = e.InitPostion + offset;

            DotweenUIManager.Instance.SlideTo(e.target, e.showPosition, MoveUISpeed);
        }
    }
    protected override void PlayShowAnimation()
    {
        foreach (var e in elements)
        {
            DotweenUIManager.Instance.SlideTo(e.target, e.showPosition, MoveUISpeed);
        }
    }

    protected override void PlayHideAnimation()
    {
        foreach (var e in elements)
        {
            DotweenUIManager.Instance.SlideTo(e.target, e.InitPostion, MoveUISpeed);
        }
    }

}
