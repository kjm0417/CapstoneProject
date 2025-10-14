using UnityEngine;
using UnityEngine.UI;

public class DailyStudySelectButton : DailySelectButtonManager
{
    public static string OnStudyPressed = "OnStudyPressEvent";
    public static string OnStudyUIClear = "OnStudyEscPressEvent"; //과목 UI 이미지 빼는 

    protected override void OnEnable()
    {
        //base.OnEnable();
        EventBus.Subscribe<UIMode>(OnUIModeChanged, HandleUIModeChanged);
        EventBus.Subscribe<bool>(UIManager.OnInGameUI, OnInGameUIActive);
        EventBus.Subscribe(OnStudyPressed, OnStudyPressButton);
        EventBus.Subscribe(OnStudyUIClear, OnEscPressed);
    }

    protected override void OnDisable()
    {
        // base.OnDisable();

        EventBus.UnSubscribe<UIMode>(OnUIModeChanged, HandleUIModeChanged);
        EventBus.UnSubscribe<bool>(UIManager.OnInGameUI, OnInGameUIActive);
        EventBus.UnSubscribe(OnStudyPressed, OnStudyPressButton);
        EventBus.UnSubscribe(OnStudyUIClear, OnEscPressed);
    }

    private void OnEscPressed()
    {
        ClearStudyColum(true);
    }
    [SerializeField]
    private UIMode localMode;
    private bool isSyncingMode = false;
    [SerializeField]
    protected int currentStudyColum;
    private void HandleUIModeChanged(UIMode newMode)
    {
        if (isSyncingMode) return; //스택 오버플로우 방지

        isSyncingMode = true;
        localMode = newMode;
        Mode = localMode;
        isSyncingMode = false;
    }
    private Button[] studyButton = new Button[7];

    [SerializeField]
    GameObject SubjectItemParent;

    SubjectItem[] subjectItems;

    private void OnInGameUIActive(bool isActive)
    {
       

        isStudyUI = isActive;

        if (isActive)
        {
            if (studyButton[0] == null)
            {
                subjectItems = SubjectItemParent.GetComponentsInChildren<SubjectItem>();

                for (int i = 0; i < studyButton.Length; i++)
                {
                    studyButton[i] = subjectItems[i].GetButton();
                }
            }
        }
        InitStudy();
    }

    private void InitStudy()
    {
        currentStudyColum = 0;
        isStudySelectReady = false;
        lastArrowKey = KeyCode.None;
    }


    protected override void MoveStudyColum(int direction)
    {
        currentStudyColum += direction;

        if(currentStudyColum < 0)
        {
            currentStudyColum = 6;
        }
        else if(currentStudyColum > 6)
        {
            currentStudyColum = 0;
        }

        SelectStudyColum();
    }

    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite defaultSprite;

    protected override void SelectStudyColum()
    {
        ClearStudyColum(false);

        base.CurrentButtonImageColorActive(false);


        isStudySelectReady = true;

        Image img = studyButton[currentStudyColum].GetComponent<Image>();

        if (img != null)
        {
            img.sprite = selectedSprite; // 원하는 스프라이트로 교체
        }

    }

    private void ClearStudyColum(bool resetToDefault)
    {
        for (int i = 0; i < studyButton.Length; i++)
        {
            if(resetToDefault == false && i == currentStudyColum) //현재 버튼은 제외하기
            {
                continue;
            }

            Image img = studyButton[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = defaultSprite;  // 원래 스프라이트로 되돌리기
            }
        }
    }

    private void OnStudyPressButton()
    {
        ClearStudyColum(true);
        InitStudy();

        string miniGameId = subjectItems[currentStudyColum].MiniGameId;

        int studyTime = subjectItems[currentStudyColum].SubjectTime;

        //choiceBtnItemList[currentIndex].PressButton(Mode);
        Debug.Log($"자습한다 버튼 선택 확정(여기서 미니게임 실행) - 미니게임 ID : {miniGameId} || 공부 시간 :  {studyTime}");
    }
    
}
