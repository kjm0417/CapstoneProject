using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyChangeSelect : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI choiceText;

    Button SelectButton;

    private ChoiceData currentChoice; // 이 버튼이 가진 ChoiceData를 저장

    private void OnEnable()
    {
        SelectButton = GetComponent<Button>();
    }
    public void InitButtonText(ChoiceData choice)
    {
        currentChoice = choice; // 현재 선택지 저장
        choiceText.text = choice.Text; // 버튼에 텍스트 표시

        SelectButton.onClick.RemoveAllListeners();

        SelectButton.onClick.AddListener(() => OnChoiceSelected());       
    }

    private void OnChoiceSelected()
    {
        Debug.Log($"선택된 텍스트: {currentChoice.Text}");
        Debug.Log($"EffectId: {currentChoice.EffectId}");

        if (!string.IsNullOrEmpty(currentChoice.EffectId))
        {
            EventBus.Publish(EffectManager.OnSelectEffectStarted, currentChoice.EffectId,currentChoice.NextPattern);
        }
        // 버튼 반환 이벤트 발행
        EventBus.Publish(DailyRoutineManager.OnReturnButtonSelected);
        EventBus.Publish(DailyRoutineManager.OnReturnButtonSelected,false);
    }

}
