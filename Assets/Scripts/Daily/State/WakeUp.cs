using UnityEngine;

public class WakeUp : IDailyRoutine
{
    public string DailyName { get; set; }
    public WakeUp() { }

    public WakeUp(string DailyName)
    {
        this.DailyName = DailyName;
    }
    public void Enter(IDailyRoutine dailyRoutine)
    {
        EventBus.Publish(DailyManager.OnDailyWakeUp, dailyRoutine);
    }

    public void Exit()
    {
       
    }
}
