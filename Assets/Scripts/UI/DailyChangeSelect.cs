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
 
    //private void OnRestSelectEvent()
    //{
    //    EventBus.Publish(RestTimeQuestionItem.OnRestTimeCalculated); //최대시간 계산

    //    EventBus.Publish(UIManager.OnRestQuestions, false); //얼마나 쉴지 보여주는 ui 비활성화

    //    EventBus.Publish(EffectManager.OnSelectEffectStarted, currentChoiceData);
    //}
}
