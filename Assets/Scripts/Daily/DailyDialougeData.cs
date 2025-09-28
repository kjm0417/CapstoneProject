using System;
using UnityEngine;

[System.Serializable]
public class DailyDialougeData
{
    public int Day;
    public string Pattern;
    public int PlayerId;
    public int DialogueOrder;

    public string DialougeType;
    public string PlayerName;
    public string PlayerDialogue;
    public string Choices;
}
