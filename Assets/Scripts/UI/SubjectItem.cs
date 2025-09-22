using TMPro;
using UnityEngine;

public class SubjectItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Period;

    [SerializeField]
    TextMeshProUGUI Subject;

    [SerializeField]
    TextMeshProUGUI SubjectDetail;

    public void SubjectSet(int period,string subject, int StartPage, int EndPage)
    {
        Period.text = period.ToString();

        Subject.text = subject;

        SubjectDetail.text = $"{StartPage} ~ {EndPage} pp";
    }


}
