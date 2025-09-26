using UnityEngine;

public class WakeUp : IDailyRoutine
{
    public void Enter()
    {
        EventBus.Subscribe<IDailyRoutine>(DailyManager.OnDailyStarted, Excute);
    }

    public void Excute(IDailyRoutine routine)
    {
        EventBus.Publish(DialogueManager.OnDialougeSelected, routine);
    }

    public void MiniGame()
    {

    }
    public void Exit()
    {
        EventBus.UnSubscribe<IDailyRoutine>(DailyManager.OnDailyStarted, Excute);
    }
}
