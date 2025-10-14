using UnityEngine;

[CreateAssetMenu(fileName = "DailyRoutineTime", menuName = "Game/DailyRoutineTime")]
public class DailyRoutineTime : ScriptableObject
{
    [Header("최대 시간 설정")]
    public int maxGotoSchool = 10; //등교시간
    public int maxBreakfast = 10; //아침 시간(조례)
    public int maxMorning = 240; //오전 시간
    public int maxLanch = 20; //점심 시간
    public int maxAfternoon = 170; //오후 시간
    public int maxParm = 50; //야자 시간 
    public int maxAfterSchool; //야자 연장 시간 or 학원 시간 

}
