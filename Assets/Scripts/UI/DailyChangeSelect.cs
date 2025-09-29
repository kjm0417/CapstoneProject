using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyChangeSelect : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI choiceText;

    public void InitButtonText(string text)
    {
        choiceText.text = text.Trim(); //공백 제거

        this.gameObject.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected(text));
    }

    private void OnChoiceSelected(string choice)
    {
        Debug.Log($"선택된 Choice: {choice}");
        EventBus.Publish(DailyRoutineManager.OnReturnButtonSelected, false);

        EventBus.Publish(DailyRoutineManager.OnReturnButtonSelected);
    }

    private void OnChangePattern()
    {

    }
}
