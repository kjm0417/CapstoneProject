using UnityEngine;

public static class EventNames //이벤트 이름 설정
{
    public const string ActiveCharacterChanged = "ActiveCharacterChanged"; // payload: PlayerData

    // 상태 변경 이벤트(선택): UI 등이 id별로 구독할 때 사용
    public static string HPChanged(string id) => $"player:{id}:hp_changed";
    public static string StressChanged(string id) => $"player:{id}:stress_changed";
    public static string ConcentrationChanged(string id) => $"player:{id}:concentration_changed";
}