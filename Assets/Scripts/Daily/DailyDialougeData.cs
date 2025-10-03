using System;
using UnityEngine;

[System.Serializable]
public class DailyDialougeData
{
    public int Day; //날짜
    public string Pattern; //패턴 이름
    public int PlayerId; //화자 ID   :  0 - 민주, 1 - 채은 , 2 - 주희
    public int PlayerImagePos; //플레이어 이미지 UI 위치 : 0 - 왼쪽 , 1 - 중간 , 2 - 오른쪽 
    
    public string PlayerName; //화자 이름 : 민주 , 채은 , 주희 
    public string PlayerDialogue; //화자 대사 내용 


    //public string Choices; //선택 대사 내용

    public string ChoiceId;
    public string MasterId;
    public string ChoiceIndex;
    public string ChoiceText;
    public string EffectId;

    public string NextPattern;

}
