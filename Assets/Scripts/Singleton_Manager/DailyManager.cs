using UnityEngine;

public class DailyManager : SingleTonGeneric<DailyManager>
{
    public static string OnDailyStarted = "OnDailyStartEvent";

    private IDailyRoutine routineState;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        EventBus.Subscribe(TimeManager.OnDayStarted, OnDailyStart);
    }

    private void OnDisable()
    {
        EventBus.Subscribe(TimeManager.OnDayStarted, OnDailyStart);
    }

    private void OnDailyStart()
    {
        routineState = new WakeUp();

        ChangeState(routineState);

        EventBus.Publish(OnDailyStarted, routineState);

    }

    private void ChangeState(IDailyRoutine routineState)
    {
        routineState?.Exit();
        this.routineState = routineState;
        routineState.Enter();
    }
}
