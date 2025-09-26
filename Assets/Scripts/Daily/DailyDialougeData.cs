using System;
using UnityEngine;

[System.Serializable]
public class DailyDialougeData
{
    public string StateName; //각 상태 클래스이름과 일치 해야 하도록
    public string PlayerDialogue;

    [Header("미니게임 관련 선택지")]
    public string Choice1;
    public string ResultMiniGame1;
    public string NextState1;

    [Header("일반 선택지")]
    public string Choice2;
    public string NextState2;

    [Header("감정 관련 선택지")]
    public string Choice3;
    public string NextState3;

}
