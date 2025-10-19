using UnityEngine;

public interface IRestTimeProvider 
{
    public int CurrentRestTime { get; set;  } //현재 RestTime 프로퍼티
    public int CurrentDecreaseRestTime { get; set; } //스터디 플래너로 인한 RestTime을 감소 하는 프로퍼티

    public int GetMaxRestMinutes(); //패턴별 고유의 최대값을 반환하는 메서드
}
