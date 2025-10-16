using UnityEngine;
using UnityEngine.UI;

public class DailyStudySelectButton : DailySelectButtonManager
{
    protected override void OnEnable()
    {
        //base.OnEnable();
        EventBus.Subscribe<UIMode>(OnUIModeChanged, HandleUIModeChanged);
        EventBus.Subscribe<bool>(UIManager.OnInGameUI, OnInGameUIActive);
    }

    protected override void OnDisable()
    {
        // base.OnDisable();

        EventBus.UnSubscribe<UIMode>(OnUIModeChanged, HandleUIModeChanged);
        EventBus.UnSubscribe<bool>(UIManager.OnInGameUI, OnInGameUIActive);
    }

    private bool isSyncingMode = false;
    [SerializeField]
    private int currentStudyColum;
    [SerializeField]
    private bool isStudyUI;

    private void HandleUIModeChanged(UIMode newMode)
    {
        if (isSyncingMode) return; //스택 오버플로우 방지

        isSyncingMode = true;
        Mode = newMode;
        isSyncingMode = false;
    }
    private Button[] studyButtons = new Button[7];

    [SerializeField]
    GameObject SubjectItemParent;

    SubjectItem[] subjectItems;

    private void OnInGameUIActive(bool isActive)
    {
       

        isStudyUI = isActive;

        if (isActive)
        {
            if (studyButtons[0] == null)
            {
                subjectItems = SubjectItemParent.GetComponentsInChildren<SubjectItem>();

                for (int i = 0; i < studyButtons.Length; i++)
                {
                    studyButtons[i] = subjectItems[i].GetButton();
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


        //isStudySelectReady = true;

        Image img = studyButtons[currentStudyColum].GetComponent<Image>();

        if (img != null)
        {
            img.sprite = selectedSprite; // 원하는 스프라이트로 교체
        }

    }

    protected override void ClearStudyColum(bool resetToDefault)
    {
        isStudySelectReady = false;

        for (int i = 0; i < studyButtons.Length; i++)
        {
            if(resetToDefault == false && i == currentStudyColum) //현재 버튼은 제외하기
            {
                continue;
            }

            Image img = studyButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = defaultSprite;  // 원래 스프라이트로 되돌리기
            }
        }
    }

    protected override void OnStudyPressButton()
    {
        ClearStudyColum(true);

        HandleUIModeChanged(UIMode.None);

        string miniGameId = subjectItems[currentStudyColum].MiniGameId;

        int studyTime = subjectItems[currentStudyColum].SubjectTime;

        DecreseRestTime = studyTime;

        EventBus.Publish("MiniGame.StartById", $"{miniGameId}");

        InitStudy();
        //choiceBtnItemList[currentIndex].PressButton(Mode);
        Debug.Log($"자습한다 버튼 선택 확정(여기서 미니게임 실행) " +
            $"- 미니게임 ID : {miniGameId} || 공부 시간 :  {DecreseRestTime}");
    }


}
