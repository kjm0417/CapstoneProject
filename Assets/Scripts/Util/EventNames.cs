using UnityEngine;

public static class EventNames //이벤트 이름 설정
{
    public const string ActiveCharacterChanged = "ActiveCharacterChanged"; // PlayerData
    public const string PlayerStatsChanged = "PlayerStatsChanged";
    public const string OnPlayerNotInput = "OnPlayerNotInputEvent"; //플레이어 행동 정지


    //미니게임관련
    public const string MiniGameSuccesed = "MiniGameSucceded";
    public const string MiniGameFailed = "MiniGameFailed";

}
