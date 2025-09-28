using UnityEngine;

public interface IDailyRoutine 
{
    public void Enter(IDailyRoutine dailyRoutine);

    public void Exit();

    public string DailyName { get; set; }
}
