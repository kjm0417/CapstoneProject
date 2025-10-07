using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyChangeSelect : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI choiceText;

    Button SelectButton;

    ChoiceData currentChoiceData; // 이 버튼이 가진 ChoiceData를 저장

    [Header("버튼 상태 이미지")]
    [SerializeField] private Sprite normalSprite;     // 기본 상태

    [SerializeField] private Sprite highlightedSprite; // 선택 상태
    private void OnEnable()
    {
        SelectButton = GetComponent<Button>();

    }
    public void InitButtonText(ChoiceData choice)
    {
        currentChoiceData = choice; // new ChoiceData() 한 격. 참조 저장

        choiceText.text = choice.Text; // 버튼에 텍스트 표시
    }

    public ChoiceData GetChoiceData()
    {
        return currentChoiceData;
    }
    public void SetHighlight(bool active)
    {
        if(active)
        {
            GetComponent<Image>().sprite = highlightedSprite;
            GetComponent<Image>().color = Color.white; // 완전 불투명
        }
        else
        {
            GetComponent<Image>().sprite = normalSprite;
            GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.75f); // 살짝 투명
        }
    }
    public void PressButton()
    {
        OnChoiceSelected();
    }

    private void OnChoiceSelected()
    {
        Debug.Log($"선택된 텍스트: {currentChoiceData.Text}");
        Debug.Log($"EffectId: {currentChoiceData.EffectId}");

        if (!string.IsNullOrEmpty(currentChoiceData.EffectId))
        {
            EventBus.Publish(EffectManager.OnSelectEffectStarted,currentChoiceData);
        }

        // 버튼 반환 이벤트 발행
        EventBus.Publish(DailySelectButtonManager.OnReturnButtonSelected);
        EventBus.Publish(DailySelectButtonManager.OnReturnButtonSelected,false);
    }

}
