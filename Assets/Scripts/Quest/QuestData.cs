using UnityEngine;

[System.Serializable]
public class QuestData  //한 과목에 대한 클래스. ex) 1교시 화법과 작문 1~5pp
{
    //아래의 변수들의 타입과 변수명은 액셀 파일의 행과 열에 맞춰서

    public int period;

    public string Subject;

    public int StartPage;
    public int EndPage;

}
