using UnityEngine;

[System.Serializable]
public class QuestData  //한 과목에 대한 클래스. ex) 1교시 화법과 작문 1~5pp
{
    //아래의 변수들의 타입과 변수명은 액셀 파일의 행과 열에 맞춰서

    public int Period; //구분 - N교시

    public string Subject; //퀘스트 - 과목

    public int StartPage; //퀘스트 난이도 - 과목 시작 페이지
    public int EndPage; //퀘스트 난이도 - 과목 끝 페이지

    public int StudyTime; //퀘스트 수행 시간 - 과목 수업 시간

    public string IsCompleted; //퀘스트 완료 여부 - 과목 수업(미니 게임) 통과 여부

    public QuestData(bool isCompleted)
    {
        this.IsCompleted = isCompleted.ToString();
    }
}
