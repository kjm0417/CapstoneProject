using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestUIManager : MonoBehaviour
{
    public static string OnRestUIBacked = "OnRestUIBackEvent"; //현재 UI 활성화 된 상태에서 ESC 키 누름에 대한 이벤트

    public static string OnRestTimeCalculated = "OnRestTimeCalculateEvent";  //다음 시간까지 "최대시간"분, 최대시간 계산하는 이벤트

    [SerializeField]
    RestTimeQuestionUI restTimeUI;

    [Header("버튼")]
    [SerializeField] public Button[] TimeSelectUpButtons;
    [SerializeField] public Button[] TimeSelectDownButtons;
    [SerializeField]
    public int currentTime;

    public Button GetHundredsUp() => TimeSelectUpButtons[2];
    public Button GetTensUp() => TimeSelectUpButtons[1];
    public Button GetOnesUp() => TimeSelectUpButtons[0];

    public Button GetHundredsDown() => TimeSelectDownButtons[2];
    public Button GetTensDown() => TimeSelectDownButtons[1];
    public Button GetOnesDown() => TimeSelectDownButtons[0];

    private void OnEnable()
    {
        for (int i = 0; i < 3; i++)
        {
            int index = i; //캡쳐를 i가 아닌 index로 수행.


            TimeSelectUpButtons[2 - index].onClick.AddListener(() => restTimeUI.ChangeDigit(index, 1));
            TimeSelectDownButtons[2 - index].onClick.AddListener(() => restTimeUI.ChangeDigit(index, -1));
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < 3; i++)
        {
            TimeSelectUpButtons[i].onClick.RemoveAllListeners();
           TimeSelectDownButtons[i].onClick.RemoveAllListeners();
        }
    }

}
