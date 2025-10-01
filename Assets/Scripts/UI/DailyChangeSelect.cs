using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyChangeSelect : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI choiceText;

    Button SelectButton;

    private string displayText;

    private string patternKey;

    private void OnEnable()
    {
        SelectButton = GetComponent<Button>();
    }
    public void InitButtonText(string text)
    {
        string[] parts  = text.Split('|');
        ChoiceTextParts(parts);

        SelectButton.onClick.RemoveAllListeners();

        SelectButton.onClick.AddListener(() => OnChoiceSelected(parts[0], parts[1]));       
    }

    /// <summary>
    /// 선택 대사의 내용을 분리하여, 버튼에 보여줄 텍스트와 패턴 이름을 저장하고,
    /// 버튼의 텍스트를 업데이트 한다.
    /// </summary>
    private void ChoiceTextParts(string[] parts) 
    {
        if(parts.Length > 0)
        {
            displayText = parts[0];

            patternKey = parts[1];
        }
        else { Debug.LogWarning("선택 대사의 내용이 비어 있음"); }

        choiceText.text = displayText;
    }

    private void OnChoiceSelected(string choice, string pattern) //선택 버튼을 통해 다음 패턴에 대한 연출
    {
        Debug.Log(choice);
        Debug.Log(pattern);

        switch (pattern)
        {
            case "등교":
                Debug.Log("이동 수단을 통해 등교하는 연출");
                break;
            case "학교":
                Debug.Log("학교 도착하는 연출");
                break;
            case "야자":
                Debug.Log("야자 하는 연출");
                break;
            case "학원":
                Debug.Log("학원에서 공부 하는 연출");
                break;
            case "방":
                Debug.Log("암전 되는 연출");
                break;
        }

        EventBus.Publish(DailyRoutineManager.OnReturnButtonSelected, false);

        EventBus.Publish(DailyRoutineManager.OnReturnButtonSelected);
    }

}
