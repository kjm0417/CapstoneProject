using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SubjectItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Period;

    [SerializeField]
    TextMeshProUGUI Subject;

    [SerializeField]
    TextMeshProUGUI SubjectDetail;

    public string MiniGameId; //미니게임 ID;

    public int SubjectTime;

    Button StudyButton;

    public Button GetButton()
    {
        if(StudyButton == null) 
        {
            StudyButton = GetComponent<Button>();
        }

        return StudyButton;
    }

    public void SubjectSet(string subject, int Time, string MasterId)
    {
        this.MiniGameId = MasterId;
        this.SubjectTime = Time;

        Subject.text = subject;

        SubjectDetail.text = Time.ToString();
    }

    //public void SubjectSet(int period,string subject, int StartPage, int EndPage)
    //{
    //    Period.text = period.ToString();

    //    Subject.text = subject;

    //    SubjectDetail.text = $"{StartPage} ~ {EndPage} pp";
    //}


}
 