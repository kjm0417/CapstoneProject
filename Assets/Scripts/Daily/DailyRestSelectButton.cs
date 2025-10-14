using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRestSelectButton : DailySelectButtonManager
{
    public static string OnRestPressed = "OnRestPressEvent";
    public static string OnRestSelectUIClear = "OnRestEscPressEvent"; //시간 선택 버튼 UI 이미지 빼는
    [Header("RestUI 버튼 관련")]
    [SerializeField]
    RestTimeQuestionItem restQuestionItem;
    private Button[] upButtons = new Button[3];
    private Button[] downButtons = new Button[3];

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.Subscribe<bool>(UIManager.OnRestQuestions, OnRestUIActive);

        EventBus.Subscribe<UIMode>(OnUIModeChanged, HandleUIModeChanged);

        EventBus.Subscribe(OnRestPressed, OnRestPressButton);

        EventBus.Subscribe(OnRestSelectUIClear, OnEscPressed);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.UnSubscribe<bool>(UIManager.OnRestQuestions, OnRestUIActive);

        EventBus.UnSubscribe<UIMode>(OnUIModeChanged, HandleUIModeChanged);

        EventBus.UnSubscribe(OnRestPressed, OnRestPressButton);

        EventBus.UnSubscribe(OnRestSelectUIClear, OnEscPressed);

    }
    private void OnEscPressed()
    {
        ClearColum(true);
    }
    [SerializeField]
    private UIMode localMode;
    private bool isSyncingMode = false;
    private void HandleUIModeChanged(UIMode newMode)
    {
        if (isSyncingMode) return; //스택 오버플로우 방지

        isSyncingMode = true;
        localMode = newMode;
        Mode = localMode;
        isSyncingMode = false;
    }

    private void OnRestUIActive(bool isActive)
    {
        isRestQuestionUI = isActive;

        if (isActive)
        {
           // Mode = UIMode.Rest;

            if (upButtons[0] == null)
            {
                upButtons[0] = restQuestionItem.GetOnesUp();
                upButtons[1] = restQuestionItem.GetTensUp();
                upButtons[2] = restQuestionItem.GetHundredsUp();
            }
            if (downButtons[0] == null)
            {
                downButtons[0] = restQuestionItem.GetOnesDown();
                downButtons[1] = restQuestionItem.GetTensDown();
                downButtons[2] = restQuestionItem.GetHundredsDown();
            }
        }

        InitRest();
    }

    private void InitRest() //초기화 메서드
    {
        currentColum = 0;
        isTimeSelectReady = false;
        isUpSelected = false;
        lastArrowKey = KeyCode.None;
        isUpSelected = false;
    }
    /// <summary>
    /// 시간 선택 버튼 움직임 관련 메서드
    /// </summary>
    protected override void MoveColumn(int direction)
    {
        ClearColum(false);

        currentColum += direction;

        if (currentColum < 0)
        {
            currentColum = 2;
        }
        else if (currentColum > 2)
        {
            currentColum = 0;
        }

        if (isUpSelected)
        {
            Debug.Log("1열 , 좌우 이동");
            SelectCloum(upButtons[currentColum]);
        }
        else
        {
            Debug.Log("2열 , 좌우 이동");
            SelectCloum(downButtons[currentColum]);
        }
    }
    /// <summary>
    /// 시간 버튼 선택 하였을 때 메서드
    /// </summary>
    protected override void SelectCloum(bool isUp)
    {
        ClearColum(false);

        if(isUp)
        {
            ColorBlock cb =  upButtons[currentColum].colors;

            cb.normalColor = Color.yellow;
            upButtons[currentColum].colors = cb;
        }
        else
        {
            ColorBlock cb = downButtons[currentColum].colors;

            cb.normalColor = Color.yellow;
            downButtons[currentColum].colors = cb;
        }
    }
    /// <summary>
    /// 시간 버튼 선택 후 확정하였을 때 버튼 클릭 이벤트 발행 메서드
    /// </summary>
    protected override void SelectFinshed(bool isUp)
    {
        if(isUp)
        {
            upButtons[currentColum].onClick.Invoke();
        }
        else
        {
            downButtons[currentColum].onClick.Invoke();
        }
    }
    /// <summary>
    /// 시간 버튼의 색상을 모두 초기화 하거나, 현재 선택된 버튼을 제외한 
    /// 버튼 색상을 모두 초기화 하는 메서드
    /// </summary>
    private void ClearColum(bool keepCurrent)
    {
        if (isRestQuestionUI == false) { return; }

        for (int i = 0; i < upButtons.Length; i++)
        {
            ColorBlock cb = upButtons[i].colors;

            if (!keepCurrent && i == currentColum && isUpSelected)
            {
               
                continue;
            }

            cb.normalColor = Color.white;
            upButtons[i].colors = cb;
        }

        for (int i = 0; i < downButtons.Length; i++)
        {
            ColorBlock cb = downButtons[i].colors;

            if (!keepCurrent && i == currentColum && !isUpSelected)
            {           
                continue;
            }

            cb.normalColor = Color.white;
            downButtons[i].colors = cb;
        }
    }
    [SerializeField]
    protected int currentColum;
    private  void OnRestPressButton()
    {
        OnRestSelectEvent();
    }
    private void OnRestSelectEvent()
    {
        EventBus.Publish(RestTimeQuestionItem.OnRestTimeCalculated); //최대시간 계산

        EventBus.Publish(UIManager.OnRestQuestions, false); //얼마나 쉴지 보여주는 ui 비활성화

        ChoiceData choiceData = choiceBtnItemList[currentIndex].GetChoiceData();
        EventBus.Publish(EffectManager.OnSelectEffectStarted, choiceData);
    }
}