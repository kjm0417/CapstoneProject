using UnityEngine;

public interface IDailyRoutine 
{
    public void Enter();
    public void Excute(IDailyRoutine dailyRoutine);

    public void MiniGame();

    public void Exit();
}
